using UnityEngine;
using UnityEngine.Purchasing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class IAPManager : Singleton<IAPManager>, IStoreListener
{

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        extensionProvider = extensions;

        Debug.LogError($"OnInitialized :: {controller}, {extensions}");
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"OnInitializeFailed :: {error}");
    }

    public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
    {
        Debug.LogError($"OnPurchaseFailed :: id: {i.definition.id}, reaseon: {p}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        JToken data = JToken.Parse(e.purchasedProduct.receipt);
        JToken payload = JToken.Parse(data["Payload"].ToString());

        string receipt = "";
        string signature = "";

#if UNITY_EDITOR
        receipt = "test";
        signature = "none";
#else
        receipt = payload["json"].ToString();
        signature = payload["signature"].ToString();
#endif

        //Debug.LogError($"receipt :: {receipt}");
        //Debug.LogError($"payload :: {payload}");
        //Debug.LogError($"signature :: {signature}");

        NetworkManager.a.SendShopIAPBuy(receipt, signature, (res) =>
        {
            UIPopup_Shop popup = UIManager.a.FindPopup<UIPopup_Shop>();

            if (popup == null)
                return;

            popup.text_cash.text = $"{res.n64Cash:N0}";

            //Debug.LogError($"reward Cash: {res.nRewardCash}, total Cash: {res.n64Cash}");
        });

        return PurchaseProcessingResult.Complete;
    }

    private IStoreController storeController;
    private IExtensionProvider extensionProvider;

    public void InitProduct(MS_ShopCashProduct[] data)
    {
        ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        for (int i = 0; i < data.Length; i++)
        {
            builder.AddProduct(data[i].szProductCode, ProductType.Consumable);
        }

        UnityPurchasing.Initialize(this, builder);
    }

    public void Purchase(string productId, string developerPayload)
    {
        if (storeController == null || extensionProvider == null)
        {
            UIManager.a.OpenPopup<UIPopup_OneButton>(UIManager.POPUP_TYPE.NORMAL, "구글 결제서버 접속에 실패하였습니다.\n구글 설정을 확인해주세요.");
            return;
        }

        var product = storeController.products.WithID(productId);

        if (product != null && product.availableToPurchase)
        {
            Debug.LogError($"Purchase :: {productId}, Payload: {developerPayload}");
            storeController.InitiatePurchase(product, developerPayload);
        }
    }
}
