using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPopup_Ticket : UIPopup
{
    public Transform trf_ticket;
    public Transform trf_tourney;

    public ScrollRect scroll;
    public Item_Ticket prefab_item;

    public override void Init(params object[] args)
    {
        base.Init(args);
        trf_ticket.SetActive(true);
        trf_tourney.SetActive(false);

        prefab_item.gameObject.SetActive(false);

        LMSC_UserTicketListRes2 p = (LMSC_UserTicketListRes2)args[0];
        MS_TicketInfo2[] infos = p.mS_TicketInfos;

        for (int i = 0; i < p.mS_TicketInfos.Length; i++)
        {
            Item_Ticket item = Instantiate(prefab_item, scroll.content);
            item.SetData(i + 1, infos[i]);
            item.gameObject.SetActive(true);
        }

        Rect rect = prefab_item.GetComponent<RectTransform>().rect;
        scroll.content.GetComponent<RectTransform>().sizeDelta = new Vector2(0, rect.height * infos.Length);

    }
}

