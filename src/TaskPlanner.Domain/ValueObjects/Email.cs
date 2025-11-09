using System;
using System.Text.RegularExpressions;
using TaskPlanner.Domain.Exceptions;

namespace TaskPlanner.Domain.ValueObjects;

public sealed record Email
{
    private static readonly Regex EmailPattern = new(
        "^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Email address must be provided.");
        }

        var normalized = value.Trim();

        if (!EmailPattern.IsMatch(normalized))
        {
            throw new DomainException($"'{value}' is not a valid email address.");
        }

        return new Email(normalized);
    }

    public override string ToString() => Value;
}
