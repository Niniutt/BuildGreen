using TMPro;
using Unity.Netcode;
using UnityEngine;

public class MiniGame : MonoBehaviour
{
    private readonly int gameDurationConst = 20;
    private HostController hostController;

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

    virtual public void MiniGameInit()
    {
        if (!hostController) hostController = GetHostController(NetworkManager.Singleton.LocalClientId);
        hostController.ToggleMiniGameState();

        gameDuration = gameDurationConst;
        score = 0;
        errors = 0;

        timer = gameDuration;

        UpdateErrors(0);

        hasStarted = true;

        InvokeRepeating(nameof(UpdateMiniGame), startDelay, repetitionDelay);
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

    public void EndMiniGame(bool win)
    {
        hasStarted = false;
        gameObject.SetActive(false);

        CancelInvoke(nameof(UpdateMiniGame));
        
        if (win)
        {
            BuildGreenUtils.ShowFeedback("Mini-game completed!");
            hostController.UpdateGrabbable();
        }
        else
        {
            BuildGreenUtils.ShowFeedback("Mini-game failed.");
        }
        hostController.ToggleMiniGameState();
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

    private HostController GetHostController(ulong clientId)
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
