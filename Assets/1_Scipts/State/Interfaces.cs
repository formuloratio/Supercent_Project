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
        void TakeDamage(int damage, UnityEngine.Vector3 hitPoint);
    }
}
