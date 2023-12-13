using UnityEngine;

public static class Sound
{
    static GameObject _soundObject;
    public static GameObject soundObject
    {
        get
        {
            if (_soundObject == null)
            {

                _soundObject = new GameObject("Sound");
                GameObject.DontDestroyOnLoad(_soundObject);
            }

            return _soundObject;
        }
    }

    public static void SourceController(System.Action<AudioSource> action, string str = null)
    {
        foreach (AudioSource t in soundObject.GetComponents<AudioSource>())
        {
            if (str == null || t.clip.name == str)
            {
                action(t);
            }
        }
    }

    public static float volume
    {
        get
        {
            AudioSource temp = soundObject.GetComponent<AudioSource>();
            if (temp != null)
            {
                return temp.volume;
            }
            else return 0;
        }

        set
        {
            SourceController(audio => audio.volume = value);

        }
    }


    static bool _bMute = false;
    public static bool bMute
    {
        get { return _bMute; }
        set
        {

            _bMute = value;

            SourceController(audio => audio.mute = value);

        }
    }

    static bool _bBg = true;
    public static bool bBg
    {
        get { return _bBg; }
        set
        {

            _bBg = value;

            if (bgMusic)
                bgMusic.mute = !value;


        }
    }

    static bool _bEffect = true;
    public static bool bEffect
    {
        get { return _bEffect; }
        set
        {

            _bEffect = value;

            SourceController(audio => { if (audio != bgMusic) audio.mute = !value; });

        }
    }

    public static void Stop()
    {

        SourceController(t => Component.Destroy(t));
    }

    public static void Stop(string str)
    {

        SourceController(t => Component.Destroy(t), str);
    }


    static AudioSource bgMusic = null;

    public static void StopBg()
    {
        if (bgMusic)
        {
            Component.Destroy(bgMusic);
            bgMusic = null;
        }
    }

    public static AudioSource PlayBg(string str)
    {

        if (!bgMusic)
            bgMusic = soundObject.AddComponent<AudioSource>();


        bgMusic.clip = Resources.Load("Sounds/" + str) as AudioClip;
        bgMusic.loop = true;
        bgMusic.Play();
        bgMusic.mute = Sound.bMute | !Sound.bBg;

        return bgMusic;
    }

    public static AudioSource Play(string str, bool bLoop = false)
    {

        AudioSource audiosource = soundObject.AddComponent<AudioSource>();
        audiosource.clip = Resources.Load("Sounds/" + str) as AudioClip;
        audiosource.loop = bLoop;
        audiosource.Play();
        audiosource.mute = Sound.bMute | !Sound.bEffect;
        if (!bLoop)
        {
            Component.Destroy(audiosource, audiosource.clip.length);
        }

        return audiosource;
    }

    public static AudioSource PlayVoice(string str, bool loop = false)
    {
        if (!Option.Voice)
            return null;

        return Play(str, loop);
    }

    public static AudioSource PlayEffect(string str, bool loop = false)
    {
        if (!Option.Effect)
            return null;

        return Play(str, loop);
    }
}



