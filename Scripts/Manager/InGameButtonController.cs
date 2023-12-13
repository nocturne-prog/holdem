using UnityEngine;
using UnityEngine.UI;

public class InGameButtonController : MonoBehaviour
{
    [Header("Top/Left")]
    public Button btn_exit;
    public Button btn_history;

    [Header("Bottom/Left")]
    public EmojiPopup popup_Emoji;
    public InputField input_chat;
    public Toggle toggle_Chat;
    public Toggle toggle_Emoji;
    public ToggleEx toggle_reserv_observ;
    public ToggleEx toggle_reserv_rest;
    public Text text_toggle_rest;

    [Header("Bottom/Right")]
    public Button btn_seat;
    public Button btn_fold;
    public Button btn_call;
    public Text text_call;
    public Button btn_check;
    public Button btn_bet;
    public Text text_bet;
    public Text text_bet_money;
    public Button btn_raise;
    public Text text_raise;
    public Text text_raise_money;
    public Button btn_show;
    public Button btn_muck;
    public ToggleEx toggle_check;
    public ToggleEx toggle_checkFold;
    public ToggleEx toggle_fold;
    public ToggleEx toggle_call;
    public Text toggle_call_text;
    public ToggleEx toggle_waitforBB;

    public RaisePopup raisePopup;
    FPDefine.MD_SPEEDBTN ReservType = 0;

    public void Init()
    {
        raisePopup.SetActive = false;

        btn_exit.SetButton(() =>
        {
            var popup = UIManager.a.OpenPopup<UIPopup_TwoButton>(UIManager.POPUP_TYPE.NORMAL, Const.TEXT_EXIT_LOBBY);
            popup.ok_callback += () => GameManager.a.ExitLobby();
        });

        btn_history.SetButton(() =>
        {
            InGameManager.a.OnClickHistory();
            btn_history.interactable = false;
        });

        btn_history.interactable = false;

        btn_seat.SetButton(() => OnClickImBack());
        btn_fold.SetButton(() => OnClickFold());
        btn_call.SetButton(() => OnClickCall());
        btn_check.SetButton(() => OnClickCheck());
        btn_bet.SetButton(() => OnClickBet());
        btn_raise.SetButton(() => OnClickRaise());
        btn_show.SetButton(() => OnClickShow());
        btn_muck.SetButton(() => OnClickMuck());

        toggle_Emoji.onValueChanged.AddListener(b =>
        {
            OnClickEmoji(b);
        });

        toggle_Chat.onValueChanged.AddListener(b =>
        {
            if (b)
            {
                OnClickChat();
            }
        });

        input_chat.onEndEdit.AddListener(t =>
        {
            SendChat(t);
            OnClickChat();
        });

        if (GameManager.a.isTourney)
        {
            text_toggle_rest.text = "다음 핸드 자리비움";
            text_toggle_rest.fontSize = 22;
        }

        toggle_reserv_observ.onValueChanged.AddListener(b => OnCheckLeaveReserve(b));
        toggle_reserv_rest.onValueChanged.AddListener(b => OnCheck5MinAway(b));

        toggle_check.onValueChanged.AddListener(b => ReservType = b ? FPDefine.MD_SPEEDBTN.CHECK : 0);
        toggle_checkFold.onValueChanged.AddListener(b => ReservType = b ? FPDefine.MD_SPEEDBTN.CHECKFOLD : 0);
        toggle_call.onValueChanged.AddListener(b => ReservType = b ? FPDefine.MD_SPEEDBTN.CALL : 0);
        toggle_fold.onValueChanged.AddListener(b => ReservType = b ? FPDefine.MD_SPEEDBTN.FOLD : 0);

        toggle_waitforBB.onValueChanged.AddListener(b =>
        {
            SendWaitForBB(b);
        });

        SetShowMuck(false);
        HideSelectButtons();
        HideToggleButtons();
        SetLeftToggleButton(false);
        ShowWaitForBB(false);
    }

    public void UpdateTableInfo(MPSC_HO_TableInfo p)
    {
        if (InGameManager.a.MySeatState.HasFlagLeast(FPDefine.MD_ST.SITOUT, FPDefine.MD_ST.SELECT_TIMEOVER))
        {
            ShowButtonSeat(true);
        }
    }

    public void StageInfo()
    {
        if (!InGameManager.a.MySeatState.HasFlag(FPDefine.MD_ST.WAITFORBB))
        {
            ShowWaitForBB(false);
        }
    }

    public void StageEnd()
    {
        HideSelectButtons();
        HideToggleButtons();
    }

    public void ButtonHistoryOpen()
    {
        btn_history.interactable = true;
    }

    public void ReceiveSelectRequest()
    {
        //Debug.LogError($"ReceiveSelectRequest :: {ReservType}");

        if (ReservType == 0)
        {
            ShowBetButtons();
        }
        else
        {
            //Debug.LogError(ReservType);

            FPDefine.MD_BTN flag = InGameManager.a.CurrentUserActionRequest.rSelect.dwBtnFlags;

            switch (ReservType)
            {
                case FPDefine.MD_SPEEDBTN.CHECK:
                case FPDefine.MD_SPEEDBTN.CHECKFOLD:

                    if (flag.HasFlag(FPDefine.MD_BTN.CHECK))
                        OnClickCheck();
                    else
                        ShowBetButtons();

                    break;

                case FPDefine.MD_SPEEDBTN.CALL:

                    if (flag.HasFlagLeast(FPDefine.MD_BTN.CALL, FPDefine.MD_BTN.ALLIN_CALL))
                        OnClickCall();
                    else
                        ShowBetButtons();

                    break;

                case FPDefine.MD_SPEEDBTN.FOLD:
                default:
                    OnClickFold();
                    break;
            }

            HideToggleButtons();
        }

        ReservType = 0;
    }

    public void SeatDown(MS_SeatData2 data)
    {
        //ShowWaitForBB(true);
        ShowButtonSeat(data != null && data.dwSeatState.HasFlag(FPDefine.MD_ST.SITOUT));
    }

    public void SeatInfo(MPSC_SeatInfo p)
    {
        ShowButtonSeat(p.dwSeatState.HasFlagLeast(FPDefine.MD_ST.SITOUT, FPDefine.MD_ST.SELECT_TIMEOVER));
        SetLeftToggleButton(p.dwSeatState.HasFlag(FPDefine.MD_ST.SITDOWN) && !p.dwSeatState.HasFlag(FPDefine.MD_ST.SITOUT));

        if (!GameManager.a.isTourney)
            ShowWaitForBB(p.dwSeatState.HasFlagLeast(FPDefine.MD_ST.PLAY_RESERVE, FPDefine.MD_ST.WAITFORBB));
    }

    #region Fold/Check/Bet/Raise..
    bool isAllin = false;
    public void ShowBetButtons()
    {
        HideToggleButtons();

        long viewRaiseValue = GetRaiseValue() + InGameManager.a.MyChip.Value;

        isAllin = GetRaiseValue() == InGameManager.a.MyIcon.Money;
        text_raise.text = isAllin ? "All in" : "Raise to";
        text_bet.text = isAllin ? "All in" : "Bet";
        btn_raise.image.sprite = ResourceManager.LoadButtonImage(isAllin);
        btn_bet.image.sprite = ResourceManager.LoadButtonImage(isAllin);

        text_call.text = GetCallText();
        raisePopup.txt_raise.text = Util.GetMoneyString(viewRaiseValue, GameManager.a.isTourney);
        text_raise_money.text = Util.GetMoneyString(viewRaiseValue, GameManager.a.isTourney);
        text_bet_money.text = Util.GetMoneyString(viewRaiseValue, GameManager.a.isTourney);

        FPDefine.MD_BTN flags = InGameManager.a.CurrentUserActionRequest.rSelect.dwBtnFlags;

        btn_call.SetActive(flags.HasFlagLeast(FPDefine.MD_BTN.CALL, FPDefine.MD_BTN.ALLIN_CALL));
        btn_check.SetActive(flags.HasFlag(FPDefine.MD_BTN.CHECK));
        btn_bet.SetActive(flags.HasFlag(FPDefine.MD_BTN.BET));
        btn_raise.SetActive((viewRaiseValue - InGameManager.a.MyChip.Value) == 0 ? false : flags.HasFlagLeast(FPDefine.MD_BTN.RAISE, FPDefine.MD_BTN.ALLIN_RAISE));

        if (Option.CheckFold && !GameManager.a.isTourney)
        {
            btn_fold.SetActive(flags.HasFlag(FPDefine.MD_BTN.FOLD));
        }
        else
        {
            btn_fold.SetActive(!flags.HasFlag(FPDefine.MD_BTN.CHECK));
        }

        //btn_fold.SetActive(flags.HasFlagLeast(FPDefine.MD_BTN.CALL, FPDefine.MD_BTN.ALLIN_CALL, FPDefine.MD_BTN.RAISE, FPDefine.MD_BTN.ALLIN_RAISE) ||
        //    flags.HasFlag(FPDefine.MD_BTN.CHECK) && Option.CheckFold);
    }

    string GetCallText()
    {
        HGS_HO_SELECT select = InGameManager.a.CurrentUserActionRequest.rSelect;

        if (select.n64Bet > 0)
        {
            return Util.GetMoneyString(select.n64Bet, GameManager.a.isTourney);
        }
        else
        {
            return "-";
        }
    }

    long GetRaiseValue()
    {
        HGS_HO_SELECT select = InGameManager.a.CurrentUserActionRequest.rSelect;
        FPDefine.MD_BTN flags = select.dwBtnFlags;

        long minRaiseValue = select.n64Bet;

        if (flags.HasFlagLeast(FPDefine.MD_BTN.RAISE, FPDefine.MD_BTN.ALLIN_RAISE))
        {
            minRaiseValue = select.n64Raise;

            if (select.n64Raise > select.n64MaxRaise && flags.HasFlag(FPDefine.MD_BTN.ALLIN_RAISE) && !flags.HasFlag(FPDefine.MD_BTN.RAISE))
            {
                select.n64MaxRaise = minRaiseValue;
            }
        }

        if (minRaiseValue > select.n64MaxRaise)
        {
            return 0;
        }
        else
        {
            return minRaiseValue;
        }
    }

    public void HideSelectButtons()
    {
        btn_fold.SetActive(false);
        btn_call.SetActive(false);
        btn_check.SetActive(false);
        btn_bet.SetActive(false);
        btn_raise.SetActive(false);
        raisePopup.SetActive = false;
        //player.MyTurn = false;
    }

    public void OnClickFold()
    {
        HideSelectButtons();
        SendPacket(FPDefine.MD_BTN.FOLD, 0);
    }

    public void OnClickCall()
    {
        HideSelectButtons();
        SendPacket((FPDefine.MD_BTN.ALLIN_CALL | FPDefine.MD_BTN.CALL), InGameManager.a.CurrentUserActionRequest.rSelect.n64Bet);
    }

    public void OnClickCheck()
    {
        HideSelectButtons();
        SendPacket(FPDefine.MD_BTN.CHECK, InGameManager.a.CurrentUserActionRequest.rSelect.n64Bet);
    }

    public void OnClickBet()
    {
        if (isAllin)
        {
            HideSelectButtons();
            SendPacket(FPDefine.MD_BTN.RAISE | FPDefine.MD_BTN.ALLIN_RAISE,
                InGameManager.a.CurrentUserActionRequest.rSelect.n64MaxRaise);
            return;
        }

        if (raisePopup.gameObject.activeInHierarchy)
        {
            HideSelectButtons();
            SendPacket(FPDefine.MD_BTN.BET, raisePopup.raiseValue);
        }
        else
        {
            raisePopup.SetActive = true;
            raisePopup.Clear();
        }
    }

    public void OnClickRaise()
    {
        if (isAllin)
        {
            HideSelectButtons();
            SendPacket(FPDefine.MD_BTN.RAISE | FPDefine.MD_BTN.ALLIN_RAISE,
               InGameManager.a.CurrentUserActionRequest.rSelect.n64MaxRaise);
            return;
        }

        if (raisePopup.gameObject.activeInHierarchy)
        {
            HideSelectButtons();
            SendPacket(FPDefine.MD_BTN.RAISE | FPDefine.MD_BTN.ALLIN_RAISE, raisePopup.raiseValue);
        }
        else
        {
            raisePopup.SetActive = true;
            raisePopup.Clear();
        }
    }

    void SendPacket(FPDefine.MD_BTN flag, long bet)
    {
        MPCS_HO_SelectRes p = new MPCS_HO_SelectRes
        {
            dwBtnFlags = InGameManager.a.CurrentUserActionRequest.rSelect.dwBtnFlags & (flag),
            n64Bet = bet,
            dwTableActionID = InGameManager.a.CurrentUserActionRequest.dwTableActionID
        };

        InGameManager.a.socket.Send(p);
    }

    #endregion

    #region AFK..
    public void ShowButtonSeat(bool active)
    {
        btn_seat.SetActive(active);

        if (active)
        {
            SetShowMuck(false);
            HideToggleButtons();
        }
    }

    void OnClickImBack()
    {
        btn_seat.SetActive(false);

        MPBT_HO_CheckOptionInfo packet = new MPBT_HO_CheckOptionInfo
        {
            bType = MPBT_HO_CheckOptionInfo.Type.eCI_SITOUTNEXTHAND,
            bCheck = 0
        };

        InGameManager.a.socket.Send(packet);
        InGameManager.a.MySeatState = (FPDefine.MD_ST.SITDOWN | FPDefine.MD_ST.SITPLAY);
        InGameManager.a.MyIcon.Money = GameData.UserInfo[InGameManager.a.MySeatIndex].n64Account;
    }
    #endregion

    public void ShowWaitForBB(bool active)
    {
        if (active)
        {
            if (InGameManager.a.PlayUserCount <= 2)
                return;
        }

        if (active != toggle_waitforBB.gameObject.activeInHierarchy)
            toggle_waitforBB.isOn_NotCallback = false;

        toggle_waitforBB.SetActive(active);
    }

    public void SendWaitForBB(bool active)
    {
        var pck = new MPBT_HO_CheckOptionInfo
        {
            bType = MPBT_HO_CheckOptionInfo.Type.eCI_WAITFORBB,
            bCheck = (byte)(active ? 1 : 0)
        };

        InGameManager.a.socket.Send(pck);
    }
    #region Reserve Fold/Check/Call..
    public long prevCallValue = 0;
    public long callValue = 0;
    bool IsPreFlop => (uint)InGameManager.a.CurrentTableState <= (uint)FPDefineTableState.HO_STATE_BET_FIRST;

    public void UpdateToggleData(MPSC_HO_SelectInfo p)
    {
        callValue = System.Math.Min((IsPreFlop && InGameManager.a.MyIcon.BigBlind) ? p.n64TopStakes : p.n64TopStakes - InGameManager.a.MyChip.Value, InGameManager.a.MyIcon.Money);
        toggle_call_text.text = $"call {Util.GetMoneyString(callValue, GameManager.a.isTourney)}";

        //Debug.LogError($"{InGameManager.a.CurrentTableState} // {prevCallValue} ==> {callValue} :: {ReservType}");

        if (prevCallValue != callValue && ReservType.HasFlag(FPDefine.MD_SPEEDBTN.CALL))
        {
            toggle_call.isOn_NotCallback = false;
            ReservType = 0;
        }
        else if (prevCallValue != callValue && ReservType.HasFlag(FPDefine.MD_SPEEDBTN.CHECKFOLD))
        {
            toggle_fold.isOn_NotCallback = true;
            ReservType = FPDefine.MD_SPEEDBTN.FOLD;
        }
        else if (prevCallValue != callValue && ReservType.HasFlag(FPDefine.MD_SPEEDBTN.CHECK))
        {
            ReservType = 0;
        }

        prevCallValue = callValue;
    }

    public void SetSpeedToggle(FPDefine.MD_SPEEDBTN state)
    {
        if (state == 0)
        {
            HideToggleButtons();
        }
        else
        {
            toggle_call.SetActive(state.HasFlag(FPDefine.MD_SPEEDBTN.CALL));
            toggle_fold.SetActive(state.HasFlag(FPDefine.MD_SPEEDBTN.CALL));
            toggle_call_text.text = $"call {Util.GetMoneyString(callValue, GameManager.a.isTourney)}";
            ToggleSetActive(toggle_check, state.HasFlag(FPDefine.MD_SPEEDBTN.CHECK));
            toggle_checkFold.SetActive(state.HasFlag(FPDefine.MD_SPEEDBTN.CHECK));
        }
    }

    public void HideToggleButtons()
    {
        toggle_check.SetActive(false);
        toggle_checkFold.SetActive(false);
        toggle_call.SetActive(false);
        toggle_fold.SetActive(false);

        toggle_check.isOn_NotCallback = false;
        toggle_checkFold.isOn_NotCallback = false;
        toggle_call.isOn_NotCallback = false;
        toggle_fold.isOn_NotCallback = false;

        ReservType = 0;
    }

    public void ToggleSetActive(ToggleEx toggle, bool active)
    {
        if (!active)
        {
            toggle.isOn = false;
        }

        toggle.SetActive(active);
    }
    #endregion

    #region Leave/Observer
    public void SetLeftToggleButton(bool active)
    {
        if (active)
        {
            toggle_reserv_rest.isOn_NotCallback = false;
            toggle_reserv_observ.isOn_NotCallback = false;
        }
        else
        {
            OnClickEmoji(false);
        }

        toggle_reserv_observ.SetActive(active);
        toggle_reserv_rest.SetActive(active);

        if (GameManager.a.isTourney)
        {
            toggle_reserv_observ.SetActive(false);
        }

        if (GameManager.a.isWCOP)
        {
            toggle_Chat.SetActive(false);
            toggle_Emoji.SetActive(false);
        }
        else
        {
            toggle_Chat.SetActive(active);
            toggle_Emoji.SetActive(active);
        }
    }

    private void OnCheck5MinAway(bool check)
    {
        //Debug.LogError($"OnCheck5MinAway :: {check}");

        MPBT_HO_CheckOptionInfo packet = new MPBT_HO_CheckOptionInfo
        {
            bType = MPBT_HO_CheckOptionInfo.Type.eCI_SITOUTNEXTHAND,
            bCheck = (byte)(check ? 1 : 0)
        };

        InGameManager.a.socket.Send(packet);
    }

    private void OnCheckLeaveReserve(bool check)
    {
        //Debug.LogError($"OnCheckLeaveReserve :: {check}");

        MPBT_HO_CheckOptionInfo packet = new MPBT_HO_CheckOptionInfo
        {
            bType = MPBT_HO_CheckOptionInfo.Type.eCI_STANDUPNEXTHAND,
            bCheck = (byte)(check ? 1 : 0)
        };

        InGameManager.a.socket.Send(packet);
    }
    #endregion

    #region Show/Muck
    public void SetShowMuck(bool active)
    {
        if (!active)
        {
            HideToggleButtons();
        }
        else
        {
            if (btn_seat.gameObject.activeInHierarchy)
                return;
        }

        btn_show.SetActive(active);
        btn_muck.SetActive(active);
    }

    public void OnClickShow()
    {
        SetShowMuck(false);

        MPCS_HO_ShowRes req = new MPCS_HO_ShowRes();
        req.dwBtnFlags = (uint)FPDefine.MD_BTN.SHOWCARDS;
        req.dwStageNo = GameData.StageNumber;
        InGameManager.a.socket.Send(req);
    }

    public void OnClickMuck()
    {
        SetShowMuck(false);
    }
    #endregion

    #region Chat/Emoji
    public void OnClickEmoji(bool active)
    {
        popup_Emoji.Active = active;
        toggle_Emoji.isOn = active;
    }

    public void OnClickChat()
    {
        input_chat.Select();
    }

    public void SendEmoji(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        InGameManager.a.SendChat(text);
        toggle_Emoji.isOn = false;
    }

    public void SendChat(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        InGameManager.a.SendChat(text);
        input_chat.text = "";
        toggle_Chat.isOn = false;
    }
    #endregion


}

