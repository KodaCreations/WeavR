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

public class VirtualAudioSource_PanByListenerIndex : VirtualAudioSource_SimulatedRolloff {
	
	/// <summary>
	/// If true, this overrides panLevel settings (sets to 0)
	/// </summary>
	public bool force2DSoundOnStartup = true; //if true, this should override panLevel settings
	
	/// <summary>
	/// should be ideally what the function is when distance = 0, but has a maximum of 1
	/// </summary>
	[RangeAttribute(0.001f,1)]
	public float baselineFunctionValue = 1f;
	/// <summary>
	/// The minimum value of the rolloff function. This can be used that a sound always plays at a minimum volume
	/// </summary>
	[RangeAttribute(0, 1f)]
	public float minimumFunctionValue = 0f;
	
	/// <summary>
	/// The normal pitch of the audio source
	/// </summary>
	[RangeAttribute(0.05f, 2.95f)]
	public float pitchBaseline = 1f;
	/// <summary>
	/// Used to control the severity of the doppler effect
	/// </summary>
	[RangeAttribute(0, 1.45f)]
	public float dopplerPitchRange = 0.05f;
	
	/// <summary>
	/// The speed at which the doppler effect caps out and is at it's maximum effect.
	/// </summary>
	public float dopplerMaxSpeed = 1f;
	
	/// <summary>
	/// If you don't want to use the doppler effect, set this to true to save some calculations. It is only rechecked on a PER CLIP basis, so if you enable it mid playthrough of a clip,
	/// it won't do anything until the next time you play the clip.
	/// </summary>
	public bool disableDoppler = false;
	
	private List<Vector3> prevListenerPositions = new List<Vector3>();
	
	protected override void OnEnable()
	{
		UpdateRolloffDelegate();
		if(force2DSoundOnStartup)
		{
			if(mySource != null)
			{
				mySource.spatialBlend = 0;
			}
			else
			{
				Debug.LogWarning("No AudioSource assigned to this virtual source! An AudioSource must be assigned for in order to function properly!");
			}
		}
		base.OnEnable();
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
				mySource.PlayDelayed(delay);
				
				if(disableDoppler)
				{
					List<VirtualAudioListener> listeners = VirtualAudioListener.allListeners;
					
					while(mySource != null && mySource.isPlaying)
					{
						if(listeners.Count == 0)
						{
							mySource.Stop();
						}
						float totalPan = 0f;
						float totalRolloffMult = 0f;
						float totalSquareRolloff = 0f;
						Vector3 myPosition = transform.position;
						
						for(int i = 0; i < listeners.Count; ++i)
						{
							float rolloffMult = Mathf.Clamp(volumeAdjustmentFunction((myPosition-listeners[i].transform.position).magnitude)/baselineFunctionValue, minimumFunctionValue, 1f);
							
							totalPan += rolloffMult*listeners[i].pan2DForListener;
							totalRolloffMult += rolloffMult;
							totalSquareRolloff += rolloffMult*rolloffMult;
							
						}
						
						if(totalRolloffMult > 0)
						{
							mySource.panStereo = totalPan/totalRolloffMult;//using this, the weighted average, is far better than using a straight(arithmetic) average
							mySource.volume = totalSquareRolloff/totalRolloffMult;
						}
						else
						{
							mySource.volume = 0;
						}
						yield return null;
					}
				}
				else
				{
					Vector3 prevPos = transform.position;
					List<VirtualAudioListener> listeners = VirtualAudioListener.allListeners;
					
					for(int i = 0; i < listeners.Count; ++i)
					{
						if(prevListenerPositions.Count <= i)
						{
							prevListenerPositions.Add(listeners[i].transform.position);
						}
						else
						{
							prevListenerPositions[i] = listeners[i].transform.position;
						}
					}
					
					while(mySource != null && mySource.isPlaying)
					{
						if(listeners.Count == 0)
						{
							mySource.Stop();
						}
						float totalPan = 0f;
						float totalRolloffMult = 0f;
						float totalSquareRolloff = 0f;
						Vector3 myPosition = transform.position;
						
						float sourceSpeed = (myPosition - prevPos).magnitude/Time.deltaTime;
						
						Vector3 sourceDirection = Vector3.Normalize(myPosition - prevPos);
						
						float totalSpeedTowards = 0;
						
						for(int i = 0; i < listeners.Count; ++i)
						{
							Vector3 listenerPos = listeners[i].transform.position;
							Vector3 prevListenerPos = prevListenerPositions.Count <= i ? listenerPos : prevListenerPositions[i];
							Vector3 directionToSource = Vector3.Normalize(listenerPos - myPosition);
							
							float rolloffMult = Mathf.Clamp(volumeAdjustmentFunction((myPosition-listeners[i].transform.position).magnitude)/baselineFunctionValue, minimumFunctionValue, 1f);
							
							totalPan += rolloffMult*listeners[i].pan2DForListener;
							totalRolloffMult += rolloffMult;
							totalSquareRolloff += rolloffMult*rolloffMult;
							totalSpeedTowards += rolloffMult*Mathf.Clamp(sourceSpeed*Vector3.Dot(sourceDirection, directionToSource) + ((listenerPos - prevListenerPos).magnitude/Time.deltaTime)*Vector3.Dot(Vector3.Normalize(listenerPos - prevListenerPos), -directionToSource),
								-dopplerMaxSpeed, dopplerMaxSpeed)/dopplerMaxSpeed;
							
							if(prevListenerPositions.Count <= i)
							{
								prevListenerPositions.Add(listenerPos);
							}
							else
							{
								prevListenerPositions[i] = listenerPos;
							}
						}
						
						prevPos = myPosition;
						
						if(totalRolloffMult > 0)
						{
							mySource.panStereo = totalPan/totalRolloffMult;//using this, the weighted average, is far better than using a straight(arithmetic) average
							mySource.volume = totalSquareRolloff/totalRolloffMult;
							mySource.pitch = pitchBaseline + dopplerPitchRange*(totalSpeedTowards/totalRolloffMult);
						}
						else
						{
							mySource.volume = 0;
						}
						yield return null;
					}
				}
			}while(loopCoroutine && !pauseAudio && VirtualAudioListener.allListeners.Count > 0 && mySource != null && mySource.gameObject.activeInHierarchy && mySource.enabled);
		}
		isCoroutinePlaying = false;
	}
	
	
}
