using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InGameTableController : MonoBehaviour
{
    public Transform tokenPositionParent;
    public Transform userChipPositionParent;
    public Transform userPositionParent;
    public Button btn_auto_buyin;
    public Button btn_changeRoom;
    public Button btn_rank;
    public Text text_rank;
    public Text text_stake;
    public Text text_totalPot;
    public Text text_pot;
    public Image[] img_cards;
    public LastCard obj_lastCard;
    public Transform moveCardContainer;
    public Transform moveCard;
    public Transform moveChipContainer;
    public Transform[] moveChip;
    public Image obj_table;
    public Image obj_bg;
    public Image obj_frame;
    public Sprite[] img_table;
    public Sprite[] img_bg;
    public Sprite[] img_frame;
    public GameObject obj_win_rank;
    public Text text_win_rank;
    public GameObject obj_next_blind;
    public Text text_next_blind;
    public UserMarking userMarking;

    [System.Serializable]
    public struct LastCard
    {
        public GameObject parent;
        public Animator ani;
        public SkinnedMeshRenderer mesh;
        public Material front;
        public Material back;
    }

    public Transform[] trf_tokenPosition { get; private set; }
    public Transform[] trf_chipPosition { get; private set; }
    public Transform[] trf_userPosition { get; private set; }

    private long totalPot;
    public long TotalPot
    {
        get => totalPot;
        set
        {
            totalPot = value;
            text_totalPot.text = $"Total Pot : {Util.GetMoneyString(value, GameManager.a.isTourney)}";
        }
    }

    private long pot;
    public long Pot
    {
        get => pot;
        set
        {
            pot = value;
            text_pot.text = $"{Util.GetMoneyString(value, GameManager.a.isTourney)}";
            text_pot.transform.SetActive(value > 0);
        }
    }

    Transform dealerToken = null;
    private int dealerPosition;
    public int DealerTokenPosition
    {
        get
        {
            return dealerPosition;
        }

        set
        {
            dealerToken.SetParent(trf_tokenPosition[value]);
            dealerToken.transform.localPosition = Vector3.zero;
            dealerPosition = value;
        }
    }

    List<UserSelect> userSelectList = new List<UserSelect>();

    public int zeroPositionIndex = 0;

    public void Init()
    {
        MPSC_HO_TableOption option = GameData.TableOption;

        //GameData.TournamentSubData.info.t_semi_info.bAnteLevel
        SetText_Stake(option.n64SBlind, option.n64BBlind, option.n64BBlind / 10);

        if (GameManager.a.isTourney)
        {
            UpdateMyRank();
        }

        int maxSeat = option.bMaxPlayer;

        int typeIndex = maxSeat == 6 ? 0 : 1;

        for (int i = 0; i < userPositionParent.childCount; i++)
        {
            bool isEnable = i == typeIndex;

            userChipPositionParent.GetChild(i).SetActive(isEnable);
            tokenPositionParent.GetChild(i).SetActive(isEnable);
            userPositionParent.GetChild(i).SetActive(isEnable);
        }

        trf_tokenPosition = new Transform[maxSeat];
        trf_chipPosition = new Transform[maxSeat];
        trf_userPosition = new Transform[maxSeat];

        UserSelect prefab = ResourceManager.Load<UserSelect>(Const.USER_SELECT);

        foreach (var select in userSelectList)
        {
            Destroy(select.gameObject);
        }

        userSelectList = new List<UserSelect>();

        for (int i = 0; i < maxSeat; i++)
        {
            int index = i;

            trf_tokenPosition[index] = tokenPositionParent.GetChild(typeIndex).GetChild(index).transform;
            trf_chipPosition[index] = userChipPositionParent.GetChild(typeIndex).GetChild(index).transform;
            trf_userPosition[index] = userPositionParent.GetChild(typeIndex).GetChild(index).transform;

            if (GameManager.a.isTourney)
                continue;

            UserSelect select = Instantiate(prefab, trf_userPosition[index].transform);
            select.transform.localPosition = Vector3.zero;
            select.Init(index, (idx) => InGameManager.a.OnClickSelect(idx));
            userSelectList.Add(select);
        }

        ReservActiveAll(false);
        SelectActiveAll(false);

        TotalPot = 0;
        Pot = 0;

        if (dealerToken == null)
            dealerToken = Instantiate(ResourceManager.Load<Transform>(Const.DEALER_TOKEN), trf_tokenPosition[0]);

        btn_auto_buyin.SetButton(() => OnClickAutoBuyin());

        obj_next_blind.SetActive(GameManager.a.isTourney);

        btn_rank.SetActive(GameManager.a.isTourney);
        btn_rank.SetButton(() =>
        {
            UIManager.a.OpenPopup<UIPopup_InGameTournamentInfo>(UIManager.POPUP_TYPE.NORMAL);
        });

        btn_changeRoom.SetActive(GameManager.a.State == GameManager.GAME_STATE.CASH_GAME);
        btn_changeRoom.SetButton(() =>
        {
            if (InGameManager.a.MySeatState == FPDefine.MD_ST.NONE)
            {
                GameManager.a.OnClickChangeRoom();
            }
            else
            {
                UIManager.a.OpenPopup<UIPopup_OneButton>(UIManager.POPUP_TYPE.NORMAL, "관전상태에서만 가능합니다.");
            }
        });

        obj_lastCard.front = obj_lastCard.mesh.materials[0];
        obj_lastCard.back = obj_lastCard.mesh.materials[1];

        moveCard.SetActive(false);

        for (int i = 0; i < moveChip.Length; i++)
        {
            moveChip[i].SetActive(false);
        }
    }

    public void UpdateTableInfo(MPSC_HO_TableInfo p)
    {
        for (int i = 0; i < img_cards.Length; i++)
        {
            img_cards[i].SetActive(false);
        }

        if (p.rBCard.bCount > 0)
        {
            for (int i = 0; i < p.rBCard.bCount; i++)
            {
                OpenCardImmediately(i, p.rBCard.bCard[i]);
            }
        }

        for (int i = 0; i < p.n64CurPot.Length; i++)
        {
            if (p.n64CurPot[i] > 0)
            {
                Pot += p.n64CurPot[i];
                TotalPot += p.n64CurPot[i];
            }
        }
    }

    public void StageClear()
    {
        TotalPot = 0;
        Pot = 0;

        for (int i = 0; i < img_cards.Length; i++)
        {
            img_cards[i].SetActive(false);
        }

        obj_lastCard.parent.SetActive(false);
        ShowWinRank(false, "");
    }

    public void UserPositionMove()
    {
        if (InGameManager.a.MySeatIndex == 0 && zeroPositionIndex == 0)
            return;

        PositionSetup(trf_tokenPosition);
        PositionSetup(trf_chipPosition);
        PositionSetup(trf_userPosition);

        zeroPositionIndex = InGameManager.a.MySeatIndex;
    }

    void PositionSetup(Transform[] positions)
    {
        int me = InGameManager.a.MySeatIndex;
        int max = InGameManager.a.MaxSeatCount;

        List<Vector3> originPosition = new List<Vector3>(positions.Select(x => x.localPosition));
        List<int> sortList = new List<int>();

        for (int i = me; i < max + me; i++)
        {
            int index = (int)Mathf.Repeat(i, max);
            sortList.Add(index);
        }

        for (int i = 0; i < positions.Length; i++)
        {
            int index = (int)Mathf.Repeat(i + zeroPositionIndex, max);
            positions[sortList[i]].localPosition = originPosition[index];
        }
    }

    public void RefreshSelect()
    {
        if (GameManager.a.isTourney)
            return;

        for (int i = 0; i < userSelectList.Count; i++)
        {
            FPDefine.MD_ST state = GameData.UserInfo[i].dwSeatState;

            if (InGameManager.a.isObserve)
            {
                SelectActive(i, false);
            }
            else
            {
                if (InGameManager.a.MySeatState.HasFlag(FPDefine.MD_ST.SITDOWN))
                    SelectActive(i, false);
                else
                    SelectActive(i, state == FPDefine.MD_ST.NONE);
            }

            ReservActive(i, state.HasFlag(FPDefine.MD_ST.BUYIN_RESERVE));
        }

        bool active = !InGameManager.a.isObserve
                        && InGameManager.a.MySeatState == FPDefine.MD_ST.NONE
                        && InGameManager.a.EmptySeatCount > 0;

        btn_auto_buyin.SetActive(active);
    }

    public void SetText_Stake(long sb, long bb, long ante = 0)
    {
        if (GameManager.a.isTourney)
        {
            text_stake.text = $"블라인드\n{Util.GetMoneyString(sb, true)} / {Util.GetMoneyString(bb, true)} ({ante})";
        }
        else
        {
            text_stake.text = $"스테이크\n{Util.GetMoneyString(sb)} / {Util.GetMoneyString(bb)}";
        }

        UpdateTableImage();
    }

    public void SelectActive(int i, bool active) => userSelectList[i].Select = active;
    public void ReservActive(int i, bool active) => userSelectList[i].Reserv = active;
    public void ReservActiveAll(bool active)
    {
        foreach (var v in userSelectList)
        {
            v.Reserv = active;
        }
    }

    public void SelectActiveAll(bool active)
    {
        foreach (var v in userSelectList)
        {
            v.Select = active;
        }
    }

    public IEnumerator MoveChip(Vector3 from, Vector3 to, long value, Action callback)
    {
        float duration = 0.5f;

        int objIndex = 0;

        if (value > (GameData.TableOption.n64BBlind * 10))
            objIndex = 2;
        else if (value > (GameData.TableOption.n64BBlind * 3))
            objIndex = 1;

        Transform obj = Instantiate(moveChip[objIndex], moveChipContainer);
        obj.transform.position = from;

        obj.SetActive(true);
        obj.gameObject.AddTween(new TweenMove(to, duration, Tween.easeOutCubic).EndEvent(() =>
        {
            Destroy(obj.gameObject);
            callback();
        }));

        yield return new WaitForSeconds(duration);
    }

    public IEnumerator MoveCard(Transform trf, Action callback)
    {
        float duration = 0.3f;

        Transform obj = Instantiate(moveCard, moveCardContainer);

        obj.SetActive(true);
        obj.gameObject.AddTween(new TweenMove(trf.position, duration, Tween.easeOutCubic).EndEvent(() => Destroy(obj.gameObject)));
        obj.gameObject.AddTween(new TweenRotate(0, 0, trf.rotation.z * (180 / Mathf.PI), duration, Tween.easeOutCubic));
        obj.gameObject.AddTween(new TweenScale(
            new Vector3(trf.lossyScale.x / InGameManager.a.mainCanvas.transform.localScale.x
                        , trf.lossyScale.y / InGameManager.a.mainCanvas.transform.localScale.y, trf.lossyScale.z)
                        , duration, Tween.easeOutCubic).EndEvent(() => callback()));

        yield return new WaitForSeconds(duration);
    }

    public void OpenCardImmediately(int index, byte card)
    {
        Image target = img_cards[index];
        Util.SetCard(target, card);
        target.SetActive(true);
    }

    public IEnumerator OpenCard(int index, byte card, bool bShowDown = false)
    {
        if (index >= 4)
        {
            Util.SetCard(obj_lastCard.front, card);
            Util.SetCard(obj_lastCard.back, 127);
            obj_lastCard.parent.SetActive(true);
            obj_lastCard.ani.SetFloat("Speed", bShowDown ? 0.333f : 1f);
            yield return new WaitForSeconds(bShowDown ? 2.1f : .7f);
            obj_lastCard.parent.SetActive(false);
            Image target = img_cards[index];
            Util.SetCard(target, card);
            target.SetActive(true);
            Sound.PlayEffect(Const.SOUND_CARD_RIVER);
        }
        else
        {
            float duration = 0.2f;

            Image target = img_cards[index];
            target.sprite = ResourceManager.LoadCardBack<Sprite>(false);
            target.SetActive(true);

            var vec = Quaternion.LookRotation(GameManager.a.mainCamera.transform.position - target.transform.position).eulerAngles;
            float gap = (180 - vec.y);

            target.gameObject.AddTween(new TweenRotateTo(0, 90 - gap, 0, duration / 2)
                .EndEvent((Action)(() =>
                {
                    target.transform.localRotation = Quaternion.AngleAxis(270 - gap, Vector3.up);
                    Sound.PlayEffect((string)(index < 3 ? Const.SOUND_CARD_FLOP : Const.SOUND_CARD_TURN));
                    Util.SetCard(target, card);
                }))
            .Next(new TweenRotateTo(0, 360, 0, duration / 2)));

            yield return new WaitForSeconds(.6f);
        }
    }

    public void OnClickAutoBuyin()
    {
        int emptyIndex = -1;

        for (int i = 0; i < userSelectList.Count; i++)
        {
            if (userSelectList[i].Select)
            {
                emptyIndex = i;
                break;
            }
        }

        if (emptyIndex < 0)
        {
            Debug.LogError($"There are no empty seats.");
            return;
        }

        btn_auto_buyin.SetActive(false);
        InGameManager.a.OnClickSelect(emptyIndex);
    }

    public void UpdateMyRank()
    {
        int rank = GameData.FindMyRank();

        text_rank.text = string.Format("{0}/{1}", rank == 0 ? "-" : rank.ToString(),
            GameData.TournamentEntrantList.Count(x => x.n64Chips > 0));
    }

    public void UpdateFinalTable()
    {
        finalTable = true;
    }

    bool finalTable = false;

    public void UpdateTableImage()
    {
        obj_table.sprite = img_table[GameManager.a.State == GameManager.GAME_STATE.CASH_GAME ? 0 : finalTable ? 2 : 1];
        obj_frame.sprite = img_frame[GameData.TableOption.n64BBlind >= 100000 ? 1 : 0];
        obj_bg.sprite = img_bg[GameManager.a.State == GameManager.GAME_STATE.CASH_GAME ? 0 : finalTable ? 2 : 1];
        obj_bg.type = GameManager.a.State == GameManager.GAME_STATE.CASH_GAME ? Image.Type.Simple : Image.Type.Sliced;
        obj_frame.SetActive(GameManager.a.State == GameManager.GAME_STATE.CASH_GAME);
    }

    public void ShowWinRank(bool active, string rank)
    {
        obj_win_rank.SetActive(active);
        text_win_rank.text = rank;
    }

    Coroutine timer = null;

    public void UpdateTimer(DateTime nextTime)
    {
        if (timer != null)
        {
            StopCoroutine(timer);
            timer = null;
        }

        timer = StartCoroutine(UpdateTime(nextTime));
    }

    IEnumerator UpdateTime(DateTime nextTime)
    {
        while (nextTime > DateTime.Now)
        {
            TimeSpan time = TimeSpan.FromTicks(nextTime.Ticks - DateTime.Now.Ticks);
            text_next_blind.text = time.ToString(@"mm\:ss");
            yield return new WaitForSecondsRealtime(1f);
        }

        text_next_blind.text = $"--:--";
    }
}
