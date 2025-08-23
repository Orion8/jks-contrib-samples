using UnityEngine;
using System.Collections;

public class Weapon_Throw_Areal_AirCapture : Weapon_Throw_Areal
{
	protected float _height;

	protected float _damage_interval;

	protected string _explosionFx;



	public void setAttributes(float radius, float height, float damageInterval, float reactionDistanceOverride, string explosionFx)
	{
		setAttributes(radius);
		_height = height;
		_damage_interval = damageInterval;
		_explosionFx = explosionFx;
		_reactionDistanceOverride = reactionDistanceOverride;
	}


	protected override void activateWeapon() 
	{ 
		base.activateWeapon();

		applyWeaponPower();  // InvokeRepeating("applyWeaponPower", 0.01f, 0.1f);

		if (_damage_interval > 0)
			InvokeRepeating("applyDamage", _damage_interval, _damage_interval);
	}


	protected void createTargetEffect(Transform parent) 
	{
		if (_explosionFx != null && _explosionFx.Length > 1)
			MadPool.Instance().poolInstantiate(_explosionFx, parent);
	}


	protected override void applyWeaponPower()
	{
		updateList();	
		captureInTheAir(_height, _life_span);
	}


	protected virtual void applyDamage()
	{
		if (_opponentsInRadius != null)
			setFlagInList( _opponentsInRadius, obj => 
				{ 
					giveDamage(obj);
					//Log.jprint(Time.time + "   "+ obj + "  - o - o - o -   A P P L Y  slow skill  : " + rate_skill + "     movement rate: "+ rate_move);
				}
			);

	}


	protected void captureInTheAir(float height, float duration)
	{
		if (_opponentsInRadius != null)
			setFlagInList( _opponentsInRadius, obj => 
				{ 
					Knowledge_Mortal_Fighter know = obj.GetComponent<Knowledge_Mortal_Fighter>();

					if (!know.IsDead) 
					{
						know.setActionCapturedInTheAir(height, duration);
						createTargetEffect(know.transform);
						know.AppliedSkillBuff_UID = OwnerKnowledge.MyCurrentSkillBuff_UID;
					}
				}
			);
	}


}
