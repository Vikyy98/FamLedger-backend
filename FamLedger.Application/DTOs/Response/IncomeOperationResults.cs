namespace FamLedger.Application.DTOs.Response
{
    public enum AddIncomeStatus
    {
        Ok,
        InvalidRequest,
        Duplicate,
        Forbidden,
        PersistenceFailed,
    }

    public sealed class AddIncomeResult
    {
        public AddIncomeStatus Status { get; init; }
        public IncomeItemDto? Response { get; init; }

        public static AddIncomeResult Success(IncomeItemDto dto) =>
            new() { Status = AddIncomeStatus.Ok, Response = dto };

        public static AddIncomeResult InvalidRequest() =>
            new() { Status = AddIncomeStatus.InvalidRequest };

        public static AddIncomeResult Duplicate() =>
            new() { Status = AddIncomeStatus.Duplicate };

        public static AddIncomeResult Forbidden() =>
            new() { Status = AddIncomeStatus.Forbidden };

        public static AddIncomeResult PersistenceFailed() =>
            new() { Status = AddIncomeStatus.PersistenceFailed };
    }

    public enum GetIncomeByIdStatus
    {
        Ok,
        NotFound,
        Forbidden,
    }

    public sealed class GetIncomeByIdResult
    {
        public GetIncomeByIdStatus Status { get; init; }
        public IncomeItemDto? Response { get; init; }

        public static GetIncomeByIdResult Success(IncomeItemDto dto) =>
            new() { Status = GetIncomeByIdStatus.Ok, Response = dto };

        public static GetIncomeByIdResult NotFound() =>
            new() { Status = GetIncomeByIdStatus.NotFound };

        public static GetIncomeByIdResult Forbidden() =>
            new() { Status = GetIncomeByIdStatus.Forbidden };
    }
}
