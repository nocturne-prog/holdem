using UnityEngine;

public static class Option
{
    public static bool ShowRank
    {
        get { return GetValue("show_rank", true); }
        set { SetValue("show_rank", value); }
    }
    public static bool ShowButton
    {
        get { return GetValue("use_showButton", true); }
        set { SetValue("use_showButton", value); }
    }
    public static bool AutoBuyin
    {
        get { return GetValue("use_auto_buyin"); }
        set { SetValue("use_auto_buyin", value); }
    }
    public static bool CheckFold
    {
        get { return GetValue("use_check_fold"); }
        set { SetValue("use_check_fold", value); }
    }
    public static bool ColorCard
    {
        get { return GetValue("show_color_card"); }
        set { SetValue("show_color_card", value); }
    }

    public static bool ShowDown
    {
        get { return GetValue("show_down", true); }
        set { SetValue("show_down", value); }
    }

    public static bool HandCard
    {
        get { return GetValue("hand_card"); }
        set { SetValue("hand_card", value); }
    }
    public static bool Effect
    {
        get { return GetValue("use_effect_sound", true); }
        set { SetValue("use_effect_sound", value); }
    }
    public static bool Voice
    {
        get { return GetValue("use_voice_sound", true); }
        set { SetValue("use_voice_sound", value); }
    }
    public static bool Alarm
    {
        get { return GetValue("use_alarm", true); }
        set { SetValue("use_alarm", value); }
    }

    public static bool Vibe
    {
        get { return GetValue("use_vibe", true); }
        set { SetValue("use_vibe", value); }
    }
    static bool GetValue(string name, bool defaultValue = false)
    {
        int dValue = defaultValue ? 1 : 0;
        return PlayerPrefs.GetInt(name, dValue) == 1;
    }

    static void SetValue(string name, bool value)
    {
        PlayerPrefs.SetInt(name, value ? 1 : 0);
    }
}
