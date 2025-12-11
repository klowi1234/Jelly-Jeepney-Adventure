using UnityEngine;

public class AudioManager : MonoBehaviour
{

    [Header("----------Audio Source-----------")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;


    [Header("----------Audio Clip-----------")]
    public AudioClip background;
    public AudioClip click;
    public AudioClip leftandright;
    public AudioClip portalin;
    public AudioClip portalout;
    public AudioClip inflate;
    public AudioClip deflate;
    public AudioClip sticky;

    private void Start()
    {
        musicSource.clip = background;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }
}
