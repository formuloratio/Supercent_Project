using UnityEngine;
using TMPro; // TextMeshPro 사용 시

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Economy")]
    [SerializeField] private long currentMoney = 0;
    public long CurrentMoney => currentMoney;

    [Header("UI Reference")]
    public TextMeshProUGUI moneyText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateMoneyUI();
    }

    // 돈을 추가하는 공용 함수
    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateMoneyUI();
    }

    // 돈을 사용하는 공용 함수 (성공 시 true 반환)
    public bool SpendMoney(int amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            UpdateMoneyUI();
            return true;
        }
        return false;
    }

    private void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            // 하이퍼캐주얼 느낌의 숫자 포맷 (예: 1,200)
            moneyText.text = currentMoney.ToString("N0");
        }
    }
}