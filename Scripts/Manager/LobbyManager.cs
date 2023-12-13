using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using System;
using System.Collections;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager a;

    public Canvas canvas;

    [Header("List")]
    public ToggleEx toggle_Holdem;
    public ToggleEx toggle_Tournament;
    public ToggleEx toggle_WCOP;
    public ScrollRect holdemScrollRect;
    public ScrollRect tournamentScrollRect;
    public ScrollRect wcopScrollRect;

    [Header("Top")]
    public Button button_shop;
    public Button button_coffer;
    public Button button_Top_Profile;
    public Button button_Top_Menu;
    public Image img_avatar;
    public Text text_NickName;
    public Text text_TotalMoney;

    [Header("Bottom")]
    public Button button_Bottom_PrivateSetting;
    public Button button_Bottom_GamePoint;
    public Button button_Bottom_Ticket;
    public BonusSpin button_Bottom_Spin;
    public Text text_Bonus;
    public Text text_Ticket;
    private List<HoldemTableItem> holdemItemList;
    private List<TournamentTableItem> tournamentItemList;
    //private List<TournamentTableItem> wcopItemList;

    public TournamentTableItem FindTourneyItem(int no) => tournamentItemList.FirstOrDefault(x => x.Data.Number == no);

    public void Awake()
    {
        a = this;
    }

    IEnumerator Co_Init()
    {
        bool cashDone = false;

        NetworkManager.a.GetRoomInfo(0, list =>
        {
            GameData.InitHoldemTableList(list);
            CreateHoldemItem();
            cashDone = true;
        });

        bool tourneyDone = false;
        NetworkManager.a.SendTournamentList(list =>
        {
            GameData.InitTournamentTableList(list);
            CreateTournamentTableList();
            tourneyDone = true;
        });

        yield return new WaitUntil(() => tourneyDone && cashDone);

        if (GameData.TEntranceNotice2 != null)
        {
            GameManager.a.EnterTournament(GameData.TEntranceNotice2);
            GameData.TEntranceNotice2 = null;
        }
        else
        {
            if (GameData.EntranceNotice != null)
            {
                MS_TableInfo info = null;

                for (int i = 0; i < GameData.HoldemTableList.Count; i++)
                {
                    for (int m = 0; m < GameData.HoldemTableList[i].infos.Count; m++)
                    {
                        if (GameData.HoldemTableList[i].infos[m].info_h.wTableNo == GameData.EntranceNotice.table_access_info.tableInfo.wTableNo)
                        {
                            info = GameData.HoldemTableList[i].infos[m];
                            break;
                        }
                    }

                    if (info != null)
                        break;
                }

                if (info != null)
                {
                    GameManager.a.EnterHoldem(info, true);
                    GameData.EntranceNotice = null;
                }
            }
        }

        Debug.LogError($"LobbyManager Init Done. || {cashDone}, {tourneyDone}");
        GameManager.a.State = GameManager.GAME_STATE.LOBBY;

        NetworkManager.a.CheckDailyBonus((res) =>
        {
            if (res.rAttendance.bRewardEnable == 1)
            {
                UIManager.a.OpenPopup<UIPopup_DailyBonus>(UIManager.POPUP_TYPE.NORMAL, res);
            }
        });

        NetworkManager.a.CheckRoulette(res =>
        {
            button_Bottom_Spin.UpdateData(res);
        });
    }

    public void Init()
    {
        Debug.LogError("LobbyManager Init");

        NetworkManager.a.SendShopProductList(res =>
        {
            IAPManager.a.InitProduct(res.pCashProducts);
        });

        canvas.worldCamera = GameManager.a.mainCamera;
        canvas.sortingLayerName = "Main";
        tournamentItemList = new List<TournamentTableItem>();
        holdemItemList = new List<HoldemTableItem>();

        UpdateProfle();

        toggle_Holdem.isOn = true;
        ChangeTab(0);

        toggle_Holdem.onValueChanged.AddListener(active =>
        {
            if (!active)
                return;

            ChangeTab(0);
        });

        toggle_Tournament.onValueChanged.AddListener(active =>
        {
            if (!active)
                return;

            ChangeTab(1);
        });

        toggle_WCOP.onValueChanged.AddListener(active =>
        {
            if (!active)
                return;

            ChangeTab(2);
        });

        button_shop.SetButton(OpenShop);
        button_Top_Profile.SetButton(OpenProfilePopup);
        button_Top_Menu.SetButton(OpenMenuPopup);
        button_Bottom_GamePoint.SetButton(SendLobbyPointToMoneyReq);
        button_Bottom_Ticket.SetButton(OpenTicketPopup);
        button_coffer.SetButton(OepnCoffer);
        button_Bottom_PrivateSetting.SetButton(OpenWebView);
        StartCoroutine(Co_Init());
    }

    public void ChangeTab(int index)
    {
        holdemScrollRect.gameObject.SetActive(index == 0);
        tournamentScrollRect.gameObject.SetActive(index == 1);
        wcopScrollRect.gameObject.SetActive(index == 2);
    }

    #region Tournament Item List.
    public void EntrantAdd(LMSC_TLEntrantAdd info)
    {
        TournamentTableItem item = tournamentItemList.FirstOrDefault(x => x.Data.Number == info.nTournamentNo);

        if (info.t_entrant_info.nUserNo == GameData.Player.UserNo)
        {
            item.Data.t_info.bParticipated = 1;
        }

        item.RefreshData();
    }

    public void EntrantDel(LMSC_TLEntrantDel info)
    {
        TournamentTableItem item = tournamentItemList.FirstOrDefault(x => x.Data.Number == info.nTournamentNo);

        if (info.nUserNo == GameData.Player.UserNo)
        {
            item.Data.t_info.bParticipated = 0;
        }

        item.RefreshData();
    }

    public void TListChangeState(LMSC_TListChangeState info)
    {
        TournamentTableItem item = tournamentItemList.FirstOrDefault(x => x.Data.Number == info.nTournamentNo);

        if (item != null)
        {
            if (info.bCurrentState.HasFlagLeast(FHDefineMC.MD_CTS.CRASH, FHDefineMC.MD_CTS.HIDE, FHDefineMC.MD_CTS.END))
            {
                var index = GameData.TournamentTableList.FindIndex(t => t.Number == item.Data.Number);
                if (index >= 0) GameData.TournamentTableList.RemoveAt(index);
                tournamentItemList.Remove(item);
                Destroy(item.gameObject);
            }
            else
            {
                item.Data.t_info.bCurrentState = info.bCurrentState;
                item.RefreshData();
            }
        }
    }

    void TournamentScrollListSort()
    {
        tournamentItemList.Sort((t0, t1) =>
           {
               if (t0.Data.t_info.bCurrentState.HasFlagLeast(FHDefineMC.MD_CTS.END, FHDefineMC.MD_CTS.HIDE,
                   FHDefineMC.MD_CTS.CRASH))
                   return 1;
               else if (t1.Data.t_info.bCurrentState.HasFlagLeast(FHDefineMC.MD_CTS.END, FHDefineMC.MD_CTS.HIDE,
                   FHDefineMC.MD_CTS.CRASH))
                   return -1;

               return (int)(t0.Data.t_info.tStartTime - t1.Data.t_info.tStartTime);
           }
        );
        int idx = 0;
        foreach (var item in tournamentItemList) item.transform.SetSiblingIndex(idx++);
    }

    public void TListCreate3(LMSC_TListCreate3 info)
    {
        GameData.TournamentTable newTable = new GameData.TournamentTable()
        {
            Name = info.t_tour_info.szTournmentName,
            Number = info.t_tour_info.TournamentNo,
            info = info.t_tour_info
        };

        GameData.TournamentTableList.Add(newTable);
        GameData.TourneySort();

        TournamentTableItem item = CreateItem<TournamentTableItem>(Const.TOURNAMENT_ITEM, newTable.t_info.bTournamentType == 1 ? wcopScrollRect.content : tournamentScrollRect.content);
        item.SetData(newTable);
        tournamentItemList.Add(item);
        TournamentScrollListSort();
    }

    public void CreateTournamentTableList()
    {
        foreach (GameData.TournamentTable table in GameData.TournamentTableList)
        {
            if (table.info.State.HasFlagLeast(FHDefineMC.MD_CTS.CRASH, FHDefineMC.MD_CTS.HIDE, FHDefineMC.MD_CTS.END))
                continue;

            TournamentTableItem item = CreateItem<TournamentTableItem>(Const.TOURNAMENT_ITEM, table.t_info.bTournamentType == 1 ? wcopScrollRect.content : tournamentScrollRect.content);
            item.SetData(table);
            tournamentItemList.Add(item);
        }
    }
    #endregion

    #region Holdem Item List.
    public void CreateHoldemItem()
    {
        foreach (GameData.HoldemTable table in GameData.HoldemTableList)
        {
            HoldemTableItem item = CreateItem<HoldemTableItem>(Const.HOLDEM_ITEM, holdemScrollRect.content);
            item.SetData(table);
            holdemItemList.Add(item);
        }
    }

    public void RefreshHoldemItem()
    {
        foreach (var item in holdemItemList)
        {
            item.RefreshData();
        }
    }

    public void UpdateHoldemTableList(LMSC_MiddleTableInfo p)
    {
        foreach (HoldemTableItem item in holdemItemList)
        {
            MS_TableInfo info = item.Data.infos.FirstOrDefault(t => t.info_h.wTableNo == p.wTableNo);

            if (info != null)
            {
                info.info_m = p.table_info_m;
                item.RefreshData();
                break;
            }
        }
    }

    public void AddTable(LMSC_AddTable addInfo)
    {
        MS_TableInfo info = new MS_TableInfo
        {
            info_h = addInfo.table_info_h,
            bGSNo = addInfo.bGSNo
        };

        MS_TableOption option = GameData.GetTableOption(info);

        var table = GameData.HoldemTableList.FirstOrDefault(t =>
        {
            return t.MaxSeat == addInfo.table_info_h.bMaxSeat && t.Blind == option.n64SeedMoney && t.Buyin == option.n64TableMoney;
        });

        if (table == null)
        {
            GameData.HoldemTable newTable = new GameData.HoldemTable();
            newTable.infos = new List<MS_TableInfo>();
            newTable.infos.Add(info);
            newTable.option = option;

            GameData.HoldemTableList.Add(newTable);
            GameData.HoldemSort();
        }
        else
        {
            table.AddTable(info);
        }
    }

    public void DelTable(LMSC_DelTable delInfo)
    {
        foreach (GameData.HoldemTable item in GameData.HoldemTableList)
        {
            var info = item.infos.FirstOrDefault(t => t.info_h.wTableNo == delInfo.wTableNo);

            if (info != null)
            {
                item.RemoveTable(info);
            }
        }
    }

    #endregion

    #region Tournament

    public void TListChangePlayer(LMSC_TListChangePlayer info)
    {
        TournamentTableItem item = tournamentItemList.FirstOrDefault(x => x.Data.Number == info.nTournamentNo);

        if (item == null)
        {
            Debug.LogError($"TListChangePlayer :: cannot find item");
        }
        else
        {
            item.SetUserCount(info.wTotalEntrantCnt);
        }

        UIPopup_TournamentInfo popup_TournamentInfo = UIManager.a.FindPopup<UIPopup_TournamentInfo>();

        if (popup_TournamentInfo == null)
            return;

        popup_TournamentInfo.UpdateUserCount();
    }

    public void PrizebyEntryTable(LMSC_PrizebyEntryTable info)
    {
        UIPopup_TournamentInfo popup_TournamentInfo = UIManager.a.FindPopup<UIPopup_TournamentInfo>();

        if (popup_TournamentInfo == null)
        {
            Debug.LogError("UIPopup_TournamentInfo :: cannot find popup");
            return;
        }

        popup_TournamentInfo.RefreshReward(info);
    }

    public void LMSC_TListBustOut(int tournamentNo)
    {
        TournamentTableItem item = tournamentItemList.FirstOrDefault(x => x.Data.Number == tournamentNo);

        if (item != null)
        {
            item.RefreshButton();
        }
    }

    #endregion

    public T CreateItem<T>(string path, Transform parent) where T : LobbyListItem
    {
        T item = Instantiate<T>(ResourceManager.Load<T>(path));

        item.transform.SetParent(parent);
        item.transform.localScale = Vector3.one;
        item.transform.localPosition = Vector3.zero;

        return item;
    }

    public void UpdateProfle()
    {
        img_avatar.sprite = ResourceManager.LoadAvatar($"{GameData.Player.AvatarIndex}");
        text_NickName.text = GameData.Player.UserName;
        text_TotalMoney.text = Util.GetMoneyString(GameData.Player.TotalMoney);
        text_Bonus.text = Util.GetMoneyString(GameData.Player.GamePoint);
        text_Ticket.text = $"{GameData.Player.Ticket:N0}";
    }

    public void SendLobbyPointToMoneyReq()
    {
        var pck = new LMCS_LobbyPointToMoneyReq();
        pck.n64Point = -1;
        NetworkManager.Lobby.Send<LMSC_LobbyPointToMoneyRes>(pck, res =>
        {
            if (res.n64Point != 0 || res.n64Money != 0)
            {
                UIManager.a.OpenPopup<UIPopup_OneButton>(UIManager.POPUP_TYPE.NORMAL, $"{Util.GetMoneyString(GameData.Player.GamePoint)} 게임 보너스를 전환하였습니다.");

                GameData.Player.GamePoint = res.n64Point;
                GameData.Player.AvailMoney = res.n64Money;
                UpdateProfle();
            }

        });
    }

    public void OpenTicketPopup()
    {
        var pck = new LMCS_UserTicketListReq();
        NetworkManager.Lobby.Send<LMSC_UserTicketListRes2>(pck, res =>
        {
            if (res.mS_TicketInfos == null || res.mS_TicketInfos.Length == 0)
            {
                UIManager.a.OpenPopup<UIPopup_OneButton>(UIManager.POPUP_TYPE.NORMAL
                    , "사용가능한 토너먼트 티켓이 없습니다.");
            }
            else
            {
                UIManager.a.OpenPopup<UIPopup_Ticket>(UIManager.POPUP_TYPE.NORMAL, res);
            }
        });
    }

    public void OpenMenuPopup()
    {
        UIManager.a.OpenPopup<UIPopup_Menu>(UIManager.POPUP_TYPE.NORMAL);
    }

    public void OpenProfilePopup()
    {
        UIManager.a.OpenPopup<UIPopup_Profile>(UIManager.POPUP_TYPE.NORMAL);
    }

    public void OpenOptionPopup()
    {
        UIManager.a.OpenPopup<UIPopup_Option>(UIManager.POPUP_TYPE.NORMAL);
    }

    public void OpenDailyBonusPopup()
    {
        NetworkManager.a.CheckDailyBonus((res) =>
        {
            UIManager.a.OpenPopup<UIPopup_DailyBonus>(UIManager.POPUP_TYPE.NORMAL, res);
        });
    }

    public void OpenBonusSpinPopup()
    {
        NetworkManager.a.CheckRoulette(res =>
        {
            UIManager.a.OpenPopup<UIPopup_BonusSpin>(UIManager.POPUP_TYPE.NORMAL, res);
        });
    }

    public void OpenWebView()
    {
        GameManager.a.OpenWebView("*****");
    }

    public void OpenShop()
    {
        NetworkManager.a.SendShopProductList(res =>
        {
            UIManager.a.OpenPopup<UIPopup_Shop>(UIManager.POPUP_TYPE.NORMAL, res);
        });
    }

    public void OepnCoffer()
    {
        NetworkManager.a.OnClickCoffer(res =>
        {
            if (res.bErrorCode == LMSC_UserCofferInfoRes.STATE.ER_NO)
            {
                UIManager.a.OpenPopup<UIPopup_Coffer>(UIManager.POPUP_TYPE.NORMAL, res);
            }
            else
            {
                UIManager.a.OpenPopup<UIPopup_OneButton>(UIManager.POPUP_TYPE.NORMAL, Const.COFFER_ERROR_MSG[res.bErrorCode]);
            }
        });
    }
}

