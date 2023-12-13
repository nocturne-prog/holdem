using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardAnimator : MonoBehaviour
{
    public void Start()
    {
        
    }

    public static CardAnimator Create(Transform parent, string path)
    {
        /// 생성 size 0;
        /// tween ( size 0 - target size)
        /// tween ( alpha0 - alpha 1)
        /// tween ( roate 0 0 0 - target rotate)
        /// 완료 후 callback
        /// 
        CardAnimator ani = Instantiate(ResourceManager.Load<CardAnimator>(path), parent);

        //Tween.easeOutCubic(

        ani.gameObject.AddTween(new TweenMoveTo(Vector3.zero, 0));

        return ani;
    }

    public void AddTween()
    {
        gameObject.AddTween(new TweenRotate(0, 0, 180, 2f));
        gameObject.AddTween(new TweenAlpha(0, 1, 2));
        gameObject.AddTween(new TweenScale(2 * Vector3.one, 2));
        gameObject.AddTween(new TweenMoveLocal(0, 0, 0, 2));
    }
}
