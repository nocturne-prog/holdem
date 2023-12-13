using u = UnityEngine.SceneManagement;
using System;

public static class SceneManager
{
    public static readonly string LoginScene = "Login";
    public static readonly string CommonScene = "Common";
    public static readonly string LobbyScene = "Lobby";
    public static readonly string InGameScene = "InGame";

    public static void LoadScene(string sceneName, Action callback = null)
    {
        u.SceneManager.LoadSceneAsync(sceneName, u.LoadSceneMode.Additive).completed += done =>
        {
            callback?.Invoke();
        };
    }

    public static void UnloadScene(string sceneName)
    {
        u.SceneManager.UnloadSceneAsync(sceneName);
    }
    public static void SetActiveScene(string sceneName)
    {
        u.SceneManager.SetActiveScene(u.SceneManager.GetSceneByName(sceneName));
    }

    public static void LoadLoginScene()
    {
        u.SceneManager.LoadSceneAsync(LoginScene).completed += done =>
        {
            UIManager.a.CloseAll();
            UIManager.a.CloseLoading();
        };
    }

}
