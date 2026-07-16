using System.Net.Mail;
using System.Text.RegularExpressions;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.src.Domain.Identity.Entities;

public class User : AuditableEntity<Guid>
{
    public const int EmailMaxLength = 255;
    public const int UsernameMaxLength = 20;
    public const int UsernameMinLength = 3;
    public const int FirstNameMaxLength = 100;
    public const int LastNameMaxLength = 100;
    public const int FullnameMaxLength = 255;
    public const int PhoneNumberMaxLength = 20;
    public const int PasswordHashMaxLength = 72;
    public const int PreferredLocaleMaxLength = 10;

    private static readonly HashSet<string> ReserveUsernames = new(StringComparer.OrdinalIgnoreCase)
    {
        "admin", "administrator", "root", "system", "support",
        "null", "undefined", "me", "api", "auth", "profile", "roles", "operations"
    };

    private static readonly Regex UsernameRegex = new(@"^[A-Za-z][A-Za-z0-9_.]{2,19}$", RegexOptions.Compiled);
    private static readonly Regex PhoneNumberRegex = new(@"^(\+989\d{9}|09\d{9})$", RegexOptions.Compiled);
    private static readonly Regex LocaleRegex = new(@"^[a-zA-Z]{2}(?:-[a-zA-Z]{2})?$", RegexOptions.Compiled);

    public string Email { get; private set; } = default!;
    public string Username { get; private set; } = default!;
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string Fullname => $"{FirstName} {LastName}".Trim();
    public string PhoneNumber { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string? PreferredLocale { get; private set; }
    public bool IsActive { get; private set; } = true;
    

    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    public ICollection<RefreshTokenSession> RefreshTokenSessions { get; private set; } = new List<RefreshTokenSession>();


    public User() { }

    public static User CreateNew(
        string email,
        string username,
        string firstName,
        string lastName,
        string phoneNumber,
        string passwordHash,
        DateTimeOffset nowUtc,
        Guid? actorUserId,
        string? preferredLocale = null
        )
    {
        var newEmail = EnsureValidEmail(email);
        var newUsername = EnsureValidUsername(username);
        var newFirstName = EnsureValidFirstName(firstName);
        var newLastName = EnsureValidLastName(lastName);
        var newPhoneNumber = EnsureValidPhoneNumber(phoneNumber);
        var newPasswordHash = EnsureValidPasswordHash(passwordHash);
        var newPreferredLocale = EnsureValidPreferredLocale(preferredLocale);

    
        var user =  new User
        {
            Id = Guid.NewGuid(),
            Email = newEmail,
            Username = newUsername,
            FirstName = newFirstName,
            LastName = newLastName,
            PhoneNumber = newPhoneNumber,
            PasswordHash = newPasswordHash,
            PreferredLocale = newPreferredLocale,
            IsActive = true,
        };
        user.SetCreated(nowUtc, actorUserId ?? user.Id);
        return user;
    }

    public void ChangeProfile(
        string firstName,
        string lastName,
        string phoneNumber,
        DateTimeOffset nowUtc,
        Guid? actorUserId)
    {
        firstName = EnsureValidFirstName(firstName);
        lastName = EnsureValidLastName(lastName);
        phoneNumber = EnsureValidPhoneNumber(phoneNumber);

        if (FirstName == firstName &&
            LastName == lastName &&
            PhoneNumber == phoneNumber)
        {
            return;
        }

        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        Touch(nowUtc, actorUserId ?? Id);
    }


    public void ChangePreferredLocale(
    string? preferredLocale,
    DateTimeOffset nowUtc,
    Guid? actorUserId)
    {
        preferredLocale = EnsureValidPreferredLocale(preferredLocale);
        if (string.Equals(PreferredLocale, preferredLocale, StringComparison.Ordinal))
            return;

        PreferredLocale = preferredLocale;
        Touch(nowUtc, actorUserId ?? Id);
    }

    [Obsolete("Use ChangeProfile instead.")]
    public void ChangeFullName(string newFullName, DateTimeOffset nowUtc, Guid? actorUserId)
    {
        var (firstName, lastName) = SplitFullName(newFullName);
        ChangeProfile(firstName, lastName, PhoneNumber, nowUtc, actorUserId);
    }

    public void ChangePasswordHash(string newPasswordHash, DateTimeOffset nowUtc, Guid? actorUserId)
    {
        newPasswordHash = EnsureValidPasswordHash(newPasswordHash);
        if (PasswordHash == newPasswordHash) return;
        PasswordHash = newPasswordHash;
        Touch(nowUtc, actorUserId ?? Id);
    }

    public void ChangeUsername(string newUsername, DateTimeOffset nowUtc, Guid? actorUserId)
    {
        newUsername = EnsureValidUsername(newUsername);
        if (Username == newUsername) return;
        Username = newUsername;
        Touch(nowUtc, actorUserId ?? Id);
    }

    public void Deactivate(DateTimeOffset nowUtc, Guid? actorUserId)
    {
        if (!IsActive) return;
        IsActive = false;
        Touch(nowUtc, actorUserId ?? Id);
    }

    public void Activate(DateTimeOffset nowUtc, Guid? actorUserId)
    {
        if (IsActive) return;
        IsActive = true;
        Touch(nowUtc, actorUserId ?? Id);
    }

    [Obsolete("Use Deactivate instead.")]
    public void Deactive(DateTimeOffset nowUtc, Guid? actorUserId) => Deactivate(nowUtc, actorUserId);

    [Obsolete("Use Activate instead.")]
    public void Active(DateTimeOffset nowUtc, Guid? actorUserId) => Activate(nowUtc, actorUserId);

    public void AssignRole(int roleId, DateTimeOffset nowUtc, Guid? actorUserId)
    {
        Guard.True(
            roleId > 0,
            DomainErrorCodes.RoleIdInvalid,
            new Dictionary<string, object?> { ["roleId"] = roleId });

        if (UserRoles.Any(x => x.RoleId == roleId)) return;
        UserRoles.Add(UserRole.Create(Id, roleId, nowUtc, actorUserId ?? Id));

        Touch(nowUtc, actorUserId ?? Id);
    }

    public void RemoveRole(int roleId, DateTimeOffset nowUtc, Guid? actorUserId)
    {
        if (roleId <= 0)
        {
            throw DomainException.For(
                DomainErrorCodes.RoleIdInvalid,
                new Dictionary<string, object?> { ["roleId"] = roleId });
        }

        var userRole = UserRoles.FirstOrDefault(x => x.RoleId == roleId);
        if (userRole is null) return;
        UserRoles.Remove(userRole);
        Touch(nowUtc, actorUserId ?? Id);
    }

    public static string NormalizeUsername(string username) => (username ?? "").Trim();
    public static string NormalizeEmail(string email) => (email ?? "").Trim().ToLowerInvariant();
    public static string NormalizeFirstName(string firstName) => (firstName ?? "").Trim();
    public static string NormalizeLastName(string lastName) => (lastName ?? "").Trim();
    public static string NormalizePhoneNumber(string phoneNumber) => (phoneNumber ?? "").Trim();
    public static string NormalizeFullname(string fullname) => (fullname ?? "").Trim();

    public static (string FirstName, string LastName) SplitFullName(string fullname)
    {
        fullname = NormalizeFullname(fullname);
        Guard.NotEmpty(fullname, nameof(fullname));
        Guard.MaxLen(fullname, FullnameMaxLength, nameof(fullname));

        var parts = fullname.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length switch
        {
            1 => (parts[0], parts[0]),
            _ => (parts[0], parts[1])
        };
    }

    private static string EnsureValidUsername(string username)
    {
        username = NormalizeUsername(username);

        Guard.NotEmpty(username, nameof(username));
        Guard.AllowedLen(username, UsernameMaxLength, UsernameMinLength, nameof(username));

        if (ReserveUsernames.Contains(username))
            throw new DomainException("username is reserved.");

        if (!UsernameRegex.IsMatch(username))
            throw new DomainException("username format is invalid.");

        if (username.StartsWith('.') || username.EndsWith('.') || username.Contains(".."))
            throw new DomainException("username format is invalid.");

        return username;
    }
    private static string EnsureValidEmail(string email)
    {
        email = NormalizeEmail(email);

        Guard.NotEmpty(email, nameof(email));
        Guard.MaxLen(email, EmailMaxLength, nameof(email));

        try
        {
            var addr = new MailAddress(email);

            if (!string.Equals(addr.Address, email, StringComparison.OrdinalIgnoreCase))
                throw new DomainException("email format is invalid.");

            var at = email.LastIndexOf('@');
            if (at < 1 || at == email.Length - 1) 
                throw new DomainException("email format is invalid.");

            var domain = email[(at + 1)..];
            if (!domain.Contains('.')) 
                throw new DomainException("email format is invalid.");
        }
        catch
        {
            throw new DomainException("email format is invalid.");
        }
        return email;
    }
    private static string EnsureValidFirstName(string firstName)
    {
        firstName = NormalizeFirstName(firstName);

        Guard.NotEmpty(firstName, nameof(FirstName));
        Guard.MaxLen(firstName, FirstNameMaxLength, nameof(FirstName));

        return firstName;
    }
    private static string EnsureValidLastName(string lastName)
    {
        lastName = NormalizeLastName(lastName);

        Guard.NotEmpty(lastName, nameof(LastName));
        Guard.MaxLen(lastName, LastNameMaxLength, nameof(LastName));

        return lastName;
    }
    private static string EnsureValidPhoneNumber(string phoneNumber)
    {
        phoneNumber = NormalizePhoneNumber(phoneNumber);

        Guard.NotEmpty(phoneNumber, nameof(PhoneNumber));
        Guard.MaxLen(phoneNumber, PhoneNumberMaxLength, nameof(PhoneNumber));

        if (!PhoneNumberRegex.IsMatch(phoneNumber))
            throw new DomainException("phone number format is invalid.");

        return phoneNumber;
    }
    private static string EnsureValidPasswordHash(string passwordHash)
    {
        Guard.NotEmpty(passwordHash, nameof(passwordHash));
        Guard.MaxLen(passwordHash, PasswordHashMaxLength, nameof(passwordHash));
        return passwordHash;
    }
    private static string? EnsureValidPreferredLocale(string? preferredLocale)
    {
        if (string.IsNullOrWhiteSpace(preferredLocale)) return null;
        preferredLocale = preferredLocale.Trim().Replace('_', '-');
        Guard.MaxLen(preferredLocale, PreferredLocaleMaxLength, nameof(PreferredLocale));

        if (!LocaleRegex.IsMatch(preferredLocale))
        {
            throw DomainException.For(
                DomainErrorCodes.LocaleFormatInvalid,
                new Dictionary<string, object?> { ["locale"] = preferredLocale });
        }

        var parts = preferredLocale.Split('-', 2);
        return parts.Length == 1
            ? parts[0].ToLowerInvariant()
            : $"{parts[0].ToLowerInvariant()}-{parts[1].ToUpperInvariant()}";
    }
}
