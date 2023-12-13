using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RaisePopup : MonoBehaviour
{
    public Text txt_raise;
    public Button btn_up;
    public Button btn_x6;
    public Button btn_x4;
    public Button btn_x2;
    public Button btn_down;
    public Image img_slider_bg;
    public Slider obj_slider;
    public Transform trf_chip_parent;
    public Transform[] obj_origin_chip;
    private List<Transform> obj_chips = new List<Transform>();

    private const int CHIP_SIZE = 8;
    private const int MIN_COUNT = 1;
    private const int MAX_COUNT = 40;

    public long raiseValue = 0;
    InGameButtonController buttonController;
    bool IsPreFlop => (uint)InGameManager.a.CurrentTableState <= (uint)FPDefineTableState.HO_STATE_BET_FIRST;
    private Text text_x6;
    private Text text_x4;
    private Text text_x2;

    public bool SetActive
    {
        set
        {
            if (value)
            {
                if (IsPreFlop)
                {
                    text_x6.text = "4BB";
                    text_x4.text = "3BB";
                    text_x2.text = "2.5BB";
                }
                else
                {
                    text_x6.text = "100%";
                    text_x4.text = "66%";
                    text_x2.text = "33%";
                }

                var selReq = InGameManager.a.CurrentUserActionRequest;
                if (selReq != null)
                {
                    var min = System.Math.Max(selReq.rSelect.n64Raise, selReq.rSelect.n64Bet);

                    long v0 = GetQuickValue(0) - InGameManager.a.MyChip.Value;
                    long v1 = GetQuickValue(1) - InGameManager.a.MyChip.Value;
                    long v2 = GetQuickValue(2) - InGameManager.a.MyChip.Value;

                    btn_x6.interactable = (v0 >= min && v0 <= InGameManager.a.MyIcon.Money && v0 <= selReq.rSelect.n64MaxRaise);
                    btn_x4.interactable = (v1 >= min && v1 <= InGameManager.a.MyIcon.Money && v1 <= selReq.rSelect.n64MaxRaise);
                    btn_x2.interactable = (v2 >= min && v2 <= InGameManager.a.MyIcon.Money && v2 <= selReq.rSelect.n64MaxRaise);
                }
            }

            gameObject.SetActive(value);
        }
    }


    public void Awake()
    {
        Vector2 pos = obj_origin_chip[0].transform.localPosition;

        for (int i = 0; i < MIN_COUNT; i++)
        {
            Transform obj = CreateChip(i);
            obj.localPosition = new Vector3(Random.Range(-3f, 3f), pos.y + CHIP_SIZE);
            pos = obj.transform.localPosition;
        }

        for (int i = 0; i < MAX_COUNT; i++)
        {
            Transform obj = CreateChip(i);
            obj.localPosition = new Vector3(Random.Range(-3f, 3f), pos.y + CHIP_SIZE);
            pos = obj.transform.localPosition;
            obj.SetActive(false);
            obj_chips.Add(obj);
        }

        for (int i = 0; i < obj_origin_chip.Length; i++)
        {
            obj_origin_chip[i].SetActive(false);
        }

        obj_slider.onValueChanged.AddListener((value) =>
        {
            if (click)
            {
                click = false;
            }
            else
            {
                SetChipSlider(value);
            }
        });

        buttonController = FindObjectOfType<InGameButtonController>();

        btn_up.SetButton(() => OnClickBetUp(true));
        btn_down.SetButton(() => OnClickBetUp(false));
        btn_x6.SetButton(() => OnClickBet(0));
        btn_x4.SetButton(() => OnClickBet(1));
        btn_x2.SetButton(() => OnClickBet(2));

        text_x6 = btn_x6.transform.GetChild(0).GetComponent<Text>();
        text_x4 = btn_x4.transform.GetChild(0).GetComponent<Text>();
        text_x2 = btn_x2.transform.GetChild(0).GetComponent<Text>();
    }

    public Transform CreateChip(int i)
    {
        return Instantiate(obj_origin_chip[i / 10], trf_chip_parent);
    }

    bool click = false;

    public void OnClickBetUp(bool up)
    {
        HGS_HO_SELECT select = InGameManager.a.CurrentUserActionRequest.rSelect;

        long max = select.n64MaxRaise;
        long min = System.Math.Max(select.n64Bet, select.n64Raise);

        if ((up && raiseValue >= max) || (!up && raiseValue <= min))
            return;

        long addValue = up ? GameData.TableOption.n64BBlind : -GameData.TableOption.n64BBlind;

        UpdateRiasePopup(min, max, raiseValue + addValue);
    }

    private long GetQuickValue(int index) => IsPreFlop ?
            (long)(GameData.TableOption.n64BBlind * (index == 0 ? 4 : index == 1 ? 3 : 2.5)) :
            (long)(InGameManager.a.tableController.TotalPot * (index == 0 ? 1 : index == 1 ? 0.66f : 0.33f));

    public void OnClickBet(int index)
    {
        HGS_HO_SELECT select = InGameManager.a.CurrentUserActionRequest.rSelect;

        long max = select.n64MaxRaise;
        long min = System.Math.Max(select.n64Bet, select.n64Raise);

        UpdateRiasePopup(min, max, GetQuickValue(index) - InGameManager.a.MyChip.Value);
    }

    public void UpdateRiasePopup(long min, long max, long value)
    {
        raiseValue = value;

        if (raiseValue >= max)
            raiseValue = max;

        if (raiseValue <= min)
            raiseValue = min;

        click = true;

        txt_raise.text = Util.GetMoneyString(raiseValue + InGameManager.a.MyChip.Value, GameManager.a.isTourney);
        obj_slider.value = (float)(raiseValue - min) / (max - min);
        buttonController.text_raise_money.text = $"{Util.GetMoneyString(raiseValue + InGameManager.a.MyChip.Value, GameManager.a.isTourney)}";
        buttonController.text_bet_money.text = $"{Util.GetMoneyString(raiseValue + InGameManager.a.MyChip.Value, GameManager.a.isTourney)}";
        ChangeChip(obj_slider.value);

        bool allin = raiseValue == InGameManager.a.MyIcon.Money;
        buttonController.text_raise.text = allin ? "All in" : "Raise to";
        buttonController.btn_raise.image.sprite = ResourceManager.LoadButtonImage(allin);
        buttonController.text_bet.text = allin ? "All in" : "Bet";
        buttonController.btn_bet.image.sprite = ResourceManager.LoadButtonImage(allin);

        //Debug.LogError($"RaiseValue: {Util.GetMoneyString(raiseValue)}, View RaiseValue: {Util.GetMoneyString(raiseValue + InGameManager.a.MyChip.Value)}, Min: {Util.GetMoneyString(min)}, Max: {Util.GetMoneyString(max)}");
    }

    public void Clear()
    {
        obj_slider.value = 0;
        SetChipSlider(0);
    }

    void SetChipSlider(float v)
    {
        ChangeChip(v);

        HGS_HO_SELECT select = InGameManager.a.CurrentUserActionRequest.rSelect;

        long bet, value;

        if (select.dwBtnFlags.HasFlag(FPDefine.MD_BTN.BET))
        {
            bet = select.n64Bet;
        }
        else
        {
            bet = select.n64Raise;
        }

        value = (long)((select.n64MaxRaise - bet) * v);

        if (v < 1f)
        {
            value -= value % GameData.TableOption.n64BBlind;
        }

        raiseValue = bet + value;
        long viewRaiseValue = raiseValue + InGameManager.a.MyChip.Value;
        txt_raise.text = Util.GetMoneyString(viewRaiseValue, GameManager.a.isTourney);
        buttonController.text_raise_money.text = $"{Util.GetMoneyString(viewRaiseValue, GameManager.a.isTourney)}";
        buttonController.text_bet_money.text = $"{Util.GetMoneyString(viewRaiseValue, GameManager.a.isTourney)}";

        bool allin = raiseValue == InGameManager.a.MyIcon.Money;
        buttonController.text_raise.text = allin ? "All in" : "Raise to";
        buttonController.btn_raise.image.sprite = ResourceManager.LoadButtonImage(allin);
        buttonController.text_bet.text = allin ? "All in" : "Bet";
        buttonController.btn_bet.image.sprite = ResourceManager.LoadButtonImage(allin);
    }

    void ChangeChip(float v)
    {
        v = Mathf.Lerp(0, 1, v);

        for (int i = 0; i < obj_chips.Count; i++)
        {
            obj_chips[i].SetActive(i < MAX_COUNT * v);
        }

        img_slider_bg.color = Color.Lerp(Color.green, Color.red, v);
    }
}