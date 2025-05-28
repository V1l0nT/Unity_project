using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GameObject hazardPrefab;
    [SerializeField]
    private int maxHazardsToSpawn = 5;
    [SerializeField]
    private TMPro.TextMeshProUGUI scoreText;
    [SerializeField]
    private TMPro.TextMeshProUGUI levelText;
    [SerializeField]
    private Image pauseMenu;
    [SerializeField]
    private GameObject mainVCam;
    [SerializeField]
    private GameObject zoomVCam;
    [SerializeField]
    private GameObject gameOverMenu;
    [SerializeField]
    private GameObject victoryMenu;
    [SerializeField]
    private GameObject player;

    [Header("UI Elements")]
    [SerializeField]
    private GameObject healthBarUI;

    private int highScore;
    private int score;
    private float timer;
    private Coroutine hazardCoroutine;
    private bool gameOver;
    private bool victory;

    private static GameManager instance;
    private const string HighScorePreferenceKey = "HighScore";

    public static GameManager Instance => instance;

    public int HighScore => highScore;

    [SerializeField]
    private float[] levelTimeThresholds = { 10f, 20f, 30f };

    [SerializeField]
    private float victoryTime = 40f;

    [SerializeField]
    private float[] spawnRateMultipliers = { 1f, 0.9f, 0.8f, 0.5f };

    private int currentLevel = 1;
    private int currentMaxHazardsToSpawn;

    void Start()
    {
        instance = this;

        highScore = PlayerPrefs.GetInt(HighScorePreferenceKey);
        levelText.text = $"Level: {currentLevel}";

        if (victoryMenu != null)
            victoryMenu.SetActive(false);
    }

    private void OnEnable()
    {
        player.SetActive(true);

        zoomVCam.SetActive(false);
        mainVCam.SetActive(true);

        gameOver = false;
        victory = false;
        scoreText.text = "0";
        score = 0;
        timer = 0;
        currentLevel = 1;
        levelText.text = $"Level: {currentLevel}";

        currentMaxHazardsToSpawn = 1;

        ShowHealthBar(true);

        hazardCoroutine = StartCoroutine(SpawnHazards());

        Time.timeScale = 1;

        EnablePlayerControl(); // Включаем управление при старте/рестарте
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 0)
            {
                Resume();
            }
            else if (Time.timeScale == 1)
            {
                Pause();
            }
        }

        if (gameOver || victory)
            return;

        timer += Time.deltaTime;

        if (timer >= score + 1)
        {
            score++;
            scoreText.text = score.ToString();
        }

        CheckLevelProgressByTime();

        if (timer >= victoryTime)
        {
            Victory();
        }
    }

    private void CheckLevelProgressByTime()
    {
        if (currentLevel - 1 < levelTimeThresholds.Length)
        {
            if (timer >= levelTimeThresholds[currentLevel - 1])
            {
                currentLevel++;
                levelText.text = $"Level: {currentLevel}";

                currentMaxHazardsToSpawn = Mathf.Min(currentMaxHazardsToSpawn + 1, maxHazardsToSpawn);

                if (hazardCoroutine != null)
                {
                    StopCoroutine(hazardCoroutine);
                }
                hazardCoroutine = StartCoroutine(SpawnHazards());

                if (player != null)
                {
                    var playerScript = player.GetComponent<Player>();
                    if (playerScript != null)
                    {
                        playerScript.Heal();
                    }
                }
            }
        }
    }

    private void Victory()
    {
        victory = true;

        if (player != null)
        {
            var playerController = player.GetComponent<Player>();
            if (playerController != null)
            {
                playerController.enabled = false;
            }
        }

        if (hazardCoroutine != null)
        {
            StopCoroutine(hazardCoroutine);
        }

        ShowHealthBar(false);

        GameObject[] hazards = GameObject.FindGameObjectsWithTag("Hazard");
        foreach (GameObject hazard in hazards)
        {
            Destroy(hazard);
        }

        if (Time.timeScale < 2)
        {
            Resume();
        }

        mainVCam.SetActive(false);
        zoomVCam.SetActive(true);

        gameObject.SetActive(false);
        if (victoryMenu != null)
            victoryMenu.SetActive(true);

        Time.timeScale = 0;
    }

    private void Pause()
    {
        LeanTween.value(1, 0, 0.3f)
                 .setOnUpdate(SetTimeScale)
                 .setIgnoreTimeScale(true);
        pauseMenu.gameObject.SetActive(true);

        ShowHealthBar(false);
    }

    private void Resume()
    {
        LeanTween.value(0, 1, 0.3f)
                 .setOnUpdate(SetTimeScale)
                 .setIgnoreTimeScale(true);
        pauseMenu.gameObject.SetActive(false);

        if (!gameOver && !victory)
        {
            ShowHealthBar(true);
        }
    }

    private void SetTimeScale(float value)
    {
        Time.timeScale = value;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    IEnumerator SpawnHazards()
    {
        while (!gameOver && !victory)
        {
            int maxSpawn = Mathf.Min(currentMaxHazardsToSpawn + currentLevel, maxHazardsToSpawn);
            int hazardToSpawn = Random.Range(1, maxSpawn + 1);

            for (int i = 0; i < hazardToSpawn; i++)
            {
                float x = Random.Range(-7, 7);
                float drag = Random.Range(2f, 5f);

                GameObject hazard = Instantiate(hazardPrefab, new Vector3(x, 11, 0), Quaternion.identity);
                Rigidbody rb = hazard.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearDamping = drag;
                }
            }

            float timeToWait = Random.Range(0.5f, 1.5f) * spawnRateMultipliers[Mathf.Clamp(currentLevel - 1, 0, spawnRateMultipliers.Length - 1)];
            yield return new WaitForSeconds(timeToWait);
        }
    }

    public void Enable()
    {
        gameObject.SetActive(true);
    }

    public void EnablePlayerControl()
    {
        if (player != null)
        {
            var playerController = player.GetComponent<Player>();
            if (playerController != null)
            {
                playerController.enabled = true;
            }
        }
    }

    public void GameOver()
    {
        if (hazardCoroutine != null)
        {
            StopCoroutine(hazardCoroutine);
        }

        gameOver = true;

        ShowHealthBar(false);

        GameObject[] hazards = GameObject.FindGameObjectsWithTag("Hazard");
        foreach (GameObject hazard in hazards)
        {
            Destroy(hazard);
        }

        if (Time.timeScale < 2)
        {
            Resume();
        }

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt(HighScorePreferenceKey, highScore);
            Debug.Log($"New HighScore: {highScore}");
        }

        mainVCam.SetActive(false);
        zoomVCam.SetActive(true);

        gameObject.SetActive(false);
        gameOverMenu.SetActive(true);
        Time.timeScale = 0;
    }

    private void ShowHealthBar(bool show)
    {
        if (healthBarUI != null)
        {
            healthBarUI.SetActive(show);
        }
    }
}
