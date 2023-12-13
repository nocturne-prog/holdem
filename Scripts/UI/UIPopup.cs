using System;
using UnityEngine;
using UnityEngine.UI;

public class UIPopup : MonoBehaviour
{
    public Button button_Close;
    public bool isBlurOff = true;

    public virtual void Init(params object[] args)
    {
        if (button_Close == null)
            return;

        button_Close.SetButton(() => Close());
    }

    public virtual void Close()
    {
        UIManager.a.ClosePopup(this);
    }

    public virtual void Hide()
    {
        transform.SetActive(false);
    }

    public virtual void Show()
    {
        transform.SetActive(true);
    }
}
