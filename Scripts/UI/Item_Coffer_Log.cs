using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item_Coffer_Log : MonoBehaviour
{
    [SerializeField] private Text text_date;
    [SerializeField] private GameObject obj_deposit;
    [SerializeField] private GameObject obj_withDraw;
    [SerializeField] private Text text_account;

    public void UpdateData(MS_UserCofferLog data)
    {
        //text_endTime.text = info.tRemainTime == 0 ? "-" : Util.GetDate(info.tRemainTime).ToString("yyyy'/'MM'/'dd HH:mm");
        text_date.text = Util.GetDate(data.tLogTime).ToString("yyyy'/'MM'/'dd HH:mm");
        obj_deposit.SetActive(data.bDeposit == 1);
        obj_withDraw.SetActive(data.bDeposit == 0);
        text_account.text = Util.GetMoneyString(data.n64Amount);
    }
}
