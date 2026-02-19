using UnityEngine;

public class GameEndManager : MonoBehaviour
{
    public static GameEndManager Instance;

    private bool gameEnded = false;

    void Awake()
    {
        Instance = this;
    }

    public void EndGame(string reason)
    {
        if (gameEnded) return;
        gameEnded = true;

        Debug.Log("GAME OVER: " + reason);

        // freeze time
        Time.timeScale = 0f;

        // unlock cursor if you want
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
