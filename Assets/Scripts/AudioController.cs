using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioController : MonoBehaviour
{

    AudioSource audioSource;
    AudioClip audioClip;
    //public string filename;
    int i = 0;
    int change = 1;
    public List<AudioClip> menuMusic;
    public List<AudioClip> raceMusic;

    [Tooltip("Sound effects that do NOT have their own position in the world")]
    public List<AudioClip> abstractSoundEffects;
    [Tooltip("Sound effect that have a position in the world")]
    public List<AudioClip> objectSoundEffects;

    Dictionary<string, AudioClip> audioClipDict;

    void Start()
    {
        // Keep this object through scenes.
        DontDestroyOnLoad(transform.gameObject);

        audioClipDict = new Dictionary<string, AudioClip>();

        foreach (AudioClip audioClip in menuMusic)
            audioClipDict.Add(audioClip.name, audioClip);
        foreach (AudioClip audioClip in raceMusic)
            audioClipDict.Add(audioClip.name, audioClip);
        foreach (AudioClip audioClip in abstractSoundEffects)
            audioClipDict.Add(audioClip.name, audioClip);
        foreach (AudioClip audioClip in objectSoundEffects)
            audioClipDict.Add(audioClip.name, audioClip);

        audioSource = GetComponent<AudioSource>();
        PlayMainMenuFile(true);
    }

    public AudioClip GetAudioClip(string filename)
    {
        return audioClipDict[filename];
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlayFile(string filename, bool loop)
    {
        audioSource.Stop();
        audioSource.loop = loop;

        audioSource.clip = audioClipDict[filename];
        audioSource.Play();

    }

    public void PlayNextFile()
    {
        audioSource.Stop();
        audioSource.loop = false;
        audioSource.clip = audioClipDict[raceMusic[i].name];
        audioSource.volume = 0.9f;
        audioSource.Play();
        i++;
        i = i % raceMusic.Count;
        Invoke("PlayNextFile", audioSource.clip.length + 0.5f);

    }

    public void PlayMainMenuFile(bool loop)
    {
        audioSource.Stop();
        audioSource.loop = loop;

        audioSource.clip = audioClipDict[menuMusic[0].name];
        audioSource.Play();
    }

    public void Stop()
    {
        audioSource.Stop();
    }

    public void LoadFile(string filename)
    {
        //audioSource.clip = (AudioClip)Resources.Load(filename);
    }
}
