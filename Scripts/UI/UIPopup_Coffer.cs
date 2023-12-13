using UnityEngine;
using UnityEngine.UI;

public class UIPopup_Coffer : UIPopup
{
    [System.Serializable]
    public class Toggle
    {
        public ToggleEx toggle;
        public Transform[] obj_on;
        public Transform[] obj_off;

        public bool isOn
        {
            get { return toggle.isOn; }
            set
            {
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

    [SerializeField] private ToggleGroupEx toggle_group;
    [SerializeField] private Toggle toggle_deposit;
    [SerializeField] private Toggle toggle_withdraw;
    [SerializeField] private Toggle toggle_log;
    [SerializeField] private Text text_currentMoney;
    [SerializeField] private Text text_cofferMoney;
    [SerializeField] private Text text_padMoney;
    [SerializeField] private Button button_deposit;
    [SerializeField] private Button button_withDraw;
    [SerializeField] private Button[] button_numPad;
    [SerializeField] private Button button_backSpace;
    [SerializeField] private Button button_Init;
    [SerializeField] private Button button_MaxMoney;
    [SerializeField] private ScrollRect scrollView;

    long l_money = 0;
    long LeftMoney
    {
        get { return l_money; }
        set
        {
            l_money = value;
            text_currentMoney.text = Util.GetMoneyString(l_money);
        }
    }

    long r_money = 0;
    long RightMoney
    {
        get { return r_money; }
        set
        {
            r_money = value;
            text_cofferMoney.text = Util.GetMoneyString(r_money);
        }
    }

    long padMoney = 0;
    long NumPadMoney
    {
        get { return padMoney; }
        set
        {
            padMoney = value;
            text_padMoney.text = Util.GetMoneyString(value);
        }
    }

    int state = 0;
    long MaxMoney => state == 0 ? LeftMoney : RightMoney;

    public override void Init(params object[] args)
    {
        base.Init(args);

        LMSC_UserCofferInfoRes res = (LMSC_UserCofferInfoRes)args[0];

        LeftMoney = res.n64AccountReal;
        RightMoney = res.n64AccountCoffer;
        NumPadMoney = 0;

        DepositWithDrawInit();
    }

    void DepositWithDrawInit()
    {
        toggle_group.onValueChanged.AddListener((v) =>
        {
            OnClickTab(v);
        });

        OnClickTab(0);

        button_deposit.SetButton(() => OnClickDeposit(true));
        button_withDraw.SetButton(() => OnClickDeposit(false));
        button_Init.SetButton(() => OnClickInit());
        button_MaxMoney.SetButton(() => OnClickMaxMoney());
        button_backSpace.SetButton(() => OnClickNumPadBackSpace());

        for (int i = 0; i < button_numPad.Length; i++)
        {
            int index = i;

            button_numPad[index].SetButton(() => OnClickNumPad(index));
        }
    }

    public void OnClickTab(int index)
    {
        toggle_deposit.isOn = index == 0;
        toggle_withdraw.isOn = index == 1;
        toggle_log.isOn = index == 2;

        state = index;
        NumPadMoney = 0;

        if (index == 2)
            UpdateLog();
    }

    void UpdateLog()
    {
        for (int i = 0; i < scrollView.content.childCount; i++)
        {
            Destroy(scrollView.content.GetChild(i).gameObject);
        }

        NetworkManager.a.OnClickCofferLog(res =>
        {
            for (int i = 0; i < res.pLogs.Length; i++)
            {
                Item_Coffer_Log item = Instantiate(ResourceManager.Load<Item_Coffer_Log>(Const.ITEM_COFFER_LOG), scrollView.content);
                item.UpdateData(res.pLogs[i]);
            }
        });
    }

    void OnClickDeposit(bool deposit)
    {
        if (NumPadMoney == 0)
            return;

        var pck = new LMCS_UserCofferTransferReq();
        pck.bDeposit = (byte)(deposit ? 1 : 0);
        pck.n64Amount = NumPadMoney;

        NetworkManager.a.OnClickDeposit(pck, res =>
        {
            if (res.bErrorCode == LMSC_UserCofferTransferRes.STATE.ER_NO)
            {
                NumPadMoney = 0;
                LeftMoney = res.n64AccountReal;
                RightMoney = res.n64AccountCoffer;
            }
            else
            {
                UIManager.a.OpenPopup<UIPopup_OneButton>(UIManager.POPUP_TYPE.NORMAL, Const.COFFER_TRANSFER_ERROR_MSG[res.bErrorCode]);
            }
        });
    }

    void OnClickInit()
    {
        NumPadMoney = 0;
    }

    void OnClickMaxMoney()
    {
        NumPadMoney = MaxMoney;
    }

    void OnClickNumPadBackSpace()
    {
        NumPadMoney /= 10;

        if (NumPadMoney <= 0)
            NumPadMoney = 0;
    }

    void OnClickNumPad(int index)
    {
        int value = index + 1;

        if (value > 10)
            value = -1;

        if (value < 0)
        {
            if (NumPadMoney == 0)
                NumPadMoney = 100;
            else
                NumPadMoney *= 100;
        }
        else if (value > 9)
        {
            if (NumPadMoney == 0)
                NumPadMoney = 10;
            else
                NumPadMoney *= 10;
        }
        else
        {
            NumPadMoney *= 10;
            NumPadMoney += value;
        }

        if (NumPadMoney >= MaxMoney)
            NumPadMoney = MaxMoney;
    }
}
