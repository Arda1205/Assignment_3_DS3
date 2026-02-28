using UnityEngine;

// Manages local end game state such as freezing movement and unlocking the cursor
public class GameEndManager : MonoBehaviour
{
    public static GameEndManager Instance { get; private set; }

    private bool gameEnded = false;

    void Awake()
    {
        Instance = this;
    }

    [System.Obsolete]

    public void EndGame(string reason)
    {
        if (gameEnded) return;
        gameEnded = true;

        Debug.Log("GAME OVER: " + reason);

        // Stop timer audio
        var timer = FindObjectOfType<CountdownTimer>();
        if (timer != null)
            timer.StopAllAudio();

        // Disable local player movement
        foreach (var player in FindObjectsOfType<PlayerController>())
        {
            if (player.IsOwner)
            {
                player.enabled = false;
                break;
            }
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
