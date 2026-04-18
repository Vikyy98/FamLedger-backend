namespace FamLedger.Application.DTOs.Response
{
    public enum AddExpenseStatus
    {
        Ok,
        InvalidRequest,
        Duplicate,
        Forbidden,
        PersistenceFailed,
    }

    public sealed class AddExpenseResult
    {
        public AddExpenseStatus Status { get; init; }
        public ExpenseItemDto? Response { get; init; }

        public static AddExpenseResult Success(ExpenseItemDto dto) =>
            new() { Status = AddExpenseStatus.Ok, Response = dto };

        public static AddExpenseResult InvalidRequest() =>
            new() { Status = AddExpenseStatus.InvalidRequest };

        public static AddExpenseResult Duplicate() =>
            new() { Status = AddExpenseStatus.Duplicate };

        public static AddExpenseResult Forbidden() =>
            new() { Status = AddExpenseStatus.Forbidden };

        public static AddExpenseResult PersistenceFailed() =>
            new() { Status = AddExpenseStatus.PersistenceFailed };
    }

    public enum GetExpenseByIdStatus
    {
        Ok,
        NotFound,
        Forbidden,
    }

    public sealed class GetExpenseByIdResult
    {
        public GetExpenseByIdStatus Status { get; init; }
        public ExpenseItemDto? Response { get; init; }

        public static GetExpenseByIdResult Success(ExpenseItemDto dto) =>
            new() { Status = GetExpenseByIdStatus.Ok, Response = dto };

        public static GetExpenseByIdResult NotFound() =>
            new() { Status = GetExpenseByIdStatus.NotFound };

        public static GetExpenseByIdResult Forbidden() =>
            new() { Status = GetExpenseByIdStatus.Forbidden };
    }

    public enum UpdateExpenseStatus
    {
        Ok,
        InvalidRequest,
        NotFound,
        Forbidden,
        PersistenceFailed,
    }

    public sealed class UpdateExpenseResult
    {
        public UpdateExpenseStatus Status { get; init; }
        public ExpenseItemDto? Response { get; init; }

        public static UpdateExpenseResult Success(ExpenseItemDto dto) =>
            new() { Status = UpdateExpenseStatus.Ok, Response = dto };

        public static UpdateExpenseResult InvalidRequest() =>
            new() { Status = UpdateExpenseStatus.InvalidRequest };

        public static UpdateExpenseResult NotFound() =>
            new() { Status = UpdateExpenseStatus.NotFound };

        public static UpdateExpenseResult Forbidden() =>
            new() { Status = UpdateExpenseStatus.Forbidden };

        public static UpdateExpenseResult PersistenceFailed() =>
            new() { Status = UpdateExpenseStatus.PersistenceFailed };
    }

    public enum DeleteExpenseStatus
    {
        Ok,
        NotFound,
        Forbidden,
        PersistenceFailed,
    }

    public sealed class DeleteExpenseResult
    {
        public DeleteExpenseStatus Status { get; init; }

        public static DeleteExpenseResult Ok() =>
            new() { Status = DeleteExpenseStatus.Ok };

        public static DeleteExpenseResult NotFound() =>
            new() { Status = DeleteExpenseStatus.NotFound };

        public static DeleteExpenseResult Forbidden() =>
            new() { Status = DeleteExpenseStatus.Forbidden };

        public static DeleteExpenseResult PersistenceFailed() =>
            new() { Status = DeleteExpenseStatus.PersistenceFailed };
    }
}
