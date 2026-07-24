#pragma warning disable CA1716

namespace EnterpriseAiPlatform.SharedKernel;

public sealed record Error(string Code, string Message, IReadOnlyList<ErrorDetail>? Details = null)
{
    public static Error None => new("None", string.Empty);

    public static Error Create(string code, string message)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Error code is required.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Error message is required.", nameof(message));
        }

        return new Error(code, message);
    }

    public static Error Create(string code, string message, IReadOnlyList<ErrorDetail> details)
        => new(code, message, details);

    public static implicit operator ErrorDetail(Error error) => new(error.Code, error.Message);
}

#pragma warning restore CA1716
