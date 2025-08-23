using UnityEngine;
using System.Collections;

public class Knowledge_Mortal_Fighter_Main_Throw : Knowledge_Mortal_Fighter_Main_Projectile
{



	protected override IEnumerator setLauncher()
	{
		//jks find correct launcher in case launcher is more than one
		
		Launcher_Throw[] launchers = GetComponents<Launcher_Throw>();

		while (launchers.Length == 0)
		{
			yield return null;
			launchers = GetComponents<Launcher_Throw>();
		}
		
		foreach (Launcher_Throw launcher in launchers )
		{
			if (launcher.LauncherTypeName == "Launcher_Throw")
			{
				_launcher = launcher;
				break;
			}
		}

	}




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



	protected bool AmICloseToLeader()
	{

		Knowledge_Mortal_Fighter leader = BattleManager.Instance.LeaderKnowledge;

		if (leader == null) return false;
		
		if (transform.position.x >= leader.transform.position.x)  //jks if leader is behind, start skill now.
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
			if (BattleBase.Instance.EnemyToKill > 0)
				return AmICloseToLeader();  // 앞에 적이 있고 전투 중인 상황에는 리더에 접근했는지를 알림.
			else
				return false;   // 앞에 적이 없으면 목적지 도달 미션을 위해 적이 없음을 알림.
		}
		else
		{
			return base.checkEnemyOnPath(false);
		}
	}

	public override void giveDamage(float reactionDistanceOverride)
	{		
//		//jks 역할 스킬 인가?
//		if (Action_RoleSkill)  
//		{
//			giveDamage_RoleSkill();
//			return;
//		}
//		//jks 지원 스킬 인가?
//		else 
		if (KnowledgeSSkill && KnowledgeSSkill.Progress_AnyAction)
		{
			KnowledgeSSkill.giveDamage(reactionDistanceOverride);
			return;
		}
		
		//jks 2016.3.14 skill buff 기능 추가.
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

			SkillType = eSkillType.ST_Card;
			StartCoroutine( _launcher.spawn(reactionDistanceOverride)); //jks reactionDistanceOverride may have spawn position info too.
			
			JumpBackCount = 0; //reset
			
		}

	}


}
