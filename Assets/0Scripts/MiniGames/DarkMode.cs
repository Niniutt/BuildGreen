using UnityEngine;
using UnityEngine.UI;

public class DarkMode : MiniGame
{
    // Logic
    [SerializeField] private Slider r;
    [SerializeField] private Slider g;
    [SerializeField] private Slider b;
    [SerializeField] private Image color;
    [SerializeField] private Image solution;
    private System.Func<float, float> rTransform;
    private System.Func<float, float> gTransform;
    private System.Func<float, float> bTransform;
    private float rMin;
    private float gMin;
    private float bMin;
    private float rValue;
    private float gValue;
    private float bValue;
    private const float tolerance = 0.1f; // Tolerance for checking the answer

    // Animations

    private void Awake()
    {
        foreach (Slider slider in new Slider[] { r, g, b })
        {
            slider.onValueChanged.AddListener(delegate { UpdateColor(); });
        }
    }

    override public void MiniGameInit()
    {
        base.MiniGameInit();

        solution.enabled = false;
        // Make sine + sine functions (of different frequencies) and get the value of the minimum of the sum
        rTransform = GenerateDoubleSineFunction();
        gTransform = GenerateDoubleSineFunction();
        bTransform = GenerateDoubleSineFunction();
        // Find minimum value
        rMin = FindMinimum(rTransform) * 0.75f;
        gMin = FindMinimum(gTransform) * 0.75f + 0.25f;
        bMin = FindMinimum(bTransform) * 0.75f + 0.25f;
        // Set solution color
        solution.color = new Color(rMin, gMin, bMin);
        // Set sliders to random values
        r.value = Random.Range(0f, 1f);
        g.value = Random.Range(0f, 1f);
        b.value = Random.Range(0f, 1f);
        // Update color
        UpdateColor();
    }

    public void CheckAnswer()
    {
        if (rValue < rMin + tolerance &&
                gValue < gMin + tolerance &&
                bValue < bMin + tolerance)
        {
            EndMiniGame(true);
        }
        else
        {
            UpdateErrors(1);
            // Make solution blink three times
            StartCoroutine(BlinkSolution());
        }
    }

    private System.Func<float, float> GenerateDoubleSineFunction()
    {
        float f1 = Random.Range(0.5f, 1.5f) * 2 * Mathf.PI;
        float f2 = Random.Range(0.5f, 1.5f) * 2 * Mathf.PI;
        // random offsets
        float offset1 = Random.Range(0f, 2 * Mathf.PI);
        float offset2 = Random.Range(0f, 2 * Mathf.PI);
        return x => (Mathf.Sin(f1 * x + offset1) + Mathf.Sin(f2 * x + offset2) + 2) * 0.25f;
    }

    private float FindMinimum(System.Func<float, float> t)
    {
        float minValue = 10f;
        for (float x = 0; x < 1; x += 0.01f)
        {
            float value = t(x);
            if (value < minValue)
            {
                minValue = value;
            }
        }
        return minValue;
    }

    private void UpdateColor()
    {
        rValue = rTransform(r.value);
        gValue = gTransform(g.value);
        bValue = bTransform(b.value);
        color.color = new Color(rValue * 0.75f, gValue * 0.75f + 0.25f, bValue * 0.75f + 0.25f);
    }

    private System.Collections.IEnumerator BlinkSolution()
    {
        for (int i = 0; i < 3; i++)
        {
            solution.enabled = true;
            yield return new WaitForSeconds(0.2f);
            solution.enabled = false;
            yield return new WaitForSeconds(0.2f);
        }
    }
}
