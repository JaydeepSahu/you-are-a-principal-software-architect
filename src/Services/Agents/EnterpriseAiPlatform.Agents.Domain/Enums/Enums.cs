namespace EnterpriseAiPlatform.Agents.Domain.Enums;

public enum StepStatus
{
    Pending = 0,
    Running = 1,
    Completed = 2,
    Failed = 3,
    WaitingForApproval = 4,
    Cancelled = 5,
    Skipped = 6
}

public enum PlanStatus
{
    Created = 0,
    Executing = 1,
    WaitingForApproval = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}

public enum ApprovalStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    TimedOut = 3
}

public enum ExecutionMode
{
    Sequential = 0,
    ParallelGroup = 1
}

public enum ToolParameterType
{
    String = 0,
    Number = 1,
    Boolean = 2,
    Object = 3,
    Array = 4
}
