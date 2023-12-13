using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class HoldemTableItem : LobbyListItem
{
    public Text gameStake;
    public Text gameBuyIn;
    public Text gameUserCount;
    public Transform type1;
    public Transform type2;
    public Button join;
    public Transform block;
    public GameData.HoldemTable Data { get; private set; }

    public void SetData(GameData.HoldemTable table)
    {
        Data = table;
        RefreshData();
    }

    public void RefreshData()
    {
        gameStake.text = $"{Util.GetMoneyString(Data.option.n64SeedMoney)} / {Util.GetMoneyString(Data.option.n64SeedMoney * 2)}";
        gameBuyIn.text = $"바이인  {Util.GetMoneyString(Data.option.n64SeedMoney * 80)}";
        gameUserCount.text = $"{Data.UserCount}";

        bool isEnablePlay = GameData.Player.AvailMoney > Data.option.n64SeedMoney * 80;

        block.SetActive(!isEnablePlay);
        join.SetActive(isEnablePlay);
        type1.SetActive(Data.info.info_h.bMaxSeat == 6);
        type2.SetActive(Data.info.info_h.bMaxSeat == 9);
        join.SetButton(() =>
        {
            UIManager.a.OpenLoading();
            GameManager.a.EnterHoldem(Data.info, isEnablePlay);
        });
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine().AppendLine()
            .AppendLine($"게임 : NL홀덤")
            .AppendLine($"타입 : {Data.info.info_h.bMaxSeat}인")
            .AppendLine($"스테이크 : {gameStake.text}")
            .AppendLine($"바이인 : {gameBuyIn.text}")
            .AppendLine($"테이블 수 : {Data.infos.Count}")
            .AppendLine($"플레이어 : {Data.UserCount}");

        return sb.ToString();
    }
}
