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
    /// Runs its own coroutine that does NOT consult the inspector shakeStrength
    /// curve. Tuned for "weight" — the player should feel the thump without it
    /// being distracting on a long play session.</summary>
    public void ShakeLight()
    {
        StartCoroutine(LightShaking(0.3f, 0.14f));
    }

    private IEnumerator LightShaking(float magnitude, float duration)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // Linear decay (1 → 0) so the bump is most noticeable at the start.
            float n = Mathf.Clamp01(elapsed / duration);
            float strength = (1f - n) * 0.5f * magnitude;
            transform.position = startPos + (Vector3)(Random.insideUnitCircle * strength);
            yield return null;
        }
        transform.position = startPos;
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
