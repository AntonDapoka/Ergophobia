namespace MG_BlocksEngine2.Block.Instruction
{
    public enum StepResultType
    {
        Completed,
        EnterBody,
        SkipBody,
        WaitForSeconds,
    }

    public struct StepResult
    {
        public StepResultType Type;
        public int BodySectionIndex;
        public float WaitDuration;

        public static StepResult Completed => new StepResult { Type = StepResultType.Completed };
        public static StepResult EnterBody(int sectionIndex = 0) => new StepResult { Type = StepResultType.EnterBody, BodySectionIndex = sectionIndex };
        public static StepResult SkipBody => new StepResult { Type = StepResultType.SkipBody };
        public static StepResult WaitForSeconds(float duration) => new StepResult { Type = StepResultType.WaitForSeconds, WaitDuration = duration };
    }
}
