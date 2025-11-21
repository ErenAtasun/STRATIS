using UnityEngine;

public enum Team { Blue = 0, Red = 1 }

public class HealthSystem : MonoBehaviour
{
    public Team team;
    public float maxHealth = 100f;
    public float Health { get; private set; }
    public bool IsDowned => Health <= 0f;
    public System.Action<HealthSystem> OnDowned;

    void Awake() { Health = maxHealth; }
    public void ResetHP() => Health = maxHealth;

    public void ApplyDamage(float dmg)
    {
        if (IsDowned) return;
        Health -= dmg;
        if (Health <= 0f) { Health = 0f; OnDowned?.Invoke(this); }
    }
}
