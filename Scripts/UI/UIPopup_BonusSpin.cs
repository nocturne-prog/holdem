using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPopup_BonusSpin : UIPopup
{
    [Header("Pannel")]
    [SerializeField] private Transform obj_pannel;
    [SerializeField] private Transform obj_select;

    [Header("Wheel")]
    [SerializeField] private Transform obj_wheel;
    [SerializeField] private Transform obj_chip1;
    [SerializeField] private Transform obj_chip2;
    [SerializeField] private Transform obj_blur1;
    [SerializeField] private Transform obj_blur2;
    [SerializeField] private Transform obj_light;
    [SerializeField] private Transform obj_list;
    [SerializeField] private Text text_reward;
    [SerializeField] private Transform obj_bonus_reward;
    [SerializeField] private Text text_bonus_text;
    [SerializeField] private Text text_desc;

    LMSC_UserRouletteInfoRes info;
    LMSC_UserRouletteRewardRes info_res;
    Transform[] select_list;

    public override void Init(params object[] args)
    {
        base.Init(args);

        info = (LMSC_UserRouletteInfoRes)args[0];
        select_list = new Transform[obj_select.childCount];

        for (int i = 0; i < select_list.Length; i++)
        {
            select_list[i] = obj_select.GetChild(i);
        }

        if (info.rRoulette.bRewardEnable == 0)
            return;

        StartCoroutine(StartBonusSpin());
    }

    float selectGap = -48f;

    IEnumerator StartBonusSpin()
    {
        obj_select.gameObject.AddTween(new TweenMoveTo(new Vector3(0, info.rRoulette.bRewardCount * selectGap, 0), 0.2f));
        //transform.Translate(new Vector3(0, info.rRoulette.bRewardCount * selectGap, 0));
        PannelSelectActive(info.rRoulette.bRewardCount);
        obj_pannel.gameObject.AddTween(new TweenMoveTo(new Vector3(-400, 0), 0.6f, Tween.easeOutBack));

        NetworkManager.a.OnClickRoulette(res =>
        {
            info_res = res;
        });

        yield return new WaitWhile(() => info_res == null);

        //info_res = new LMSC_UserRouletteRewardRes();
        //info_res.wDropIdx = 6;

        NetworkManager.a.CheckRoulette(res =>
        {
            LobbyManager.a.button_Bottom_Spin.UpdateData(res);
        });

        yield return new WaitForSeconds(2f);

        obj_pannel.gameObject.AddTween(new TweenMoveTo(new Vector3(400, 0), 0.4f, Tween.easeInBack));
        obj_light.gameObject.AddTween(new TweenRotateTo(0, 0, -360, 0.3f).Repeat(-1));
        obj_light.SetActive(true);

        obj_blur1.SetActive(true);
        obj_blur2.SetActive(true);

        obj_blur1.gameObject.AddTween(new TweenMoveTo(new Vector3(0, 40), 0.1f, Tween.spring).Repeat(-1));
        obj_blur2.gameObject.AddTween(new TweenMoveTo(new Vector3(0, -40), 0.1f, Tween.spring).Repeat(-1));

        obj_list.SetActive(true);

        Vector3 pos = obj_list.position;
        obj_list.gameObject.AddTween(new TweenMoveTo(new Vector3(0, -105), 0.05f).EndEvent(() =>
        {
            obj_list.position = pos;
            obj_list.GetChild(obj_list.childCount - 1).SetAsFirstSibling();
        }).Repeat(-1));

        obj_list.gameObject.AddTween(new TweenAlpha(0, 1f));

        yield return new WaitForSeconds(2f);

        obj_list.gameObject.AddTween(new TweenAlpha(1, 0.5f));
        yield return new WaitForSeconds(0.5f);

        obj_list.gameObject.FlushTween();
        obj_list.gameObject.KillTween();

        obj_blur1.gameObject.FlushTween();
        obj_blur2.gameObject.FlushTween();
        obj_blur1.gameObject.KillTween();
        obj_blur2.gameObject.KillTween();

        obj_blur1.gameObject.AddTween(new TweenAlpha(0, 0.5f));
        obj_blur2.gameObject.AddTween(new TweenAlpha(0, 0.5f));

        obj_light.gameObject.KillTween();
        obj_light.gameObject.SetActive(false);

        int target = int.Parse(obj_list.GetChild(3).name);
        int repeatCount = (int)Mathf.Repeat(target - info_res.wDropIdx, 6);

        if (repeatCount > 0)
        {
            obj_list.gameObject.AddTween(new TweenMoveTo(new Vector3(0, -105), 0.05f).EndEvent(() =>
            {
                obj_list.position = pos;
                obj_list.GetChild(obj_list.childCount - 1).SetAsFirstSibling();
            }).Repeat(repeatCount - 1));
        }

        yield return new WaitForSeconds(1f);
        obj_wheel.gameObject.AddTween(new TweenScaleTo(1.5f, 1.5f, 1.5f, 0.2f));
        obj_list.gameObject.AddTween(new TweenAlpha(0, 0.5f));

        text_reward.text = $"{info_res.n64RewardMoney:N0}";
        text_reward.SetActive(true);
        text_reward.gameObject.AddTween(new TweenAlpha(1, 0.3f));
        obj_chip1.gameObject.AddTween(new TweenMoveTo(new Vector3(0, 200), 0.5f));
        obj_chip2.gameObject.AddTween(new TweenMoveTo(new Vector3(0, 200), 0.5f));

        yield return new WaitForSeconds(0.5f);

        text_bonus_text.text = $"{info_res.rRoulette.bRewardCount}회차 보너스 {info_res.nBonusPercent}% {info_res.n64RewardBonus:N0}";
        text_bonus_text.SetActive(true);

        obj_bonus_reward.SetActive(true);
        obj_bonus_reward.gameObject.AddTween(new TweenAlpha(1, 0.3f));

        yield return new WaitForSeconds(0.5f);

        text_bonus_text.gameObject.AddTween(new TweenText(info_res.n64RewardBonus, 0, 0.5f, (a) =>
            text_bonus_text.text = $"{info_res.rRoulette.bRewardCount}회차 보너스 {info_res.nBonusPercent}% {a:N0}"));

        text_reward.gameObject.AddTween(new TweenText(info_res.n64RewardMoney, info_res.n64RewardMoney + info_res.n64RewardBonus, 0.5f));

        yield return new WaitForSeconds(0.7f);

        obj_bonus_reward.gameObject.AddTween(new TweenAlpha(0, 0.3f));
        text_desc.SetActive(true);
        text_desc.gameObject.AddTween(new TweenAlpha(1, 0.5f));

        yield return new WaitForSeconds(1f);
        Close();
    }

    void PannelSelectActive(int index)
    {
        for (int i = 0; i < select_list.Length; i++)
        {
            select_list[i].SetActive(index == i);
        }
    }

    void UpdateBonusReawrd(long value, string text)
    {
    }
}
