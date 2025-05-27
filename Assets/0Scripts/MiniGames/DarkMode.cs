using UnityEngine;
using UnityEngine.UI;

public class DarkMode : MiniGame
{
    // Logic
    [SerializeField] private Button[] buttons = new Button[8];

    // Animations

    private void Start()
    {
        MiniGameInit(); // Temporary: Test
    }

    override public void MiniGameInit()
    {
        base.MiniGameInit();

    }

    private void CheckAnswer(int i)
    {

    }
}
