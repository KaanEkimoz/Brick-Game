using System.Collections;
using UnityEngine;

//Shakes the object with this component
//Use on camera for screen shake effect
public class ScreenShakeEffect : MonoBehaviour
{
    [SerializeField] private AnimationCurve shakeStrength;
    [SerializeField] private float shakeDuration = 0.5f;

    public void Shake()
    {
        StartCoroutine(Shaking(1f, shakeDuration));
    }

    /// <summary>Longer, stronger shake for big events like Tetris line clears.</summary>
    public void ShakeStrong()
    {
        StartCoroutine(Shaking(2.2f, shakeDuration * 1.4f));
    }

    /// <summary>Very soft, short shake for routine impacts (e.g. hard-drop landing).
    /// Uses a fixed magnitude/duration on purpose — the scene's shakeDuration was
    /// tuned to 0.075 for the celebration shake, which would render this call
    /// invisible if it scaled off the inspector value. 0.18s @ 0.6 magnitude reads
    /// as "weight" without being distracting.</summary>
    public void ShakeLight()
    {
        StartCoroutine(Shaking(0.6f, 0.18f));
    }

    IEnumerator Shaking(float magnitude, float duration)
    {
        Vector3 startPos = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float strength = shakeStrength.Evaluate(elapsedTime / duration) * 0.5f * magnitude;
            transform.position = startPos + Random.insideUnitSphere * strength;
            yield return null;
        }
        transform.position = startPos;
    }
}
