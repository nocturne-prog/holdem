using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BonusSpin : MonoBehaviour
{
    private Button button;
    [SerializeField] private Transform obj_moveWheel;
    [SerializeField] private Transform obj_wheel_dimming;
    [SerializeField] private Transform obj_able;
    [SerializeField] private Transform obj_disable;
    [SerializeField] private Text text_time;

    LMSC_UserRouletteInfoRes info;

    bool Active
    {
        set
        {
            obj_able.SetActive(value);
            obj_disable.SetActive(!value);
            obj_wheel_dimming.SetActive(!value);

            if (value)
            {
                CancelInvoke(nameof(UpdateTime));
                InvokeRepeating(nameof(ActiveAni), 0, 1f);
                obj_moveWheel.gameObject.AddTween(new TweenRotateTo(0, 0, -360, 4f).Repeat(-1));

            }
            else
            {
                obj_moveWheel.gameObject.KillTween();
                CancelInvoke(nameof(ActiveAni));
                InvokeRepeating(nameof(UpdateTime), 0, 1f);
            }
        }
    }

    public void ActiveAni()
    {
        obj_able.gameObject.AddTween(new TweenMoveTo(new Vector3(30, 0), 0.4f)
                             .Next(new TweenMoveTo(new Vector3(-30, 0), 0.1f)));
    }

    public void UpdateTime()
    {
        TimeSpan t = Util.GetLeft(Math.Min(info.rRoulette.tHourReset, info.rRoulette.tDailyReset));
        text_time.text = $"{t:hh\\:mm\\:ss}";

        //Debug.LogError($"Total Second :: {t.TotalSeconds}");

        if (t.TotalSeconds >= 0)
        {
            NetworkManager.a.CheckRoulette(res =>
            {
                UpdateData(res);
            });
        }

        //if (t.TotalSeconds > 0)
        //{
        //    NetworkManager.a.CheckRoulette(res =>
        //    {
        //        if (res.rRoulette.bRewardEnable == 1)
        //        {
        //            CancelInvoke(nameof(UpdateTime));
        //            InvokeRepeating(nameof(ActiveAni), 0, 1f);
        //            obj_moveWheel.gameObject.AddTween(new TweenRotateTo(0, 0, -360, 4f).Repeat(-1));
        //        }
        //    });
        //}
    }

    public void UpdateData(LMSC_UserRouletteInfoRes p)
    {
        info = p;
        Active = info.rRoulette.bRewardEnable == 1;
    }

    public void Start()
    {
        button = transform.GetComponent<Button>();
        button.SetButton(() =>
        {
            if (info.rRoulette.bRewardEnable == 1)
                LobbyManager.a.OpenBonusSpinPopup();
        });
    }
}
