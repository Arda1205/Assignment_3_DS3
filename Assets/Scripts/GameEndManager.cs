using UnityEngine;

public class GameEndManager : MonoBehaviour
{
    public static GameEndManager Instance;

    private bool gameEnded = false;

    private CountdownTimer timer;

    void Awake()
    {
        Instance = this;
    }

    [System.Obsolete]
    void Start()
    {
        timer = FindObjectOfType<CountdownTimer>();
    }

    public void EndGame(string reason)
    {
        if (gameEnded) return;
        gameEnded = true;

        Debug.Log("GAME OVER: " + reason);

        // stop alarm + countdown audio
        if (timer != null)
            timer.StopAllAudio();

        // freeze time
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
