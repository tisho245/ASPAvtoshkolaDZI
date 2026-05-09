using Microsoft.AspNetCore.Identity;

namespace Avtoshkola_DZI.Data
{
    /// <summary>
    /// Български превод на стандартните Identity грешки.
    /// </summary>
    public class BgIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DefaultError() =>
            new() { Code = nameof(DefaultError), Description = "Възникна неочаквана грешка." };

        public override IdentityError ConcurrencyFailure() =>
            new() { Code = nameof(ConcurrencyFailure), Description = "Записът беше променен от друг потребител. Моля, опитайте отново." };

        public override IdentityError DuplicateEmail(string email) =>
            new() { Code = nameof(DuplicateEmail), Description = $"E-mail адресът \"{email}\" вече е регистриран." };

        public override IdentityError DuplicateUserName(string userName) =>
            new() { Code = nameof(DuplicateUserName), Description = $"Потребителското име \"{userName}\" вече се използва." };

        public override IdentityError InvalidEmail(string? email) =>
            new() { Code = nameof(InvalidEmail), Description = $"E-mail адресът \"{email}\" е невалиден." };

        public override IdentityError InvalidUserName(string? userName) =>
            new() { Code = nameof(InvalidUserName), Description = $"Потребителското име \"{userName}\" е невалидно." };

        public override IdentityError PasswordTooShort(int length) =>
            new() { Code = nameof(PasswordTooShort), Description = $"Паролата трябва да е поне {length} символа." };

        public override IdentityError PasswordRequiresDigit() =>
            new() { Code = nameof(PasswordRequiresDigit), Description = "Паролата трябва да съдържа поне една цифра (0-9)." };

        public override IdentityError PasswordRequiresLower() =>
            new() { Code = nameof(PasswordRequiresLower), Description = "Паролата трябва да съдържа поне една малка буква (a-z)." };

        public override IdentityError PasswordRequiresUpper() =>
            new() { Code = nameof(PasswordRequiresUpper), Description = "Паролата трябва да съдържа поне една главна буква (A-Z)." };

        public override IdentityError PasswordRequiresNonAlphanumeric() =>
            new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Паролата трябва да съдържа поне един специален символ." };

        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) =>
            new() { Code = nameof(PasswordRequiresUniqueChars), Description = $"Паролата трябва да съдържа поне {uniqueChars} различни символа." };
    }
}

