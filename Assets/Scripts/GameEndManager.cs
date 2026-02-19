using UnityEngine;

public class GameEndManager : MonoBehaviour
{
    public static GameEndManager Instance { get; private set; }

    private bool gameEnded = false;

    void Awake()
    {
        Instance = this;
    }

    // Called locally (client). Freezes time & shows cursor.
    public void EndGame(string reason)
    {
        if (gameEnded) return;
        gameEnded = true;
        Debug.Log("GAME OVER: " + reason);

        // freeze time for local client
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
