using TMPro;
using UnityEngine;

// Tracks score/time survived, shows Game Over, supports restart
public class GameManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI scoreText;      // assign TMP text (top-left)
    public TextMeshProUGUI highScoreText;  // assign TMP text (top-right)
    public TextMeshProUGUI gameOverText;   // assign TMP text (center, start inactive)
    public TextMeshProUGUI victoryText;    // assign TMP text for victory message

    [Header("Scoring")]
    public float scoreMultiplier = 1f;     // 1 point per second by default
    public float victoryScore = 150f;      // score needed to win

    [Header("Victory Settings")]
    public GameObject buildingWithDoor;     // assign the building GameObject with the door animation
    public string doorOpenAnimationName = "DoorOpen";  // animation name (check your Animator Controller)

    private float score;
    private float highScore;
    private bool isGameOver;
    private bool isVictory;

    void Start()
    {
        score = 0f;
        
        // load high score from PlayerPrefs
        highScore = PlayerPrefs.GetFloat("HighScore", 0f);
        
        if (gameOverText != null) gameOverText.gameObject.SetActive(false);
        if (victoryText != null) victoryText.gameObject.SetActive(false);
        UpdateScoreUI();
        UpdateHighScoreUI();
    }

    void Update()
    {
        if (!isGameOver && !isVictory)
        {
            score += Time.deltaTime * scoreMultiplier;
            UpdateScoreUI();
            
            // check for victory condition
            if (score >= victoryScore)
            {
                OnVictory();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                // reload current scene
                var s = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                UnityEngine.SceneManagement.SceneManager.LoadScene(s);
            }
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + Mathf.FloorToInt(score).ToString();
        }
        
        // update high score if current score beats it
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetFloat("HighScore", highScore);
            PlayerPrefs.Save();
            UpdateHighScoreUI();
        }
    }
    
    private void UpdateHighScoreUI()
    {
        if (highScoreText != null)
        {
            highScoreText.text = "High Score: " + Mathf.FloorToInt(highScore).ToString();
        }
    }

    // Add score when coins are collected
    public void AddScore(int points)
    {
        if (!isGameOver && !isVictory)
        {
            score += points;
            UpdateScoreUI();
            
            // check for victory condition
            if (score >= victoryScore)
            {
                OnVictory();
            }
        }
    }

    public void OnGameOver()
    {
        if (isGameOver || isVictory) return;
        isGameOver = true;
        
        // check if new high score
        bool newHighScore = score > highScore;
        if (newHighScore)
        {
            highScore = score;
            PlayerPrefs.SetFloat("HighScore", highScore);
            PlayerPrefs.Save();
            UpdateHighScoreUI();
        }
        
        if (gameOverText != null)
        {
            string message = "Game Over\nFinal: " + Mathf.FloorToInt(score);
            if (newHighScore)
            {
                message += "\nNEW HIGH SCORE!";
            }
            message += "\nPress R to Restart";
            gameOverText.text = message;
            gameOverText.gameObject.SetActive(true);
        }
    }
    
    public void OnVictory()
    {
        if (isVictory || isGameOver) return;
        isVictory = true;
        
        // save high score
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetFloat("HighScore", highScore);
            PlayerPrefs.Save();
            UpdateHighScoreUI();
        }
        
        // stop spawning
        var copSpawner = FindObjectOfType<CopSpawner>();
        if (copSpawner != null) copSpawner.enabled = false;
        
        var coinSpawner = FindObjectOfType<CoinSpawner>();
        if (coinSpawner != null) coinSpawner.enabled = false;
        
        var bombSpawner = FindObjectOfType<BoogieBombSpawner>();
        if (bombSpawner != null) bombSpawner.enabled = false;
        
        // destroy all cops
        GameObject[] allCops = GameObject.FindGameObjectsWithTag("Cop");
        foreach (GameObject cop in allCops)
        {
            Destroy(cop);
        }
        
        // open doors
        OpenDoors();
        
        // show victory message
        if (victoryText != null)
        {
            victoryText.text = "VICTORY";
            victoryText.gameObject.SetActive(true);
        }
        
        // freeze player
        var player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = Vector2.zero;
            player.enabled = false;
        }
    }
    
    void OpenDoors()
    {
        if (buildingWithDoor == null) return;
        
        // trigger the door opening animation on the building
        Animator buildingAnimator = buildingWithDoor.GetComponent<Animator>();
        if (buildingAnimator != null)
        {
            // play the animation by name
            buildingAnimator.Play(doorOpenAnimationName);
            Debug.Log("Opening door on building: " + buildingWithDoor.name);
        }
        else
        {
            Debug.LogWarning("Building GameObject doesn't have an Animator component! Make sure it has an Animator with your door opening animation.");
        }
    }
}


