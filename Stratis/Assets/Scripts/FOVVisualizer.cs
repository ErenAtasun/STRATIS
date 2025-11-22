using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class FOVVisualizer : MonoBehaviour
{
    public StratisAgent agent;
    [Range(0f, 180f)]
    public float viewAngle = 60f;
    public int segments = 30;

    private LineRenderer line;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.loop = true;
        line.widthMultiplier = 0.03f;
        line.positionCount = segments + 2;
    }

    void LateUpdate()
    {
        if (agent == null) return;
        DrawFOV();
    }

    void DrawFOV()
    {
        if (line == null) return;

        Vector3 origin = agent.shootOrigin != null
            ? agent.shootOrigin.position
            : agent.transform.position + Vector3.up * 0.5f;

        Vector3 forward = agent.transform.forward;
        float viewDistance = agent.shootRange;

        line.SetPosition(0, origin);

        float deltaAngle = (viewAngle * 2f) / segments;
        float startAngle = -viewAngle;

        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = startAngle + deltaAngle * i;
            Quaternion rot = Quaternion.AngleAxis(currentAngle, Vector3.up);
            Vector3 dir = rot * forward;

            Vector3 endPoint = origin + dir.normalized * viewDistance;

            // ENGELLERE GÖRE KIRP: Raycast ile duvara kadar çiz
            if (Physics.Raycast(origin, dir, out RaycastHit hit, viewDistance, agent.shootLayers))
            {
                endPoint = hit.point;
            }

            line.SetPosition(i + 1, endPoint);
        }
    }
}
