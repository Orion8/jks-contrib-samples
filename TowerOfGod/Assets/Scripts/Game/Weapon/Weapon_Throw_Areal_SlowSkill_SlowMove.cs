using UnityEngine;
using System.Collections;

//jks  스킬속도와 이동속도를 느리게하는 던져 발동하는 무기.
public class Weapon_Throw_Areal_SlowSkill_SlowMove : Weapon_Throw_Areal_SlowSkill
{
	protected float _slow_rate_move;

	public override bool IsSlowTrap { get { return true; }}


	public void setAttributes(float rate, float rate2, float radius, float damageInterval, float reactionDistanceOverride)
	{
		setAttributes(rate, radius, damageInterval, reactionDistanceOverride);

		_slow_rate_move = rate2;

	}


	protected override void applyWeaponPower()
	{
		if (_isDone) return;

		updateList();	
		setSlow(_slow_rate_skill, _slow_rate_move);
	}


//	protected override void refreshAnimationForSlowReset(Knowledge_Mortal_Fighter know)
//	{
//		if (know.AmIManualLeader) return;
//		if (know.Progress_SkillAction && _slow_rate_skill >= 0.99f) return;  //jks 이동속도 만 감소하는 스킬이면, 스킬 중인 캐릭터는 flag 리셋 하지 않음.
//
////		Log.jprint(Time.time+ "    " + know.gameObject.name +  "    refreshAnimation()  SkillAction: " + know.Progress_SkillAction
////			+  "    run: " + know.Action_Run + " : " + _slow_rate_skill + "  :  "+ _slow_rate_move);
//
//		know.forceResetFlags();
//		know.Action_Run = true;
//	}



	protected override void setSlow(float rate_skill, float rate_move)
	{
		if (_opponentsInRadius != null)
			setFlagInList( _opponentsInRadius, obj =>
				{
					Knowledge_Mortal_Fighter know = obj.GetComponent<Knowledge_Mortal_Fighter>();

					if (rate_move != know.WeaponSlowMovement)
					{
						//jks 같은 타입의 스킬을 가지고 있는 상대는 trap 에 걸리지 않음.
						if (know.AttackType == eAttackType.AT_Throwing )
						{
							Launcher_Throw launcher = know.GetComponent<Launcher_Throw>();
							if (launcher != null && launcher.IsSlowTypeSkill)
							{
								return;
							}
						}

						//jks update slow rate
						know.WeaponSlowMovement = rate_move;
						know.SkillSpeedAdjustment = rate_skill; 

						//Log.jprint(Time.time + " owner: "+ _owner + "  target: "+ obj + "  captured ...  skill: " + rate_skill + "     movement: "+ rate_move);
						//refreshAnimationForSlowReset(know);
						if (know.Progress_SkillAction)
							know.AnimCon.setAnimationSpeed(know.calculate_SkillAnimationPlayRate());
						else
							know.AnimCon.setAnimationSpeed(know.calculate_MovementPlayRate());

						know.AppliedSkillBuff_UID = OwnerKnowledge.MyCurrentSkillBuff_UID;
					}
				}
			);

		if (_opponentsNotInRadius != null)
			setFlagInList( _opponentsNotInRadius,obj => 
				{ 
								
					Knowledge_Mortal_Fighter know = obj.GetComponent<Knowledge_Mortal_Fighter>();
								
					if (1.0f != know.WeaponSlowMovement)
					{
						know.WeaponSlowMovement = 1.0f; 
						know.SkillSpeedAdjustment = 1.0f; 

						//Log.jprint(Time.time + "   "+ obj + "  o * o * o * o   R E S E T  slow  ");
						refreshAnimationForSlowReset(know);
					}
			    }
			);
		
	}



}
