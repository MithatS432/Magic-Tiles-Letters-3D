using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Kelime eşleştiğinde slottaki harflere burst + fade efekti uygular.
/// LevelManager'ın filledLetters listesini referans alır.
/// </summary>
public class LetterMatchVFX : MonoBehaviour
{
    [Header("VFX Settings")]
    [SerializeField] private float burstScale = 1.4f;
    [SerializeField] private float burstDuration = 0.12f;
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private Color flashColor = Color.yellow;

    [Header("Particle (Opsiyonel)")]
    [SerializeField] private GameObject particlePrefab;
    [SerializeField] private RectTransform particleParent;

    public void PlayMatchVFX(List<GameObject> letters)
    {
        foreach (GameObject go in letters)
        {
            if (go == null) continue;
            StartCoroutine(BurstAndFade(go));

            if (particlePrefab != null)
            {
                RectTransform slotRect = go.transform.parent as RectTransform;
                if (slotRect != null)
                {
                    SpawnParticle(slotRect);
                }
                else
                {
                    RectTransform rt = go.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        SpawnParticle(rt);
                    }
                }
            }
        }
    }

    private IEnumerator BurstAndFade(GameObject target)
    {
        if (target == null) yield break;

        RectTransform rt = target.GetComponent<RectTransform>();
        CanvasGroup cg = target.GetComponent<CanvasGroup>();
        if (cg == null) cg = target.AddComponent<CanvasGroup>();

        Image img = target.GetComponentInChildren<Image>();
        Color originalColor = img != null ? img.color : Color.white;

        if (img != null) img.color = flashColor;

        Vector3 originalScale = rt != null ? rt.localScale : Vector3.one;
        float t = 0f;

        while (t < burstDuration)
        {
            if (target == null || rt == null) yield break;
            t += Time.deltaTime;
            float ratio = t / burstDuration;
            rt.localScale = Vector3.Lerp(originalScale, originalScale * burstScale, ratio);
            yield return null;
        }

        if (target == null) yield break;
        if (img != null) img.color = originalColor;

        t = 0f;
        while (t < fadeDuration)
        {
            if (target == null || cg == null) yield break;
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }

        if (target != null)
        {
            Destroy(target);
        }
    }

    private void SpawnParticle(RectTransform sourceRect)
    {
        if (sourceRect == null || particlePrefab == null) return;
        GameObject fx = Instantiate(particlePrefab, particleParent);
        if (fx != null)
        {
            fx.transform.position = sourceRect.position;
            fx.transform.localScale = particlePrefab.transform.localScale;
            Destroy(fx, 2f);
        }
    }
}