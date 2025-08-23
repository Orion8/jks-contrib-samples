using UnityEngine;
using System.Collections;



//jks-  지속 데미지 무기.
public class Weapon_Throw_Areal_Continuous_Damage : Weapon_Throw_Areal_SlowSkill
{
	protected float _damage_penetration_delta = 0;

	protected int _damageCount = 1;

	public void setAttributes_continuous_damage(float radius, float reactionDistanceOverride,
								float damage_interval, float damage_penetration_delta
	)
	{
		setAttributes(1, radius, damage_interval, reactionDistanceOverride);
		_damage_penetration_delta = damage_penetration_delta;
	}


	protected override void applyWeaponPower()
	{
	}



	protected override void applyDamage()
	{
		updateList();
		//Log.print_always(Time.time + "   --------------------------------- updateList -----------------_opponentsInRadius count: "+_opponentsInRadius.Count);

		if (_opponentsInRadius != null)
			setFlagInList( _opponentsInRadius, obj => 
				{ 
					giveDamage(obj); 
				}
			);
	}


	protected override int damageAdjustment(int curDamage)
	{

		int delta = (int)(curDamage * (_damage_penetration_delta * _damageCount));

		int newDamage = curDamage + delta;


		#if UNITY_EDITOR
		if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
		{
			Log.print_always(Time.time + "   --------------------------------- 지속형 Throw . ---------------------------------_damage_interval: "+_damage_interval);
			Log.print_always("   데미지 보정  ::    공격력: " + curDamage + "     "+ (_damage_penetration_delta * _damageCount * 100) + " % = "+ newDamage);

		}
		#endif



		_damageCount ++;

		return newDamage;
	}



}
