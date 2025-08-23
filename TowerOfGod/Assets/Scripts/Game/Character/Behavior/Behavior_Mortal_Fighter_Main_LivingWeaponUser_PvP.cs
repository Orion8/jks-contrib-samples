using UnityEngine;
using System.Collections;
using React;
//This is a little shortcut which creates an alias for a type, and makes out Action methods look much nicer.
using  Action = System.Collections.Generic.IEnumerator<React.NodeResult>;







public class Behavior_Mortal_Fighter_Main_LivingWeaponUser_PvP : Behavior_Mortal_Fighter_Main_LivingWeaponUser
{

//	protected override void scan()
//	{
//	}


	public override Action walk()
	{
		//Log.jprint (gameObject + "   Knowledge.Action_Walk: "+ Knowledge.Action_Walk);
		if (Knowledge.Action_Walk && !Knowledge.Progress_SkillAnimation)
		{			
			#if DEBUG_LOG		
			Log.jprint (gameObject + "   walk");
			#endif
			
			if (AnimCon == null)
			{
				Log.jprint(gameObject + "   AnimCon == null");
			}
			if (Knowledge == null)
			{
				Log.jprint(gameObject + "   Knowledge == null");
			}
			
			if (!AnimCon.isAnimPlaying(Knowledge.Anim_Walk))
			{
				animate_Walk();
//				setCameraTarget(CameraManager.TargetState.Walk);
			}
			
			//scanEnemy();
			
			yield return React.NodeResult.Success;
		}
		
		yield return React.NodeResult.Failure;
	}	
	
	



	//jks override to disable when team members moved back enough.
	public override Action walkBack()
	{
		//Log.jprint ("Knowledge.Action_Walk: "+ Knowledge.Action_Walk);
		if (Knowledge.Action_WalkBack && !Knowledge.Progress_SkillAnimation)  //jks - let attack finish it's action before start move.
		{			
			#if DEBUG_LOG		
			Log.jprint (gameObject + "   walk back");
			#endif			
			
			if (!AnimCon.isAnimPlaying(Knowledge.Anim_WalkBack))
			{
				animate_WalkBack();
			}
			
			if (transform.forward.x > 0)
			{
				if (transform.position.x < Knowledge.PvpHomePosition)
					forceIdle();
			}
			else
			{
				if (transform.position.x > Knowledge.PvpHomePosition)
					forceIdle();
			}

			yield return React.NodeResult.Success;
		}
		
		yield return React.NodeResult.Failure;
	}


	protected override void scanEnemy()
	{
		//Log.jprint(gameObject + " . . . . . . . Scan enemy ");
		
		bool found = Knowledge.checkEnemyOnPath(false);
		if (found)
		{
			if (Knowledge.DoNotAttack)
			{
				Knowledge.forceResetFlags();
				Knowledge.Action_PvpJustBeforeHit = true;
			}
			else
			{
				StartCoroutine( Knowledge.startSkill() );
			}
		}
	}


	protected override void scan()
	{
		if (Knowledge == null || !Knowledge.Action_Run) return;

		scanEnemy();
	}

}
