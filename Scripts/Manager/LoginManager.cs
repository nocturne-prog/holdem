using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    public Canvas canvas;
    public Transform logo;
    public Transform warning;
    public Transform login;
    public InputField inputField_id;
    public InputField inputField_password;
    public Button btn_login;
    public Button btn_Terms;
    public Button btn_Join;

    string id;
    string password;

    public static LoginManager a;

    private void Awake()
    {
        a = this;
    }

    public IEnumerator Start()
    {
        bool sceneLoad = false;

        GameManager.a.State = GameManager.GAME_STATE.LOG_IN;

        SceneManager.LoadScene(SceneManager.CommonScene, () =>
        {
            SceneManager.SetActiveScene(SceneManager.CommonScene);

            GameManager.a.mainCamera = Camera.main;
            canvas.worldCamera = GameManager.a.mainCamera;
            canvas.sortingLayerName = "Main";

            UIManager.a.CloseAll();
            sceneLoad = true;
        });

        yield return new WaitWhile(() => !sceneLoad);
        yield return StartCoroutine(ShowLogo());
        yield return StartCoroutine(ShowWarning());

        StartLogin();
        Init();
    }

    public void Init()
    {
        id = PlayerPrefs.GetString("id");
        password = PlayerPrefs.GetString("password");

        inputField_id.text = id;
        inputField_password.text = password;

        inputField_id.onEndEdit.AddListener(text =>
        {
            id = text;
        });

        inputField_password.onEndEdit.AddListener(text =>
        {
            password = text;
        });

        btn_login.SetButton(() =>
        {
            Sound.PlayEffect(Const.SOUND_INTRO_LOGIN);
            Connect();
            btn_login.interactable = false;
        });

        btn_Terms.SetButton(() =>
        {
            GameManager.a.OpenWebView("*****");
        });

        btn_Join.SetButton(() =>
        {
            UIManager.a.OpenPopup<UIPopup_Join>(UIManager.POPUP_TYPE.NORMAL);
        });
    }

    public void StartLogin()
    {
        Sound.PlayEffect(Const.SOUND_INTRO_WELCOM);
        login.SetActive(true);
    }

    public enum LOGIN_STATE
    {
        None = 0,
        Success,
        Failed
    }

    public LOGIN_STATE state { get; private set; } = LOGIN_STATE.None;

    public void Connect()
    {
        if (state == LOGIN_STATE.Success) return;

        GameData.Player.UserId = id;
        GameData.Player.Password = password;

        PlayerPrefs.SetString("id", id);
        PlayerPrefs.SetString("password", password);

        Invoke(nameof(OpenErrorPopup), 2f);

        NetworkManager.a.ConnectToLobby(result =>
        {
            CancelInvoke(nameof(OpenErrorPopup));

            if (result == LMSC_LoginRes3.Code.ER_NO)
            {
                state = LOGIN_STATE.Success;
                GameManager.a.LoginComplete();
            }
            else if (result == LMSC_LoginRes3.Code.ER_SELF_BREAKTIME || result == LMSC_LoginRes3.Code.ER_SELF_LIMIT_BREAKTIME)
            {
                NetworkManager.Lobby.LoginFail();
                state = LOGIN_STATE.Failed;
                UIPopup_OneButton popup = UIManager.a.OpenPopup<UIPopup_OneButton>(UIManager.POPUP_TYPE.SYSTEM, Const.LOGIN_ERROR_MSG[result]);
                popup.callback += Application.Quit;
            }
            else if (result == LMSC_LoginRes3.Code.ER_VERSION_ERROR)
            {
                NetworkManager.Lobby.LoginFail();
                state = LOGIN_STATE.Failed;
                UIManager.a.OpenPopup<UIPopup_Update>(UIManager.POPUP_TYPE.SYSTEM);
            }
            else
            {
                NetworkManager.Lobby.LoginFail();
                state = LOGIN_STATE.Failed;
                UIManager.a.OpenPopup<UIPopup_OneButton>(UIManager.POPUP_TYPE.SYSTEM, $"{Const.LOGIN_ERROR_MSG[result]}");
                btn_login.interactable = true;
            }
        });
    }

    IEnumerator ShowWarning()
    {
        warning.gameObject.SetAlpha(0);
        warning.SetActive(true);

        float duration = 1.5f;
        float value = 0;

        while (duration > value)
        {
            value += Time.deltaTime;
            warning.gameObject.SetAlpha(value / duration);

            yield return null;
        }

        warning.gameObject.SetAlpha(1);

        yield return new WaitForSeconds(duration);

        warning.SetActive(false);
    }

    IEnumerator ShowLogo()
    {
        logo.SetActive(true);
        Sound.PlayEffect(Const.SOUND_INTRO_BGM);

        Transform icon = logo.GetChild(1).transform;
        Transform mask = icon.GetChild(0).transform;

        icon.SetActive(true);
        icon.gameObject.SetAlpha(0);

        float showDuration = 1f;
        float value = 0;

        while (showDuration > value)
        {
            value += Time.deltaTime;

            icon.gameObject.SetAlpha(value / showDuration);
            yield return null;
        }

        icon.gameObject.SetAlpha(1);

        float moveDuration = 1f;
        value = 0;

        // -230 ~ 230
        while (moveDuration > value)
        {
            value += Time.deltaTime;

            mask.transform.localPosition = Vector3.Lerp(new Vector3(-230, 0, 0), new Vector3(230, 0, 0), value / moveDuration);
            yield return null;
        }

        value = 0;

        while (showDuration > value)
        {
            value += Time.deltaTime;

            icon.gameObject.SetAlpha(1 - (value / showDuration));
            yield return null;
        }

        icon.gameObject.SetAlpha(0);

        logo.SetActive(false);
    }

    void OpenErrorPopup()
    {
        UIPopup_OneButton popup = UIManager.a.OpenPopup<UIPopup_OneButton>(UIManager.POPUP_TYPE.NORMAL, "서버에 연결할 수 없습니다.");
        popup.callback += () => btn_login.interactable = true;
    }
}
