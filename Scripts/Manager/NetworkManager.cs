using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UnityEngine;

public class NetworkManager : Singleton<NetworkManager>
{
    private LobbySocket LobbySocket;
    //public Dictionary<uint, RoomSocket> GameSocket = new Dictionary<uint, RoomSocket>();

    public static LobbySocket Lobby => a.LobbySocket;

    public int port;

    protected override void Initialize()
    {
        base.Initialize();

    }

    public void GetReEntryCount(int tNum, Action<LMSC_T_ReBuy_ReEntry> action)
    {
        LMSC_T_No pk = new LMSC_T_No();
        pk.nTournamentNo = tNum;
        LobbySocket.Send(pk, action);
    }

    public void SendTournamentList(Action<LMSC_T_ListRes3> action)
    {
        LMCS_T_ListReq req = new LMCS_T_ListReq
        {
            HWord = FPPacket_LSMC.HPD_LM_T_DATA,
            LWord = FPPacket_LSMC.LMCS_T_LIST_REQ_TAG
        };

        LobbySocket.Send(req, action);
    }

    public void SendTournamentView(int nTournaNo, byte bView, Action<LMSC_TLBasicInfo4, LMSC_TLBlindOption, LMSC_PrizebyEntryTable> action)
    {
        Debug.LogError(string.Format("nTouraNo: {0} :: {1}", nTournaNo, bView == 0 ? "구독취소" : "구독"));

        //View == 1 구독, 0 == 구독 취소.

        LMCS_TLViewChangeReq req = new LMCS_TLViewChangeReq
        {
            HWord = FPPacket_LSMC.HPD_LM_T_DATA,
            LWord = FPPacket_LSMC.LMCS_TL_VIEWCHANGE_REQ_TAG,
            nTournamentNo = nTournaNo,
            bView = bView
        };

        LobbySocket.Send(req, action);
    }

    public void ConnectToLobby(Action<LMSC_LoginRes3.Code> action)
    {
        LobbySocket = new GameObject("LobbySocket").AddComponent<LobbySocket>();
        LobbySocket.Connect(Config.a.ServerURL, Config.a.Port, action);
    }

    public void GetRoomInfo(byte type = 0, Action<LMSC_TableList> action = null)
    {
        LMCS_TableListReq req_tableList = new LMCS_TableListReq();
        req_tableList.HWord = FPPacket_LSMC.HPD_LM_DATA;
        req_tableList.LWord = FPPacket_LSMC.LMCS_TABLELIST_REQ_TAG;
        req_tableList.bSINo = type;
        LobbySocket.Send(req_tableList, action);
    }

    void OpenTable(RoomSocket socket, byte[] ip, int port, bool bObserve, Action<LMSC_LoginRes3.Code> action = null)
    {
        socket.Connect(GetIpFromByte(ip), port, b =>
        {
            if (b == LMSC_LoginRes3.Code.ER_NO)
            {
                //GameSocket.Add(socket.TableNumber, socket);
                //GameManager.a.AddRoom(socket.TableNumber, bObserve);
            }
            else
            {
                Debug.LogError($"Error: {b}");
            }

            action?.Invoke(b);
        });

    }


    public void EnterTournTable(int nTournamentNo, short tableNumber, byte[] ip, int port, int seatType, Action<LMSC_LoginRes3.Code> action = null)
    {
        TournamentSocket tournamentSocket = FindObjectOfType<TournamentSocket>();

        if (tournamentSocket == null)
        {
            Debug.LogError("Create TournamentSocket");
            tournamentSocket = new GameObject("TournamentSocket").AddComponent<TournamentSocket>();
        }
        else
        {
            Debug.LogError("TournamentSocket Overlap!!");
        }

        tournamentSocket.SetTableNumber(nTournamentNo, tableNumber);
        OpenTable(tournamentSocket, ip, port, seatType != 0, action);
    }

    public void JoinTable(byte serverNumber, ushort tableNum, int seatType, Action<LMSC_LoginRes3.Code> action = null)
    {
        RoomSocket roomSocket = FindObjectOfType<RoomSocket>();

        if (roomSocket == null)
        {
            Debug.LogError("Create Room Socket");
            roomSocket = new GameObject("RoomSocket").AddComponent<RoomSocket>();
        }
        else
        {
            Debug.LogError("RoomSocket Overlap!!");
        }

        roomSocket.SetRoomSocket(tableNum);

        var gsInfo = GameData.GameServers[serverNumber];
        OpenTable(roomSocket, gsInfo.dwServerIp, gsInfo.wGSPort, seatType != 0, action);
    }

    public void CheckDailyBonus(Action<LMSC_UserAttendanceInfoRes> callback)
    {
        var pck = new LMCS_UserAttendanceInfoReq();
        LobbySocket.Send<LMSC_UserAttendanceInfoRes>(pck, res =>
        {
            callback(res);
        });
    }

    public void OnClickDailyBonus(Action<LMSC_UserAttendanceRewardRes> callback)
    {
        var pck = new LMCS_UserAttendanceRewardReq();
        LobbySocket.Send<LMSC_UserAttendanceRewardRes>(pck, res =>
        {
            callback(res);
        });
    }

    public void CheckRoulette(Action<LMSC_UserRouletteInfoRes> callback)
    {
        var pck = new LMCS_UserRouletteInfoReq();
        LobbySocket.Send<LMSC_UserRouletteInfoRes>(pck, res =>
        {
            callback(res);
        });
    }

    public void OnClickRoulette(Action<LMSC_UserRouletteRewardRes> callback)
    {
        var pck = new LMCS_UserRouletteRewardReq();
        LobbySocket.Send<LMSC_UserRouletteRewardRes>(pck, res =>
        {
            callback(res);
        });
    }

    public void OnClickCoffer(Action<LMSC_UserCofferInfoRes> callback)
    {
        var pck = new LMCS_UserCofferInfoReq();
        LobbySocket.Send<LMSC_UserCofferInfoRes>(pck, res =>
        {
            callback(res);
        });
    }

    public void OnClickDeposit(LMCS_UserCofferTransferReq pck, Action<LMSC_UserCofferTransferRes> callback)
    {
        LobbySocket.Send<LMSC_UserCofferTransferRes>(pck, res =>
        {
            callback(res);
        });
    }

    public void OnClickCofferLog(Action<LMSC_UserCofferLogRes> callback)
    {
        var pck = new LMCS_UserCofferLogReq();
        LobbySocket.Send<LMSC_UserCofferLogRes>(pck, res =>
        {
            callback(res);
        });
    }

    public void SendWebTokenReq(Action<LMSC_WebTokenRes> callback)
    {
        var pck = new LMCS_WebTokenReq();
        LobbySocket.Send<LMSC_WebTokenRes>(pck, res =>
        {
            callback(res);
        });
    }

    public void SendShopProductList(Action<LMSC_ShopProductListRes> callback)
    {
        var pck = new LMCS_ShopProductListReq
        {
            bMarketType = MD_MARKET_TYPE.MD_MARKET_TYPE_GOOGLE
        };

        LobbySocket.Send<LMSC_ShopProductListRes>(pck, res =>
        {
            callback(res);
        });
    }

    public void SendShopIAPInfo(int productId, Action<LMSC_ShopIAPInfoRes> callback)
    {
        var pck = new LMCS_ShopIAPInfoReq
        {
            bMarketType = MD_MARKET_TYPE.MD_MARKET_TYPE_GOOGLE,
            nProductId = productId
        };

        LobbySocket.Send<LMSC_ShopIAPInfoRes>(pck, res =>
        {
            if (res.bErrorCode == LMSC_ShopIAPInfoRes.STATE.ER_NO)
            {
                callback(res);
            }
            else
            {
                UIManager.a.OpenPopup<UIPopup_OneButton>(UIManager.POPUP_TYPE.NORMAL, Const.SHOP_IAPINFO_ERROR_MSG[res.bErrorCode]);
            }
        });
    }

    public void SendShopIAPBuy(string receipt, string signature, Action<LMSC_ShopIAPBuyRes> callback)
    {
        var pck = new LMCS_ShopIAPBuyReq
        {
            bMarketType = MD_MARKET_TYPE.MD_MARKET_TYPE_GOOGLE,
            nProductId = 0,
            n64Amount = 0,
            szCurrency = "KRW",
            szReceipt = receipt,
            wReceiptSize = (short)receipt.Length,
            szSignature = signature,
            wSignatureSize = (short)signature.Length
        };

        LobbySocket.Send<LMSC_ShopIAPBuyRes>(pck, res =>
        {
            if (res.bErrorCode == LMSC_ShopIAPBuyRes.STATE.ER_NO)
            {
                callback(res);
            }
            else
            {
                UIManager.a.OpenPopup<UIPopup_OneButton>(UIManager.POPUP_TYPE.NORMAL, Const.SHOP_IAPBUY_ERROR_MSG[res.bErrorCode]);
            }
        });
    }

    public void SendShopAvatarBuy(int avatar, Action<LMSC_ShopAvatarBuyRes> callback)
    {
        var pck = new LMCS_ShopAvatarBuyReq
        {
            nAvatarSeq = avatar,
            bCount = 1
        };

        LobbySocket.Send<LMSC_ShopAvatarBuyRes>(pck, res =>
        {
            if (res.bErrorCode == LMSC_ShopAvatarBuyRes.STATE.ER_NO)
            {
                callback(res);
            }
            else
            {
                UIManager.a.OpenPopup<UIPopup_OneButton>(UIManager.POPUP_TYPE.NORMAL, Const.SHOP_AVATARBUY_ERROR_MSG[res.bErrorCode]);
            }
        });
    }

    public void SendSetColorTag(int userNo,  byte value ,Action<LMSC_UserColorTagSetRes> callback)
    {
        var pck = new LMCS_UserColorTagSetReq
        {
            nUserNo = userNo,
            bColor = value
        };

        LobbySocket.Send<LMSC_UserColorTagSetRes>(pck, res =>
        {
            callback(res);
        });
    }

    public static byte[] GetLocalIPAddress()
    {
        string strIP = "0.0.0.0";//addrInfo.Address.ToString();
        string[] arrayIP = strIP.Split('.');
        byte[] arrByte = new byte[4];
        for (int i = 0; i < 4; i++)
        {
            arrByte[i] = byte.Parse(arrayIP[i]);
            Debug.Log("StringIPToByte Num:" + i + " arrayIP:" + arrayIP[i] + " arrByte:" + arrByte[i]);
        }

        return arrByte;
    }

    public string GetIpFromByte(byte[] b)
    {
        return string.Format("{0}.{1}.{2}.{3}", b[0], b[1], b[2], b[3]);
    }

}
