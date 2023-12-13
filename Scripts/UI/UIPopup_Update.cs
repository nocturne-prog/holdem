using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopup_Update : UIPopup
{
    public override void Init(params object[] args)
    {
        button_Close.SetButton(() =>
        {
            Application.OpenURL("*****");
        });
    }
}
