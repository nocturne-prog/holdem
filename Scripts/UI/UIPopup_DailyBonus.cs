using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPopup_DailyBonus : UIPopup
{
    [SerializeField] private Text text_timer;
    [SerializeField] private Item_DailyBonus_Reward[] icon;

    long tick = 0;

    public override void Init(params object[] args)
    {
        base.Init(args);

        LMSC_UserAttendanceInfoRes info = (LMSC_UserAttendanceInfoRes)args[0];
        //Debug.LogError($"{info.rAttendance.bRewardDay}, {info.rAttendance.tDailyReset}, {info.rAttendance.tWeeklyReset}");

        tick = info.rAttendance.tWeeklyReset;
        InvokeRepeating(nameof(UpdateTimer), 0, 1f);

        for (int i = 0; i < info.rReward.Length; i++)
        {
            icon[i].Today = i == info.rAttendance.bRewardDay && info.rAttendance.bRewardEnable == 1;
            icon[i].Complete = i < info.rAttendance.bRewardDay;

            if (info.rAttendance.bRewardEnable == 1)
            {
                icon[i].Disable = i > info.rAttendance.bRewardDay;
            }
            else
            {
                icon[i].Disable = i >= info.rAttendance.bRewardDay;
            }
        }
    }

    public void UpdateTimer()
    {
        TimeSpan time = Util.GetLeft(tick);
        text_timer.text = $"{Math.Abs(time.Days)}일 {time:hh\\:mm\\:ss}";
    }
}
