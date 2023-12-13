using System;
using UnityEngine;
using UnityEngine.UI;

public class UIPopup_Error : UIPopup, SingletonePopup
{
    [SerializeField] private Text desc;
    [SerializeField] private Button ok;

    public Action callbackOk;

    public override void Init(params object[] args)
    {
        isBlurOff = false;

        base.Init(args);

        desc.text = (string)args[0];
        ok.SetButton(() =>
        {
            Close();
            callbackOk?.Invoke();
        });
    }
}
