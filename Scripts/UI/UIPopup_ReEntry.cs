using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class UIPopup_ReEntry : UIPopup
{
    [Header("Top")]
    [SerializeField] private Text text_title;
    [SerializeField] private Text text_money;

    [Header("Middle")]
    [SerializeField] private Text text_content_title;
    [SerializeField] private Text text_cost;
    [SerializeField] private Text text_cost_sum;
    [SerializeField] private Text text_chip;

    [Header("Bottom")]
    [SerializeField] private Text text_count;
    [SerializeField] private Transform trf_time;
    [SerializeField] private Text text_timer_title;
    [SerializeField] private Text text_time;
    [SerializeField] private Button btn_ok;

    DateTime limitTime;
    public override void Init(params object[] args)
    {
        isBlurOff = false;

        int mode = (int)args[0];

        //Debug.LogError($"UIPopup_ReEntry :: {mode}");

        /// mode == 0 rebuy
        /// mode == 1 addon
        /// mode == 2 reentry

        text_title.text = mode == 2 ? "리엔트리" : mode == 1 ? "애드온" : "리바이";
        text_content_title.text = mode == 2 ? "토너먼트 리엔트리 비용" : mode == 1 ? "토너먼트 애드온 비용" : "토너먼트 리바이 비용";
        trf_time.SetActive(mode < 2);
        text_timer_title.text = mode == 0 ? "리바이 가능 시간" : "애드온 가능 시간";

        var info = GameData.TournamentTableList.FirstOrDefault(x => x.Number == GameManager.a.CurrentTournamentNo);

        text_money.text = $"{Util.GetMoneyString(GameData.Player.AvailMoney)}";

        if (mode < 2)
        {
            MPSC_MoreChipsInfo p = (MPSC_MoreChipsInfo)args[1];

            text_cost.text = $"{Util.GetMoneyString(mode == 0 ? info.t_info.dwBuyin : info.s_info.dwAddon)} + {Util.GetMoneyString(mode == 0 ? info.t_info.dwFee : info.s_info.dwAddonFee)}";
            text_cost_sum.text = $"{Util.GetMoneyString(mode == 0 ? info.t_info.dwBuyin : info.s_info.dwAddon)}";
            text_chip.text = string.Format("{0:N0}", mode == 0 ? info.s_info.dwNormalChips : info.s_info.dwAddonChips);
            text_count.text = $"{p.bCount}";

            limitTime = DateTime.Now.AddSeconds(p.bWaitTime);
            text_time.text = p.bWaitTime.ToString("00");

            InvokeRepeating(nameof(UpdateTimeText), 0, 1f);
            btn_ok.SetButton(() =>
            {
                MPCS_MoreChipsReq pck = new MPCS_MoreChipsReq();
                pck.bButton = (byte)(p.bButton ^ FPDefine_T.MD_RAB.NONE);
                pck.bMethod = 0;
                InGameManager.a.socket.Send(pck);

                Close();
            });

            button_Close.onClick.AddListener(() =>
            {
                MPCS_MoreChipsReq pck = new MPCS_MoreChipsReq();
                pck.bButton = (byte)(FPDefine_T.MD_RAB.NONE);
                pck.bMethod = 0;
                InGameManager.a.socket.Send(pck);
            });
        }
        else
        {
            LMSC_T_ReBuy_ReEntry p = (LMSC_T_ReBuy_ReEntry)args[1];

            text_cost.text = $"{Util.GetMoneyString(info.t_info.dwBuyin)} + {Util.GetMoneyString(info.t_info.dwFee)}";
            text_cost_sum.text = $"{Util.GetMoneyString(info.t_info.dwBuyin + info.t_info.dwFee)}";
            text_chip.text = info.s_info.dwNormalChips.ToString("N0");
            text_count.text = $"{info.s_info.bReentryCount - p.nUserReEntryCount + 1}";

            btn_ok.SetButton(() =>
            {
                UIManager.a.OpenLoading();
                GameManager.a.OnClickReEntry();
            });
        }

        base.Init(args);
    }

    public void UpdateTimeText()
    {
        TimeSpan time = limitTime - DateTime.Now;

        if (time.Ticks <= 0)
        {
            CancelInvoke(nameof(UpdateTimeText));
            Close();
        }

        text_time.text = time.ToString(@"ss");
    }
}
