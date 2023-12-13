using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class UIPopup_Menu : UIPopup
{
    public Button btn_profile;
    public Button btn_option;
    public Button btn_dailyBonus;

    public override void Init(params object[] args)
    {
        base.Init(args);

        btn_profile.SetButton(() => LobbyManager.a.OpenProfilePopup());
        btn_option.SetButton(() => LobbyManager.a.OpenOptionPopup());
        btn_dailyBonus.SetButton(() => LobbyManager.a.OpenDailyBonusPopup());
    }
}
