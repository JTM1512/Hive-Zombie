using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverMenu : MonoBehaviour
{
    // Other scripts call this. There is only one menu in the scene.
    public static GameOverMenu Instance { get; private set; }

    // PauseMenu reads this so Escape cannot unpause a game over.
    public static bool IsShown { get; private set; }
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] string gameSceneName = "SampleScene";
    [SerializeField] string mainMenuSceneName = "MainMenu";
    [SerializeField] TextMeshProUGUI bestHordeText;
    void Awake()
    {
        Instance = this;

        // Hidden until the player actually dies.
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }
    public void Show()
    {
        // Health can hit 0 more than once. Only open the screen once.
        if (IsShown) return;

        IsShown = true;
        StartCoroutine(ShowAfterDeath());
    }
    IEnumerator ShowAfterDeath()
    {
        PlayerMovement movement = FindAnyObjectByType<PlayerMovement>();
        float wait = 0.6f;
        if (movement != null)
        {
            // Stop walking while the death animation plays.
            movement.enabled = false;

            Rigidbody2D body = movement.GetComponent<Rigidbody2D>();
            if (body != null)
                body.linearVelocity = Vector2.zero;

            ZombiePlayerAnimation anim = movement.GetComponent<ZombiePlayerAnimation>();
            if (anim != null)
                wait = anim.PlayDeath();
        }
        // Wait while time is still moving, or the death animation never finishes.
        yield return new WaitForSeconds(wait);

        if (bestHordeText != null && Tracker.Instance != null)
        {
            bestHordeText.text = "Biggest horde: " + Tracker.Instance.Best;
        }
            
        // Freeze the world, then show the panel.
        Time.timeScale = 0f;
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }
    public void Restart()
    {
        // Time was frozen. Turn it back on before loading.
        Time.timeScale = 1f;
        IsShown = false;
        SceneManager.LoadScene(gameSceneName);
    }
    public void MainMenu()
    {
        Time.timeScale = 1f;
        IsShown = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
