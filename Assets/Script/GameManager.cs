using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameObject startGamePannal;
    public GameObject levelWinPannal;

    public TextMeshProUGUI levelText;

    [Header("Âm thanh")]
    public AudioSource audioSource;
    public AudioClip winSound;

    private bool startGame = false;     
    private bool levelWin = false;

    private int currentLevelIndex;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Giữ GameManager
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        currentLevelIndex = SceneManager.GetActiveScene().buildIndex;

        if (startGamePannal != null) startGamePannal.SetActive(true);
        if (levelWinPannal != null) levelWinPannal.SetActive(false);
         
        UpdateLevelText();
        Time.timeScale = 0; // Game dừng khi bắt đầu
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        if (!startGame && Input.GetMouseButtonDown(0))
        {
            StartGame();
        }
        else if (levelWin && Input.GetMouseButtonDown(0))
        {
            LoadNextLevel();
        }
    }

    private void StartGame()
    {
        startGame = true;
        Time.timeScale = 1;
        if (startGamePannal != null)
            startGamePannal.SetActive(false);
    }

    public void CheckWinCondition()
{
    if (levelWin) return;

    Tower[] towers = FindObjectsOfType<Tower>();
    int totalTowers = towers.Length;
    int validTowerCount = 0;

    // Đếm số trụ hợp lệ (đầy + đồng màu)
    foreach (var tower in towers)
    {
        int count = tower.rings.Count;

        if (count == tower.maxRings)
        {
            var r0 = tower.rings[0]?.GetComponent<Renderer>();
            if (r0 == null) continue;

            var targetMat = r0.sharedMaterial;
            bool sameColor = true;

            for (int i = 1; i < count; i++)
            {
                var ri = tower.rings[i]?.GetComponent<Renderer>();
                if (ri == null || ri.sharedMaterial != targetMat)
                {
                    sameColor = false;
                    break;
                }
            }

            if (sameColor)
            {
                validTowerCount++;
            }
        }
    }

    // Xác định số trụ cần thiết theo luật
    int requiredValid = 0;
    switch (totalTowers)
    {
        case 3: requiredValid = 2; break;
        case 4: requiredValid = 3; break;
        case 7: requiredValid = 5; break;
        case 8: requiredValid = 6; break;
        case 10: requiredValid = 7; break;
        default: requiredValid = totalTowers; break; // mặc định yêu cầu tất cả
    }

    // Nếu số trụ hợp lệ >= yêu cầu
    if (validTowerCount >= requiredValid)
    {
        Debug.Log($"LEVEL WIN TRIGGERED ({validTowerCount}/{requiredValid} trụ hợp lệ)");
        levelWin = true;
        Time.timeScale = 0;
        if (levelWinPannal != null) levelWinPannal.SetActive(true);
        if (levelText != null) levelText.gameObject.SetActive(false);
        // phát nhạc win
        if (audioSource != null && winSound != null) audioSource.PlayOneShot(winSound);           
    }
}

    private void LoadNextLevel()
    {
        // Dừng nhạc trước khi qua level khác
        if (audioSource != null && audioSource.isPlaying) audioSource.Stop();
        int nextSceneIndex = currentLevelIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("Bạn đã hoàn thành tất cả các level!");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentLevelIndex = scene.buildIndex;
        startGame = false;
        levelWin = false;
       
        // Reset lại UI
        if (startGamePannal != null) startGamePannal.SetActive(true);
        if (levelWinPannal != null) levelWinPannal.SetActive(false);
        if (levelText != null) levelText.gameObject.SetActive(true);
        UpdateLevelText();
        Time.timeScale = 1f;
    }
    private void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text = "Level " + (currentLevelIndex+1) ;
        }
    }
}
