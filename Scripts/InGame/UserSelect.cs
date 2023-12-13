using System;
using UnityEngine;
using UnityEngine.UI;

public class UserSelect : MonoBehaviour
{
    [SerializeField] private Button btn_select;
    [SerializeField] private Transform trf_reserv;

    public bool Select
    {
        get
        {
            return btn_select.gameObject.activeInHierarchy;
        }

        set
        {
            btn_select.SetActive(value);
        }
    }

    public bool Reserv
    {
        set
        {
            trf_reserv.SetActive(value);
        }
    }


    public void Init(int index, Action<int> callback)
    {
        btn_select.SetButton(() =>
        {
            callback(index);
        });
    }
}
