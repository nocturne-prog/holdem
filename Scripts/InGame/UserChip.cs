using UnityEngine;
using UnityEngine.UI;

public class UserChip : MonoBehaviour
{
    [SerializeField] private Text text_chip;
    [SerializeField] private GameObject obj_rate;
    [SerializeField] private Text text_rate;

    private void Start()
    {
        Rate_Value = -1;
    }

    private long chip = 0;
    public long Value
    {
        get
        {
            return chip;
        }

        set
        {
            chip = value;
            text_chip.transform.SetActive(value > 0);
            text_chip.text = $"{Util.GetMoneyString(value, GameManager.a.isTourney)}";
        }
    }

    public float Rate_Value
    {
        set
        {
            obj_rate.SetActive(value >= 0);
            text_rate.text = value.ToString("P");
        }
    }

    public bool SetActive { set { gameObject.SetActive(value); } }
    public Vector3 MoneyPos => text_chip.transform.position;

    public void UpdateScale()
    {
        bool reflection = transform.parent.localPosition.x > 0;

        if (reflection)
        {
            transform.parent.localScale = new Vector3(-1, 1, 1);
            text_chip.transform.localScale = new Vector3(-1, 1, 1);
            text_rate.transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.parent.localScale = Vector3.one;
            text_chip.transform.localScale = Vector3.one;
            text_rate.transform.localScale = Vector3.one;
        }
    }

    public void Clear()
    {
        Value = 0;
        Rate_Value = -1;
    }

    public void Delete()
    {
        Destroy(gameObject);
    }
}
