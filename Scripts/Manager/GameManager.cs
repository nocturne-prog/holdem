using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    public Camera mainCamera;

    public enum GAME_STATE
    {
        LOG_IN,
        LOADING,
        LOBBY,
        CASH_GAME,
        TOURNAMENT_GAME,
        EXIT_LOBBY,
        TOURNAMENT_DESTROY,
    }

    public GAME_STATE State { get; set; }
    public bool isTourney => State == GAME_STATE.TOURNAMENT_GAME;

    public bool isWCOP
    {
        get
        {
            if (!isTourney)
                return false;

            var v = GameData.TournamentTableList.FirstOrDefault(x => x.info.TournamentNo == CurrentTournamentNo);

            if (v == null)
                return false;

            return v.t_info.bTournamentType == 1;
        }
    }

    public int CurrentTournamentNo = 0;
    public ushort CurrentTableNo = 0;

    protected override void Initialize()
    {
        base.Initialize();

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    public void InitCanvas(Canvas c)
    {
        var cs = c.GetComponent<CanvasScaler>();

        if (mainCamera.aspect > (18.5f / 9f))
            cs.referenceResolution = new Vector2(1280f * mainCamera.aspect / (18.5f / 9f), 720);
    }

    public void LoginComplete()
    {
        if (LoginManager.a.state == LoginManager.LOGIN_STATE.Success)
        {
            StartCoroutine(LoginToLobby());
        }
        else
        {
        }
    }

    public IEnumerator LoginToLobby()
    {
        LoadingManager.a.Show();
        State = GAME_STATE.LOADING;

        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime;

            LoadingManager.a.SetPercent(t);

            yield return null;
        }

        LoadingManager.a.SetPercent(1);

        SceneManager.LoadScene(SceneManager.LobbyScene, () =>
        {
            // Lobby Init/씬 삭제 완료 후 롤딩 끝!
            SceneManager.UnloadScene(SceneManager.LoginScene);
            LobbyManager.a.Init();

            LoadingManager.a.Hide();
        });

        yield break;
    }

    public void ExitLobby()
    {
        UIManager.a.CloseAll();

        State = GAME_STATE.EXIT_LOBBY;

        if (GameData.TournamentSubData != null)
        {
            NetworkManager.a.SendTournamentView(GameData.TournamentSubData.info.TournamentNo, 0, null);
            GameData.ClearSubscriptionData();
        }

        SceneManager.LoadScene(SceneManager.LobbyScene, () =>
        {
            LobbyManager.a.Init();

            if (InGameManager.a.MySeatState.HasFlag(FPDefine.MD_ST.SITPLAY))
            {
                MPBT_ConnectClose packet = new MPBT_ConnectClose();
                packet.HWord = FPPacket.HPD_CONNECT;
                packet.LWord = FPPacket.MPBT_CONNECT_CLOSE_TAG;

                if (InGameManager.a.Connected)
                {
                    InGameManager.a.socket.Send<MPSC_CloseInfo>(packet, result =>
                    {
                        InGameManager.a.socket.Close();
                        Destroy(InGameManager.a.socket.gameObject);
                        SceneManager.UnloadScene(SceneManager.InGameScene);

                    });
                }
            }
            else
            {
                InGameManager.a.socket.Close();
                Destroy(InGameManager.a.socket.gameObject);
                SceneManager.UnloadScene(SceneManager.InGameScene);
            }
        });
    }

    public void OnClickChangeRoom()
    {
        int targetIndex = -1;

        for (int i = 0; i < GameData.HoldemTableList.Count; i++)
        {
            var v = GameData.HoldemTableList[i].infos.FirstOrDefault(x => x.info_h.wTableNo == CurrentTableNo);

            if (v != null)
            {
                targetIndex = i;
                break;
            }
        }

        if (targetIndex >= 0)
        {
            GameData.HoldemTable target = GameData.HoldemTableList[targetIndex];

            if (target != null)
            {
                UIManager.a.OpenLoading();
                StartCoroutine(ChangeRoom(target));
            }
        }
        else
        {
            Debug.LogError("can't find index !!");
        }
    }

    public IEnumerator ChangeRoom(GameData.HoldemTable target)
    {
        ExitLobby();

        yield return new WaitUntil(() => State == GAME_STATE.LOBBY);

        List<MS_TableInfo> list = new List<MS_TableInfo>(target.infos);
        List<MS_TableInfo> removeList = new List<MS_TableInfo>();

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].info_m.bSeatCount >= list[i].info_h.bMaxSeat)
                removeList.Add(list[i]);
        }

        list.RemoveAll(removeList.Contains);

        MS_TableInfo info = RandomItem(target.infos);
        EnterHoldem(info, true);

    }

    public void OnClickReEntry()
    {
        StartCoroutine(JumpTournament());
    }

    public IEnumerator JumpTournament()
    {
        ExitLobby();

        GameData.TEntranceNotice2 = null;

        yield return new WaitUntil(() => State == GAME_STATE.LOBBY);

        LobbyManager.a.toggle_Tournament.isOn = true;
        LobbyManager.a.FindTourneyItem(CurrentTournamentNo)?.OnClickButton(TournamentTableItem.BUTTON_STATE.Proceeding_g);
        UIManager.a.CloseLoading();
    }

    public void EnterHoldem(MS_TableInfo info, bool isEnablePlay)
    {
        NetworkManager.a.JoinTable(info.bGSNo, info.info_h.wTableNo, isEnablePlay ? 0 : 1, result =>
        {
            Sound.Stop();

            SceneManager.LoadScene(SceneManager.InGameScene, () =>
            {
                InGameManager.a.isObserve = isEnablePlay == false;
                SceneManager.UnloadScene(SceneManager.LobbyScene);
                State = GAME_STATE.CASH_GAME;
                CurrentTableNo = info.info_h.wTableNo;

                Debug.LogError($"Enter Table No :: {CurrentTableNo}");
            });
        });
    }

    public void EnterTournament(LMSC_TEntranceNotice2 p)
    {
        EnterTournament(p.nTournamentNo, p.gs_info);
    }

    public void EnterTournament(int tournamentNo, MS_T_GSInfo gs_info)
    {
        NetworkManager.a.SendTournamentView(tournamentNo, 1, (info, option, prize) =>
        {
            GameData.UpdateSubscriptionData(info, option, prize);
        });

        var pck = new LMCS_TEntranceInfoReq();
        pck.nTournamentNo = tournamentNo;
        bool bResult = false;
        NetworkManager.Lobby.Send<LMSC_TEntranceNotice2>(pck, p =>
        {
            bResult = true;
            NetworkManager.a.EnterTournTable(p.nTournamentNo, p.wTableNo, gs_info.dwServerIp, gs_info.wPort, 1, result =>
            {
                CurrentTournamentNo = tournamentNo;

                SceneManager.LoadScene(SceneManager.InGameScene, () =>
                {
                    InGameManager.a.isObserve = false;
                    SceneManager.UnloadScene(SceneManager.LobbyScene);
                    State = GAME_STATE.TOURNAMENT_GAME;
                });
            });
        });

        // ToDo: 네트워크 로딩을 넣어주세요.
        gameObject.CallDelay(1500, () =>
        {
            if (!bResult)
            {
                UIManager.a.OpenPopup<UIPopup_OneButton>(UIManager.POPUP_TYPE.NORMAL, "해당하는 테이블이 없습니다.");
            }
        });

    }

    public void OpenWebView(string url)
    {
        var obj = UIManager.a.OpenPopup<UIPopup_WebView>(UIManager.POPUP_TYPE.NORMAL);
        WebViewObject webViewObject = obj.GetComponent<WebViewObject>();

        webViewObject.Init((msg) =>
        {
            Debug.Log(string.Format("CallFromJS[{0}]", msg));
        });

        webViewObject.LoadURL(url);
        webViewObject.SetVisibility(true);
        webViewObject.SetMargins(50, 50, 150, 50);
    }


    public T RandomItem<T>(List<T> list)
    {
        int index = Random.Range(0, list.Count);
        return list[index];
    }
}
