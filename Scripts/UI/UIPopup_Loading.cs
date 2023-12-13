using UnityEngine.UI;
using UnityEngine;

public class UIPopup_Loading : UIPopup
{
    public Image img;
    public Transform desc;

    public override void Init(params object[] args)
    {
        isBlurOff = false;
        img.gameObject.AddTween(new TweenRotateTo(0, 0, -360, 1f).Repeat(-1));

        bool networkLoading = (bool)args[0];
        desc.SetActive(networkLoading);
    }
}

