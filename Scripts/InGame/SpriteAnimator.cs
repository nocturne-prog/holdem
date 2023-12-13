using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SpriteAnimator : MonoBehaviour
{
    float duration = 1f;
    float destroyDelay = 1f;
    public Sprite[] list;
    public Image target;

    public static SpriteAnimator Create(Transform parent, string path, float duration = 1f, float destroyDelay = 1f)
    {
        SpriteAnimator ani = Instantiate(ResourceManager.Load<SpriteAnimator>(path), parent);
        ani.transform.localPosition = Vector3.zero;

        ani.duration = duration;
        ani.destroyDelay = destroyDelay;
        return ani;
    }

    public void Start()
    {
        if (target == null)
            target = gameObject.GetComponent<Image>();

        StartCoroutine(AnimationStart());
    }

    public IEnumerator AnimationStart()
    {
        float gap = duration / list.Length;

        for (int i = 0; i < list.Length; i++)
        {
            target.sprite = list[i];

            yield return new WaitForSeconds(gap);
        }

        yield return new WaitForSeconds(destroyDelay);

        Destroy(this.gameObject);
    }
}
