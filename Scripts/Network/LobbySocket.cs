using System;
using System.Linq;
using UnityEngine;

public class LobbySocket : ClientSocket
{
    protected override string TAG => "Lobby";
    public bool isConnect => socket.Connected;

    public void LoginFail()
    {
        bDisconnectNotify = true;
        socket.Close();
    }

    protected override void OnDisconnectHandler(FPDefineString.Define reason)
    {
        Debug.LogError($"Lobby Socket OnDisconnectHandler :: {reason}");

        if (bForcedDisconnect)
        {
            UIPopup_OneButton popup = UIManager.a.OpenPopup<UIPopup_OneButton>(UIManager.POPUP_TYPE.SYSTEM, Const.MSG_CONNECT_FAIL);
            popup.callback += () => SceneManager.LoadLoginScene();
        }
        else
        {
            Reconnect();
        }
    }

    protected override void OnLoginProcess(Action<LMSC_LoginRes3.Code> result)
    {
        LMCS_LoginReq4 loginReq_pck = new LMCS_LoginReq4()
        {
            szID = GameData.Player.UserId,
            szPW = GameData.Player.Password,
            bPlatform = GameData.iPlatform,
            bLanguage = GameData.eLanguage,
            bSiteKey = (byte)8,
            wMCVersion = 22,
            bForceLoginYN = 1,
            bIP = NetworkManager.GetLocalIPAddress(),
            bClientPlatform = (byte)LMCS_LoginReq4.MD_CLIENTPLATFORM.MD_CLIENTPLATFORM_ANDROID,
            szClientVersion = Application.version
        };

        Send<LMSC_LoginRes3>(loginReq_pck, r =>
        {
            if (r.bErrorCode != 0)
            {

            }

            GameData.Player.szLoginToken = r.login_info.szLoginToken;
            GameData.Player.szTempID = r.id;
            GameData.Player.UserNo = r.login_info.nUserNo;
            GameData.Player.AvailMoney = r.login_info.n64TotalAccount;
            GameData.Player.PlayMoney = r.login_info.n64TotalPlayMoney;
            GameData.Player.UserName = r.login_info.osNickName;
            GameData.Player.GamePoint = r.login_info.n64FP;
            GameData.Player.Ticket = r.login_info.wTicketCount;
            GameData.Player.Male = r.login_info.bMale;
            GameData.Player.email = r.login_info.szEmail;
            GameData.Player.AvatarIndex = r.login_info.nAvatar;

            Logger.Log("Login result=" + r.bErrorCode);

            LMCS_UserColorTagListReq req = new LMCS_UserColorTagListReq();
            Send<LMSC_UserColorTagListRes>(req, res =>
            {
                GameData.UpdateUserColorTag(res);
            });

            RunInMainThread(() => result.Invoke(r.bErrorCode));
        });
    }

    protected override bool ReceivePacketHandler(HS_PK_HEADER packet)
    {
        return GetPacket(packet);
    }

    public bool GetPacket(HS_PK_HEADER packet)
    {
        switch (packet)
        {
            case LMSC_AddTable p:

                if (LobbyManager.a == null)
                    return true;

                LobbyManager.a.AddTable(p);
                return true;

            case LMSC_DelTable p:

                if (LobbyManager.a == null)
                    return true;

                LobbyManager.a.DelTable(p);
                return true;

            case LMSC_MiddleTableInfo p:

                if (LobbyManager.a == null)
                    return true;

                LobbyManager.a.UpdateHoldemTableList(p);
                return true;

            case LMSC_TLGameServerInfo p:

                GameData.UpdateTGameServerInfo(p);
                return true;

            case LMSC_EntranceNotice p:

                GameData.UpdateEntranceNotice(p);
                return true;

            case LMSC_TEntranceNotice2 p:

                //NetworkManager.a.SendTournamentView(p.nTournamentNo, 0, null);
                //NetworkManager.a.SendTournamentView(p.nTournamentNo, 1, (info, option, prize) =>
                //{
                //    Debug.LogError($"구독 완료! {prize.table_Lists.Length}");
                //    GameData.UpdateSubscriptionData(info, option, prize);
                //});

                if (GameManager.a.State == GameManager.GAME_STATE.CASH_GAME)
                {
                    UIManager.a.OpenPopup<UIPopup_OneButton>(UIManager.POPUP_TYPE.NORMAL, "토너먼트가 곧 시작됩니다.\n플레이중인 테이블을 종료해 주세요.");
                    GameData.UpdateTEntranceNotice2(p);
                    return true;
                }

                if (LobbyManager.a == null)
                {
                    GameData.UpdateTEntranceNotice2(p);
                }
                else
                {
                    GameManager.a.EnterTournament(p);
                }

                return true;

            case LMSC_TLChangeChips p:

                GameData.UpdateTLChangeChips(p);

                if (GameManager.a.isTourney)
                {
                    InGameManager.a.tableController.UpdateMyRank();
                }
                return true;

            case LMSC_TLEntrantsInfo p:

                GameData.UpdateTLEntrantsInfo(p);

                if (GameManager.a.isTourney)
                {
                    InGameManager.a.tableController.UpdateMyRank();
                }
                return true;

            case LMSC_TLChangeTableNo p:
                //if (GameManager.a.TourneyInfos.ContainsKey(p.nTournamentNo))
                //    GameManager.a.TourneyInfos[p.nTournamentNo].SetTableChange(p);
                return true;

            case LMSC_TLBustOut p:

                GameData.UpdateLMSC_TLBustOut(p);

                if (GameManager.a.isTourney)
                {
                    InGameManager.a.tableController.UpdateMyRank();
                }

                return true;

            case LMSC_TLEntrantAdd p:

                GameData.AddTLEntrantsInfo(p);

                if (GameManager.a.isTourney)
                {
                    InGameManager.a.tableController.UpdateMyRank();
                }

                if (LobbyManager.a == null)
                    return true;

                LobbyManager.a.EntrantAdd(p);

                return true;

            case LMSC_TLEntrantDel p:

                GameData.DeleteTLEntrantsInfo(p);

                if (GameManager.a.isTourney)
                {
                    InGameManager.a.tableController.UpdateMyRank();
                }

                if (LobbyManager.a == null)
                    return true;

                LobbyManager.a.EntrantDel(p);
                return true;

            case LMSC_TListChangePlayer p:

                GameData.UpdateTListChangePlayer(p);

                if (LobbyManager.a == null)
                    return true;

                LobbyManager.a.TListChangePlayer(p);
                return true;

            case LMSC_TListChangeState p:
                LobbyManager.a.TListChangeState(p);
                return true;

            case LMSC_PrizebyEntryTable p:
                GameData.TournamentSubData.prize = p;

                if (LobbyManager.a == null)
                    return true;

                LobbyManager.a.PrizebyEntryTable(p);
                return true;

            case LMSC_TListCreate3 p:

                if (LobbyManager.a == null)
                    return true;

                LobbyManager.a.TListCreate3(p);
                return true;

            case LMSC_SummaryInfo p:

                return true;

            case LMSC_LevelInfo p:

                GameData.UpdateLevelInfo(p);
                return true;

            case LMSC_AccountInfo p:

                GameData.Player.PlayMoney = p.account_info.n64TotalPlayMoney;
                GameData.Player.AvailMoney = p.account_info.n64TotalAccount;
                GameData.Player.GamePoint = p.account_info.n64FP;
                GameData.Player.Ticket = p.account_info.wTicketCount;

                if (LobbyManager.a != null)
                {
                    LobbyManager.a.UpdateProfle();
                    LobbyManager.a.RefreshHoldemItem();
                }

                return true;

            case LMSC_TListBustOut p:

                GameData.UpdateTListBustOut(p);

                if (LobbyManager.a != null)
                {
                    LobbyManager.a.LMSC_TListBustOut(p.nTournamentNo);
                }

                return true;

            case MPBT_SysError p:
            case LMSC_SysError q:

                UIPopup_OneButton popup_OneButton = UIManager.a.OpenPopup<UIPopup_OneButton>(UIManager.POPUP_TYPE.SYSTEM, Const.MSG_CONNECT_FAIL);
                popup_OneButton.callback += Application.Quit;
                return true;
        }

        return false;
    }
}
