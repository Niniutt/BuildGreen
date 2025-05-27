using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public struct Goal
{
    public string name;
    public int index;
}

public class FileType : MiniGame
{
    // Logic
    [SerializeField] private Texture2D[] textures = new Texture2D[8];
    [SerializeField] private GameObject buttonPrefab; // Prefab for the buttons
    [SerializeField] private GameObject parent; // Parent GameObject for the buttons
    [SerializeField] private TMP_Text infoText;
    private Button[] buttons = new Button[8];
    private int round = 0;
    private int solutionIndex = 0;

    private const int maxRounds = 3;
    private readonly Goal[] goals = new Goal[4]
    {
        new Goal { name = "Highest fidelity", index = 4 },
        new Goal { name = "Highest resolution", index = 1 },
        new Goal { name = "Lowest size", index = 5 },
        new Goal { name = "Best quality/size ratio", index = 3 },
    }; // Name of file - 1 for array indexing
    private readonly float newRoundDelay = 0.5f;

    // Animations

    private void Awake()
    {
        // New buttons using the Button prefab and placing them in the scene (2 lines, 4 columns)
        for (int i = 0; i < buttons.Length; i++)
        {
            GameObject buttonObj = Instantiate(buttonPrefab, parent.transform);
            // Set position based on index
            float xPos = new int[] { -250, -85, 85, 250 }[i % 4]; // Adjust X position for columns
            float yPos = (i < 4) ? 60 : -60; // Adjust Y position for rows
            buttonObj.transform.localPosition = new Vector3(xPos, yPos, 0);
            buttons[i] = buttonObj.GetComponent<Button>();
        }

        // Add onclick to all frames that call CheckAnswer
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i; // capture the value
            buttons[i].GetComponent<Button>().onClick.AddListener(() => CheckAnswer(index));
        }
    }

    override public void MiniGameInit()
    {
        base.MiniGameInit();

        round = 0;
        InitRound();
    }

    private void InitRound()
    {
        int[] randomOrder = RandomlySortedInts();
        int goalIndex = Random.Range(0, goals.Length); // Randomly select a goal index
        Goal selectedGoal = goals[goalIndex];
        // Initialize the round with the textures and set the buttons' images
        for (int i = 0; i < buttons.Length; i++)
        {
            int randomInt = randomOrder[i];
            buttons[i].GetComponent<Image>().sprite = Sprite.Create(textures[randomInt], new Rect(0, 0, textures[i].width, textures[i].height), new Vector2(0.5f, 0.5f));
            if (randomInt == selectedGoal.index)
            {
                solutionIndex = i;
            }
        }
        // Set goal UI
        infoText.text = selectedGoal.name;
    }

    private int[] RandomlySortedInts()
    {
        // List of randomly sorted ints until 8
        int[] randomOrder = new int[textures.Length];
        for (int i = 0; i < randomOrder.Length; i++)
        {
            randomOrder[i] = i;
        }
        for (int i = 0; i < randomOrder.Length; i++)
        {
            int randomIndex = Random.Range(i, randomOrder.Length);
            // Swap
            int temp = randomOrder[i];
            randomOrder[i] = randomOrder[randomIndex];
            randomOrder[randomIndex] = temp;
        }
        return randomOrder;
    }

    private void CheckAnswer(int index)
    {
        bool correct = index == solutionIndex;
        if (correct)
        {
            UpdateScore(1);
            if (round < maxRounds - 1)
            {
                round++;
                buttons[solutionIndex].GetComponent<Image>().color = Color.green;
                Invoke(nameof(ResetButtonColors), newRoundDelay);
                Invoke(nameof(ToggleButtons), newRoundDelay);
                Invoke(nameof(InitRound), newRoundDelay * 2);
                Invoke(nameof(ToggleButtons), newRoundDelay * 2);
            }
            else
            {
                EndMiniGame(true); // Win condition
            }
        }
        else
        {
            UpdateErrors(1);
        }
    }

    private void ResetButtonColors()
    {
        foreach (Button button in buttons)
        {
            button.GetComponent<Image>().color = Color.white; // Reset to white
        }
    }

    private void ToggleButtons()
    {
        foreach (Button button in buttons)
        {
            button.gameObject.SetActive(button.gameObject.activeSelf == false); // Toggle visibility
        }
    }
}
