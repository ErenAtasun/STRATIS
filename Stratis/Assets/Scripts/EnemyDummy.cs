using UnityEngine;

public class EnemyDummy : MonoBehaviour
{
    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    public void OnHit()
    {
        // Çok basit: vurulunca baþka random bir yere zýplasýn
        Vector3 randomPos = new Vector3(
            Random.Range(-8f, 8f),
            startPosition.y,
            Random.Range(2f, 10f)
        );

        transform.position = randomPos;
    }
}
