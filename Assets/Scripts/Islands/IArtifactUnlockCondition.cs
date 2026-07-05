public interface IArtifactUnlockCondition
{
    bool IsComplete { get; }
    string Prompt { get; }
}
