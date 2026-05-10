namespace FamLedger.Application.DTOs.Response
{
    public enum AddDebtStatus
    {
        Ok,
        InvalidRequest,
        Duplicate,
        Forbidden,
        PersistenceFailed,
    }

    public sealed class AddDebtResult
    {
        public AddDebtStatus Status { get; init; }
        public DebtItemDto? Response { get; init; }

        public static AddDebtResult Success(DebtItemDto dto) =>
            new() { Status = AddDebtStatus.Ok, Response = dto };

        public static AddDebtResult InvalidRequest() =>
            new() { Status = AddDebtStatus.InvalidRequest };

        public static AddDebtResult Duplicate() =>
            new() { Status = AddDebtStatus.Duplicate };

        public static AddDebtResult Forbidden() =>
            new() { Status = AddDebtStatus.Forbidden };

        public static AddDebtResult PersistenceFailed() =>
            new() { Status = AddDebtStatus.PersistenceFailed };
    }

    public enum GetDebtByIdStatus
    {
        Ok,
        NotFound,
        Forbidden,
    }

    public sealed class GetDebtByIdResult
    {
        public GetDebtByIdStatus Status { get; init; }
        public DebtItemDto? Response { get; init; }

        public static GetDebtByIdResult Success(DebtItemDto dto) =>
            new() { Status = GetDebtByIdStatus.Ok, Response = dto };

        public static GetDebtByIdResult NotFound() =>
            new() { Status = GetDebtByIdStatus.NotFound };

        public static GetDebtByIdResult Forbidden() =>
            new() { Status = GetDebtByIdStatus.Forbidden };
    }

    public enum UpdateDebtStatus
    {
        Ok,
        InvalidRequest,
        NotFound,
        Forbidden,
        PersistenceFailed,
    }

    public sealed class UpdateDebtResult
    {
        public UpdateDebtStatus Status { get; init; }
        public DebtItemDto? Response { get; init; }

        public static UpdateDebtResult Success(DebtItemDto dto) =>
            new() { Status = UpdateDebtStatus.Ok, Response = dto };

        public static UpdateDebtResult InvalidRequest() =>
            new() { Status = UpdateDebtStatus.InvalidRequest };

        public static UpdateDebtResult NotFound() =>
            new() { Status = UpdateDebtStatus.NotFound };

        public static UpdateDebtResult Forbidden() =>
            new() { Status = UpdateDebtStatus.Forbidden };

        public static UpdateDebtResult PersistenceFailed() =>
            new() { Status = UpdateDebtStatus.PersistenceFailed };
    }

    public enum DeleteDebtStatus
    {
        Ok,
        NotFound,
        Forbidden,
        PersistenceFailed,
    }

    public sealed class DeleteDebtResult
    {
        public DeleteDebtStatus Status { get; init; }

        public static DeleteDebtResult Ok() =>
            new() { Status = DeleteDebtStatus.Ok };

        public static DeleteDebtResult NotFound() =>
            new() { Status = DeleteDebtStatus.NotFound };

        public static DeleteDebtResult Forbidden() =>
            new() { Status = DeleteDebtStatus.Forbidden };

        public static DeleteDebtResult PersistenceFailed() =>
            new() { Status = DeleteDebtStatus.PersistenceFailed };
    }
}
