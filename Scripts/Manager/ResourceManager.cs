using UnityEngine;
using System.Collections.Generic;

public static class ResourceManager
{
    public static T Load<T>(string path)
    {
        GameObject obj = Resources.Load<GameObject>(path);

        if (obj == null)
        {
            Debug.LogError($"[{path} ({typeof(T)})] Load fail!!");
            return default;
        }
        else
        {
            //Debug.Log($"[{path} ({typeof(T)})] Load success");
            return obj.GetComponent<T>();
        }
    }

    public static Sprite LoadEmoji(int index)
    {
        if (index < 1)
            return null;

        return Resources.Load<Sprite>($"{Const.EMOJI_PATH}/{index:00}");
    }

    public static Sprite LoadButtonImage(bool isRed)
    {
        return Resources.Load<Sprite>(isRed ? Const.BUTTON_RED : Const.BUTTON_GRAY);
    }

    public static Sprite LoadAvatar(string name)
    {
        return Resources.Load<Sprite>($"{Const.AVATAR}/{name}");
    }

    static string[] cardSuit = new string[] { "c", "h", "d", "s" };

    public static T LoadCard<T>(int suit, int number, bool small) where T : Object
    {
        string numString = "";

        if (number > 9)
        {
            if (number == 10) numString = "j";
            if (number == 11) numString = "q";
            if (number == 12) numString = "k";
        }
        else
        {
            numString = $"{number + 1}";
        }

        if (small)
        {
            return Resources.Load<T>($"{Const.CARD_SMALL}/{cardSuit[suit]}_{numString}");
        }
        else
        {
            string cardPath = Option.ColorCard ? Const.COLOR_CARD : Const.CARD;
            return Resources.Load<T>($"{cardPath}/{cardSuit[suit]}_{numString}");
        }
    }

    public static T LoadCardBack<T>(bool small) where T : Object
    {
        if (small)
        {
            return Resources.Load<T>($"{Const.CARD_SAMLL_BACK}");
        }
        else
        {
            string path = Option.ColorCard ? Const.COLOR_CARD_BACK : Const.CARD_BACK;
            return Resources.Load<T>($"{path}");
        }
    }

    public static Texture LoadMyCardBack()
    {
        return Resources.Load<Texture>(Const.CARD_MY_BACK);
    }

    public static Sprite Load_Shop_AvatarFrame(int index)
    {
        return Resources.Load<Sprite>(string.Format("{0}{1}", Const.SHOP_AVATAR_FRAME, index < 0 ? "00" : index.ToString()));
    }

    public static Sprite Load_Shop_CashFrame(int index)
    {
        return Resources.Load<Sprite>(string.Format("{0}{1}", Const.SHOP_CASH_FRAME, index.ToString("00")));
    }
}
