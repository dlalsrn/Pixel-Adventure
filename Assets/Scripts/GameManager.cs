

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private static bool IsInitialized = false; // 게임 시작 시 초기화 여부

    [SerializeField] private Player player;

    public event Action<int> OnScoreTextUpdate;
    public event Action<int> OnStageStart; // StageNum, TotalScore
    public event Action<bool> OnGameFinish; // 게임의 승, 패 (스테이지 모두 클리어 = 승리, 목숨 모두 잃음 = 패배)

    private int totalScore;
    private int currentStage;

    public bool IsGameOver { get; private set; }

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

        if (!IsInitialized)
        {
            PlayerPrefs.DeleteAll();
            IsInitialized = true;
        }
    }

    private void Start()
    {
        IsGameOver = false;
        totalScore = PlayerPrefs.GetInt("TotalScore", 0);
        currentStage = PlayerPrefs.GetInt("CurrentStage", 1);
        OnStageStart?.Invoke(totalScore);
        player.OnPlayerGetScore += AddScore;
        player.OnPlayerStageClear += StageClear;
        player.OnPlayerDead += SetGameOver;
    }

    private void OnDestroy()
    {
        player.OnPlayerGetScore -= AddScore;
        player.OnPlayerStageClear -= StageClear;
        player.OnPlayerDead -= SetGameOver;
    }

    public void AddScore(int score)
    {
        totalScore += score;
        OnScoreTextUpdate?.Invoke(totalScore);
    }

    public void StageClear(int nextStageNum)
    {
        if (nextStageNum == currentStage) // 마지막 Stage까지 Clear했다면
        {
            SetGameOver(true); // 게임 Clear
        }
        else // 남은 Stage가 존재한다면
        {
            currentStage = nextStageNum;
            PlayerPrefs.SetInt("TotalScore", totalScore);
            PlayerPrefs.SetInt("CurrentStage", currentStage);
            SceneManager.LoadScene($"Scenes/Stage{nextStageNum}");
        }
    }

    public void SetGameOver(bool result)
    {
        if (!IsGameOver)
        {
            IsGameOver = true;
            OnGameFinish?.Invoke(result);
        }
    }

    public void StartGame() // Load Stage1 Scene
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("Scenes/Stage1");
    }
}
