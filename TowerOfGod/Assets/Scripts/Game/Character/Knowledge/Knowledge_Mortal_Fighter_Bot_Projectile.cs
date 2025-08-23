using UnityEngine;
using System.Collections;

public class Knowledge_Mortal_Fighter_Bot_Projectile : Knowledge_Mortal_Fighter_Bot
{


//	void Update()
//	{
//		Log.jprint(gameObject.name + " transform.forward   :  " + transform.forward);
//	}


	public override eHitType getFinalHitType(Knowledge_Mortal_Fighter opponent)
	{
		float finalHitRate;
		
		finalHitRate = calcHitRate(opponent);
		
		if (finalHitRate < 0) finalHitRate = 0;
		
		eHitType hitType = judgeHitState(finalHitRate);

		if ((IsBoss) // 보스이고,
			&&
			BattleManager.Instance.isLeaderBuffIgnoreBossCriticalValid()) // 리더버프 크리티컬 무시 이면,
		{
			hitType = eHitType.HT_BAD;

			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
			{
				Log.print_always("공격자 : " + gameObject.name +  " 리더버프: 크리티컬 무시 함 hit type 은 BAD.");
			}
			#endif
		}

		return hitType;
	}


	public override int getReaction(eHitType hitType)
	{
		int hitReactionAnimID;
		
		if (hitType == eHitType.HT_CRITICAL)
		{
			hitReactionAnimID = getReactionAnimID(3, hitType);
		}
		else 
		{
			int index = ComboCurrent;
			if (ComboCurrent == 3)
			{
				index = 2;  //jks avoid always critical hit by combo number since combo3 is shooting on projectile
			}
			hitReactionAnimID = getReactionAnimID(index, hitType); 
		}
		
		return hitReactionAnimID;
	}

	

	public override void giveDamage(float reactionDistanceOverride)
	{		
		
		if (KnowledgePSkill && KnowledgePSkill.Progress_AnyAction)
		{
			KnowledgePSkill.giveDamage(reactionDistanceOverride);
			return;
		}

		//jks 2016.3.14 skill buff 기능 추가.
		addSkillBuff();

		//Log.jprint(gameObject.name +". . . . . giveDamage(): " + _attackType);
		_skillCompleted = isLastDamageInSkill(reactionDistanceOverride);

		Launcher_Projectile launcher = GetComponent<Launcher_Projectile>();
		if (launcher == null) 
		{
			Log.Warning("Can not find Launcher_Projectile component. ");
			return;
		}

		updateProjectileTarget();
		if (_projectileOriginalTarget == null)
		{
			_projectileOriginalTarget = BattleBase.Instance.findClosestOpponent(AllyID, transform.position.x);
			setTarget(_projectileOriginalTarget);
			//updateProjectileTarget();
		}

		if (_projectileOriginalTarget == null) return;

		startAim();

//		if (_projectileOriginalTarget)
		{
			JumpBackCount = 0; //reset

			StartCoroutine( launcher.spawn(_projectileOriginalTarget, AttackInfo, AttackType, _weaponType_ForAnimation, reactionDistanceOverride));

			updateTotalGiveDamageCount();

		}

	}
	

	public override bool amIMelee()
	{
		return false;
	}


	public override bool canIAttackFlyingOpponent()
	{
		return true;
	}


	
//	protected override bool isTooCloseToAttack()
//	{
//		GameObject target = getCurrentTarget();
//		
//		if (target == null) return false;
//		
//		float dist = Mathf.Abs(transform.position.x - target.transform.position.x);
//		float distShell = dist - Radius - target.GetComponent<Knowledge_Mortal_Fighter>().Radius;
//		
//		if (distShell < _attackDistanceMax * 1.5f)
//			return true;
//		
//		return false;
//	}


}
