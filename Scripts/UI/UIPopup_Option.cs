using System;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System.Collections.Generic;

public class UIPopup_Option : UIPopup
{
    [System.Serializable]
    public class OptionTab
    {
        public ToggleEx toggle;
        public Transform[] obj_on;
        public Transform[] obj_off;

        public bool isOn
        {
            get { return toggle.isOn; }
            set
            {
                //toggle.isOn = value;

                for (int i = 0; i < obj_on.Length; i++)
                {
                    obj_on[i].SetActive(value);
                }

                for (int i = 0; i < obj_off.Length; i++)
                {
                    obj_off[i].SetActive(!value);
                }
            }
        }
    }

    public ToggleGroupEx tabGroup;

    public OptionTab tab_game;
    public OptionTab tab_system;
    public OptionTab tab_account;
    public ToggleGroupEx[] toggleGroups;

    public List<string> option_name = new List<string>();


    [Header("Account")]
    public Text text_id;
    public Text text_nickname;
    public Text text_email;
    public Text text_version;
    public Button button_contactUs;
    public Button button_logOut;

    public override void Init(params object[] args)
    {
        base.Init(args);

        option_name = new List<string>()
        {
            { "ShowRank" },
            { "ShowButton" },
            { "AutoBuyin" },
            { "CheckFold" },
            { "ColorCard" },
            { "ShowDown" },
            { "HandCard" },
            { "Effect" },
            { "Voice" },
            { "Alarm" },
            { "Vibe" },
        };

        tabGroup.onValueChanged.AddListener((v) =>
        {
            OnClickTab(v);
        });

        OnClickTab(0);

        Type option = typeof(Option);

        for (int i = 0; i < toggleGroups.Length; i++)
        {
            int index = i;

            PropertyInfo info = option.GetProperty(option_name[index]);

            int value = (bool)info.GetValue(option) ? 1 : 0;

            toggleGroups[index].TogglesChangeOn(value);
            toggleGroups[index].onValueChanged.AddListener((v) =>
            {
                OnClickToggle(index, v);
            });
        }

        text_id.text = GameData.Player.UserId;
        text_nickname.text = GameData.Player.UserName;
        text_email.text = GameData.Player.email;
        text_version.text = Application.version;

        button_contactUs.SetButton(() =>
        {
            GameManager.a.OpenWebView("*****");
        });

        button_logOut.SetButton(() =>
        {
            SceneManager.LoadLoginScene();
        });
    }

    public void OnClickTab(int index)
    {
        tab_game.isOn = index == 0;
        tab_system.isOn = index == 1;
        tab_account.isOn = index == 2;
    }

    public void OnClickToggle(int index, int value)
    {
        Type option = typeof(Option);
        PropertyInfo info = option.GetProperty(option_name[index]);
        info.SetValue(null, value == 1);
    }
}
