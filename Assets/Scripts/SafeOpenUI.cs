using UnityEngine;
using UnityEngine.UI;

public class SafeOpenUI : MonoBehaviour
{
    public Slider openBar;
    public float fillTime = 5f; // seconds to reach 100

    [Header("Safe Object")]
    public GameObject safeParent;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip openCompleteSFX;

    private float currentValue = 0f; // 0 → 100
    private bool completed = false;

    void Start()
    {
        currentValue = 0f;
        openBar.minValue = 0f;
        openBar.maxValue = 100f;
        openBar.value = 0f;

        ShowBar(false);
    }

    public void ShowBar(bool show)
    {
        openBar.gameObject.SetActive(show);
    }

    // delta = Time.deltaTime coming from player script
    public void Fill(float delta)
    {
        if (completed) return;

        // how much to add per second to reach 100 in fillTime
        float rate = 100f / fillTime;

        currentValue += rate * delta;
        currentValue = Mathf.Clamp(currentValue, 0f, 100f);

        openBar.value = currentValue;

        if (currentValue >= 100f)
        {
            CompleteOpen();
        }
    }

    public void StopFill()
    {
        // leave empty if you want progress saved
        // OR uncomment to reset when letting go:

        // currentValue = 0f;
        // openBar.value = 0f;
    }

    void CompleteOpen()
    {
        if (completed) return;
        completed = true;

        openBar.value = 100f;

        // play SFX
        if (audioSource != null && openCompleteSFX != null)
            audioSource.PlayOneShot(openCompleteSFX);

        // disable safe
        if (safeParent != null)
            safeParent.SetActive(false);

        ShowBar(false);
    }
}
