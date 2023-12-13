using UnityEngine;
using UnityEngine.UI;

public class Item_Tournament_Blind : MonoBehaviour
{
    [SerializeField] public Text text_level;
    [SerializeField] public Text text_blind;
    [SerializeField] public Text text_ante;
    [SerializeField] public Text text_blindupTime;

    public int Level { set { text_level.text = $"{value}"; } }
    public string Blind { set { text_blind.text = value; } }
    public int Blind_Time { set { text_blindupTime.text = $"{value}분"; } }

    public long Ante
    {
        set
        {
            if (value == 0)
                text_ante.text = "-";
            else
                text_ante.text = $"{Util.GetMoneyString(value)}";
        }
    }
}
