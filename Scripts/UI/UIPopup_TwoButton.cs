
using System;
using UnityEngine.UI;

public class UIPopup_TwoButton : UIPopup, SingletonePopup
{
    public Button button_ok;
    public Text desc;
    public Action ok_callback;

    public override void Init(params object[] args)
    {
        isBlurOff = false;

        base.Init(args);
        desc.text = (string)args[0];

        button_ok.onClick.AddListener(() =>
        {
            ok_callback?.Invoke();
        });
    }
}
