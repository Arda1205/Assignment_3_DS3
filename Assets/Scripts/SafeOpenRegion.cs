using UnityEngine;

public class SafeOpenRegion : MonoBehaviour
{
    public SafeOpenUI safeUI; // drag slider UI script here

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerInteraction pi = other.GetComponent<PlayerInteraction>();
        if (pi != null)
        {
            pi.SetSafeRegion(this);
            safeUI.ShowBar(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerInteraction pi = other.GetComponent<PlayerInteraction>();
        if (pi != null)
        {
            pi.ClearSafeRegion(this);
            safeUI.ShowBar(false);
        }
    }

    public void FillBar(float amount)
    {
        safeUI.Fill(amount);
    }

    public void StopFilling()
    {
        safeUI.StopFill();
    }
}
