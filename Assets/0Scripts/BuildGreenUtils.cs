using System.Collections;
using TMPro;
using UnityEngine;
using static Unity.Collections.Unicode;

public class BuildGreenUtils : MonoBehaviour
{
    // The class for static functions
    private class CoroutineRunner : MonoBehaviour { }
    private static CoroutineRunner runner;
    private static void EnsureRunnerExists()
    {
        if (runner == null)
        {
            var go = new GameObject("BuildGreenUtils_CoroutineRunner");
            runner = go.AddComponent<CoroutineRunner>();
            DontDestroyOnLoad(go);
        }
    }

    // Player feedback text on the bottom of the screen that fades away after a few seconds
    public static void ShowFeedback(string text)
    {
        // Find the feedback text object
        var feedbackText = GameObject.Find("FeedbackText");
        {
            var tmp = feedbackText.GetComponent<TMP_Text>();
            tmp.text = text;

            EnsureRunnerExists();
            runner.StartCoroutine(HideAfterDelay(feedbackText, 1f));
        }
    }

    private static IEnumerator HideAfterDelay(GameObject feedbackText, float delay)
    {
        yield return new WaitForSeconds(delay);
        var tmp = feedbackText.GetComponent<TMP_Text>();
        tmp.text = "";
    }
}
