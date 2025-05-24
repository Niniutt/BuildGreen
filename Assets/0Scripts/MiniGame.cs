using TMPro;
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
        MiniGameStart();
    }

    private void MiniGameStart()
    {
        MiniGameInit();

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
    public void EndMiniGame(bool win, Grabbable grab = null)
    {
        hasStarted = false;
        gameObject.SetActive(false);
        if (win && grab)
        {
            // Fix grabbable's data
            grab.isDeliveryReady.Value = true;
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
}
