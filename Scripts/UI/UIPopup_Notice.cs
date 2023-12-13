using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopup_Notice : UIPopup, SingletonePopup
{
    void Start()
    {
        isBlurOff = false;

        Invoke(nameof(Close), 2f);
    }
}
