using UnityEngine;
using static Interfaces;

public class MachineZone : MonoBehaviour, IInteractable
{
    public enum ZoneType { In, Out }
    public ZoneType zoneType;

    private ConverterMachine machine;

    private void Start()
    {
        // 부모 오브젝트에서 '진짜 기계' 스크립트를 찾아옵니다.
        machine = GetComponentInParent<ConverterMachine>();

        if (machine == null)
            Debug.LogError($"{gameObject.name}의 부모에게 ConverterMachine이 없습니다!");
    }

    public void Interact(PlayerInteraction player)
    {
        if (machine == null) return;

        // 구역 타입에 따라 부모 기계의 특정 함수만 호출
        if (zoneType == ZoneType.In)
            machine.Deposit(player);
        else
            machine.Collect(player);
    }
}