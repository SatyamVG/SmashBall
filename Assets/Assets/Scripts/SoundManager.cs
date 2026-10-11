using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    public bool sound = true;
    private AudioSource audioSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            sound = PlayerPrefs.GetInt("Sound", 1) == 1;
        }
        else
        {
            Destroy(gameObject);
        }

        audioSource = GetComponent<AudioSource>();
    }

    public void SoundOnOff()
    {
        sound = !sound;
        PlayerPrefs.SetInt("Sound", sound ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void PlaySound(AudioClip clip, float volume)
    {
        if (sound)
            audioSource.PlayOneShot(clip, volume);
    }
}
