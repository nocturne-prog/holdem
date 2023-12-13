using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TournamentSocket : RoomSocket
{
    protected override string TAG => "Tourney" + (TableNumber >> 16) + ":" + (TableNumber & 0xFFFF);

    public void SetTableNumber(int nTournamentNo, short tableNumber)
    {
        TableNumber = (uint)((nTournamentNo << 16) + tableNumber);
    }

    protected override void OnLoginProcess(Action<LMSC_LoginRes3.Code> result)
    {
        MPCS_T_ConnectReq packet_login = new MPCS_T_ConnectReq();
        packet_login.wClientType = FPDefine_T.MD_CLIENTTYPE_THO;

        packet_login.wVersionHigh = FPDefineHO_T.MD_THO_HI_VERSION;
        packet_login.wVersionLow = FPDefineHO_T.MD_THO_LO_VERSION;

        packet_login.nTournamentNo = ((int)(TableNumber >> 16));

        packet_login.bPlatform = GameData.iPlatform;
        packet_login.bLanguage = GameData.eLanguage;
        packet_login.wTableNo = (ushort)(TableNumber & 0xFFFF);
        packet_login.szUserID = GameData.Player.UserId;
        packet_login.szLoginToken = GameData.Player.szLoginToken;

        packet_login.nMCUserNo = GameData.Player.UserNo;
        packet_login.bTakeMySeat = (byte)(packet_login.wTableNo == 0 ? 1 : 0);

        Send<MPSC_ConnectRes>(packet_login, res =>
        {
            UIManager.a.CloseAll();
            var r = res as MPSC_ConnectRes;
            result.Invoke(LMSC_LoginRes3.Code.ER_NO);
        });
    }
}
