using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingManager : Singleton<LoadingManager>
{
    private Loading loading;
    private Canvas canvas;

    public void Show()
    {
        loading = Instantiate(ResourceManager.Load<Loading>(Const.LOADING));

        canvas = loading.gameObject.GetComponent<Canvas>();
        canvas.worldCamera = GameManager.a.mainCamera;
        canvas.sortingLayerName = "Loading";
    }

    public void Hide()
    {
        if (loading == null) return;

        Destroy(loading.gameObject);
    }

    public void SetPercent(float p)
    {
        if (loading == null) return;

        p = Mathf.Clamp(p, 0, 1);

        loading.slider.value = p;
    }
}
