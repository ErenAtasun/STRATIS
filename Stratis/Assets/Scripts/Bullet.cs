using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 15f;
    public float lifeTime = 2f;

    private Vector3 direction;
    private bool hasHit = false;
    private StratisAgent owner;  // mermiyi atan agent

    public void Init(StratisAgent owner, Vector3 dir, float overrideSpeed = -1f)
    {
        this.owner = owner;
        direction = dir.normalized;
        if (overrideSpeed > 0f)
            speed = overrideSpeed;
    }

    private void Update()
    {
        // Mermiyi hareket ettir
        transform.position += direction * speed * Time.deltaTime;

        // Ömrü bitti mi?
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f)
        {
            if (!hasHit && owner != null)
            {
                owner.AddReward(owner.missPenalty);
            }
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        if (other.CompareTag("Enemy"))
        {
            hasHit = true;

            if (owner != null)
            {
                owner.AddReward(owner.hitReward);
            }

            EnemyDummy dummy = other.GetComponent<EnemyDummy>();
            if (dummy != null)
            {
                dummy.OnHit();   // burada enemy respawn oluyor
            }

            Destroy(gameObject);
        }
    }
}
