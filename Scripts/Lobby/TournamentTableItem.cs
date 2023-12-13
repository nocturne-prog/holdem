using System.Text;
using System;
using UnityEngine.UI;
using UnityEngine;

public class TournamentTableItem : LobbyListItem
{
    public Image img_bg;
    public Image img_bg_wcop;

    public Text text_limitTime;
    public Text text_userCount;
    public Text text_reward;
    public Text text_reward_wcop;
    public Text text_fillingFee;
    public Text text_fee;
    public MoveText text_name;
    public Transform img_onlyTicket;

    public Button end;                      // 종료
    public Button proceeding_r;             // 진행중(빨강)
    public Button proceeding_g;             // 진행중(초록)
    public Button accepting;                // 접수중
    public Button applying;                 // 신청중
    public Button participating;            // 참가중
    public Button preparing;                // 준비중

    public GameData.TournamentTable Data { get; private set; }

    public enum BUTTON_STATE
    {
        None = 0,
        End,
        Proceeding_r,
        Proceeding_g,
        Accepting,
        Applying,
        Participating,
        Preparing,
    }

    public void SetData(GameData.TournamentTable table)
    {
        Data = table;
        RefreshData();
    }

    public void SetUserCount(ushort value)
    {
        Data.t_info.wTotalEntrantCnt = value;
        RefreshData();
    }

    public void RefreshData()
    {
        bool isWCOP = Data.t_info.bTournamentType == 1;

        img_bg.SetActive(!isWCOP);
        text_reward.SetActive(!isWCOP);
        img_bg_wcop.SetActive(isWCOP);
        text_reward_wcop.SetActive(isWCOP);

        text_limitTime.text = $"{Util.DrawDate(Data.t_info.tStartTime)}";
        text_userCount.text = $"{Data.t_info.wTotalEntrantCnt}";


        if (Data.info.bPrizeSet == 1)
        {
            long ticketCount = Data.t_info.lGuarantee / 10000;
            long totalPrize = Math.Max(Data.t_info.dwBuyin * Data.t_info.wTotalEntrantCnt, ticketCount * Data.info.lPrizePerTicket);
            long addOn = totalPrize - ticketCount * Data.info.lPrizePerTicket;

            if (addOn > 0)
            {
                long addTicket = addOn / Data.info.lPrizePerTicket;
                addOn -= (Data.info.lPrizePerTicket * addTicket);
                ticketCount += addTicket;
            }

            if (addOn > 0)
            {
                text_reward.SetText($"{Data.info.szTicketName.Trim('\0')} {ticketCount }장 + {Util.GetMoneyString(addOn)}");
            }
            else
            {
                text_reward.SetText($"{Data.info.szTicketName.Trim('\0')} {ticketCount }장");
            }
        }
        else
        {
            long totalPrize = Math.Max((Data.t_info.dwBuyin * Data.t_info.wTotalEntrantCnt) + (Data.c_info.wAddonCnt * Data.s_info.dwAddon) + (Data.c_info.wRebuyCnt * Data.t_info.dwBuyin)
                                        , Data.t_info.lGuarantee);
            text_reward.SetText($"{Util.GetMoneyString(totalPrize)}");
        }

        text_fillingFee.text = $"{Util.GetMoneyString(Data.t_info.dwBuyin)}";
        text_fee.text = $"{Util.GetMoneyString(Data.t_info.dwFee)}";
        text_name.Init(Data.info.szTournmentName);

        img_onlyTicket.SetActive(Data.t_info.bBuyinType == FHDefineMC.MD_BT.TICKET);

        end.SetButton(() => OnClickButton(BUTTON_STATE.End));
        proceeding_r.SetButton(() => OnClickButton(BUTTON_STATE.Proceeding_r));
        proceeding_g.SetButton(() => OnClickButton(BUTTON_STATE.Proceeding_g));
        accepting.SetButton(() => OnClickButton(BUTTON_STATE.Accepting));
        applying.SetButton(() => OnClickButton(BUTTON_STATE.Applying));
        participating.SetButton(() => OnClickButton(BUTTON_STATE.Participating));
        preparing.SetButton(() => OnClickButton(BUTTON_STATE.Preparing));

        AllButtonOff();
        RefreshButton();
    }

    public void RefreshButton()
    {
        FHDefineMC.MD_CTS state = Data.t_info.bCurrentState;
        bool IsJoinMe = Data.t_info.bParticipated == 1;

        if (state.HasFlag(FHDefineMC.MD_CTS.PLAY))
        {
            if (IsJoinMe)
            {
                /// 참가중
                participating.SetActive(true);
            }
            else
            {
                if (state.HasFlagLeast(FHDefineMC.MD_CTS.REGISTRATION, FHDefineMC.MD_CTS.LATERAGISTRATION))
                {
                    // 진행중(초록)
                    proceeding_g.SetActive(true);
                }
                else
                {
                    // 진행중(빨강)
                    proceeding_r.SetActive(true);
                }
            }

        }
        else if (state.HasFlagLeast(FHDefineMC.MD_CTS.REGISTRATION, FHDefineMC.MD_CTS.LATERAGISTRATION))
        {
            if (IsJoinMe)
            {
                // 신청중
                applying.SetActive(true);
            }
            else
            {
                // 접수중
                accepting.SetActive(true);
            }
        }
        else if (state.HasFlag(FHDefineMC.MD_CTS.END))
        {
            // 종료
            end.SetActive(true);
        }
        else if (state.HasFlag(FHDefineMC.MD_CTS.VIEW))
        {
            // 준비중
            preparing.SetActive(true);
        }
    }

    void AllButtonOff()
    {
        end.SetActive(false);
        proceeding_r.SetActive(false);
        proceeding_g.SetActive(false);
        accepting.SetActive(false);
        applying.SetActive(false);
        participating.SetActive(false);
        preparing.SetActive(false);
    }

    public void OnClickButton(BUTTON_STATE state)
    {
        NetworkManager.a.SendTournamentView(Data.Number, 1, (info2, option, prize) =>
        {
            GameData.UpdateSubscriptionData(info2, option, prize);
            UIManager.a.OpenPopup<UIPopup_TournamentInfo>(UIManager.POPUP_TYPE.NORMAL, Data.info, option, prize, state);
        });
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        string str = Data.Name.Replace("\0", "");

        sb.AppendLine(string.Format("게임 : {0} ({1}인)", str, Data.s_info.bMaxSeat))
            .AppendLine($"블라인드업 : {Data.s_info.bLevelUpTime}")
            .AppendLine($"브레이크 타임 : 매시 55분 부터 정각")
            .AppendLine($"바이인 : {Util.GetMoneyString(Data.t_info.dwBuyin)} + {Util.GetMoneyString(Data.t_info.dwFee)} ({Data.s_info.dwNormalChips:N0}칩)")
            .AppendLine($"참여마감시간 : {Util.DrawDate(Data.s_info.tRegistEndTime == 0 ? Data.t_info.tStartTime : Data.s_info.tRegistEndTime)}")
            .AppendLine($"최소인원 : {Data.info.wMinRegistration}명")
            .AppendLine($"{Util.GetMoneyString(Data.t_info.lGuarantee)} 개런티 토너먼트")
            .AppendLine($"{Data.info.szDescription}");

        return sb.ToString();
    }
}
