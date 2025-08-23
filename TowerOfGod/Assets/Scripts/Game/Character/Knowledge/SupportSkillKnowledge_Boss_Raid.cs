using UnityEngine;
using System.Collections;
using CodeStage.AntiCheat.ObscuredTypes;

public class SupportSkillKnowledge_Boss_Raid : SupportSkillKnowledge_Boss
{

	new public Knowledge_Mortal_Fighter_Bot_RaidBoss Knowledge
	{
		get
		{
			if (_knowledge == null)
			{
				_knowledge = GetComponent<Knowledge_Mortal_Fighter_Bot_RaidBoss>();
			}
			return (Knowledge_Mortal_Fighter_Bot_RaidBoss)_knowledge;
		}
	}




	public override int AttackPoint 
	{ 
		get
		{
			ObscuredInt distributedAttackPoint = Info[ActiveSkillNum]._attack_point_raidboss; //jks 2016.6.13 레이드보스는 스킬 별 공격력을 가지기 위해 수정.

			if (DamageFrequency < 0) //Info[ActiveSkillNum] == null
				return -1;

			if (DamageFrequency > 1)// if multiple attack skill?
			{
				distributedAttackPoint = Mathf.RoundToInt(Knowledge._attackPoint / DamageFrequency);  //jks 2015.11.23:  new calc
				distributedAttackPoint += Mathf.RoundToInt(distributedAttackPoint * BattleTuning.Instance._multipleAttackSkillAdjustmentFactor);
			}

			return distributedAttackPoint;
		}
	}





	public override void startSkill_Support(int skillNum)
	{
		//jks Blitz 가 이미 사용되어 진행 중이면,
		if (((BattleManager_BossRaid)BattleManager_BossRaid.Instance).BlitzInProgress) return;

		//jks 패턴 스킬 쿨타임 중이 아니면, 
		if (! Knowledge.CoolTimer.IsCoolingInProgress)
		{
//			//jks blitz 사용할 수 있는 상태이면, blitz 사용.
//			if (((BattleManager_BossRaid)BattleManager_BossRaid.Instance).IsBlitzReady)
//			{
//				((BattleManager_BossRaid)BattleManager_BossRaid.Instance).startBlitz();
//			}
//			//jks blitz 사용할 수 없는 상태이면, 패턴 스킬 사용.
//			else
			{
				base.startSkill_Support(skillNum);
			}	
		}
	}



}
