using UnityEngine.UI;

public class UIPopup_HandForHand : UIPopup, SingletonePopup
{
    public override void Init(params object[] args)
    {
        isBlurOff = false;
        base.Init(args);
    }
}