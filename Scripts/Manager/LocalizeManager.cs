using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizeManager : Singleton<LocalizeManager>
{
    public enum Locale
    {
        KOR=1,
        ENG
    };

    public string this[string key] => GetString(key);

    public Locale locale;
    
    public TextAsset localizeAsset;

    public Dictionary<string, string> localeText=new Dictionary<string, string>();
    
    public void Start()
    {
        LoadLocalizeText(locale);
       
    } 

    public void LoadLocalizeText(Locale l)
    {
        locale = l;
        localeText.Clear();
        var resource=CSVReader.SplitCsvGrid(localizeAsset.text);
        foreach (var v in CSVReader.SplitCsvGrid(localizeAsset.text)) {
            localeText.Add(v[0],v[(int)locale]);
        }
        
    }

    public string GetString(string key)
    {
        if(localeText.ContainsKey(key))
            return localeText[key];
        else
        {
            Logger.Where(" Error! can't find locale key="+key);
            return "TEXT="+key;
        }
    }
    
}
