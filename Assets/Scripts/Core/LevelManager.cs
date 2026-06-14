using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Level Info UI")]
    public TextMeshProUGUI levelText;
    [SerializeField] private int currentLevel = 1;

    [Header("Word Slots")]
    public List<Image> wordSlots = new List<Image>();
    private List<GameObject> filledLetters = new List<GameObject>();
    private int activeSlotCount = 0;


    [Header("Level Info VFX and Audio")]
    public GameObject winEffect;
    public GameObject loseEffect;

    public AudioClip winSound;
    public AudioClip loseSound;
    public AudioClip touchSound;

    [Header("Skills / Power-Ups UI")]
    public Button skill1Button;
    public TextMeshProUGUI skill1Text;
    [SerializeField] private int skill1Count = 3;

    public Button skill2Button;
    public TextMeshProUGUI skill2Text;
    [SerializeField] private int skill2Count = 3;

    public Button skill3Button;
    public TextMeshProUGUI skill3Text;
    [SerializeField] private int skill3Count = 3;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    void Start()
    {
        UpdateActiveSlotCount(new string[] { "" });
        UpdateLevelText();
        InitializeSkills();
    }
    void OnEnable()
    {
        if (WordManager.Instance != null)
            WordManager.Instance.OnWordsAssigned += words => UpdateActiveSlotCount(words);
    }

    void OnDisable()
    {
        if (WordManager.Instance != null)
            WordManager.Instance.OnWordsAssigned -= words => UpdateActiveSlotCount(words);
    }
    private void UpdateActiveSlotCount(string[] words)
    {
        activeSlotCount = 0;
        foreach (string w in words)
            if (w.Length > activeSlotCount) activeSlotCount = w.Length;
    }
    public void OnLetterClicked(GameObject letterObj)
    {
        if (filledLetters.Contains(letterObj)) return;

        RectTransform targetSlot = null;
        for (int i = 0; i < wordSlots.Count; i++)
        {
            if (wordSlots[i].transform.childCount == 0)
            {
                targetSlot = wordSlots[i].GetComponent<RectTransform>();
                break;
            }
        }

        if (targetSlot == null) return;

        filledLetters.Add(letterObj);

        ClickableLetter letterScript = letterObj.GetComponent<ClickableLetter>();
        if (letterScript != null)
            letterScript.MoveToSlot(targetSlot);
    }
    public void OnLetterArrived()
    {
        CheckLiveWord();
    }
    public List<GameObject> GetFilledLetterObjects()
    {
        return new List<GameObject>(filledLetters);
    }
    private bool isGameOver = false;

    private void CheckLiveWord()
    {
        string input = BuildWordFromSlots();

        if (input.Length < 3)
        {
            if (filledLetters.Count >= wordSlots.Count)
            {
                TriggerLose();
            }
            return;
        }

        WordManager.Instance.CheckLive(input, OnWordResult);
    }
    private string BuildWordFromSlots()
    {
        System.Text.StringBuilder sb = new();

        for (int i = 0; i < wordSlots.Count; i++)
        {
            Transform slot = wordSlots[i].transform;

            if (slot.childCount > 0)
            {
                ClickableLetter cl = slot.GetChild(0).GetComponent<ClickableLetter>();
                if (cl != null)
                    sb.Append(cl.letterValue);
            }
        }

        return sb.ToString().Trim();
    }
    private void OnWordResult(bool isMatch, int startIndex, int matchLength)
    {
        if (!isMatch)
        {
            if (filledLetters.Count >= wordSlots.Count)
            {
                TriggerLose();
            }
            return;
        }

        Debug.Log($"[LevelManager] Live match confirmed: index={startIndex}, length={matchLength}");

        ClearMatchedLetters(startIndex, matchLength);
    }

    private void ClearMatchedLetters(int startIndex, int matchLength)
    {
        List<GameObject> matchedLetters = new List<GameObject>();
        for (int i = startIndex; i < startIndex + matchLength; i++)
        {
            if (i < filledLetters.Count)
            {
                matchedLetters.Add(filledLetters[i]);
            }
        }

        Canvas canvas = FindAnyObjectByType<Canvas>();
        Transform tempVFXParent = canvas != null ? canvas.transform : null;

        var vfx = FindAnyObjectByType<LetterMatchVFX>();
        if (vfx != null)
        {
            vfx.PlayMatchVFX(matchedLetters);
        }

        foreach (GameObject go in matchedLetters)
        {
            if (go != null)
            {
                RectTransform rt = go.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = Vector2.zero;
                    rt.localRotation = Quaternion.identity;
                }

                go.transform.SetParent(tempVFXParent, true);

                ClickableLetter cl = go.GetComponent<ClickableLetter>();
                if (cl != null)
                {
                    cl.enabled = false;
                }

                if (vfx == null)
                {
                    Destroy(go);
                }
            }
        }

        foreach (GameObject go in matchedLetters)
        {
            filledLetters.Remove(go);
        }

        ShiftLettersLeft();
    }

    private void ShiftLettersLeft()
    {
        for (int i = 0; i < filledLetters.Count; i++)
        {
            GameObject letterObj = filledLetters[i];
            if (letterObj == null) continue;

            RectTransform targetSlot = wordSlots[i].GetComponent<RectTransform>();
            ClickableLetter letterScript = letterObj.GetComponent<ClickableLetter>();
            if (letterScript != null)
            {
                letterScript.MoveToSlot(targetSlot);
            }
        }
    }

    public void OnLevelWon()
    {
        Debug.Log("[LevelManager] Level Completed!");

        if (winEffect != null)
        {
            Vector3 spawnPos = Vector3.zero;
            Quaternion spawnRot = Quaternion.identity;
            Transform parentTransform = null;

            RectTransform rect = winEffect.GetComponent<RectTransform>();
            if (rect != null)
            {
                Canvas canvas = FindAnyObjectByType<Canvas>();
                if (canvas != null)
                {
                    parentTransform = canvas.transform;
                }
            }

            if (parentTransform == null && Camera.main != null)
            {
                spawnPos = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 5f));
                spawnRot = Camera.main.transform.rotation;
            }

            if (winSound != null)
            {
                AudioSource.PlayClipAtPoint(winSound, spawnPos);
            }

            GameObject effect = Instantiate(winEffect, spawnPos, spawnRot, parentTransform);

            if (rect != null && parentTransform != null)
            {
                RectTransform effectRect = effect.GetComponent<RectTransform>();
                if (effectRect != null)
                {
                    effectRect.anchoredPosition = Vector2.zero;
                    effectRect.localPosition = Vector3.zero;
                    effectRect.localScale = Vector3.one;
                }
            }

            Destroy(effect, 3f);
        }
        else
        {
            if (winSound != null && Camera.main != null)
            {
                Vector3 spawnPos = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 5f));
                AudioSource.PlayClipAtPoint(winSound, spawnPos);
            }
        }

        currentLevel++;
        UpdateLevelText();

        RewardRandomSkill();
    }

    private const int MAX_SKILL_COUNT = 3;

    private void RewardRandomSkill()
    {
        List<int> eligibleSkills = new List<int>();
        if (skill1Count < MAX_SKILL_COUNT) eligibleSkills.Add(1);
        if (skill2Count < MAX_SKILL_COUNT) eligibleSkills.Add(2);
        if (skill3Count < MAX_SKILL_COUNT) eligibleSkills.Add(3);

        if (eligibleSkills.Count == 0)
        {
            Debug.Log("[LevelManager] All skills are at max. No reward given.");
            return;
        }

        int chosen = eligibleSkills[Random.Range(0, eligibleSkills.Count)];

        switch (chosen)
        {
            case 1:
                skill1Count++;
                UpdateSkillText(skill1Text, skill1Count);
                if (skill1Button != null) skill1Button.interactable = true;
                Debug.Log($"[LevelManager] Skill 1 rewarded! Now x{skill1Count}");
                break;
            case 2:
                skill2Count++;
                UpdateSkillText(skill2Text, skill2Count);
                if (skill2Button != null) skill2Button.interactable = true;
                Debug.Log($"[LevelManager] Skill 2 rewarded! Now x{skill2Count}");
                break;
            case 3:
                skill3Count++;
                UpdateSkillText(skill3Text, skill3Count);
                if (skill3Button != null) skill3Button.interactable = true;
                Debug.Log($"[LevelManager] Skill 3 rewarded! Now x{skill3Count}");
                break;
        }
    }

    private void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text = "LEVEL " + currentLevel;
        }
    }

    private void TriggerLose()
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log("[LevelManager] Game Over! Player lost.");

        if (loseEffect != null)
        {
            Vector3 spawnPos = Vector3.zero;
            Quaternion spawnRot = Quaternion.identity;
            Transform parentTransform = null;

            RectTransform rect = loseEffect.GetComponent<RectTransform>();
            if (rect != null)
            {
                Canvas canvas = FindAnyObjectByType<Canvas>();
                if (canvas != null)
                {
                    parentTransform = canvas.transform;
                }
            }

            if (parentTransform == null && Camera.main != null)
            {
                spawnPos = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 5f));
                spawnRot = Camera.main.transform.rotation;
            }

            if (loseSound != null)
            {
                AudioSource.PlayClipAtPoint(loseSound, spawnPos);
            }

            GameObject effect = Instantiate(loseEffect, spawnPos, spawnRot, parentTransform);

            if (rect != null && parentTransform != null)
            {
                RectTransform effectRect = effect.GetComponent<RectTransform>();
                if (effectRect != null)
                {
                    effectRect.anchoredPosition = Vector2.zero;
                    effectRect.localPosition = Vector3.zero;
                    effectRect.localScale = Vector3.one;
                }
            }

            Destroy(effect, 3f);
        }
        else
        {
            if (loseSound != null && Camera.main != null)
            {
                Vector3 spawnPos = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 5f));
                AudioSource.PlayClipAtPoint(loseSound, spawnPos);
            }
        }

        StartCoroutine(RestartLevelAfterDelay(2f));
    }

    private System.Collections.IEnumerator RestartLevelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    #region  ── Skills Power-Ups Logic
    private void InitializeSkills()
    {
        // Auto-find if not assigned in Inspector
        if (skill1Button == null)
        {
            GameObject btnObj = GameObject.Find("Skill1Button") ?? GameObject.Find("Skill 1 Button") ?? GameObject.Find("Skill1");
            if (btnObj != null) skill1Button = btnObj.GetComponent<Button>();
        }
        if (skill1Text == null)
        {
            GameObject txtObj = GameObject.Find("Skill1Text") ?? GameObject.Find("Skill 1 Text") ?? GameObject.Find("Skill1Count");
            if (txtObj != null) skill1Text = txtObj.GetComponent<TextMeshProUGUI>();
        }

        if (skill2Button == null)
        {
            GameObject btnObj = GameObject.Find("Skill2Button") ?? GameObject.Find("Skill 2 Button") ?? GameObject.Find("Skill2");
            if (btnObj != null) skill2Button = btnObj.GetComponent<Button>();
        }
        if (skill2Text == null)
        {
            GameObject txtObj = GameObject.Find("Skill2Text") ?? GameObject.Find("Skill 2 Text") ?? GameObject.Find("Skill2Count");
            if (txtObj != null) skill2Text = txtObj.GetComponent<TextMeshProUGUI>();
        }

        if (skill3Button == null)
        {
            GameObject btnObj = GameObject.Find("Skill3Button") ?? GameObject.Find("Skill 3 Button") ?? GameObject.Find("Skill3");
            if (btnObj != null) skill3Button = btnObj.GetComponent<Button>();
        }
        if (skill3Text == null)
        {
            GameObject txtObj = GameObject.Find("Skill3Text") ?? GameObject.Find("Skill 3 Text") ?? GameObject.Find("Skill3Count");
            if (txtObj != null) skill3Text = txtObj.GetComponent<TextMeshProUGUI>();
        }

        if (skill1Button != null)
        {
            skill1Button.onClick.RemoveAllListeners();
            skill1Button.onClick.AddListener(UseSkill1);
            UpdateSkillText(skill1Text, skill1Count);
        }
        if (skill2Button != null)
        {
            skill2Button.onClick.RemoveAllListeners();
            skill2Button.onClick.AddListener(UseSkill2);
            UpdateSkillText(skill2Text, skill2Count);
        }
        if (skill3Button != null)
        {
            skill3Button.onClick.RemoveAllListeners();
            skill3Button.onClick.AddListener(UseSkill3);
            UpdateSkillText(skill3Text, skill3Count);
        }
    }

    private void UpdateSkillText(TextMeshProUGUI tmpText, int count)
    {
        if (tmpText != null)
        {
            tmpText.text = "x" + count;
        }
    }

    private void UseSkill1()
    {
        if (skill1Count <= 0 || isGameOver) return;

        if (WordManager.Instance != null && WordManager.Instance.MatchRandomActiveWord())
        {
            skill1Count--;
            UpdateSkillText(skill1Text, skill1Count);
            if (skill1Count <= 0 && skill1Button != null)
            {
                skill1Button.interactable = false;
            }
        }
    }

    private void UseSkill2()
    {
        if (skill2Count <= 0 || isGameOver || filledLetters.Count == 0) return;

        if (touchSound != null)
        {
            Vector3 playPos = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
            AudioSource.PlayClipAtPoint(touchSound, playPos);
        }

        int lastIndex = filledLetters.Count - 1;
        GameObject lastLetter = filledLetters[lastIndex];

        if (lastLetter != null)
        {
            lastLetter.transform.SetParent(null);
            Destroy(lastLetter);
        }

        filledLetters.RemoveAt(lastIndex);

        skill2Count--;
        UpdateSkillText(skill2Text, skill2Count);
        if (skill2Count <= 0 && skill2Button != null)
        {
            skill2Button.interactable = false;
        }
    }

    private void UseSkill3()
    {
        if (skill3Count <= 0 || isGameOver) return;

        if (WordManager.Instance == null) return;

        int slotIndex;
        string targetWord = WordManager.Instance.GetRandomUnmatchedWord(out slotIndex);
        if (string.IsNullOrEmpty(targetWord)) return;

        // 1. Clear current slots
        foreach (GameObject go in filledLetters)
        {
            if (go != null) Destroy(go);
        }
        filledLetters.Clear();

        var spawner = FindAnyObjectByType<LetterSpawner>();
        if (spawner == null) return;

        for (int i = 0; i < targetWord.Length; i++)
        {
            char letterChar = targetWord[i];
            GameObject prefab = spawner.GetLetterPrefab(letterChar);
            if (prefab == null) continue;

            RectTransform targetSlot = wordSlots[i].GetComponent<RectTransform>();

            GameObject letterObj = Instantiate(prefab, targetSlot);

            RectTransform letterRt = letterObj.GetComponent<RectTransform>();
            if (letterRt != null)
            {
                letterRt.anchoredPosition = Vector2.zero;
                letterRt.localRotation = Quaternion.identity;
                letterRt.localScale = prefab.GetComponent<RectTransform>().localScale;
            }

            ClickableLetter cl = letterObj.GetComponent<ClickableLetter>();
            if (cl != null)
            {
                cl.letterValue = letterChar.ToString();
                cl.enabled = false;
                var btn = letterObj.GetComponent<Button>();
                if (btn != null) btn.interactable = false;
            }

            filledLetters.Add(letterObj);
        }

        WordManager.Instance.MatchWordDirectly(slotIndex);

        OnWordResult(true, 0, targetWord.Length);

        skill3Count--;
        UpdateSkillText(skill3Text, skill3Count);
        if (skill3Count <= 0 && skill3Button != null)
        {
            skill3Button.interactable = false;
        }
    }
    #endregion
}