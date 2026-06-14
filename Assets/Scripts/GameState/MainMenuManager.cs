using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI")]
    public Button startButton;
    public Button optionsButton;
    public GameObject optionsPanel;
    public Button exitButton;
    public Button backButton;

    public Image masterSoundIcon;
    public Image sfxSoundIcon;

    public Sprite masterSoundOnSprite;
    public Sprite masterSoundOffSprite;
    public Sprite sfxSoundOnSprite;
    public Sprite sfxSoundOffSprite;


    [Header("Fade Overlay (BLACK SCREEN)")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 1f;
    private bool isTransitioning = false;

    private void Awake()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    private void Start()
    {
        StartCoroutine(StartFadeIn());
    }

    public void StartGame()
    {
        if (isTransitioning) return;

        UIAudioManager.Instance.PlayClick();
        StartCoroutine(TransitionToScene());
    }

    public void OpenOptions()
    {
        UIAudioManager.Instance.PlayClick();

        if (optionsPanel != null)
            optionsPanel.SetActive(!optionsPanel.activeSelf);
    }

    public void ExitGame()
    {
        if (isTransitioning) return;

        UIAudioManager.Instance.PlayClick();
        StartCoroutine(FadeOutAndQuit());
    }

    public void BackToMainMenu()
    {
        UIAudioManager.Instance.PlayClick();

        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }



    #region Audio Toggles
    public void ToggleMasterSound()
    {
        UIAudioManager.Instance.ToggleMasterSound();

        masterSoundIcon.sprite =
            UIAudioManager.Instance.IsMasterSoundOn
            ? masterSoundOnSprite
            : masterSoundOffSprite;
    }

    public void ToggleSfxSound()
    {
        UIAudioManager.Instance.ToggleSfxSound();

        sfxSoundIcon.sprite =
            UIAudioManager.Instance.IsSfxSoundOn
            ? sfxSoundOnSprite
            : sfxSoundOffSprite;
    }
    #endregion






    #region Fade
    private IEnumerator TransitionToScene()
    {
        isTransitioning = true;

        yield return StartCoroutine(FadeOut());

        yield return null;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private IEnumerator FadeOutAndQuit()
    {
        isTransitioning = true;

        yield return StartCoroutine(FadeOut());

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private IEnumerator StartFadeIn()
    {
        yield return new WaitForEndOfFrame();
        yield return FadeIn();
    }

    private IEnumerator FadeIn()
    {
        if (canvasGroup == null)
            yield break;

        float t = 0f;
        canvasGroup.alpha = 1f;

        while (t < fadeDuration)
        {
            canvasGroup.alpha = 1f - (t / fadeDuration);
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }

    private IEnumerator FadeOut()
    {
        if (canvasGroup == null)
            yield break;

        float t = 0f;
        canvasGroup.alpha = 0f;

        while (t < fadeDuration)
        {
            canvasGroup.alpha = t / fadeDuration;
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }
    #endregion
}