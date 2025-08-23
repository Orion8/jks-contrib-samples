using UnityEngine;
using System.Collections;
using CodeStage.AntiCheat.ObscuredTypes;

//jks  스킬속도를 느리게하는 던져 발동하는 무기.
public class Weapon_Throw_Areal_SlowSkill : Weapon_Throw_Areal
{

	protected float _slow_rate_skill;

	protected float _damage_interval;

	protected bool _isDone = false;


	public override bool IsSlowTrap { get { return true; }}


	public void setAttributes(float rate, float radius, float damageInterval, float reactionDistanceOverride)
	{
		setAttributes(radius);
		_slow_rate_skill = rate;
		_damage_interval = damageInterval;
		_reactionDistanceOverride = reactionDistanceOverride;
	}


	protected override void activateWeapon() 
	{ 
		base.activateWeapon();

		_isDone = false;
		InvokeRepeating("applyWeaponPower", 0.01f, 0.1f);

		if (_damage_interval > 0)
			InvokeRepeating("applyDamage", _damage_interval, _damage_interval);
	}
	
	
	protected override void applyWeaponPower()
	{
		if (_isDone) return;

		updateList();	
		setSlow(_slow_rate_skill, 1);
	}


	protected virtual void applyDamage()
	{
		if (_opponentsInRadius != null)
			setFlagInList( _opponentsInRadius, obj => 
				{ 
					Knowledge_Mortal_Fighter know = obj.GetComponent<Knowledge_Mortal_Fighter>();

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

						giveDamage(obj); 
						//Log.jprint(Time.time + "   "+ obj + "  - o - o - o -   A P P L Y  slow skill  : " + rate_skill + "     movement rate: "+ rate_move);

					}
				}
			);

	}

	
	public override void killMe()
	{
		//jks reset all opponents slow skill flag.
//		updateList();
		releaseSlowedOpponent();
		resetFlag();

		base.killMe();
	}

	
	protected virtual void resetFlag()
	{
		_isDone = true;
//		setSlow(1.0f, 1.0f);
	}


	protected virtual void setSlow(float rate_skill, float rate_move)
	{
		if (_opponentsInRadius != null)
			setFlagInList( _opponentsInRadius, obj => 
				{ 
					Knowledge_Mortal_Fighter know = obj.GetComponent<Knowledge_Mortal_Fighter>();
								
					if (rate_skill != know.SkillSpeedAdjustment)
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

						know.SkillSpeedAdjustment = rate_skill; 
						//Log.jprint(Time.time + "   "+ obj + "  - o - o - o -   A P P L Y  slow skill  : " + rate_skill + "     movement rate: "+ rate_move);

//						refreshAnimationForSlowReset(know);
						know.AnimCon.setAnimationSpeed(know.calculate_SkillAnimationPlayRate());

						know.AppliedSkillBuff_UID = OwnerKnowledge.MyCurrentSkillBuff_UID;

					}
				}
			);

		//jks reset opponents not in radius
		if (_opponentsNotInRadius != null)
			setFlagInList( _opponentsNotInRadius, obj => 
				{ 
					Knowledge_Mortal_Fighter know = obj.GetComponent<Knowledge_Mortal_Fighter>();
					if (1.0f != know.SkillSpeedAdjustment)
					{
						know.SkillSpeedAdjustment = 1.0f; 

						refreshAnimationForSlowReset(know);
					}
          		 }
			);
	}
	








}
