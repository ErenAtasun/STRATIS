using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.AI;
using System.Linq;

public class StratisAgent : Agent
{
    public Team team;
    NavMeshAgent nav; PerceptionSystem per; WeaponSystem wep; HealthSystem hp;
    bool inCover; Vector3 coverTarget;

    void Awake()
    {
        nav = GetComponent<NavMeshAgent>();
        per = GetComponent<PerceptionSystem>();
        wep = GetComponent<WeaponSystem>();
        hp = GetComponent<HealthSystem>(); hp.team = team;
    }

    public override void OnEpisodeBegin()
    {
        inCover = false; coverTarget = Vector3.zero; hp.ResetHP(); nav.ResetPath();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation((int)team);
        sensor.AddObservation(hp.Health / hp.maxHealth);
        sensor.AddObservation(inCover ? 1f : 0f);

        var enemies = per.GetEnemies(team, 2).ToList();
        foreach (var e in enemies)
        {
            Vector3 to = e.position - transform.position;
            sensor.AddObservation(Mathf.Clamp01(to.magnitude / 40f));
            sensor.AddObservation(Vector3.SignedAngle(transform.forward, to.normalized, Vector3.up) / 180f);
            sensor.AddObservation(per.enemyVisible && per.currentEnemy == e ? 1f : 0f);
        }
        for (int i = enemies.Count; i < 2; i++) sensor.AddObservation(new float[] { 0, 0, 0 });

        var ally = FindObjectsOfType<StratisAgent>().Where(a => a != this && a.team == team)
                    .OrderBy(a => Vector3.Distance(a.transform.position, transform.position)).FirstOrDefault();
        if (ally)
        {
            Vector3 toA = ally.transform.position - transform.position;
            sensor.AddObservation(Mathf.Clamp01(toA.magnitude / 40f));
            sensor.AddObservation(Vector3.SignedAngle(transform.forward, toA.normalized, Vector3.up) / 180f);
        }
        else sensor.AddObservation(new float[] { 0, 0 });

        var covers = per.GetNearbyCovers(2);
        foreach (var c in covers)
        {
            sensor.AddObservation(Mathf.Clamp01(Vector3.Distance(transform.position, c) / 30f));
            sensor.AddObservation(0f);
        }
        for (int i = covers.Count; i < 2; i++) sensor.AddObservation(new float[] { 0, 0 });
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Continuous: moveX, moveZ
        var moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        var moveZ = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        Vector3 dir = (transform.forward * moveZ + transform.right * moveX).normalized;
        nav.Move(dir * 3.5f * Time.deltaTime);

        // Discrete: fire, takeCover
        bool fire = actions.DiscreteActions[0] == 1;
        bool takeCover = actions.DiscreteActions[1] == 1;

        if (fire && per.enemyVisible && per.currentEnemy != null)
        {
            bool hit = wep.TryFire(per.currentEnemy, team);
            if (hit) AddReward(+0.4f);
        } // else if (fire) AddReward(-0.01f);

        if (takeCover)
        {
            var cs = per.GetNearbyCovers(1);
            if (cs.Count > 0)
            {
                coverTarget = cs[0];
                nav.SetDestination(coverTarget);
                float d = Vector3.Distance(transform.position, coverTarget);
                AddReward(+0.02f * Mathf.Clamp01((15f - d) / 15f));
            }
        }

        inCover = Physics.Raycast(transform.position + Vector3.up * 1.2f, -transform.forward, 1.0f, per.coverMask);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var ca = actionsOut.ContinuousActions; var da = actionsOut.DiscreteActions;
        ca[0] = Input.GetAxis("Horizontal");  // A/D
        ca[1] = Input.GetAxis("Vertical");    // W/S
        da[0] = Input.GetKey(KeyCode.Space) ? 1 : 0; // fire
        da[1] = Input.GetKey(KeyCode.LeftControl) ? 1 : 0; // cover
    }
}
