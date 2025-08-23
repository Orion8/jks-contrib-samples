using UnityEngine;
using System.Collections;

public class Knowledge_Mortal_Fighter_Main_UI : Knowledge_Mortal_Fighter_Main 
{

	protected override void initializeBeforeUpdateBegin()
	{
		base.initializeBeforeUpdateBegin();
		
		_walk = false;
		_idle = true;

		GetComponent<AnimationControl>().applyRootMotion(false);

	}

}
