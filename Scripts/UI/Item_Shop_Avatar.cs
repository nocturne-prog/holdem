using System;
using UnityEngine;
using UnityEngine.UI;

public class Item_Shop_Avatar : MonoBehaviour
{
    [SerializeField] private Image img_frame;
    [SerializeField] private GameObject trf_sale;
    [SerializeField] private Text text_discountPercent;
    [SerializeField] private Text text_discountBeforPrice;
    [SerializeField] private GameObject trf_time;
    [SerializeField] private Text text_time;
    [SerializeField] private Image img_avatar;
    [SerializeField] private Text text_avatarName;
    [SerializeField] private Text text_reward;
    [SerializeField] private Button button_buy;
    [SerializeField] private Text text_price;

    MS_ShopAvatarProduct data;

    public void UpdateData(MS_ShopAvatarProduct d, bool frame)
    {
        data = d;

        img_frame.sprite = GetFrameSprite(data.bSalesType, frame);
        img_avatar.sprite = ResourceManager.LoadAvatar($"{data.nAvatarSeq}");

        text_avatarName.SetActive(data.bSalesType != MD_SHOP_SALES_TYPE.MD_SHOP_SALES_TYPE_EVENT_VIP);
        text_avatarName.text = data.wszAvatarName;

        text_reward.SetActive(data.n64RewardMoney > 0);
        text_reward.text = $"{Util.GetMoneyString(data.n64RewardMoney)}";

        button_buy.SetActive(data.bSalesType != MD_SHOP_SALES_TYPE.MD_SHOP_SALES_TYPE_EVENT_VIP);
        text_price.text = $"{data.nPrice:N0}";

        trf_sale.SetActive(data.nDiscountBeforePrice > 0);
        text_discountPercent.text = $"{data.bDiscountPercent}% 할인";
        text_discountBeforPrice.text = $"{data.nDiscountBeforePrice:N0}";

        trf_time.SetActive(data.tEventStart > 0);
        text_time.text = $"판매기간 : {Util.GetDate(data.tEventStart).ToString("M")} ~ {Util.GetDate(data.tEventEnd).ToString("M")}";

        button_buy.SetActive(data.nPrice > 0);

        button_buy.SetButton(() =>
        {
            OnClickBuy();
        });
    }

    private Sprite GetFrameSprite(MD_SHOP_SALES_TYPE type, bool f)
    {
        switch (type)
        {
            case MD_SHOP_SALES_TYPE.MD_SHOP_SALES_TYPE_EVENT_VIP:
                return ResourceManager.Load_Shop_AvatarFrame(-1);
            case MD_SHOP_SALES_TYPE.MD_SHOP_SALES_TYPE_EVENT_PERIOD:
                return ResourceManager.Load_Shop_AvatarFrame(0);

            default:
                return ResourceManager.Load_Shop_AvatarFrame(f ? 2 : 1);
        }
    }

    public void OnClickBuy()
    {
        UIPopup_TwoButton popup = UIManager.a.OpenPopup<UIPopup_TwoButton>(UIManager.POPUP_TYPE.NORMAL, "해당 아바타와 게임머니로\n교환하시겠습니까?");
        popup.ok_callback += () =>
        {
            NetworkManager.a.SendShopAvatarBuy(data.nAvatarSeq, (res) =>
            {
                UIManager.a.FindPopup<UIPopup_Shop>().text_cash.text = $"{res.n64Cash:N0}";
                UIManager.a.OpenPopup<UIPopup_OneButton>(UIManager.POPUP_TYPE.NORMAL, "구매가 완료되었습니다.");
            });

            popup.Close();
        };
    }
}
