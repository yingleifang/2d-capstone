using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Rendering;

public class BossController : MonoBehaviour
{
    public Animator animator;
    public AudioSource aud;

    public AudioClip slam;
    public AudioClip slamRoar;
    public AudioClip screech;
    public AudioClip takeDamage;

    public float volume = 4f;

    private void Start()
    {
        animator = GetComponent<Animator>();
        aud = GetComponent<AudioSource>();
    }

    public IEnumerator SlamAnimation()
    {
        animator.SetTrigger("slam");
        aud.PlayOneShot(slamRoar, volume);
        yield return new WaitForSeconds(1f);
        aud.PlayOneShot(slam, volume);
        yield return new WaitForSeconds(1f);

    }

    public IEnumerator DamageAnimation()
    {
        animator.SetTrigger("takeDamage");
        aud.PlayOneShot(takeDamage, volume);
        yield return new WaitForSeconds(3f);

    }

    public void screechSound()
    {
        aud.PlayOneShot(screech, volume);
    }
}
