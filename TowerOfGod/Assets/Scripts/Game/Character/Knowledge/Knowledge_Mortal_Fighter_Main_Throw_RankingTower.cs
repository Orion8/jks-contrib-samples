using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Knowledge_Mortal_Fighter_Main_Throw_RankingTower : Knowledge_Mortal_Fighter_Main_Projectile_RankingTower
{
	protected override List<FighterActor> Opponents { get { return BattleBase.Instance.List_Ally; } }

	protected bool AmICloseToLeader()
	{
		
		Knowledge_Mortal_Fighter leader = BattleManager.Instance.LeaderKnowledge_P2;
		
		if (leader == null) return false;
		
		if (transform.position.x <= leader.transform.position.x)  //jks if leader is behind, start skill now.
			return true;
		
		return (Mathf.Abs(transform.position.x - leader.transform.position.x) - Radius - leader.Radius < _attackDistanceMax);			 
	}



	public override bool checkEnemyOnPath(bool bQuickSkill)
	{
		Launcher_Throw launcher = GetComponent<Launcher_Throw>();
		
		if ( launcher.ThrowWeaponType == eThrowWeaponType.TWT_Sinsu ||
		    launcher.ThrowWeaponType == eThrowWeaponType.TWT_Cooltime ||
		    launcher.ThrowWeaponType == eThrowWeaponType.TWT_FastSkill )
		{
			return AmICloseToLeader();
		}
		else
		{
			return base.checkEnemyOnPath(false);
		}
	}



	public override void giveDamage(float reactionDistanceOverride)
	{		

		//jks 2016.3.14 skill buff 기능 추가.
		addSkillBuff();

		_everGaveDamageForTheAttack = true;
		
		_skillCompleted = isLastDamageInSkill(reactionDistanceOverride);
		
		Launcher_Throw launcher = GetComponent<Launcher_Throw>();
		if (launcher == null) 
		{
			Log.Warning("Can not find Launcher_Throw component. ");
			return;
		}
		
		SkillType = eSkillType.ST_Card;
		StartCoroutine( launcher.spawn(reactionDistanceOverride)); //jks reactionDistanceOverride may have spawn position info too.
		
		JumpBackCount = 0; //reset
	}



	public override void incrementContinuousHitCount() 
	{ 
		if (!IsLeader) return;

		_continuousHitCount_QuickSkill++; 

		BattleBase.Instance.setLeaderBuffInvincibleMode_P2(_continuousHitCount_QuickSkill);
	}

}
