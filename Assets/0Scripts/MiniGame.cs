using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class MiniGame : MonoBehaviour
{
    private int gameDuration = 10;
    private int timer = 0;
    private float startDelay = 0f;
    private float repetitionDelay = 1f;
    private int score = 0;
    private int errors = 0;
    private readonly int maxErrors = 3;
    public bool hasStarted = false;

    // UI
    [SerializeField] private TMP_Text timeUI;
    [SerializeField] private TMP_Text scoreUI;
    [SerializeField] private TMP_Text errorUI;

    public void Start()
    {
        MiniGameInit();
    }

    public void MiniGameStart()
    {
        InvokeRepeating(nameof(UpdateMiniGame), startDelay, repetitionDelay);
    }

    private void MiniGameInit(int gameDuration = 10, int score = 0, int errors = 0)
    {
        this.gameDuration = gameDuration;
        this.score = score;
        this.errors = errors;

        timer = this.gameDuration;
        hasStarted = true;

        UpdateErrors(0);
    }

    private void UpdateMiniGame()
    {
        if (hasStarted)
        {
            UpdateTime();
        }
        if (timer == 0 || errors == maxErrors)
        {
            EndMiniGame(false);
        }
    }

    // MAKE THIS SERVER RPC
    public void EndMiniGame(bool win)
    {
        hasStarted = false;
        gameObject.SetActive(false);

        CancelInvoke(nameof(UpdateMiniGame));
        MiniGameInit();
        
        if (win)
        {
            // Get Player
            HostController hc = GetHostController(NetworkManager.Singleton.LocalClientId);
            if (hc == null)
            {
                Debug.LogError("GetHostController not working");
            }
            hc.UpdateGrabbable();
        }
        else
        {
            // Handle failure logic here, e.g., reset the game or notify the player
            Debug.Log("Mini-game failed.");
        }
    }

    public void UpdateTime()
    {
        timer -= 1;
        timeUI.text = "Time left: " + timer;
    }

    public void UpdateScore(int newScore)
    {
        score += newScore;
        scoreUI.text = "Score: " + score;
    }

    public void UpdateErrors(int newErrors)
    {
        errors += newErrors;
        errorUI.text = "Errors: " + errors;
    }

    public HostController GetHostController(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var networkClient))
        {
            var playerObj = networkClient.PlayerObject;
            if (playerObj != null)
            {
                return playerObj.GetComponent<HostController>();
            }
        }

        Debug.LogWarning($"HostController not found for client ID {clientId}.");
        return null;
    }
}
