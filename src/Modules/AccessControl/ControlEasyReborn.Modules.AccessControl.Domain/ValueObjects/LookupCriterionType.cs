namespace ControlEasyReborn.Modules.AccessControl.Domain.ValueObjects;

public enum LookupCriterionType
{
    Cpf = 0,
    IdentityDocument = 1,
    Name = 2,
    Apartment = 3,
    Block = 4
}

public static class LookupCriterionTypeCodes
{
    public static readonly IReadOnlyDictionary<string, LookupCriterionType> ParseTable = new Dictionary<string, LookupCriterionType>(StringComparer.OrdinalIgnoreCase)
    {
        ["cpf"] = LookupCriterionType.Cpf,
        ["identity_document"] = LookupCriterionType.IdentityDocument,
        ["name"] = LookupCriterionType.Name,
        ["apartment"] = LookupCriterionType.Apartment,
        ["block"] = LookupCriterionType.Block
    };

    public static bool TryParse(string? value, out LookupCriterionType criterion)
    {
        if (!string.IsNullOrWhiteSpace(value) && ParseTable.TryGetValue(value, out criterion))
        {
            return true;
        }
        criterion = LookupCriterionType.Name;
        return false;
    }

    public static string ToWire(LookupCriterionType criterion) => criterion switch
    {
        LookupCriterionType.Cpf => "cpf",
        LookupCriterionType.IdentityDocument => "identity_document",
        LookupCriterionType.Name => "name",
        LookupCriterionType.Apartment => "apartment",
        LookupCriterionType.Block => "block",
        _ => "unknown"
    };
}
