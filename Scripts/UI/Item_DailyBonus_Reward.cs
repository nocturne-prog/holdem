using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item_DailyBonus_Reward : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Transform today;
    [SerializeField] private Transform complete;
    [SerializeField] private Transform blur;

    public bool Today
    {
        set
        {
            today.SetActive(value);
            button.interactable = value;
        }
    }

    public bool Complete
    {
        set
        {
            complete.SetActive(value);
        }
    }

    public bool Disable
    {
        set
        {
            blur.SetActive(value);
        }
    }

    private void Start()
    {
        button.SetButton(() => OnClickButton());
    }

    public void OnClickButton()
    {
        NetworkManager.a.OnClickDailyBonus((res) =>
        {
            if (res.bErrorCode == LMSC_UserAttendanceRewardRes.STATE.ER_NO)
            {
                CompleteAni();
            }
            else
            {
                UIManager.a.OpenPopup<UIPopup_OneButton>(UIManager.POPUP_TYPE.NORMAL, Const.DAILY_BONUS_ERROR_MSG[res.bErrorCode]);
            }
        });
    }

    public void CompleteAni()
    {
        Today = false;
        Disable = false;

        complete.gameObject.transform.localScale = Vector3.one * 3f;
        Complete = true;
        complete.gameObject.AddTween(new TweenScale(Vector3.one, 0.15f));
    }
}
