namespace EnterpriseAiPlatform.SharedKernel;

public class Result
{
    protected Result(bool isSuccess, ErrorDetail error)
    {
        if (isSuccess && error != ErrorDetail.None)
        {
            throw new ArgumentException("Successful results cannot contain an error.", nameof(error));
        }

        if (!isSuccess && error == ErrorDetail.None)
        {
            throw new ArgumentException("Failed results require an error.", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public ErrorDetail Error { get; }

    public static Result Success()
    {
        return new Result(true, ErrorDetail.None);
    }

    public static Result Failure(ErrorDetail error)
    {
        return new Result(false, error);
    }

    public static Result<TValue> Success<TValue>(TValue value)
    {
        return new Result<TValue>(value);
    }

    public static Result<TValue> Failure<TValue>(ErrorDetail error)
    {
        return new Result<TValue>(error);
    }
}

public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    internal Result(TValue value)
        : base(true, ErrorDetail.None)
    {
        _value = value;
    }

    internal Result(ErrorDetail error)
        : base(false, error)
    {
    }

    public TValue Value
    {
        get
        {
            if (IsFailure)
            {
                throw new InvalidOperationException("The value of a failed result cannot be accessed.");
            }

            return _value!;
        }
    }

    public static implicit operator Result<TValue>(TValue value) => new(value);
}
