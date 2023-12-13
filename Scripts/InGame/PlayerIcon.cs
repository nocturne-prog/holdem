using UnityEngine.UI;

public class PlayerIcon : UserIcon
{
    public Text text_rank;

    public string Rank
    {
        set
        {
            if (text_rank == null)
                return;

            if (!Option.ShowRank)
                return;

            if (Die)
                text_rank.text = "";

            text_rank.text = value;
        }
    }


    public override bool Die
    {
        get => base.Die;
        set
        {
            base.Die = value;
            RankClear();
        }
    }

    public override bool Fold
    {
        set
        {
            if (value)
            {
                Sound.PlayVoice(Const.SOUND_BET_FOLD);
            }

            obj_betTye[1].SetActive(value);
        }
    }

    public override bool MyTurn
    {
        set
        {
            if (value)
            {
                Sound.PlayEffect(Const.SOUND_MY_TURN);
            }

            obj_myTurn.SetActive(value);
        }
    }

    public void Start()
    {
        RankClear();
    }

    void RankClear()
    {
        Rank = "";
    }

    public override void Clear()
    {
        base.Clear();
        RankClear();
    }
}
