using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ShapeDescriptor
{
    Circle,
    Square,
    Triangle,
    Blue,
    Red,
    Yellow,
}

public struct Option
{
    public string name;
    public ShapeDescriptor shapeDescriptor;
    public bool correct;
    public int index;

    public Option(string name, ShapeDescriptor shapeDescriptor, bool correct, int index)
    {
        this.name = name;
        this.shapeDescriptor = shapeDescriptor;
        this.correct = correct;
        this.index = index;
    }

    public Option(string name, ShapeDescriptor shapeDescriptor)
    {
        this.name = name;
        this.shapeDescriptor = shapeDescriptor;
        this.correct = false;
        this.index = 999; // no index
    }
}

public class SearchAlgo : MiniGame
{
    // Logic
    [SerializeField] private List<Option>[] solutions = new List<Option>[3]
    {
        new List<Option> // Red Triangle
        {
            new Option("blue", ShapeDescriptor.Blue, true, 1),
            new Option("●", ShapeDescriptor.Circle),
            new Option("■", ShapeDescriptor.Square, true, 0),
            new Option("red", ShapeDescriptor.Red),
            new Option("yellow", ShapeDescriptor.Yellow, true, 2),
            new Option("▲", ShapeDescriptor.Triangle)
        },
        new List<Option> // Blue Circle
        {
            new Option("red", ShapeDescriptor.Red, true, 0),
            new Option("yellow", ShapeDescriptor.Yellow, true, 1),
            new Option("blue", ShapeDescriptor.Blue),
            new Option("▲", ShapeDescriptor.Triangle, true, 1),
            new Option("■", ShapeDescriptor.Square),
            new Option("●", ShapeDescriptor.Circle),
        },
        new List<Option> // Yellow Square
        {
            new Option("■", ShapeDescriptor.Square),
            new Option("▲", ShapeDescriptor.Triangle, true, 1),
            new Option("red", ShapeDescriptor.Red, true, 0),
            new Option("yellow", ShapeDescriptor.Yellow),
            new Option("●", ShapeDescriptor.Circle, true, 2),
            new Option("■", ShapeDescriptor.Square),
        }
    };
    // ■●
    private List<Option> currentSolution;
    private int currentIndex = 0; // Current index in the solution path
    [SerializeField] private RawImage[] goals = new RawImage[3];
    [SerializeField] private TMP_Text[] infoTexts = new TMP_Text[3];

    // Animations
    [SerializeField] private Button[] optionButtons = new Button[3];
    [SerializeField] private GameObject[] shapeInfos = new GameObject[7];

    private void Start()
    {
        // Add onclick to all frames that call CheckAnswer
        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i; // capture the value
            optionButtons[i].GetComponent<Button>().onClick.AddListener(() => CheckAnswer(index));
        }
    }

    override public void MiniGameInit()
    {
        base.MiniGameInit();

        int randomGoalIndex = Random.Range(0, goals.Length);
        // Only activate the goal that is selected randomly
        for (int i = 0; i < goals.Length; i++)
        {
            goals[i].gameObject.SetActive(i == randomGoalIndex);
        }
        currentSolution = solutions[randomGoalIndex];
        UpdateInfoTexts();
        for (int i = 0; i < shapeInfos.Length; i++)
        {
            shapeInfos[i].SetActive(true);
        }

        currentIndex = 0;
    }

    private void CheckAnswer(int i)
    {
        // Path to solution is always the same length
        Option chosenOption = currentSolution[i];

        if (!chosenOption.correct || chosenOption.index > currentIndex + 2)
        {
            UpdateErrors(3);
            Debug.Log("Wrong answer: " + chosenOption.name);
        }
        else if (chosenOption.index == currentIndex)
        {
            UpdateScore(1);
            Debug.Log("Correct answer");
            // Check if there is another option in currentSolution that has same index
            // If so, do not increment currentIndex
            if (currentSolution.FindAll(option => option.index == currentIndex).Count > 1)
            {
                Debug.Log("Multiple options with same index, not incrementing currentIndex");
            }
            else
            {
                currentIndex++;
            }
            UpdateOptions(i, chosenOption.shapeDescriptor);
        }
        if (currentIndex == 3 || currentSolution.FindAll(option => option.correct == true).Count == 0)
        {
            Debug.Log("Game finished!");
            EndMiniGame(true);
        }
        Debug.Log("Current index: " + currentIndex);
    }

    private void UpdateOptions(int index, ShapeDescriptor sd)
    {
        // Path to solution is always the same length
        currentSolution.RemoveAt(index);

        // Update options
        UpdateInfoTexts();

        // Update shape pool
        for (int i = 0; i < shapeInfos.Length; i++)
        {
            if (shapeInfos[i].GetComponent<ShapeInfo>().shape == sd || shapeInfos[i].GetComponent<ShapeInfo>().color == sd)
            {
                shapeInfos[i].SetActive(false);
            }
        }
    }

    private void UpdateInfoTexts()
    {
        for (int i = 0; i < infoTexts.Length; i++)
        {
            infoTexts[i].text = currentSolution[i].name;
            switch (currentSolution[i].shapeDescriptor)
            {
                case ShapeDescriptor.Blue:
                    infoTexts[i].color = Color.blue;
                    break;
                case ShapeDescriptor.Red:
                    infoTexts[i].color = Color.red;
                    break;
                case ShapeDescriptor.Yellow:
                    infoTexts[i].color = Color.yellow;
                    break;
                default:
                    infoTexts[i].color = Color.white;
                    break;
            }
        }
    }
}

