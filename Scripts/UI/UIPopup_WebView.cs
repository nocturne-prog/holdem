using System;

public class UIPopup_WebView : UIPopup
{
    public override void Init(params object[] args)
    {
        isBlurOff = false;
        base.Init(args);
    }
}
