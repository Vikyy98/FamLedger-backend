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

    public enum FamilyInvitationStatus
    {
        Ok,
        Forbidden,
        NotFound,
        Failed,
    }

    public sealed class FamilyInvitationResult
    {
        public FamilyInvitationStatus Status { get; init; }
        public FamilyInvitationResponse? Response { get; init; }

        public static FamilyInvitationResult Success(FamilyInvitationResponse response) =>
            new() { Status = FamilyInvitationStatus.Ok, Response = response };

        public static FamilyInvitationResult Forbidden() =>
            new() { Status = FamilyInvitationStatus.Forbidden };

        public static FamilyInvitationResult NotFound() =>
            new() { Status = FamilyInvitationStatus.NotFound };

        public static FamilyInvitationResult Failed() =>
            new() { Status = FamilyInvitationStatus.Failed };
    }

    public enum FamilyMembersStatus
    {
        Ok,
        NotFound,
        Forbidden,
    }

    public sealed class FamilyMembersListResult
    {
        public FamilyMembersStatus Status { get; init; }
        public IReadOnlyList<FamilyMemberDto>? Members { get; init; }

        public static FamilyMembersListResult Ok(IReadOnlyList<FamilyMemberDto> members) =>
            new() { Status = FamilyMembersStatus.Ok, Members = members };

        public static FamilyMembersListResult NotFound() =>
            new() { Status = FamilyMembersStatus.NotFound };

        public static FamilyMembersListResult Forbidden() =>
            new() { Status = FamilyMembersStatus.Forbidden };
    }
}
