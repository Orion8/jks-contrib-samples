using UnityEngine;
using System.Collections;



//jks-  투척 폭탄 무기.
public class Weapon_Throw_Areal_Bomb : Weapon_Throw_Areal 
{
	protected int _attack_point;
	protected float _push_power;

	//protected eHitType _hitType;

	protected string _explosionFx;


	public void setAttributes(int attack_point, float radius, float reactionDistanceOverride, string explosionFx)
	{
		setAttributes(radius);
		_attack_point = attack_point;
		_reactionDistanceOverride = reactionDistanceOverride;
		_explosionFx = explosionFx;
	}


	protected override void activateWeapon()
	{
		createExplosionEffect();
		updateList();
		giveDamageOnList();
		killMe();
	}


	protected void createExplosionEffect() 
	{
		if (_explosionFx != null && _explosionFx.Length > 1)
			MadPool.Instance().poolInstantiate(_explosionFx, transform.position, transform.rotation);
	}



	protected void giveDamageOnList()
	{
		Knowledge_Mortal_Fighter ownerKnow = _owner.GetComponent<Knowledge_Mortal_Fighter>();

		ownerKnow.getReactionAnimID(0, eHitType.HT_CRITICAL);

		foreach (GameObject go in _opponentsInRadius)
		{
			Knowledge_Mortal_Fighter knowledgeOpponent = go.GetComponent<Knowledge_Mortal_Fighter>();

			if (AllyID == knowledgeOpponent.AllyID) continue;

			eHitType hitType = ownerKnow.getFinalHitType(knowledgeOpponent);
			int hitReactionAnimID = ownerKnow.getReaction(hitType);

			knowledgeOpponent.takeDamage(_attack_point, hitReactionAnimID, hitType, eAttackType.AT_Throwing, eWeaponType_ForAnimation.WTA_Bomb, _owner, _reactionDistanceOverride);
		}
	}



}
