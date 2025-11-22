using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class StratisAgent : Agent
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotateSpeed = 180f;
    private Rigidbody rb;

    [Header("Combat")]
    public Transform shootOrigin;
    public float shootRange = 20f;
    public LayerMask shootLayers;
    public float hitReward = 1.0f;
    public float missPenalty = -0.01f;

    [Header("Environment References")]
    public Transform enemyTransform;

    [Header("Agent State")]
    public float health = 100f;
    public float maxHealth = 100f;
    public int ammo = 10;
    public int maxAmmo = 10;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.freezeRotation = true; // Yalnızca yatay dönsün
    }

    public override void OnEpisodeBegin()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        health = maxHealth;
        ammo = maxAmmo;

        // Düşmanı rastgele bir yere koy
        if (enemyTransform != null)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-8f, 8f),
                0.5f,
                Random.Range(2f, 10f)
            );
            enemyTransform.position = randomPos;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 1–3: düşmana göre lokal yön + mesafe
        if (enemyTransform != null)
        {
            Vector3 toEnemy = enemyTransform.position - transform.position;
            Vector3 localEnemy = transform.InverseTransformDirection(toEnemy.normalized);

            sensor.AddObservation(localEnemy.x);
            sensor.AddObservation(localEnemy.z);
            sensor.AddObservation(toEnemy.magnitude / 20f); // normalize mesafe
        }
        else
        {
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(1f);
        }

        // 4: health
        sensor.AddObservation(health / maxHealth);

        // 5: ammo
        sensor.AddObservation((float)ammo / maxAmmo);

        // 6: dummy cover mesafesi yoksa şimdilik 0
        sensor.AddObservation(0f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var continuousActions = actions.ContinuousActions;

        float moveX = Mathf.Clamp(continuousActions[0], -1f, 1f);
        float moveZ = Mathf.Clamp(continuousActions[1], -1f, 1f);
        float rotateY = Mathf.Clamp(continuousActions[2], -1f, 1f);
        float shootSignal = Mathf.Clamp(continuousActions[3], -1f, 1f);

        // Hareket
        Vector3 moveDir = transform.right * moveX + transform.forward * moveZ;
        Vector3 velocity = moveDir * moveSpeed;
        rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);

        // Dönme
        transform.Rotate(Vector3.up, rotateY * rotateSpeed * Time.fixedDeltaTime);

        // Ateş
        if (shootSignal > 0.5f)
        {
            Shoot();
        }

        // Küçük step penalty
        AddReward(-0.0005f);
    }
    private void Shoot()
    {
        if (ammo <= 0)
        {
            AddReward(-0.005f);
            Debug.Log("Mermi yok!");
            return;
        }

        ammo--;

        Vector3 origin = shootOrigin != null ? shootOrigin.position : transform.position + Vector3.up * 0.5f;
        Vector3 direction = transform.forward;

        // SADECE SAHNEDE GÖRÜNEN ÇİZGİ (Scene view, Game içinde değil)
        Debug.DrawRay(origin, direction * shootRange, Color.red, 0.2f);

        Ray ray = new Ray(origin, direction);

        if (Physics.Raycast(ray, out RaycastHit hit, shootRange, shootLayers))
        {
            Debug.Log($"Raycast hit: {hit.collider.name}");

            if (hit.collider.CompareTag("Enemy"))
            {
                Debug.Log("ENEMY VURULDU!");
                AddReward(hitReward);

                EnemyDummy dummy = hit.collider.GetComponent<EnemyDummy>();
                if (dummy != null)
                {
                    dummy.OnHit();
                }
            }
            else
            {
                Debug.Log("Başka bir objeye çarptı.");
                AddReward(missPenalty);
            }
        }
        else
        {
            Debug.Log("Raycast BOŞA gitti.");
            AddReward(missPenalty);
        }
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuous = actionsOut.ContinuousActions;

        float moveX = 0f;
        float moveZ = 0f;
        float rotateY = 0f;
        float shoot = 0f;

        // WASD
        if (Input.GetKey(KeyCode.W)) moveZ = 1f;
        if (Input.GetKey(KeyCode.S)) moveZ = -1f;
        if (Input.GetKey(KeyCode.D)) moveX = 1f;
        if (Input.GetKey(KeyCode.A)) moveX = -1f;

        // Q / E ile dön
        if (Input.GetKey(KeyCode.Q)) rotateY = -1f;
        if (Input.GetKey(KeyCode.E)) rotateY = 1f;

        // Space ile ateş sinyali
        if (Input.GetKey(KeyCode.Space)) shoot = 1f;

        continuous[0] = moveX;
        continuous[1] = moveZ;
        continuous[2] = rotateY;
        continuous[3] = shoot;
    }
    private void OnDrawGizmosSelected()
    {
        // Yalnızca Scene view'de agent seçiliyken görünür
        Gizmos.color = Color.yellow;

        float viewAngle = 45f;    // sağ/sol açı
        float viewDistance = shootRange;

        Vector3 origin = shootOrigin != null ? shootOrigin.position : transform.position + Vector3.up * 0.5f;
        Vector3 forward = transform.forward;

        // Orta ray
        Gizmos.DrawRay(origin, forward * viewDistance);

        // Sağ ve sol sınır
        Quaternion rightRot = Quaternion.AngleAxis(viewAngle, Vector3.up);
        Quaternion leftRot = Quaternion.AngleAxis(-viewAngle, Vector3.up);

        Vector3 rightDir = rightRot * forward;
        Vector3 leftDir = leftRot * forward;

        Gizmos.DrawRay(origin, rightDir * viewDistance);
        Gizmos.DrawRay(origin, leftDir * viewDistance);
    }

}
