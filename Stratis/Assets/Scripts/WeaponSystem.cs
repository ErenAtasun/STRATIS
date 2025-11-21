using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
    public float fireRate = 6f;
    public float spread = 2.5f;
    public float damage = 20f;
    float _nextTime;

    public bool TryFire(Transform target, Team myTeam)
    {
        if (Time.time < _nextTime || target == null) return false;
        _nextTime = Time.time + 1f / fireRate;

        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 to = (target.position + Vector3.up * 1.5f) - origin;
        Vector3 dir = Quaternion.Euler(Random.Range(-spread, spread), Random.Range(-spread, spread), 0) * to.normalized;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, 50f))
        {
            var hp = hit.collider.GetComponentInParent<HealthSystem>();
            if (hp != null && hp.team != myTeam) { hp.ApplyDamage(damage); return true; }
        }
        return false;
    }
}
