namespace BGC.Parameters.Algorithms.ConstantStimulus
{
    [PropertyGroupTitle("Dimensions")]
    public interface IConstantStimulusDimensions : IPropertyGroup
    {
        int GetSteps(int dimension);
        int DimensionLimit { get; }
    }

    [PropertyChoiceTitle("1 Dimension", "1Dim")]
    [FieldMirrorDisplay("Dim1Steps", "Steps", "Dimension 1 Steps")]
    public class ConstantStim1Dim : StimulusPropertyGroup, IConstantStimulusDimensions
    {
        [DisplayInputField("Dim1Steps")]
        public int Dim1Steps { get; set; }

        int IConstantStimulusDimensions.GetSteps(int dimension)
        {
            switch (dimension)
            {
                case 0: return Dim1Steps;
                case 1: return 1;
                case 2: return 1;
                case 3: return 1;

                default:
                    UnityEngine.Debug.LogError($"Unexpected dimension: {dimension}");
                    return 0;
            }
        }

        int IConstantStimulusDimensions.DimensionLimit => 1;
    }


    [PropertyChoiceTitle("2 Dimensions", "2Dim")]
    [FieldMirrorDisplay("Dim1Steps", "Steps", "Dimension 1 Steps")]
    [FieldMirrorDisplay("Dim2Steps", "Steps", "Dimension 2 Steps")]
    public class ConstantStim2Dim : StimulusPropertyGroup, IConstantStimulusDimensions
    {
        [DisplayInputField("Dim1Steps")]
        public int Dim1Steps { get; set; }
        [DisplayInputField("Dim2Steps")]
        public int Dim2Steps { get; set; }

        int IConstantStimulusDimensions.GetSteps(int dimension)
        {
            switch (dimension)
            {
                case 0: return Dim1Steps;
                case 1: return Dim2Steps;
                case 2: return 1;
                case 3: return 1;

                default:
                    UnityEngine.Debug.LogError($"Unexpected dimension: {dimension}");
                    return 0;
            }
        }

        int IConstantStimulusDimensions.DimensionLimit => 2;
    }

    [PropertyChoiceTitle("3 Dimensions", "3Dim")]
    [FieldMirrorDisplay("Dim1Steps", "Steps", "Dimension 1 Steps")]
    [FieldMirrorDisplay("Dim2Steps", "Steps", "Dimension 2 Steps")]
    [FieldMirrorDisplay("Dim3Steps", "Steps", "Dimension 3 Steps")]
    public class ConstantStim3Dim : StimulusPropertyGroup, IConstantStimulusDimensions
    {
        [DisplayInputField("Dim1Steps")]
        public int Dim1Steps { get; set; }
        [DisplayInputField("Dim2Steps")]
        public int Dim2Steps { get; set; }
        [DisplayInputField("Dim3Steps")]
        public int Dim3Steps { get; set; }

        int IConstantStimulusDimensions.GetSteps(int dimension)
        {
            switch (dimension)
            {
                case 0: return Dim1Steps;
                case 1: return Dim2Steps;
                case 2: return Dim3Steps;
                case 3: return 1;

                default:
                    UnityEngine.Debug.LogError($"Unexpected dimension: {dimension}");
                    return 0;
            }
        }

        int IConstantStimulusDimensions.DimensionLimit => 3;
    }

    [PropertyChoiceTitle("4 Dimensions", "4Dim")]
    [FieldMirrorDisplay("Dim1Steps", "Steps", "Dimension 1 Steps")]
    [FieldMirrorDisplay("Dim2Steps", "Steps", "Dimension 2 Steps")]
    [FieldMirrorDisplay("Dim3Steps", "Steps", "Dimension 3 Steps")]
    [FieldMirrorDisplay("Dim4Steps", "Steps", "Dimension 4 Steps")]
    public class ConstantStim4Dim : StimulusPropertyGroup, IConstantStimulusDimensions
    {
        [DisplayInputField("Dim1Steps")]
        public int Dim1Steps { get; set; }
        [DisplayInputField("Dim2Steps")]
        public int Dim2Steps { get; set; }
        [DisplayInputField("Dim3Steps")]
        public int Dim3Steps { get; set; }
        [DisplayInputField("Dim4Steps")]
        public int Dim4Steps { get; set; }

        int IConstantStimulusDimensions.GetSteps(int dimension)
        {
            switch (dimension)
            {
                case 0: return Dim1Steps;
                case 1: return Dim2Steps;
                case 2: return Dim3Steps;
                case 3: return Dim4Steps;

                default:
                    UnityEngine.Debug.LogError($"Unexpected dimension: {dimension}");
                    return 0;
            }
        }

        int IConstantStimulusDimensions.DimensionLimit => 4;
    }
}
