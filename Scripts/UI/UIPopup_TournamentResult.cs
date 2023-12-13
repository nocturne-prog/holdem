using System;
using UnityEngine;
using UnityEngine.UI;

public class UIPopup_TournamentResult : UIPopup
{
    public Transform trf_champ;
    public Transform trf_win;
    public Transform trf_end;
    public Transform trf_prize;
    public Transform trf_prize_ticket;

    public Text text_rank;
    public Text text_prize;
    public Text text_prize_ticket;

    public Action callback;

    public override void Init(params object[] args)
    {
        base.Init(args);

        MPSC_PrizeInfo2 info = (MPSC_PrizeInfo2)args[0];

        bool isChamp = info.wRank == 1;
        bool isEnd = !isChamp & info.bTicketType == 0;
        bool isWin = !isChamp & !isEnd;

        text_rank.text = $"{info.wRank} 위";
        trf_champ.SetActive(isChamp);
        trf_win.SetActive(isWin);
        trf_end.SetActive(isEnd);
        trf_prize.SetActive(false);
        trf_prize_ticket.SetActive(false);

        //trf_prize.SetActive(info.bTicketType != 2);
        //trf_prize_ticket.SetActive(info.bTicketType == 2);

        if (info.bTicketType == 2)
        {
            text_prize_ticket.SetText(info.wssTicketName);
            trf_prize_ticket.SetActive(true);
        }
        else
        {
            text_prize.SetText(Util.GetMoneyString(info.n64Prize));
            trf_prize.SetActive(isChamp || isWin);
        }
    }

    public override void Close()
    {
        callback?.Invoke();

        base.Close();
    }
}
