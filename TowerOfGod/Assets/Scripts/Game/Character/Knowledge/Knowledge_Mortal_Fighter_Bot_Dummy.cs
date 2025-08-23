using UnityEngine;
using System.Collections;
using CodeStage.AntiCheat.ObscuredTypes;

public class Knowledge_Mortal_Fighter_Bot_Dummy : Knowledge_Mortal_Fighter_Bot 
{

	void Awake()
	{
		_AIMoveType = eAIMoveType.AMT_Stay;
	}



	public override int animHitReaction() 
	{ 
		//Log.jprint(Time.time +" : "+ gameObject + " ? ? ? ? ? ? ? animHitReaction() : " + _animHitNumber);
		return getAnimHash("Reaction99");
	}

	


	public override ObscuredInt takeDamage(ObscuredInt damagePoint, int reactionAnimID, eHitType hitType, eAttackType attackType, eWeaponType_ForAnimation weaponType, GameObject attacker,  float reactionDistanceOverride)
	{
		_recentHitType = hitType; ///jks save recent hit type

		_animHitNumber = reactionAnimID;

		IsLastDamageInSkill = isLastDamageInSkill(reactionDistanceOverride, attacker); //jks reset


		//Log.jprint("* * * * * * * * reaction number: " + _animHitNumber);

//jks 2015.4.13 -		if (!IsLastDamageInSkill && hitType != eHitType.HT_CRITICAL) //jks 스킬을 마지막 타이거나 크리티컬이면 뒤로 밀리는 부분 생략.- (크리티컬 리액션은 root motion 사용하기 때문에 따로 미는 부분 생략.) 	
		{
			applyReactionDistance(reactionDistanceOverride);
		}


		//		base.takeDamage(damagePoint, reactionAnimID, hitType, attacker);
		//jsm - set damage fx (sound, particle)
//		eAttackType attackType = attacker.GetComponent<Knowledge_Mortal_Fighter>().AttackType;
//		eWeaponType_ForAnimation weaponType = attacker.GetComponent<Knowledge_Mortal_Fighter>()._weaponType_ForAnimation;

		if (ProjectileObject != null)
			spawnDamageEffect( ProjectileObject, attackType, hitType, weaponType );
		else 
			spawnDamageEffect( attacker, attackType, hitType, weaponType );



//		if (!Action_Hit) //jks if hit already, don't reset flag for new hit reaction.
//		if (!Progress_HitReaction) //jks if hit already, don't reset flag for new hit reaction.
		{
			forceResetFlags();
			Action_Hit = true;
			//Log.jprint(Time.time +" : "+ gameObject +" ! ! ! ! ! ! ! takeDamage() ... reactionDistanceOverride: " + reactionDistanceOverride  );
		}

//		if (hitType == eHitType.HT_CRITICAL)
//		{
//			CameraShake.Shake(1, Vector3.one, new Vector3(0.1f, 0.1f, 0.1f), 0.2f, 70.0f, 0.2f, 1.0f, false);
//		}
		//jks reset depth position
		Vector3 temp = transform.position;
		temp.z = 0;
		transform.position = temp;

		ProjectileObject = null;

		return damagePoint;
	}



	protected override void pushPassedOpponents(){}
	
	protected override void pushPassedOpponentsWhenIMove(){}


	protected override void resetActionCapturedInTheAir_Delay(float delay) {}

	public override void setActionCapturedInTheAir(float height, float delay) {}

}
