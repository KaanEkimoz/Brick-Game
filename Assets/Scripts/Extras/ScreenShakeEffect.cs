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
    /// Roughly 40% of normal magnitude over half the default duration — meant to feel
    /// like weight, not to disorient the player.</summary>
    public void ShakeLight()
    {
        StartCoroutine(Shaking(0.4f, shakeDuration * 0.5f));
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
