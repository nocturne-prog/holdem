using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    public enum POPUP_TYPE
    {
        NORMAL = 0,
        SYSTEM = 1
    }

    public Canvas canvas { get; private set; }
    private Button btn_Blur;

    public bool Blur
    {
        get { return btn_Blur.gameObject.activeInHierarchy; }
        set
        {
            if (value)
            {
                btn_Blur.SetActive(true);
                btn_Blur.transform.parent = trf_popup;
                btn_Blur.transform.SetSiblingIndex(popupList.Count - 1);
            }
            else
            {
                if (popupList.Count == 0)
                {
                    btn_Blur.SetActive(false);
                }
                else
                {
                    btn_Blur.transform.SetSiblingIndex(popupList.Count - 1);
                }
            }
        }
    }

    private Transform trf_system;
    private Transform trf_popup;
    private List<UIPopup> popupList = new List<UIPopup>();

    protected override void Initialize()
    {
        base.Initialize();

        canvas = Instantiate(ResourceManager.Load<Canvas>(Const.CANVAS), this.transform);
        canvas.worldCamera = GameManager.a.mainCamera;
        canvas.sortingLayerName = "Popup";

        btn_Blur = canvas.transform.Find("Blur").gameObject.GetComponent<Button>();
        btn_Blur.SetButton(() => OnClickBlur());
        trf_system = canvas.transform.Find("Contents/System").transform;
        trf_system.SetAsLastSibling();
        trf_popup = canvas.transform.Find("Contents/Popup").transform;


        GameManager.a.InitCanvas(canvas);
    }

    public T OpenPopup<T>(POPUP_TYPE type, params object[] args) where T : UIPopup
    {
        string path = typeof(T).ToString();

        T obj = Instantiate<T>(ResourceManager.Load<T>($"{Const.UI_POPUP}/{path}"), type == POPUP_TYPE.NORMAL ? trf_popup : trf_system);

        if (obj == null)
        {
            Debug.LogError($"popup load fail.. {path}");
            return default;
        }

        if (obj is SingletonePopup)
        {
            for (int i = 0; i < popupList.Count; i++)
            {
                if (popupList[i] is SingletonePopup)
                {
                    popupList[i].Hide();
                }
            }
        }

        obj.transform.SetAsLastSibling();
        obj.Init(args);
        popupList.Add(obj);

        var loading = FindPopup<UIPopup_Loading>();

        if (loading != null)
            loading.transform.SetAsLastSibling();

        Blur = true;
        return obj;
    }

    public T FindPopup<T>() where T : UIPopup
    {
        foreach (var v in popupList)
        {
            if (v.GetType().Equals(typeof(T)))
                return (T)v;
        }

        return default;
    }

    public void CloseAll()
    {
        if (popupList.Count == 0)
        {
            Blur = false;
            return;
        }

        List<UIPopup> removeList = new List<UIPopup>(popupList);

        foreach (var p in removeList)
        {
            if (p is UIPopup_Loading)
                continue;

            popupList.FirstOrDefault(x => x = p)?.Close();
        }
    }

    public void ClosePopup<T>() where T : UIPopup
    {
        foreach (UIPopup popup in Enumerable.Reverse(popupList))
        {
            if (popup is T)
            {
                ClosePopup(popup);
                break;
            }
        }
    }

    public void ClosePopup(UIPopup obj)
    {
        if (obj == null)
            return;

        popupList.Remove(obj);
        Destroy(obj.gameObject);

        foreach (UIPopup popup in Enumerable.Reverse(popupList))
        {
            if (popup is SingletonePopup)
            {
                popup.Show();
                break;
            }
        }

        Blur = false;
    }

    public void OpenLoading(bool networkLoading = false)
    {
        var popup = FindPopup<UIPopup_Loading>();

        if (popup == null)
            OpenPopup<UIPopup_Loading>(POPUP_TYPE.NORMAL, networkLoading);
    }

    public void CloseLoading()
    {
        FindPopup<UIPopup_Loading>()?.Close();
    }

    public void OnClickBlur()
    {
        if (Blur == false)
            return;

        UIPopup popup = popupList[popupList.Count - 1];

        if (popup.isBlurOff == false)
            return;

        popup.button_Close.onClick.Invoke();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (FindPopup<UIPopup_Error>() == true)
                return;

            if (GameManager.a.State == GameManager.GAME_STATE.CASH_GAME ||
                GameManager.a.State == GameManager.GAME_STATE.TOURNAMENT_GAME)
            {
                var popup = OpenPopup<UIPopup_TwoButton>(POPUP_TYPE.NORMAL, Const.TEXT_EXIT_LOBBY);
                popup.ok_callback += () => GameManager.a.ExitLobby();
            }
            else
            {
                var popup = OpenPopup<UIPopup_Error>(POPUP_TYPE.SYSTEM, "종료하시겠습니까?");
                popup.callbackOk += Application.Quit;
            }
        }

        //if (Input.GetKeyDown(KeyCode.Alpha1))
        //{
        //    OpenLoading();
        //}

        //if (Input.GetKeyDown(KeyCode.Alpha2))
        //{
        //    CloseLoading();
        //}

        //if (Input.GetKeyDown(KeyCode.Alpha3))
        //{
        //    OpenPopup<UIPopup_OneButton>(UIManager.POPUP_TYPE.NORMAL, "토너먼트가 곧 시작됩니다.\n플레이중인 테이블을 종료해 주세요.");
        //}

        //if (Input.GetKeyDown(KeyCode.Alpha1))
        //{
        //    OpenPopup<UIPopup_ChangeBlind>(POPUP_TYPE.NORMAL, new MPSC_LevelInfo() { bLevel = 100, n64SBlind = 10, n64BBlind = 20, n64Ante = 5 });
        //}

        //if (Input.GetKeyDown(KeyCode.Alpha2))
        //{
        //    OpenPopup<UIPopup_RestInfo>(POPUP_TYPE.NORMAL, "REST_INFO", (uint)5);
        //}

        //if (Input.GetKeyDown(KeyCode.Alpha3))
        //{
        //    OpenPopup<UIPopup_RestInfo>(POPUP_TYPE.NORMAL, "REST_INFO", (uint)10);
        //}

        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    OpenPopup<UIPopup_Update>(POPUP_TYPE.NORMAL);
        //    OpenPopup<UIPopup_Error>(POPUP_TYPE.NORMAL, "Error");
        //    //OpenPopup<UIPopup_ChangeBlind>(POPUP_TYPE.NORMAL, new MPSC_LevelInfo() { bLevel = 100, n64SBlind = 10, n64BBlind = 20, n64Ante = 5 });
        //    //OpenPopup<UIPopup_HandForHand>(POPUP_TYPE.NORMAL);
        //    OpenPopup<UIPopup_Notice>(POPUP_TYPE.NORMAL);
        //    OpenPopup<UIPopup_OneButton>(POPUP_TYPE.NORMAL, "ONE_BUTTON");
        //    OpenPopup<UIPopup_TwoButton>(POPUP_TYPE.NORMAL, "TWO_BUTTON");
        //    OpenPopup<UIPopup_RestInfo>(POPUP_TYPE.NORMAL, "REST_INFO", (uint)10);
        //}

    }
}
