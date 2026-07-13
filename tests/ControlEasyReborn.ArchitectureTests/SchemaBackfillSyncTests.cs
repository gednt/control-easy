using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace ControlEasyReborn.ArchitectureTests;

public sealed class SchemaBackfillSyncTests
{
    private static readonly string InitScriptsDir = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "docker", "mysql", "init"));

    [Fact]
    public void Every_CREATE_TABLE_in_init_scripts_must_be_listed_in_backfill_script()
    {
        if (!Directory.Exists(InitScriptsDir))
        {
            return;
        }

        var backfillPath = Path.Combine(InitScriptsDir, "03-tenant-backfill.sql");
        if (!File.Exists(backfillPath))
        {
            return;
        }

        var backfillContent = File.ReadAllText(backfillPath);

        var createTableNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var scriptFile in Directory.GetFiles(InitScriptsDir, "*.sql")
                     .OrderBy(f => f, StringComparer.Ordinal))
        {
            var content = File.ReadAllText(scriptFile);
            var matches = Regex.Matches(content, @"CREATE\s+TABLE\s+(?:IF\s+NOT\s+EXISTS\s+)?(\w+)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);
            foreach (Match match in matches)
            {
                createTableNames.Add(match.Groups[1].Value);
            }
        }

        var backfillTableNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var alterMatches = Regex.Matches(backfillContent, @"ALTER\s+TABLE\s+(\w+)\s+ADD\s+COLUMN\s+(?:IF\s+NOT\s+EXISTS\s+)?tenant_id",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        foreach (Match match in alterMatches)
        {
            backfillTableNames.Add(match.Groups[1].Value);
        }

        var exemptTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Tenants",
            "RefreshTokens",
            "DemoMetadata",
            "Migrations"
        };

        var missing = createTableNames
            .Except(backfillTableNames)
            .Except(exemptTables)
            .ToList();

        missing.Should().BeEmpty(
            $"the following CREATE TABLE entries are not listed in 03-tenant-backfill.sql: {string.Join(", ", missing)}");
    }
}