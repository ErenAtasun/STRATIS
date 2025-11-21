using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class PerceptionSystem : MonoBehaviour
{
    public float viewAngle = 120f, viewDistance = 35f;
    public LayerMask coverMask;   // Inspector: Cover
    public Transform currentEnemy;
    public bool enemyVisible;

    public IEnumerable<Transform> GetEnemies(Team myTeam, int max = 2)
    {
        var all = FindObjectsOfType<StratisAgent>();
        return all.Where(a => a.team != myTeam && !a.GetComponent<HealthSystem>().IsDowned)
                  .OrderBy(a => Vector3.Distance(a.transform.position, transform.position))
                  .Take(max).Select(a => a.transform);
    }

    void Update()
    {
        enemyVisible = false; currentEnemy = null;
        foreach (var e in GetEnemies(GetComponent<StratisAgent>().team, 1))
        {
            Vector3 dir = (e.position + Vector3.up * 1.5f) - (transform.position + Vector3.up * 1.5f);
            if (dir.magnitude > viewDistance) continue;
            float ang = Vector3.Angle(transform.forward, dir.normalized);
            if (ang > viewAngle * 0.5f) continue;
            if (Physics.Raycast(transform.position + Vector3.up * 1.5f, dir.normalized, out RaycastHit hit, viewDistance))
            {
                if (hit.transform.IsChildOf(e) || hit.transform == e) { enemyVisible = true; currentEnemy = e; break; }
            }
        }
    }

    public List<Vector3> GetNearbyCovers(int max = 2)
    {
        var hits = Physics.OverlapSphere(transform.position, 15f, coverMask);
        return hits.OrderBy(h => Vector3.Distance(transform.position, h.transform.position))
                   .Take(max).Select(h => h.transform.position).ToList();
    }
}
