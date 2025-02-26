using System.Collections.Generic;
using BGC.Parameters;

[PropertyGroupTitle("Ease Behaviour", "EaseBehaviour")]
public interface IEaseBehaviour : IPropertyGroup
{

}

[PropertyChoiceTitle(title: "Ease Mirrored")]
[PropertyChoiceInfo("Will copy the Start Ease Behaviour")]
public class MirroredEaseBehaviour : CommonPropertyGroup, IEaseBehaviour
{

}

[PropertyChoiceTitle(title: "Ease Fixed")]
[EnumDropdownDisplay(nameof(EaseType), displayTitle: "Ease Type", initialValue: (int)EasingType.Linear, choiceListMethodName: nameof(EaseTypeChoices))]
[DoubleFieldDisplay(nameof(EaseDuration), "Ease duration", postfix: "ms")]
[DoubleFieldDisplay(nameof(EaseOffset), "Ease offset", postfix: "ms")]
public class FixedEaseBehaviour : CommonPropertyGroup, IEaseBehaviour
{
    [DisplayInputField(nameof(EaseType))]
    public EasingType EaseType { get; set; }

    [DisplayInputField(nameof(EaseDuration))]
    public double EaseDuration { get; set; }

    [DisplayInputField(nameof(EaseOffset))]
    public double EaseOffset { get; set; }

    public static List<ValueNamePair> EaseTypeChoices()
    {
        return new List<ValueNamePair>
            {
                new ((int) EasingType.EaseIn, EasingType.EaseIn.ToDisplayName()),
                new ((int) EasingType.EaseOut, EasingType.EaseOut.ToDisplayName()),
                new ((int) EasingType.EaseInOut, EasingType.EaseInOut.ToDisplayName()),
                new ((int) EasingType.Linear, EasingType.Linear.ToDisplayName()),
            };
    }
}
