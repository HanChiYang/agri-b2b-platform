namespace AgriB2B.Api.Data.Entities;

/// <summary>
/// 登入帳號，管理員和經銷商共用。
/// </summary>
public class User : EntityBase
{
    public required string Account { get; set; }

    public required string DisplayName { get; set; }

    public required string PasswordHash { get; set; }

    // 沒有預設值：忘了指定時編譯就會失敗，不會不小心建出管理員帳號
    public required UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    // 新帳號的密碼由管理員設定，第一次登入要自己改
    public bool MustChangePassword { get; set; } = true;

    public required string SecurityStamp { get; set; }

    public int FailedLoginCount { get; set; }

    public DateTimeOffset? LockoutUntil { get; set; }

    public DateTimeOffset? LastLoginAt { get; set; }
}
