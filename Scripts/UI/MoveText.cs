using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveText : MonoBehaviour
{
    public RectTransform obj_move;
    public Text[] obj_texts;

    bool isInit = false;

    public void Init(string value)
    {
        if (isInit)
        {
            return;
        }

        obj_texts = obj_move.GetComponentsInChildren<Text>();
        float min = transform.GetComponent<RectTransform>().rect.width;

        foreach (var v in obj_texts)
        {
            v.rectTransform.sizeDelta = new Vector2(Math.Max(min, value.Length * 20f), v.rectTransform.rect.height);
        }

        Rect rect = obj_texts[0].rectTransform.rect;
        obj_move.sizeDelta = new Vector2((rect.width) * 2, rect.height);

        obj_move.anchoredPosition = Vector3.zero;
        float length = (obj_move.rect.width + obj_move.anchoredPosition.x) / 2;
        float duration = length / 48f;

        obj_move.gameObject.AddTween(new TweenMoveTo((length * -1), 0, 0, duration).Repeat(-1));

        foreach (var v in obj_texts)
            v.text = value;

        isInit = true;
    }
}
