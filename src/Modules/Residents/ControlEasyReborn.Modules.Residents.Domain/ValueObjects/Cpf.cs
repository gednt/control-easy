namespace ControlEasyReborn.Modules.Residents.Domain.ValueObjects;

public sealed record Cpf
{
    public string Value { get; }

    public Cpf(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("CPF cannot be empty.", nameof(value));

        var digits = new string(value.Where(char.IsDigit).ToArray());

        if (digits.Length != 11)
            throw new ArgumentException("CPF must contain exactly 11 digits.", nameof(value));

        Value = digits;
    }

    public static bool IsValid(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (digits.Length != 11)
            return false;

        if (digits.All(c => c == digits[0]))
            return false;

        var numbers = digits.Select(c => c - '0').ToArray();

        var sum1 = 0;
        for (var i = 0; i < 9; i++)
            sum1 += numbers[i] * (10 - i);

        var remainder1 = sum1 % 11;
        var checkDigit1 = remainder1 < 2 ? 0 : 11 - remainder1;
        if (numbers[9] != checkDigit1)
            return false;

        var sum2 = 0;
        for (var i = 0; i < 10; i++)
            sum2 += numbers[i] * (11 - i);

        var remainder2 = sum2 % 11;
        var checkDigit2 = remainder2 < 2 ? 0 : 11 - remainder2;
        if (numbers[10] != checkDigit2)
            return false;

        return true;
    }

    public string Formatted => $"{Value[..3]}.{Value[3..6]}.{Value[6..9]}-{Value[9..]}";

    public override string ToString() => Value;
}