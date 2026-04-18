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

    // 컴포넌트 참조
    private Renderer rockRenderer;
    private Collider rockCollider;

    public bool IsActive => currentHealth > 0;

    private void Awake()
    {
        // 자기 자신에게 붙은 컴포넌트들을 가져옴
        rockRenderer = GetComponent<Renderer>();
        rockCollider = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        ResetRock();
    }

    public void TakeDamage(int damage, Vector3 hitPoint)
    {
        if (!IsActive) return;

        currentHealth -= damage;
        PlayHitFeedback();

        if (currentHealth <= 0)
        {
            // [핵심] 오브젝트를 끄지 않고, 모습만 감추는 코루틴 실행
            StartCoroutine(DieAndRespawnRoutine());
        }
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
        // 1. "사라짐" 처리: 메쉬와 콜라이더만 비활성화
        SetAppearance(false);

        // 2. 5초 대기 (스크립트가 살아있으므로 정상 작동)
        yield return new WaitForSeconds(respawnDelay);

        // 3. "다시 나타남" 처리
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
        // 자기 자신의 렌더러와 콜라이더를 켜고 끔
        if (rockRenderer != null) rockRenderer.enabled = visible;
        if (rockCollider != null) rockCollider.enabled = visible;

        // 만약 자식 오브젝트에도 메쉬가 있다면 아래 코드 추가 (선택 사항)
        // foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = visible;
    }
}