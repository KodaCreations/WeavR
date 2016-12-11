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
using System;

//[SerializableAttribute]
public abstract class VirtualAudioSource_SimulatedRolloff : VirtualAudioSource {

	//if the mode is linear, this is treated as y = volumeScalar*(x+volumeInsideParenthesisAddition)+volumeOutsideParenthesisAddition
	//if the mode is logarithmic, this is treated as y = volumeScalar*log(x+volumeInsideParenthesisAddition)+volumeOutsideParenthesisAddition
	//if you want to incorperate things like a different base, you can calculate the change of base formula and apply that to the volume scalar
	//these are used strictly for non custom rolloffs
	/// <summary>
	/// To have volume decrease with increased distance, the scalar should always be negative
	/// </summary>
	public float volumeScalar = -0.1f;
	public float volumeInsideParenthesisAddition = 0f;
	public float volumeOutsideParenthesisAddition = 1f;
	
	/// <summary>
	/// Used strictly for custom rolloff - ignored when using linear or logarithmic rolloff
	/// </summary>
	public AnimationCurve customRolloff;
	
	
	
	public delegate float GetDistanceAdjustedVolume(float distMagnitude);
	/// <summary>
	/// Pass a distance magnitude to this delegate and it will return you what the rolloff value based on that distance should be
	/// </summary>
	public GetDistanceAdjustedVolume volumeAdjustmentFunction;
	
	/*
	public enum RolloffMode
	{
		LINEAR,
		LOGARITHMIC,
		CUSTOM
	}
	public RolloffMode volumeRolloffType;
	*/
	
	public override AudioRolloffMode rolloffMode
	{
		get
		{
			return mySource.rolloffMode;
		}
		set
		{
			mySource.rolloffMode = value;
			UpdateRolloffDelegate();
		}
	}
	
	public float GetDistanceAdjustedVolumeLinear(float distMagnitude)
	{
		return volumeScalar*(distMagnitude+volumeInsideParenthesisAddition)+volumeOutsideParenthesisAddition;
	}
	public float GetDistanceAdjustedVolumeLogarithmic(float distMagnitude)
	{
		return volumeScalar*Mathf.Log(distMagnitude+volumeInsideParenthesisAddition)+volumeOutsideParenthesisAddition;
	}
	public float GetDistanceAdjustedVolumeCustom(float distMagnitude)
	{
		return customRolloff.Evaluate(distMagnitude);
	}
	public void UpdateRolloffDelegate()
	{
		switch(rolloffMode)
		{
			case AudioRolloffMode.Linear:
				volumeAdjustmentFunction = GetDistanceAdjustedVolumeLinear;
				break;
			
			case AudioRolloffMode.Logarithmic:
				volumeAdjustmentFunction = GetDistanceAdjustedVolumeLogarithmic;
				break;
			
			case AudioRolloffMode.Custom:
				volumeAdjustmentFunction = GetDistanceAdjustedVolumeCustom;
				break;
		}
	}
	/*
	public void UpdateRolloffDelegate()
	{
		switch(volumeRolloffType)
		{
			case RolloffMode.LINEAR:
				volumeAdjustmentFunction = GetDistanceAdjustedVolumeLinear;
				break;
			
			case RolloffMode.LOGARITHMIC:
				volumeAdjustmentFunction = GetDistanceAdjustedVolumeLogarithmic;
				break;
			
			case RolloffMode.CUSTOM:
				volumeAdjustmentFunction = GetDistanceAdjustedVolumeCustom;
				break;
		}
	}
	*/
	
	
}
