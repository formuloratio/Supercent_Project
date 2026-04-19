using UnityEngine;

public class Interfaces : MonoBehaviour
{
    public interface IInteractable
    {
        void Interact(PlayerInteraction player);
    }

    public interface IMineable
    {
        bool IsActive { get; }
        // 바위가 파괴되면 true, 아니면 false를 반환하도록 수정
        bool TakeDamage(int damage, Vector3 hitPoint);
    }
}
