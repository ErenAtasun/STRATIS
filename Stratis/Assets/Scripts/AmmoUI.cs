using UnityEngine;
using TMPro;

public class AmmoUI : MonoBehaviour
{
    public StratisAgent agent;      // referans
    public TextMeshProUGUI ammoText;

    void Update()
    {
        if (agent == null || ammoText == null) return;

        // agent içindeki ammo deðiþkenlerini okuyalým
        int max = agent.maxAmmo;
        // currentAmmo private, o yüzden bir getter ekleyeceðiz
        int current = agent.GetCurrentAmmo();
        bool reloading = agent.IsReloading();
        float reloadLeft = agent.GetReloadRemainingTime();

        if (reloading)
        {
            ammoText.text = $"Reloading... {reloadLeft:F1}s";
        }
        else
        {
            ammoText.text = $"Ammo: {current} / {max}";
        }
    }
}
