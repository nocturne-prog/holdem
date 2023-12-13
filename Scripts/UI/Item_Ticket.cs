using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item_Ticket : MonoBehaviour
{
    public Text text_index;
    public Image image_ticket;
    public Text text_name;
    public Text text_endTime;
    public Text text_count;

    public void SetData(int i, MS_TicketInfo2 info)
    {
        text_index.text = i.ToString();
        text_name.text = info.wszTicketName;
        text_endTime.text = info.tRemainTime == 0 ? "-" : Util.GetDate(info.tRemainTime).ToString("yyyy'/'MM'/'dd HH:mm");
        text_count.text = info.nTicketCnt.ToString();
    }
}
