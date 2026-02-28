using UnityEngine;
using TMPro;
using Unity.Netcode;

// Handles displaying and updating the countdown timer UI during gameplay
public class CountdownTimer : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI timerText;

    [Header("TIP TEXT TO HIDE")]
    public TextMeshProUGUI tipTextToDisable;

    [Header("Audio")]
    public AudioSource countdownAudio;
    public AudioSource alarmAudio;

    private GameManager gm;

    void Start()
    {
        gm = GameManager.Instance;
        UpdateTimerDisplay(0f);

        // If GameManager exists and timer is already running, we might want to start audio
        if (gm != null)
        {
            gm.TimerRunning.OnValueChanged += OnTimerRunningChanged;
            gm.TimerValue.OnValueChanged += OnTimerValueChanged;
        }
    }

    private void OnDestroy()
    {
        if (gm != null)
        {
            gm.TimerRunning.OnValueChanged -= OnTimerRunningChanged;
            gm.TimerValue.OnValueChanged -= OnTimerValueChanged;
        }
    }

    void OnTimerValueChanged(float oldVal, float newVal)
    {
        UpdateTimerDisplay(newVal);
    }

    void OnTimerRunningChanged(bool oldVal, bool newVal)
    {
        if (newVal)
        {
            // timer started
            if (countdownAudio != null) countdownAudio.Play();
            if (alarmAudio != null) alarmAudio.Play();

            if (tipTextToDisable != null)
                tipTextToDisable.gameObject.SetActive(false);
        }
        else
        {
            // timer stopped
            if (countdownAudio != null && countdownAudio.isPlaying) countdownAudio.Stop();
            if (alarmAudio != null && alarmAudio.isPlaying) alarmAudio.Stop();
        }
    }

    void UpdateTimerDisplay(float currentTime)
    {
        int hours = Mathf.FloorToInt(currentTime / 3600f);
        int minutes = Mathf.FloorToInt((currentTime % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);

        timerText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }

    void Update()
    {
        // Read and display the server-driven timer variable each frame (client-side)
        if (gm != null)
        {
            float t = gm.TimerValue.Value;
            UpdateTimerDisplay(t);
        }
    }

    // For GameEndManager or GameManager to stop audio on this client if needed:
    public void StopAllAudio()
    {
        if (countdownAudio != null && countdownAudio.isPlaying) countdownAudio.Stop();
        if (alarmAudio != null && alarmAudio.isPlaying) alarmAudio.Stop();
    }
}
