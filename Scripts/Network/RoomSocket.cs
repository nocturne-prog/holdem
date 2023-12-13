using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Timers;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Assertions.Must;

public class RoomSocket : ClientSocket
{
    protected override string TAG => "Room" + TableNumber;

    public uint TableNumber = 0;
    private int autoSeatType;

    public bool Connected => socket.Connected;

    protected override void Reconnect()
    {
        StartCoroutine(Co_Reconnect());
    }

    IEnumerator Co_Reconnect()
    {
        UIManager.a.OpenLoading(true);

        if (bReconnecting)
            yield break;

        string prevToken = GameData.Player.szLoginToken;

        yield return new WaitUntil(() => !prevToken.Equals(GameData.Player.szLoginToken));

        bReconnecting = true;
        Connect(currentIp, currentPort, re =>
        {
            bReconnecting = false;

            if (re.HasFlag(LMSC_LoginRes3.Code.ER_NO))
            {
                UIManager.a.CloseLoading();
            }
            else
            {
                Invoke(nameof(Reconnect), 4f);
            }
        });
    }

    protected override void OnDisconnectHandler(FPDefineString.Define reason)
    {
        if (GameManager.a.State == GameManager.GAME_STATE.TOURNAMENT_DESTROY ||
            GameManager.a.State == GameManager.GAME_STATE.EXIT_LOBBY)
        {
            return;
        }
        else if (NetworkManager.Lobby.isConnect)
        {
            GameManager.a.ExitLobby();
        }
        else
        {
            Debug.LogError($"Room Socket OnDisconnectHandler :: {reason}, GAME_STATE = {GameManager.a.State}");

            if (bForcedDisconnect)
            {
                if (GameManager.a.State == GameManager.GAME_STATE.CASH_GAME ||
                    GameManager.a.State == GameManager.GAME_STATE.TOURNAMENT_GAME)
                {
                    UIPopup_OneButton popup =
                        UIManager.a.OpenPopup<UIPopup_OneButton>(UIManager.POPUP_TYPE.SYSTEM, Const.MSG_CONNECT_FAIL);
                    popup.callback += () => SceneManager.LoadLoginScene();
                }
            }
            else
            {
                Reconnect();
            }
        }
    }

    public void SetRoomSocket(uint tn)
    {
        TableNumber = tn;
        autoSeatType = FPDefine.MD_APST_NONE;
    }

    protected override void OnLoginProcess(Action<LMSC_LoginRes3.Code> result)
    {
        MPCS_R_ConnectReq packet_logIn = new MPCS_R_ConnectReq();

        packet_logIn.wClientType = FPDefine.MD_CLIENTTYPE_HO;

        packet_logIn.wVersionHigh = FPDefineHO_R.MD_RHO_HI_VERSION;
        packet_logIn.wVersionLow = FPDefineHO_R.MD_RHO_LO_VERSION;

        packet_logIn.bPlatform = GameData.iPlatform;
        packet_logIn.bLanguage = GameData.eLanguage;
        packet_logIn.wTableNo = (ushort)(TableNumber < 0 ? 0 : TableNumber);
        packet_logIn.szTablePW = "";
        packet_logIn.szUserID = GameData.Player.szTempID;
        packet_logIn.szLoginToken = GameData.Player.szLoginToken;
        packet_logIn.bAutoSitdownType = (byte)autoSeatType;
        packet_logIn.cSeat = -1;
        Send<MPSC_ConnectRes>(packet_logIn, res =>
        {
            var r = res as MPSC_ConnectRes;
            result.Invoke(LMSC_LoginRes3.Code.ER_NO);
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
            case MPSC_BuyinInfo p:
                UIManager.a.OpenPopup<UIPopup_BuyIn>(UIManager.POPUP_TYPE.NORMAL, p);
                return true;

            case MPSC_SeatInfo p:

                if (GameManager.a.isTourney)
                {
                    AddPacketQeueue(packet);
                }
                else
                {
                    InGameManager.a.MPSC_SeatInfo(p);
                }

                return true;

            case MPSC_SeatDown2 p:

                if (GameManager.a.isTourney)
                {
                    AddPacketQeueue(packet);
                }
                else
                {
                    InGameManager.a.MPSC_SeatDown2(p);
                }

                return true;

            case MPBT_SysMsg p:

                if (InGameManager.a != null)
                {
                    InGameManager.a.ReceiveChat(p);
                }

                return true;

            case MPBT_SysError er:

                //Logger.Error("Receive Error " + er.dwCode);

                if (er.dwCode == FPDefineString.Define.IDS_USER_SELF_LIMIT_BREAKTIME)
                {
                    UIPopup_OneButton popup = UIManager.a.OpenPopup<UIPopup_OneButton>(UIManager.POPUP_TYPE.SYSTEM, Const.IDS_ERROR_MSG[er.dwCode]);
                    popup.callback += Application.Quit;
                }
                else if (er.dwCode == FPDefineString.Define.IDS_NOT_YOU_RESERVEED_SEAT)
                {
                    UIManager.a.CloseAll();
                    UIManager.a.OpenPopup<UIPopup_Error>(UIManager.POPUP_TYPE.SYSTEM, Const.IDS_ERROR_MSG[er.dwCode]);
                }
                else if (er.dwCode == FPDefineString.Define.IDS_TOUR_DESTROY_BY_MINPALYER)
                {
                    GameManager.a.State = GameManager.GAME_STATE.TOURNAMENT_DESTROY;
                    UIPopup_OneButton popup = UIManager.a.OpenPopup<UIPopup_OneButton>(UIManager.POPUP_TYPE.SYSTEM, Const.IDS_ERROR_MSG[er.dwCode]);
                    popup.callback += () => GameManager.a.ExitLobby();
                }
                else if (er.dwCode == FPDefineString.Define.IDS_TOUR_DESTROY)
                {
                    GameManager.a.State = GameManager.GAME_STATE.TOURNAMENT_DESTROY;
                    UIPopup_OneButton popup = UIManager.a.OpenPopup<UIPopup_OneButton>(UIManager.POPUP_TYPE.SYSTEM, Const.IDS_ERROR_MSG[er.dwCode]);
                    popup.callback += () => GameManager.a.ExitLobby();
                }
                else
                {
                    return false;
                }

                return true;

            default: AddPacketQeueue(packet); return true;

        }
    }

    public Queue<HS_PK_HEADER> packetQueue = new Queue<HS_PK_HEADER>();

    void AddPacketQeueue(HS_PK_HEADER packet)
    {
        lock (packetQueue)
        {
            packetQueue.Enqueue(packet);
        }
    }
}
