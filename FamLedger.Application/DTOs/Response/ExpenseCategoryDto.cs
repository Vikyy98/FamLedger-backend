namespace FamLedger.Application.DTOs.Response
{
    /// <summary>
    /// Returned by GET /api/expenses/categories so the frontend dropdown
    /// can render labels without hard-coding them.
    /// </summary>
    public class ExpenseCategoryDto
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
