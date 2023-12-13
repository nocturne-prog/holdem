using System;
using UnityEngine.UI;

interface SingletonePopup { }
public class UIPopup_OneButton : UIPopup, SingletonePopup
{
    public Text desc;
    public Action callback;

    public override void Init(params object[] args)
    {
        isBlurOff = false;

        base.Init(args);
        desc.text = (string)args[0];
        button_Close.onClick.AddListener(() =>
        {
            callback?.Invoke();
        });
    }
}
