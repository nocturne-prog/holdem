
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIPopup_RestInfo : UIPopup, SingletonePopup
{
    public Text text_desc;
    public Text text_time;

    DateTime t;
    public override void Init(params object[] args)
    {
        isBlurOff = false;

        text_desc.text = $"{(string)args[0]}";
        uint timer = (uint)args[1];

        t = DateTime.Now.AddSeconds(timer);
        InvokeRepeating(nameof(UpdateTimer), 0, 1f);
    }

    public void UpdateTimer()
    {
        TimeSpan timer = t - DateTime.Now;

        if (timer.Ticks < 0)
        {
            CancelInvoke(nameof(UpdateTimer));
            Close();
        }

        text_time.text = timer.ToString(@"mm\:ss");
    }
}
