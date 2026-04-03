namespace FamLedger.Application.Utilities;

public static class FamilyCodeGenerator
{
    public static string Next(string? lastCode)
    {
        if (string.IsNullOrEmpty(lastCode))
            return "FAM001";

        var number = int.Parse(lastCode.Substring(3));
        return $"FAM{(number + 1).ToString("D3")}";
    }
}
