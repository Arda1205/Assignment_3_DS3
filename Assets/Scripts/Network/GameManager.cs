using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

// Central server authority that controls timer logic, win conditions, and scene transitions
public class GameManager : NetworkBehaviour
{
    // Singleton reference so other scripts can access the active GameManager
    public static GameManager Instance { get; private set; }

    [Header("Timer")]
    public float defaultStartTime = 60f;

    public NetworkVariable<float> TimerValue = new NetworkVariable<float>(
        0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> TimerRunning = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("UI")]
    // References to UI elements shown on all clients
    public TextMeshProUGUI player1EscapedText;
    public TextMeshProUGUI player2EscapedText;
    public TextMeshProUGUI gameOverText;

    [Header("Audio")]
    public AudioSource gameOverAudioSource;
    public AudioClip gameOverSFX;

    // Tracks which clients have successfully escaped
    private HashSet<ulong> escapedPlayers = new HashSet<ulong>();

    private bool gameEnded = false;

    void Awake()
    {
        Instance = this;
    }

    // Server-only update loop that controls the countdown timer
    [System.Obsolete]
    void Update()
    {
        // Only the server updates the timer
        if (!IsServer || gameEnded) return;

        if (TimerRunning.Value)
        {
            TimerValue.Value -= Time.deltaTime;

            // When timer reaches zero, end the game
            if (TimerValue.Value <= 0f)
            {
                TimerValue.Value = 0f;
                TimerRunning.Value = false;
                EndGameServer();
            }
        }
    }

    // Called by PlayerEscape when a player reaches the exit
    [System.Obsolete]
    public void RegisterEscape(ulong clientId)
    {
        if (!IsServer || gameEnded) return;

        // Ensure the player is only registered once
        if (!escapedPlayers.Contains(clientId))
            escapedPlayers.Add(clientId);

        ShowEscapedClientRpc(clientId);

        FreezePlayerClientRpc(clientId);

        // If all connected players escaped, end the match
        if (escapedPlayers.Count >= NetworkManager.Singleton.ConnectedClients.Count)
        {
            EndGameServer();
        }
    }

    // Server-side logic for ending the game
    [System.Obsolete]
    void EndGameServer()
    {
        if (gameEnded) return;
        gameEnded = true;

        // Remove money from players who did not escape before time ran out
        foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
        {
            var playerObj = kvp.Value.PlayerObject;
            if (playerObj == null) continue;

            var state = playerObj.GetComponent<PlayerState>();
            var escape = playerObj.GetComponent<PlayerEscape>();

            if (state != null && escape != null)
            {
                if (!escape.HasEscaped.Value)
                    state.Money.Value = 0;
            }
        }

        // Calculate final match data before changing scenes
        int p1Money = 0, p2Money = 0;
        float p1Time = 0f, p2Time = 0f;
        int p1Score = 0, p2Score = 0;

        foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
        {
            ulong id = kvp.Key;
            var playerObj = kvp.Value.PlayerObject;
            if (playerObj == null) continue;

            var state = playerObj.GetComponent<PlayerState>();
            var escape = playerObj.GetComponent<PlayerEscape>();
            if (state == null || escape == null) continue;

            // Final score combines money and time left
            int money = state.Money.Value;
            float timeLeft = escape.HasEscaped.Value ? escape.EscapeTimeLeft.Value : 0f;
            int score = money + Mathf.RoundToInt(timeLeft) * 500;

            if (id == 0)
            {
                p1Money = money;
                p1Time = timeLeft;
                p1Score = score;
            }
            else if (id == 1)
            {
                p2Money = money;
                p2Time = timeLeft;
                p2Score = score;
            }
        }

        // Send final results to all clients before scene transition
        SendFinalResultsClientRpc(p1Money, p1Time, p1Score,
                                  p2Money, p2Time, p2Score);

        // Show game over UI and freeze players.
        ShowGameOverClientRpc();
        FreezeAllClientRpc();
    }

    // Displays escaped UI per player on all clients
    [ClientRpc]
    void ShowEscapedClientRpc(ulong clientId)
    {
        if (clientId == 0 && player1EscapedText != null)
            player1EscapedText.gameObject.SetActive(true);

        if (clientId == 1 && player2EscapedText != null)
            player2EscapedText.gameObject.SetActive(true);
    }

    // Displays game over UI and triggers summary scene load
    [ClientRpc]
    void ShowGameOverClientRpc()
    {
        if (gameOverText != null)
            gameOverText.gameObject.SetActive(true);

        if (gameOverAudioSource != null && gameOverSFX != null)
            gameOverAudioSource.PlayOneShot(gameOverSFX);

        StartCoroutine(LoadSummaryAfterDelay());
    }

    // Waits before transitioning scenes so players can see the result
    IEnumerator LoadSummaryAfterDelay()
    {
        yield return new WaitForSecondsRealtime(3f);

        // Only the server initiates scene change
        if (IsServer)
        {
            NetworkManager.SceneManager.LoadScene("Summary", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    // Freezes all players locally
    [ClientRpc]
    [System.Obsolete]
    void FreezeAllClientRpc()
    {
        GameEndManager.Instance?.EndGame("Game Over");
    }

    // Freezes only the player who escaped
    [ClientRpc]
    [System.Obsolete]
    void FreezePlayerClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        GameEndManager.Instance?.EndGame("Escaped");
    }

    // Sends final match results to all clients so summary scene can read them
    [ClientRpc]
    void SendFinalResultsClientRpc(
    int p1Money, float p1Time, int p1Score,
    int p2Money, float p2Time, int p2Score)
    {
        FinalGameData.player1Money = p1Money;
        FinalGameData.player1TimeLeft = p1Time;
        FinalGameData.player1Score = p1Score;

        FinalGameData.player2Money = p2Money;
        FinalGameData.player2TimeLeft = p2Time;
        FinalGameData.player2Score = p2Score;
    }

    // Called by the safe to begin the global countdown
    [ServerRpc(RequireOwnership = false)]
    public void StartTimerServerRpc(ServerRpcParams rpcParams = default)
    {
        TimerValue.Value = defaultStartTime;
        TimerRunning.Value = true;
    }
}