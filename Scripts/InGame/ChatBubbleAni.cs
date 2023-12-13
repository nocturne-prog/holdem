using UnityEngine;
using UnityEngine.UI;

public class ChatBubbleAni : MonoBehaviour
{
    public Text text;

    public static ChatBubbleAni Create(Transform parent, string text)
    {
        ChatBubbleAni ani = Instantiate(ResourceManager.Load<ChatBubbleAni>(Const.CHAT_BUBBLE_PREFAB), parent);
        ani.text.text = text;

        ani.transform.localScale = Vector3.zero;
        ani.gameObject.AddTween(new TweenScale(Vector3.one, 0.3f, Tween.spring)
            .Next(new TweenDelay(2f)
            .Next(new TweenScale(Vector3.zero, 0.3f, Tween.easeInBack).EndEvent(() => Destroy(ani.gameObject)))));

        return ani;
    }
}
