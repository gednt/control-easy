using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace ControlEasyReborn.ArchitectureTests;

public sealed class TokensContractTests
{
    [Fact]
    public void TokensContract_light_and_dark_tokens_exist_in_Angular_theme()
    {
        var root = FindRepositoryRoot();
        var mockupStyles = File.ReadAllText(Path.Combine(root, "mockup", "styles.css"));
        var contractBlocks = Regex.Matches(
            mockupStyles,
            @"(?s)(?::root|\[data-theme=[""']dark[""']\])\s*\{(?<body>.*?)\}");
        contractBlocks.Count.Should().BeGreaterThanOrEqualTo(2, "mockup/styles.css defines the canonical light and dark token blocks");
        var contractTokens = contractBlocks
            .SelectMany(block => Regex.Matches(block.Groups["body"].Value, @"(?m)^\s*(--[\w-]+)\s*:")
                .Select(match => match.Groups[1].Value))
            .ToHashSet();

        var styles = File.ReadAllText(Path.Combine(root, "src", "Web", "ControlEasyReborn.Web", "src", "styles.css"));
        var theme = Regex.Match(styles, @"@theme\s*\{(?<body>[\s\S]*?)\}");
        theme.Success.Should().BeTrue("styles.css must contain an @theme block");
        var themeTokens = Regex.Matches(theme.Groups["body"].Value, @"(?m)^\s*(--[\w-]+)\s*:")
            .Select(match => match.Groups[1].Value)
            .ToHashSet();

        var missing = contractTokens.Except(themeTokens).OrderBy(name => name).ToArray();
        missing.Should().BeEmpty("every canonical mockup token must be exposed by Angular's @theme block");
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
                return directory.FullName;
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("Could not locate repository root.");
    }
}
