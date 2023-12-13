
using AOT;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UIPopup_BuyIn : UIPopup
{
    public Text text_account;
    public Text text_min;
    public Text text_max;
    public Slider slider;
    public Text text_slider;
    public Button button_buyin;
    public Button button_left;
    public Button button_right;

    long min = 0;
    long max = 0;
    long currentBuyin = 0;

    public long Buyin
    {
        get { return currentBuyin; }
        set
        {
            currentBuyin = value;
            text_slider.text = $"{Util.GetMoneyString(currentBuyin)}";
        }
    }

    long bb = 0;
    long gap = 0;

    bool click = false;

    public override void Init(params object[] args)
    {
        MPSC_BuyinInfo info = (MPSC_BuyinInfo)args[0];

        text_account.text = $"{Util.GetMoneyString(info.n64Account)}";

        Buyin = info.n64DefaultBuyinAccount;
        min = info.n64Min;
        max = System.Math.Min(info.n64Max,info.n64Account);

        text_min.text = $"{Util.GetMoneyString(min)}";
        text_max.text = $"{Util.GetMoneyString(max)}";


        slider.value = 0;

        bb = info.n64Max / 100;
        gap = max - min;

        slider.onValueChanged.AddListener((v) =>
        {
            if (!click)
            {
                Buyin = info.n64Min + (long)(gap * v);
                Buyin -= Buyin % GameData.TableOption.n64BBlind;
            }
            else
                click = false;
        });

        button_left.SetButton(() => OnClickArrow(true));
        button_right.SetButton(() => OnClickArrow(false));
        button_buyin.SetButton(() => OnClickBuyin());
        button_Close.SetButton(() => OnClickExit());
    }

    public void OnClickArrow(bool left)
    {
        if (left)
        {
            if (Buyin <= min)
                return;

            long remain = (Buyin - min) % bb;
            long value = (Buyin - min) / bb;


            if (remain == 0)
            {
                value--;
            }

            Buyin = min + (bb * value);
        }
        else
        {
            if (Buyin >= max)
                return;

            long value = (Buyin - min) / bb;
            Buyin = min + (bb * (value + 1));
        }

        click = true;
        slider.value = (float)(Buyin - min) / (max - min);
    }

    public void OnClickBuyin()
    {
        button_buyin.interactable = false;

        MPCS_AccountReq accountReq = new MPCS_AccountReq();
        accountReq.HWord = FPPacket_R.HPD_PLAY_R;
        accountReq.LWord = FPPacket_R.MPCS_ACCOUNT_REQ_TAG;
        accountReq.bType = (byte)MPCS_AccountReq.Type.eAR_BUYIN;
        accountReq.n64Cash = Buyin;
        InGameManager.a.socket.Send(accountReq);

        Close();
    }

    public void OnClickExit()
    {
        InGameManager.a.SendSeatDown(-1, false);
        Close();
    }

}
