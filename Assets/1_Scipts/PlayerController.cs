using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState { Idle, Move, Mining }
    public PlayerState currentState;

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

    private void HandleMovement()
    {
        Vector3 direction = Vector3.forward * joystick.Vertical + Vector3.right * joystick.Horizontal;
        if (direction.magnitude > 0.1f)
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
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