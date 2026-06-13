namespace ControlEasyReborn.Infrastructure.Demo;

public sealed record DemoResidentFixture(
    string Name,
    string Block,
    string Apt,
    bool Active);

public static class DemoFixtures
{
    public static readonly DemoResidentFixture[] AuroraResidents =
    [
        new("Chaves", "A", "Barril", true),
        new("Dona Florinda", "A", "8", true),
        new("Quico", "A", "8", true),
        new("Prof. Girafales", "A", "8", true),
        new("Seu Madruga", "A", "14", true),
        new("Chilindrina", "A", "14", true),
        new("Chiquinha", "A", "14", true),
        new("Sr. Barriga", "A", "1", true),
        new("Popis", "A", "11", true),
        new("Paty", "A", "10", false),
        new("Godinez", "A", "12", true),
        new("Jaça", "A", "7", true),
        new("Pingüinos", "A", "7", true),
        new("Jaiminho", "A", "Correios", true),
        new("Dona Neves", "A", "15", true),
        new("Gloria", "A", "16", true),
        new("Botija", "A", "17", false),
        new("Maruxa", "A", "18", true),
        new("Serafim", "A", "18", true),
        new("Pelón", "A", "13", true),
        new("Michael De Santa", "B", "101", true),
        new("Franklin Clinton", "B", "102", true),
        new("Trevor Philips", "B", "103", true),
        new("CJ", "B", "201", true),
        new("Tommy Vercetti", "B", "202", true),
        new("Niko Bellic", "B", "203", true),
        new("Lamar Davis", "B", "301", true),
        new("Lester Crest", "B", "302", false),
        new("Roman Bellic", "B", "303", true),
        new("Sweet Johnson", "B", "401", true),
        new("Amanda De Santa", "B", "104", true),
        new("Lucia De Santa", "B", "105", true),
        new("Wade", "B", "204", true),
        new("Stretch", "B", "205", true),
        new("Big Smoke", "B", "304", true),
        new("Ryder", "B", "305", true),
        new("Tenpenny", "B", "402", false),
        new("Woozie", "B", "403", true),
        new("Lance Vance", "B", "404", true),
        new("Ken Rosenberg", "B", "405", true),
        new("Kratos", "C", "Sparta-1", true),
        new("Atreus", "C", "Sparta-1", true),
        new("Deimos", "C", "Sparta-1", true),
        new("Callisto", "C", "Sparta-1", false),
        new("Athena", "C", "Olympus-2", true),
        new("Ares", "C", "Olympus-3", false),
        new("Aphrodite", "C", "Olympus-3", true),
        new("Hephaestus", "C", "Forge-1", true),
        new("Hermes", "C", "Olympus-4", true),
        new("Hades", "C", "Underworld-1", true),
        new("Persephone", "C", "Underworld-1", true),
        new("Poseidon", "C", "Sea-1", true),
        new("Helios", "C", "Sun-1", true),
        new("Perseus", "C", "Hero-1", false),
        new("Theseus", "C", "Hero-2", true),
        new("Hercules", "C", "Hero-3", true),
        new("Orpheus", "C", "Hero-4", true),
        new("Pandora", "C", "Hero-5", true),
        new("Gaia", "C", "Titan-1", true),
        new("Cronos", "C", "Titan-2", true),
        new("Zeus", "D", "Pantheon-601", false),
    ];

    public static readonly DemoResidentFixture[] ParqueVerdeResidents =
    [
        new("Rogelio", "A", "110", true),
        new("Úrsulo", "A", "111", true),
        new("Denise", "B", "210", true),
        new("Mallorie", "B", "211", true),
        new("Eurydice", "C", "310", true),
        new("Charon", "C", "311", false),
    ];

    public static string GenerateValidCpf(int seed)
    {
        var nums = new int[9];
        var value = 100000000 + (seed % 899999999);
        for (var i = 8; i >= 0; i--)
        {
            nums[i] = value % 10;
            value /= 10;
        }

        if (nums.All(n => n == nums[0]))
            nums[8] = (nums[8] + 1) % 10;

        var check1 = ComputeCheckDigit(nums, 10);
        var withFirst = nums.Concat(new[] { check1 }).ToArray();
        var check2 = ComputeCheckDigit(withFirst, 11);
        return string.Concat(nums) + check1 + check2;
    }

    private static int ComputeCheckDigit(int[] digits, int weightStart)
    {
        var sum = 0;
        for (var i = 0; i < digits.Length; i++)
            sum += digits[i] * (weightStart - i);

        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }
}
