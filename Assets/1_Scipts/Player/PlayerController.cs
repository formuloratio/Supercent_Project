using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public FloatingJoystick joystick;
    public Animator animator;
    public Transform equipmentPivot; // 장비가 붙을 위치

    private EquipmentDataSO currentEquipment;
    private GameObject currentVisual;

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
        float speed = currentEquipment != null ? currentEquipment.moveSpeed : 5f;

        if (direction.magnitude > 0.1f)
        {
            transform.position += direction * speed * Time.deltaTime;
            transform.forward = Vector3.Slerp(transform.forward, direction, Time.deltaTime * 10f); // 부드러운 회전
            animator.SetBool(isMovingHash, true);
        }
        else
        {
            animator.SetBool(isMovingHash, false);
        }
    }

    public void ChangeEquipment(EquipmentDataSO newData)
    {
        currentEquipment = newData;

        // 기존 비주얼 제거
        if (currentVisual != null) Destroy(currentVisual);

        // 새 비주얼 생성
        if (newData.visualPrefab != null)
        {
            currentVisual = Instantiate(newData.visualPrefab, equipmentPivot);
        }

        // 애니메이션 오버라이드 (차량 탑승 등)
        if (newData.animatorOverride != null)
        {
            animator.runtimeAnimatorController = newData.animatorOverride;
        }

        // 데이터 동기화
        GetComponent<PlayerInteraction>().UpdateEquipmentData(newData);
    }
}