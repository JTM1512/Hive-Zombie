using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "SampleScene";
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private Button howToPlayButton;
    [SerializeField] private Button howToPlayCloseButton;

    public void Play()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void ShowHowToPlay()
    {
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(true);

        if (howToPlayCloseButton != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(howToPlayCloseButton.gameObject);
    }

    public void HideHowToPlay()
    {
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);

        if (howToPlayButton != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(howToPlayButton.gameObject);
    }

    public void Quit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
