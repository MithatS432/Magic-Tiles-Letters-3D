using UnityEngine;
using UnityEngine.UI;

public class ClickableLetter : MonoBehaviour
{
    public string letterValue;


    [Header("Animation Settings")]
    [SerializeField] private float moveSpeed = 15f;


    [Header("Audio")]
    private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;


    [Header("Internal References")]
    private RectTransform rectTransform;
    private RectTransform targetRect;
    private Button button;
    private CanvasGroup canvasGroup;
    private bool isMoving = false;
    private Vector3 originalScale;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        button = GetComponent<Button>();

        originalScale = rectTransform.localScale;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (button != null)
        {
            button.onClick.AddListener(OnLetterSelected);

            var colors = button.colors;
            colors.disabledColor = colors.normalColor;
            button.colors = colors;
        }

        if (string.IsNullOrEmpty(letterValue))
        {
            letterValue = gameObject.name.Replace("(Clone)", "").Trim();
        }
    }

    void OnLetterSelected()
    {
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }

        LevelManager.Instance.OnLetterClicked(this.gameObject);
    }

    public void MoveToSlot(RectTransform slotRect)
    {
        targetRect = slotRect;

        transform.SetParent(slotRect, true);
        isMoving = true;

        if (button != null)
        {
            button.interactable = false;
            var colors = button.colors;
            colors.disabledColor = colors.normalColor;
            button.colors = colors;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    void Update()
    {
        if (isMoving && targetRect != null)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, Vector2.zero, Time.deltaTime * moveSpeed);
            rectTransform.localRotation = Quaternion.Lerp(rectTransform.localRotation, Quaternion.identity, Time.deltaTime * moveSpeed);

            if (Vector2.Distance(rectTransform.anchoredPosition, Vector2.zero) < 0.1f)
            {
                rectTransform.anchoredPosition = Vector2.zero;
                isMoving = false;

                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.OnLetterArrived();
                }
            }
        }
    }
}