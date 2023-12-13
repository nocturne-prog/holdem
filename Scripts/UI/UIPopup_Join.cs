using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPopup_Join : UIPopup
{
    [SerializeField] WebViewObject webVeiw;

    public Vector2 webVieSize;
    public Vector2 webViewPos;

    public override void Init(params object[] args)
    {
        isBlurOff = false;

        base.Init(args);

        webVeiw.Init(msg =>
        {
        });

        webVeiw.LoadURL("*****");
        webVeiw.SetVisibility(true);

        CanvasScaler scaler = UIManager.a.canvas.GetComponent<CanvasScaler>();
        Vector2 canvasSize = scaler.referenceResolution;
        Vector2 ScreenSize = new Vector2(Screen.width, Screen.height);
        float ratio = ScreenSize.x / canvasSize.x;

        Vector2 imgScreenSize = new Vector2(webVieSize.x * ratio, webVieSize.y * ratio);
        Vector3 ScreenPos = new Vector3(webViewPos.x * ratio, webViewPos.y * ratio);

        float width = (ScreenSize.x - imgScreenSize.x) / 2;
        float height = (ScreenSize.y - imgScreenSize.y) / 2;

        float left = width;
        float right = width;
        float top = height;
        float bottom = height;

        if (Mathf.Abs(ScreenPos.x) > 0)
        {
            if (ScreenPos.x > 0)
            {
                left += Mathf.Abs(ScreenPos.x);
                right -= Mathf.Abs(ScreenPos.x);
            }
            else
            {
                left += Mathf.Abs(ScreenPos.x);
                right -= Mathf.Abs(ScreenPos.x);
            }
        }

        if (Mathf.Abs(ScreenPos.y) > 0)
        {
            if (ScreenPos.y > 0)
            {
                top += Mathf.Abs(ScreenPos.y);
                bottom -= Mathf.Abs(ScreenPos.y);
            }
            else
            {
                top += Mathf.Abs(ScreenPos.y);
                bottom -= Mathf.Abs(ScreenPos.y);
            }
        }

        webVeiw.SetMargins((int)left, (int)top, (int)right, (int)bottom);

        //Debug.LogError($"DPI: {Screen.dpi}");
        //Debug.LogError($"WebVieSize :{webVieSize}, Fixed Size: {imgScreenSize}");
        //Debug.LogError($"Canvas: {canvasSize}, Screen: {ScreenSize}, Pos: {ScreenPos}, Ratio: {ratio}");
        //Debug.LogError($"Left: {left}, Top: {top}, Right: {right}, Bottom: {bottom}");
    }
}
