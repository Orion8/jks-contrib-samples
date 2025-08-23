using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;



public class PerSkillStatus
{
	public int _skill_ID;
	public eAttackType _attackType;

	//jks skill
	public float _attackDistanceMin = 0.5f;
	public float _attackDistanceMax = 1.0f;
	public float _damageRange = 1.0f;
	public byte _totalCombo = 1;
	public byte _lastCombo = 1;
	
	public float _attackInfo;
	public int _attackInfo2;
	public int _attackInfo3;
	public string _attackInfo4;

	public float _projectileRadius;//_attackInfo5;
	public int _damage_frequency;

	//jks for Meteor -  AttackInfo2 : damage duration in second. (table)1 = (code)0.01  
	public float _damage_duration;
	
	public eWeaponType_ForAnimation _weaponType_ForAnimation;

	public bool _skillCompleted = false;

	public int _attack_point_raidboss; //jks 2016.6.13 레이드 보스는 스킬 별로 다른 공격력 가지기 위해 추가.

	//jks skill extra info
	//jks skill extra infomation
	public bool _reactionOff = false;
	public float _damagePenetration = 1.0f;
	public float _buff_probability = 0;	//버프 발생 시킬지 판정하는 확율.
	public int _buff_duration = 0;		//버프 시작될 경우, 몇초 지속인지 설정.
	public float _attack_up = 0;			//버프시 증가할 아군의 공격치 %	
	public float _defense_up = 0;		//버프시 증가시킬 아군의 방어력 %
	public float _critical_up = 0;		//버프시 증가시킬 아군의 적중율 %
	public float _attack_down = 0;		//버프시 감소시킬 상대의 공격치 %
	public float _defense_down = 0;		//버프시 감소시킬 상대의 방어력 %
	public float _critical_down = 0;		//버프시 감소시킬 상대의 적중율 %
	public bool _skill_hit_only_buff_on = false;

	public bool _firstGiveDamage = true;

	public int _anim_combo1; //jks combo1 attack animation controller surfix number
	public int _anim_combo2; //jks combo2 -
	public int _anim_combo3; //jks combo3 -
	public int _anim_combo4; //jks combo4 -
	public int _anim_combo5; //jks combo5 -
	public int _anim_combo6; //jks combo6 -
	public int _anim_combo7; //jks combo6 -
	public int _anim_combo8; //jks combo6 -

	public int _weaponPathID;
	public int _weaponProjectilePathID;
	public float _weaponLength;
	
	public byte _mask_combo = 0;   //jks - 해당 bit 이 1이면 액션 시작이라는 의미.
	public byte _mask_combo_anim = 0;  //jks - 해당 bit 이 1이면 애니메이션 시작됨.
	
	public bool _isLastDamageInSkill = false;

	public int _currentCombo = 0;
	public int _recentCombo; //jks used to find reaction type in giveDamage()- usally called later by anim event and _currentCombo may be reset, so save recent combo number here.

	public int[,] _anim_reaction = new int[6,7]{{0,0,0,0,0,0,0},{0,0,0,0,0,0,0},{0,0,0,0,0,0,0},{0,0,0,0,0,0,0},{0,0,0,0,0,0,0},{0,0,0,0,0,0,0}}; //jks _anim_reaction[1,] : combo2 reaction animation controller surfix number.

	public Transform _weaponEnd = null;


}



public class SupportSkillKnowledge : MonoBehaviour 
{
	protected Knowledge_Mortal_Fighter _knowledge; //jks Knowledge component  에 저장되어 있는 정보는 이 레퍼런스로 접근.

	int _active_skillNum;

	protected PerSkillStatus[] _perSkillAttribuites = new PerSkillStatus[6];
	public PerSkillStatus[] Info { get { return _perSkillAttribuites; }}

	//jks weapon bones
	Transform _right_hand;
	Transform _right_foot;
	Transform _left_hand; 
	Transform _left_foot;


	//public bool _everGaveDamageForTheAttack = false; //jks to decide cooltime activation

	public bool[] _everGaveDamageForTheAttack = new bool[6]{false,false,false,false,false,false}; //jks to decide cooltime activation
	public bool EverGaveDamage { get { return _everGaveDamageForTheAttack[ActiveSkillNum];}  set { _everGaveDamageForTheAttack[ActiveSkillNum] = value; }}

	public Knowledge_Mortal_Fighter Knowledge
	{
		get
		{
			if (_knowledge == null)
			{
				_knowledge = GetComponent<Knowledge_Mortal_Fighter>();
			}
			return _knowledge;
		}
	}

	public virtual SupportSkillInfo[] SskillInfo { get { return BattleManager.Instance.SupportSkillInfo; }}
	public AttackCoolTime_SupportSkill getSupportSkillCoolTimer(int skillNum) { return BattleBase.Instance.getSupportSkillCoolTimer(skillNum); }

	protected virtual float Cooltime { get { return SskillInfo[ActiveSkillNum]._coolTime; } }
	public virtual int AttackPoint { 	get { return SskillInfo[ActiveSkillNum]._attackPoint; } }

	public int ActiveSkillNum { get { return _active_skillNum; }  set { _active_skillNum = value; }}

	public float AttackInfo 					{ get { return Info[ActiveSkillNum]._attackInfo; } }
	public int AttackInfo2 						{ get { return Info[ActiveSkillNum]._attackInfo2; } }
	public int AttackInfo3 						{ get { return Info[ActiveSkillNum]._attackInfo3; } }
	public string AttackInfo4 					{ get { return Info[ActiveSkillNum]._attackInfo4; } }

	public int WeaponPathID 					{ get { return Info[ActiveSkillNum]._weaponPathID; } }

	public virtual GameObject WeaponObject				{ get { return BattleBase.Instance.WeaponObjects_SupportSkill[ActiveSkillNum,0]; } }
	public virtual GameObject getWeaponObject(int skillNum)
	{ 
		return BattleBase.Instance.WeaponObjects_SupportSkill[skillNum,0]; 
	}

	public float AttackDistanceMax { get { return Info[ActiveSkillNum]._attackDistanceMax; } }


//	public int getComboCurrent(int skillNum) { return Info[skillNum]._currentCombo; }
//	public int getComboRecent(int skillNum) { return Info[skillNum]._recentCombo; }

//	public void setComboCurrent(int skillNum, int combo) { Info[skillNum]._currentCombo = combo; }
//	public void setComboRecent(int skillNum, int combo) { Info[skillNum]._recentCombo = combo; }

	public int ComboCurrent { get { return Info[ActiveSkillNum]._currentCombo; } set { Info[ActiveSkillNum]._currentCombo = value; }}
	public int ComboRecent { get { return Info[ActiveSkillNum]._recentCombo; } set { Info[ActiveSkillNum]._recentCombo = value; }}

	public int TotalCombo { get { return Info[ActiveSkillNum]._totalCombo; }}

	public bool IsFinalCombo { get { return TotalCombo == ComboCurrent; } }
	

	public bool NoNextComboAction { get { return !Progress_AnyAction; }}	
	
	public bool Progress_Action { get { return  Info[ActiveSkillNum] != null && Info[ActiveSkillNum]._mask_combo > 0;} }
	public bool isActionProgress(int skillNum) {  return  Info[skillNum] != null && Info[skillNum]._mask_combo > 0; }
	public virtual bool Progress_AnyAction { get { return Info[0]._mask_combo > 0 || Info[1]._mask_combo > 0 || Info[2]._mask_combo > 0;} }

	public bool Progress_SkillAnimation { get { return Info[ActiveSkillNum]._mask_combo_anim > 0; }}
	public virtual bool Progress_AnySkillAnimation { get { return Info[0]._mask_combo_anim > 0 || Info[1]._mask_combo_anim > 0 || Info[2]._mask_combo_anim > 0;} }


//	public bool combo_Action(int skillNum, int combo) { return Utility.isBitOn(Info[skillNum]._mask_combo, combo); }
//	public void set_combo_Action(int skillNum, int combo, bool value) { Utility.setBit(Info[skillNum]._mask_combo, combo, value); }
	public bool Action_Combo1 { get { if (Info[ActiveSkillNum] == null) return false; return (Info[ActiveSkillNum]._mask_combo & Utility.BIT_1) > 0; } set { if (value) Info[ActiveSkillNum]._mask_combo |= Utility.BIT_1; else Info[ActiveSkillNum]._mask_combo &= Utility.NOT_BIT_1; }}
	public bool Action_Combo2 { get { if (Info[ActiveSkillNum] == null) return false; return (Info[ActiveSkillNum]._mask_combo & Utility.BIT_2) > 0; } set { if (value) Info[ActiveSkillNum]._mask_combo |= Utility.BIT_2; else Info[ActiveSkillNum]._mask_combo &= Utility.NOT_BIT_2; }}
	public bool Action_Combo3 { get { if (Info[ActiveSkillNum] == null) return false; return (Info[ActiveSkillNum]._mask_combo & Utility.BIT_3) > 0; } set { if (value) Info[ActiveSkillNum]._mask_combo |= Utility.BIT_3; else Info[ActiveSkillNum]._mask_combo &= Utility.NOT_BIT_3; }}
	public bool Action_Combo4 { get { if (Info[ActiveSkillNum] == null) return false; return (Info[ActiveSkillNum]._mask_combo & Utility.BIT_4) > 0; } set { if (value) Info[ActiveSkillNum]._mask_combo |= Utility.BIT_4; else Info[ActiveSkillNum]._mask_combo &= Utility.NOT_BIT_4; }}
	public bool Action_Combo5 { get { if (Info[ActiveSkillNum] == null) return false; return (Info[ActiveSkillNum]._mask_combo & Utility.BIT_5) > 0; } set { if (value) Info[ActiveSkillNum]._mask_combo |= Utility.BIT_5; else Info[ActiveSkillNum]._mask_combo &= Utility.NOT_BIT_5; }}
	public bool Action_Combo6 { get { if (Info[ActiveSkillNum] == null) return false; return (Info[ActiveSkillNum]._mask_combo & Utility.BIT_6) > 0; } set { if (value) Info[ActiveSkillNum]._mask_combo |= Utility.BIT_6; else Info[ActiveSkillNum]._mask_combo &= Utility.NOT_BIT_6; }}
	public bool Action_Combo7 { get { if (Info[ActiveSkillNum] == null) return false; return (Info[ActiveSkillNum]._mask_combo & Utility.BIT_7) > 0; } set { if (value) Info[ActiveSkillNum]._mask_combo |= Utility.BIT_7; else Info[ActiveSkillNum]._mask_combo &= Utility.NOT_BIT_7; }}
	public bool Action_Combo8 { get { if (Info[ActiveSkillNum] == null) return false; return (Info[ActiveSkillNum]._mask_combo & Utility.BIT_8) > 0; } set { if (value) Info[ActiveSkillNum]._mask_combo |= Utility.BIT_8; else Info[ActiveSkillNum]._mask_combo &= Utility.NOT_BIT_8; }}

	public bool combo_Animation(int skillNum, int combo) { return Utility.isBitOn(Info[skillNum]._mask_combo_anim, combo); }

	public void set_combo_Animation(int skillNum, int combo, bool value) { Utility.setBit(ref Info[skillNum]._mask_combo_anim, combo, value); }

	public void reset_combo_Action(int skillNum) { if (Info[skillNum] != null) 
													{
														Info[skillNum]._mask_combo = 0; 
														Info[skillNum]._firstGiveDamage= true;
												  } }
	
	public void reset_combo_Animation(int skillNum) { if (Info[skillNum] != null) Info[skillNum]._mask_combo_anim = 0; }

	public bool isActionInProgress(int skillNum) { return Info[skillNum]._mask_combo > 0; }
	public bool isSkillInProgress(int skillNum) { return Info[skillNum]._mask_combo_anim > 0; }





//	public bool FirstGiveDamage 
//	{ 
//		get 
//		{ 
//			if (Info[ActiveSkillNum] == null) 
//				return false; 
//			return (Info[ActiveSkillNum]._firstGiveDamage;
//		} 
//		set 
//		{ 
//			Info[ActiveSkillNum]._firstGiveDamage = value; 
//		} 
//	}

	//jks 2016.3.14 skill buff 기능 추가.
	public void addSkillBuff()
	{
		if (Info[ActiveSkillNum] == null) return;

		if (Info[ActiveSkillNum]._firstGiveDamage)
		{
			// 스킬버프 시동 할지 판정.
			if (Info[ActiveSkillNum]._buff_probability > 0)
			if ( Random.Range(0f, 1f) < Info[ActiveSkillNum]._buff_probability )
				BattleBase.Instance.addSkillBuffAgent_SupportSkill(this);

			Info[ActiveSkillNum]._firstGiveDamage = false;
		}
	}



	public void setAttributesFromTable(int skillNum, Table_Skill_Info_Attack tbl)
	{
		Info[skillNum]._reactionOff = tbl._reaction_off != 0;
		Info[skillNum]._damagePenetration = tbl._damage_penetration * 0.01f;
		Info[skillNum]._buff_probability = tbl._buff_probability * 0.01f;	
		Info[skillNum]._buff_duration = tbl._buff_duration;		
		Info[skillNum]._attack_up = tbl._attack_up * 0.01f;			
		Info[skillNum]._defense_up = tbl._defense_up * 0.01f;		
		Info[skillNum]._critical_up = tbl._critical_up * 0.01f;		
		Info[skillNum]._attack_down = tbl._attack_down * 0.01f;		
		Info[skillNum]._defense_down = tbl._defense_down * 0.01f;		
		Info[skillNum]._critical_down = tbl._critical_down * 0.01f;		
		Info[skillNum]._skill_hit_only_buff_on = tbl._skill_hit_only_buff_on == 1;
	}


	public float Attack_up 			{ get { return Info[ActiveSkillNum]._attack_up; } }
	public float Defense_up 		{ get { return Info[ActiveSkillNum]._defense_up; } }
	public float Critical_up 		{ get { return Info[ActiveSkillNum]._critical_up; } }
	public float Attack_down 		{ get { return Info[ActiveSkillNum]._attack_down; } }
	public float Defense_down 		{ get { return Info[ActiveSkillNum]._defense_down; } }
	public float Critical_down 		{ get { return Info[ActiveSkillNum]._critical_down; } }

	public int Buff_duration 		{ get { return Info[ActiveSkillNum]._buff_duration; } }
	public float Buff_probability 	{ get { return Info[ActiveSkillNum]._buff_probability; } }
	public float DamagePenetration 	{ get { return Info[ActiveSkillNum]._damagePenetration; } }
	public bool ReactionOff 		{ get { return Info[ActiveSkillNum]._reactionOff; } }
	public bool SkillHitOnlyBuffOn 	{ get { return Info[ActiveSkillNum]._skill_hit_only_buff_on; } }



	public bool Progress_Anim_Combo1 { get { return (Info[ActiveSkillNum]._mask_combo_anim & Utility.BIT_1) > 0; } set { if (value) Info[ActiveSkillNum]._mask_combo_anim |= Utility.BIT_1; else Info[ActiveSkillNum]._mask_combo_anim &= Utility.NOT_BIT_1; }}
	public bool Progress_Anim_Combo2 { get { return (Info[ActiveSkillNum]._mask_combo_anim & Utility.BIT_2) > 0; } set { if (value) Info[ActiveSkillNum]._mask_combo_anim |= Utility.BIT_2; else Info[ActiveSkillNum]._mask_combo_anim &= Utility.NOT_BIT_2; }}
	public bool Progress_Anim_Combo3 { get { return (Info[ActiveSkillNum]._mask_combo_anim & Utility.BIT_3) > 0; } set { if (value) Info[ActiveSkillNum]._mask_combo_anim |= Utility.BIT_3; else Info[ActiveSkillNum]._mask_combo_anim &= Utility.NOT_BIT_3; }}
	public bool Progress_Anim_Combo4 { get { return (Info[ActiveSkillNum]._mask_combo_anim & Utility.BIT_4) > 0; } set { if (value) Info[ActiveSkillNum]._mask_combo_anim |= Utility.BIT_4; else Info[ActiveSkillNum]._mask_combo_anim &= Utility.NOT_BIT_4; }}
	public bool Progress_Anim_Combo5 { get { return (Info[ActiveSkillNum]._mask_combo_anim & Utility.BIT_5) > 0; } set { if (value) Info[ActiveSkillNum]._mask_combo_anim |= Utility.BIT_5; else Info[ActiveSkillNum]._mask_combo_anim &= Utility.NOT_BIT_5; }}
	public bool Progress_Anim_Combo6 { get { return (Info[ActiveSkillNum]._mask_combo_anim & Utility.BIT_6) > 0; } set { if (value) Info[ActiveSkillNum]._mask_combo_anim |= Utility.BIT_6; else Info[ActiveSkillNum]._mask_combo_anim &= Utility.NOT_BIT_6; }}
	public bool Progress_Anim_Combo7 { get { return (Info[ActiveSkillNum]._mask_combo_anim & Utility.BIT_7) > 0; } set { if (value) Info[ActiveSkillNum]._mask_combo_anim |= Utility.BIT_7; else Info[ActiveSkillNum]._mask_combo_anim &= Utility.NOT_BIT_7; }}
	public bool Progress_Anim_Combo8 { get { return (Info[ActiveSkillNum]._mask_combo_anim & Utility.BIT_8) > 0; } set { if (value) Info[ActiveSkillNum]._mask_combo_anim |= Utility.BIT_8; else Info[ActiveSkillNum]._mask_combo_anim &= Utility.NOT_BIT_8; }}



	public int Anim_Combo1 		{ get { return Info[ActiveSkillNum]._anim_combo1; } }
	public int Anim_Combo2 		{ get { return Info[ActiveSkillNum]._anim_combo2; } }
	public int Anim_Combo3 		{ get { return Info[ActiveSkillNum]._anim_combo3; } }
	public int Anim_Combo4 		{ get { return Info[ActiveSkillNum]._anim_combo4; } }
	public int Anim_Combo5 		{ get { return Info[ActiveSkillNum]._anim_combo5; } }
	public int Anim_Combo6 		{ get { return Info[ActiveSkillNum]._anim_combo6; } }
	public int Anim_Combo7 		{ get { return Info[ActiveSkillNum]._anim_combo7; } }
	public int Anim_Combo8 		{ get { return Info[ActiveSkillNum]._anim_combo8; } }

	
	public int getAnimHash(string animStateName)
	{
		return Animator.StringToHash("Base Layer." + animStateName);
	}


	public void setAnimCombo1(int whichSkill, int attackID) 	{ Info[whichSkill]._anim_combo1 = getAnimHash("Attack" + attackID.ToString()); Log.nprint("지원 skill: "+ whichSkill +" - - - combo1 - - - "+ "Attack" + attackID.ToString() + " hash: "+ Info[whichSkill]._anim_combo1); }
	public void setAnimCombo2(int whichSkill, int attackID) 	{ Info[whichSkill]._anim_combo2 = getAnimHash("Attack" + attackID.ToString()); Log.nprint("지원 skill: "+ whichSkill +" - - - combo2 - - - "+ "Attack" + attackID.ToString() + " hash: "+ Info[whichSkill]._anim_combo2); }
	public void setAnimCombo3(int whichSkill, int attackID) 	{ Info[whichSkill]._anim_combo3 = getAnimHash("Attack" + attackID.ToString()); Log.nprint("지원 skill: "+ whichSkill +" - - - combo3 - - - "+ "Attack" + attackID.ToString() + " hash: "+ Info[whichSkill]._anim_combo3); }
	public void setAnimCombo4(int whichSkill, int attackID) 	{ Info[whichSkill]._anim_combo4 = getAnimHash("Attack" + attackID.ToString()); Log.nprint("지원 skill: "+ whichSkill +" - - - combo4 - - - "+ "Attack" + attackID.ToString() + " hash: "+ Info[whichSkill]._anim_combo4); }
	public void setAnimCombo5(int whichSkill, int attackID) 	{ Info[whichSkill]._anim_combo5 = getAnimHash("Attack" + attackID.ToString()); Log.nprint("지원 skill: "+ whichSkill +" - - - combo5 - - - "+ "Attack" + attackID.ToString() + " hash: "+ Info[whichSkill]._anim_combo5); }
	public void setAnimCombo6(int whichSkill, int attackID) 	{ Info[whichSkill]._anim_combo6 = getAnimHash("Attack" + attackID.ToString()); Log.nprint("지원 skill: "+ whichSkill +" - - - combo6 - - - "+ "Attack" + attackID.ToString() + " hash: "+ Info[whichSkill]._anim_combo6); }
	public void setAnimCombo7(int whichSkill, int attackID) 	{ Info[whichSkill]._anim_combo7 = getAnimHash("Attack" + attackID.ToString()); Log.nprint("지원 skill: "+ whichSkill +" - - - combo7 - - - "+ "Attack" + attackID.ToString() + " hash: "+ Info[whichSkill]._anim_combo7); }
	public void setAnimCombo8(int whichSkill, int attackID) 	{ Info[whichSkill]._anim_combo8 = getAnimHash("Attack" + attackID.ToString()); Log.nprint("지원 skill: "+ whichSkill +" - - - combo8 - - - "+ "Attack" + attackID.ToString() + " hash: "+ Info[whichSkill]._anim_combo8); }



	public eAttackType AttackType { get { return Info[ActiveSkillNum]._attackType; } }

	public float ProjectileRadius { get { return Info[ActiveSkillNum]._projectileRadius; } }

	public virtual void setAttributesFromTable(int skillNum, Table_Skill tbl)
	{
		if (Info[skillNum] == null)
			Info[skillNum] = new PerSkillStatus();

		Info[skillNum]._skill_ID = tbl.ID;
		Info[skillNum]._attackDistanceMin = tbl._attackDistanceMin * 0.1f;  //jks fixed point conversion (0~255 to 0~25.5m)
		Info[skillNum]._attackDistanceMax = tbl._attackDistanceMax * 0.1f;
		Info[skillNum]._damageRange = tbl._damageRange * 0.1f;
		Info[skillNum]._totalCombo = tbl._totalCombo;
		Info[skillNum]._lastCombo = tbl._lastCombo;
		Info[skillNum]._attackType = (eAttackType)tbl._attackType;
		Info[skillNum]._attackInfo = tbl._attackInfo;

		Info[skillNum]._attackInfo3 = tbl._attackInfo3;
		Info[skillNum]._attackInfo4 = tbl._attackInfo4;

		if (Info[skillNum]._attackType == eAttackType.AT_Meteor)
		{
			Info[skillNum]._damage_duration = tbl._attackInfo2 * 0.01f;
			float damage_interval = Info[skillNum]._attackInfo3 * 0.1f;
			if (damage_interval == 0)
				damage_interval = 0.05f; //jks give minimum interval
			Info[skillNum]._damage_frequency = Mathf.RoundToInt(Info[skillNum]._damage_duration / damage_interval);
		}
		else
		{
			Info[skillNum]._attackInfo2 = tbl._attackInfo2;
			Info[skillNum]._damage_frequency = tbl._attackInfo6;
			if (Info[skillNum]._damage_frequency < 1)
				Info[skillNum]._damage_frequency = 1;
		}

		Info[skillNum]._attack_point_raidboss = tbl._attack_point_raidboss;

		Info[skillNum]._projectileRadius = (float)tbl._attackInfo5 * 0.001f;  //jks 1000 = 1m

		Info[skillNum]._weaponType_ForAnimation = (eWeaponType_ForAnimation)tbl._weaponType;

		Info[skillNum]._weaponPathID = tbl._assetPathID_Weapon;

		setAnimCombo1(skillNum, tbl._attack1);
		setAnimCombo2(skillNum, tbl._attack2);
		setAnimCombo3(skillNum, tbl._attack3); 
		setAnimCombo4(skillNum, tbl._attack4); 
		setAnimCombo5(skillNum, tbl._attack5); 
		setAnimCombo6(skillNum, tbl._attack6); 
		setAnimCombo7(skillNum, tbl._attack7); 
		setAnimCombo8(skillNum, tbl._attack8); 

		Table_ReactionType tableReaction = (Table_ReactionType)TableManager.GetContent(tbl._reactionTypeID);
		setAttributesFromTable(skillNum, tableReaction);

		StartCoroutine( updateWeaponInfo(skillNum));


		if (tbl._skill_info_attack_id > 0)
		{
			Table_Skill_Info_Attack tableInfo = (Table_Skill_Info_Attack)TableManager.GetContent(tbl._skill_info_attack_id);
			if (tableInfo != null)
				setAttributesFromTable(skillNum, tableInfo);
		}


	}

	




	void setAttributesFromTable(int skillNum, Table_ReactionType table)
	{
		Table_ReactionGroup tbl = (Table_ReactionGroup)TableManager.GetContent(table._reactionGroupID1);
		
		if (tbl != null)
		{
			Info[skillNum]._anim_reaction[0,0] = tbl._reactionID1;
			Info[skillNum]._anim_reaction[0,1] = tbl._reactionID2;
			Info[skillNum]._anim_reaction[0,2] = tbl._reactionID3;
			Info[skillNum]._anim_reaction[0,3] = tbl._reactionID4;
			Info[skillNum]._anim_reaction[0,4] = tbl._reactionID5;
			Info[skillNum]._anim_reaction[0,5] = tbl._reactionID6;
			Info[skillNum]._anim_reaction[0,6] = tbl._reactionID7;
		}
		
		if (table._reactionGroupID2 == 0) return;
		tbl = (Table_ReactionGroup)TableManager.GetContent(table._reactionGroupID2);
		if (tbl != null)
		{
			Info[skillNum]._anim_reaction[1,0] = tbl._reactionID1;
			Info[skillNum]._anim_reaction[1,1] = tbl._reactionID2;
			Info[skillNum]._anim_reaction[1,2] = tbl._reactionID3;
			Info[skillNum]._anim_reaction[1,3] = tbl._reactionID4;
			Info[skillNum]._anim_reaction[1,4] = tbl._reactionID5;
			Info[skillNum]._anim_reaction[1,5] = tbl._reactionID6;
			Info[skillNum]._anim_reaction[1,6] = tbl._reactionID7;
		}
		
		if (table._reactionGroupID3 == 0) return;
		tbl = (Table_ReactionGroup)TableManager.GetContent(table._reactionGroupID3);
		if (tbl != null)
		{
			Info[skillNum]._anim_reaction[2,0] = tbl._reactionID1;
			Info[skillNum]._anim_reaction[2,1] = tbl._reactionID2;
			Info[skillNum]._anim_reaction[2,2] = tbl._reactionID3;
			Info[skillNum]._anim_reaction[2,3] = tbl._reactionID4;
			Info[skillNum]._anim_reaction[2,4] = tbl._reactionID5;
			Info[skillNum]._anim_reaction[2,5] = tbl._reactionID6;
			Info[skillNum]._anim_reaction[2,6] = tbl._reactionID7;
		}
		
		
		if (table._reactionGroupID4 == 0) return;
		tbl = (Table_ReactionGroup)TableManager.GetContent(table._reactionGroupID4);
		if (tbl != null)
		{
			Info[skillNum]._anim_reaction[3,0] = tbl._reactionID1;
			Info[skillNum]._anim_reaction[3,1] = tbl._reactionID2;
			Info[skillNum]._anim_reaction[3,2] = tbl._reactionID3;
			Info[skillNum]._anim_reaction[3,3] = tbl._reactionID4;
			Info[skillNum]._anim_reaction[3,4] = tbl._reactionID5;
			Info[skillNum]._anim_reaction[3,5] = tbl._reactionID6;
			Info[skillNum]._anim_reaction[3,6] = tbl._reactionID7;
		}
		
		if (table._reactionGroupID5 == 0) return;
		tbl = (Table_ReactionGroup)TableManager.GetContent(table._reactionGroupID5);
		if (tbl != null)
		{
			Info[skillNum]._anim_reaction[4,0] = tbl._reactionID1;
			Info[skillNum]._anim_reaction[4,1] = tbl._reactionID2;
			Info[skillNum]._anim_reaction[4,2] = tbl._reactionID3;
			Info[skillNum]._anim_reaction[4,3] = tbl._reactionID4;
			Info[skillNum]._anim_reaction[4,4] = tbl._reactionID5;
			Info[skillNum]._anim_reaction[4,5] = tbl._reactionID6;
			Info[skillNum]._anim_reaction[4,6] = tbl._reactionID7;
		}
		
		if (table._reactionGroupID6 == 0) return;
		tbl = (Table_ReactionGroup)TableManager.GetContent(table._reactionGroupID6);
		if (tbl != null)
		{
			Info[skillNum]._anim_reaction[5,0] = tbl._reactionID1;
			Info[skillNum]._anim_reaction[5,1] = tbl._reactionID2;
			Info[skillNum]._anim_reaction[5,2] = tbl._reactionID3;
			Info[skillNum]._anim_reaction[5,3] = tbl._reactionID4;
			Info[skillNum]._anim_reaction[5,4] = tbl._reactionID5;
			Info[skillNum]._anim_reaction[5,5] = tbl._reactionID6;
			Info[skillNum]._anim_reaction[5,6] = tbl._reactionID7;
		}
	}



	void Awake()
	{
		initVariables();
	}



	void Start()
	{
		findWeaponBones();
	}


	protected virtual void initVariables()
	{
	}


	public virtual void forceResetComboFlags()
	{
		reset_combo_Action(0);
		reset_combo_Action(1);
		reset_combo_Action(2);

		reset_combo_Animation(0);
		reset_combo_Animation(1);
		reset_combo_Animation(2);

//		if (DoIHoldOpponent)
//		{
//			releaseOpponent();
//		}
	}






	public IEnumerator updateWeaponInfo(int skillNum)
	{
		while (Knowledge == null)
			yield return null;

		float legLength = Knowledge.Height * 0.5f;

		GameObject curWeapon = getWeaponObject(skillNum);
		
		if (curWeapon == null) 
		{
			//_weaponLength = defaultWeaponLength; //default
			Info[skillNum]._weaponLength = legLength;
			yield break;
		}
		
		Transform curWeaponTransform = Utility.findTransformUsingKeyword(curWeapon, "tip"); //jks if we have weapon, find tip.
		
		if (curWeaponTransform == null) //jks arrow
		{
			//_weaponLength = 0.7f;
			Info[skillNum]._weaponLength = legLength;
			yield break;
		}
		
		Info[skillNum]._weaponLength = Mathf.Abs(curWeaponTransform.localPosition.x);
		
		Info[skillNum]._weaponLength += legLength;
	}

	
	
	public void findWeaponBones()
	{
		_right_hand = Utility.findTransformUsingKeyword(gameObject, "bone_weapon_r"); //jks if no weapon, use weapon bone. 
		if (_right_hand == null) Debug.LogError(" Can not find a bone with keyword: 'bone_weapon_r' ");
		
		_left_hand = Utility.findTransformUsingKeyword(gameObject, "bone_weapon_l");
		if (_left_hand == null) Debug.LogError(" Can not find a bone with keyword: 'bone_weapon_l' ");
		
		_right_foot = Utility.findTransformUsingKeyword(gameObject, "r toe");
		if (_right_foot == null) Debug.LogError(" Can not find a bone with keyword: 'r toe' ");
		
		_left_foot = Utility.findTransformUsingKeyword(gameObject, "l toe");
		if (_left_foot == null) Debug.LogError(" Can not find a bone with keyword: 'l toe' ");
		
	}



	
	public Vector3 WeaponEndPosition(int skillNum)
	{
		Vector3 endPosition;
		endPosition = transform.position;
		
		if (_right_foot == null || _left_foot == null || _right_hand == null || _left_hand == null)
			findWeaponBones();

		GameObject curWeapon = getWeaponObject(skillNum);
		
		//jks if no weapon, dynamically get position of hand or foot bone.
		if (curWeapon == null)
		{
			if (transform.forward.x > 0)
			{
				endPosition.x = Mathf.Max(Mathf.Max(_right_hand.position.x, _left_hand.position.x), Mathf.Max(_right_foot.position.x, _left_foot.position.x));
				endPosition.x = Mathf.Max(endPosition.x, (transform.position.x + Knowledge.Radius));
				//endPosition.x += Radius;
			}
			else
			{
				endPosition.x = Mathf.Min(Mathf.Min(_right_hand.position.x, _left_hand.position.x), Mathf.Min(_right_foot.position.x, _left_foot.position.x));
				endPosition.x = Mathf.Min(endPosition.x, (transform.position.x - Knowledge.Radius));
				//endPosition.x -= Radius;
			}
			
			endPosition.y = Mathf.Max(Mathf.Max(_right_hand.position.y, _left_hand.position.y), Mathf.Max(_right_foot.position.y, _left_foot.position.y));
			
			return endPosition;
		}
		
		//jks if we have weapon, get from tip bone postion and compare to body front
		if (Info[skillNum]._weaponEnd == null)
		{		
			Info[skillNum]._weaponEnd = Utility.findTransformUsingKeyword(curWeapon, "tip"); //jks if we have weapon, find tip.
			if (Info[skillNum]._weaponEnd == null)
			{
				Log.Warning(" Weapon '"+ curWeapon + "' does not have a 'tip' child object.");
				Info[skillNum]._weaponEnd = Utility.findTransformUsingKeyword(gameObject, "bone_weapon_r"); //jks if no tip, use weapon bone for now.
			}
		}
		
		if (transform.forward.x > 0)
		{
			endPosition.x = Mathf.Max(Info[skillNum]._weaponEnd.position.x, (transform.position.x + Knowledge.Radius));
		}
		else
		{
			endPosition.x = Mathf.Min(Info[skillNum]._weaponEnd.position.x, (transform.position.x - Knowledge.Radius));
		}
		
		endPosition.y = Mathf.Max(Mathf.Max(Info[skillNum]._weaponEnd.position.y, transform.position.y + Knowledge.Height));
		
		return endPosition;
	}




//	public void keepOpponentAtWeaponEnd(GameObject opponent, float penetration)
//	{
//		float opponentRadius = opponent.GetComponent<Knowledge_Mortal_Fighter>().Radius;
//		
//		Vector3 newPosition = opponent.transform.position;
//		
//		if (opponentRadius < penetration)  //jks for very thin character,  penetrate only 20 % of it's radius
//		{
//			opponentRadius += opponentRadius * 0.2f;
//		}
//
//
//		float weaponEndx = WeaponEndPosition(ActiveSkillNum).x;
//
//		if (transform.forward.x > 0)
//		{
//			if (weaponEndx > opponent.transform.position.x - opponentRadius)
//			{
//				newPosition.x = weaponEndx + opponentRadius - penetration;
//				newPosition.x = Mathf.Max(newPosition.x, opponent.transform.position.x);
//			}
//		}
//		else
//		{
//			if (weaponEndx < opponent.transform.position.x + opponentRadius)
//			{
//				newPosition.x = weaponEndx - opponentRadius + penetration;
//				newPosition.x = Mathf.Min(newPosition.x, opponent.transform.position.x);
//			}
//		}
//
//		opponent.transform.position = newPosition;
//		
//		//if (opponent.name.Contains("C"))
//		//Log.jprint(opponent + "  ~ ~ ~ ~ ~   keepOpponentAtWeaponEnd()     position: "+ opponent.transform.position);
//		
//	}


	public void keepOpponentAtWeaponEnd(GameObject opponent, float penetration)
	{
		float opponentRadius = opponent.GetComponent<Knowledge_Mortal_Fighter>().Radius;

		//Profiler.BeginSample("222");
		float newX = opponent.transform.position.x;

		if (opponentRadius < penetration)  //jks for very thin character,  penetrate only 20 % of it's radius
		{
			opponentRadius += opponentRadius * 0.2f;
		}
		//Profiler.EndSample();


		float weaponEndx = WeaponEndPosition(ActiveSkillNum).x;


		//Profiler.BeginSample("333");
		if (transform.forward.x > 0)
		{
			//Profiler.BeginSample("334");
			if (weaponEndx > opponent.transform.position.x - opponentRadius)
			{
				newX = weaponEndx + opponentRadius - penetration;
				newX = Mathf.Max(newX, opponent.transform.position.x);
			}
			//Profiler.EndSample();
		}
		else
		{
			//Profiler.BeginSample("335");
			if (weaponEndx < opponent.transform.position.x + opponentRadius)
			{
				newX = weaponEndx - opponentRadius + penetration;
				newX = Mathf.Min(newX, opponent.transform.position.x);
			}
			//Profiler.EndSample();
		}
		//Profiler.EndSample();

		newX = Mathf.Abs(newX - opponent.transform.position.x);

		Vector3 myPosition = transform.position;
		Vector3 opPosition = opponent.transform.position;

		myPosition.x += newX * (1 - TestOption.Instance()._battle_attack_push_opponent_weight) * ( transform.forward.x > 0 ? -1 : 1 );
		opPosition.x += newX * TestOption.Instance()._battle_attack_push_opponent_weight * ( transform.forward.x > 0 ? -1 : 1 );

		opponent.transform.position = opPosition;
		transform.position = myPosition;

		//if (opponent.name.Contains("C"))
			//Log.jprint(opponent + "  ~ ~ ~ ~ ~   keepOpponentAtWeaponEnd()     position: "+ opponent.transform.position);

	}



	public void installSSWeapon()
	{
		if (WeaponObject == null) return; //jks means no weapon
		
		Weapon_Hold weapon = WeaponObject.GetComponent<Weapon_Hold>();
		if (weapon == null)  //jks throw 타입 경우 예) 폭탄.  는 weapon script 없을 수 있음.
		{
			Debug.LogWarning("Can't find Weapon script on (throw 타입 무기 경우는 없을 수 있음.) ");
		}
			
		//weapon.setPairItemPath(tablePath._assetPath);
		weapon.install_SS(gameObject);
		
		//jks make it visible
		weapon.show(true);
	}


	public void hideSSWeaponAndShowDefault()
	{
		//jks hide support skill weapon
		hideSSWeaponAll();

		//jks show support skill weapon
		Knowledge.showWeapon(true);
	}


	protected virtual void hideSSWeaponAll()
	{
		hideSSWeapon(0);
		hideSSWeapon(1);
		hideSSWeapon(2);
	}

	protected void hideSSWeapon(int skillNum)
	{
		GameObject wpObj = getWeaponObject(skillNum);
		
		if (wpObj == null) return; //jks means no weapon
		
		Weapon_Hold weaponHold = wpObj.GetComponent<Weapon_Hold>();
		if (weaponHold == null)  //jks throw 타입 경우 예) 폭탄.  는 weapon script 없을 수 있음.
		{
			Debug.LogWarning("Can't find Weapon script on (throw 타입 무기 경우는 없을 수 있음.) ");
		}
			
		weaponHold.show(false);
	}



	public virtual void uninstallWeaponAll()
	{
		uninstallWeapon(0);
		uninstallWeapon(1);
		uninstallWeapon(2);
	}

	protected void uninstallWeapon(int skillNum)
	{
		GameObject wpObj = getWeaponObject(skillNum);

		if (wpObj == null) return; //jks means no weapon
		
		Weapon_Hold weapon = wpObj.GetComponent<Weapon_Hold>();
		if (weapon == null)  //jks throw 타입 경우 예) 폭탄.  는 weapon script 없을 수 있음.
		{
			Debug.LogWarning("Can't find Weapon script on (throw 타입 무기 경우는 없을 수 있음.) ");
		}

		weapon.show(false);
		weapon.unInstall_SS();
	}


	public virtual bool isCoolingInProgress(int skillNum)
	{

		AttackCoolTime_SupportSkill cooltimer = getSupportSkillCoolTimer(skillNum);
		if (cooltimer != null) //jks in case if we have cool timer
		{
			if (cooltimer.IsCoolingInProgress)//jks and cool time is running, then dont use skill
			{
				return false;
			}
		}

		return true;
	}


	public virtual bool canUseSkill_Support(int skillNum)
	{

		AttackCoolTime_SupportSkill cooltimer = getSupportSkillCoolTimer(skillNum);
		if (cooltimer != null) //jks in case if we have cool timer
		{
			if (cooltimer.IsCoolingInProgress)//jks and cool time is running, then dont use skill
			{
				return false;
			}
		}
		
		return true;
	}


	public virtual void startSkill_Support(int skillNum)
	{
//jks override 된 method 에서 첵.		if (Knowledge.IsLeader && canUseSkill_Support(skillNum))
		{
			// cancel previous skill
			Knowledge.forceResetFlags();
			forceResetComboFlags();

			// start touched support skill.
			ActiveSkillNum = skillNum;
			Action_Combo1 = true;

//			BattleBase.Instance.showUICoolTime_SupportSkill(skillNum, Cooltime_ClassFit);

			GameObject leader_target = Knowledge.getClosestEnemy();
			BattleBase.Instance.updateLeaderTargetKnowledge(leader_target);			


			//jks hide current weapon
			if (Knowledge.Weapon)
				Knowledge.Weapon.GetComponent<Weapon_Hold>().show(false);

			//jks hide current support skill weapon
			hideSSWeaponAll();

			//jks install new support skill weapon
			installSSWeapon();
		}
	}



	#region CoolTime
	
	
//	public AttackCoolTime AttackCoolTimer0 
//	{
//		get { return BattleBase.Instance.CoolTime_SkillSupport_0; } 
//	}
//	public AttackCoolTime AttackCoolTimer1 
//	{
//		get { return BattleBase.Instance.CoolTime_SkillSupport_1; } 
//	}
//	public AttackCoolTime AttackCoolTimer2 
//	{
//		get { return BattleBase.Instance.CoolTime_SkillSupport_2; } 
//	}
//
//	void processCooltimeFinished()
//	{
//		if (Knowledge.IsDead) return;
//		
//		//Log.jprint(Time.time + "  :  " + gameObject.name + "C O O L  T I M E   E N D: CardDeckIndex : " + CardDeckIndex); 
//		//		BattleBase.Instance.doneUICoolTime(CardDeckIndex);
//
//		if (Knowledge.IsLeader)
//		{
//			GetComponent<Behavior_Mortal_Fighter_Main>().leaderStrategyLocomotion_AfterCooled();
//		}
//	}

	public void resetCoolTimer(int skillNum)
	{
		AttackCoolTime_SupportSkill cooltimer = getSupportSkillCoolTimer(skillNum);
		if (cooltimer != null && !cooltimer.IsCoolingInProgress)
		{
			cooltimer.reset();
		}
	}

	
//	public bool isCoolingInProgress(int skillNum)
//	{
//		AttackCoolTime_SupportSkill cooltimer = getSupportSkillCoolTimer(skillNum);
//		if (cooltimer == null) return true;
//		
//
//		return cooltimer.IsCoolingInProgress;
//	}

	/// <summary>
	/// Starts the cool timer.
	/// </summary>
	/// <param name="skillNum">Skill number.</param>
	private void startCoolTimer(int skillNum)
	{
		AttackCoolTime_SupportSkill cooltimer = getSupportSkillCoolTimer(skillNum);
		if (cooltimer == null || cooltimer.IsCoolingInProgress)
			return;
		
		cooltimer.activateTimer(Cooltime);


		//TODO		Knowledge.Action_Idle = true; //play Idle/Cool animation during cooltime
	}

	
	/// <summary>
	/// Starts the cool time and reset combo flags.
	/// </summary>
	/// <param name="skillNum">Skill number.</param>
	protected void startCoolTimeAndResetComboFlag(int skillNum)
	{
		startCoolTimer(skillNum);
//TODO		
//		Knowledge.RecentHitType = eHitType.HT_NONE;// reset
		Info[skillNum]._skillCompleted = false; // reset
		ComboRecent = 0;// reset
		ComboCurrent = 0;// reset
	}





	#endregion








	public bool isLastDamageInSkill(float reactionDistanceOverride)
	{
		if (reactionDistanceOverride >= 1000)  	//jks  if last damage in combo
		{
			//jks 콤보 끝에 블랜딩 애니메이션이 들어가거나 뒤로 점프하는 애니메이션을 넣어 사용하는 경우로 인해 실제 데미지를 주는 마지막 공격인지 판단을 위해 추가된 LastCombo를  사용..
			if (Info[ActiveSkillNum]._lastCombo == ComboCurrent) //jks  if last combo in skill
				return true;
		}
		return false;
	}

	
	protected int _damageCount=0;
	public void giveDamage(float reactionDistanceOverride)
	{			
		if (CameraManager.Instance == null) return;

		Info[ActiveSkillNum]._skillCompleted = isLastDamageInSkill(reactionDistanceOverride);

		_everGaveDamageForTheAttack[ActiveSkillNum] = true; //_everGaveDamageForTheAttack = true;

		switch (Info[ActiveSkillNum]._attackType)
		{
			case eAttackType.AT_Sword:
			case eAttackType.AT_Fist : 
				giveDamage_Melee(reactionDistanceOverride); break;

			case eAttackType.AT_Meteor: 
				_damageCount=0;  //reset
				giveDamage_Meteor(reactionDistanceOverride); break;

			case eAttackType.AT_LivingWeapon: 
				giveDamage_LivingWeapon(reactionDistanceOverride); break;

			case eAttackType.AT_Throwing: 
				giveDamage_Throwing(reactionDistanceOverride); break;


			// Projectile
			case eAttackType.AT_Arrow:
			case eAttackType.AT_Sinsu:
			case eAttackType.AT_Stun: 
			case eAttackType.AT_Bullet:
			case eAttackType.AT_Homing: 
				giveDamage_Projectile(reactionDistanceOverride); break;

			default: Log.Warning("Support Skill: giveDamage_Projectile() - No such attack type."); break;
		}
	}




	#region Projectile

	Launcher_Projectile getProjectileLauncher(eAttackType attackType)
	{
		if (attackType == eAttackType.AT_Arrow || attackType == eAttackType.AT_Sinsu)
			return GetComponent<Launcher_Projectile_SupportSkill>();

		else if (attackType == eAttackType.AT_Stun)
			return (Launcher_Projectile) GetComponent<Launcher_Projectile_SupportSkill_Stun>();

		else if (attackType == eAttackType.AT_Homing)
			return (Launcher_Projectile) GetComponent<Launcher_Projectile_SupportSkill_Homing>();

		else
			return null;
	}


	public void giveDamage_Projectile(float reactionDistanceOverride)
	{				
		//Log.jprint(". . . . . giveDamage(): " + _attackType);
		Launcher_Projectile launcher = (Launcher_Projectile)getProjectileLauncher(Info[ActiveSkillNum]._attackType);

		if (launcher == null) 
		{
			Log.Warning("Can not find Launcher_Projectile_SupportSkill component. ");
			return;
		}

		//jks 2016.3.14 skill buff 기능 추가.
		addSkillBuff();

		Knowledge.updateProjectileTarget();
		if (Knowledge.ProjectileOriginalTarget == null)
		{
			Knowledge.ProjectileOriginalTarget = BattleBase.Instance.findClosestOpponent(Knowledge.AllyID, transform.position.x);
			Knowledge.setTarget(Knowledge.ProjectileOriginalTarget);
			//updateProjectileTarget();
		}
		if (Knowledge.ProjectileOriginalTarget == null) return;


		Knowledge.startAim();
		
		launcher.initPrefabReference(Info[ActiveSkillNum]._skill_ID);

		Knowledge.SkillType = eSkillType.ST_Support;
		Knowledge.updateProjectileTarget();
		StartCoroutine( launcher.spawn(Knowledge.ProjectileOriginalTarget, Info[ActiveSkillNum]._attackInfo, Info[ActiveSkillNum]._attackType, Info[ActiveSkillNum]._weaponType_ForAnimation, reactionDistanceOverride));

	}
	
	#endregion





	#region Throwing

	public void giveDamage_Throwing(float reactionDistanceOverride)
	{				
//		_skillCompleted = isLastDamageInSkill(reactionDistanceOverride);

		//jks 2016.3.14 skill buff 기능 추가.
		addSkillBuff();

		Launcher_Throw launcher = GetComponent<Launcher_Throw>();
		if (launcher == null) 
		{
			Log.Warning("giveDamage_Throwing() : Can not find Launcher_Throw component. ");
			return;
		}

		launcher.initPrefabReference(Info[ActiveSkillNum]._skill_ID);

		Knowledge.SkillType = eSkillType.ST_Support;
		StartCoroutine( launcher.spawn(reactionDistanceOverride)); //jks reactionDistanceOverride may have spawn position info too.
	}
	
	#endregion




	#region Living weapon (Summoned character)
	

	public void giveDamage_LivingWeapon(float reactionDistanceOverride)
	{				
//		_skillCompleted = isLastDamageInSkill(reactionDistanceOverride);
		//Log.jprint(". . . . . giveDamage(): " + _attackType);
		Launcher_LivingWeapon launcher = GetComponent<Launcher_LivingWeapon>();
		if (launcher == null) 
		{
			Log.Warning("giveDamage_LivingWeapon() : Can not find Launcher_LivingWeapon component. ");
			return;
		}
		
		launcher.initPrefabReference(Info[ActiveSkillNum]._skill_ID);
		//if (_target_attack)
		{
			//Log.jprint("giveDamage()");
			Knowledge.SkillType = eSkillType.ST_Support;
			StartCoroutine(launcher.spawn(Knowledge.getCurrentTarget()));//jks launcher.spawn(_target_attack);
		}
	}
	
	#endregion
	



	#region Melee
	protected virtual void giveDamage_Melee(float reactionDistanceOverride)
	{			
	}
	#endregion




	
	#region Meteor

	public void giveDamage_Meteor(float reactionDistanceOverride)
	{		
		//jks 2016.3.14 skill buff 기능 추가.
		addSkillBuff();

		Knowledge.SkillType = eSkillType.ST_Support;
		StartCoroutine(giveDamageDelayed(reactionDistanceOverride));
		
	}
	
	IEnumerator giveDamageDelayed(float reactionDistanceOverride)
	{
		float delay = Info[ActiveSkillNum]._attackInfo;
		yield return new WaitForSeconds(delay);
		
		StartCoroutine(continuousDamage(reactionDistanceOverride));
	}
	
	
	float _startTime;
	IEnumerator continuousDamage(float reactionDistanceOverride)
	{
		if (Info[ActiveSkillNum]._damage_duration == 0) yield return null;
		
		float damageInterval = Info[ActiveSkillNum]._attackInfo3 * 0.1f;
		if (damageInterval == 0)
			damageInterval = 0.05f; //jks give minimum interval
		
		int damage_frequency = Mathf.RoundToInt(Info[ActiveSkillNum]._damage_duration / damageInterval);
		if (damage_frequency < 1)
			damage_frequency = 1;

		_startTime = Time.time;
		
		
		float initialCenter = transform.position.x;
		//Log.jprint(gameObject.name + "    0000000   damage center: "+ initialCenter);
		
		while (Time.time - _startTime < Info[ActiveSkillNum]._damage_duration) 
		{
			if (++_damageCount == damage_frequency) //jks 2016.5.23 막타면,
				Info[ActiveSkillNum]._skillCompleted = true;
			
			Knowledge.SkillType = eSkillType.ST_Support;

			//Log.jprint(Time.time + "   ? ? ? continuousDamage()    interval: " + damageInterval + "   duration : "+ Info[ActiveSkillNum]._damage_duration);
			giveAreaDamageNow(reactionDistanceOverride, initialCenter);


			yield return new WaitForSeconds(damageInterval);
			
			//jks debugging 
			//_arealAttack_ShowDamageRange = false;
		}
	}



	#if UNITY_EDITOR
	protected bool _arealAttack_ShowDamageRange = false;
	protected Vector3 _arealAttack_DamageCenter = new Vector3();
	protected float _arealAttack_Radius;
	protected Color _arealAttack_Color = new Color(0.7f, 0.7f, 0.1f, 0.5f);
	protected void OnDrawGizmos() 
	{
		if (_arealAttack_ShowDamageRange)
		{
			Gizmos.color = _arealAttack_Color;
			Gizmos.DrawSphere(_arealAttack_DamageCenter, _arealAttack_Radius);
			
			//Log.jprint(gameObject.name + "    QQQQQQ   damage center: "+ _arealAttack_DamageCenter);
		}
	}
	#endif


	protected virtual List<FighterActor> Opponents { get { return BattleBase.Instance.List_Enemy; }}


	void giveAreaDamageNow(float reactionDistanceOverride, float initialCenter)
	{
		if (CameraManager.Instance == null) return;
		
//		Knowledge._opponentsInAttackRange.Clear(); //reset
		
//		bool found = HaveOpponentAhead(10 ,"Fighters", Knowledge._opponentsInAttackRange);
//		if (!found) return;
		if (Opponents.Count <= 0) return;

		//Log.jprint(gameObject.name + "    1111111   damage center: "+ initialCenter);
		
		float areaCenter;
		bool headingRight = transform.forward.x > 0;
		if (headingRight)
		{
			areaCenter = initialCenter + Info[ActiveSkillNum]._attackDistanceMax;
		}
		else
		{
			areaCenter = initialCenter - Info[ActiveSkillNum]._attackDistanceMax;
		}
		
		//Log.jprint(gameObject.name + "    2222222   damage center: "+ areaCenter);
		
		#if UNITY_EDITOR
		//jks debugging
		_arealAttack_DamageCenter = transform.position;
		_arealAttack_DamageCenter.x = areaCenter;
		_arealAttack_ShowDamageRange = true;
		_arealAttack_Radius = Info[ActiveSkillNum]._damageRange;
		#endif

//		foreach(GameObject go in Knowledge._opponentsInAttackRange)
		foreach (FighterActor opponent in Opponents)
		{
			GameObject go = opponent._go;
			if (go == null) continue;
			if (!go.activeSelf) continue;

			//jks check damage range
			Knowledge_Mortal goKnowledge = go.GetComponent<Knowledge_Mortal>();
			if (goKnowledge == null) continue;
			
			//			float dist = Mathf.Abs(areaCenter - go.transform.position.x);		
			//			float distShell = dist - this.Radius - goKnowledge.Radius;
			//			if (distShell > _damageRange) continue;
			float distFromCenter = Mathf.Abs(areaCenter - go.transform.position.x) - goKnowledge.Radius;		
			if (distFromCenter > Info[ActiveSkillNum]._damageRange) continue;
			
			
			Knowledge_Mortal_Fighter knowledgeOpponent = go.GetComponent<Knowledge_Mortal_Fighter>();
			
			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
			{
				Log.print_always("   --------------------------------- Area Damage ---------------------------------");
			}
			#endif
			eHitType hitType = getFinalHitType(knowledgeOpponent);
			
			int hitReactionAnimID = getReaction(hitType);

			ObscuredInt finalAttack = AttackPoint;

			if (Knowledge.IsLeader || TestOption.Instance()._classRelationBuffAll)
			{
				ObscuredInt classRelationAttackPoint = Knowledge.calculate_ClassRelation_AttackPoint(AttackPoint, knowledgeOpponent);
				//jks 2015.8.26 no more: int leaderBuffAttackPoint = Knowledge.calculate_LeaderBuff_AttackPoint_Opponent();
				//jks 2015.5.8 remove leader strategy-				int leaderStrategyAttack = calculate_LeaderStrategy_AttackPoint();
				
				
				finalAttack = AttackPoint + classRelationAttackPoint;//jks 2015.8.26 no more:  + leaderBuffAttackPoint;//jks 2015.5.8 remove leader strategy-	 + leaderStrategyAttack;
				
				#if UNITY_EDITOR
				if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
				{
					//Log.print("   --------------------------------- Area Damage ---------------------------------");
					Log.print_always("   현재 리더 클래스: None ");
					Log.print_always("   공격자 : " + gameObject.name + "  -->  피해자: " + knowledgeOpponent.name);
					Log.print_always("   공격자 클래스 : " + Knowledge.Class + "  -->  피해자 클래스: " + knowledgeOpponent.Class + "   피격 타입: " + hitType);
					Log.print_always("   G I V E  D A M A G E      Original: "+ AttackPoint + "    + 클래스 상성 공격력: "+classRelationAttackPoint+  " = " + finalAttack);
				}
				#endif
			}
			else
			{
				#if UNITY_EDITOR
				if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
				{
					//Log.print("   --------------------------------- Area Damage ---------------------------------");
					Log.print_always("   피격 타입: " + hitType);
					Log.print_always("   G I V E  D A M A G E      Original: " + finalAttack);
				}
				#endif
			}


			Knowledge.SkillType = eSkillType.ST_Support;
			knowledgeOpponent.takeDamage(finalAttack, hitReactionAnimID, hitType, Info[ActiveSkillNum]._attackType, Info[ActiveSkillNum]._weaponType_ForAnimation, gameObject, reactionDistanceOverride);
		}
		
	}


	bool HaveOpponentAhead(float scanDistance, string tagNameToSearch, List<GameObject> opponentsInScanDistance)
	{
		GameObject[] gosInWorld = GameObject.FindGameObjectsWithTag(tagNameToSearch);
		if (gosInWorld.Length == 0) return false;
		
		float heading = transform.forward.x; 
		
		List<GameObject> fightersInScanDist = new List<GameObject>();
		fightersInScanDist.Clear();
		//jks check if I can attack (in attackable distance).
		Utility.findGameObjectsInRange(heading, scanDistance, transform.position, gosInWorld, fightersInScanDist);
		
		if (fightersInScanDist.Count == 0) return false;
		
		getOpponents(fightersInScanDist, opponentsInScanDistance);
		
		if (opponentsInScanDistance.Count == 0) return false;
		
		return true;
	}
	
	
	//jks filter out same team
	bool getOpponents(List<GameObject> objects, List<GameObject> opponents)
	{
		foreach(GameObject go in objects)
		{
			Knowledge_Mortal_Fighter goKnowledge = go.GetComponent<Knowledge_Mortal_Fighter>();
			if (goKnowledge == null) continue;
			if (goKnowledge.AllyID == Knowledge.AllyID) continue;
			if (goKnowledge.IsDead) continue;
			if (goKnowledge.AmICaptured) continue;
			//if (!isOpponentInFrontOfMe(go)) continue;
			
			opponents.Add(go);
		}
		
		return opponents.Count > 0;
	}
	



	#endregion



	public GameObject getOpponentsInScanDistance_WeaponPositionBased()
	{
		
		GameObject closestOpponent = BattleBase.Instance.findClosestOpponent(Knowledge.AllyID, transform.position.x);
		Knowledge.setTarget(closestOpponent);
		
		if (closestOpponent == null) return null;
		
		Knowledge_Mortal opponentKnowledge = closestOpponent.GetComponent<Knowledge_Mortal>();
		
		//jks - body based
		float distShell_AttackerAndClosestOpponent = Mathf.Abs(transform.position.x - closestOpponent.transform.position.x) - Knowledge.Radius - opponentKnowledge.Radius;
		
		//jks - weapon based
		float distWeapon_AttackerAndClosestOpponent = Mathf.Abs(WeaponEndPosition(ActiveSkillNum).x  - closestOpponent.transform.position.x) - opponentKnowledge.Radius;
		
		//jks 무기를 뒤로 휘두르는 경우 몸보다 더 뒤에 위치하기 때문에  이 경우는 몸 위치로 계산. 
		float finalDistToCheck = Mathf.Min(distShell_AttackerAndClosestOpponent, distWeapon_AttackerAndClosestOpponent);
		
		if (Info[ActiveSkillNum]._attackDistanceMax + Info[ActiveSkillNum]._weaponLength < finalDistToCheck) return null;
		
		
		return closestOpponent;
	}


	public eHitType getFinalHitType(Knowledge_Mortal_Fighter opponent)
	{
		eHitType hitType = getFinalHitType_Base(opponent);
		
		if (hitType == eHitType.HT_CRITICAL && Knowledge.IsLeader)
		{
			if (BattleBase.Instance.CriticalCount < CameraManager.Instance.MaxCritical)
			{
				BattleBase.Instance.CriticalCount ++;
//				if (BattleUI.Instance() != null)
//					BattleUI.Instance().setCriticalCount(CameraManager.Instance.MaxCritical, BattleBase.Instance.CriticalCount);
			}
			
			//0210 - 피버 이벤트를 버튼 발동 형식으로 변경.
//			if (BattleBase.Instance.AttackPoint < CameraManager.Instance.MaxAttackPoint && !CameraManager.Instance.IsFeverTime  && !BattleBase.Instance.IsSkillStagingInProgress)
//			{
//				BattleBase.Instance.AttackPoint = (BattleBase.Instance.AttackPoint + CameraManager.Instance.IncreseAttackPoint) <= CameraManager.Instance.MaxAttackPoint ? BattleBase.Instance.AttackPoint + CameraManager.Instance.IncreseAttackPoint : CameraManager.Instance.MaxAttackPoint;
//				if (BattleUI.Instance() != null)
//					BattleUI.Instance().setSlowEventGauge();
//			}
		}
		
		return hitType;
	}


	bool IsMeleeAttack { get { return Info[ActiveSkillNum]._attackType == eAttackType.AT_Fist || Info[ActiveSkillNum]._attackType == eAttackType.AT_Sword; }}

	public eHitType getFinalHitType_Base(Knowledge_Mortal_Fighter opponent)
	{
		ObscuredFloat finalHitRate;
		
		finalHitRate = calcHitRate(opponent);
		
		if (finalHitRate < 0) finalHitRate = 0;
		
		eHitType hitType = Knowledge.judgeHitState(finalHitRate);

		//2016.5.25			
//		if (IsMeleeAttack)
//		{
//			//jks 2014.9.27  크리티컬은 스킬의 막지막 타에만 가능 하도록 수정. 
//			if (hitType == eHitType.HT_CRITICAL)
//			{
//				if (! Info[ActiveSkillNum]._skillCompleted)
//				{
//					hitType = eHitType.HT_GOOD;
//				}
//			}
//		}


		return hitType;
	}


	public float calcHitRate_Base(Knowledge_Mortal_Fighter opponent)
	{
		float hitRate;
		//jks 2015.8.26 game design change.		float classRelationHitRate = Knowledge.calculate_ClassRelation_HitRate(opponent);
		
		hitRate = Knowledge._criticalRate;//jks 2015.8.26 game design change. + classRelationHitRate;
		
		#if UNITY_EDITOR
		if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
		{
			Log.print_always("공격자 : " + gameObject.name +  " 적중률: "+Knowledge._criticalRate + " = " + "  중간계산 적중률: " + hitRate);
		}
		#endif


		//jks 2016.3.14 : 
		#if UNITY_EDITOR
		float originalHitRate = hitRate;
		#endif

		//jks 적중율 스킬 버프 적용.
		float skillBuff_HitRate = BattleBase.Instance.getSkillBuff_CriticalUp(Knowledge.AllyID) - BattleBase.Instance.getSkillBuff_CriticalDown(Knowledge.AllyID, opponent);
		hitRate += hitRate * skillBuff_HitRate;
		if (hitRate < 0)
			hitRate = 0;

		#if UNITY_EDITOR
		if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
		{
			Log.print_always("   스킬버프 적중율 증감 적용       적용전: " + originalHitRate + "      증감치: "+ skillBuff_HitRate + "      적용후: "+ hitRate );
		}
		#endif

		
		
		return hitRate;
	}

	public virtual ObscuredFloat calcHitRate(Knowledge_Mortal_Fighter opponent)
	{
		return 0;
	}


	public int getReaction(eHitType hitType)
	{
		int hitReactionAnimID;
		
		if (hitType == eHitType.HT_CRITICAL)
		{
			hitReactionAnimID = getReactionAnimID(Info[ActiveSkillNum]._totalCombo, hitType);
		}
		else 
		{
			hitReactionAnimID = getReactionAnimID(Info[ActiveSkillNum]._recentCombo, hitType); 
		}
		
		return hitReactionAnimID;
	}


	public int getReactionAnimID(int combo, eHitType hitType)
	{
		if(combo == 0) //jks HACK: safty check
			combo = 1;
		
		int reactionChoice;
		
		if (hitType == eHitType.HT_CRITICAL)
		{
			reactionChoice = Random.Range(4, 6);  //jks critical reaction only
		}
		else if (hitType == eHitType.HT_MISS || hitType == eHitType.HT_BAD)
		{
			//jks  //jks-  reactionChoice = 3;  
			return Knowledge.Anim_Protect;//jks protect motion
		}
		else 
		{
			reactionChoice = Random.Range(0, 3);
		}

		return Info[ActiveSkillNum]._anim_reaction[combo-1,reactionChoice]; //jks index 0 == combo1, index 1 == combo2, index 2 == combo3
	}




	protected void setNextComboActionFlag()
	{
		if (ComboCurrent == 1  && TotalCombo > 1 ) { Action_Combo2  = true; }
		else if (ComboCurrent == 2  && TotalCombo > 2 ) { Action_Combo3  = true; } //jsm - reset zoom value 
		else if (ComboCurrent == 3  && TotalCombo > 3 ) { Action_Combo4  = true; }
		else if (ComboCurrent == 4  && TotalCombo > 4 ) { Action_Combo5  = true; }
		else if (ComboCurrent == 5  && TotalCombo > 5 ) { Action_Combo6  = true; }
		else if (ComboCurrent == 6  && TotalCombo > 6 ) { Action_Combo7  = true; }
		else if (ComboCurrent == 7  && TotalCombo > 7 ) { Action_Combo8  = true; }
	}
	




}
