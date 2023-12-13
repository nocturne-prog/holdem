using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class UserIcon : MonoBehaviour
{
    [SerializeField] protected Image image_avatar;
    [SerializeField] protected Image image_cardLeft;
    [SerializeField] protected Image image_cardRight;
    [SerializeField] protected GameObject[] obj_betTye;
    [SerializeField] protected Text text_nickName;
    [SerializeField] protected Text text_money;
    [SerializeField] protected GameObject obj_myTurn;
    [SerializeField] protected GameObject obj_die;
    [SerializeField] protected GameObject obj_bb;
    [SerializeField] protected GameObject obj_sb;
    [SerializeField] protected Image image_colorTag;
    [SerializeField] protected Transform trf_lock_timer;
    [SerializeField] protected Transform trf_sitout_timer;
    [SerializeField] protected Text text_timer;
    [SerializeField] protected Image img_timer;
    [SerializeField] protected Transform trf_emoji;
    [SerializeField] protected Transform trf_chat;
    [SerializeField] protected Transform trf_afk;
    [SerializeField] protected Button btn_profile;

    public byte[] cards { get; private set; }

    public int SeatIndex = -1;

    public MS_SeatData2 Data => GameData.UserInfo[SeatIndex];
    public FPDefine.MD_ST SeatState => Data.dwSeatState;

    public bool Show => SeatState != FPDefine.MD_ST.NONE && !SeatState.HasFlag(FPDefine.MD_ST.VIEWER);
    public Vector3 MoneyPos => text_money.transform.position;

    public bool SetActive
    {
        set
        {
            gameObject.SetActive(value);
        }
    }

    public bool Raise
    {
        set
        {
            if (value)
            {
                Sound.PlayVoice(Const.SOUND_BET_RAISE);
            }

            obj_betTye[0].SetActive(value);
        }
    }

    public virtual bool Fold
    {
        set
        {
            if (value)
            {
                Sound.PlayVoice(Const.SOUND_BET_FOLD);
            }

            obj_betTye[1].SetActive(value);
            CardLeft = false;
            CardRight = false;
        }
    }

    public bool Check
    {
        set
        {
            if (value)
            {
                Sound.PlayVoice(Const.SOUND_BET_CHECK);
            }

            obj_betTye[2].SetActive(value);
        }
    }

    public bool Bet
    {
        set
        {
            if (value)
            {
                Sound.PlayVoice(Const.SOUND_BET_BET);
            }

            obj_betTye[3].SetActive(value);
        }
    }

    public bool Allin
    {
        set
        {
            if (value)
            {
                Sound.PlayVoice(Const.SOUND_BET_ALLIN);
            }

            obj_betTye[4].SetActive(value);
        }
    }

    public bool Call
    {
        set
        {
            if (value)
            {
                Sound.PlayVoice(Const.SOUND_BET_CALL);
            }

            obj_betTye[5].SetActive(value);
        }
    }

    public bool AFK
    {
        set
        {
            trf_afk.SetActive(value);
        }
    }

    public string NickName
    {
        set { text_nickName.text = value; }
    }

    private long money;
    public long Money
    {
        get
        {
            return Data.n64Account;
        }
        set
        {
            AFK = SeatState.HasFlagLeast(FPDefine.MD_ST.SITOUT, FPDefine.MD_ST.SELECT_TIMEOVER);
            text_money.text = Util.GetMoneyString(value, GameManager.a.isTourney);
            Data.n64Account = value;
        }
    }

    public virtual bool MyTurn
    {
        set
        {
            obj_myTurn.SetActive(value);
        }
    }

    public virtual bool Die
    {
        get { return obj_die.activeSelf; }

        set
        {
            if (obj_die == null)
                return;

            obj_die.SetActive(value);
        }
    }

    public bool BigBlind
    {
        get { return obj_bb.activeSelf; }
        set { obj_bb.SetActive(value); }
    }

    public bool SmallBlind
    {
        get { return obj_sb.activeSelf; }
        set { obj_sb.SetActive(value); }
    }

    public bool CardLeft
    {
        set { image_cardLeft.gameObject.SetActive(value); }
    }

    public bool CardRight
    {
        set { image_cardRight.gameObject.SetActive(value); }
    }

    public Transform trf_cardLeft => image_cardLeft.transform;
    public Transform trf_cardRight => image_cardRight.transform;

    public void UpdateData()
    {
        if (SeatState == FPDefine.MD_ST.NONE ||
            SeatState.HasFlag(FPDefine.MD_ST.VIEWER))
            Delete();

        SetBetType(FPDefine.MD_BTN.NONE);
        Money = Data.n64Account;
    }

    public void CreateData()
    {
        SetBetType(FPDefine.MD_BTN.NONE);

        CardLeft = false;
        CardRight = false;
        MyTurn = false;
        Die = false;
        BigBlind = false;
        SmallBlind = false;

        NickName = Data.osNickName;
        Money = Data.n64Account;
        image_avatar.sprite = ResourceManager.LoadAvatar($"{Data.nAvatar}");
        btn_profile.SetButton(() => InGameManager.a.tableController.userMarking.Show(this));
        UpdateColorTag();
    }

    public void UpdateColorTag()
    {
        if (this is PlayerIcon)
            return;

        if (GameData.UserColorTag.ContainsKey(Data.nUserNo))
        {
            byte value = GameData.UserColorTag[Data.nUserNo];

            image_colorTag.SetActive(value > 0);
            image_colorTag.color = InGameManager.a.tableController.userMarking.setColor[GameData.UserColorTag[Data.nUserNo]];
        }
        else
        {
            image_colorTag.SetActive(false);
        }
    }

    public void SetBetType(FPDefine.MD_BTN type)
    {
        BetTypeClear();

        if (type == FPDefine.MD_BTN.NONE)
            return;

        if (type.HasFlag(FPDefine.MD_BTN.FOLD))
        {
            Fold = true;
            Die = true;
        }
        else if (type.HasFlagLeast(FPDefine.MD_BTN.ALLIN_CALL, FPDefine.MD_BTN.ALLIN_RAISE) || Money == 0)
            Allin = true;
        else if (type.HasFlag(FPDefine.MD_BTN.CALL))
            Call = true;
        else if (type.HasFlag(FPDefine.MD_BTN.RAISE))
            Raise = true;
        else if (type.HasFlag(FPDefine.MD_BTN.CHECK))
            Check = true;
        else if (type == FPDefine.MD_BTN.BET)
            Bet = true;
    }

    public void BetTypeClear()
    {
        for (int i = 0; i < obj_betTye.Length; i++)
        {
            obj_betTye[i].SetActive(false);
        }
    }

    public virtual void Clear()
    {
        SetBetType(FPDefine.MD_BTN.NONE);

        CardLeft = false;
        CardRight = false;
        MyTurn = false;
        Die = false;
        BigBlind = false;
        SmallBlind = false;
        cards = null;

        Util.SetCard(image_cardLeft, 0);
        Util.SetCard(image_cardRight, 0);
    }

    public void Delete()
    {
        Destroy(gameObject);
    }

    bool timerStop = false;
    public void HideTimer()
    {
        SetTimer(MPSC_TimeInfo.TimerType.eTI_HIDE, 0);
    }

    public void SetTimer(MPSC_TimeInfo.TimerType type, float v, Action c = null)
    {
        if (v == 0)
        {
            timerStop = true;
            return;
        }

        timerStop = false;

        if (type == MPSC_TimeInfo.TimerType.eTI_NORMAL)
        {
            StartCoroutine(BarTimer(v, c));
            trf_lock_timer.SetActive(false);
            trf_sitout_timer.SetActive(false);
            text_timer.SetActive(false);
        }
        else
        {
            StartCoroutine(TextTimer(type, v));
        }
    }

    IEnumerator TextTimer(MPSC_TimeInfo.TimerType type, float v)
    {
        trf_lock_timer.SetActive(type == MPSC_TimeInfo.TimerType.eTI_LOCK);
        trf_sitout_timer.SetActive(type == MPSC_TimeInfo.TimerType.eTI_SITOUT);
        text_timer.SetActive(true);

        DateTime targetTime = DateTime.Now.AddSeconds(v);
        TimeSpan time = targetTime - DateTime.Now;

        while (time.TotalSeconds > 0 && timerStop == false)
        {
            time = targetTime - DateTime.Now;
            text_timer.text = time.ToString(@"mm\:ss");
            yield return null;
        }

        trf_lock_timer.SetActive(false);
        trf_sitout_timer.SetActive(false);
        text_timer.SetActive(false);

        yield break;
    }

    bool playSound = false;
    IEnumerator BarTimer(float v, Action endCallback)
    {
        img_timer.transform.localScale = Vector3.one;
        img_timer.SetActive(true);

        DateTime targetTime = DateTime.Now.AddSeconds(v);
        TimeSpan time = targetTime - DateTime.Now;

        Color32 g = new Color32(0, 255, 21, 255);
        Color32 y = new Color32(255, 229, 0, 255);
        Color32 r = new Color32(255, 0, 42, 255);

        //Color start = new Color(0, 255, 21);
        //Color end = Color.

        while (time.TotalSeconds > 0 && timerStop == false)
        {
            time = targetTime - DateTime.Now;
            float value = (float)time.TotalSeconds / v;

            //Debug.LogError($"{time.TotalSeconds} / {v} ::: {value}");

            if ((int)(time.TotalSeconds) == 5 && !playSound)
            {
                Sound.PlayEffect(Const.SOUND_TIME_LIMIT);
                playSound = true;
            }

            float oneValue = 1 - value;

            if (oneValue > 0.5f)
            {
                img_timer.color = Color32.Lerp(y, r, (oneValue - 0.5f) * 2 * 1.2f);
            }
            else
            {
                img_timer.color = Color32.Lerp(g, y, oneValue * 2 * 1.2f);
            }

            img_timer.transform.localScale = new Vector3(value, 1, 1);

            yield return null;
        }

        img_timer.SetActive(false);

        if (!timerStop)
            endCallback?.Invoke();

        playSound = false;
        Sound.Stop(Const.SOUND_TIME_LIMIT);
        yield break;
    }

    public void SetCards(byte[] card, bool ani = false)
    {
        cards = card;

        if (ani)
        {
            SetCardAni(image_cardLeft, cards[0]);
            SetCardAni(image_cardRight, card[1]);
        }
        else
        {
            Util.SetCard(image_cardLeft, card[0]);
            Util.SetCard(image_cardRight, card[1]);

            CardLeft = true;
            CardRight = true;
        }
    }

    public void SetCardAni(Image target, byte card)
    {
        float duration = 0.4f;

        float originZ = target.transform.eulerAngles.z > 180 ? target.transform.eulerAngles.z - 360 : target.transform.eulerAngles.z;

        target.sprite = ResourceManager.LoadCardBack<Sprite>(false);
        target.SetActive(true);

        var vec = Quaternion.LookRotation(GameManager.a.mainCamera.transform.position - target.transform.position).eulerAngles;
        float gap = (180 - vec.y);

        target.transform.localRotation = Quaternion.identity;
        target.gameObject.AddTween(new TweenRotateTo(0, 90 - gap, 0, duration / 2)
            .EndEvent((Action)(() =>
            {
                target.transform.localRotation = Quaternion.AngleAxis(270 - gap, Vector3.up);
                Util.SetCard(target, card);
            }))
        .Next(new TweenRotateTo(0, 360, 0, duration / 2))
        .Next(new TweenRotateTo(0, 360, originZ, 0.1f)));
    }

    public void ShowEmoji(int index)
    {
        EmojiAni.Create(trf_emoji, index, transform.position.x < 0);
    }

    public void ShowChat(string text)
    {
        if (text.Length >= 36)
        {
            text = text.Substring(0, 36);
            text += "...";
        }

        StringBuilder sb = new StringBuilder();

        foreach (var v in GetNextChars(text, 12))
        {
            sb.AppendLine(v);
        }

        ChatBubbleAni.Create(trf_chat, sb.ToString());
    }

    IEnumerable<string> GetNextChars(string str, int iterateCount)
    {
        var words = new List<string>();

        for (int i = 0; i < str.Length; i += iterateCount)
            if (str.Length - i >= iterateCount) words.Add(str.Substring(i, iterateCount));
            else words.Add(str.Substring(i, str.Length - i));

        return words;
    }
}
