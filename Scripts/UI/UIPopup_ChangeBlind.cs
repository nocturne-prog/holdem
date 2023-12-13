using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIPopup_ChangeBlind : UIPopup, SingletonePopup
{
    public Text text_level;
    public Text text_blind;

    public override void Init(params object[] args)
    {
        isBlurOff = false;

        MPSC_LevelInfo info = (MPSC_LevelInfo)args[0];

        text_level.text = $"레벨 {info.bLevel}";
        text_blind.text = $"{info.n64SBlind} / {info.n64BBlind} / {info.n64Ante}";

        Invoke(nameof(Close), 2f);
    }
}
