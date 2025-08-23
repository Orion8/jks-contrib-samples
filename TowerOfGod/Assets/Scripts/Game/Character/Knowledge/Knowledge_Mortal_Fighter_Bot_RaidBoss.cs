using UnityEngine;
using System.Collections;
using CodeStage.AntiCheat.ObscuredTypes;


public class SkillInfo
{
	public int _skill_ID;

	public float _attackDistanceMin = 0.5f;
	public ObscuredFloat _attackDistanceMax = 1.0f;
	public ObscuredFloat _damageRange = 1.0f;
	public byte _totalCombo = 1;
	public byte _lastCombo = 1;

	protected float _projectileRadius;//_attackInfo5;

	protected ObscuredFloat _attackInfo;
	protected ObscuredInt _attackInfo2;
	protected ObscuredInt _attackInfo3;
	protected string _attackInfo4;
	protected ObscuredInt _damage_frequency;
	protected eAttackType _attackType;
	public eWeaponType_ForAnimation _weaponType_ForAnimation;
	public int _weaponPathID;

	public int[,] _anim_reaction_blitz = new int[6,7]{{0,0,0,0,0,0,0},{0,0,0,0,0,0,0},{0,0,0,0,0,0,0},{0,0,0,0,0,0,0},{0,0,0,0,0,0,0},{0,0,0,0,0,0,0}}; //jks _anim_reaction[1,] : combo2 reaction animation controller surfix number.

	public ObscuredInt _attackPoint_blitz;




	public void setAttributesFromTable(Table_Skill tbl)
	{
		_skill_ID = tbl.ID;

		//Log.jprint("2 . . . . . . . . setAttributesFromTable()");
		_attackDistanceMin = tbl._attackDistanceMin * 0.1f;  //jks fixed point conversion (0~255 to 0~25.5m)
		_attackDistanceMax = tbl._attackDistanceMax * 0.1f;
		_damageRange = tbl._damageRange * 0.1f;
		_totalCombo = tbl._totalCombo;
		_lastCombo = tbl._lastCombo;
		_attackType = (eAttackType)tbl._attackType;
		_attackInfo = tbl._attackInfo;
		_attackInfo2 = tbl._attackInfo2;
		_attackInfo3 = tbl._attackInfo3;
		_attackInfo4 = tbl._attackInfo4;

		_projectileRadius = (float)tbl._attackInfo5 * 0.001f;  //jks 1000 = 1m

		_damage_frequency = (int)tbl._attackInfo6;
		if (_damage_frequency < 1)
			_damage_frequency = 1;

		_weaponType_ForAnimation = (eWeaponType_ForAnimation)tbl._weaponType;

		setAttributesFromTable(tbl._reactionTypeID, _anim_reaction_blitz);

		_weaponPathID = tbl._assetPathID_Weapon;

//		if (tbl._skill_info_attack_id > 0)
//		{
//			Table_Skill_Info_Attack tableInfo = (Table_Skill_Info_Attack)TableManager.GetContent(tbl._skill_info_attack_id);
//			if (tableInfo != null)
//				setAttributesFromTable(tableInfo);
//		}

		_attackPoint_blitz = tbl._attack_point_raidboss;
	}


	public void setAttributesFromTable(int reactionTypeID, int[,] anim_reaction)
	{
		Table_ReactionType table = (Table_ReactionType)TableManager.GetContent(reactionTypeID);

		Table_ReactionGroup tbl = (Table_ReactionGroup)TableManager.GetContent(table._reactionGroupID1);

		if (tbl != null)
		{
			anim_reaction[0,0] = tbl._reactionID1;
			anim_reaction[0,1] = tbl._reactionID2;
			anim_reaction[0,2] = tbl._reactionID3;
			anim_reaction[0,3] = tbl._reactionID4;
			anim_reaction[0,4] = tbl._reactionID5;
			anim_reaction[0,5] = tbl._reactionID6;
			anim_reaction[0,6] = tbl._reactionID7;
		}

		if (table._reactionGroupID2 == 0) return;
		tbl = (Table_ReactionGroup)TableManager.GetContent(table._reactionGroupID2);
		if (tbl != null)
		{
			anim_reaction[1,0] = tbl._reactionID1;
			anim_reaction[1,1] = tbl._reactionID2;
			anim_reaction[1,2] = tbl._reactionID3;
			anim_reaction[1,3] = tbl._reactionID4;
			anim_reaction[1,4] = tbl._reactionID5;
			anim_reaction[1,5] = tbl._reactionID6;
			anim_reaction[1,6] = tbl._reactionID7;
		}

		if (table._reactionGroupID3 == 0) return;
		tbl = (Table_ReactionGroup)TableManager.GetContent(table._reactionGroupID3);
		if (tbl != null)
		{
			anim_reaction[2,0] = tbl._reactionID1;
			anim_reaction[2,1] = tbl._reactionID2;
			anim_reaction[2,2] = tbl._reactionID3;
			anim_reaction[2,3] = tbl._reactionID4;
			anim_reaction[2,4] = tbl._reactionID5;
			anim_reaction[2,5] = tbl._reactionID6;
			anim_reaction[2,6] = tbl._reactionID7;
		}


		if (table._reactionGroupID4 == 0) return;
		tbl = (Table_ReactionGroup)TableManager.GetContent(table._reactionGroupID4);
		if (tbl != null)
		{
			anim_reaction[3,0] = tbl._reactionID1;
			anim_reaction[3,1] = tbl._reactionID2;
			anim_reaction[3,2] = tbl._reactionID3;
			anim_reaction[3,3] = tbl._reactionID4;
			anim_reaction[3,4] = tbl._reactionID5;
			anim_reaction[3,5] = tbl._reactionID6;
			anim_reaction[3,6] = tbl._reactionID7;
		}

		if (table._reactionGroupID5 == 0) return;
		tbl = (Table_ReactionGroup)TableManager.GetContent(table._reactionGroupID5);
		if (tbl != null)
		{
			anim_reaction[4,0] = tbl._reactionID1;
			anim_reaction[4,1] = tbl._reactionID2;
			anim_reaction[4,2] = tbl._reactionID3;
			anim_reaction[4,3] = tbl._reactionID4;
			anim_reaction[4,4] = tbl._reactionID5;
			anim_reaction[4,5] = tbl._reactionID6;
			anim_reaction[4,6] = tbl._reactionID7;
		}

		if (table._reactionGroupID6 == 0) return;
		tbl = (Table_ReactionGroup)TableManager.GetContent(table._reactionGroupID6);
		if (tbl != null)
		{
			anim_reaction[5,0] = tbl._reactionID1;
			anim_reaction[5,1] = tbl._reactionID2;
			anim_reaction[5,2] = tbl._reactionID3;
			anim_reaction[5,3] = tbl._reactionID4;
			anim_reaction[5,4] = tbl._reactionID5;
			anim_reaction[5,5] = tbl._reactionID6;
			anim_reaction[5,6] = tbl._reactionID7;
		}

	}

}

public class Knowledge_Mortal_Fighter_Bot_RaidBoss : Knowledge_Mortal_Fighter_Bot
{
	SkillInfo[] _info_blitz = new SkillInfo[2];

	protected ObscuredInt _attackPoint_blitz_A = 0;
	protected ObscuredInt _attackPoint_blitz_B = 0;


	public ObscuredInt attackPoint(eBlitzType type)
	{
		if (_info_blitz[(int)type] == null) return 0;

		return _info_blitz[(int)type]._attackPoint_blitz;
	}


	public override void setAttributesFromTable(Table_Enemy enemyTable)
	{
		base.setAttributesFromTable(enemyTable);


		BattleBase.Instance.RaidbossSearchingTime = (float)(enemyTable._raid_boss_searching_time) * 0.1f;

		//jks 2016.6.13  레이드 보스 전체스킬은 공격력을 스킬에서 가져오게 수정.

		int skillID = enemyTable._raid_boss_blitz_skill_ID1;
		if (skillID > 0)
		{
			_info_blitz[0] = new SkillInfo();
			Table_Skill skillTable = (Table_Skill)TableManager.GetContent(skillID);
			_info_blitz[0].setAttributesFromTable(skillTable);
		}

		skillID = enemyTable._raid_boss_blitz_skill_ID2;
		if (skillID > 0)
		{
			_info_blitz[1] = new SkillInfo();
			Table_Skill skillTable = (Table_Skill)TableManager.GetContent(skillID);
			_info_blitz[1].setAttributesFromTable(skillTable);
		}

	}



	//jks boss raid give damage 
	public ObscuredInt giveDamage_Blitz(GameObject target, eBlitzType blitzType)
	{
		if (target == null) return 0;

		Knowledge_Mortal_Fighter_Main knowledgeOpponent = target.GetComponent<Knowledge_Mortal_Fighter_Main>();

		#if UNITY_EDITOR
		if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
		{
			Log.print_always("   --------------------------------- 레이드 보스 전면 공격 ---------------------------------");
		}
		#endif

		eHitType hitType = getFinalHitType(knowledgeOpponent);

		int hitReactionAnimID = getReaction(hitType, blitzType);  //jks skill knows reactions

		ObscuredInt finalAttack = attackPoint(blitzType);

//		if (knowledgeOpponent.IsLeader || TestOption.Instance()._classRelationBuffAll)
//		{
////jks 2016.6.28 기획요청.			ObscuredInt classRelationAttackPoint = calculate_ClassRelation_AttackPoint(finalAttack, knowledgeOpponent);
//			//jks 2015.8.26 no more: int leaderBuffAttackPoint = calculate_LeaderBuff_AttackPoint_Opponent();
//			//jks 2015.5.8 remove leader strategy-				int leaderStrategyAttack = calculate_LeaderStrategy_AttackPoint();
//
////jks 2016.6.28 기획요청.			finalAttack = finalAttack + classRelationAttackPoint;//jks 2015.8.26 no more:  + leaderBuffAttackPoint;//jks 2015.5.8 remove leader strategy-	 + leaderStrategyAttack;
//
//
//			#if UNITY_EDITOR
//			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
//			{
//				//Log.print("   --------------------------------- Blitz ---------------------------------");
//				//if (BattleBase.Instance.LeaderTransform)
//					//Log.print_always("giveDamage_Blitz   현재 리더 클래스: "+ BattleBase.Instance.LeaderClass + "   :  " + BattleBase.Instance.LeaderTransform.gameObject.name);
//				Log.print_always("giveDamage_Blitz   공격자 : " + gameObject.name + "  -->  피해자: " + knowledgeOpponent.name);
//				//Log.print_always("giveDamage_Blitz   공격자 클래스 : " + Class + "  -->  피해자 클래스: " + knowledgeOpponent.Class + "   피격 타입: " + hitType);
//				//Log.print_always("giveDamage_Blitz   G I V E  D A M A G E      기본 공격력: " + attackPoint(blitzType) + "  +  클래스상성 공격력: " + classRelationAttackPoint + "  =  " + finalAttack);
//				Log.print_always("giveDamage_Blitz   G I V E  D A M A G E      기본 공격력: " + attackPoint(blitzType) + " 최종공격력  =  " + finalAttack);
//			}
//			#endif
//		}
//		else
		{
			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
			{
				Log.print_always("giveDamage_Blitz   공격자 : " + gameObject.name + "  -->  피해자: " + knowledgeOpponent.name);
				//Log.print_always("giveDamage_Blitz   피격 타입: " + hitType);
				Log.print_always("giveDamage_Blitz   G I V E  D A M A G E      기본 공격력: " + finalAttack);
			}
			#endif
		}

		ObscuredInt givenDamage = knowledgeOpponent.takeDamage_Blitz(finalAttack, hitReactionAnimID, hitType, gameObject);

		return givenDamage;

	}


	protected int getReaction(eHitType hitType, eBlitzType blitzType)
	{
		int hitReactionAnimID;

		if (hitType == eHitType.HT_CRITICAL)
		{
			hitReactionAnimID = getReactionAnimID(TotalCombo, hitType, blitzType);
		}
		else 
		{
			hitReactionAnimID = getReactionAnimID(ComboRecent, hitType, blitzType); 
		}

		return hitReactionAnimID;
	}


	protected int getReactionAnimID(int combo, eHitType hitType, eBlitzType blitzType)
	{
		if(combo <= 0) //jks HACK: safty check
			combo = 1;

		int reactionChoice;

		if (hitType == eHitType.HT_CRITICAL)
		{
			reactionChoice = Random.Range(4, 6);  //jks critical reaction only
		}
		else if (hitType == eHitType.HT_MISS || hitType == eHitType.HT_BAD)
		{
			//jks  //jks-  reactionChoice = 3;  
			Log.nprint(gameObject.name + "    getReactionAnimID()   _anim_protect: "+ _anim_protect + "hitType: "+ hitType);
			return _anim_protect;//jks protect motion
		}
		else 
		{
			reactionChoice = Random.Range(0, 3);
		}

		Log.nprint(gameObject.name + "    combo-1: "+ (combo-1) + "   reaction choice: "+ reactionChoice + "     reaciton anim #: "+ _info_blitz[(int)blitzType]._anim_reaction_blitz[combo-1,reactionChoice]);

		return _info_blitz[(int)blitzType]._anim_reaction_blitz[combo-1,reactionChoice]; //jks index 0 == combo1, index 1 == combo2, index 2 == combo3
	}


	protected override void alignPositionZ(Transform trm, float z)
	{
		//jks raid 보스는 z = 4 로 고정. (이펙트 보스 안으로 들어가 그려지지 않는 이슈에 대한 보정.)
		Vector3 newPosition = trm.position;
		newPosition.z = 4;
		trm.position = newPosition;
	}

}
