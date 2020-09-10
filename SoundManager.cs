using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{

    public AudioSource efxSource; // used to hold the sound effect source
    public AudioSource musicSource; // used to store the background music source
    public static SoundManager instance = null; // initializes a static instance of the sound manager so that there can only be one running at a time

    public float lowPitchRange = 0.95f; // these 2 variables are used to add random variation to the pitch of the sound effects
    public float highPitchRange = 1.05f; // .95 to 1.05 represents a 5% change in pitch, enough to be noticable but not distracting
    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null) // checks if an instance of the sound manager is already running
        {
            instance = this; // if not, assign this instance of the sound manager to the static holder
        }
        else if (instance != this) // if there is already an instance of the sound manager class
        {
            Destroy(gameObject); // destroy this one
        }

        DontDestroyOnLoad(gameObject); // stops the game manager from destroying the sound manager when loading a new level
    }

    public void PlaySingle (AudioClip clip) // used to play a single clip
    {
        efxSource.clip = clip; // assigns the clip passed as a parameter to the sound effects source
        efxSource.Play(); // plays the clip
    }

    public void RandomizeSfx(params AudioClip[] clips) // thje params keyword allows us to send any number of audio clips to the function as long as they are comma seperated
    {
        int randomIndex = Random.Range(0, clips.Length); // used to choose a random clip from the array
        float randomPitch = Random.Range(lowPitchRange, highPitchRange); // used to select a random valuw between the lower and higher boundaries of pitch

        efxSource.pitch = randomPitch; // sets the pitch to the randomly selected value
        efxSource.clip = clips[randomIndex]; // loads the randomly selected clip
        efxSource.Play(); // plays the clip
    }
}
