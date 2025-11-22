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

    [Header("Combat / Raycast")]
    public Transform shootOrigin;
    public float shootRange = 20f;
    public LayerMask shootLayers;

    [Header("Environment")]
    public Transform enemyTransform;

    [Header("Agent State")]
    public float maxHealth = 100f;
    public float health = 100f;

    [Header("Ammo / Reload")]
    public int maxAmmo = 5;
    public float reloadDuration = 3f;

    private int currentAmmo;
    private bool isReloading = false;
    private float reloadTimer = 0f;

    [Header("Bullet Visual")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 20f;

    [Header("Rewards")]
    public float hitReward = 1f;
    public float missPenalty = -0.01f;

    [Header("FOV")]
    public FOVVisualizer fovVisualizer;   // FOV script'i buraya atanacak

    // ------------------------------

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.freezeRotation = true;

        health = maxHealth;
        currentAmmo = maxAmmo;
    }

    public override void OnEpisodeBegin()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        health = maxHealth;
        currentAmmo = maxAmmo;
        isReloading = false;
        reloadTimer = 0f;

        if (enemyTransform != null)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-4f, 4f),
                enemyTransform.position.y,
                Random.Range(3f, 7f)
            );
            enemyTransform.position = randomPos;
        }
    }

    private void Update()
    {
        // Reload sayacý
        if (isReloading)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0f)
            {
                FinishReload();
            }
        }
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        if (enemyTransform != null)
        {
            Vector3 toEnemy = enemyTransform.position - transform.position;
            Vector3 localEnemy = transform.InverseTransformDirection(toEnemy.normalized);

            sensor.AddObservation(localEnemy.x);
            sensor.AddObservation(localEnemy.z);
            sensor.AddObservation(toEnemy.magnitude / 20f);
        }
        else
        {
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(1f);
        }

        sensor.AddObservation(health / maxHealth);
        sensor.AddObservation((float)currentAmmo / maxAmmo);
        sensor.AddObservation(isReloading ? 1f : 0f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var c = actions.ContinuousActions;

        float moveX = Mathf.Clamp(c[0], -1f, 1f);
        float moveZ = Mathf.Clamp(c[1], -1f, 1f);
        float rotateY = Mathf.Clamp(c[2], -1f, 1f);
        float shootSignal = Mathf.Clamp(c[3], -1f, 1f);

        Vector3 moveDir = transform.right * moveX + transform.forward * moveZ;
        Vector3 vel = moveDir * moveSpeed;
        rb.velocity = new Vector3(vel.x, rb.velocity.y, vel.z);

        transform.Rotate(Vector3.up, rotateY * rotateSpeed * Time.fixedDeltaTime);

        if (shootSignal > 0.5f)
            Shoot();

        AddReward(-0.0005f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var c = actionsOut.ContinuousActions;

        float moveX = 0f;
        float moveZ = 0f;
        float rotateY = 0f;
        float shoot = 0f;

        if (Input.GetKey(KeyCode.W)) moveZ = 1f;
        if (Input.GetKey(KeyCode.S)) moveZ = -1f;
        if (Input.GetKey(KeyCode.D)) moveX = 1f;
        if (Input.GetKey(KeyCode.A)) moveX = -1f;

        if (Input.GetKey(KeyCode.Q)) rotateY = -1f;
        if (Input.GetKey(KeyCode.E)) rotateY = 1f;

        // cooldown yok: Space'e bastýðýn anda mermi çýkar
        if (Input.GetKeyDown(KeyCode.Space)) shoot = 1f;

        c[0] = moveX;
        c[1] = moveZ;
        c[2] = rotateY;
        c[3] = shoot;
    }

    // ---------------- SHOOT ----------------

    private void Shoot()
    {
        // Reload sýrasýnda ateþ yok
        if (isReloading)
            return;

        // Mermi yoksa reload baþlat
        if (currentAmmo <= 0)
        {
            StartReload();
            return;
        }

        // Mermi düþür
        currentAmmo--;

        // Çýkýþ noktasý
        Vector3 origin = shootOrigin != null
            ? shootOrigin.position
            : transform.position + Vector3.up * 0.5f;

        // Varsayýlan yön: ajan nereye bakýyorsa orasý
        Vector3 direction = transform.forward;

        // Eðer enemy varsa ve FOV içindeyse, yönü enemy'e çevir
        float allowedAngle = 60f;
        if (fovVisualizer != null)
            allowedAngle = fovVisualizer.viewAngle;

        if (enemyTransform != null)
        {
            Vector3 toEnemy = (enemyTransform.position + Vector3.up * 0.5f) - origin;
            float angle = Vector3.Angle(transform.forward, toEnemy);

            if (angle <= allowedAngle)
            {
                direction = toEnemy.normalized;
            }
        }

        // ---- SADECE GÖRSEL MERMÝ SPAWN ----
        if (bulletPrefab != null)
        {
            Quaternion rot = Quaternion.LookRotation(direction, Vector3.up);
            GameObject b = Instantiate(bulletPrefab, origin, rot);

            Bullet bullet = b.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.Init(this, direction, bulletSpeed);
            }
        }

        // Mermi bittiyse reload
        if (currentAmmo <= 0)
        {
            StartReload();
        }
    }

    private void StartReload()
    {
        isReloading = true;
        reloadTimer = reloadDuration;
    }

    private void FinishReload()
    {
        isReloading = false;
        currentAmmo = maxAmmo;
    }

    // ------ UI GETTERS ------

    public int GetCurrentAmmo() => currentAmmo;
    public bool IsReloading() => isReloading;
    public float GetReloadRemainingTime() => reloadTimer;
}
