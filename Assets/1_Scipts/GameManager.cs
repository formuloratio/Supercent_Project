using UnityEngine;
using TMPro;
using System.Collections; // 코루틴을 위해 추가

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Economy")]
    [SerializeField] private long currentMoney = 0;
    public long CurrentMoney => currentMoney;

    [Header("Prisoner Management")]
    public int currentPrisonerCount = 0;
    public int maxPrisonerCount = 20;

    [Header("UI Reference")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI prisonerStatusText;
    public GameObject gameOverPanel;
    public GameObject joystick;

    private bool isGameOverTriggered = false; // 중복 실행 방지

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void Start()
    {
        UpdateMoneyUI();
        UpdatePrisonerUI();
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateMoneyUI();
    }

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

    // 죄수 추가 함수
    public void AddPrisoner()
    {
        // 이미 게임 오버가 진행 중이면 무시
        if (isGameOverTriggered) return;

        currentPrisonerCount++;
        UpdatePrisonerUI();

        if (currentPrisonerCount >= maxPrisonerCount)
        {
            isGameOverTriggered = true; // 플래그 설정
            StartCoroutine(GameOverSequence(2f)); // 2초 뒤 실행
        }
    }

    // 2초 대기 후 게임 오버를 처리하는 코루틴
    private IEnumerator GameOverSequence(float delay)
    {
        Debug.Log($"{delay}초 뒤 게임 오버됩니다...");

        // 지정된 시간(2초) 동안 대기
        yield return new WaitForSeconds(delay);

        TriggerGameOver();
    }

    private void TriggerGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        if (joystick != null) joystick.SetActive(false);

        Time.timeScale = 0f; // 여기서 게임을 멈춤
        Debug.Log("게임 오버: 수용소 만원 (2초 후 처리 완료)");
    }

    private void UpdateMoneyUI()
    {
        if (moneyText != null)
            moneyText.text = currentMoney.ToString("N0");
    }

    private void UpdatePrisonerUI()
    {
        if (prisonerStatusText != null)
        {
            prisonerStatusText.text = $"{currentPrisonerCount} / {maxPrisonerCount}";
            if (currentPrisonerCount >= maxPrisonerCount)
                prisonerStatusText.color = Color.red;
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}