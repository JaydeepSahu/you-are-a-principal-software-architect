namespace EnterpriseAiPlatform.Evaluation.Domain;

public readonly struct EvaluationId
{
    private EvaluationId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static EvaluationId New() => new(Guid.NewGuid());

    public static EvaluationId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Evaluation identifier cannot be empty.", nameof(value));
        }

        return new EvaluationId(value);
    }

    public override string ToString() => Value.ToString("D");
}
