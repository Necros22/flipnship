using UnityEngine;

public class SFXWall : MonoBehaviour
{
    public AudioSource sfxSource;
    public float playDelay = 0f;

    void Start()
    {
        Invoke(nameof(PlaySFX), playDelay);
    }

    public void PlaySFX()
    {
        sfxSource.Play();
    }
}
