using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
	private static AudioManager _instance;

    public AudioClip[] layers;
    public AudioClip[] seasons;

    private AudioSource fxSource;
    private AudioSource musicSource;

    private bool musicMarkedForStop;
    private float timerToStop;

    public static AudioManager instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = GameObject.Find("AudioManager").GetComponent<AudioManager>();
			}
			return _instance;
		}
	}

	void Awake ()
	{
        fxSource = GetComponents<AudioSource>()[0];
        musicSource = GetComponents<AudioSource>()[1];

        musicMarkedForStop = false;
        timerToStop = 0f;
    }

	void Update ()
	{
        if (musicMarkedForStop)
        {
            musicSource.volume -= Time.deltaTime;
            if (musicSource.volume <= 0)
            {
                musicSource.Stop();
                musicMarkedForStop = false;
            }
        }
	}

    public void playMusic(int seasonIndex)
    {
        if (!musicSource.isPlaying || musicSource.volume <= 0)
        {
            //musicSource.clip = seasons[seasonIndex];
            musicSource.volume = 0.1f;
            musicSource.Play();
        }
    }

    public void stopMusic ()
    {
        if (musicSource.isPlaying && musicSource.volume > 0)
        {
            //musicMarkedForStop = true;
            musicSource.Stop();
        }
    }

    public void playLayer (int layerIndex)
    {
        fxSource.PlayOneShot(layers[layerIndex]);
    }
}