using Microsoft.AspNetCore.Identity;

namespace Vbodlaci.Web.Application.Security;

/// <summary>
/// Czech translations for the Identity errors that can surface on the
/// custom authentication pages (login and password recovery).
/// </summary>
public sealed class CzechIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError()
        => new() { Code = nameof(DefaultError), Description = "Došlo k neočekávané chybě." };

    public override IdentityError InvalidToken()
        => new() { Code = nameof(InvalidToken), Description = "Odkaz pro obnovu hesla je neplatný nebo vypršel. Vyžádej si prosím nový." };

    public override IdentityError PasswordTooShort(int length)
        => new() { Code = nameof(PasswordTooShort), Description = $"Heslo musí mít alespoň {length} znaků." };

    public override IdentityError PasswordRequiresDigit()
        => new() { Code = nameof(PasswordRequiresDigit), Description = "Heslo musí obsahovat alespoň jednu číslici." };

    public override IdentityError PasswordRequiresUpper()
        => new() { Code = nameof(PasswordRequiresUpper), Description = "Heslo musí obsahovat alespoň jedno velké písmeno." };

    public override IdentityError PasswordRequiresLower()
        => new() { Code = nameof(PasswordRequiresLower), Description = "Heslo musí obsahovat alespoň jedno malé písmeno." };

    public override IdentityError PasswordRequiresNonAlphanumeric()
        => new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Heslo musí obsahovat alespoň jeden speciální znak." };

    public override IdentityError PasswordMismatch()
        => new() { Code = nameof(PasswordMismatch), Description = "Nesprávné heslo." };
}
