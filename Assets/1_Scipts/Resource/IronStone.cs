using UnityEngine;
using System.Collections;
using static Interfaces;

public class IronStone : MonoBehaviour, IMineable
{
    [Header("Stats")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Settings")]
    public float respawnDelay = 5f;

    private Renderer rockRenderer;
    private Collider rockCollider;
    private AudioSource rockAudioSource; // [추가] 3D 사운드 발생기

    public bool IsActive => currentHealth > 0;

    private void Awake()
    {
        rockRenderer = GetComponent<Renderer>();
        rockCollider = GetComponent<Collider>();
        rockAudioSource = GetComponent<AudioSource>(); // [추가]
    }

    private void OnEnable()
    {
        ResetRock();
    }

    public bool TakeDamage(int damage, Vector3 hitPoint)
    {
        if (!IsActive) return false;

        currentHealth -= damage;
        PlayHitFeedback();

        if (currentHealth <= 0)
        {
            StartCoroutine(DieAndRespawnRoutine());
            return true;
        }

        return false;
    }

    private void PlayHitFeedback()
    {
        transform.localScale = Vector3.one * 0.85f;
        StopCoroutine(nameof(ResetScale));
        StartCoroutine(nameof(ResetScale));
    }

    private IEnumerator ResetScale()
    {
        yield return new WaitForSeconds(0.1f);
        if (IsActive) transform.localScale = Vector3.one;
    }

    private IEnumerator DieAndRespawnRoutine()
    {
        SetAppearance(false);
        yield return new WaitForSeconds(respawnDelay);
        ResetRock();
    }

    private void ResetRock()
    {
        currentHealth = maxHealth;
        transform.localScale = Vector3.one;
        SetAppearance(true);
    }

    private void SetAppearance(bool visible)
    {
        if (rockRenderer != null) rockRenderer.enabled = visible;
        if (rockCollider != null) rockCollider.enabled = visible;
    }
}