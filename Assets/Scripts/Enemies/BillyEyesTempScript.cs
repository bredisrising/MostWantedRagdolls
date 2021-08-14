using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillyEyesTempScript : MonoBehaviour
{
    [SerializeField] Transform lookAt;
    [SerializeField] LineRenderer laser;
    [SerializeField] Animator animator;
    [SerializeField] GameObject toonExplosion;

    AudioSource eyeAudioSource;
    [SerializeField] AudioClip chargeUpSound;
    [SerializeField] AudioClip laserbeamSound;

    private bool isLaser = false;

    private Vector3 lookingAt;

    private float explosionTime;
    private bool hasExploded;

    [SerializeField] bool explode;

    private void Start()
    {
        eyeAudioSource = GetComponent<AudioSource>();
    }
    void Update()
    {
        if(Time.realtimeSinceStartup > 5 && !animator.GetBool("startBeam"))
        {
            animator.SetBool("startBeam", true);
            eyeAudioSource.PlayOneShot(chargeUpSound);
        }


        lookingAt = new Vector3(lookAt.position.x, lookAt.position.y - .35f, lookAt.position.z + .5f);

        transform.LookAt(lookingAt);
        if (isLaser)
        {
            Laser();
            if(Time.realtimeSinceStartup > explosionTime && !hasExploded && explode)
            {
                Instantiate(toonExplosion, lookingAt + Vector3.up * 1, Quaternion.identity);
                hasExploded = true;
                laser.enabled = false;

            }
        }
    }

    public void Laser()
    {
        laser.SetPosition(1, new Vector3(0, 0, Vector3.Distance(transform.position, lookingAt)));
    }

    public void enableLaser()
    {
        isLaser = true;
        laser.enabled = true;
        explosionTime = Time.realtimeSinceStartup + 1f;

        eyeAudioSource.pitch = 1;
        eyeAudioSource.PlayOneShot(laserbeamSound);

        //StartCoroutine(LineDraw());
    }

    IEnumerator LineDraw()
    {
        float t = 0;
        float time = 1;
        Vector3 orig = laser.GetPosition(0);
        Vector3 orig2 = laser.GetPosition(1);
        laser.SetPosition(1, orig);
        Vector3 newpos;
        for (; t < time; t += Time.deltaTime)
        {
            newpos = Vector3.Lerp(orig, orig2, t / time);
            laser.SetPosition(1, newpos);
            yield return null;
        }
        laser.SetPosition(1, orig2);
    }
}
