namespace EnterpriseAiPlatform.SharedKernel;

public sealed record ErrorDetail(string Code, string Message)
{
    public static readonly ErrorDetail None = new("None", string.Empty);

    public static ErrorDetail Create(string code, string message)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Error code is required.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Error message is required.", nameof(message));
        }

        return new ErrorDetail(code, message);
    }
}
