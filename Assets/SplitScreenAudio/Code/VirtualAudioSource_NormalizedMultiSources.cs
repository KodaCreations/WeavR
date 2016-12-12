/************************************************************
 * Created in 2014 by:  LunaArgenteus
 * This software is not free. If you acquired this code without paying for it, please consider supporting me
 * by purchasing it on the Unity Asset Store to help me continue creating awesome stuff!
 * 
 * If you have any questions that are not answered by the readme, you can ask on the official support thread on Unity forums, 
 * forum.unity3d.com/threads/273344/
 * send me a private message, or can email me at LunaArgenteus@gmail.com (Please include the name of this product (Split Screen Audio) in the subject line or I may not respond),
 * but please consult the readme first!
 ************************************************************
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class VirtualAudioSource_NormalizedMultiSources : VirtualAudioSource {
	
	/// <summary>
	/// If true, this overrides panLevel settings (sets to 1)
	/// </summary>
	public bool force3DSoundOnStartup = true;
	
	/// <summary>
	/// The base volume for the sound. Treat it as you would treat the volume for the AudioSource component in the inspector
	/// </summary>
	[RangeAttribute(0f, 1f)]
	public float volume = 1f;
	
	private List<AudioSource> sourceCopies = new List<AudioSource>();
	
	void Awake()
	{
		//if(mySource == null)
		//{
		//	Debug.LogWarning("No AudioSource assigned to this virtual source! An AudioSource must be assigned for in order to function properly!");
		//}
		sourceCopies.Add(mySource);
	}

    public void AddSource()
    {
        sourceCopies.Add(mySource);
    }
	
	protected override void OnEnable()
	{
		if(force3DSoundOnStartup)
		{
			if(mySource != null)
			{
				mySource.spatialBlend = 1;
			}
			else
			{
				Debug.LogWarning("No AudioSource assigned to this virtual source! An AudioSource must be assigned for in order to function properly!");
			}
		}
		base.OnEnable();
		
	}
	protected override void OnDisable()
	{
		base.OnDisable();
		while(sourceCopies.Count > 1) //I don't ever want to remove the original
		{
			if(sourceCopies[sourceCopies.Count-1] != null)
			{
				GameObject.Destroy(sourceCopies[sourceCopies.Count-1].gameObject);
			}
			sourceCopies.RemoveAt(sourceCopies.Count-1);
		}
	}
	
	public override IEnumerator PlayAudioSource(float delay = 0f)
	{
		isCoroutinePlaying = true;
		
		if(mySource == null)
		{
			Debug.LogWarning("No AudioSource assigned to this virtual source! An AudioSource must be assigned for in order to function properly!");
		}
		else if(!mySource.gameObject.activeInHierarchy || !mySource.enabled)
		{
			Debug.LogWarning("Cannot play a disabled AudioSource");
		}
		else
		{
			do
			{
				while(sourceCopies.Count < VirtualAudioListener.allListeners.Count)
				{
					sourceCopies.Add(Instantiate(mySource) as AudioSource);
				}
				/*
				while(sourceCopies.Count > VirtualAudioListener.allListeners.Count && sourceCopies.Count > 1) //I don't ever want to remove the original
				{
					sourceCopies.RemoveAt(sourceCopies.Count-1);
				}
				*/
				int numSourcesToPlay = VirtualAudioListener.allListeners.Count;
				for(int i = 0; i < numSourcesToPlay; ++i)//for(int i = 0; i < sourceCopies.Count; ++i)
				{
					if(sourceCopies[i] != null)
					{
						sourceCopies[i].PlayDelayed(delay);
					}
				}
				
				while(mySource != null && mySource.isPlaying)
				{
					List<VirtualAudioListener> listeners = VirtualAudioListener.allListeners;
					
					for(int i = 0; i < sourceCopies.Count && i < listeners.Count; ++i)
					{
						if(sourceCopies[i] != null)
						{
							try
							{
								sourceCopies[i].transform.position = Quaternion.Inverse(listeners[i].transform.rotation)*(this.transform.position - listeners[i].transform.position) + VirtualAudioListener.sceneAudioListener.transform.position;
								sourceCopies[i].volume = volume/numSourcesToPlay;//1f/sourceCopies.Count; 
								//the volume adjustment is simple here since I can let Unity's built in 3d rolloff handle the weights for each source
								//And since each source has its own volume, I don't need to weigh the volumes against each other to get a single weighted average volume like for the 2d sound version
							}
							catch(IndexOutOfRangeException e)
							{
								e.ToString(); //for some reason, unity gets internal compiler errors if I choose not to use 'e' at all
								sourceCopies[i].volume = 0; //hush the sound if there is no one to listen to it
							}
						}
					}
					yield return null;
				}
			}while(loopCoroutine && !pauseAudio && VirtualAudioListener.allListeners.Count > 0 && mySource != null && mySource.gameObject.activeInHierarchy && mySource.enabled);
		}
		
		isCoroutinePlaying = false;
		
	}
	
	/// <summary>
	/// Removes all AudioSource duplicates from the scene
	/// </summary>
	public void CleanUpDuplicateSources()
	{
		while(sourceCopies.Count > VirtualAudioListener.allListeners.Count && sourceCopies.Count > 1) //I don't ever want to remove the original
		{
			if(sourceCopies[sourceCopies.Count-1] != null)
			{
				GameObject.Destroy(sourceCopies[sourceCopies.Count-1].gameObject);
			}
			sourceCopies.RemoveAt(sourceCopies.Count-1);
		}
	}
	
#region consistency methods
	public override bool bypassEffects
	{
		get
		{
			return mySource.bypassEffects;
		}
		set
		{
			for(int i = 0; i < sourceCopies.Count; ++i)
			{
				if(sourceCopies[i] != null)
				{
					sourceCopies[i].bypassEffects = value;
				}
			}
		}
	}
	public override AudioClip clip
	{
		get
		{
			return mySource.clip;
		}
		set
		{
			for(int i = 0; i < sourceCopies.Count; ++i)
			{
				if(sourceCopies[i] != null)
				{
					sourceCopies[i].clip = value;
				}
			}
		}
	}
	
	public override float dopplerLevel
	{
		get
		{
			return mySource.dopplerLevel;
		}
		set
		{
			for(int i = 0; i < sourceCopies.Count; ++i)
			{
				if(sourceCopies[i] != null)
				{
					sourceCopies[i].dopplerLevel = value;
				}
			}
		}
	}
	public override bool ignoreListenerPause
	{
		get
		{
			return mySource.ignoreListenerPause;
		}
		set
		{
			for(int i = 0; i < sourceCopies.Count; ++i)
			{
				if(sourceCopies[i] != null)
				{
					sourceCopies[i].ignoreListenerPause = value;
				}
			}
		}
	}
	public override bool ignoreListenerVolume
	{
		get
		{
			return mySource.ignoreListenerVolume;
		}
		set
		{
			for(int i = 0; i < sourceCopies.Count; ++i)
			{
				if(sourceCopies[i] != null)
				{
					sourceCopies[i].ignoreListenerVolume = value;
				}
			}
		}
	}
	public override bool loop
	{
		get
		{
			return mySource.loop;
		}
		set
		{
			for(int i = 0; i < sourceCopies.Count; ++i)
			{
				if(sourceCopies[i] != null)
				{
					sourceCopies[i].loop = value;
				}
			}
		}
	}
	
	public override float maxDistance
	{
		get
		{
			return mySource.maxDistance;
		}
		set
		{
			for(int i = 0; i < sourceCopies.Count; ++i)
			{
				if(sourceCopies[i] != null)
				{
					sourceCopies[i].maxDistance = value;
				}
			}
		}
	}
	
	public override float minDistance
	{
		get
		{
			return mySource.minDistance;
		}
		set
		{
			for(int i = 0; i < sourceCopies.Count; ++i)
			{
				if(sourceCopies[i] != null)
				{
					sourceCopies[i].minDistance = value;
				}
			}
		}
	}
	public override bool mute
	{
		get
		{
			return mySource.mute;
		}
		set
		{
			for(int i = 0; i < sourceCopies.Count; ++i)
			{
				if(sourceCopies[i] != null)
				{
					sourceCopies[i].mute = value;
				}	
			}
		}
	}
	
	public override float pan
	{
		get
		{
			return mySource.panStereo;
		}
		set
		{
			for(int i = 0; i < sourceCopies.Count; ++i)
			{
				if(sourceCopies[i] != null)
				{
					sourceCopies[i].panStereo = value;
				}
			}
		}
	}
	
	public override float panLevel
	{
		get
		{
			return mySource.spatialBlend;
		}
		set
		{
			for(int i = 0; i < sourceCopies.Count; ++i)
			{
				if(sourceCopies[i] != null)
				{
					sourceCopies[i].spatialBlend = value;
				}
			}
		}
	}
	
	public override float pitch
	{
		get
		{
			return mySource.pitch;
		}
		set
		{
			for(int i = 0; i < sourceCopies.Count; ++i)
			{
				if(sourceCopies[i] != null)
				{
					sourceCopies[i].pitch = value;
				}	
			}
		}
	}
	
	public override bool playOnAwake
	{
		get
		{
			return mySource.playOnAwake;
		}
		set
		{
			for(int i = 0; i < sourceCopies.Count; ++i)
			{
				if(sourceCopies[i] != null)
				{
					sourceCopies[i].playOnAwake = value;
				}
			}
		}
	}
	public override int priority
	{
		get
		{
			return mySource.priority;
		}
		set
		{
			for(int i = 0; i < sourceCopies.Count; ++i)
			{
				if(sourceCopies[i] != null)
				{
					sourceCopies[i].priority = value;	
				}
			}
		}
	}
	
	public override AudioRolloffMode rolloffMode
	{
		get
		{
			return mySource.rolloffMode;
		}
		set
		{
			for(int i = 0; i < sourceCopies.Count; ++i)
			{
				if(sourceCopies[i] != null)
				{
					sourceCopies[i].rolloffMode = value;
				}
			}
		}
	}
	public override float spread
	{
		get
		{
			return mySource.spread;
		}
		set
		{
			for(int i = 0; i < sourceCopies.Count; ++i)
			{
				if(sourceCopies[i] != null)
				{
					sourceCopies[i].spread = value;
				}
			}
		}
	}
	public override float time
	{
		get
		{
			return mySource.time;
		}
		set
		{
			for(int i = 0; i < sourceCopies.Count; ++i)
			{
				if(sourceCopies[i] != null)
				{
					sourceCopies[i].time = value;
				}
			}
		}
	}
	public override int timeSamples
	{
		get
		{
			return mySource.timeSamples;
		}
		set
		{
			for(int i = 0; i < sourceCopies.Count; ++i)
			{
				if(sourceCopies[i] != null)
				{
					sourceCopies[i].timeSamples = value;
				}
			}
		}
	}
	public override AudioVelocityUpdateMode velocityUpdateMode
	{
		get
		{
			return mySource.velocityUpdateMode;
		}
		set
		{
			for(int i = 0; i < sourceCopies.Count; ++i)
			{
				if(sourceCopies[i] != null)
				{
					sourceCopies[i].velocityUpdateMode = value;
				}
			}
		}
	}
	
	public override void Pause()
	{
		pauseAudio = true;
		for(int i = 0; i < sourceCopies.Count; ++i)
		{
			if(sourceCopies[i] != null)
			{
				sourceCopies[i].Pause();
			}
		}
	}
	public override void Stop()
	{
		StopCoroutine(PLAY_AUDIO_SOURCE_STRING);
		for(int i = 0; i < sourceCopies.Count; ++i)
		{
			if(sourceCopies[i] != null)
			{
				sourceCopies[i].Stop();
			}
		}
	}
#endregion
}
