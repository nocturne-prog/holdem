using UnityEngine;
using UnityEngine.UI;

public class Item_Shop_Cash : MonoBehaviour
{
    [SerializeField] private Image img_frame;
    [SerializeField] private Text text_price;
    [SerializeField] private Button button_buy;

    string payload = string.Empty;

    public void UpdateData(MS_ShopCashProduct data, int index)
    {
        text_price.text = $"₩ {data.nPrice:N0}";
        img_frame.sprite = ResourceManager.Load_Shop_CashFrame(index + 1);
        //Debug.LogError($"Product ID: {data.nProductId}, RewardCash: {data.nRewardCash}, Product Code: {data.szProductCode}");

        button_buy.SetButton(() =>
        {
            NetworkManager.a.SendShopIAPInfo(data.nProductId, res =>
            {
                if (string.IsNullOrEmpty(res.szPayload))
                {
                    Debug.LogError($"{res.nProductId}: payload is null");
                    return;
                }

                payload = res.szPayload;

                Debug.LogError($"szProductCode: {data.szProductCode}, payload: {payload}");
                IAPManager.a.Purchase(data.szProductCode, payload);
            });
        });
    }
}
