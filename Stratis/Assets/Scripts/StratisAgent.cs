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
        // Şimdilik basit reset
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Demo için 6 gözlem dolduralım
        // İleride bunları gerçek değerlerle değiştireceğiz

        // 1-3: düşman pozisyonu yerine 0
        sensor.AddObservation(0f);
        sensor.AddObservation(0f);
        sensor.AddObservation(0f);

        // 4: cover mesafesi
        sensor.AddObservation(0f);

        // 5: health
        sensor.AddObservation(1f);

        // 6: ammo
        sensor.AddObservation(1f);
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

        // Şimdilik shootSignal'i kullanmıyoruz, sadece log atalım
        if (shootSignal > 0.5f)
        {
            // Debug.Log("Shoot!");
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
}
