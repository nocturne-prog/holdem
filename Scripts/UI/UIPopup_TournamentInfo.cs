using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIPopup_TournamentInfo : UIPopup
{
    [System.Serializable]
    public class Tab
    {
        public ToggleEx toggle;
        public Transform[] obj_on;
        public Transform[] obj_off;

        public bool isOn
        {
            get { return toggle.isOn; }
            set
            {
                for (int i = 0; i < obj_on.Length; i++)
                {
                    obj_on[i].SetActive(value);
                }

                for (int i = 0; i < obj_off.Length; i++)
                {
                    obj_off[i].SetActive(!value);
                }
            }
        }
    }

    public ToggleGroupEx tabGroup;
    public Button button_cancel;
    public Button button_ok;
    public Button button_play;

    public Tab tab_info;
    public Tab tab_reward;
    public Tab tab_blind;

    [Header("Info Tab")]
    public Text text_title;
    public Text text_startTime;
    public Text text_remainingTime;
    public Text text_deadLineTime;
    public Text text_reentry;
    public Text text_buyin;
    public Text text_addon;
    public Text text_rebuy;
    public Text text_userCount;
    public Text text_leastUserCount;
    public Transform trf_remainingTime;
    public Transform trf_proceedTime;
    public ToggleEx toggle_userTicket;

    [Header("Reward Tab")]
    public Text text_totalReward;
    public RectTransform reward_contents;

    [Header("Blind Tab")]
    public RectTransform blind_contents;

    private int TournamentNo;
    private bool useTicket = false;
    private LMSC_TLBasicInfo4 info;
    private LMSC_TLBlindOption option;
    private LMSC_PrizebyEntryTable prize;

    public override void Init(params object[] args)
    {
        info = (LMSC_TLBasicInfo4)args[0];
        option = (LMSC_TLBlindOption)args[1];
        prize = (LMSC_PrizebyEntryTable)args[2];

        TournamentTableItem.BUTTON_STATE state = (TournamentTableItem.BUTTON_STATE)args[3];
        TournamentNo = info.t_tour_info.nTournamentNo;
        InfoInit();
        BlindInit();
        RewardInit();

        button_ok.SetButton(() =>
        {
            button_ok.interactable = false;
            toggle_userTicket.interactable = false;

            //var t = GameData.TournamentTableList.FirstOrDefault(x => x.t_info.bParticipated == 1 && !x.info.State.HasFlagLeast(FHDefineMC.MD_CTS.END, FHDefineMC.MD_CTS.HIDE, FHDefineMC.MD_CTS.CRASH));

            //if (t != null)
            //{
            //    UIManager.a.OpenPopup<UIPopup_OneButton>(UIManager.POPUP_TYPE.NORMAL, "진행중인 토너먼트가 존재하여 신청이 불가능 합니다.");
            //    return;
            //}

            LMCS_TLRegistrationReq regist = new LMCS_TLRegistrationReq();
            regist.nTournamentNo = info.t_tour_info.nTournamentNo;
            regist.bRegistrationType = (byte)(useTicket ? FPDefineMC.MD_TRT_TICKET : FPDefineMC.MD_TRT_BUYIN);

            NetworkManager.Lobby.Send<LMSC_TLRegistrationRes>(regist, pck =>
            {
                if (pck.bErrorCode == LMSC_TLRegistrationRes.Code.ER_NO)
                {
                    var popup = UIManager.a.OpenPopup<UIPopup_OneButton>(UIManager.POPUP_TYPE.NORMAL, "신청이 완료 되었습니다.");
                    popup.callback += () =>
                    {
                        button_cancel.interactable = true;
                        SetButton(TournamentTableItem.BUTTON_STATE.Applying);
                    };
                }
                else if (pck.bErrorCode == LMSC_TLRegistrationRes.Code.ER_NO_TICKET)
                {
                    var popup = UIManager.a.OpenPopup<UIPopup_OneButton>(UIManager.POPUP_TYPE.NORMAL, "티켓이 부족하여 신청할 수 없습니다.");
                }
                else
                {
                    UIManager.a.OpenPopup<UIPopup_OneButton>(UIManager.POPUP_TYPE.NORMAL, Const.TL_REGI_ERROR_MSG[pck.bErrorCode]);
                }
            });
        });

        button_cancel.SetButton(() =>
        {
            button_cancel.interactable = false;

            LMCS_TLRegistrationReq regist = new LMCS_TLRegistrationReq
            {
                bRegistrationType = FPDefineMC.MD_TRT_BUYIN_CANCEL,
                nTournamentNo = info.t_tour_info.nTournamentNo
            };

            NetworkManager.Lobby.Send<LMSC_TLRegistrationRes>(regist, res =>
            {
                if (res.bErrorCode == LMSC_TLRegistrationRes.Code.ER_NO)
                {
                    var popup = UIManager.a.OpenPopup<UIPopup_OneButton>(UIManager.POPUP_TYPE.NORMAL, "취소 되었습니다.");
                    popup.callback += () =>
                    {
                        SetButton(TournamentTableItem.BUTTON_STATE.Accepting);
                        button_ok.interactable = true;
                        toggle_userTicket.interactable = true;
                    };
                }
                else
                {
                    UIManager.a.OpenPopup<UIPopup_OneButton>(UIManager.POPUP_TYPE.NORMAL, Const.TL_REGI_ERROR_MSG[res.bErrorCode]);
                }
            });
        });

        button_play.SetButton(() =>
        {
            button_play.interactable = false;
            GameManager.a.EnterTournament(info.TournamentNo, GameData.TGameServerInfo.t_gs_info);
        });

        tabGroup.onValueChanged.AddListener((v) =>
        {
            OnClickTab(v);
        });

        OnClickTab(0);
        SetButton(state);

        button_Close.SetButton(() =>
        {
            NetworkManager.a.SendTournamentView(info.t_tour_info.nTournamentNo, 0, null);
            Close();
        });
    }

    public void SetButton(TournamentTableItem.BUTTON_STATE state)
    {
        button_ok.SetActive(state != TournamentTableItem.BUTTON_STATE.Applying && info.t_tour_info.bCurrentState.HasFlagLeast(FHDefineMC.MD_CTS.REGISTRATION, FHDefineMC.MD_CTS.LATERAGISTRATION));
        button_cancel.SetActive(state == TournamentTableItem.BUTTON_STATE.Applying && !info.t_tour_info.bCurrentState.HasFlag(FHDefineMC.MD_CTS.PLAY));
        button_play.SetActive(state == TournamentTableItem.BUTTON_STATE.Participating);

        LMCS_EnableTicketReq pck = new LMCS_EnableTicketReq();
        pck.nTournamentNo = TournamentNo;
        NetworkManager.Lobby.Send<LMSC_EnabelTicketRes>(pck, res =>
        {
            bool active = button_ok.gameObject.activeInHierarchy && res.nTicketCnt > 0;

            toggle_userTicket.SetActive(active);
            toggle_userTicket.isOn = active;

        });
    }

    public void OnClickTab(int index)
    {
        tab_info.isOn = index == 0;
        tab_reward.isOn = index == 1;
        tab_blind.isOn = index == 2;
    }

    public void InfoInit()
    {
        toggle_userTicket.SetActive(false);
        toggle_userTicket.isOn = false;
        toggle_userTicket.onValueChanged.AddListener((v) =>
        {
            useTicket = v;
        });

        text_title.text = $"{info.szTournmentName}";
        text_startTime.text = $"{Util.DrawDate(info.t_tour_info.tStartTime)}";
        text_deadLineTime.text = $"{Util.DrawDate(info.t_semi_info.tRegistEndTime == 0 ? info.t_tour_info.tStartTime : info.t_semi_info.tRegistEndTime)}";
        text_reentry.text = string.Format("{0}", info.t_semi_info.bReentryCount > 0 ? info.t_semi_info.bReentryCount.ToString() : "-");
        text_rebuy.text = string.Format("{0}", info.t_semi_info.bRebuyCount > 0 ? info.t_semi_info.bRebuyCount.ToString() : "-");
        text_addon.text = string.Format("{0}", info.t_semi_info.dwAddon > 0
            ? $"{Util.GetMoneyString(info.t_semi_info.dwAddon)} + {Util.GetMoneyString(info.t_semi_info.dwAddonFee)}\n({Util.GetMoneyString(info.t_semi_info.dwAddonChips, true)}칩)"
            : "-");

        if (info.t_tour_info.bBuyinType == FHDefineMC.MD_BT.TICKET)
        {
            text_buyin.text = "티켓전용";
        }
        else
        {
            text_buyin.text = $"{Util.GetMoneyString(info.t_tour_info.dwBuyin)} + {Util.GetMoneyString(info.t_tour_info.dwFee)}";
        }

        text_userCount.text = $"{info.t_tour_info.wTotalEntrantCnt:N0}/{info.wMaxRegistration:N0}";
        text_leastUserCount.text = $"{info.wMinRegistration:N0}";

        bool isRemain = Util.GetLeft(info.t_tour_info.tStartTime).Ticks < 0;

        trf_remainingTime.SetActive(isRemain);
        trf_proceedTime.SetActive(!isRemain);

        StartCoroutine(UpdateTime(info.t_tour_info.tStartTime));
    }

    public void UpdateUserCount()
    {
        text_userCount.text = $"{info.t_tour_info.wTotalEntrantCnt:N0}/{info.wMaxRegistration:N0}";
    }

    IEnumerator UpdateTime(long startTime)
    {
        while (true)
        {
            TimeSpan time = Util.GetLeft(startTime);

            int day = Math.Abs(time.Days);

            if (day > 0)
            {
                text_remainingTime.text = $"{day}일 {time:hh\\:mm\\:ss}";
            }
            else
            {
                text_remainingTime.text = time.ToString(@"hh\:mm\:ss");
            }

            button_cancel.interactable = IsRemainTime(info.t_tour_info.tStartTime, 120);

            yield return new WaitForSecondsRealtime(1f);
        }
    }

    public void BlindInit()
    {
        Item_Tournament_Blind prefab = ResourceManager.Load<Item_Tournament_Blind>(Const.ITEM_TOURNAMENT_BLIND);
        int count = option.t_blind_info.dwBigBlind.Length;
        float height = prefab.GetComponent<RectTransform>().rect.height;

        blind_contents.sizeDelta = new Vector2(0, height * count);

        for (int i = 0; i < count; i++)
        {
            int lv = i + 1;
            long blind, ante;

            blind = option.t_blind_info.dwBigBlind[i];

            if (i < option.t_blind_info.bAnteLevel - 1)
                ante = 0;
            else ante = blind / 10;

            Item_Tournament_Blind item = Instantiate(prefab, blind_contents);

            item.Level = lv;
            item.Blind = $"{blind / 2:N0} / {blind:N0}";
            item.Ante = ante;
            item.Blind_Time = info.t_semi_info.bLevelUpTime;
        }
    }

    List<Item_Tournament_Reward> list_reward = new List<Item_Tournament_Reward>();

    public void RewardInit()
    {
        Item_Tournament_Reward prefab = ResourceManager.Load<Item_Tournament_Reward>(Const.ITEM_TOURNAMENT_REWARD);
        int count = prize.table_Lists.Length;
        float height = prefab.GetComponent<RectTransform>().rect.height;

        reward_contents.sizeDelta = new Vector2(0, height * count);

        for (int i = 0; i < count; i++)
        {
            Item_Tournament_Reward item = Instantiate(prefab, reward_contents);

            int r0 = prize.table_Lists[i].nRank_Start;
            int r1 = prize.table_Lists[i].nRank_End;

            if (r0 == r1)
                item.Rank = $"{r0}";
            else
                item.Rank = $"{r0} - {r1}";

            long n64Prize = prize.table_Lists[i].n64Prize;
            item.Reward = n64Prize > 0 ? $"{Util.GetMoneyString(n64Prize)}" : info.szTicketName;
            item.Percent = prize.nPrizeTicketCount > 0 ? "-" : $"{prize.table_Lists[i].PrizebyEntry / 1000f:F2}";
            list_reward.Add(item);
        }

        if (prize.nPrizeTicketCount > 0)
            text_totalReward.text = $"{info.szTicketName.Trim('\0')} {prize.nPrizeTicketCount}장";
        else
            text_totalReward.text = "";

        if (prize.n64PrizeTotal > 0)
        {
            if (prize.nPrizeTicketCount > 0)
                text_totalReward.text += " + ";

            text_totalReward.text += Util.GetMoneyString(prize.n64PrizeTotal);
        }
    }

    public void RefreshReward(LMSC_PrizebyEntryTable data)
    {
        foreach (var v in list_reward)
        {
            Destroy(v.gameObject);
        }

        list_reward = new List<Item_Tournament_Reward>();

        prize = data;
        RewardInit();
    }

    bool IsRemainTime(long startTime, float second)
    {
        return Math.Abs(Util.GetLeft(startTime).TotalSeconds) > second;
    }
}
