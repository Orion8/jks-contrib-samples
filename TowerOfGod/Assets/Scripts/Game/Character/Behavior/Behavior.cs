using UnityEngine;
using System.Collections;

//jks- Methods in child classes of Behavior are mostly used by behavior tree attached to the gameobject.

public class Behavior : MonoInit 
{

	protected Knowledge	 _knowledge = null;
	protected AnimationControl _animControl = null;
	
	
	protected Knowledge Knowledge
	{
		get
		{
			if (_knowledge == null)
			{
				_knowledge = GetComponent<Knowledge>();
			}
			return (Knowledge)_knowledge;
		}
	}

	protected AnimationControl AnimCon
	{
		get
		{
			if (_animControl == null)
			{
				_animControl = GetComponent<AnimationControl>();
			}
			return (AnimationControl)_animControl;
		}
	}

	
	protected override void initializeBeforeUpdateBegin()
	{
		base.initializeBeforeUpdateBegin();
		_knowledge = GetComponent<Knowledge>();
		_animControl = GetComponent<AnimationControl>();		
	}


}
