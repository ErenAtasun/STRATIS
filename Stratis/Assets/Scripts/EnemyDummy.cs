using UnityEngine;

public class EnemyDummy : MonoBehaviour
{
    private Vector3 startPosition;
    private Renderer rend;
    private Color baseColor;

    void Start()
    {
        startPosition = transform.position;
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            baseColor = rend.material.color;
        }
    }

    public void OnHit()
    {
        // Renk deðiþtirerek vurulduðunu belli et
        if (rend != null)
        {
            rend.material.color = Color.red;
            // 0.2 saniye sonra eski rengine dönsün
            Invoke(nameof(ResetColor), 0.2f);
        }

        // Daha dar bir alanda teleport et
        Vector3 randomPos = new Vector3(
            Random.Range(-4f, 4f),    // X
            startPosition.y,          // Y (ground ile ayný)
            Random.Range(3f, 7f)      // Z (çok uzak deðil)
        );

        transform.position = randomPos;
    }

    private void ResetColor()
    {
        if (rend != null)
        {
            rend.material.color = baseColor;
        }
    }
}
