using TMPro;
using UnityEngine;

// Tracks score/time survived, shows Game Over, supports restart
public class GameManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI scoreText;      // assign TMP text (top-left)
    public TextMeshProUGUI highScoreText;  // assign TMP text (top-right)
    public TextMeshProUGUI gameOverText;   // assign TMP text (center, start inactive)

    [Header("Scoring")]
    public float scoreMultiplier = 1f;     // 1 point per second by default

    private float score;
    private float highScore;
    private bool isGameOver;

    void Start()
    {
        score = 0f;
        
        // load high score from PlayerPrefs
        highScore = PlayerPrefs.GetFloat("HighScore", 0f);
        
        if (gameOverText != null) gameOverText.gameObject.SetActive(false);
        UpdateScoreUI();
        UpdateHighScoreUI();
    }

    void Update()
    {
        if (!isGameOver)
        {
            score += Time.deltaTime * scoreMultiplier;
            UpdateScoreUI();
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
        if (!isGameOver)
        {
            score += points;
            UpdateScoreUI();
        }
    }

    public void OnGameOver()
    {
        if (isGameOver) return;
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
}


