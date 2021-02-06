namespace BGC.Parameters.Algorithms.StagedStaircase
{
    public enum StaircaseStage
    {
        Stage1 = 0,
        Stage2,
        Stage3,
        MAX
    }


    [PropertyGroupTitle("Stages")]
    public interface IStaircaseStage : IPropertyGroup
    {
        int GetReversals(StaircaseStage stage);
        int GetStepsUp(StaircaseStage stage);
        int GetStepsDown(StaircaseStage stage);
        StaircaseStage GetStageLimit();
    }

    [PropertyChoiceTitle("1 Stage", "1Stage")]
    [FieldMirrorDisplay("Stage1StepsUp", "BigSteps", "Stage 1 Steps Up")]
    [FieldMirrorDisplay("Stage1StepsDown", "BigSteps", "Stage 1 Steps Down")]
    public class Staircase1Stage : StimulusPropertyGroup, IStaircaseStage
    {
        [DisplayInputField("Stage1StepsUp")]
        public int Stage1StepsUp { get; set; }
        [DisplayInputFieldKey("Stage1StepsUp")]
        public string Stage1StepsUpKey { get; set; }

        [DisplayInputField("Stage1StepsDown")]
        public int Stage1StepsDown { get; set; }
        [DisplayInputFieldKey("Stage1StepsDown")]
        public string Stage1StepsDownKey { get; set; }

        int IStaircaseStage.GetStepsUp(StaircaseStage parameter)
        {
            switch (parameter)
            {
                case StaircaseStage.Stage1: return Stage1StepsUp;
                case StaircaseStage.Stage2: return 0;
                case StaircaseStage.Stage3: return 0;

                default:
                    UnityEngine.Debug.LogError($"Unexpected StaircaseStage: {parameter}");
                    return 0;
            }
        }

        int IStaircaseStage.GetStepsDown(StaircaseStage parameter)
        {
            switch (parameter)
            {
                case StaircaseStage.Stage1: return Stage1StepsDown;
                case StaircaseStage.Stage2: return 0;
                case StaircaseStage.Stage3: return 0;

                default:
                    UnityEngine.Debug.LogError($"Unexpected StaircaseStage: {parameter}");
                    return 0;
            }
        }

        int IStaircaseStage.GetReversals(StaircaseStage parameter)
        {
            switch (parameter)
            {
                case StaircaseStage.Stage1: return 0;
                case StaircaseStage.Stage2: return 0;
                case StaircaseStage.Stage3: return 0;

                default:
                    UnityEngine.Debug.LogError($"Unexpected StaircaseStage: {parameter}");
                    return 0;
            }
        }

        StaircaseStage IStaircaseStage.GetStageLimit() => StaircaseStage.Stage1;
    }

    [PropertyChoiceTitle("2 Stages", "2Stages")]
    [FieldMirrorDisplay("Stage1Reversals", "Reversals", "Stage 1 Reversals")]
    [FieldMirrorDisplay("Stage1StepsUp", "BigSteps", "Stage 1 Steps Up")]
    [FieldMirrorDisplay("Stage1StepsDown", "BigSteps", "Stage 1 Steps Down")]
    [FieldMirrorDisplay("Stage2StepsUp", "Steps", "Stage 2 Steps Up")]
    [FieldMirrorDisplay("Stage2StepsDown", "Steps", "Stage 2 Steps Down")]
    public class Staircase2Stage : StimulusPropertyGroup, IStaircaseStage
    {
        [DisplayInputField("Stage1Reversals")]
        public int Stage1Reversals { get; set; }
        [DisplayInputFieldKey("Stage1Reversals")]
        public string Stage1ReversalsKey { get; set; }

        [DisplayInputField("Stage1StepsUp")]
        public int Stage1StepsUp { get; set; }
        [DisplayInputFieldKey("Stage1StepsUp")]
        public string Stage1StepsUpKey { get; set; }

        [DisplayInputField("Stage1StepsDown")]
        public int Stage1StepsDown { get; set; }
        [DisplayInputFieldKey("Stage1StepsDown")]
        public string Stage1StepsDownKey { get; set; }

        [DisplayInputField("Stage2StepsUp")]
        public int Stage2StepsUp { get; set; }
        [DisplayInputFieldKey("Stage2StepsUp")]
        public string Stage2StepsUpKey { get; set; }

        [DisplayInputField("Stage2StepsDown")]
        public int Stage2StepsDown { get; set; }
        [DisplayInputFieldKey("Stage2StepsDown")]
        public string Stage2StepsDownKey { get; set; }

        int IStaircaseStage.GetStepsUp(StaircaseStage parameter)
        {
            switch (parameter)
            {
                case StaircaseStage.Stage1: return Stage1StepsUp;
                case StaircaseStage.Stage2: return Stage2StepsUp;
                case StaircaseStage.Stage3: return 0;

                default:
                    UnityEngine.Debug.LogError($"Unexpected StaircaseStage: {parameter}");
                    return 0;
            }
        }

        int IStaircaseStage.GetStepsDown(StaircaseStage parameter)
        {
            switch (parameter)
            {
                case StaircaseStage.Stage1: return Stage1StepsDown;
                case StaircaseStage.Stage2: return Stage2StepsDown;
                case StaircaseStage.Stage3: return 0;

                default:
                    UnityEngine.Debug.LogError($"Unexpected StaircaseStage: {parameter}");
                    return 0;
            }
        }

        int IStaircaseStage.GetReversals(StaircaseStage parameter)
        {
            switch (parameter)
            {
                case StaircaseStage.Stage1: return Stage1Reversals;
                case StaircaseStage.Stage2: return 0;
                case StaircaseStage.Stage3: return 0;

                default:
                    UnityEngine.Debug.LogError($"Unexpected StaircaseStage: {parameter}");
                    return 0;
            }
        }

        StaircaseStage IStaircaseStage.GetStageLimit() => StaircaseStage.Stage2;
    }

    [PropertyChoiceTitle("3 Stages", "3Stages")]
    [FieldMirrorDisplay("Stage1Reversals", "Reversals", "Stage 1 Reversals")]
    [FieldMirrorDisplay("Stage1StepsUp", "BigSteps", "Stage 1 Steps Up")]
    [FieldMirrorDisplay("Stage1StepsDown", "BigSteps", "Stage 1 Steps Down")]
    [FieldMirrorDisplay("Stage2Reversals", "Reversals", "Stage 2 Reversals")]
    [FieldMirrorDisplay("Stage2StepsUp", "Steps", "Stage 2 Steps Up")]
    [FieldMirrorDisplay("Stage2StepsDown", "Steps", "Stage 2 Steps Down")]
    [FieldMirrorDisplay("Stage3StepsUp", "Steps", "Stage 3 Steps Up")]
    [FieldMirrorDisplay("Stage3StepsDown", "Steps", "Stage 3 Steps Down")]
    public class Staircase3Stage : StimulusPropertyGroup, IStaircaseStage
    {
        [DisplayInputField("Stage1Reversals")]
        public int Stage1Reversals { get; set; }
        [DisplayInputFieldKey("Stage1Reversals")]
        public string Stage1ReversalsKey { get; set; }

        [DisplayInputField("Stage1StepsUp")]
        public int Stage1StepsUp { get; set; }
        [DisplayInputFieldKey("Stage1StepsUp")]
        public string Stage1StepsUpKey { get; set; }

        [DisplayInputField("Stage1StepsDown")]
        public int Stage1StepsDown { get; set; }
        [DisplayInputFieldKey("Stage1StepsDown")]
        public string Stage1StepsDownKey { get; set; }

        [DisplayInputField("Stage2Reversals")]
        public int Stage2Reversals { get; set; }
        [DisplayInputFieldKey("Stage2Reversals")]
        public string Stage2ReversalsKey { get; set; }

        [DisplayInputField("Stage2StepsUp")]
        public int Stage2StepsUp { get; set; }
        [DisplayInputFieldKey("Stage2StepsUp")]
        public string Stage2StepsUpKey { get; set; }

        [DisplayInputField("Stage2StepsDown")]
        public int Stage2StepsDown { get; set; }
        [DisplayInputFieldKey("Stage2StepsDown")]
        public string Stage2StepsDownKey { get; set; }

        [DisplayInputField("Stage3StepsUp")]
        public int Stage3StepsUp { get; set; }
        [DisplayInputFieldKey("Stage3StepsUp")]
        public string Stage3StepsUpKey { get; set; }

        [DisplayInputField("Stage3StepsDown")]
        public int Stage3StepsDown { get; set; }
        [DisplayInputFieldKey("Stage3StepsDown")]
        public string Stage3StepsDownKey { get; set; }

        int IStaircaseStage.GetStepsUp(StaircaseStage parameter)
        {
            switch (parameter)
            {
                case StaircaseStage.Stage1: return Stage1StepsUp;
                case StaircaseStage.Stage2: return Stage2StepsUp;
                case StaircaseStage.Stage3: return Stage3StepsUp;

                default:
                    UnityEngine.Debug.LogError($"Unexpected StaircaseStage: {parameter}");
                    return 0;
            }
        }

        int IStaircaseStage.GetStepsDown(StaircaseStage parameter)
        {
            switch (parameter)
            {
                case StaircaseStage.Stage1: return Stage1StepsDown;
                case StaircaseStage.Stage2: return Stage2StepsDown;
                case StaircaseStage.Stage3: return Stage3StepsDown;

                default:
                    UnityEngine.Debug.LogError($"Unexpected StaircaseStage: {parameter}");
                    return 0;
            }
        }

        int IStaircaseStage.GetReversals(StaircaseStage parameter)
        {
            switch (parameter)
            {
                case StaircaseStage.Stage1: return Stage1Reversals;
                case StaircaseStage.Stage2: return Stage2Reversals;
                case StaircaseStage.Stage3: return 0;

                default:
                    UnityEngine.Debug.LogError($"Unexpected StaircaseStage: {parameter}");
                    return 0;
            }
        }

        StaircaseStage IStaircaseStage.GetStageLimit() => StaircaseStage.Stage3;
    }
}
