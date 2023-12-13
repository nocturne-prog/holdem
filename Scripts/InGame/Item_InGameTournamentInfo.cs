using UnityEngine;
using UnityEngine.UI;

public class Item_InGameTournamentInfo : MonoBehaviour
{
    public Text[] texts;

    public void SetText(int i, string text)
    {
        if (texts.Length - 1 < i)
        {
            return;
        }

        texts[i].text = text;
    }
}
