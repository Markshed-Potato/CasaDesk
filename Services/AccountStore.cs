using System.Collections.Concurrent;
using Microsoft.AspNetCore.Identity;

namespace CasaDesk.Services;

public sealed class AccountStore
{
    private readonly ConcurrentDictionary<string, Account> _accounts = new(StringComparer.OrdinalIgnoreCase);
    private readonly PasswordHasher<Account> _passwordHasher = new();

    public bool TryRegister(string name, string email, string password, out Account? account)
    {
        var normalizedEmail = email.Trim();
        account = new Account(name.Trim(), normalizedEmail);
        account.PasswordHash = _passwordHasher.HashPassword(account, password);
        if (_accounts.TryAdd(normalizedEmail, account)) return true;
        account = null;
        return false;
    }

    public bool TryAuthenticate(string email, string password, out Account? account)
    {
        account = null;
        if (!_accounts.TryGetValue(email.Trim(), out var candidate)) return false;
        if (_passwordHasher.VerifyHashedPassword(candidate, candidate.PasswordHash, password) == PasswordVerificationResult.Failed) return false;
        account = candidate;
        return true;
    }

    public sealed class Account(string name, string email)
    {
        public string Name { get; } = name;
        public string Email { get; } = email;
        public string PasswordHash { get; set; } = string.Empty;
    }
}