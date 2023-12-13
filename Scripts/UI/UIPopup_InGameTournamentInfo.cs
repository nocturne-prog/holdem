using UnityEngine;
using UnityEngine.UI;
using OneP.InfinityScrollView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class UIPopup_InGameTournamentInfo : UIPopup
{
    [System.Serializable]
    public class ToggleTab
    {
        public ToggleEx toggle;
        public Transform[] obj_on;
        public Transform[] obj_off;

        public bool isOn
        {
            get { return toggle.isOn; }
            set
            {
                foreach (var v in obj_on)
                {
                    v.SetActive(value);
                }

                foreach (var v in obj_off)
                {
                    v.SetActive(!value);
                }
            }
        }
    }

    [Header("Common")]
    [SerializeField] Text text_tilte;
    [SerializeField] ToggleGroupEx tabGroup;
    [SerializeField] ToggleTab tab_reward;
    [SerializeField] ToggleTab tab_rank;
    [SerializeField] ToggleTab tab_blind;

    [Header("Reward")]
    [SerializeField] Text text_money;
    [SerializeField] ScrollRect scroll_reward;

    [Header("Rank")]
    [SerializeField] Text text_rank;
    [SerializeField] Text text_stack;
    [SerializeField] InfinityScrollView scroll_rank;

    [Header("Blind")]
    [SerializeField] Text text_timer;
    [SerializeField] ScrollRect scroll_blind;

    public override void Init(params object[] args)
    {
        base.Init(args);

        tabGroup.onValueChanged.AddListener((v) =>
        {
            OnClickTab(v);
        });

        OnClickTab(0);

        text_tilte.SetText(GameData.TournamentSubData.info.szTournmentName);

        RewardInit();
        BlindInit();
        RankInit();
    }

    public void OnClickTab(int idx)
    {
        tab_reward.isOn = idx == 0;
        tab_rank.isOn = idx == 1;
        tab_blind.isOn = idx == 2;
    }

    public void RewardInit()
    {
        var data = GameData.TournamentSubData;

        Item_InGameTournamentInfo prefab = ResourceManager.Load<Item_InGameTournamentInfo>(Const.ITEM_INGAME_TOURNAMENT_REWARD);
        int count = data.prize.table_Lists.Length;

        for (int i = 0; i < count; i++)
        {
            Item_InGameTournamentInfo item = Instantiate(prefab, scroll_reward.content);

            int r0 = data.prize.table_Lists[i].nRank_Start;
            int r1 = data.prize.table_Lists[i].nRank_End;

            if (r0 == r1)
                item.SetText(0, $"{r0}");
            else
                item.SetText(0, $"{r0} - {r1}");

            long prize = data.prize.table_Lists[i].n64Prize;

            item.SetText(1, prize > 0 ? $"{Util.GetMoneyString(prize)}" : data.info.szTicketName);
        }

        if (data.prize.nPrizeTicketCount > 0)
            text_money.text = $"{data.info.szTicketName.Trim('\0')} {data.prize.nPrizeTicketCount}장";
        else
            text_money.text = "";

        if (data.prize.n64PrizeTotal > 0)
        {
            if (data.prize.nPrizeTicketCount > 0)
                text_money.text += " + ";

            text_money.text += Util.GetMoneyString(data.prize.n64PrizeTotal);
        }
    }

    public void RankInit()
    {
        List<MS_T_EntrantInfo> entrantList = GameData.TournamentEntrantList;

        int myRank = GameData.FindMyRank();
        text_rank.text = string.Format("<color=#46dcf7>{0}</color>/{1}", myRank == 0 ? "-" : myRank.ToString(), entrantList.Count(x => x.n64Chips > 0));
        text_stack.text = $"{entrantList.Where(x => x.n64Chips > 0).Average(x => x.n64Chips):N0}";
        scroll_rank.Init(entrantList.Count);
    }

    public void BlindInit()
    {
        var data = GameData.TournamentSubData;

        Item_InGameTournamentInfo prefab = ResourceManager.Load<Item_InGameTournamentInfo>(Const.ITEM_INGAME_TOURNAMENT_BLIND);
        int count = data.blindOption.t_blind_info.dwBigBlind.Length;

        for (int i = 0; i < count; i++)
        {
            int lv = i + 1;
            long blind, ante;

            blind = data.blindOption.t_blind_info.dwBigBlind[i];

            if (i < data.blindOption.t_blind_info.bAnteLevel - 1)
                ante = 0;
            else ante = blind / 10;

            Item_InGameTournamentInfo item = Instantiate(prefab, scroll_blind.content);

            item.SetText(0, lv.ToString());
            item.SetText(1, $"{blind / 2:N0} / {blind:N0}");
            item.SetText(2, ante == 0 ? "-" : $"{ante:N0}");
            item.SetText(3, $"{data.info.t_semi_info.bLevelUpTime}분");
        }

        if (GameData.NextLevelTime > DateTime.Now)
        {
            StartCoroutine(UpdateTime(GameData.NextLevelTime));
        }
        else
        {
            text_timer.text = $"--:--";
        }
    }

    IEnumerator UpdateTime(DateTime nextTime)
    {
        while (nextTime > DateTime.Now)
        {
            TimeSpan time = TimeSpan.FromTicks(nextTime.Ticks - DateTime.Now.Ticks);
            text_timer.text = time.ToString(@"mm\:ss");
            yield return new WaitForSecondsRealtime(1f);
        }

        text_timer.text = $"--:--";
    }
}