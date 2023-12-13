using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    public Slider slider;

    public void SetPercent(float f)
    {
        f = Mathf.Clamp(f, 0, 1);

        slider.value = f;
    }
}
