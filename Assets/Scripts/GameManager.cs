using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public float noiseIncreaseRate = 2f; // how fast noise increases per second
    public float maxNoise = 100f; // when noise hits this, game over
    
    [Header("UI References")]
    public Slider noiseMeterSlider;
    public Text scoreText;
    public Text timeText;
    
    // singleton pattern so other scripts can easily access this
    public static GameManager Instance { get; private set; }
    
    // game state variables
    public float currentNoise = 0f;
    public float gameTime = 0f;
    public int guestsSaved = 0;
    public int repairsCompleted = 0;
    public bool gameOver = false;
    
    void Awake()
    {
        // make sure only one GameManager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // initialize the noise meter
        if (noiseMeterSlider != null)
        {
            noiseMeterSlider.maxValue = maxNoise;
            noiseMeterSlider.value = currentNoise;
        }
    }
    
    void Update()
    {
        if (!gameOver)
        {
            // increase noise over time (simulates party getting louder)
            currentNoise += noiseIncreaseRate * Time.deltaTime;
            
            // update game time
            gameTime += Time.deltaTime;
            
            // update UI
            UpdateUI();
            
            // check for game over condition
            if (currentNoise >= maxNoise)
            {
                GameOver();
            }
        }
    }
    
    void UpdateUI()
    {
        // update noise meter
        if (noiseMeterSlider != null)
        {
            noiseMeterSlider.value = currentNoise;
            
            // change color based on noise level
            Image fillImage = noiseMeterSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                if (currentNoise < 50f)
                {
                    fillImage.color = Color.green; // safe zone
                }
                else if (currentNoise < 75f)
                {
                    fillImage.color = Color.yellow; // warning zone
                }
                else
                {
                    fillImage.color = Color.red; // danger zone
                }
            }
        }
        
        // update score text
        if (scoreText != null)
        {
            int totalScore = Mathf.RoundToInt(gameTime) + (guestsSaved * 10) + (repairsCompleted * 5);
            scoreText.text = "Score: " + totalScore;
        }
        
        // update time text
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(gameTime / 60);
            int seconds = Mathf.FloorToInt(gameTime % 60);
            timeText.text = "Time: " + minutes.ToString("00") + ":" + seconds.ToString("00");
        }
    }
    
    // methods to change noise level (will be called by other scripts)
    public void IncreaseNoise(float amount)
    {
        currentNoise += amount;
        currentNoise = Mathf.Clamp(currentNoise, 0f, maxNoise);
    }
    
    public void DecreaseNoise(float amount)
    {
        currentNoise -= amount;
        currentNoise = Mathf.Clamp(currentNoise, 0f, maxNoise);
    }
    
    // called when player completes a minigame successfully
    public void OnMinigameSuccess()
    {
        DecreaseNoise(15f); // reduce noise when player succeeds
        Debug.Log("Minigame success! Noise reduced.");
    }
    
    // called when player fails a minigame
    public void OnMinigameFailure()
    {
        IncreaseNoise(25f); // increase noise when player fails
        Debug.Log("Minigame failed! Noise increased.");
    }
    
    void GameOver()
    {
        gameOver = true;
        Debug.Log("Game Over! Final Score: " + (Mathf.RoundToInt(gameTime) + (guestsSaved * 10) + (repairsCompleted * 5)));
        
        // TODO: show game over screen
        Time.timeScale = 0f; // pause the game
    }
    
    // restart the game
    public void RestartGame()
    {
        currentNoise = 0f;
        gameTime = 0f;
        guestsSaved = 0;
        repairsCompleted = 0;
        gameOver = false;
        Time.timeScale = 1f;
    }
}
