using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPopup_Profile : UIPopup
{
    public Image img_avatar;
    public Text text_id;
    public Text text_nickName;
    public Text text_gameBonus;
    public Text text_tournamentTicket;
    public Text text_totalMoney;
    public Text text_useMoney;
    public Text text_ableMoney;
    public Text text_record_play;
    public Text text_record_winRate;
    public Text text_record_join;
    public Text text_record_tropy;

    public Button button_changeProfle;

    public override void Init(params object[] args)
    {
        base.Init(args);

        img_avatar.sprite = ResourceManager.LoadAvatar($"{GameData.Player.AvatarIndex}");
        text_id.text = GameData.Player.UserId;
        text_nickName.text = GameData.Player.UserName;
        text_gameBonus.text = $"{GameData.Player.GamePoint:N0}";
        text_tournamentTicket.text = $"{GameData.Player.Ticket:N0}";
        text_totalMoney.text = $"{Util.GetMoneyString(GameData.Player.TotalMoney)}";
        text_useMoney.text = $"{  Util.GetMoneyString(GameData.Player.PlayMoney)}";
        text_ableMoney.text = $"{ Util.GetMoneyString(GameData.Player.AvailMoney)}";
        text_record_play.text = "-";
        text_record_winRate.text = "-";
        text_record_join.text = "-";
        text_record_tropy.text = "-";

        button_changeProfle.SetButton(() =>
        {
            MS_AvatarListReq packet = new MS_AvatarListReq();
            packet.HWord = FPPacket_LSMC.HPD_LM_DATA;
            packet.LWord = FPPacket_LSMC.LMCS_LOBBY_AVATAR_LIST_REQ_TAG;

            NetworkManager.Lobby.Send<LMSC_LobbyAvatarList>(packet, list =>
            {
                UIManager.a.OpenPopup<UIPopup_ChangeProfile>(UIManager.POPUP_TYPE.NORMAL, list);
                Close();
            });
        });
    }
}
