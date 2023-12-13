using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserMarking : MonoBehaviour
{
    private Button btn_blur;
    [SerializeField] private Transform popup;
    private Toggle[] toggles;
    public Color[] setColor;
    private UserIcon selectUserIcon;

    bool Active
    {
        get
        {
            return gameObject.activeInHierarchy;
        }
        set
        {
            gameObject.SetActive(value);
        }
    }

    public void Awake()
    {
        btn_blur = this.GetComponent<Button>();
        btn_blur.SetButton(() => Active = false);

        toggles = popup.GetComponentsInChildren<Toggle>();

        for (int i = 0; i < toggles.Length; i++)
        {
            int index = i;

            toggles[index].onValueChanged.AddListener(b =>
            {
                if (b)
                {
                    Send((byte)index);
                    Active = false;
                }
            });
        }

        Active = false;
    }

    public void Show(UserIcon user)
    {
        popup.transform.position = user.transform.position;
        selectUserIcon = user;
        Active = true;
    }

    public void Send(byte value)
    {
        if (selectUserIcon == null)
            return;

        NetworkManager.a.SendSetColorTag(selectUserIcon.Data.nUserNo, value, (res) =>
        {
            GameData.UserColorTag[res.rColorTag.nUserNo] = res.rColorTag.bColor;
            selectUserIcon.UpdateColorTag();
        });
    }
}
