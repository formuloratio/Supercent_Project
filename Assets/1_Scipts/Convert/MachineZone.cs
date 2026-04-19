using UnityEngine;
using static Interfaces;

public class MachineZone : MonoBehaviour, IInteractable
{
    public enum ZoneType { In, Out }
    public ZoneType zoneType;

    // 두 가지 기계 타입을 모두 담을 수 있도록 변수 선언
    private ConverterMachine converterMachine;
    private BookingDesk bookingDesk;

    private void Start()
    {
        // 부모 오브젝트에서 두 스크립트 중 하나를 찾아옵니다.
        converterMachine = GetComponentInParent<ConverterMachine>();
        bookingDesk = GetComponentInParent<BookingDesk>();

        if (converterMachine == null && bookingDesk == null)
            Debug.LogError($"{gameObject.name}의 부모에게 ConverterMachine이나 BookingDesk가 없습니다!");
    }

    public void Interact(PlayerInteraction player)
    {
        // 구역 타입과 연결된 기계에 따라 알맞은 함수 호출
        if (zoneType == ZoneType.In)
        {
            if (converterMachine != null) converterMachine.Deposit(player);
            else if (bookingDesk != null) bookingDesk.Deposit(player);
        }
        else
        {
            if (converterMachine != null) converterMachine.Collect(player);
            else if (bookingDesk != null) bookingDesk.Collect(player);
        }
    }
}