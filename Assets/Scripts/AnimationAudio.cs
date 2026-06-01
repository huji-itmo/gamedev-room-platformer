using UnityEngine;

public class AnimationAudio : MonoBehaviour
{
    public AudioClip clip;
    public AudioSource audioSource;

    public void PlayClip()
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }
}