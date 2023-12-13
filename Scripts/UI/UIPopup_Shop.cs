using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

public class UIPopup_Shop : UIPopup
{
    public ToggleGroupEx tabGroup;
    public ScrollRect scroll_cash;
    public ScrollRect scroll_avatar;
    public Text text_cash;

    public override void Init(params object[] args)
    {
        isBlurOff = false;
        base.Init(args);

        LMSC_ShopProductListRes data = (LMSC_ShopProductListRes)args[0];

        tabGroup.onValueChanged.AddListener((i) =>
        {
            scroll_cash.gameObject.SetActive(i == 0);
            scroll_avatar.gameObject.SetActive(i == 1);
        });

        tabGroup.TogglesChangeOn(0);

        text_cash.text = $"{data.n64Cash:N0}";

        InitCash(data.pCashProducts);
        InitAvatar(data.pAvatarProducts);
    }

    public void InitCash(MS_ShopCashProduct[] list)
    {
        for (int i = 0; i < list.Length; i++)
        {
            Item_Shop_Cash item = Instantiate(ResourceManager.Load<Item_Shop_Cash>(Const.SHOP_ITEM_CASH), scroll_cash.content);
            item.UpdateData(list[i], i);
            //item.transform.SetAsFirstSibling();
        }
    }

    public void InitAvatar(MS_ShopAvatarProduct[] list)
    {
        bool blueFrame = true;

        for (int i = 0; i < list.Length; i++)
        {
            Item_Shop_Avatar item = Instantiate(ResourceManager.Load<Item_Shop_Avatar>(Const.SHOP_ITEM_AVATAR), scroll_avatar.content);
            item.UpdateData(list[i], blueFrame);
            //item.transform.SetAsFirstSibling();

            if (list[i].bSalesType == MD_SHOP_SALES_TYPE.MD_SHOP_SALES_TYPE_NORMAL)
                blueFrame = !blueFrame;
        }
    }
}
