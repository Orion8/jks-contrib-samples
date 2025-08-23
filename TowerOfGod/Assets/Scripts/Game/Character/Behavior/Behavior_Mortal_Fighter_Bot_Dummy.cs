using UnityEngine;
using System.Collections;
using React;
using  Action = System.Collections.Generic.IEnumerator<React.NodeResult>;

public class Behavior_Mortal_Fighter_Bot_Dummy : Behavior_Mortal_Fighter_Bot
{


	public override Action run()
	{
		yield return React.NodeResult.Failure;
	}	

	public override Action idle()
	{
		//jks default animation  //		if (Knowledge.Action_Idle)
		if (!AnimCon.isAnimPlaying(Knowledge.Anim_Idle))
		{
			animate_Idle();
		}
		
		yield return React.NodeResult.Success;
	}	


}
