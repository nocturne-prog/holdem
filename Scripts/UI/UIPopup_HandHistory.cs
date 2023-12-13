using UnityEngine;
using UnityEngine.UI;

public class UIPopup_HandHistory : UIPopup
{
    [Header("Top")]
    public Text text_title;
    public Button button_left;
    public Button button_right;

    [Header("Bottom")]
    public Transform trf_dotParent;
    public Text text_roomNumber;
    public Text text_stake;

    public HoleCard obj_holeCard;
    public TotalPot obj_totalPot;
    public Board obj_board;
    public User[] obj_user;

    [System.Serializable]
    public struct HoleCard
    {
        public GameObject parent;
        public Image image_card1;
        public Image image_card2;

        public bool Active
        {
            set
            {
                image_card1.SetActive(value);
                image_card2.SetActive(value);
            }
        }
    }

    [System.Serializable]
    public struct TotalPot
    {
        public GameObject parent;
        public Text text_totalPot;
    }

    [System.Serializable]
    public struct Board
    {
        public GameObject parent;
        public Image[] images;

        public bool Active
        {
            set
            {
                foreach (var v in images)
                {
                    v.gameObject.SetActive(value);
                }
            }
        }
    }

    [System.Serializable]
    public struct User
    {
        public GameObject parent;
        public Text text_userName;
        public Image image_card1;
        public Image image_card2;
        public Text text_rank;
        public Text text_win;
    }

    Transform obj_dotOn;
    int pageIndex = 0;
    HandHistoryData data;
    bool init = false;

    public override void Init(params object[] args)
    {
        data = (HandHistoryData)args[0];

        if (data.Count > 0)
        {
            Transform on = trf_dotParent.GetChild(0);
            Transform off = trf_dotParent.GetChild(1);

            obj_dotOn = on;
            obj_dotOn.SetActive(true);

            for (int i = 0; i < data.Count - 1; i++)
            {
                Instantiate(off, trf_dotParent).SetActive(true);
            }

            Destroy(off.gameObject);

            UpdateData(data[pageIndex]);

            button_left.SetButton(() => SetPage(false));
            button_right.SetButton(() => SetPage(true));

            init = true;
        }

        base.Init(args);
    }

    public void SetPage(bool right)
    {
        if (!init)
            return;

        if (right)
        {
            if (pageIndex >= trf_dotParent.childCount - 1)
                return;

            pageIndex++;
        }
        else
        {
            if (pageIndex == 0)
                return;

            pageIndex--;
        }

        obj_dotOn.SetSiblingIndex(pageIndex);
        UpdateData(data[pageIndex]);
    }

    public override void Close()
    {
        InGameManager.a.buttonController.btn_history.interactable = true;
        base.Close();
    }

    public void UpdateData(HandHistoryData.HistoryData data)
    {
        text_title.text = $"핸드 : {data.handNumber}";
        text_roomNumber.text = $"#{data.roomNumber}";
        text_stake.text = $"{Util.GetMoneyString(data.sb, GameManager.a.isTourney)} / {Util.GetMoneyString(data.bb, GameManager.a.isTourney)}";

        obj_holeCard.Active = data.myCard != null;

        if (data.myCard != null)
        {
            //Debug.LogError($"{data.myCard[0]} // {data.myCard[1]}");

            Util.SetCard(obj_holeCard.image_card1, data.myCard[0], true);
            Util.SetCard(obj_holeCard.image_card2, data.myCard[1], true);
        }

        obj_totalPot.text_totalPot.text = $"{Util.GetMoneyString(data.totalPot, GameManager.a.isTourney)}";

        if (data.communityCard == null)
        {
            obj_board.Active = false;
        }
        else
        {
            if (data.communityCard.Count == 0 || data.communityCard[0] == 0)
            {
                obj_board.Active = false;
            }
            else
            {
                for (int i = 0; i < data.communityCard.Count; i++)
                {
                    Util.SetCard(obj_board.images[i], data.communityCard[i], true);
                    obj_board.images[i].SetActive(true);
                }

                for (int i = data.communityCard.Count; i < obj_board.images.Length; i++)
                {
                    obj_board.images[i].SetActive(false);
                }
            }
        }

        for (int i = 0; i < obj_user.Length; i++)
        {
            if (!data.userHistroy.ContainsKey(i))
            {
                obj_user[i].parent.SetActive(false);
                continue;
            }

            User u = obj_user[i];
            HandHistoryData.UserHandHistory h = data.userHistroy[i];

            if (h.card == null)
            {
                obj_user[i].parent.SetActive(false);
                continue;
            }

            u.text_userName.text = h.name;

            if (h.show)
            {
                Util.SetCard(u.image_card1, h.card[0], true);
                Util.SetCard(u.image_card2, h.card[1], true);

                if (h.fold)
                {
                    u.text_rank.text = string.Format("<color=#ffffff>Fold</color> <color=#ffee2c>{0}</color>"
                        , Util.GetRankString(h.card_data, h.card, data.communityCard.ToArray()));
                }
                else
                {
                    u.text_rank.text = string.Format("<color=#ffee2c>{0}</color>"
                        , Util.GetRankString(h.card_data, h.card, data.communityCard.ToArray()));
                }
            }
            else
            {
                Util.SetCard(u.image_card1, 0, true);
                Util.SetCard(u.image_card2, 0, true);

                if (h.win == 0)
                {
                    u.text_rank.text = "<color=#ffffff>Fold</color>";
                }
                else
                {
                    u.text_rank.text = "<color=#ffee2c>Win</color>";

                }
            }

            u.text_win.text = h.win == 0 ? "-" : Util.GetMoneyString(h.win, GameManager.a.isTourney);
            obj_user[i].parent.SetActive(true);
        }
    }
}
