using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SearchAlgo : MiniGame
{
    // Logic


    // Animations
    [SerializeField] private RawImage Frame1;
    [SerializeField] private RawImage Frame2;
    [SerializeField] private RawImage Frame3;

    override public void MiniGameInit()
    {
        base.MiniGameInit();
    }

    private void Update()
    {

    }

    new void EndMiniGame(bool win)
    {
        base.EndMiniGame(win);
    }
}

