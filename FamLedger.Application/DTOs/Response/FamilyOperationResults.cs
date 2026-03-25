namespace FamLedger.Application.DTOs.Response
{
    public enum FamilyCreateStatus
    {
        Ok,
        UserNotFound,
        AlreadyInFamily,
    }

    public sealed class FamilyCreateResult
    {
        public FamilyCreateStatus Status { get; init; }
        public FamilyResponse? Response { get; init; }

        public static FamilyCreateResult Success(FamilyResponse response) =>
            new() { Status = FamilyCreateStatus.Ok, Response = response };

        public static FamilyCreateResult UserNotFound() =>
            new() { Status = FamilyCreateStatus.UserNotFound };

        public static FamilyCreateResult UserAlreadyInFamily() =>
            new() { Status = FamilyCreateStatus.AlreadyInFamily };
    }

    public enum FamilyGetStatus
    {
        Ok,
        NotFound,
        Forbidden,
    }

    public sealed class FamilyGetResult
    {
        public FamilyGetStatus Status { get; init; }
        public FamilyResponse? Response { get; init; }

        public static FamilyGetResult Ok(FamilyResponse response) =>
            new() { Status = FamilyGetStatus.Ok, Response = response };

        public static FamilyGetResult NotFound() =>
            new() { Status = FamilyGetStatus.NotFound };

        public static FamilyGetResult Forbidden() =>
            new() { Status = FamilyGetStatus.Forbidden };
    }
}
