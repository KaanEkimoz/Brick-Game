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
