using System.Collections; 
using System.Collections.Generic; 
using UnityEngine; 
using UnityEngine.SceneManagement; 
using TMPro;
using UnityEngine.UI; 
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI Panels")]
    public GameObject startGamePannal;
    public GameObject levelWinPannal;
    public GameObject gameOverPannal;

    [Header("UI Texts")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI starsText;
    
    [Header("Âm thanh")]
    public AudioSource audioSource;
    public AudioClip winSound;
    public AudioClip gameOverSound;

    [Header("Gameplay")]
    public float levelTimeLimit = 60f; 
    public int maxMoves = 30; 

    [Header("Buttons")]
    public Button goHomeButton;     
    public Button rollbackButton; 

    private float currentTime;
    private bool startGame = false;     
    private bool levelWin = false;
    private bool isgameOver = false;
    private bool isRestarting = false;
    private int currentLevelIndex;
    private int currentScore = 0;
    private int moveCount = 0;

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
        if (gameOverPannal != null) gameOverPannal.SetActive(false);
         
        currentTime = levelTimeLimit;
        UpdateLevelText();
        UpdateTimerUI();
        UpdateScoreUI();
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
        else if (isgameOver && Input.GetMouseButtonDown(0))
        {
            RestartLevel();
        }
        if (startGame && !levelWin && !isgameOver)
        {
            // Đếm ngược thời gian
            currentTime -= Time.deltaTime;
            if (currentTime <= 0)
            {
                currentTime = 0;
                GameOver(); // Hết giờ => thua
            }
            UpdateTimerUI();
        }
    }

    private void StartGame()
    {
        startGame = true;
        Time.timeScale = 1;
        if (startGamePannal != null) startGamePannal.SetActive(false);
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
            if (sameColor) validTowerCount++;   
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
        levelWin = true;
        Time.timeScale = 0;
        if (levelWinPannal != null) levelWinPannal.SetActive(true);
        if (levelText != null) levelText.gameObject.SetActive(false);
        if (gameOverPannal != null) gameOverPannal.SetActive(false);

        CalculateScore();
        UpdateStarsUI();
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
    levelWin = false;
    moveCount = 0;
    currentScore = 0;

    // Nếu ở MainMenu
    if (scene.name == "MainMenu")
    {
        // Ẩn toàn bộ UI level
        if (levelText != null) levelText.gameObject.SetActive(false);
        if (gameOverPannal != null) gameOverPannal.SetActive(false);
        if (levelWinPannal != null) levelWinPannal.SetActive(false);
        if (startGamePannal != null) startGamePannal.SetActive(false);

        if (goHomeButton != null) goHomeButton.gameObject.SetActive(false);
        if (rollbackButton != null) rollbackButton.gameObject.SetActive(false);
        return;
    }

    // Nếu là Level → tìm và gán lại button
        if (scene.name.StartsWith("Level"))
        {
            // Gán lại nút GoHome
            if (goHomeButton != null)
            {
                Button btn = goHomeButton.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(GoHome);
            }

            // Gán lại nút Rollback
            if (rollbackButton != null)
            {
                Button btn = rollbackButton.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(RollBack);
            }

    Tower[] towers = FindObjectsOfType<Tower>();
    int totalTowers = towers.Length;

    // Gán thời gian theo số trụ
    switch (totalTowers)
    {
        case 3: levelTimeLimit = 15f; break;
        case 4: levelTimeLimit = 25f; break;
        case 7: levelTimeLimit = 35f; break;
        case 8: levelTimeLimit = 40f; break;
        case 10: levelTimeLimit = 50f; break;
        default: levelTimeLimit = 60f; break;
    }
    currentTime = levelTimeLimit;

    if (isRestarting) // nếu đang restart thì auto start
    {
        startGame = true;
        isRestarting = false;
        if (startGamePannal != null) startGamePannal.SetActive(false);
    }
    else // lần đầu vào level
    {
        startGame = false;
        if (startGamePannal != null) startGamePannal.SetActive(true);
    }

    if (levelWinPannal != null) levelWinPannal.SetActive(false);
    if (levelText != null) levelText.gameObject.SetActive(true);
    if (gameOverPannal != null) gameOverPannal.SetActive(false);

    UpdateLevelText();
    UpdateTimerUI();
    UpdateScoreUI();

    Time.timeScale = 1f;
}
}

    private void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text = "Level " + (currentLevelIndex-1) ;
        }
    }
    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore;
        }
    }

    private void UpdateStarsUI()
    {
        if (starsText != null)
        {
            starsText.text = "★".PadRight(GetStars(), '☆'); // Ví dụ: ★★☆
        }
    }

    private void CalculateScore()
    {
        // Tính điểm dựa trên thời gian còn lại và số bước
        int timeBonus = Mathf.RoundToInt(currentTime * 10);
        int moveBonus = Mathf.Max(0, (maxMoves - moveCount) * 5);

        currentScore = timeBonus + moveBonus;
        Debug.Log($"Score: {currentScore} (Time: {timeBonus}, Moves: {moveBonus})");

        UpdateScoreUI();
    }

    private int GetStars()
    {
        if (currentTime > levelTimeLimit * 0.5f) return 3; // Hoàn thành nhanh
        else if (currentTime > levelTimeLimit * 0.25f) return 2;
        return 1;
    }

    private void GameOver()
{
    if (isgameOver) return;
    
    isgameOver = true;
    Time.timeScale = 0;
    Debug.Log("Game Over: Hết thời gian!");

    if (gameOverPannal != null) gameOverPannal.SetActive(true);
    if (startGamePannal != null) startGamePannal.SetActive(false);
    if (levelWinPannal != null) levelWinPannal.SetActive(false);
    if (levelText != null) levelText.gameObject.SetActive(false);

    if (audioSource != null && gameOverSound != null)
    {
        audioSource.Stop();           // dừng nhạc cũ (nếu có)
        audioSource.clip = gameOverSound;
        audioSource.loop = false;
        audioSource.Play();
    }

}

    private void RestartLevel()
{
    if (audioSource != null) audioSource.Stop();

    isRestarting = true; // đánh dấu đang restart

    isgameOver = false;
    levelWin = false;
    startGame = false;

    if (gameOverPannal != null) gameOverPannal.SetActive(false);

    Time.timeScale = 1f;
    SceneManager.LoadScene(currentLevelIndex);
}
    // Quay về MainMenu
    public void GoHome()
    {
        //if (audioSource != null) audioSource.Stop();

        isRestarting = false;
        isgameOver = false;
        levelWin = false;
        startGame = false;

        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void RollBack()
    {
        if (audioSource != null) audioSource.Stop();

        isRestarting = true;
        isgameOver = false;
        levelWin = false;
        startGame = false;

        Time.timeScale = 1f;
        SceneManager.LoadScene(currentLevelIndex);
    }

}
