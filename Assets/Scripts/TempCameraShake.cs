using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempCameraShake : MonoBehaviour
{
    [SerializeField] float duration;
    [SerializeField] float magnitude;

    IEnumerator Shake (float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;

        float elapsed = 0.0f;

        while(elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1, 1f) * magnitude;

            transform.localPosition = new Vector3(x, y, originalPos.z);

            yield return null;
        }

        transform.localPosition = originalPos;

    }

    private void Start()
    {
        StartCoroutine(Shake(duration, magnitude));
    }

}
