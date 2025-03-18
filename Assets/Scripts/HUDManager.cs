using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [SerializeField] private Player player;

    [SerializeField] private Image[] healthImage;
    [SerializeField] private TextMeshProUGUI scoreText;

    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI resultText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        player.OnPlayerDamaged += UpdateHpUI;
        GameManager.Instance.OnScoreTextUpdate += UpdateScoreText;
        GameManager.Instance.OnStageStart += UpdateStageInfo;
        GameManager.Instance.OnGameFinish += ShowGameOverPanel;
    }

    private void OnDestroy()
    {
        player.OnPlayerDamaged -= UpdateHpUI;
        GameManager.Instance.OnScoreTextUpdate -= UpdateScoreText;
        GameManager.Instance.OnStageStart -= UpdateStageInfo;
        GameManager.Instance.OnGameFinish -= ShowGameOverPanel;
    }

    private void UpdateScoreText(int score)
    {
        scoreText.SetText($"{score}");
    }

    private void UpdateStageInfo(int totalScore)
    {
        scoreText.SetText($"{totalScore}");
    }

    private void UpdateHpUI(int health)
    {
        for (int i = 0; i < healthImage.Length; i++)
        {
            healthImage[i].color = new Color(1f, 1f, 1f, (i < health ? 1f : 0.4f));
        }
    }

    private void ShowGameOverPanel(bool result)
    {
        resultText.SetText(result ? "YOU WIN!" : "YOU LOSE!");
        gameOverPanel.SetActive(true);
    }
}
