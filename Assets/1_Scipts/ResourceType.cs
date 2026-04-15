// 자원 타입 정의
public enum ResourceType { IronOre, Handcuff, Cash }

// 상호작용 인터페이스
public interface IInteractable
{
    void OnEnter(PlayerController player);
    void OnExit(PlayerController player);
}

// 채굴 대상 인터페이스
public interface IMineable
{
    void TakeDamage(int damage, System.Action onMined);
    bool IsActive { get; }
}