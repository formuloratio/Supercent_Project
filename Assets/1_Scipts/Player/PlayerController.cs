using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5.0f; // 장비와 상관없이 플레이어의 기본 이동 속도
    public FloatingJoystick joystick;
    public Animator animator;
    public Transform equipmentPivot;

    private EquipmentDataSO currentEquipment;
    private readonly int isMovingHash = Animator.StringToHash("isMoving");
    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        Vector3 forward = mainCam.transform.forward;
        Vector3 right = mainCam.transform.right;
        forward.y = 0f; right.y = 0f;
        forward.Normalize(); right.Normalize();

        Vector3 direction = (forward * joystick.Vertical) + (right * joystick.Horizontal);

        // [수정] 장비 데이터(SO)를 참조하지 않고, 이 스크립트의 moveSpeed를 바로 사용합니다.
        if (direction.magnitude > 0.1f)
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
            transform.forward = Vector3.Slerp(transform.forward, direction, Time.deltaTime * 10f);
            animator.SetBool(isMovingHash, true);
        }
        else
        {
            animator.SetBool(isMovingHash, false);
        }
    }

    public void ChangeEquipment(EquipmentDataSO newData)
    {
        // 1. 비주얼 담당에게 명령
        var handler = GetComponent<PlayerEquipmentHandler>();
        if (handler != null)
        {
            handler.ChangeVisual(newData.visualPrefab, newData.targetPivotIndex);
        }

        // 2. 데이터 담당에게 명령
        var interaction = GetComponent<PlayerInteraction>();
        if (interaction != null)
        {
            interaction.UpdateEquipmentData(newData);
        }
    }
}