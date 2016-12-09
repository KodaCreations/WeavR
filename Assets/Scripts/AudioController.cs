using UnityEngine;
using System.Collections;

public class AudioController : MonoBehaviour {

    AudioSource audioSource;
    AudioClip audioClip;
    public string filename;

	// Use this for initialization
	void Start () 
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void PlayFile()
    {
        
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void LoadFile(string filename)
    {
        audioSource.clip = (AudioClip)Resources.Load(filename);
    }




}
