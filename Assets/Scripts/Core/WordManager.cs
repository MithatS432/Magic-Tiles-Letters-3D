using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class WordManager : MonoBehaviour
{
    public static WordManager Instance { get; private set; }

    public event System.Action<string[]> OnWordsAssigned;

    public event System.Action<int> OnWordMatchedVisual;

    [Header("Word Datas")]
    [SerializeField] private WordData[] wordDataSets;

    [Header("UI Slots")]
    [SerializeField] private TextMeshProUGUI[] wordDisplaySlots;

    [Header("Match Audio")]
    [SerializeField] private AudioSource matchAudioSource;
    [SerializeField] private AudioClip matchSound;

    private int currentDataSetIndex = 0;
    private List<string> remainingWords = new List<string>();
    private string[] activeWords = new string[3];
    private bool[] matchedFlags = new bool[3];
    private bool isProcessingMatch = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        LoadDataSet(currentDataSetIndex);
        AssignWordsToSlots();
    }

    public void CheckLive(string input, System.Action<bool, int, int> callback)
    {
        if (isProcessingMatch) return;

        string upper = input.Trim().ToUpper();

        for (int i = 0; i < activeWords.Length; i++)
        {
            if (matchedFlags[i]) continue;

            int index = upper.IndexOf(activeWords[i]);
            if (index != -1)
            {
                isProcessingMatch = true;
                OnWordMatched(i);
                callback?.Invoke(true, index, activeWords[i].Length);
                isProcessingMatch = false;
                return;
            }
        }

        callback?.Invoke(false, -1, 0);
    }

    public string GetActiveWord(int slotIndex) => activeWords[slotIndex];


    private void LoadDataSet(int index)
    {
        while (index < wordDataSets.Length)
        {
            var words = wordDataSets[index].GetValidWords();
            if (words.Count > 0)
            {
                remainingWords = words;
                ShuffleList(remainingWords);
                return;
            }
            index++;
            currentDataSetIndex = index;
        }
        Debug.LogWarning("[WordManager] No valid datasets left.");
    }

    private void AssignWordsToSlots()
    {
        for (int i = 0; i < matchedFlags.Length; i++)
            matchedFlags[i] = false;

        for (int i = 0; i < activeWords.Length; i++)
        {
            activeWords[i] = DrawNextWord();
            RefreshSlotUI(i, matched: false);
        }

        OnWordsAssigned?.Invoke((string[])activeWords.Clone());
    }

    private void OnWordMatched(int slotIndex)
    {
        Debug.Log($"[WordManager] Matched: {activeWords[slotIndex]}");

        matchedFlags[slotIndex] = true;

        RefreshSlotUI(slotIndex, matched: true);

        PlayMatchSound();

        OnWordMatchedVisual?.Invoke(slotIndex);

        bool allDone = System.Array.TrueForAll(matchedFlags, f => f);
        if (!allDone) return;

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnLevelWon();
        }

        StartCoroutine(AdvanceAfterDelay(1.0f));
    }

    private IEnumerator AdvanceAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (remainingWords.Count == 0)
        {
            currentDataSetIndex++;
            if (currentDataSetIndex >= wordDataSets.Length)
            {
                OnAllSetsCompleted();
                yield break;
            }
            LoadDataSet(currentDataSetIndex);
        }

        AssignWordsToSlots();
    }

    private void OnAllSetsCompleted()
    {
        Debug.Log("[WordManager] All sets completed. Restarting from first dataset.");
        currentDataSetIndex = 0;
        LoadDataSet(currentDataSetIndex);
        AssignWordsToSlots();
    }

    private string DrawNextWord()
    {
        if (remainingWords.Count == 0) return string.Empty;
        string word = remainingWords[remainingWords.Count - 1];
        remainingWords.RemoveAt(remainingWords.Count - 1);
        return word;
    }

    private void RefreshSlotUI(int index, bool matched)
    {
        if (wordDisplaySlots == null || index >= wordDisplaySlots.Length) return;
        if (wordDisplaySlots[index] == null) return;

        string word = activeWords[index];

        wordDisplaySlots[index].text = matched
            ? $"<s>{word}</s>"
            : word;
    }

    private void PlayMatchSound()
    {
        if (matchAudioSource != null && matchSound != null)
            matchAudioSource.PlayOneShot(matchSound);
    }

    private void ShuffleList(List<string> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }

    #region  Skills / Power-Ups Helper Methods 
    public bool MatchRandomActiveWord()
    {
        List<int> unmatchedIndices = new List<int>();
        for (int i = 0; i < activeWords.Length; i++)
        {
            if (!matchedFlags[i] && !string.IsNullOrEmpty(activeWords[i]))
            {
                unmatchedIndices.Add(i);
            }
        }

        if (unmatchedIndices.Count == 0) return false;

        int randomIndex = unmatchedIndices[Random.Range(0, unmatchedIndices.Count)];
        OnWordMatched(randomIndex);
        return true;
    }

    public string GetRandomUnmatchedWord(out int slotIndex)
    {
        List<int> unmatchedIndices = new List<int>();
        for (int i = 0; i < activeWords.Length; i++)
        {
            if (!matchedFlags[i] && !string.IsNullOrEmpty(activeWords[i]))
            {
                unmatchedIndices.Add(i);
            }
        }

        if (unmatchedIndices.Count == 0)
        {
            slotIndex = -1;
            return string.Empty;
        }

        slotIndex = unmatchedIndices[Random.Range(0, unmatchedIndices.Count)];
        return activeWords[slotIndex];
    }

    public void MatchWordDirectly(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= activeWords.Length) return;
        OnWordMatched(slotIndex);
    }
    #endregion
}