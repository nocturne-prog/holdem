using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InGameManager : MonoBehaviour
{
    public static InGameManager a;
    public Canvas mainCanvas;

    public bool Connected => socket == null ? false : socket.Connected;

    public struct UserInfo
    {
        public UserChip chip;
        public UserIcon icon;

        public void SetActive(bool value)
        {
            chip.SetActive = value;
            icon.SetActive = value;
        }

        public void Clear()
        {
            chip.Clear();
            icon.Clear();
        }

        public void Delete()
        {
            chip.Delete();
            icon.Delete();
        }

        public UserInfo(UserChip c, UserIcon i)
        {
            chip = c;
            icon = i;
        }
    }

    private Dictionary<int, UserInfo> userList;
    HandMeter handMeter = new HandMeter();
    public InGameTableController tableController { get; private set; }
    public InGameButtonController buttonController { get; private set; }

    public int EmptySeatCount => MaxSeatCount - userList.Count;
    public PlayerIcon MyIcon => userList[MySeatIndex].icon as PlayerIcon;
    public UserChip MyChip => userList[MySeatIndex].chip;
    public int PlayUserCount => GameData.UserInfo.Count(x => x.Value.dwSeatState != FPDefine.MD_ST.NONE);

    public RoomSocket socket { get; private set; }
    public bool isObserve = false;
    public MPSC_HO_SelectReq CurrentUserActionRequest { get; private set; }
    public List<byte> CurrentCommunityCards { get; private set; } = new List<byte>();
    public FPDefine.MD_ST MySeatState
    {
        get
        {
            if (GameData.UserInfo.ContainsKey(MySeatIndex))
            {
                return GameData.UserInfo[MySeatIndex].dwSeatState;
            }
            else
                return FPDefine.MD_ST.NONE;
        }
        set
        {
            if (GameData.UserInfo.ContainsKey(MySeatIndex))
            {
                GameData.UserInfo[MySeatIndex].dwSeatState = value;
            }
        }
    }

    public FPDefineTableState CurrentTableState { get; private set; }

    private int mySeatIndex = -1;
    public int MySeatIndex
    {
        get { return mySeatIndex; }
        set { mySeatIndex = value; }
    }

    public bool IsMySeat(int index) => MySeatIndex == index;
    public int MaxSeatCount = 0;
    public Transform championEffect;
    private bool IsShowDown = false;
    private HandHistoryData historyData;

    bool IsShowDownEffect = false;
    private void Awake()
    {
        a = this;

        socket = FindObjectOfType<RoomSocket>();
        tableController = FindObjectOfType<InGameTableController>();
        buttonController = FindObjectOfType<InGameButtonController>();
    }

    private void Start()
    {
        mainCanvas.worldCamera = GameManager.a.mainCamera;
        userList = new Dictionary<int, UserInfo>();
        historyData = new HandHistoryData();

        GameManager.a.InitCanvas(mainCanvas);
    }

    void DrawRate()
    {
        foreach (var rate in handMeter.GetRates())
        {
            userList[rate.Item1].chip.Rate_Value = rate.Item2;
        }
    }

    private void Update()
    {
        lock (socket.packetQueue)
        {
            if (socket.packetQueue.Count > 0)
            {
                renderQueue.Enqueue(ProgressPacket(socket.packetQueue.Dequeue()));

                if (!isProgress)
                    StartCoroutine(StartRender());
            }
        }
    }

    Queue<IEnumerator> renderQueue = new Queue<IEnumerator>();

    public IEnumerator ProgressPacket(HS_PK_HEADER packet)
    {
        switch (packet)
        {
            case MPSC_HO_TableOption p:

                GameData.UpdateTableOption(p);
                tableController.Init();
                buttonController.Init();
                yield break;

            case MPSC_HO_TableInfo p:

                GameData.UpdateTableInfo(p);
                historyData.UpdateTableInfo();
                handMeter.Init();
                SetTableInnfo();
                tableController.UpdateTableInfo(p);
                buttonController.UpdateTableInfo(p);

                UIManager.a.CloseLoading();
                yield break;

            case MPSC_SeatDown2 p:
                MPSC_SeatDown2(p);
                yield break;

            case MPSC_BuyinInfo p:

                UIManager.a.OpenPopup<UIPopup_BuyIn>(UIManager.POPUP_TYPE.NORMAL, p);
                yield break;

            case MPSC_ChangeAccount p:

                if (p.account_info.nAccountSeqNo > 0)
                {
                    GameData.Player.AvailMoney = p.account_info.n64TotalAccount;
                    GameData.Player.PlayMoney = p.account_info.n64TotalPlayMoney;
                    GameData.Player.GamePoint = p.account_info.n64FP;
                    GameData.Player.Ticket = p.account_info.wTicketCount;
                }
                yield break;

            case MPSC_SeatInfo p:
                MPSC_SeatInfo(p);
                yield break;

            case MPSC_StageReady p:

                //Debug.LogError("Stage Ready !!");
                yield break;

            case MPSC_StageClear p:

                //Debug.LogError("MPSC_StageClear");

                foreach (var v in userList)
                {
                    v.Value.Clear();
                }

                historyData.StageClear();
                tableController.StageClear();
                IsShowDown = false;
                buttonController.SetShowMuck(false);

                if (historyData.Count > 0)
                    buttonController.ButtonHistoryOpen();

                yield break;

            case MPSC_StageEndReq p:
                yield return new WaitForSeconds(1f);

                //Debug.LogError("MPSC_StageEndReq");

                MPCS_StageEndRes req = new MPCS_StageEndRes();
                req.dwStageNo = GameData.StageNumber;
                socket.Send(req);

                buttonController.StageEnd();
                yield break;

            case MPSC_StageInfo p:

                GameData.UpdateStageInfo(p);
                tableController.DealerTokenPosition = p.cDealerSeat;
                buttonController.StageInfo();
                historyData.UpdateStageInfo();
                handMeter.Init();
                yield break;

            case MPSC_TableState p:
                if (IsShowDownEffect)
                {
                    IsShowDownEffect = false;
                    DrawRate();
                    buttonController.HideToggleButtons();
                    Sound.PlayEffect(Const.SOUND_SHOWDOWN);
                    SpriteAnimator.Create(mainCanvas.transform, Const.ANI_SHOWDOWN, 2f, 0f);
                    yield return new WaitForSeconds(2.5f);
                }

                if (CurrentTableState != FPDefineTableState.HO_STATE_BLIND)
                {
                    buttonController.prevCallValue = 0;
                }

                if (p.dwTableState.HasFlag(FPDefineTableState.HO_STATE_END))
                {
                    foreach (var v in userList)
                        v.Value.chip.Rate_Value = -1;
                }

                CurrentTableState = p.dwTableState;

                yield break;

            case MPSC_HO_CardHandInfo p:

                if (IsMySeat(p.cSeat) && p.rCardHand.bEnableHi == 1)
                {
                    MyIcon.Rank = Util.GetRankString(p.rCardHand, MyIcon.cards, CurrentCommunityCards.ToArray());
                }

                yield break;

            case MPSC_HO_DealtCardInfo p:
                historyData.UpdateDealtCardInfo(p);
                yield return StartCoroutine(DealtCardInfo(p));
                yield break;

            case MPSC_HO_SelectReq p:
                //Debug.LogError($"RECEIVE MPSC_HO_SelectReq ::: {p.rSelect.dwBtnFlags}");

                CurrentUserActionRequest = p;
                buttonController.ReceiveSelectRequest();
                yield break;

            case MPSC_HO_ChipTableInfo p:
                yield return StartCoroutine(MPSC_HO_ChipTableInfo(p));
                yield break;


            case MPSC_HO_BCard3Info p:

                CurrentCommunityCards = new List<byte>(p.bCard);

                for (int i = 0; i < 3; i++)
                {
                    handMeter.AddBoard(p.bCard[i]);
                    StartCoroutine(tableController.OpenCard(i, p.bCard[i]));
                }

                historyData.UpdateCommunityCard(CurrentCommunityCards);
                if (IsShowDown) DrawRate();
                yield break;

            case MPSC_HO_BCard1Info p:

                CurrentCommunityCards.Add(p.bCard);

                if (p.bCardIndex == 3)
                {
                    yield return StartCoroutine(tableController.OpenCard(3, p.bCard));
                }
                else
                {
                    yield return StartCoroutine(tableController.OpenCard(4, p.bCard, IsShowDown));
                }

                historyData.UpdateCommunityCard(CurrentCommunityCards);
                handMeter.AddBoard(p.bCard);
                if (IsShowDown) DrawRate();
                yield break;

            case MPSC_TimeInfo p:

                if (!GameData.UserInfo.ContainsKey(p.cSeat))
                {
                    //Debug.LogError($"User is null :: index: {p.cSeat}");
                    yield break;
                }

                if (p.bTimerType == MPSC_TimeInfo.TimerType.eTI_NORMAL)
                {
                    userList[p.cSeat].icon.MyTurn = true;

                    if (!IsMySeat(p.cSeat))
                        yield return new WaitForSeconds(0.5f);
                }

                userList[p.cSeat].icon.SetTimer(p.bTimerType, p.bTime, () =>
                {
                    if (IsMySeat(p.cSeat))
                    {
                        buttonController.HideSelectButtons();

                        MPCS_TimeOutInfo t = new MPCS_TimeOutInfo
                        {
                            HWord = FPPacket.HPD_PLAY,
                            LWord = FPPacket.MPCS_TIMEOUT_INFO_TAG,
                            dwTableActionID = CurrentUserActionRequest.dwTableActionID
                        };

                        socket.Send(t);
                    }
                });

                yield break;

            case MPSC_HO_ResultInfo p:

                //Debug.LogError("MPSC_HO_ResultInfo");

                foreach (var v in userList)
                {
                    v.Value.icon.Money = p.n64Account[v.Key];
                }

                yield break;

            case MPSC_HO_AnteInfo p:

                foreach (var v in userList)
                {
                    v.Value.icon.Money = p.n64Account[v.Key];
                }
                yield break;

            case MPSC_HO_SelectInfo p:

                yield return new WaitForSeconds(0.3f);
                yield return StartCoroutine(MPSC_HO_SelectInfo(p));
                yield break;

            case MPSC_HO_ReturnChipInfo p:

                userList[p.cSeat].icon.Money = p.n64Account;
                yield break;

            case MPSC_HO_BlindInfo p:

                yield return StartCoroutine(MPSC_HO_BlindInfo(p));
                yield break;

            case MPSC_AccountCashRes p:

                userList[p.cSeat].icon.Money = p.n64Cash;
                yield break;

            case MPSC_HO_PotInfo p:

                historyData.UpdatePotInfo(p);

                for (int i = 0; i < p.n64ReturnHi.Length; i++)
                {
                    if (p.n64ReturnHi[i] < 1)
                        continue;

                    if (IsMySeat(i))
                        Sound.PlayEffect(Const.SOUND_WIN);

                    if (GameData.HandInfo.rCardHand[i].bEnableHi == 1)
                    {
                        string winText = Util.GetRankString(GameData.HandInfo.rCardHand[i], userList[i].icon.cards, CurrentCommunityCards.ToArray());

                        if (p.rKickerCard[i].bCount > 0)
                        {
                            winText += " - ";
                            int ct = 0;

                            for (int k = 0; k < p.rKickerCard[i].bCount; k++)
                            {
                                ct = p.rKickerCard[i].bCardPos[k];
                                var c = ct < 2 ? userList[i].icon.cards[ct] : CurrentCommunityCards[ct - 2];

                                winText += CardDefine.CARD_NUMBER_STRING(c);
                                if (k < p.rKickerCard[i].bCount - 1)
                                {
                                    winText += "+";
                                }

                            }
                            winText += " Kicker";

                        }

                        //Debug.LogError($"{winText}");
                        tableController.ShowWinRank(true, winText);
                    }

                    SpriteAnimator.Create(tableController.trf_userPosition[i], Const.ANI_RESULT_WIN, 1f, 2f);

                    yield return StartCoroutine(tableController.MoveChip(tableController.text_pot.transform.position, userList[i].icon.MoneyPos, p.n64ReturnHi[i], () =>
                     {
                         Sound.PlayEffect(Const.SOUND_CHIP_WIN02);
                         userList[i].chip.Value += p.n64ReturnHi[i];
                         tableController.Pot -= p.n64ReturnHi[i];
                     }));
                }

                yield break;

            case MPSC_HO_SpeedBtnInfo p:

                if (MySeatIndex >= 0 && MySeatState.HasFlag(FPDefine.MD_ST.SITPLAY) && !MySeatState.HasFlag(FPDefine.MD_ST.SITOUT))
                {
                    //Debug.LogError(p);
                    buttonController.SetSpeedToggle(p.dwSpeedBtn[MySeatIndex]);
                }

                yield break;

            case MPSC_HO_ShowReq p:

                //Debug.LogError($"ShowReq:: {p}");

                if (IsMySeat(p.cSeat) && MySeatState.HasFlag(FPDefine.MD_ST.SITPLAY) && !IsShowDown && Option.ShowButton)
                {
                    buttonController.HideToggleButtons();
                    buttonController.SetShowMuck(true);
                }

                yield break;

            case MPSC_HO_TournamentOption p:    /// 토너먼트 시작이 얼마 얼마 남았습니다.

                //Debug.LogError($"MPSC_HO_TournamentOption: {p.bCurrentState}");

                if (p.bCurrentState == (FHDefineMC.MD_CTS)6)
                {
                    UIManager.a.OpenPopup<UIPopup_RestInfo>(UIManager.POPUP_TYPE.NORMAL, "토너먼트가 곧 시작됩니다", p.dwRemainOrPassedTime - 5);
                }
                else if (p.bCurrentState.HasFlagLeast(FHDefineMC.MD_CTS.END, FHDefineMC.MD_CTS.HIDE))
                {
                    UIManager.a.ClosePopup<UIPopup_RestInfo>();
                }

                yield break;

            case MPSC_T_StatusInfo p:

                //Debug.LogError($"MPSC_T_StatusInfo: {p.bType}");

                switch (p.bType)
                {
                    case MPSC_T_StatusInfo.eType.eSI_HAND_FOR_HAND:

                        if (p.n64Data == 1)
                        {
                            UIManager.a.OpenPopup<UIPopup_HandForHand>(UIManager.POPUP_TYPE.NORMAL);
                        }
                        else
                        {
                            UIManager.a.ClosePopup<UIPopup_HandForHand>();
                        }

                        break;

                    case MPSC_T_StatusInfo.eType.eSI_CHANGE_TABLE:

                        if (p.n64Data == 1)
                        {
                            UIManager.a.OpenPopup<UIPopup_Notice>(UIManager.POPUP_TYPE.NORMAL);
                        }

                        break;

                    case MPSC_T_StatusInfo.eType.eSI_FINAL_TABLE_STATE:
                        tableController.UpdateFinalTable();
                        //tableController.UpdateTableImage();
                        break;

                    case MPSC_T_StatusInfo.eType.eSI_FINAL_TABLE:
                    case MPSC_T_StatusInfo.eType.eSI_ITM:
                        UIManager.a.OpenPopup<UIPopup_TournamentMessage>(UIManager.POPUP_TYPE.NORMAL, p.bType);
                        break;
                }

                yield break;

            case MPSC_BreakTimeInfo p:

                if (p.bShowMsg == 0)
                {
                    UIManager.a.ClosePopup<UIPopup_RestInfo>();
                }
                else
                {
                    UIManager.a.OpenPopup<UIPopup_RestInfo>(UIManager.POPUP_TYPE.NORMAL, "휴식 시간입니다.", p.dwBreakTime);
                }

                yield break;

            case MPSC_LevelInfo p:      /// 블라인드 업 팝업

                UIManager.a.ClosePopup<UIPopup_RestInfo>();

                GameData.UpdateLevelInfo(p);
                tableController.SetText_Stake(p.n64SBlind, p.n64BBlind, p.n64Ante);
                historyData.UpdateStake(p.n64SBlind, p.n64BBlind);
                UIManager.a.OpenPopup<UIPopup_ChangeBlind>(UIManager.POPUP_TYPE.NORMAL, p);
                tableController.UpdateTimer(GameData.NextLevelTime);

                yield break;

            case MPSC_PrizeInfo2 p:     /// 토너먼트 종료!

                if (p.cTopRankSeatIdx < 0 || IsMySeat(p.cTopRankSeatIdx))
                {
                    var popup = UIManager.a.OpenPopup<UIPopup_TournamentResult>(UIManager.POPUP_TYPE.NORMAL, p);

                    popup.callback += () =>
                    {
                        NetworkManager.a.GetReEntryCount(GameManager.a.CurrentTournamentNo, response =>
                        {
                            var info = GameData.TournamentTableList.FirstOrDefault(x => x.Number == GameManager.a.CurrentTournamentNo);

                            if (info.s_info.bReentryCount > response.nUserReEntryCount - 1)
                                UIManager.a.OpenPopup<UIPopup_ReEntry>(UIManager.POPUP_TYPE.NORMAL, 2, response);
                        });
                    };
                }

                if (p.wRank == 1 && !championEffect.gameObject.activeInHierarchy)
                    championEffect.SetActive(true);

                yield break;

            case MPSC_HO_PCardInfo p:

                if ((CurrentTableState & FPDefineTableState.HOG_STATE_SELECT) != 0 && !IsShowDown)
                {
                    IsShowDown = true;

                    if (Option.ShowDown)
                    {
                        IsShowDownEffect = true;
                    }
                }

                userList[p.cSeat].icon.SetCards(p.rPCard.bCard, true);
                handMeter.AddHand(p.cSeat, p.rPCard.bCard);
                historyData.UpdatePCardInfo(p);
                yield break;


            case MPSC_HO_HandsInfo p:

                GameData.UpdateHandInfo(p);
                historyData.UpdateHandsInfo();

                for (int i = 0; i < MaxSeatCount; i++)
                {
                    if (userList.ContainsKey(i))
                    {
                        userList[i].chip.Value = 0;
                    }
                }

                yield break;

            case MPSC_MoreChipsInfo p:

                ///0 : rebuy,
                ///1 : addon

                if (p.bMode == 2)
                    yield break;

                if (IsMySeat(p.cSeat) || p.bMode == 1)
                {
                    UIManager.a.OpenPopup<UIPopup_ReEntry>(UIManager.POPUP_TYPE.NORMAL, (int)p.bMode, p);
                }

                yield break;

            case MPSC_MoreChipsResult p:

                if (userList.ContainsKey(p.cSeat))
                {
                    userList[p.cSeat].icon.Money = p.n64Chips;
                }

                yield break;

            default:

                //Debug.LogError($"Default Log: {packet}");

                yield break;
        }
    }

    bool isProgress = false;
    IEnumerator StartRender()
    {
        isProgress = true;
        while (renderQueue.Count > 0)
        {
            yield return StartCoroutine(renderQueue.Dequeue());
        }
        isProgress = false;
    }

    public void SetTableInnfo()
    {
        MySeatIndex = GameData.TableInfo.cSeat;
        MaxSeatCount = GameData.TableOption.bMaxPlayer;

        foreach (var v in userList)
        {
            v.Value.Delete();
        }

        userList = new Dictionary<int, UserInfo>();

        for (int i = 0; i < MaxSeatCount; i++)
        {
            if (GameData.UserInfo[i].dwSeatState == FPDefine.MD_ST.NONE)
                continue;

            CreateUserIcon(i);
            userList[i].SetActive(!GameData.UserInfo[i].dwSeatState.HasFlag(FPDefine.MD_ST.BUYIN_RESERVE));
        }

        RefreshChipScale();
        tableController.RefreshSelect();

        CurrentCommunityCards = new List<byte>();

        for (int i = 0; i < GameData.TableInfo.rBCard.bCount; i++)
        {
            CurrentCommunityCards.Add(GameData.TableInfo.rBCard.bCard[i]);
        }

        historyData.UpdateCommunityCard(CurrentCommunityCards);

        for (int i = 0; i < GameData.TableInfo.n64PBet.Length; i++)
        {
            if (GameData.TableInfo.n64PBet[i] > 0)
            {
                userList[i].chip.Value = GameData.TableInfo.n64PBet[i];
            }
        }

        for (int i = 0; i < GameData.TableInfo.rPCard.Length; i++)
        {
            if (GameData.TableInfo.rPCard[i].bCount > 0)
            {
                userList[i].icon.SetCards(GameData.TableInfo.rPCard[i].bCard);
            }
        }
    }

    public void CreateUserIcon(int index)
    {
        if (userList.ContainsKey(index))
            return;

        if (MySeatIndex >= 0 && IsMySeat(index))
        {
            //Debug.LogError($"PlayerIcon Instantiate:: {index}");

            PlayerIcon player = Instantiate(ResourceManager.Load<PlayerIcon>(Const.PLAYER_ICON)
                , tableController.trf_userPosition[index].transform);
            player.SeatIndex = index;

            UserChip chip = Instantiate(ResourceManager.Load<UserChip>(Const.USER_CHIP)
                , tableController.trf_chipPosition[index].transform);
            chip.Value = 0;
            chip.SetActive = player.Show;

            userList.Add(index, new UserInfo(chip, player));
            player.CreateData();
            tableController.UserPositionMove();
        }
        else
        {
            //Debug.LogError($"UserIcon Instantiate:: {index}");

            UserIcon icon = Instantiate(ResourceManager.Load<UserIcon>(Const.USER_ICON)
                , tableController.trf_userPosition[index].transform);
            icon.SeatIndex = index;

            UserChip chip = Instantiate(ResourceManager.Load<UserChip>(Const.USER_CHIP)
                , tableController.trf_chipPosition[index].transform);
            chip.Value = 0;
            chip.SetActive = icon.Show;

            userList.Add(index, new UserInfo(chip, icon));
            icon.CreateData();
        }
    }

    public void DeleteUser(int index)
    {
        if (index == MySeatIndex)
            MySeatIndex = -1;

        if (!userList.ContainsKey(index))
            return;

        userList[index].Delete();
        userList.Remove(index);
    }

    public void RefreshChipScale()
    {
        foreach (var v in userList)
        {
            v.Value.chip.UpdateScale();
        }
    }

    public IEnumerator DealtCardInfo(MPSC_HO_DealtCardInfo p)
    {
        int idx = (tableController.DealerTokenPosition + 1) % MaxSeatCount;

        for (int i = 0; i < MaxSeatCount; i++)
        {
            int _idx = idx;

            if (p.rPCard[_idx].bCount > 0)
            {
                //if (GameData.UserInfo[idx].dwSeatState != FPDefine.MD_ST.NONE)
                //{
                //    GameData.UserInfo[idx].dwSeatState = GameData.UserInfo[idx].dwSeatState & (~FPDefine.MD_ST.SITPLAY);
                //}

                StartCoroutine(tableController.MoveCard(userList[_idx].icon.trf_cardLeft, () => userList[_idx].icon.CardLeft = true));
                Sound.PlayEffect(Const.SOUND_CARD_DEALING_LOOP);
                yield return new WaitForSeconds(0.1f);
            }

            idx = (++idx) % MaxSeatCount;
        }

        idx = (tableController.DealerTokenPosition + 1) % MaxSeatCount;

        for (int i = 0; i < MaxSeatCount; i++)
        {
            int _idx = idx;

            if (p.rPCard[_idx].bCount > 0)
            {
                StartCoroutine(tableController.MoveCard(userList[_idx].icon.trf_cardRight, () => userList[_idx].icon.CardRight = true));
                Sound.PlayEffect(Const.SOUND_CARD_DEALING_LOOP);
                yield return new WaitForSeconds(0.1f);

                if (IsMySeat(idx))
                {
                    if (Option.HandCard)
                    {
                        UIManager.a.CloseAll();
                        MyCardAni ani = Instantiate(ResourceManager.Load<MyCardAni>(Const.MY_CARD_ANI), UIManager.a.transform);
                        StartCoroutine(ani.StartAni(p.rPCard[idx].bCard, 5f));
                    }

                    userList[idx].icon.SetCards(p.rPCard[idx].bCard);
                }
            }

            idx = (++idx) % MaxSeatCount;
        }
    }

    public IEnumerator MPSC_HO_ChipTableInfo(MPSC_HO_ChipTableInfo p)
    {
        tableController.TotalPot = p.n64CurPot.Sum() + p.n64PBet.Sum();

        foreach (var user in userList)
        {
            if (user.Value.chip.Value > 0)
            {
                StartCoroutine(tableController.MoveChip(user.Value.chip.MoneyPos, tableController.text_pot.transform.position, user.Value.chip.Value, () =>
                {
                    tableController.Pot += user.Value.chip.Value;
                }));

                user.Value.chip.Value = 0;
            }

            user.Value.icon.BetTypeClear();
        }

        tableController.Pot = p.n64CurPot.Sum();
        yield return new WaitForSeconds(0.5f);
    }

    public void MPSC_SeatDown2(MPSC_SeatDown2 p)
    {
        if (p.rSeatData.nUserNo == GameData.Player.UserNo)
            MySeatIndex = p.cSeat;

        GameData.UpdateSetDown2(p);
        tableController.RefreshSelect();

        if (IsMySeat(p.cSeat))
        {
            buttonController.SeatDown(p.rSeatData);
        }

        CreateUserIcon(p.cSeat);
        userList[p.cSeat].SetActive(GameManager.a.isTourney);
        RefreshChipScale();
    }

    public void MPSC_SeatInfo(MPSC_SeatInfo p)
    {
        GameData.UpdateSeatInfo(p);

        if (IsMySeat(p.cSeat))
        {
            buttonController.SeatInfo(p);
        }

        if (p.dwSeatState == FPDefine.MD_ST.NONE || p.dwSeatState.HasFlag(FPDefine.MD_ST.VIEWER))
        {
            DeleteUser(p.cSeat);
        }
        else
        {
            userList[p.cSeat].SetActive(true);
            userList[p.cSeat].icon.UpdateData();
        }

        tableController.RefreshSelect();
    }

    public IEnumerator MPSC_HO_SelectInfo(MPSC_HO_SelectInfo p)
    {
        tableController.TotalPot += p.n64Bet;

        if (!userList.ContainsKey(p.cSeat))
            yield break;

        UserInfo user = userList[p.cSeat];

        user.icon.Money = p.n64Account;
        user.icon.SetBetType(p.dwBtnFlags);
        user.icon.HideTimer();
        user.icon.MyTurn = false;

        if (p.n64Bet > 0)
        {
            yield return StartCoroutine(tableController.MoveChip(user.icon.MoneyPos, user.chip.MoneyPos, p.n64Bet, () =>
           {
               if (p.dwBtnFlags.HasFlag(FPDefine.MD_BTN.CALL))
               {
                   Sound.PlayEffect(Const.SOUND_CHIP_CALL);
               }
               else if (p.dwBtnFlags.HasFlagLeast(FPDefine.MD_BTN.RAISE, FPDefine.MD_BTN.BET))
               {
                   Sound.PlayEffect(Const.SOUND_CHIP_BET);
               }

               userList[p.cSeat].chip.Value += p.n64Bet;
           }));
        }

        if (MySeatState.HasFlag(FPDefine.MD_ST.SITPLAY | FPDefine.MD_ST.SITDOWN) && !MySeatState.HasFlag(FPDefine.MD_ST.SITOUT) && !IsMySeat(p.cSeat))
        {
            buttonController.UpdateToggleData(p);
        }

        historyData.UpdateSelectInfo(p);
    }

    public IEnumerator MPSC_HO_BlindInfo(MPSC_HO_BlindInfo p)
    {
        UserInfo user = userList[p.cSeat];

        user.icon.BigBlind = p.rBlind.dwBtnFlags == FPDefine.MD_BTN.BIG;
        user.icon.SmallBlind = p.rBlind.dwBtnFlags == FPDefine.MD_BTN.SMALL;
        user.icon.Money = p.n64Account;

        if (mySeatIndex >= 0)
        {
            buttonController.callValue = MyIcon.SmallBlind ? GameData.TableOption.n64SBlind : GameData.TableOption.n64BBlind;
            buttonController.prevCallValue = buttonController.callValue;
        }

        if (p.rBlind.n64Bet > 0)
        {
            yield return StartCoroutine(tableController.MoveChip(user.icon.MoneyPos, user.chip.MoneyPos, p.rBlind.n64Bet, () =>
              {
                  user.chip.Value += p.rBlind.n64Bet;
                  tableController.TotalPot += p.rBlind.n64Bet;
              }));
        }
    }

    public void ReceiveChat(MPBT_SysMsg p)
    {
        if (GameManager.a.isWCOP)
            return;

        if (p.bType == MPBT_SysMsg.Type.eB_CHAT_PLAYER && userList.ContainsKey(p.cSeat))
        {
            if (p.pMsg[0] != '/')
            {
                userList[p.cSeat].icon.ShowChat(p.pMsg);
            }
            else
            {
                if (p.pMsg.Length > 2)
                {
                    if (int.TryParse(p.pMsg.Substring(1, 3), out int idx))
                    {
                        userList[p.cSeat].icon.ShowEmoji(idx);
                    }
                }
            }
        }
    }

    public void OnClickHistory()
    {
        UIManager.a.OpenPopup<UIPopup_HandHistory>(UIManager.POPUP_TYPE.NORMAL, historyData);
    }
    public void OnClickSelect(int index)
    {
        //MySeatIndex = index;
        tableController.SelectActiveAll(false);
        SendSeatDown(index);
    }

    public void SendSeatDown(int index, bool bSeatDown = true)
    {
        MPCS_SitDownReq seatDownReq = new MPCS_SitDownReq();
        seatDownReq.HWord = FPPacket_R.HPD_PLAY_R;
        seatDownReq.LWord = FPPacket_R.MPCS_SITDOWN_REQ_TAG;
        seatDownReq.cSeat = (sbyte)index;
        seatDownReq.bType = (byte)(bSeatDown ? MPCS_SitDownReq.Type.eSR_ADD : MPCS_SitDownReq.Type.eSR_DEL);
        seatDownReq.bAutoSitdownType = 1;// bAutoSitDownType;
        socket.Send(seatDownReq);

        MPBT_HO_CheckOptionInfo checkOptionInfoReq = new MPBT_HO_CheckOptionInfo();
        checkOptionInfoReq.bType = MPBT_HO_CheckOptionInfo.Type.eCI_WAITFORBB;
        checkOptionInfoReq.bCheck = 0; //(byte)(toggleWait4BB.IsChecked == true ? 1 : 0);
        socket.Send(checkOptionInfoReq);

        MPCS_HO_ClientOptoinInfo clientOptionInfoReq = new MPCS_HO_ClientOptoinInfo();
        clientOptionInfoReq.bUseAutoRebuy = (byte)(Option.AutoBuyin ? 1 : 0); //(byte)(GameData.Config.IsAutoRefill ? 1 : 0);
        //clientOptionInfoReq.wDefaultBuyinBB = 0; //(ushort)(GameData.Config.IsAutoRefillFirst ? FirstBuyinValue : 200);
        socket.Send(clientOptionInfoReq);
    }

    public void SendChat(string str)
    {
        if (MySeatIndex < 0)
            return;

        if (GameManager.a.isWCOP)
            return;

        MPBT_SysMsg packet = new MPBT_SysMsg();
        packet.HWord = FPPacket.HPD_SYS;
        packet.LWord = FPPacket.MPBT_MSG_TAG;
        packet.bType = MPBT_SysMsg.Type.eB_CHAT;
        packet.cSeat = (sbyte)MySeatIndex;
        packet.bPlatform = GameData.iPlatform;
        packet.bSiteKey = 31;
        packet.pMsg = str;
        socket.Send(packet);
    }
}
