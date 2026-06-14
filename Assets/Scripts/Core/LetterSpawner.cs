using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LetterSpawner : MonoBehaviour
{
    [Header("Letter Prefabs")]
    [Tooltip("A-Z için prefab'lar. Prefab adı tek harf olmalı (A, B, C...).")]
    [SerializeField] private GameObject[] letterPrefabs;

    [Header("UI Parent")]
    [SerializeField] private RectTransform container;

    [Header("Spawn Settings")]
    [SerializeField] private float minX = -400f;
    [SerializeField] private float maxX = 400f;
    [SerializeField] private float minY = -300f;
    [SerializeField] private float maxY = 300f;
    [SerializeField] private float minDistanceBetweenLetters = 120f;

    private readonly List<GameObject> spawnedLetters = new();

    private Dictionary<char, GameObject> prefabMap;

    [Header("Spawn Settings")]
    [SerializeField] private int totalLetterCount = 20;
    [SerializeField] private string paddingLetters = "RSLNEAIO";
    void Awake()
    {
        BuildPrefabMap();
    }

    void OnEnable()
    {
        if (WordManager.Instance != null)
            WordManager.Instance.OnWordsAssigned += HandleWordsAssigned;
    }

    void OnDisable()
    {
        if (WordManager.Instance != null)
            WordManager.Instance.OnWordsAssigned -= HandleWordsAssigned;
    }


    /// <summary>
    /// Verilen kelimeler için gereken harfleri spawn eder.
    /// Önceki harfler otomatik temizlenir.
    /// </summary>
    public void SpawnForWords(string[] words)
    {
        ClearSpawnedLetters();

        List<char> pool = BuildLetterPool(words);
        ShuffleList(pool);
        SpawnPool(pool);
    }


    private void HandleWordsAssigned(string[] words)
    {
        SpawnForWords(words);
    }

    /// <summary>
    /// Her kelimeden gelen harfleri birleştirip pool oluşturur.
    /// Örnek: ["CAT","DOG","SUN"] → [C,A,T,D,O,G,S,U,N] + padding harfleri
    /// </summary>
    private List<char> BuildLetterPool(string[] words)
    {
        var pool = new List<char>();

        foreach (string word in words)
        {
            if (string.IsNullOrEmpty(word)) continue;

            foreach (char c in word)
            {
                char upperChar = char.ToUpper(c);

                if (upperChar == 'T' || upperChar == 'V') continue;

                pool.Add(upperChar);
            }
        }

        int safeGuard = 0;
        while (pool.Count < totalLetterCount && safeGuard++ < 100)
        {
            char pad = paddingLetters[Random.Range(0, paddingLetters.Length)];
            pool.Add(pad);
        }

        return pool;
    }

    private void SpawnPool(List<char> pool)
    {
        var usedPositions = new List<Vector2>(pool.Count);

        foreach (char letter in pool)
        {
            if (!prefabMap.TryGetValue(letter, out GameObject prefab))
            {
                Debug.LogWarning($"[LetterSpawner] Prefab bulunamadı: '{letter}'");
                continue;
            }

            Vector2 pos = FindValidPosition(usedPositions);
            usedPositions.Add(pos);

            GameObject obj = Instantiate(prefab, container);
            obj.GetComponent<RectTransform>().anchoredPosition = pos;

            if (obj.TryGetComponent<ClickableLetter>(out var cl))
                cl.letterValue = letter.ToString();

            spawnedLetters.Add(obj);
        }
    }

    private void ClearSpawnedLetters()
    {
        foreach (GameObject go in spawnedLetters)
        {
            if (go != null) Destroy(go);
        }
        spawnedLetters.Clear();
    }

    private Vector2 FindValidPosition(List<Vector2> used)
    {
        const int MaxAttempts = 60;

        for (int i = 0; i < MaxAttempts; i++)
        {
            var candidate = new Vector2(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY));

            bool valid = true;
            foreach (Vector2 p in used)
            {
                if (Vector2.Distance(candidate, p) < minDistanceBetweenLetters)
                {
                    valid = false;
                    break;
                }
            }

            if (valid) return candidate;
        }

        Debug.LogWarning("[LetterSpawner] Could not find valid position, using fallback.");
        return new Vector2(Random.Range(-100f, 100f), Random.Range(-100f, 100f));
    }

    private void BuildPrefabMap()
    {
        prefabMap = new Dictionary<char, GameObject>(letterPrefabs.Length);

        foreach (GameObject prefab in letterPrefabs)
        {
            if (prefab == null) continue;

            string name = prefab.name.Replace("(Clone)", "").Trim().ToUpper();
            if (name.Length == 1 && char.IsLetter(name[0]))
                prefabMap[name[0]] = prefab;
            else
                Debug.LogWarning($"[LetterSpawner] Geçersiz prefab adı: '{prefab.name}'. Tek harf olmalı.");
        }
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }

    public GameObject GetLetterPrefab(char letter)
    {
        char upper = char.ToUpper(letter);
        if (prefabMap != null && prefabMap.TryGetValue(upper, out GameObject prefab))
        {
            return prefab;
        }
        return null;
    }
}