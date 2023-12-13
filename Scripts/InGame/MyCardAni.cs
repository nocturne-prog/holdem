using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MyCardAni : MonoBehaviour
{
    public Canvas canvas;
    public Slider slider;
    public Animator ani;
    public SkinnedMeshRenderer mesh;
    AnimationClip clip;

    public IEnumerator StartAni(byte[] card, float duration)
    {
        canvas.worldCamera = GameManager.a.mainCamera;
        canvas.sortingLayerName = "Popup";

        Material card1 = mesh.materials[0];
        Material card2 = mesh.materials[2];
        Material card_back = mesh.materials[1];

        Util.SetCard(card1, card[0]);
        Util.SetCard(card2, card[1]);
        card_back.mainTexture = ResourceManager.LoadMyCardBack();

        clip = ani.GetCurrentAnimatorClipInfo(0)[0].clip;
        slider.onValueChanged.AddListener((v) => UpdateAni(v));

        ani.speed = 0;
        ani.Play(clip.name, 0, 0);

        /// Touch 확인해봐야함. + 카드 교체.
        yield return StartCoroutine(Done(duration));
        yield break;
    }
    public void UpdateAni(float value)
    {
        //float time = value * clip.length;
        //Debug.LogError(time);

        ani.Play(clip.name, 0, value);
    }

    IEnumerator Done(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
