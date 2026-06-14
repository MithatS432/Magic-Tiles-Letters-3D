using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WordData", menuName = "Game/Word Data")]
public class WordData : ScriptableObject
{
    [TextArea]
    public List<string> words = new List<string>();

    /// <summary>
    /// 3-6 karakter arası, sadece alfabetik kelimeleri filtreler.
    /// </summary>
    public List<string> GetValidWords()
    {
        List<string> valid = new List<string>();

        foreach (string word in words)
        {
            string cleaned = word.Trim().ToUpper();

            if (cleaned.Length >= 3 && cleaned.Length <= 6 && IsAlpha(cleaned))
                valid.Add(cleaned);
        }

        return valid;
    }

    private bool IsAlpha(string s)
    {
        foreach (char c in s)
        {
            if (!char.IsLetter(c)) return false;
        }
        return true;
    }
}