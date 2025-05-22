using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TextMinimizing : MiniGame
{
    // Logic
    [SerializeField] private TMP_Text sentence;
    private string solution;
    private string current;
    private bool comment;

    private readonly string[] sentences = new string[]{ "print('Yo') % Hi", "array =    ['Yo', 'Lo']", "number = 42"};
    private readonly string[] solutions = new string[] { "print('Yo')", "array=['Yo','Lo']", "number=42" };

    // Animations
    [SerializeField] private TMP_Text mark;
    [SerializeField] private RawImage FrameX;
    [SerializeField] private RawImage FrameS;
    private readonly Color baseColor = Color.white;
    private readonly Color correct = Color.green;
    private readonly Color wrong = Color.red;

    void Start()
    {
        base.Start();

        int index = Random.Range(0, sentences.Length - 1);
        solution = solutions[index];
        sentence.text = sentences[index];
        current = sentences[index];
        comment = false;
        hasStarted = true;

        InvokeRepeating(nameof(AnimateText), 0f, 0.5f); // Alright with hardcoded here
    }

    private void AnimateText()
    {
        mark.enabled = !mark.enabled;
    }

    private void Update()
    {
        if (!hasStarted) return;
        char c = current[0];
        bool isUseful;
        if (c == '%') comment = true;
        if (c == ' ' || comment) isUseful = false;
        else isUseful = true;
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (isUseful)
            {
                FrameX.color = correct;
                RemoveCharacter();
                UpdateScore(1);
            }
            else { FrameX.color = wrong; UpdateErrors(1); }
            FrameS.color = baseColor;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isUseful)
            {
                FrameS.color = correct;
                RemoveCharacter();
                UpdateScore(1);
            }
            else { FrameS.color = wrong; UpdateErrors(1); }
            FrameX.color = baseColor;
        }
        if (current == "")
        {
            EndMiniGame();
            FrameX.color = baseColor;
            FrameS.color = baseColor;
        }
    }

    void RemoveCharacter()
    {
        current = current.Substring(1);
        sentence.text = current;
    }
}
