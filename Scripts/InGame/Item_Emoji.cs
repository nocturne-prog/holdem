using UnityEngine;
using UnityEngine.UI;

public class Item_Emoji : MonoBehaviour
{
    public Image image;
    public Text text;

    public void SetData(int idx, string txt)
    {
        image.sprite = ResourceManager.LoadEmoji(idx);
        text.text = txt;
    }
}
