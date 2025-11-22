using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class FOVVisualizer : MonoBehaviour
{
    [Range(0f, 180f)]
    public float viewAngle = 60f;      // Sað/sol açý (toplam açý = 2 * viewAngle)
    public float viewDistance = 6f;    // Kaç metre öteye kadar
    public int segments = 30;          // Yay ne kadar pürüzsüz olsun

    private LineRenderer line;

    void Awake()
    {
        line = GetComponent<LineRenderer>();

        // LineRenderer ayarlarý
        line.useWorldSpace = true;
        line.loop = true;                     // Çizgiyi kapalý þekil yap
        line.widthMultiplier = 0.03f;         // Çizgi kalýnlýðý
        line.positionCount = segments + 2;    // Merkez + yay noktalarý + kapanýþ
    }

    void LateUpdate()
    {
        DrawFOV();
    }

    void DrawFOV()
    {
        if (line == null) return;

        // Merkez, ajan gövdesinin biraz üstünde olsun
        Vector3 origin = transform.position + Vector3.up * 0.05f;

        line.SetPosition(0, origin);

        float deltaAngle = (viewAngle * 2f) / segments;
        Vector3 forward = transform.parent != null ? transform.parent.forward : transform.forward;
        Quaternion startRot = Quaternion.AngleAxis(-viewAngle, Vector3.up);

        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = -viewAngle + deltaAngle * i;
            Quaternion rot = Quaternion.AngleAxis(currentAngle, Vector3.up);
            Vector3 dir = rot * forward;

            Vector3 point = origin + dir.normalized * viewDistance;
            line.SetPosition(i + 1, point);
        }
    }
}
