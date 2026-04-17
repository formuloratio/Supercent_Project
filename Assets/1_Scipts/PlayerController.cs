using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState { Idle, Move, Mining }
    public PlayerState currentState;
    private Transform camTransform; // 카메라 위치 정보 저장용

    [Header("Movement")]
    public FloatingJoystick joystick;
    public float moveSpeed = 5f;

    [Header("Stats")]
    public int maxOreCapacity = 10;
    public float miningCooldown = 0.5f;
    private float lastMiningTime;
    public bool isInMiningArea; // 선언 추가

    [Header("Equipment")]
    public GameObject pickaxe;
    public GameObject drillObj;
    public GameObject truckObj;

    public StackHandler oreStack;
    public StackHandler cashStack;

    private void Update()
    {
        HandleMovement();
        CheckMining();
    }

    private void Start()
    {
        // 매 프레임 Camera.main을 호출하는 것은 성능에 좋지 않으므로 미리 캐싱합니다.
        if (Camera.main != null)
            camTransform = Camera.main.transform;
    }

    private void HandleMovement()
    {
        if (camTransform == null || joystick == null) return;

        // 1. 카메라가 바라보는 방향(앞, 우측)을 가져옵니다.
        Vector3 forward = camTransform.forward;
        Vector3 right = camTransform.right;

        // 2. 캐릭터가 하늘로 날아오르거나 땅에 박히지 않게 Y축(높이)을 0으로 만듭니다.
        forward.y = 0f;
        right.y = 0f;

        // 3. 방향 데이터만 남기도록 정규화(Normalize) 합니다.
        forward.Normalize();
        right.Normalize();

        // 4. 카메라 기준 '앞'에 조이스틱 수직 입력을, '우측'에 수평 입력을 곱합니다.
        Vector3 direction = (forward * joystick.Vertical) + (right * joystick.Horizontal);

        if (direction.magnitude > 0.1f)
        {
            // 이동: 등속도 이동을 위해 deltaTime을 곱합니다.
            transform.position += direction * moveSpeed * Time.deltaTime;

            // 회전: 이동하려는 방향을 부드럽게 바라보게 합니다.
            transform.forward = direction;

            currentState = PlayerState.Move;
        }
        else
        {
            currentState = PlayerState.Idle;
        }
    }

    private void CheckMining()
    {
        // 채굴 조건 체크
        if (isInMiningArea && Time.time - lastMiningTime > miningCooldown)
        {
            if (oreStack.Count < maxOreCapacity)
            {
                PerformMining();
                lastMiningTime = Time.time;
            }
        }
    }

    private void PerformMining()
    {
        // 주변 IMineable을 찾아 데미지를 줌 (Raycast나 OverlapSphere 활용 가능)
        Collider[] targets = Physics.OverlapSphere(transform.position + transform.forward, 1.5f);
        foreach (var col in targets)
        {
            if (col.TryGetComponent<IMineable>(out var mineable) && mineable.IsActive)
            {
                // 애니메이션 트리거 (기획: 곡괭이/드릴 애니메이션)
                // GetComponent<Animator>().SetTrigger("Attack"); 

                mineable.TakeDamage(1, () => {
                    // 기획: 채굴 시 전용 프리펩을 등 뒤에 쌓음
                    // oreStack.AddStack(oreDisplayPrefab); 
                });
                break; // 한 번에 하나씩 채굴
            }
        }
    }

    public void UpgradeEquipment(int level)
    {
        if (level == 1)
        {
            drillObj.SetActive(true);
            pickaxe.SetActive(false);
            maxOreCapacity = 20;
            miningCooldown = 0.2f;
        }
        else if (level == 2)
        {
            truckObj.SetActive(true);
            drillObj.SetActive(false);
            maxOreCapacity = 40;
        }
    }
}