using UnityEngine;
using TMPro;
using System.Collections;

// Controls the summary scene sequence and displays final scores and winner
public class SummaryController : MonoBehaviour
{
    [Header("Player 1 UI")]
    public TextMeshProUGUI p1MoneyText;
    public TextMeshProUGUI p1TimeText;
    public TextMeshProUGUI p1ScoreText;

    [Header("Player 2 UI")]
    public TextMeshProUGUI p2MoneyText;
    public TextMeshProUGUI p2TimeText;
    public TextMeshProUGUI p2ScoreText;

    [Header("Winner")]
    public TextMeshProUGUI winnerText;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip revealSFX;

    void Start()
    {
        StartCoroutine(ShowResultsRoutine());
    }

    IEnumerator ShowResultsRoutine()
    {
        yield return new WaitForSecondsRealtime(2f);

        // Money
        p1MoneyText.text = FinalGameData.player1Money + "$";
        p2MoneyText.text = FinalGameData.player2Money + "$";
        PlaySFX();

        yield return new WaitForSecondsRealtime(2f);

        // Time
        p1TimeText.text = FormatTime(FinalGameData.player1TimeLeft);
        p2TimeText.text = FormatTime(FinalGameData.player2TimeLeft);
        PlaySFX();

        yield return new WaitForSecondsRealtime(2f);

        // Score
        p1ScoreText.text = "Total Score: " + FinalGameData.player1Score;
        p2ScoreText.text = "Total Score: " + FinalGameData.player2Score;
        PlaySFX();

        yield return new WaitForSecondsRealtime(2f);

        // Winner
        if (FinalGameData.player1Score > FinalGameData.player2Score)
            winnerText.text = "Winner is... Player 1!";
        else if (FinalGameData.player2Score > FinalGameData.player1Score)
            winnerText.text = "Winner is... Player 2!";
        else
            winnerText.text = "It's a Tie!";

        PlaySFX();
    }

    string FormatTime(float t)
    {
        int hours = Mathf.FloorToInt(t / 3600f);
        int minutes = Mathf.FloorToInt((t % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(t % 60f);

        return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }

    void PlaySFX()
    {
        if (audioSource != null && revealSFX != null)
            audioSource.PlayOneShot(revealSFX);
    }
}