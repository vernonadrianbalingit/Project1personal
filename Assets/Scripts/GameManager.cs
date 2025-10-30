using TMPro;
using UnityEngine;

// Tracks score/time survived, shows Game Over, supports restart
public class GameManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI scoreText;      // assign TMP text (top-left)
    public TextMeshProUGUI gameOverText;   // assign TMP text (center, start inactive)

    [Header("Scoring")]
    public float scoreMultiplier = 1f;     // 1 point per second by default

    private float score;
    private bool isGameOver;

    void Start()
    {
        score = 0f;
        if (gameOverText != null) gameOverText.gameObject.SetActive(false);
        UpdateScoreUI();
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
        if (gameOverText != null)
        {
            gameOverText.text = "Game Over\nFinal: " + Mathf.FloorToInt(score) + "\nPress R to Restart";
            gameOverText.gameObject.SetActive(true);
        }
    }
}


