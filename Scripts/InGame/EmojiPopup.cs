using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class EmojiPopup : MonoBehaviour
{
    public Button btn;
    public Item_Emoji prefab;
    public ToggleGroupEx toggleGroup;
    public GameObject block;
    public Text block_text;

    public bool Active
    {
        get
        {
            return gameObject.activeInHierarchy;
        }
        set
        {
            block.SetActive(emojiBlock);
            gameObject.SetActive(value);
        }
    }
    public void Awake()
    {
        prefab.gameObject.SetActive(false);

        foreach (var v in Const.EmojiData)
        {
            Item_Emoji item = Instantiate(prefab, toggleGroup.transform).GetComponent<Item_Emoji>();
            item.SetData(v.Key, v.Value);
            ToggleEx toggle = item.GetComponent<ToggleEx>();
            toggleGroup.RegisterToggle(toggle);
            toggle.group = toggleGroup;

            item.gameObject.SetActive(true);
        }

        toggleGroup.onValueChanged.AddListener((key) => OnClickToggle(key));

        btn.SetButton(() =>
        {
            InGameManager.a.buttonController.OnClickEmoji(false);
        });

        Active = false;
    }

    DateTime sendTime = DateTime.Now;
    int blockCount = 0;
    bool emojiBlock = false;

    public void OnClickToggle(int key)
    {
        if (block.activeSelf)
            return;

        if ((DateTime.Now - sendTime).TotalSeconds < 2.5f)
        {
            blockCount++;

            if (blockCount >= 2)
            {
                ShowBlock();
            }
        }
        else
        {
            blockCount = 0;
            sendTime = DateTime.Now;
            InGameManager.a.buttonController.SendEmoji($"/{key + 1:00}");
        }
    }

    public void ShowBlock()
    {
        emojiBlock = true;
        block.SetActive(true);
        StartTimer();
    }

    public void StartTimer()
    {
        InvokeRepeating(nameof(SetTextTimer), 0, 1);
    }

    public void SetTextTimer()
    {
        TimeSpan time = DateTime.Now - sendTime;
        double value = 15 - time.TotalSeconds;
        block_text.SetText($"이모티콘은 {value:0} 초 후에 다시 사용 가능합니다.");

        if (value < 0)
        {
            CancelInvoke(nameof(SetTextTimer));
            block.SetActive(false);
            emojiBlock = false;
        }
    }
}
