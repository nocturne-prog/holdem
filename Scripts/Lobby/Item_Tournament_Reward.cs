using UnityEngine;
using UnityEngine.UI;

public class Item_Tournament_Reward : MonoBehaviour
{
    [SerializeField] public Text text_rank;
    [SerializeField] public Text text_reward;
    [SerializeField] public Text text_percent;

    public string Rank { set { text_rank.text = value; } }
    public string Reward { set { text_reward.text = value; } }
    public string Percent { set { text_percent.text = value; } }
}
