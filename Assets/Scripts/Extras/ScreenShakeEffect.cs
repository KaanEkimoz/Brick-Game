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
        StartCoroutine(Shaking());
    }
    IEnumerator Shaking()
    {
        Vector3 startPos = transform.position;
        float elapsedTime = 0f;
        
        while(elapsedTime < shakeDuration)
        {
            elapsedTime += Time.deltaTime;
            float strength = shakeStrength.Evaluate(elapsedTime / shakeDuration) * 0.5f;
            transform.position = startPos + Random.insideUnitSphere;
            yield return null;
        }
        transform.position = startPos;
    }
}
