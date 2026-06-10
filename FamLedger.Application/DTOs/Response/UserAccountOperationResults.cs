namespace FamLedger.Application.DTOs.Response
{
    public enum RegisterUserStatus
    {
        Ok,
        InvalidRequest,
        EmailAlreadyExists,
        InviteInvalid,
        Failed,
    }

    public sealed class RegisterUserResult
    {
        public RegisterUserStatus Status { get; init; }
        public RegisterUserResponse? Response { get; init; }

        public static RegisterUserResult Success(RegisterUserResponse response) =>
            new() { Status = RegisterUserStatus.Ok, Response = response };

        public static RegisterUserResult InvalidRequest() =>
            new() { Status = RegisterUserStatus.InvalidRequest };

        public static RegisterUserResult EmailAlreadyExists() =>
            new() { Status = RegisterUserStatus.EmailAlreadyExists };

        public static RegisterUserResult InviteInvalid() =>
            new() { Status = RegisterUserStatus.InviteInvalid };

        public static RegisterUserResult Failed() =>
            new() { Status = RegisterUserStatus.Failed };
    }

    public enum LoginStatus
    {
        Ok,
        MissingInput,
        // Covers unknown email, wrong password, and disabled account — avoids enumeration.
        InvalidCredentials,
        TokenFailed,
    }

    public sealed class LoginResult
    {
        public LoginStatus Status { get; init; }
        public UserLoginResponse? User { get; init; }

        public static LoginResult Success(UserLoginResponse user) =>
            new() { Status = LoginStatus.Ok, User = user };

        public static LoginResult MissingInput() =>
            new() { Status = LoginStatus.MissingInput };

        public static LoginResult InvalidCredentials() =>
            new() { Status = LoginStatus.InvalidCredentials };

        public static LoginResult TokenFailed() =>
            new() { Status = LoginStatus.TokenFailed };
    }
}
