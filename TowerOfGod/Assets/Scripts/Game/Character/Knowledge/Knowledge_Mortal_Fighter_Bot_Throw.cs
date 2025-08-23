using UnityEngine;
using System.Collections;

public class Knowledge_Mortal_Fighter_Bot_Throw : Knowledge_Mortal_Fighter_Bot_Projectile 
{


	public override IEnumerator installWeapon()
	{
		yield break;
	}


	public override void startAim()
	{
		//jks do not aim
	}


	public override bool amIMelee()
	{
		return false;
	}


	protected bool amICloseToBoss()
	{
		Knowledge_Mortal_Fighter knowBoss = null;

		if (BattleBase.Instance.HaveLeaderBoss)
			knowBoss = BattleBase.Instance.LeaderBoss.GetComponent<Knowledge_Mortal_Fighter_Bot>();
		else if (BattleBase.Instance.HaveStoryBoss)
			knowBoss = BattleBase.Instance.StoryBoss.GetComponent<Knowledge_Mortal_Fighter_Bot>();
		else if (BattleBase.Instance.HaveRaidBoss)
			knowBoss = BattleBase.Instance.RaidBoss.GetComponent<Knowledge_Mortal_Fighter_Bot>();
		else if (BattleBase.Instance.SimpleBoss)
			knowBoss = BattleBase.Instance.SimpleBoss.GetComponent<Knowledge_Mortal_Fighter_Bot>();
		
		if (knowBoss == null) return false;

		if (transform.position.x <= knowBoss.transform.position.x)  //jks if boss is behind, start skill now.
			return true;

		return (Mathf.Abs(transform.position.x - knowBoss.transform.position.x) - Radius - knowBoss.Radius < _attackDistanceMax);			 
	}



	public override bool checkEnemyOnPath(bool bQuickSkill)
	{
		Launcher_Throw launcher = GetComponent<Launcher_Throw>();

		if ( launcher.ThrowWeaponType == eThrowWeaponType.TWT_Sinsu ||
			launcher.ThrowWeaponType == eThrowWeaponType.TWT_Cooltime ||
			launcher.ThrowWeaponType == eThrowWeaponType.TWT_FastSkill )
		{
			return amICloseToBoss();
		}
		else
		{
			return base.checkEnemyOnPath(false);
		}
	}


	public override void giveDamage(float reactionDistanceOverride)
	{		
		if (KnowledgePSkill && KnowledgePSkill.Progress_AnyAction)
		{
			KnowledgePSkill.giveDamage(reactionDistanceOverride);
			return;
		}

		//jks 2016.3.11 skill buff 기능 추가.
		addSkillBuff();

		_everGaveDamageForTheAttack = true;

		_skillCompleted = isLastDamageInSkill(reactionDistanceOverride);

		//Log.jprint(". . . . . giveDamage(): " + _attackType);
		//		Launcher_Throw launcher = GetComponent<Launcher_Throw>();
		//		if (launcher == null) 
		//		{
		//			Log.Warning("Can not find Launcher_Throw component. ");
		//			return;
		//		}

		{
			//Log.jprint("giveDamage()");
			Launcher_Throw launcher = GetComponent<Launcher_Throw>();

			SkillType = eSkillType.ST_Card;
			StartCoroutine( launcher.spawn(reactionDistanceOverride)); //jks reactionDistanceOverride may have spawn position info too.

			JumpBackCount = 0; //reset

		}

	}
}
