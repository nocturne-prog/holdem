using OneP.InfinityScrollView;
using UnityEngine.UI;

public class Item_InGameTournamentInfo_Rank : InfinityBaseItem
{
    public Text text_rank;
    public Text text_player;
    public Text text_stack;

    string Rank
    {
        set { text_rank.text = value; }
    }

    string Player
    {
        set { text_player.text = value; }
    }

    //long Stack
    //{
    //    set { text_stack.text = $"{value:N0}"; }
    //}

    public override void Reload(InfinityScrollView infinity, int _index)
    {
        base.Reload(infinity, _index);
        UpdateData(_index);
    }

    void UpdateData(int idx_)
    {
        MS_T_EntrantInfo data = GameData.TournamentEntrantList[idx_];

        if (data.wRank == 0)
        {
            if (data.n64Chips == 0)
                Rank = "-";
            else
                Rank = $"{idx_ + 1}";
        }
        else
        {
            Rank = data.wRank.ToString();
        }

        Player = data.nUserEntryNo > 1 ?
            $"{data.szNickName.Replace("\0", "")} ({data.nUserEntryNo - 1})" :
            $"{data.szNickName}";
        text_stack.text = data.n64Prize > 0 ? Util.GetMoneyString(data.n64Prize) : data.n64Chips.ToString("N0");
    }
}