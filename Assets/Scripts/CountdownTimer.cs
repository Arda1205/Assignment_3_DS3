using UnityEngine;
using TMPro;
using UnityEngine.InputSystem; // NEW INPUT SYSTEM

public class CountdownTimer : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI timerText;

    [Header("TIP TEXT TO HIDE")]
    public TextMeshProUGUI tipTextToDisable;

    [Header("Audio")]
    public AudioSource countdownAudio;
    public AudioSource alarmAudio;

    private float startTimeSeconds = 60f; 
    private float audioCutoffEarly = 0.25f; // stop audio this many seconds early



    private float currentTime;
    private bool isCounting = false;
    private bool audioStoppedEarly = false;

    void Start()
    {
        currentTime = startTimeSeconds;
        UpdateTimerDisplay();
    }

    void Update()
    {
        // Start countdown on F1 (new input system)
        if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame && !isCounting)
        {
            StartCountdown();
            // don't let the timer tick in the same frame we start it
            // so the display stays at the full start time for one full second
            return;
        }

        if (!isCounting)
            return;

        // count down
        currentTime -= Time.deltaTime;

        // stop audio slightly early
        if (!audioStoppedEarly && currentTime <= audioCutoffEarly)
        {
            audioStoppedEarly = true;

            if (countdownAudio != null && countdownAudio.isPlaying)
                countdownAudio.Stop();
        }

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isCounting = false;

            if (GameEndManager.Instance != null)
                GameEndManager.Instance.EndGame("Time ran out");
        }

        UpdateTimerDisplay();
    }

    public void StartCountdown()
    {
        isCounting = true;
        audioStoppedEarly = false;
        currentTime = Mathf.Max(0f, currentTime); // safety

        if (countdownAudio != null)
            countdownAudio.Play();

        if (alarmAudio != null)
            alarmAudio.Play();

        // hide tip text when countdown begins
        if (tipTextToDisable != null)
            tipTextToDisable.gameObject.SetActive(false);


        UpdateTimerDisplay(); // ensure UI updates immediately to show the start time
    }

    void UpdateTimerDisplay()
    {
        int hours = Mathf.FloorToInt(currentTime / 3600f);
        int minutes = Mathf.FloorToInt((currentTime % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);

        // fixed format: hours:minutes:seconds
        timerText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }

    public void StopAllAudio()
    {
        if (countdownAudio != null && countdownAudio.isPlaying)
            countdownAudio.Stop();

        if (alarmAudio != null && alarmAudio.isPlaying)
            alarmAudio.Stop();
    }

}
