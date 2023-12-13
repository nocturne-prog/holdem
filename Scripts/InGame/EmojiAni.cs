using UnityEngine;
using UnityEngine.UI;

public class EmojiAni : MonoBehaviour
{
    public Image image_Emoji;
    public Transform trf_MsgBox;
    public Transform trf_panel;
    public Text text;

    public static EmojiAni Create(Transform parent, int index, bool left = true)
    {
        EmojiAni ani = Instantiate(ResourceManager.Load<EmojiAni>(Const.EMOJI_PREFAB), parent);
        ani.text.text = Const.EmojiData[index];
        ani.image_Emoji.sprite = ResourceManager.LoadEmoji(index);

        if (!left)
        {
            ani.trf_MsgBox.localPosition
                = new Vector3(ani.trf_MsgBox.localPosition.x * -1, ani.trf_MsgBox.localPosition.y, ani.trf_MsgBox.localPosition.z);

            ani.trf_panel.transform.localScale
                = new Vector3(ani.trf_panel.transform.localScale.x * -1, ani.trf_panel.transform.localScale.y, ani.trf_panel.transform.localScale.z);
        }

        ani.transform.localScale = Vector3.zero;
        ani.gameObject.AddTween(new TweenScale(Vector3.one, 0.3f, Tween.spring)
            .Next(new TweenDelay(2f)
            .Next(new TweenScale(Vector3.zero, 0.3f, Tween.easeInBack).EndEvent(() => Destroy(ani.gameObject)))));

        return ani;
    }
}

