using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;


public enum eAttackType 
{
	AT_Sword = 0,
	AT_Arrow = 1,
	AT_Bullet = 2,
	AT_Throwing = 3,
	AT_Sinsu = 4,
	AT_BUFF = 5,
	AT_LivingWeapon = 6,
	AT_Fist = 7,
	AT_Stun = 8,
	AT_Meteor = 9,
	AT_Homing = 10,
}

public enum eSkillType
{
	ST_Quick, 	//jks 평타
	ST_Card, 	//jks 일반 스킬
	ST_Support, //jks 지원 스킬
//2015.10.2 역할 스킬 제거.	ST_Role,	//jks 역할 스킬.
}


//jks for animation
//jks 몸 = 1, 신수 = 2, 활 = 3, 총 = 4, 한손검 = 5, 양손검 = 6, 창/도끼/봉 = 7, 옵저버 = 8, 등대 = 9
public enum eWeaponType_ForAnimation 
{
	WTA_Body = 1,
	WTA_Sinsu = 2,
	WTA_Arrow = 3,
	WTA_Gun = 4,
	WTA_Onehand = 5,
	WTA_Twohand = 6,
	WTA_Spear = 7,
	WTA_Observer = 8,
	WTA_Lighthouse = 9,
	WTA_Bomb = 10,
}


public enum eFighterType
{
	FT_None,
	FT_Humanoid,
	FT_Machine
}



public class SpecialOpponent
{
	public int _characterID;
	public float _attack_rate;
	public float _defense_rate;
}


public struct LeaderBuffData
{
	public ObscuredFloat info;
	public ObscuredFloat amount;
}

public class LeaderBuffs
{
	public LeaderBuffData _critical_up;				//jks 팀원 적중률 n % 증가.
	public LeaderBuffData _attack_up;				//jks 팀원 공격력 n % 증가.
	public LeaderBuffData _defense_up;				//jks 팀원 방어력 n % 증가.
	public LeaderBuffData _hp_up;					//jks 팀원 HP n % 증가.
	public LeaderBuffData _skill_speed_up;			//jks 팀원 스킬 속도 n % 증가.
	public LeaderBuffData _move_speed_up;			//jks 팀원 이동 속도 n % 증가.
	public LeaderBuffData _defense_down;			//jks 상대 방어력 n % 감소 시킴.
	public LeaderBuffData _critical_down;			//jks 상대 적중률 n % 감소 시킴.
	public LeaderBuffData _ignore_boss_critical; 	//jks 이 수치 만큼 보스 크리티컬을 무시함.
	public LeaderBuffData _invincible_mode;			//jks 리더가 n 개의 combo를 달성하면 팀원이 m 초 동안 무적 상태가 됨.
}



public class Knowledge_Mortal_Fighter : Knowledge_Mortal
{
	Locomotion_Mortal_Fighter _locomotion;

	public Locomotion_Mortal_Fighter Loco
	{
		get
		{
			if (_locomotion == null)
			{
				_locomotion = GetComponent<Locomotion_Mortal_Fighter>();
			}
			return _locomotion;
		}
	}


	#region Summoning
	protected bool _livingWeapon = false;
	public bool IsLivingWeapon { get { return _livingWeapon; } }
	public void setLivingWeapon(bool value) 
	{ 
		_livingWeapon = value; 
	} 

	protected float _summon_position_x = 0;
	
	
	public float SummonPositionX
	{
		get
		{
			if (transform.forward.x > 0)
			{
				return _summon_position_x;
			}
			else
			{
				return - _summon_position_x;
			}
		}
	}
	#endregion


	protected ObscuredInt _character_ID; //jks - (from table id) - is the character Bam, Yuri, ..?
	protected ObscuredInt _skill_ID;

	protected SpecialOpponent[] _specialOpponents = new SpecialOpponent[3];

	protected ObscuredInt _count_special_opponent = 0;

	//jks serialize for now, later maybe get it from character table.
	public ObscuredFloat _coolTime = 2.0f;
//	public float _coolTime_GuardUp = 2.0f;
	public ObscuredInt _attackPoint = 100;
	public ObscuredFloat _criticalRate = 0.3f;

	public ObscuredFloat _boostSkillSpeed = 0; // 능력(ABI - Ability) 
	public ObscuredFloat _boostMovementSpeed = 0;  // 민첩도(AGI - Agility) 
	public ObscuredFloat _boostSupportAndQuickSkill = 0; // (WIS - Wisdom) 


	protected eAttackType _attackType;

	protected eSkillType _skillType;

	public ObscuredInt _pvpAttackBoost = 1;

	public ObscuredInt _defense = 0;

	public ObscuredInt _level;


	protected int _body_type = 0;
	protected int _gender = 0;

	protected SupportSkillKnowledge _knowledgeSupportSkill;

	public virtual SupportSkillKnowledge KnowledgeMultiSkill 
	{ 
		get 
		{ 
			if (_knowledgeSupportSkill == null)
				_knowledgeSupportSkill = GetComponent<SupportSkillKnowledge>(); 

			return _knowledgeSupportSkill;
		} 
	}



	#region leader buff

	protected LeaderBuffs _my_leader_buff_info = new LeaderBuffs();

	public LeaderBuffs MyLeaderBuff { get { return _my_leader_buff_info; }}


	protected int getLeaderBuffDefenseDown(int curDefense) 
	{ 
		if (AllyID == eAllyID.Ally_Human_Me)
			return 0;
		
		if (BattleBase.Instance.CurrentLeaderBuff == null || BattleBase.Instance.CurrentLeaderBuff._defense_down.amount < 0)
			return 0;

		int defense_down =  Mathf.RoundToInt(curDefense * BattleBase.Instance.CurrentLeaderBuff._defense_down.amount);

		#if UNITY_EDITOR
		if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
		{
			if (defense_down > 0)
			{
				if (BattleBase.Instance.LeaderGameObject == null)
					Log.print_always("  <<리더 버프>>  현재 리더: DEAD !"+" :  방어력 감소 없음.");				
				else
					Log.print_always("  <<리더 버프>>  현재 리더: "+BattleBase.Instance.LeaderGameObject.name +" :  방어력 감소  :   버프: "+ BattleBase.Instance.CurrentLeaderBuff._defense_down.amount 
						+ " *  현재 방어력: "+ curDefense + " = 방어력 감소치: " + defense_down);
			}
		}
		#endif

		if (BattleBase.Instance.LeaderGameObject == null)
			return curDefense;
		
		return defense_down;
	}

	protected float getLeaderBuffCriticalDown(float curCritical) 
	{ 
		if (AllyID == eAllyID.Ally_Human_Me)
			return 0;

		if (BattleBase.Instance.CurrentLeaderBuff == null || BattleBase.Instance.CurrentLeaderBuff._critical_down.amount < 0)
			return 0;

		float critical_down = curCritical * BattleBase.Instance.CurrentLeaderBuff._critical_down.amount;

		#if UNITY_EDITOR
		if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
		{
			if (critical_down > 0)
			{
				if (BattleBase.Instance.LeaderGameObject == null)
					Log.print_always("  <<리더 버프>>  현재 리더: DEAD !"+" :  적중율 감소 없음.");				
				else
					Log.print_always("  <<리더 버프>>  현재 리더: "+BattleBase.Instance.LeaderGameObject.name +" :  적중율 감소  :   버프: "+ BattleBase.Instance.CurrentLeaderBuff._critical_down.amount 
						+ " *  현재 적중율: "+ curCritical + " = 적중율 감소치: " + critical_down);
			}
		}
		#endif

		if (BattleBase.Instance.LeaderGameObject == null)
			return curCritical;
		

		return critical_down;
	}

	public virtual int getLeaderBuffAttackUp(int curAttackPoint) { return 0; }
	protected virtual int getLeaderBuffDefenseUp(int curDefensePoint)  { return 0; }
	public virtual float getLeaderBuffCriticalUp(float curCritical)  { return 0; }
	public virtual void updateLeaderBuffHPUp(bool hp_up, float exLeader_hp_up = 0f)  { return; }
	protected virtual float getLeaderBuffSkillSpeed(float curSpeedRate)  { return 0; }
	public virtual float getLeaderBuffMoveSpeed()  { return 0; }
	public virtual int getLeaderBuffBossCriticalIgnoreCount()  { return 0; }

	public virtual int getLeaderBuffInvincibleComboCount()  { return 0; }
	public virtual float getLeaderBuffInvinciblePeriod()  { return 0; }


	#endregion

	
	#region From AnimInfo table
	public float _opponentSearchingTime = 3.3f;
	public float _teamMemberStartDistance = 4;
	public float _leaderCatchUpDistance = 3; // 3 meter.
	public float _leaderCatchUpDistance_FightingTeamMember = 3; // 3 meter. //팀원이 앞에서 공격 중일 때 리더와 이 거리 이상 멀어지면 리더 전진 하게 함.
	
	public float OpponentSearchingTime 
	{ 
		get 
		{ 
			if (BattleBase.Instance.IsBossRaid)
				return BattleBase.Instance.RaidbossSearchingTime;
			else
				return _opponentSearchingTime;
		}
	}

	public float TeamMemberStartDistance { get { return _teamMemberStartDistance; }}
	public float LeaderCatchUpDistance { get { return _leaderCatchUpDistance; }}
	public float LeaderCatchUpDistance_FightingTeamMember { get { return _leaderCatchUpDistance_FightingTeamMember; }}
	
	protected int _cameraTargetAttack = 0;
	protected int _cameraTargetIdle = 1;
	protected int _cameraTargetMove = 2;
	public int CameraTargetAttack { get { return _cameraTargetAttack; }}
	public int CameraTargetIdle { get { return _cameraTargetIdle; }}
	public int CameraTargetMove { get { return _cameraTargetMove; }}

	public float _speedWalk = 1.2f;
	public float _speedWalkBack = -1.5f;
	public float _speedWalkFast = 1.5f;
	public float _speedRun = 2.0f;
	
	public float _animSpeedWalk = 1.0f;
	public float _animSpeedWalkBack = 1.0f;
	public float _animSpeedWalkFast = 1.5f;
	public float _animSpeedRun = 1.0f;
	public virtual float AnimSpeed_Walk { get { return _animSpeedWalk + _animSpeedWalk * _boostMovementSpeed; }}
	public virtual float AnimSpeed_WalkBack { get { return _animSpeedWalkBack + _animSpeedWalkBack * _boostMovementSpeed; }}
	public virtual float AnimSpeed_WalkFast { get { return _animSpeedWalkFast + _animSpeedWalkFast * _boostMovementSpeed; }}
	public virtual float AnimSpeed_Run { get { return _animSpeedRun + _animSpeedRun * _boostMovementSpeed; }}

	//jks save animation state hash
	protected int _anim_victory; 
	protected int _anim_walk; 
	protected int _anim_walkback; //jks walkback -
	protected int _anim_walkbackturn; //jks walkback -
	protected int _anim_walkfast; //jks walkfast -
	protected int _anim_run; //jks run -
	protected int _anim_protect;
	protected int _anim_idle; 

	protected int _anim_installWeapon;
	protected int _anim_exhausted; 
	protected int _anim_coolingjump;
	protected int _anim_block; 

	#endregion


	//jks skill
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

	public virtual ObscuredInt DamageFrequency 
	{ get {
			if (_damage_frequency > 0)
			{
				return _damage_frequency; 
			}
			Log.Warning(gameObject + " _damage_frequency = "+ _damage_frequency);
			return 1;
		}

	  set { _damage_frequency = value; }
	}

	public virtual bool IsNotAttacking 
	{ get 
		{
			return ! Progress_SkillAction;
		}
	}

	//jks skill extra infomation
	protected bool _reactionOff = false;
	protected float _damagePenetration = 1.0f;
	protected float _buff_probability = 0;	//버프 발생 시킬지 판정하는 확율.
	protected int _buff_duration = 0;		//버프 시작될 경우, 몇초 지속인지 설정.
	protected float _attack_up = 0;			//버프시 증가할 아군의 공격치 %	
	protected float _defense_up = 0;		//버프시 증가시킬 아군의 방어력 %
	protected float _critical_up = 0;		//버프시 증가시킬 아군의 적중율 %
	protected float _attack_down = 0;		//버프시 감소시킬 상대의 공격치 %
	protected float _defense_down = 0;		//버프시 감소시킬 상대의 방어력 %
	protected float _critical_down = 0;		//버프시 감소시킬 상대의 적중율 %
	protected bool _skill_hit_only_buff_on = false;


	public eWeaponType_ForAnimation _weaponType_ForAnimation;

	protected bool _skillCompleted = false;
	protected ObscuredBool _everGaveDamageForTheAttack = false; //jks to decide cooltime activation
	
	//jks save animation state hash
	protected int _anim_pvpReady; 
	protected int _anim_pvpJustBeforeHit;
	protected int _anim_installWeapon_pre;
	protected int _anim_stun; 
	protected int _anim_captured; 
	protected int _anim_captured_in_the_air; 
	protected int _anim_combo1; //jks combo1 attack animation controller surfix number
	protected int _anim_combo2; //jks combo2 -
	protected int _anim_combo3; //jks combo3 -
	protected int _anim_combo4; //jks combo4 -
	protected int _anim_combo5; //jks combo5 -
	protected int _anim_combo6; //jks combo6 -
	protected int _anim_combo7; //jks combo6 -
	protected int _anim_combo8; //jks combo6 -
	protected int _anim_skill_staging; 
	protected int _anim_death; 
	protected int _anim_deathB; 
	protected int _anim_pause; 
	protected int[] _anim_guard_up = new int[6]{0,0,0,0,0,0}; //클래스 별.  (enum CardClass) 사용: 0: 사용안함, 1: 탐색꾼, 2: 낚시꾼, 3: 창지기, 4: 등대지기, 5: 파도잡이 


	public int[,] _anim_reaction = new int[6,7]{{0,0,0,0,0,0,0},{0,0,0,0,0,0,0},{0,0,0,0,0,0,0},{0,0,0,0,0,0,0},{0,0,0,0,0,0,0},{0,0,0,0,0,0,0}}; //jks _anim_reaction[1,] : combo2 reaction animation controller surfix number.
	
	public int _weaponPathID;
	//public int _weaponWeight;
	public int _weaponProjectilePathID;

	public GameObject _weaponAssetRef = null;

	protected float _weaponLength;

	protected AttackCoolTime _coolTimer;
	public AttackCoolTime CoolTimer { get { return _coolTimer; }}

	protected AttackCoolTime_GuardUp _coolTimer_GuardUp;

	protected GameObject _target_attack;
	protected Vector3 _target_position;

	protected int _attackDistanceKeepingJumpCount = 0; //jks to not jump back forever to keep projectile attack distance
	
//	public List<GameObject> _opponentsInAttackRange = new List<GameObject>();
//	public List<GameObject> OpponentsInAttackRange { get { return _opponentsInAttackRange; }  }
	

	protected bool _postVictory = false;

	protected bool _deathStart = false;
	protected bool _deathBStart = false;  //jks 보스가 죽지 않아도 승리할 수 있는 mission의 경우 유저 승리 시 보스액션. 
	protected bool _deathInProgress = false;
	protected bool _deathBInProgress = false;

	protected bool _blockInProgress = false;
	protected bool _guardUpAnimInProgress = false;

	protected bool _skillStagingInProgress = false;

	protected bool _hit = false;
	protected bool _hitReactionInProgress = false;

	protected bool _idle = false;
	protected bool _paused = false;

	protected bool _walk = false;
	protected bool _walkBack = false;
	protected bool _walkBackTurn = false;
	protected bool _walkFast = false;

	protected bool _run = false;
	protected bool _stun = false;
	protected bool _captured = false;
	protected bool _captured_in_the_air = false;
	protected bool _pvpReady = false;
	protected bool _pvpJustBeforeHit = false;
	protected bool _installWeapon = false;
	protected bool _installWeapon_pre = false;

	protected bool _block = false;
	
	protected bool _combo1 = false;
	protected bool _combo2 = false;
	protected bool _combo3 = false;
	protected bool _combo4 = false;
	protected bool _combo5 = false;
	protected bool _combo6 = false;
	protected bool _combo7 = false;
	protected bool _combo8 = false;
	protected bool _combo1AnimInProgress = false;
	protected bool _combo2AnimInProgress = false;
	protected bool _combo3AnimInProgress = false;
	protected bool _combo4AnimInProgress = false;
	protected bool _combo5AnimInProgress = false;
	protected bool _combo6AnimInProgress = false;
	protected bool _combo7AnimInProgress = false;
	protected bool _combo8AnimInProgress = false;

	protected bool _coolingJump = false;
	protected bool _coolingJumpInProgress = false;

	protected int _animHitNumber;

	protected bool _isLastDamageInSkill = false;

	protected int _currentCombo = 0;
	protected int _recentCombo; //jks used to find reaction type in giveDamage()- usally called later by anim event and _currentCombo may be reset, so save recent combo number here.


	protected CardClass _class;

	protected ObscuredFloat _skillAnimationPlayRate = -1;
	protected ObscuredFloat _movementPlayRate = -1;

	protected eFighterType _fighterType = eFighterType.FT_None;

	protected bool _holdUpdate = false;

//	public byte[,] _isSuperiorClass_Melee; //jks class comparison for additional attack point
//	public byte[,] _isSuperiorClass_Distance; //jks class comparison for additional attack point

	//jks weapon bones
	Transform _right_hand;
	Transform _right_foot;
	Transform _left_hand; 
	Transform _left_foot;


	//jks for new skill/weapon 
	protected ObscuredFloat _slowSkill = 1.0f;
	protected ObscuredFloat _slowMovement = 1.0f;

	protected float _slowPause = 1.0f;

	protected bool _tooDangerous = false;
	protected bool _jumpBackDelay = false; //jks 후진 interval 제한.
	public void resetJumpBackDelay() { _jumpBackDelay = false;}

	protected ObscuredFloat _moveSpeedChangeRate = 1.0f;
	public float MoveSpeedChangeRate { get {return _moveSpeedChangeRate;} set {_moveSpeedChangeRate = value;}}


	public float SkillSpeedAdjustment { get {return _slowSkill;} set {_slowSkill = value;}}
	public float WeaponSlowMovement { get {return _slowMovement;} set {_slowMovement = value;}}

	public float PauseSpeedAdjustment { get {return _slowPause;} set {_slowPause = value;}}

	public virtual bool IsLeader { get { return false; } }


	public virtual bool IsBoss
	{
		get { return false; } //default
	}

	public virtual bool IsRaidBoss
	{
		get { return false; }
	}

	public virtual ObscuredInt AttackPoint 
	{ 
		get
		{
			ObscuredInt distributedAttackPoint = _attackPoint;

//			#if UNITY_EDITOR
//			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
//				Log.print_always(gameObject.name + "  #####    A T T A C K    P O I N T    #####");
//			#endif


			if (DamageFrequency > 1)// if multiple attack skill?
			{
				distributedAttackPoint = Mathf.RoundToInt(_attackPoint / DamageFrequency);  //jks 2015.11.23:  new calc
//				#if UNITY_EDITOR
//				if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
//					Log.print_always("##   _attackPoint: " + _attackPoint + " /  DamageFrequency: " + DamageFrequency  + "  = distributedAttackPoint: "+distributedAttackPoint);
//				#endif
						
				distributedAttackPoint += Mathf.RoundToInt(distributedAttackPoint * BattleTuning.Instance._multipleAttackSkillAdjustmentFactor);
//				#if UNITY_EDITOR
//				if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
//					Log.print_always("##   _multipleAttackSkillAdjustmentFactor: " + BattleTuning.Instance._multipleAttackSkillAdjustmentFactor  + " x 적용  = distributedAttackPoint: "+distributedAttackPoint);
//				#endif
			}

			if (BattleBase.Instance.IsRankingTower)
			{
				distributedAttackPoint = Mathf.RoundToInt(distributedAttackPoint * BattleTuning.Instance._rankingTower_attack_boost);

//				#if UNITY_EDITOR
//				if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
//					Log.print_always("##   _rankingTower_attack_boost: " + BattleTuning.Instance._rankingTower_attack_boost  + " % 적용  = distributedAttackPoint: "+distributedAttackPoint);
//				#endif
			}



			if (_count_special_opponent > 0)
			{
				float specialAttackRate = getAttackRate_SpecialOpponent();
				if (specialAttackRate == float.MinValue)
					return distributedAttackPoint;

//				#if UNITY_EDITOR
//				if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
//					Log.print_always("##   specialAttackRate: " + specialAttackRate  + " % 적용  = distributedAttackPoint: "+(Mathf.RoundToInt(distributedAttackPoint * specialAttackRate)));
//				#endif
				return Mathf.RoundToInt(distributedAttackPoint * specialAttackRate); //jks use special attack.
			}
			else
			{
				return distributedAttackPoint;  //jks use original.
			}
		}
	}
		

	public bool TooDangerous
	{
		get { return _tooDangerous; } //default
		set {_tooDangerous = value; }
	}


	public bool IsCloseAttack
	{
		get { return _attackType == eAttackType.AT_Fist || _attackType == eAttackType.AT_Sword; } 
	}


	protected virtual List<FighterActor> Opponents { get { return BattleBase.Instance.List_Enemy; }}


	public virtual bool IsProtectee { get { return false; }}


	public virtual eAttackType AttackType { get { return _attackType; } }

	public virtual eSkillType SkillType { get { return eSkillType.ST_Card; } set { _skillType = value; }}

	public bool HoldUpdate
	{
		get { return _holdUpdate; }
		set {_holdUpdate = value; }
	}


	//jsm
	protected GameObject _weaponObject = null;  // default skill weapon
	public bool DoIHaveWeapon 
	{ 
		get 
		{
			return _weaponObject != null;				
		} 
	}

	protected GameObject _weaponObject_current = null; //jks current weapon. (one of skill, sskill1, skill2, skill3)
	public GameObject WeaponObject_Current { get {return _weaponObject_current;} set { _weaponObject_current = value;}}

	
	public bool Action_PostVictory { get { return _postVictory; } set {_postVictory = value; } }
	public bool Action_Death { get { return _deathStart; } set {_deathStart = value; } }
	public bool Action_DeathB { get { return _deathBStart; } set {_deathBStart = value; } }
	public bool Action_Hit { get { return _hit; } set {_hit = value; } }
	public bool Action_Idle { get { return _idle; } set {_idle = value; } }
	public bool Action_Paused { get { return _paused; } set {_paused = value; } }
	public bool Action_Walk { get { return _walk; } 
		set 
		{
//				if (value && IsLeader)
//					Log.jprint("Action_Walk = "+ value);
				_walk = value; 
		} 
	}
	public bool Action_WalkBack { get { return _walkBack; } set {_walkBack = value; } }
	public bool Action_WalkBackTurn { get { return _walkBackTurn; } set {_walkBackTurn = value; } }
	public bool Action_WalkFast { get { return _walkFast; } set {_walkFast = value; } }
	public bool Action_Run 
	{ get { return _run; } 
		set 
		{
//			if (value && IsLeader)
//				Log.jprint(Time.time+ "    " + gameObject + " **********   **********  Action Run = "+ value);
			_run = value; 
		} 
	}
	public bool Action_Stun { get { return _stun; } set {_stun = value; } }
	public bool Action_Captured { get { return _captured; } set {_captured = value; } }



	#region Captured in the air

	float _delay_adjustment = 0.3f;
	protected  float _captured_air_height;
	public bool Action_Captured_InTheAir { get { return _captured_in_the_air; } set {_captured_in_the_air = value; } }
	public void resetActionCapturedInTheAir() 
	{
		if (AmIHumanoid)
			placeAirPosition(false);
		
		Invoke("resetAirCaptureAction", _delay_adjustment);
	}

	protected void resetAirCaptureAction()
	{
		Action_Captured_InTheAir = false; 
		if (AmIAuto)
		{
			if (KnowledgeMultiSkill != null)
				KnowledgeMultiSkill.forceResetComboFlags();  //jks capture_air 된 이후에 움직이지 않는 현상 수정.
			Action_Run = true;
		}
	}


	protected  virtual void resetActionCapturedInTheAir_Delay(float delay) 
	{ 
		Invoke("resetActionCapturedInTheAir", delay-_delay_adjustment); 
	}

	public virtual void setActionCapturedInTheAir(float height, float delay) 
	{  
		Action_Captured_InTheAir = true;

		if (AmIHumanoid)
		{
			_captured_air_height = height + Random.Range(-0.2f, 0.7f);

			//set initial local height
			Vector3 localPos = transform.localPosition;
			localPos.y = 0;
			transform.localPosition = localPos;

			placeAirPosition(true);
		}
		resetActionCapturedInTheAir_Delay(delay);
	}

	protected void placeAirPosition(bool up)
	{
		if (up)
//			iTween.MoveTo(gameObject, iTween.Hash(
//				"y", _captured_air_height, 
//				"time", 0.3, 
//				"easytype", iTween.EaseType.linear, 
//				"islocal", true)); 
			StartCoroutine(airCapture_MoveUp(_captured_air_height));
		else
//			iTween.MoveTo(gameObject, iTween.Hash(
//				"y", 0,  
//				"time", 0.3,  
//				"easytype", iTween.EaseType.linear, 
//				"islocal", true));
			StartCoroutine(airCapture_MoveDown());
	}

	float _upDownSpeed = 4.0f;
	protected IEnumerator airCapture_MoveUp(float targerHeight)
	{
		Vector3 newPos = new Vector3();
		while (transform.localPosition.y < targerHeight)
		{
//			_velocity.y *= Time.deltaTime;
//			transform.Translate(_velocity);

			newPos = transform.localPosition;
			newPos.y += Time.deltaTime * _upDownSpeed;
			transform.localPosition = newPos;
			yield return null;
		}
	}
	protected IEnumerator airCapture_MoveDown()
	{
		Vector3 newPos = new Vector3();

		while (transform.localPosition.y > 0)
		{
//			_velocity.y *= -Time.deltaTime;
//			transform.Translate(_velocity);

			newPos = transform.localPosition;
			newPos.y -= Time.deltaTime * _upDownSpeed;
			transform.localPosition = newPos;
			yield return null;
		}

		newPos = transform.position;
		newPos.y = 0;
		transform.position = newPos;
	}

	#endregion



	public bool Action_PvpReady { get { return _pvpReady; } set {_pvpReady = value; } }
	public bool Action_PvpJustBeforeHit { get { return _pvpJustBeforeHit; } set {_pvpJustBeforeHit = value; } }
	public bool Action_InstallWeapon { get { return _installWeapon; } set {_installWeapon = value; } }
	public bool Action_InstallWeapon_Pre { get { return _installWeapon_pre; } set {_installWeapon_pre = value; } }
	public bool Action_Combo1 { get { return _combo1; } 
		set 
		{
//			if (value && name.Contains("C"))
//				Log.jprint(Time.time+ "    " + gameObject + " ******  *******  *******  Action_Combo1 = "+ value);
			_combo1 = value; 
		}
	}
	public bool Action_Combo2 { get { return _combo2; } 
		set 
		{
//			if (value && name.Contains("C"))
//				Log.jprint(Time.time+ "    " + gameObject + " ******  *******  *******  Action_Combo2 = "+ value);
			_combo2 = value; 
		}
	}
	public bool Action_Combo3 { get { return _combo3; } 
		set 
		{
//			if (value && name.Contains("C"))
//				Log.jprint(Time.time+ "    " + gameObject + " ******  *******  *******  Action_Combo3 = "+ value);
			_combo3 = value; 
		}
	}
	public bool Action_Combo4 { get { return _combo4; } 
		set 
		{
//			if (value && name.Contains("C"))
//				Log.jprint(Time.time+ "    " + gameObject + " ******  *******  *******  Action_Combo4 = "+ value);
			_combo4 = value; 
		}
	}
	public bool Action_Combo5 { get { return _combo5; } set {_combo5 = value; } }
	public bool Action_Combo6 { get { return _combo6; } set {_combo6 = value; } }
	public bool Action_Combo7 { get { return _combo7; } set {_combo7 = value; } }
	public bool Action_Combo8 { get { return _combo8; } set {_combo8 = value; } }

	public bool Action_Block { get { return _block; } set {_block = value; } }

	public bool Action_CoolingJump {get { return _coolingJump;} set 
		{
//			if (IsLeader)
//				Log.jprint(gameObject + "          Action_CoolingJump: " + value);
			_coolingJump = value; 
		}}


	public bool Progress_WalkBack { get { return Action_WalkBack || Action_WalkBackTurn; } }


	public bool Progress_SkillAnimation { get { return 	Progress_Anim_Combo1 || Progress_Anim_Combo2 || Progress_Anim_Combo3 || Progress_Anim_Combo4 || Progress_Anim_Combo5 ||
												Progress_Anim_Combo6 || Progress_Anim_Combo7 || Progress_Anim_Combo8; } }

	public bool Progress_SkillAction { get { return 	Action_Combo1 || Action_Combo2 || Action_Combo3 || Action_Combo4 || Action_Combo5 ||
												Action_Combo6 || Action_Combo7 || Action_Combo8; } }

	public virtual bool Progress_Action_Quick { get { return false; } }

	protected bool _firstGiveDamage = true;
	public bool FirstGiveDamage { get { return _firstGiveDamage; } set { _firstGiveDamage = value; } }



	protected Queue_Limited_UniqueItem<int> _appliedSkillBuffUIDs = new Queue_Limited_UniqueItem<int>(10);  //jks 공격 받은 스킬의 스킬버프의 고유 ID 들 저장. (영역에서 스킬을 맞은 캐릭터에게만 활성화되는 버프의 적용에 사용됨.)
	public int AppliedSkillBuff_UID { set { _appliedSkillBuffUIDs.Enqueue(value); }}
	public Queue_Limited_UniqueItem<int> AppliedSkillBuff_UID_Queue { get { return _appliedSkillBuffUIDs; }}

	protected int _myCurrentSkillBuffUID;  //jks 스킬 사용 시 마다 추가된 스킬버프의 고유 ID. (영역에서 스킬을 맞은 캐릭터에게만 활성화되는 버프의 적용에 사용됨.)
	public int MyCurrentSkillBuff_UID { get { return _myCurrentSkillBuffUID; }}

	//jks 2016.3.14 skill buff 기능 추가.
	public void addSkillBuff()
	{
		if (FirstGiveDamage)
		{
			// 스킬버프 시동 할지 판정.
			if (_buff_probability > 0)
			if ( Random.Range(0f, 1f) < _buff_probability )
				_myCurrentSkillBuffUID = BattleBase.Instance.addSkillBuffAgent(this);

			FirstGiveDamage = false;
		}
	}


	public void resetActionComboFlag()
	{
		Action_Combo1 = Action_Combo2 = Action_Combo3 = Action_Combo4 = Action_Combo5 =
		Action_Combo6 = Action_Combo7 = Action_Combo8 = false;
		
		_firstGiveDamage = true;
	}


	public void resetProgressAnimComboFlag()
	{
		Progress_Anim_Combo1 = Progress_Anim_Combo2 = Progress_Anim_Combo3 = Progress_Anim_Combo4 = Progress_Anim_Combo5 =
		Progress_Anim_Combo6 = Progress_Anim_Combo7 = Progress_Anim_Combo8 = false;
	}

	public virtual void resetActionComboQuickFlag() {}

	public virtual void resetProgressComboQuickFlag() {}

	public virtual void incrementContinuousHitCount() {}



	public bool NoNextComboAction { get { return !Progress_SkillAction; }}	

	public bool Progress_HitReaction { get { return _hitReactionInProgress;} set {_hitReactionInProgress = value; } }

	public bool Progress_Anim_Combo1 { get { return _combo1AnimInProgress; } set {
			
//			if (IsLeader && gameObject.name.Contains("C"))
//				Log.jprint(Time.time + "          Progress_Anim_Combo1 = " + value + "              " + gameObject);
			
			_combo1AnimInProgress = value;
		} }
	
	public bool Progress_Anim_Combo2 { get { return _combo2AnimInProgress; } set {_combo2AnimInProgress = value; } }
	public bool Progress_Anim_Combo3 { get { return _combo3AnimInProgress; } set {_combo3AnimInProgress = value; } }
	public bool Progress_Anim_Combo4 { get { return _combo4AnimInProgress; } set {_combo4AnimInProgress = value; } }
	public bool Progress_Anim_Combo5 { get { return _combo5AnimInProgress; } set {_combo5AnimInProgress = value; } }
	public bool Progress_Anim_Combo6 { get { return _combo6AnimInProgress; } set {_combo6AnimInProgress = value; } }
	public bool Progress_Anim_Combo7 { get { return _combo7AnimInProgress; } set {_combo7AnimInProgress = value; } }
	public bool Progress_Anim_Combo8 { get { return _combo8AnimInProgress; } set {_combo8AnimInProgress = value; } }

	public bool Progress_Death { get { return _deathInProgress; } set {_deathInProgress = value; } }
	public bool Progress_DeathB { get { return _deathBInProgress; } set {_deathBInProgress = value; } }

	public bool Progress_Block { get { return _blockInProgress; } set {_blockInProgress = value; } }

	//public bool Progress_RoleSkillAnimation { get { return _roleSkillAnimInProgress; } set {_roleSkillAnimInProgress = value; } }
	public bool Progress_GuardUpAnimation { get { return _guardUpAnimInProgress; } set {_guardUpAnimInProgress = value; } }

	public bool Progress_SkillStaging { get { return _skillStagingInProgress; } set {_skillStagingInProgress = value; } }


	public bool Progress_CoolingJump {get { return _coolingJumpInProgress;} 
		set {
//			if (IsLeader)
//			{
//				Log.jprint (gameObject + "    Progress_CoolingJump : " + value);
//			}
			_coolingJumpInProgress = value; 
		}}

	public int ComboCurrent { get { return _currentCombo; } set {_currentCombo = value; } }
	public int ComboRecent { get { return _recentCombo; } set {_recentCombo = value; } }

	public bool IsLastDamageInSkill { get { return _isLastDamageInSkill; } set {_isLastDamageInSkill = value; } }

	public CardClass Class { get { return _class; } set {_class = value;}}

	protected bool _coolingJumpDone = false;
	public bool CoolingJumpDone { get { return _coolingJumpDone; } set {_coolingJumpDone = value; } }

	public int Anim_Victory 					{ get { return _anim_victory; } }
	public int Anim_Walk 						{ get { return _anim_walk; } }
	public int Anim_WalkBack 					{ get { return _anim_walkback; } }
	public int Anim_WalkBackTurn 				{ get { return _anim_walkbackturn; } }
	public int Anim_WalkFast 					{ get { return _anim_walkfast; } }
	public int Anim_Run 						{ get { return _anim_run; } }
	public int Anim_Protect 					{ get { return _anim_protect; } }
	public int Anim_Idle 						{ get { return _anim_idle; } }
	public int Anim_PvpReady 					{ get { return _anim_pvpReady; } }
	public int Anim_PvpJustBeforeHit 			{ get { return _anim_pvpJustBeforeHit; } }
	public int Anim_InstallWeapon	 			{ get { return _anim_installWeapon; } }
	public int Anim_InstallWeapon_Pre	 		{ get { return _anim_installWeapon_pre; } }
	public int Anim_Stun 						{ get { return _anim_stun; } set { _anim_stun = value;}}
	public int Anim_Captured 					{ get { return _anim_captured; } }
	public int Anim_Captured_InTheAir			{ get { return _anim_captured_in_the_air; } }
	public int Anim_Exhausted 					{ get { return _anim_exhausted; } }
	public int Anim_CoolingJump					{ get { return _anim_coolingjump; } }
	public int Anim_Death 						{ get { return _anim_death; } }
	public int Anim_DeathB 						{ get { return _anim_deathB; } }
	public int Anim_Pause 						{ get { return _anim_pause; } }
	public int Anim_Block 						{ get { return _anim_block; } }
	public int Anim_Combo1 						{ get { return _anim_combo1; } }
	public int Anim_Combo2 						{ get { return _anim_combo2; } }
	public int Anim_Combo3 						{ get { return _anim_combo3; } }
	public int Anim_Combo4 						{ get { return _anim_combo4; } }
	public int Anim_Combo5 						{ get { return _anim_combo5; } }
	public int Anim_Combo6 						{ get { return _anim_combo6; } }
	public int Anim_Combo7 						{ get { return _anim_combo7; } }
	public int Anim_Combo8 						{ get { return _anim_combo8; } }

	public int Anim_Skill_Staging 				{ get { return _anim_skill_staging; } }

	public int Anim_GuardUp 					
	{ 
		get 
		{ 
			return _anim_guard_up[(int)_class]; 
		} 
	}

	public virtual int TotalCombo 				{ get { return _totalCombo; } }
	public virtual int LastCombo 				{ get { return _lastCombo; } }

	public virtual bool ReactionOff 			{ get { return _reactionOff; } }
	public virtual float DamagePenetration 		{ get { return _damagePenetration; } }
	public virtual float Buff_probability 		{ get { return _buff_probability; } }
	public virtual float Buff_duration 			{ get { return _buff_duration; } }
	public virtual float Attack_up 				{ get { return _attack_up; } }
	public virtual float Defense_up 			{ get { return _defense_up; } }
	public virtual float Critical_up 			{ get { return _critical_up; } }
	public virtual float Attack_down 			{ get { return _attack_down; } }
	public virtual float Defense_down 			{ get { return _defense_down; } }
	public virtual float Critical_down 			{ get { return _critical_down; } }
	public virtual bool SkillHitOnlyBuffOn 		{ get { return _skill_hit_only_buff_on; } }


	public float AttackInfo 					{ get { return _attackInfo; } }
	public int AttackInfo2 						{ get { return _attackInfo2; } }
	public int AttackInfo3 						{ get { return _attackInfo3; } }
	public string AttackInfo4 					{ get { return _attackInfo4; } }

	public virtual float ProjectileRadius 		{ get { return _projectileRadius; } }

	public int CharacterID 					{ get { return _character_ID; } }
	public int SkillID 					{ get { return _skill_ID; } }

	public int PvpAttackBoost { get { return _pvpAttackBoost; } set {_pvpAttackBoost = value; } }

	public int JumpBackCount { get { return _attackDistanceKeepingJumpCount;} set { _attackDistanceKeepingJumpCount = value;}}

	public bool AmIHumanoid 
	{ 
		get 
		{ 
			if (_fighterType == eFighterType.FT_None)
			{
				if (Utility.findTransformUsingKeyword(gameObject, "spine1") != null)
				{
					_fighterType = eFighterType.FT_Humanoid;
				}
				else
				{
					_fighterType = eFighterType.FT_Machine;
				}
			}

			return _fighterType == eFighterType.FT_Humanoid; 
		} 
	} 

	public ObscuredInt Defense { get { return _defense; } set {_defense = value; } }

	public int getDistributedDefense(int attacker_damage_frequency)
	{
		if (attacker_damage_frequency == 0)
			return Defense;
		
		return Mathf.RoundToInt(Defense / attacker_damage_frequency);
	}


	public int getAnimHash(string animStateName)
	{
		return Animator.StringToHash("Base Layer." + animStateName);
	}



	public virtual int getAnimInfoID() { return 0; }




//	//jks get  "anim_groupID" from "weapon_type" of skill table and  "anim_group_by_weaponID" of character table
//	protected int getAnimGroupID(eWeaponType_ForAnimation weaponType, int animGroupByWeaponID)
//	{
//		Table_AnimGroupByWeapon tableAnimGroupByWeapon = (Table_AnimGroupByWeapon)TableManager.GetContent(animGroupByWeaponID);
//
//		int animGroupID = tableAnimGroupByWeapon._body_anim_groupID; //default
//
//		switch(weaponType)
//		{
//		case eWeaponType_ForAnimation.WTA_Body:
//			animGroupID = tableAnimGroupByWeapon._body_anim_groupID;
//			break;
//		case eWeaponType_ForAnimation.WTA_Sinsu:
//			animGroupID = tableAnimGroupByWeapon._sinsu_anim_groupID;
//			break;
//		case eWeaponType_ForAnimation.WTA_Arrow:
//			animGroupID = tableAnimGroupByWeapon._arrow_anim_groupID;
//			break;
//		case eWeaponType_ForAnimation.WTA_Gun:
//			animGroupID = tableAnimGroupByWeapon._gun_anim_groupID;
//			break;
//		case eWeaponType_ForAnimation.WTA_Onehand:
//			animGroupID = tableAnimGroupByWeapon._onehand_anim_groupID;
//			break;
//		case eWeaponType_ForAnimation.WTA_Twohand:
//			animGroupID = tableAnimGroupByWeapon._twohand_anim_groupID;
//			break;
//		case eWeaponType_ForAnimation.WTA_Spear:
//			animGroupID = tableAnimGroupByWeapon._spear_anim_groupID;
//			break;
//		case eWeaponType_ForAnimation.WTA_Observer:
//			animGroupID = tableAnimGroupByWeapon._observer_anim_groupID;
//			break;
//		case eWeaponType_ForAnimation.WTA_Lighthouse:
//			animGroupID = tableAnimGroupByWeapon._lighthouse_anim_groupID;
//			break;
//		}
//
//		//Log.jprint("????? getAnimGroupID ???????      Table_AnimGroupByWeapon id:  "  + animGroupByWeaponID + "  weaponType: " + weaponType + "      animGroup table id: " + animGroupID);
//
//		return animGroupID;
//	}
	
//	//jks get  "animID" using anim_groupID of anim group table
//	protected virtual int getAnimID_Idle()
//	{
//		int animGroupID = getAnimGroupID(_weaponType_ForAnimation, _animGroupByWeaponID);
//		//Log.jprint("......  ........      idle"  + ((Table_AnimGroup)TableManager.GetContent( animGroupID ))._idle);
//		return ((Table_AnimGroup)TableManager.GetContent( animGroupID ))._idle;
//	}
//	protected virtual int getAnimID_Pause()
//	{
//		int animGroupID = getAnimGroupID(_weaponType_ForAnimation, _animGroupByWeaponID);
//		//Log.jprint("......  ........      idle"  + ((Table_AnimGroup)TableManager.GetContent( animGroupID ))._idle);
//		return ((Table_AnimGroup)TableManager.GetContent( animGroupID ))._idle;
//	}
//	protected virtual int getAnimID_Walk()
//	{
//		int animGroupID = getAnimGroupID(_weaponType_ForAnimation, _animGroupByWeaponID);
//		//Log.jprint("......  ........      walk"  + ((Table_AnimGroup)TableManager.GetContent( animGroupID ))._walk);
//		return ((Table_AnimGroup)TableManager.GetContent( animGroupID ))._walk;
//	}
//	protected virtual int getAnimID_Walkback()
//	{
//		int animGroupID = getAnimGroupID(_weaponType_ForAnimation, _animGroupByWeaponID);
//		//Log.jprint("......  ........      walkback"  + ((Table_AnimGroup)TableManager.GetContent( animGroupID ))._walkback);
//		return ((Table_AnimGroup)TableManager.GetContent( animGroupID ))._walkback;
//	}
//	protected virtual int getAnimID_WalkbackTurn()
//	{
//		int animGroupID = getAnimGroupID(_weaponType_ForAnimation, _animGroupByWeaponID);
//		//Log.jprint("......  ........      walkbackTurn"  + ((Table_AnimGroup)TableManager.GetContent( animGroupID ))._walkbackTurn);
//		return ((Table_AnimGroup)TableManager.GetContent( animGroupID ))._walkbackTurn;
//	}
//	protected virtual int getAnimID_Walkfast()
//	{
//		int animGroupID = getAnimGroupID(_weaponType_ForAnimation, _animGroupByWeaponID);
//		//Log.jprint("......  ........      walkfast"  + ((Table_AnimGroup)TableManager.GetContent( animGroupID ))._walkfast);
//		return ((Table_AnimGroup)TableManager.GetContent( animGroupID ))._walkfast;
//	}
//	protected virtual int getAnimID_Run()
//	{
//		int animGroupID = getAnimGroupID(_weaponType_ForAnimation, _animGroupByWeaponID);
//		//Log.jprint("......  ........      run"  + ((Table_AnimGroup)TableManager.GetContent( animGroupID ))._run);
//		return ((Table_AnimGroup)TableManager.GetContent( animGroupID ))._run;
//	}
//	protected virtual int getAnimID_Protect()
//	{
//		int animGroupID = getAnimGroupID(_weaponType_ForAnimation, _animGroupByWeaponID);
//		//Log.jprint("......  ........      protect"  + ((Table_AnimGroup)TableManager.GetContent( animGroupID ))._protect);
//		return ((Table_AnimGroup)TableManager.GetContent( animGroupID ))._protect;
//	}



	/// <summary>
	/// 자신이 자동으로 움직여야 하는 캐릭터인지 판단.
	/// </summary>
	/// <value><c>true</c> if am I auto; otherwise, <c>false</c>.</value>
//	public bool AmIAuto
//	{
//		get
//		{
//			return (IsLeader && BattleBase.Instance.IsPlay_AutoLeader) 
//				|| (!IsLeader && BattleBase.Instance.IsPlay_AutoTeam);
//		}
//	}

	public bool AmIAuto
	{
		get
		{
			if (AllyID == eAllyID.Ally_Human_Me)
			{
				return (IsLeader && BattleBase.Instance.IsPlay_AutoLeader) 
					|| (!IsLeader && BattleBase.Instance.IsPlay_AutoTeam);
			}
			else
			{
				return true;
			}
		}
	}

	public bool AmIManualLeader
	{
		get 
		{
			return IsLeader && !BattleBase.Instance.IsPlay_AutoLeader && AllyID == eAllyID.Ally_Human_Me;
		}
	}


	
	//default for dummy
	public void setAnimIdle() 					
	{ 
		//string animStateName = "Idle" + Loco.AnimID_Idle.ToString();
		string animStateName = "Idle" + "99";
		
		_anim_idle = getAnimHash(animStateName);
		Log.nprint(gameObject.name + "      " + animStateName + "    hash: " + _anim_idle);
	}


	public void setAnimIdle(Table_AnimInfo animInfoTable) 					
	{ 
		//string animStateName = "Idle" + Loco.AnimID_Idle.ToString();
		string animStateName = "Idle" + animInfoTable._anim_idle.ToString();
		
		_anim_idle = getAnimHash(animStateName);
		Log.nprint(gameObject.name + "      " + animStateName + "    hash: " + _anim_idle);
	}


	public void setAnimWalk(Table_AnimInfo animInfoTable) 					
	{
		if (BattleBase.Instance.isSkillPreviewMode()) 
			return; 

		//string animStateName = "Walk" + Loco.AnimID_Walk.ToString();
		//string animStateName = "Walk" + getAnimID_Walk().ToString();
		string animStateName = "Walk" + animInfoTable._anim_walk.ToString();

		_anim_walk = getAnimHash(animStateName);

		Log.nprint(gameObject.name + "      Walk: " + animStateName + "    hash: " + _anim_walk);
	}

	public void setAnimWalkback(Table_AnimInfo animInfoTable) 				
	{ 
		if (BattleBase.Instance.isSkillPreviewMode()) 
			return; 
		
		//string animStateName = "Walkback" + Loco.AnimID_Walkback.ToString();
		//string animStateName = "Walkback" + getAnimID_Walkback().ToString();
		string animStateName = "Walkback" + animInfoTable._anim_walkback.ToString();

		_anim_walkback = getAnimHash(animStateName);

		Log.nprint(gameObject.name + "      " + animStateName + "    hash: " + _anim_walkback);

	}
	
	public void setAnimWalkbackTurn(Table_AnimInfo animInfoTable) 				
	{ 
		if (BattleBase.Instance.isSkillPreviewMode()) 
			return; 
		
		//string animStateName = "WalkbackTurn" + getAnimID_WalkbackTurn().ToString();
		string animStateName = "WalkbackTurn" + animInfoTable._anim_walkbackTurn.ToString();

		_anim_walkbackturn = getAnimHash(animStateName);

		Log.nprint(gameObject.name + "      " + animStateName + "    hash: " + _anim_walkbackturn);
	}
	
	public void setAnimWalkfast(Table_AnimInfo animInfoTable) 				
	{ 
		if (BattleBase.Instance.isSkillPreviewMode()) 
			return; 
		
		//string animStateName = "Walkfast" + Loco.AnimID_Walkfast.ToString();
		string animStateName = "Walkfast" + animInfoTable._anim_walkfast.ToString();

		_anim_walkfast = getAnimHash(animStateName);

		Log.nprint(gameObject.name + "      " + animStateName + "    hash: " + _anim_walkfast);
	}
	

	public void setAnimRun(Table_AnimInfo animInfoTable) 					
	{ 
		//string animStateName = "Run" + Loco.AnimID_Run.ToString();
		//string animStateName = "Run" + getAnimID_Run().ToString();
		string animStateName = "Run" + animInfoTable._anim_run.ToString();

		_anim_run = getAnimHash(animStateName);

		Log.nprint(gameObject.name + "      " + animStateName + "    hash: " + _anim_run);
	}
	
	public void setAnimProtect(Table_AnimInfo animInfoTable) 					
	{ 
		//_anim_protect = getAnimID_Protect();  //jks surfix of "Protect"
		_anim_protect = animInfoTable._anim_protect; //jks surfix of "Protect"
	}
	


	public void setAnimDeath() 					
	{
		if (BattleBase.Instance.isSkillPreviewMode()) 
			return; 
		
		string animStateName = "Death" + Loco.AnimID_Death.ToString();
		
		_anim_death = getAnimHash(animStateName);

		Log.nprint(gameObject.name + "      " + animStateName + "    hash: " + _anim_death);
	}


	public void setAnimDeathB() 					
	{
		if (BattleBase.Instance.isSkillPreviewMode()) 
			return; 

		string animStateName = "DeathB" + Loco.AnimID_DeathB.ToString();

		_anim_deathB = getAnimHash(animStateName);

		Log.nprint(gameObject.name + "      " + animStateName + "    hash: " + _anim_deathB);
	}


	public void setAnimPause() 					
	{ 
		string animStateName = "Pause" + Loco.AnimID_Pause.ToString();

		_anim_pause = getAnimHash(animStateName);
		
		Log.nprint(gameObject.name + "      " + animStateName + "    hash: " + _anim_pause);
	}
	

	public void setAnimVictory(Table_AnimInfo animInfoTable) 					
	{
		if (BattleBase.Instance.isSkillPreviewMode()) 
			return; 
		
		//string animStateName = "Victory" + Loco.AnimID_Victory.ToString();
		string animStateName = "Victory" + animInfoTable._anim_victory.ToString();

		_anim_victory = getAnimHash(animStateName);
		
		Log.nprint(gameObject.name + "      " + animStateName + "    hash: " + _anim_victory);
	}


	public void setAnimPvpReady() 					
	{ 
		string animStateName = "PvpReady" + Loco.AnimID_PvpReady.ToString();
		
		_anim_pvpReady = getAnimHash(animStateName);
		
		Log.nprint(gameObject.name + "      " + animStateName + "    hash: " + _anim_pvpReady);
	}
	
	public void setAnimPvpJustBeforeHit() 					
	{ 
		string animStateName = "PvpJustBeforeHit" + Loco.AnimID_PvpJustBeforeHit.ToString();
		
		_anim_pvpJustBeforeHit = getAnimHash(animStateName);
		
		Log.nprint(gameObject.name + "      " + animStateName + "    hash: " + _anim_pvpJustBeforeHit);
	}
	
	public void setAnimInstallWeapon(Table_AnimInfo animInfoTable) 					
	{ 
		//string animStateName = "InstallWeapon" + Loco.AnimID_InstallWeapon.ToString();
		string animStateName = "InstallWeapon" + animInfoTable._anim_installweapon.ToString();

		_anim_installWeapon = getAnimHash(animStateName);
		
		Log.nprint(gameObject.name + "      " + animStateName + "    hash: " + _anim_installWeapon);
	}
	
	public void setAnimInstallWeapon_Pre() 					
	{ 
		string animStateName = "InstallWeapon_Pre" + Loco.AnimID_InstallWeapon_Pre.ToString();
		
		_anim_installWeapon_pre = getAnimHash(animStateName);
		
		Log.nprint(gameObject.name + "      " + animStateName + "    hash: " + _anim_installWeapon_pre);
	}
	
//	public void setAnimStun() 					
//	{ 
////		if (BattleBase.Instance.isSkillPreviewMode()) 
////		{
////			_anim_stun 		= getAnimHash("Stun1"); 
////			return; 
////		}
//
//		string animStateName = "Stun" + Loco.AnimID_Stun.ToString();
//		
//		_anim_stun = getAnimHash(animStateName);
//	}

	public void setAnimCaptured_InTheAir(Table_AnimInfo animInfoTable) 					
	{ 
		int animForGeneric = animInfoTable._anim_generic_captured_air;

		string animStateName;

		if (animForGeneric != 0)
		{
			animStateName = "Idle" + animForGeneric.ToString();
			_fighterType = eFighterType.FT_Machine;
		}
		else
			animStateName = "Captured_Air" + animInfoTable._anim_captured_air.ToString();

		_anim_captured_in_the_air = getAnimHash(animStateName);

		Log.nprint(gameObject + "      " + animStateName + "    hash: " + _anim_captured_in_the_air);
	}

	public void setAnimCaptured() 					
	{ 
		string animStateName = "Captured" + Loco.AnimID_Captured.ToString();

		_anim_captured = getAnimHash(animStateName);

		Log.nprint(gameObject.name + "      " + animStateName + "    hash: " + _anim_captured);
	}

	public void setAnimExhausted(Table_AnimInfo animInfoTable) 					
	{ 
		if (BattleBase.Instance.isSkillPreviewMode()) 
			return; 
		
		//string animStateName = "Exhausted" + Loco.AnimID_Exhausted.ToString();
		string animStateName = "Exhausted" + animInfoTable._anim_exhausted.ToString();

		_anim_exhausted = getAnimHash(animStateName);
		
		Log.nprint(gameObject.name + "      " + animStateName + "    hash: " + _anim_exhausted);
	}
	
	public void setAnimCoolingJump(Table_AnimInfo animInfoTable) 					
	{ 
		if (BattleBase.Instance.isSkillPreviewMode()) 
			return; 
		
		//string animStateName = "CoolingJump" + Loco.AnimID_CoolingJump.ToString();
		string animStateName = "CoolingJump" + animInfoTable._anim_coolingjump.ToString();

		_anim_coolingjump = getAnimHash(animStateName);
		
		Log.nprint(gameObject.name + "      " + animStateName + "    hash: " + _anim_coolingjump);
	}
	
	public void setAnimBlock(Table_AnimInfo animInfoTable) 					
	{ 
		if (BattleBase.Instance.isSkillPreviewMode()) 
			return; 
		
		//string animStateName = "Block" + Loco.AnimID_Block.ToString();
		string animStateName = "Block" + animInfoTable._anim_block.ToString();

		_anim_block = getAnimHash(animStateName);
		
		Log.nprint(gameObject.name + "      " + animStateName + "    hash: " + _anim_block);
	}
	


//	float _standardWeight = 80;

//jks - it's hard for game designer to control this feature, this will be obsolete from now (2015.1.2).
//	public float calculate_SkillAnimationPlayRate()
//	{
//		if (_skillAnimationPlayRate < 0)
//		{
//			float M = _standardWeight;
//			float W = _weight;
//			if (W == 0)
//			{
//				Debug.LogWarning(gameObject + "    Character weight is not specified.  80 kg is used for now.  캐릭터 무게가 0 입니다. 캐릭터 테이블 (Table_Character)에서 설정하세요. 임시로 디폴트( 80 kg) 으로 적용합니다. ~");
//				W = _standardWeight; //jks default weight
//			}
//			float B = Mathf.Pow(W, 1.7f);
//			float C = Mathf.Pow(_weaponWeight, 2.0f);
//
//			float agility = 1+(M/W-1)*0.4f; //jks 캐릭터 weight에 의한 민첩성 
//			float applicationRate = 1-(C/B); //jks 무기 weight 로 받는 영향률   
//			
//			float animationPlayRate = agility * applicationRate;
//
//			if (animationPlayRate < 0.6f)
//				_skillAnimationPlayRate = 0.6f;
//			else if (animationPlayRate > 1.8f)
//				_skillAnimationPlayRate = 1.8f;
//			else
//				_skillAnimationPlayRate = animationPlayRate;
//
//			//Log.print("> > > > > CHARACTER WEIGHT  :  " + gameObject + "    character: "+ W +  "   weapon: "+ _weaponWeight +"  range clamped play rate : " + _skillAnimationPlayRate + "  before clamping: "+animationPlayRate);
//		}
//
//		//Log.print("> > > > >  " + gameObject + "  :  Skill Animation Play Rate: " + _skillAnimationPlayRate);
//
//		return _skillAnimationPlayRate;
//	}

	public float calculate_SkillAnimationPlayRate()
	{
		//jks if not initialized
		if (_skillAnimationPlayRate < 0)
		{
			//jks TODO: 6 가문 속성 중 speed 값 가져와 여기서 적용.
			_skillAnimationPlayRate = 1;
			_skillAnimationPlayRate += _boostSkillSpeed;
			//Log.jprint(" _skillAnimationPlayRate : "+ _skillAnimationPlayRate +"      _boostSkillSpeed: " + _boostSkillSpeed );
		}

		//if (! gameObject.name.Contains("C"))
		//	Log.jprint(gameObject + "      ComboCurrent: "+ ComboCurrent +  " _skillAnimationPlayRate : "+ _skillAnimationPlayRate +"      _slowSkill: " + _slowSkill +"      final: " + (_skillAnimationPlayRate * _slowSkill));

		float leaderBuff = 0;
		leaderBuff = getLeaderBuffSkillSpeed (_skillAnimationPlayRate);

		return (_skillAnimationPlayRate + leaderBuff) * SkillSpeedAdjustment * PauseSpeedAdjustment;
	}


	public virtual float calculate_MovementPlayRate()
	{
		//jks if not initialized
//		if (_movementPlayRate < 0)
//		{
//			_movementPlayRate = 1;
//		}
//		
//		Log.jprint(" _movementPlayRate : "+ _movementPlayRate +"      WeaponSlowMovement: " + WeaponSlowMovement +"      final: " + (_movementPlayRate * WeaponSlowMovement));

		return WeaponSlowMovement * PauseSpeedAdjustment;
	}


	public void setAnimCombo1(int attackID) 	{ _anim_combo1 = getAnimHash("Attack" + attackID.ToString()); Log.nprint(gameObject.name +"- - - combo1 - - - "+ "Attack" + attackID.ToString() + " hash: "+ _anim_combo1); }
	public void setAnimCombo2(int attackID) 	{ _anim_combo2 = getAnimHash("Attack" + attackID.ToString()); Log.nprint(gameObject.name +"- - - combo2 - - - "+ "Attack" + attackID.ToString() + " hash: "+ _anim_combo2); }
	public void setAnimCombo3(int attackID) 	{ _anim_combo3 = getAnimHash("Attack" + attackID.ToString()); Log.nprint(gameObject.name +"- - - combo3 - - - "+ "Attack" + attackID.ToString() + " hash: "+ _anim_combo3); }
	public void setAnimCombo4(int attackID) 	{ _anim_combo4 = getAnimHash("Attack" + attackID.ToString()); Log.nprint(gameObject.name +"- - - combo4 - - - "+ "Attack" + attackID.ToString() + " hash: "+ _anim_combo4); }
	public void setAnimCombo5(int attackID) 	{ _anim_combo5 = getAnimHash("Attack" + attackID.ToString()); Log.nprint(gameObject.name +"- - - combo5 - - - "+ "Attack" + attackID.ToString() + " hash: "+ _anim_combo5); }
	public void setAnimCombo6(int attackID) 	{ _anim_combo6 = getAnimHash("Attack" + attackID.ToString()); Log.nprint(gameObject.name +"- - - combo6 - - - "+ "Attack" + attackID.ToString() + " hash: "+ _anim_combo6); }
	public void setAnimCombo7(int attackID) 	{ _anim_combo7 = getAnimHash("Attack" + attackID.ToString()); Log.nprint(gameObject.name +"- - - combo7 - - - "+ "Attack" + attackID.ToString() + " hash: "+ _anim_combo7); }
	public void setAnimCombo8(int attackID) 	{ _anim_combo8 = getAnimHash("Attack" + attackID.ToString()); Log.nprint(gameObject.name +"- - - combo8 - - - "+ "Attack" + attackID.ToString() + " hash: "+ _anim_combo8); }

	public void setAnimGuardUp(int characterClassType) 		
	{ 
		_anim_guard_up[characterClassType] = getAnimHash("GuardUp" + characterClassType.ToString()); 
#if UNITY_EDITOR
		Log.nprint(gameObject.name + "      " + "GuardUp" + characterClassType.ToString() + "    hash: " + _anim_guard_up[characterClassType]);
#endif
	}

	public void setAnimSkillStaging(int stagingID) 	
	{ 
		_anim_skill_staging = getAnimHash("SkillStaging" + stagingID.ToString());		
#if UNITY_EDITOR
		Log.nprint(gameObject.name + "      " + "SkillStaging" + stagingID.ToString() + "    hash: " + _anim_skill_staging);
#endif
	}

	
	public virtual int animHitReaction() 
	{ 
		if (Loco.ReactionAnimID_Override == 0)
		{
			if (_animHitNumber > 1000) //jks if hit type is miss or bad, do protection motion.
			{
#if UNITY_EDITOR
				Log.nprint(gameObject.name + "      " + "Protect" + _animHitNumber.ToString() + "    hash: " + getAnimHash("Protect" + _animHitNumber.ToString()));
#endif
				return getAnimHash("Protect" + _animHitNumber.ToString()); 
			}
			else
			{
#if UNITY_EDITOR
				Log.nprint(gameObject.name + "      " + "Reaction" + _animHitNumber.ToString() + "    hash: " + getAnimHash("Reaction" + _animHitNumber.ToString()));
#endif
				return getAnimHash("Reaction" + _animHitNumber.ToString()); 
			}
		}
		else
		{
#if UNITY_EDITOR
			Log.nprint(gameObject.name + "      " + "Reaction" + Loco.ReactionAnimID_Override.ToString() + "    hash: " + getAnimHash("Reaction" + Loco.ReactionAnimID_Override.ToString()));
#endif
			return getAnimHash("Reaction" + Loco.ReactionAnimID_Override.ToString()); 
		}
	}


	public virtual int animStunReaction(int animNum)
	{
		if (Loco.AnimID_Stun != 0)
		{
			return getAnimHash("Stun" + Loco.AnimID_Stun.ToString());
		}
		else
		{
			return getAnimHash("Stun" + animNum);
		}
	}

	
	protected override void initializeBeforeUpdateBegin()
	{
		base.initializeBeforeUpdateBegin();

		forceResetFlags();//resetActionInfo();

		accordAttackDirection();

		findWeaponBones();

		//jks  이동.. attachHPBar();
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


	public void accordAttackDirection()
	{
		if (IsDead) return;
		
		if (AllyID == eAllyID.Ally_Human_Me)
		{
			transform.rotation = Quaternion.Euler(0, 90, 0);
		}
		else
		{
			transform.rotation = Quaternion.Euler(0, -90, 0);
		}
	}


	public bool isEnemyAppeared()
	{
		GameObject[] gosInWorld = GameObject.FindGameObjectsWithTag("Fighters");
		if (gosInWorld.Length == 0) return false;

		foreach(GameObject go in gosInWorld)
		{
			Knowledge_Mortal goKnowledge = go.GetComponent<Knowledge_Mortal>();

			if (goKnowledge == null) continue;
			if (goKnowledge.AllyID == AllyID) continue;

			if (Utility.isGameObjectAhead(transform, go))
				return true;
		}

		return false;
	}



	public virtual bool canUseSkill()
	{
		if (MadPauser.Instance.TimeState == eTimeState.TIME_PAUSE) return false;

		//jks 전투 진행 중이면, 지금 스킬 사용 못 함.
		if (! IsBattleInProgress) return false;

		//jks 이미 스킬 시작 되었으면, 지금 스킬 사용 못 함.
		if (Progress_SkillAction || Progress_SkillAnimation) return false;

//jks 2016.4.21 Mantis[1449]리더 고유 스킬 카드의 사용을 1 순위로.	//jks 현재 맞는 동작 중이면, 지금 스킬 사용 못 함.
//jks 2016.4.21 Mantis[1449]리더 고유 스킬 카드의 사용을 1 순위로:  if (Action_Hit) return false;

		//jks 쿨타임 중이면, 지금 스킬 사용 못 함.
		//if (_coolTimer != null && _coolTimer.IsCoolingInProgress) return false;
		if (_coolTimer != null && _coolTimer.IsEnabled) return false;


		return true;
	}




	#region CoolTime


	public AttackCoolTime AttackCoolTimer 
	{
		get { return _coolTimer; } 
		set 
		{ 
			_coolTimer = value;
			_coolTimer.registerCooltimeFinishedEvent(processCooltimeFinished);
		}
	}

	public AttackCoolTime_GuardUp AttackCoolTimer_GuardUp 
	{
		get { return _coolTimer_GuardUp; } 
		set 
		{ 
			_coolTimer_GuardUp = value;
//			_coolTimer_GuardUp.registerCooltimeFinishedEvent2(processCooltimeFinished_RoleSkill);
		}
	}
	


	protected  virtual void processCooltimeFinished()
	{
	}
//	protected  virtual void processCooltimeFinished_RoleSkill()
//	{
//	}

	public void resetCoolTimer()
	{
		if (_coolTimer)
		{
			_coolTimer.reset();
		}
	}
	public void resetCoolTimer_GuardUp()
	{
		if (_coolTimer_GuardUp)
		{
			_coolTimer_GuardUp.reset();
		}
	}


	public virtual bool isCoolingInProgress()
	{
		if (_coolTimer == null) return true;
		
		if (BattleBase.Instance.isSkillPreviewMode())
			return false;

		return _coolTimer.IsCoolingInProgress;
	}
	public virtual bool isCoolingInProgress_GuardUp()
	{
		if (_coolTimer_GuardUp == null) return true;
		
		if (BattleBase.Instance.isSkillPreviewMode())
			return false;
		
		return _coolTimer_GuardUp.IsCoolingInProgress;
	}

	

	public virtual void startCoolTimer()
	{
		if (_coolTimer.IsCoolingInProgress)
			return;

		_coolTimer.activateTimer(_coolTime);


		//play Idle/Cool animation during cooltime
		//jks stop freezing - forceResetFlags();
		Action_Idle = true;
	}
	public virtual void startCoolTimer_GuardUp()
	{
	}



	protected virtual void startCoolTimeAndResetComboFlag()
	{
		startCoolTimer();
		
		_recentHitType = eHitType.HT_NONE;
		_skillCompleted = false; // reset
		ComboRecent = 0;
		ComboCurrent = 0;
	}

	#endregion



	public GameObject getCurrentTarget()
	{
		return _target_attack;
	}

	public virtual void setTarget(GameObject target)
	{
//		if (target != null)
//			if (gameObject.name.Contains("FX"))
//				Log.print(gameObject + "          setTarget : " + target);
		_target_attack = target;
	}


	public virtual void unsetTarget()
	{
		setTarget(null);
	}
	
	
	public virtual void setTarget(Vector3 target)
	{
		_target_position = target;
	}

//	public bool IsTargetPositionFar
//	{
//		get
//		{			
//			float dist = Mathf.Abs(transform.position.x - _target_position.x);
//			if (dist > 0.1f)
//			{			
//				return true;
//			}
//			return false;
//		}
//	}
	
//	public virtual bool IsTargetObjectInRange
//	{
//		get
//		{
//			if (_target_attack == null) return false;
//			
//			float dist = Mathf.Abs(transform.position.x - _target_attack.transform.position.x);
//
//			float distShell = dist - _radius - _target_attack.GetComponent<Knowledge_Mortal>()._radius;
//
//			if (_attackDistanceMin < distShell && distShell < _attackDistanceMax)
//			{			
//				return true;
//			}
//			return false;
//		}
//	}
	
//	public bool IsTargetDead
//	{
//		get
//		{
//			if (_target_attack == null) return true;
//			Knowledge_Mortal knowledge =  _target_attack.GetComponent<Knowledge_Mortal>();
//			if (knowledge == null) return true;
//			if  (knowledge.IsDead) unsetTarget();
//			return knowledge.IsDead;
//		}
//	}

//	public int calculateClassRelationAdditionalPoint(int originalAttack, CardClass opponent )
//	{
//		if (isAttackerSuperiorClass(this.Class, opponent))
//		{
//			return (int)(originalAttack * 0.1f); //jks give 10% more.  //jks TODO : get 0.1 from table
//		}
//		else
//		{
//			return 0;
//		}
//	}

	//	protected virtual bool isAttackerSuperiorClass(CardClass attacker, CardClass victim)
//	{
//		//Log.jprint(gameObject + "   me: "+ (int)this.Class + " opp: " + (int)opponent + " superior table : " + _isSuperiorClass_Melee[(int)this.Class, (int)opponent]);
//		return _isSuperiorClass_Melee[(int)attacker, (int)victim] == 1;
//	}

	public virtual bool amIMelee()
	{
		return true;
	}

	public virtual bool canIAttackFlyingOpponent()
	{
		return false;
	}
	
	public virtual bool amIAirBorne()
	{
		return Loco.AmIAirborne;
	}

	public virtual bool amIMoving()
	{
		return AnimCon.isAnimPlaying(Anim_Run) || AnimCon.isAnimPlaying(Anim_Walk);
	}


	public virtual bool checkEnemyOnPath(bool bQuickSkill)
	{
		return false;
	}


	protected bool isOpponentsInDistance(float scanDistance)
	{
		if (Opponents.Count == 0) return false;
		

		foreach (FighterActor opp in Opponents)
		{
			GameObject go = opp._go;
			if (go == null) continue;
			if (!go.activeSelf) continue;

			//jks check damage range
			Knowledge_Mortal oppKnow = go.GetComponent<Knowledge_Mortal>();
			if (oppKnow == null) continue;

			float dist = Mathf.Abs(transform.position.x - go.transform.position.x);		
			float distShell = dist - this.Radius - oppKnow.Radius;

			if (distShell < scanDistance) 
				return true;
		}

		return false;
	}

//	protected bool isOpponentsInDistance(float scanDistance, string tagNameToSearch)
//	{
//		List<GameObject> oppnentsInDistance = new List<GameObject>();
//
//		float heading = transform.forward.x;
//
//		GameObject[] gosInWorld = GameObject.FindGameObjectsWithTag(tagNameToSearch);
//		if (gosInWorld.Length == 0) return false;
//
//
//		//jks check if I can attack (in attackable distance).
//		Utility.findGameObjectsInRange(heading, scanDistance, transform.position, gosInWorld, oppnentsInDistance);
//
//		foreach(GameObject go in oppnentsInDistance)
//		{
//			Knowledge_Mortal goKnowledge = go.GetComponent<Knowledge_Mortal>();
//
//			if (goKnowledge == null) continue;
//			if (goKnowledge.AllyID == AllyID) continue;
//
//			return true;
//		}
//
//		return false;
//	}


	#region Agent Seperation

	bool _pauseProcessDone = false;

	//jks pause 했을 때  캐릭터 떨리는 현상 방지.
	protected void pauseProcess()
	{
		if (MadPauser.Instance.TimeState == eTimeState.TIME_PAUSE)
		{
			if (!_pauseProcessDone)
			{
				_pauseProcessDone = true;
				iTween.Stop(gameObject);  
			}
		}
		else
		{
			_pauseProcessDone = false;
		}
	}


	protected virtual void LateUpdate()
	{
		if (Progress_SkillAnimation)
		{
			//Profiler.BeginSample("000");
			pushOpponentWhenIAttack();
			//Profiler.EndSample();
		}
		else if (Action_Run || Action_Walk || Action_WalkFast)
		{
			//GameObject target = getCurrentTarget();
			pushPassedOpponentsWhenIMove();

		}

		pauseProcess();

//jks 2015.11.4 보스액션 제거.		checkBossAction();
	}


//jks 2015.11.4 보스액션 제거.
//	protected virtual void checkBossAction()
//	{
//		return;
//	}


	protected virtual void pushOpponentWhenIAttack()
	{
		if (!IsBattleTimeStarted) return;

		GameObject target = getCurrentTarget();
		
		if (target == null) return;

		if (IsDead) return;
		if (AmICaptured) return;

		Knowledge_Mortal_Fighter targetKnowledge = target.GetComponent<Knowledge_Mortal_Fighter>();

		if (targetKnowledge == null || targetKnowledge.IsDead) return;
		if (targetKnowledge.AmICaptured) return;

		//Profiler.BeginSample("111");
		if (target != _captured_target) // if i hold opponent, do not push it to keep hold.
			keepOpponentAtWeaponEnd(target, 0.5f);
		//Profiler.EndSample();

		//jks check other opponent who pass me
		pushPassedOpponents();
	}


	protected virtual void pushPassedOpponents(){}

	protected virtual void pushPassedOpponentsWhenIMove(){}
	
	


//	protected virtual void keepOpponentAtWeaponEnd(GameObject opponent, float penetration)
//	{
//		float opponentRadius = opponent.GetComponent<Knowledge_Mortal_Fighter>().Radius;
//
//		//Profiler.BeginSample("222");
//		Vector3 newPosition = opponent.transform.position;
//
//		if (opponentRadius < penetration)  //jks for very thin character,  penetrate only 20 % of it's radius
//		{
//			opponentRadius += opponentRadius * 0.2f;
//		}
//		//Profiler.EndSample();
//
//		//Profiler.BeginSample("333");
//		if (transform.forward.x > 0)
//		{
//			//Profiler.BeginSample("334");
//			if (WeaponEndPosition.x > opponent.transform.position.x - opponentRadius)
//			{
//				newPosition.x = WeaponEndPosition.x + opponentRadius - penetration;
//				newPosition.x = Mathf.Max(newPosition.x, opponent.transform.position.x);
//			}
//			//Profiler.EndSample();
//		}
//		else
//		{
//			//Profiler.BeginSample("335");
//			if (WeaponEndPosition.x < opponent.transform.position.x + opponentRadius)
//			{
//				newPosition.x = WeaponEndPosition.x - opponentRadius + penetration;
//				newPosition.x = Mathf.Min(newPosition.x, opponent.transform.position.x);
//			}
//			//Profiler.EndSample();
//		}
//		//Profiler.EndSample();
//
//		opponent.transform.position = newPosition;
//
//		//if (opponent.name.Contains("C"))
//			//Log.jprint(opponent + "  ~ ~ ~ ~ ~   keepOpponentAtWeaponEnd()     position: "+ opponent.transform.position);
//
//	}



	protected virtual void keepOpponentAtWeaponEnd(GameObject opponent, float penetration)
	{
		float opponentRadius = opponent.GetComponent<Knowledge_Mortal_Fighter>().Radius;

		//Profiler.BeginSample("222");
		float newX = opponent.transform.position.x;

		if (opponentRadius < penetration)  //jks for very thin character,  penetrate only 20 % of it's radius
		{
			opponentRadius += opponentRadius * 0.2f;
		}
		//Profiler.EndSample();

		//Profiler.BeginSample("333");
		if (transform.forward.x > 0)
		{
			//Profiler.BeginSample("334");
			if (WeaponEndPosition.x > opponent.transform.position.x - opponentRadius)
			{
				newX = WeaponEndPosition.x + opponentRadius - penetration;
				newX = Mathf.Max(newX, opponent.transform.position.x);
			}
			//Profiler.EndSample();
		}
		else
		{
			//Profiler.BeginSample("335");
			if (WeaponEndPosition.x < opponent.transform.position.x + opponentRadius)
			{
				newX = WeaponEndPosition.x - opponentRadius + penetration;
				newX = Mathf.Min(newX, opponent.transform.position.x);
			}
			//Profiler.EndSample();
		}
		//Profiler.EndSample();

		newX = Mathf.Abs(newX - opponent.transform.position.x);

		Vector3 myPosition = transform.position;
		Vector3 opPosition = opponent.transform.position;

		myPosition.x += newX * (1 - TestOption.Instance()._battle_attack_push_opponent_weight) * ( transform.forward.x > 0 ? -1 : 1 );
		opPosition.x += newX * TestOption.Instance()._battle_attack_push_opponent_weight * ( opponent.transform.forward.x > 0 ? -1 : 1 );

//		if (IsBoss)
//		{
//			Log.jprint(opponent + "    ~ ~ ~ ~ ~   opponent position:   "+ opPosition.x + "    old: "+ opponent.transform.position.x);
//			Log.jprint(gameObject + "   ~ ~ ~ ~ ~   me  position :  " + myPosition.x+ "      old: "+ transform.position.x);
//		}

		opponent.transform.position = opPosition;
		transform.position = myPosition;

	}


	protected void keepOpponentAtShellEnd(GameObject opponent, float penetration)
	{

		float opponentRadius = opponent.GetComponent<Knowledge_Mortal_Fighter>().Radius;

		//Profiler.BeginSample("222");
		Vector3 newPosition = opponent.transform.position;
		
		if (opponentRadius < penetration)  //jks for very thin character,  penetrate only 20 % of it's radius
		{
			opponentRadius += opponentRadius * 0.2f;
		}
		//Profiler.EndSample();

		float shellEndX;

		//Profiler.BeginSample("333");
		if (transform.forward.x > 0)
		{
			//Profiler.BeginSample("334");
			shellEndX = transform.position.x + Radius;
			if (shellEndX > opponent.transform.position.x - opponentRadius)
			{
				newPosition.x = shellEndX + opponentRadius - penetration;
				newPosition.x = Mathf.Max(newPosition.x, opponent.transform.position.x);
			}
			//Profiler.EndSample();
		}
		else
		{
			//Profiler.BeginSample("335");
			shellEndX = transform.position.x - Radius;
			if (shellEndX < opponent.transform.position.x + opponentRadius)
			{
				newPosition.x = shellEndX - opponentRadius + penetration;
				newPosition.x = Mathf.Min(newPosition.x, opponent.transform.position.x);
			}
			//Profiler.EndSample();
		}
		//Profiler.EndSample();

//		if (opponent.GetComponent<Knowledge_Mortal_Fighter>().IsLeader)
//		{
//			Log.jprint(opponent + "  ~ ~ ~ ~ ~   keepOpponentAtShellEnd()     position: "+ opponent.transform.position);
//			Log.jprint(opponent + "  ~ ~ ~ ~ ~   pos delta : " + (newPosition.x-opponent.transform.position.x));
//		}

		
		opponent.transform.position = newPosition;

//		if (opponent.name.Contains("C"))
//			Log.jprint(opponent + "  ~ ~ ~ ~ ~   keepOpponentAtShellEnd()     position: "+ opponent.transform.position);

	}


	
//	protected virtual void bossBorderLine()
//	{
//		if (IsDead) return;
//		
////		if (BattleBase.Instance.IsPriorityTargetStrategyOn)
////		{
////			if (BattleBase.Instance.PriorityTarget != gameObject)
////				return;
////		}
//		
//		GameObject go = findOpponentCloseEnough();
//		
//		if (go == null) return;
//
//		//jks in PVP, do not push if they are not in battle.
//		if (BattleBase.Instance.IsPVP)
//		{
//			if (PvPManager.Instance.PvpFirstAttacker != go && PvPManager.Instance.PvpSecondAttacker != go) return;
//			if (PvPManager.Instance.PvpFirstAttacker != gameObject && PvPManager.Instance.PvpSecondAttacker != gameObject) return;
//		}
//
//		
//		Vector3 forceDirection = transform.forward;
//		
//		
//		//Log.jprint(">>>>   <<<<   repel ");
//		if (amIAirBorne()) //jks if i am flying, do not pass through the melee opponent. 
//		{
//			float dist = Random.Range(2.0f, 5.0f);
//			float duration = Random.Range(0.1f, 1.5f);
//			GetComponent<Locomotion_Mortal_Fighter>().pushObject(dist, -forceDirection, duration);
//		}
//		else
//		{
//			go.GetComponent<Locomotion_Mortal_Fighter>().pushObject(1.0f, forceDirection, 0.1f);
//			GetComponent<Locomotion_Mortal_Fighter>().pushObject(1.0f, -forceDirection, 0.1f);
//		}
//	}


//	/// <summary>
//	/// jks -  in case character collide monster do not push forward.
//	/// </summary>
//	protected virtual void doNotLetPassThrough()
//	{
//		MoveSpeedChangeRate = 1.0f;
//
//		if (IsDead) return;
//		if (AmICaptured) return;
//
//			
//		GameObject[] gosInWorld = GameObject.FindGameObjectsWithTag("Fighters");
//		if (gosInWorld.Length == 0) return;
//		
//		foreach(GameObject go in gosInWorld)
//		{
//			Knowledge_Mortal_Fighter goKnowledge = go.GetComponent<Knowledge_Mortal_Fighter>();
//
//			if (goKnowledge == null) continue;
//			if (goKnowledge.IsDead) continue;
//			if (goKnowledge.AllyID == AllyID) continue;
//			if (_captured_target == go) continue;
//			if (goKnowledge.AmICaptured) continue;
//
////jks obsolete:
////			//jks in PVP, do not push if they are not in battle.
////			if (BattleBase.Instance.IsPVP)
////			{
////				if (PvP2Manager.Instance.PvpFirstAttacker != go && PvP2Manager.Instance.PvpSecondAttacker != go) continue;
////				if (PvP2Manager.Instance.PvpFirstAttacker != gameObject && PvP2Manager.Instance.PvpSecondAttacker != gameObject) continue;
////			}
//
//			float dist = Mathf.Abs(transform.position.x - go.transform.position.x);
//			float distShell = dist - this.Radius - goKnowledge.Radius;
//			
//			//Log.jprint( "  dist:" + dist + "  _radius: " + _radius + "  go._radius: " + go.GetComponent<Knowledge_Mortal>()._radius);
//			//Log.jprint("_attackDistanceMin " + _attackDistanceMin + " < distShell: " + distShell + " < _attackDistanceMax: " + _attackDistanceMax);
//			//if (distShell < 0.05f) // closer than 30 cm
//			if (distShell < 0) 
//			{			
//				Vector3 newPos = go.transform.position;
//				newPos.x = transform.position.x + (this.Radius + goKnowledge.Radius + 0.06f) * transform.forward.x;
//				go.transform.position = newPos;
//
//				MoveSpeedChangeRate = 0.3f;
//
////				if (go.name.Contains("M0"))
////					Log.jprint(go + "  ~ ~ ~ ~ ~   doNotLetPassThrough()     position: "+ go.transform.position);
//			}
//		}
//	}

	protected virtual void doNotLetPassThrough()
	{
		MoveSpeedChangeRate = 1.0f;

		if (IsDead) return;
		if (AmICaptured) return;

			
		GameObject[] gosInWorld = GameObject.FindGameObjectsWithTag("Fighters");
		if (gosInWorld.Length == 0) return;
		
		foreach(GameObject go in gosInWorld)
		{
			Knowledge_Mortal_Fighter goKnowledge = go.GetComponent<Knowledge_Mortal_Fighter>();

			if (goKnowledge == null) continue;
			if (goKnowledge.IsDead) continue;
			if (goKnowledge.AllyID == AllyID) continue;
			if (_captured_target == go) continue;
			if (goKnowledge.AmICaptured) continue;

			float dist = Mathf.Abs(transform.position.x - go.transform.position.x);
			float distShell = dist - this.Radius - goKnowledge.Radius;
			

			if (distShell < 0) 
			{			
//				float delta = (-distShell + 0.06f)  ;
//
//				Vector3 myPosition = transform.position;
//				Vector3 opPosition = go.transform.position;
//		
////				myPosition.x += delta * (1 - TestOption.Instance()._battle_attack_push_opponent_weight) * ( transform.forward.x > 0 ? -1 : 1 );
////				opPosition.x += delta * (TestOption.Instance()._battle_attack_push_opponent_weight) * ( go.transform.forward.x > 0 ? -1 : 1 );
//		
//				myPosition.x += delta * 0.5f * ( transform.forward.x > 0 ? -1 : 1 );
//				opPosition.x += delta * 0.5f * ( go.transform.forward.x > 0 ? -1 : 1 );
//				go.transform.position = opPosition;
//				transform.position = myPosition;

				MoveSpeedChangeRate = 0.05f;

			}


//			//Log.jprint( "  dist:" + dist + "  _radius: " + _radius + "  go._radius: " + go.GetComponent<Knowledge_Mortal>()._radius);
//			//Log.jprint("_attackDistanceMin " + _attackDistanceMin + " < distShell: " + distShell + " < _attackDistanceMax: " + _attackDistanceMax);
//			//float checkDist = Radius + Radius + 0.05f;
//			if (distShell < 0) 
//			{			
//				Vector3 newPos = go.transform.position;
//				//newPos.x = transform.position.x + (this.Radius + goKnowledge.Radius + checkDist) * transform.forward.x;
//				newPos.x = transform.position.x + (this.Radius + goKnowledge.Radius) * transform.forward.x;
//				go.transform.position = newPos;
//
//				MoveSpeedChangeRate = 0.3f;
//
//				//				if (go.name.Contains("C"))
//				//					Log.jprint(go + "  ~ ~ ~ ~ ~   doNotLetPassThrough()     position: "+ go.transform.position);
//			}
//
		}
	}
	

	
	protected GameObject findOpponentCloseEnoughToAttack()
	{
		GameObject[] gosInWorld = GameObject.FindGameObjectsWithTag("Fighters");
		if (gosInWorld.Length == 0) return null;
		
		foreach(GameObject go in gosInWorld)
		{
			Knowledge_Mortal goKnowledge = go.GetComponent<Knowledge_Mortal>();
			
			if (goKnowledge == null) continue;
			if (goKnowledge.AllyID == AllyID) continue;
			
			float dist = Mathf.Abs(transform.position.x - go.transform.position.x);
			
			float distShell = dist - this.Radius - goKnowledge.Radius;
			
			//Log.jprint( "  dist:" + dist + "  _radius: " + _radius + "  go._radius: " + go.GetComponent<Knowledge_Mortal>()._radius);
			//Log.jprint("_attackDistanceMin " + _attackDistanceMin + " < distShell: " + distShell + " < _attackDistanceMax: " + _attackDistanceMax);
			if (distShell < _attackDistanceMax) // closer than 20 cm
			{			
				if(!go.GetComponent<Knowledge_Mortal>().IsDead)
				{
					return go;
				}
			}
		}
		return null;
	}
	#endregion


	//jks check if close enough to start attack while walking or running
	public bool getOpponentsToAttackWhileMoving(bool bQuickSkill)
	{
		List<GameObject> fightersInAttackDistance = new List<GameObject>();
		fightersInAttackDistance.Clear();
		
		bool isClosestOpponentInAttackDistance = getOpponentsInScanDistance(bQuickSkill);

		return isClosestOpponentInAttackDistance;
	}



	//jks check if close enough to start attack
	public bool getOpponentsToAttack()
	{
		//bool isClosestOpponentInAttackDistance = getOpponentsInScanDistance(scanDistance, tagNameToSearch, fightersInAttackDistance);
		GameObject target = getOpponentsInScanDistance_WeaponPositionBased();

		if (target == null) return false; 
			
		return true;
	}



	//protected bool _isTargetInShowWeaponRange = false;
	//public bool IsTargetInShowWeaponRange { get { return _isTargetInShowWeaponRange;}}

	public virtual eBotType getBotType() { return eBotType.BT_None; }

	public virtual bool getOpponentsInScanDistance(bool bQuickSkill)
	{
		return false;
	}


	protected virtual float getAttackDistanceMax()
	{
		return _attackDistanceMax;
	}


	public virtual GameObject getOpponentsInScanDistance_WeaponPositionBased()
	{

		GameObject closestOpponent = BattleBase.Instance.findClosestOpponent(AllyID, transform.position.x);
		setTarget(closestOpponent);

		if (closestOpponent == null) return null;
		
		Knowledge_Mortal opponentKnowledge = closestOpponent.GetComponent<Knowledge_Mortal>();
		
		//jks - body based
		float distShell_AttackerAndClosestOpponent = Mathf.Abs(transform.position.x - closestOpponent.transform.position.x) - this.Radius - opponentKnowledge.Radius;
		
		//jks - weapon based
		float distWeapon_AttackerAndClosestOpponent = Mathf.Abs(WeaponEndPosition.x  - closestOpponent.transform.position.x) - opponentKnowledge.Radius;
		
		//jks 무기를 뒤로 휘두르는 경우 몸보다 더 뒤에 위치하기 때문에  이 경우는 몸 위치로 계산. 
		float finalDistToCheck = Mathf.Min(distShell_AttackerAndClosestOpponent, distWeapon_AttackerAndClosestOpponent);
		
		//		if (gameObject.name.Contains("C"))
		//		{
		//			Log.jprint("+++++++++  me: "+ transform.position.x + " weapon: " + WeaponEndPosition.x + " opponent: " + closestOpponent.transform.position.x);
		//			Log.jprint(Time.time + ": " + gameObject + " found as closest: " + closestOpponent + " ++++++++++++ " + "finalDistToCheck: " + finalDistToCheck + " < ? " + (_attackDistanceMax + _weaponLength)); 
		//		}

		if (!(IsLivingWeapon && ComboCurrent >= 1)) //jks - if summoned character,  do not check distance if combo started to finish attack.
		if (_attackDistanceMax + _weaponLength < finalDistToCheck) return null;
		
		
		return closestOpponent;
	}




//	public bool HaveOpponentAhead(float scanDistance, string tagNameToSearch, List<GameObject> opponentsInScanDistance)
//	{
//		GameObject[] gosInWorld = GameObject.FindGameObjectsWithTag(tagNameToSearch);
//		if (gosInWorld.Length == 0) return false;
//		
//		float heading = transform.forward.x; 
//
//		List<GameObject> fightersInScanDist = new List<GameObject>();
//		fightersInScanDist.Clear();
//		//jks check if I can attack (in attackable distance).
//		Utility.findGameObjectsInRange(heading, scanDistance, transform.position, gosInWorld, fightersInScanDist);
//		
//		if (fightersInScanDist.Count == 0) return false;
//		
//		getOpponents(fightersInScanDist, opponentsInScanDistance);
//		
//		if (opponentsInScanDistance.Count == 0) return false;
//
//		return true;
//	}


//	//jks filter out same team
//	protected virtual bool getOpponents(List<GameObject> objects, List<GameObject> opponents)
//	{
//		foreach(GameObject go in objects)
//		{
//			Knowledge_Mortal_Fighter goKnowledge = go.GetComponent<Knowledge_Mortal_Fighter>();
//			if (goKnowledge == null) continue;
//			if (goKnowledge.AllyID == AllyID) continue;
//			if (goKnowledge.IsDead) continue;
//			if (goKnowledge.AmICaptured) continue;
//			//if (!isOpponentInFrontOfMe(go)) continue;
//
//			opponents.Add(go);
//		}
//		
//		return opponents.Count > 0;
//	}


	protected virtual void giveAreaDamageNow(float reactionDistanceOverride, float initialCenter)
	{
	}






#if UNITY_EDITOR
	//jks areal
	protected bool _arealAttack_ShowDamageRange = false;
	protected Vector3 _arealAttack_DamageCenter = new Vector3();
	protected float _arealAttack_Radius;
	protected Color _arealAttack_Color = new Color(0.7f, 0.7f, 0.1f, 0.5f);
	
	//jks melee
	protected bool _meleeAttack_ShowAttackRange = false;
	protected Vector3 _meleeAttack_Center_Opp = new Vector3();
	protected float _meleeAttack_Weapon;
	protected float _meleeAttack_Radius_Opp;
	
	protected Color _meleeAttack_Color_Weapon = new Color(0.7f, 0.2f, 0.2f, 0.2f);
	protected Color _meleeAttack_Color_Radius = new Color(0.2f, 0.7f, 0.2f, 0.5f);


	protected void OnDrawGizmos() 
	{
		Gizmos.color = _meleeAttack_Color_Radius;
		Gizmos.DrawSphere(transform.position, Radius);		
		
		Gizmos.color = _meleeAttack_Color_Weapon;
		Gizmos.DrawSphere(transform.position, _weaponLength+Radius);	
		
		if (_meleeAttack_ShowAttackRange)
		{
			Gizmos.color = _meleeAttack_Color_Radius;
			Gizmos.DrawSphere(_meleeAttack_Center_Opp, _meleeAttack_Radius_Opp);	
			
			Invoke("resetDebugShowGizmo", 1);
		}

		if (_arealAttack_ShowDamageRange)
		{
			Gizmos.color = _arealAttack_Color;
			Gizmos.DrawSphere(_arealAttack_DamageCenter, _arealAttack_Radius);
			
			//Log.jprint(gameObject + "    QQQQQQ   damage center: "+ _arealAttack_DamageCenter);
		}
	}

	protected void resetDebugShowGizmo()
	{
		_meleeAttack_ShowAttackRange = false;
	}
#endif



//	//jks give generous distance for combo2 and combo3.
//	protected float getAttackDistanceMax()
//	{
////		if (ComboCurrent != 0)
////		{
////			return _attackDistanceMax * 1.35f;
////		}
//
//		return _attackDistanceMax;
//	}

	

	protected virtual bool amIInDamageRange()
	{
		GameObject target = getCurrentTarget();
		
		if (target == null) return false;
		
		float dist = Mathf.Abs(transform.position.x - target.transform.position.x);

		Knowledge_Mortal_Fighter targetKnow = target.GetComponent<Knowledge_Mortal_Fighter>();
		float distShell = dist - Radius - target.GetComponent<Knowledge_Mortal_Fighter>().Radius;

		float limit = targetKnow._attackDistanceMax+1.5f;

		if (distShell < limit)
		{
			//Log.jprint(gameObject +  "     distShell: "+ distShell + "  <    limit: "+ limit);
			return true;
		}

		return false;
	}


	protected virtual bool isTooCloseToFlee()
	{
		GameObject target = getCurrentTarget();
		
		if (target == null) return false;
		
		float dist = Mathf.Abs(transform.position.x - target.transform.position.x);
		
		Knowledge_Mortal_Fighter targetKnow = target.GetComponent<Knowledge_Mortal_Fighter>();
		float distShell = dist - Radius - target.GetComponent<Knowledge_Mortal_Fighter>().Radius;
		
		float limit = (targetKnow._attackDistanceMax+1.5f)*0.5f;

		//Log.jprint(gameObject +  "    isTooCloseToFlee()    distShell: "+ distShell + "  <    limit: "+ limit);

		if (distShell < limit)
		{
			//Log.jprint(gameObject + "    too close don't flee !!!!!!!");
			return true;
		}
		
		return false;
	}


	
	public virtual bool shouldIJumpBack()
	{
		return false;
	}


	public virtual bool shouldIJumpBackToKeepDistance()
	{
		return false;
	}


	protected virtual bool isTooFarFromCamera()
	{
		if (Mathf.Abs(transform.position.x - CameraManager.Instance.CameraTransform.position.x) > 5)
			return true;
		
		return false;
	}

	protected virtual bool isFarEnoughNotToJumpBack()
	{
		GameObject target = getCurrentTarget();
		
		if (target == null) return false;
		
		float dist = Mathf.Abs(transform.position.x - target.transform.position.x);
		float distShell = dist - Radius - target.GetComponent<Knowledge_Mortal_Fighter>().Radius;
		
		if (distShell >= _attackDistanceMax+1)
			return true;
		
		return false;

	}



	
	public GameObject getClosestEnemy()
	{
		//Log.jprint("> > > getClosestEnemy()   _opponentsInAttackRange: " + _opponentsInAttackRange.Count);
		//getOpponentsInScanDistance(10 ,"Fighters", _opponentsInAttackRange);
		return getOpponentsInScanDistance_WeaponPositionBased();

		//return Utility.findClosestGameObjectAmongPool(gameObject, _opponentsInAttackRange);
	}



	public void resetPosition_Y()
	{
		Vector3 resetPosition = transform.position;
		resetPosition.y = 0;
		transform.position = resetPosition;
	}


	public void cancelRepeatingInvokes()
	{
		CancelInvoke();
	}


	public virtual void forceResetFlags()
	{
//		if (gameObject.name.Contains("C"))
//			Log.jprint(Time.time +" : "+ gameObject + "        forceResetFlags()");

		_isResetActionInfoDone = true;

		_hit = false;
		_idle = false;
		_walk = false;
		_run = false;
		_walkBack = false;
		_walkFast = false;

		_paused = false;

		_hitReactionInProgress = false;

		_block = false;
		_blockInProgress = false;

		resetActionComboFlag();
		resetProgressAnimComboFlag();

		resetActionComboQuickFlag();
		resetProgressComboQuickFlag();

		Progress_CoolingJump = false;

		//jks stun should not be reset here-  _stun = false; 
		_pvpReady = false;
		_pvpJustBeforeHit = false;

		_installWeapon = false;

		if (DoIHoldOpponent)
		{
			releaseOpponent();
		}

		_coolingJump = false;
	}


		
	protected bool _canStartSkillPreview = true;

	public void validateSkillPreview()
	{
		//_canStartSkillPreview = true;
		StartCoroutine( co_delayed_SkillValidation() );
	}


	IEnumerator co_delayed_SkillValidation()
	{
		yield return new WaitForSeconds(0.5f);
		
		_canStartSkillPreview = true;
	}


	public override void resetActionInfo()
	{
		forceResetFlags();
		Action_Idle = true; //jks default
		//Action_Walk = true; //jks default

		
		if (IsDead)
		{
			_deathStart = true;
		}
		else
		{
			//jks after victory action, goto postVictory for "idle" animation
			if ((IsBattleVictorious && AllyID == eAllyID.Ally_Human_Me) ||
			    (IsBattleFailed && AllyID != eAllyID.Ally_Human_Me))
			{
				Action_PostVictory = true;
			}

			doNextAction();
		}

		//jsm_0912 - hide trail
		setTrail(false);

	}

	protected virtual void doNextAction()
	{
	}


	protected virtual void setActionFlag_Preview()
	{
		     if (ComboCurrent == 1 && TotalCombo > 1) { Action_Combo2 = true; }
		else if (ComboCurrent == 2 && TotalCombo > 2) { Action_Combo3 = true; }
		else if (ComboCurrent == 3 && TotalCombo > 3) { Action_Combo4 = true; }		
		else if (ComboCurrent == 4 && TotalCombo > 4) { Action_Combo5 = true; }
		else if (ComboCurrent == 5 && TotalCombo > 5) { Action_Combo6 = true; }
		else if (ComboCurrent == 6 && TotalCombo > 6) { Action_Combo7 = true; }
		else if (ComboCurrent == 7 && TotalCombo > 7) { Action_Combo8 = true; }
	}


	protected virtual void setNextComboActionFlag()
	{
		     if (ComboCurrent == 1  && TotalCombo > 1 ) { if (getOpponentsToAttack()) Action_Combo2  = true; }
		else if (ComboCurrent == 2  && TotalCombo > 2 ) { if (getOpponentsToAttack()) Action_Combo3  = true; else setZoom(0); } //jsm - reset zoom value 
		else if (ComboCurrent == 3  && TotalCombo > 3 ) { if (getOpponentsToAttack()) Action_Combo4  = true; else setZoom(0); }
		else if (ComboCurrent == 4  && TotalCombo > 4 ) { if (getOpponentsToAttack()) Action_Combo5  = true; else setZoom(0); }
		else if (ComboCurrent == 5  && TotalCombo > 5 ) { if (getOpponentsToAttack()) Action_Combo6  = true; else setZoom(0); }
		else if (ComboCurrent == 6  && TotalCombo > 6 ) { if (getOpponentsToAttack()) Action_Combo7  = true; else setZoom(0); }
		else if (ComboCurrent == 7  && TotalCombo > 7 ) { if (getOpponentsToAttack()) Action_Combo8  = true; else setZoom(0); }
	}



	public virtual IEnumerator startSkill()
	{
		//finishArming(); //jks stop looking inventory position

//		Log.jprint(gameObject + "  0 0 0 0     startSkill initiated   > > > > > > > > >    "  + Time.time);
		if (Progress_SkillAnimation) yield break;
		if (Action_Hit) yield break;//jks if (_hitReactionInProgress) return;

//		if (IsLeader)
//			Log.jprint(Time.time + "           startSkill()         " + gameObject);

		Action_Combo1 = true;
		//Log.jprint(gameObject + "  Action_Combo1 = true;     startSkill initiated   > > > > > > > > >    "  + Time.time);
	}


	public void doNotStartSkill()
	{
		Action_Combo1 = false;
		//jks let combo2 play even i dont see enemy right now.			_combo2 = false;  
		//jks let combo3 play even i dont see enemy right now.			_combo3 = false;
	}

	
	protected virtual void checkEnemy()
	{
		bool found = checkEnemyOnPath(false);

		if (!found)
		{
			doNotStartSkill();
		}
	}


//	public override float getReactionDistance()
//	{
//		//Log.jprint("reactionDistance : " + _reactionDistance[(int)_hitType]);
//
////		if (HitType == eHitType.HT_CRITICAL)
////		{
////			return 0;  //jks critical reaction using root bone movement, so we do not need to push character manually.
////		}
//		    
//		return _reactionDistance[(int)_hitType];
//	}



	public virtual int getReactionAnimID(int combo, eHitType hitType)
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

		Log.nprint(gameObject.name + "    combo-1: "+ (combo-1) + "   reaction choice: "+ reactionChoice + "     reaciton anim #: "+ _anim_reaction[combo-1,reactionChoice]);

		return _anim_reaction[combo-1,reactionChoice]; //jks index 0 == combo1, index 1 == combo2, index 2 == combo3
	}

	

//	public virtual eHitType getFinalHitType()
//	{
//		eHitType hitType = judgeHitState(_criticalRate);
//
//		if(TotalCombo > 2 && ComboCurrent == TotalCombo)
//		{
//			hitType = eHitType.HT_CRITICAL;
//		}
//
//		return hitType;
//	}


//	public virtual eHitType getFinalHitType(CardClass opponentClass)
//	{
//		return eHitType.HT_BAD;
//	}


	#region card class relation
	
	public int calculate_ClassRelation_AttackPoint(int originalAttack, Knowledge_Mortal_Fighter opponent)
	{
		ObscuredFloat rate = CharacterClassRelation.getAttackRate(_class, opponent.Class) * 0.01f ;

		return Mathf.RoundToInt(originalAttack * rate); 
	}

	//jks 2015.8.26 game design change.
//	public float calculate_ClassRelation_HitRate(Knowledge_Mortal_Fighter opponent)
//	{
//		float rate = CharacterClassRelation.getCriticalRate(_class, opponent.Class, _attackType, opponent._attackType) * 0.01f; 
//		
//		return rate; 
//	}	
	public int calculate_ClassRelation_DefensePoint(int originalDefense, Knowledge_Mortal_Fighter opponent)
	{
		ObscuredFloat rate = CharacterClassRelation.getDefenseRate(_class, opponent.Class) * 0.01f; 

		return Mathf.RoundToInt(originalDefense * rate); 
	}	

	#endregion
	

//	#region Leader Buff
//	
//	public virtual int calculate_LeaderBuff_AttackPoint_Self(CardClass enemyClass)
//	{
//		return 0;
//	}
//	
//	public virtual float calculate_LeaderBuff_HitRate_Self(CardClass enemyClass)
//	{
//		return 0;
//	}
//
////	public virtual int calculate_LeaderBuff_AttackPoint_Opponent()
//	{
//		float rate = CharacterClassRelation.getAttackRate_Buff_Opponent(BattleBase.Instance.LeaderClass, Class) * 0.01f;
//		
//		return Mathf.RoundToInt(_attackPoint * rate);
//	}
	
//	public virtual float calculate_LeaderBuff_HitRate_Opponent()
//	{
//		float rate = CharacterClassRelation.getCriticalRate_Buff_Opponent(BattleBase.Instance.LeaderClass, Class) * 0.01f;
//		
//		return rate;
//	}
//
//	#endregion

	//jks 2015.5.8 remove leader strategy-	
//	#region Leader Strategy
//
//	public virtual int calculate_LeaderStrategy_AttackPoint()
//	{
//		return 0;
//	}
//	
//	public virtual float calculate_LeaderStrategy_HitRate()
//	{
//		return 0;
//	}
//	
//	#endregion


	protected string _saved_tag;
	protected Vector3 _saved_scale;
	protected Quaternion _saved_rotation;
	protected GameObject _captured_target = null;
	public bool DoIHoldOpponent
	{
		get {return (_captured_target != null);}
	}
	
	protected bool _amICaptured = false;
	public bool AmICaptured
	{
		set { _amICaptured = value;}
		get 
		{
			if (_amICaptured && transform.localPosition.x != 0)
				transform.localPosition = Vector3.zero;
			
			return _amICaptured;
		}
	}
	
	
	public override void holdOpponent()
	{
		if (_target_attack == null) return;
		
		if (DoIHoldOpponent) return;
		
		Knowledge_Mortal_Fighter knowledgeCapturedTarget = _target_attack.GetComponent<Knowledge_Mortal_Fighter>();
		if (knowledgeCapturedTarget.DoIHoldOpponent) return;
		if (knowledgeCapturedTarget.Height > 3.5f || knowledgeCapturedTarget.Radius > 3.5f) return; //jks too big to capture
		
		_captured_target = _target_attack;
		_saved_scale = _captured_target.transform.localScale;
		_saved_rotation = _captured_target.transform.rotation;
		_saved_tag = _captured_target.tag;
		_captured_target.tag = "Untagged";
		
		knowledgeCapturedTarget.Action_Captured = true;
		knowledgeCapturedTarget.AmICaptured = true;
		
		Utility.attachGameObjectToTransform(_captured_target, _weaponEnd);
		
	}
	
	public override void releaseOpponent()
	{
		if (! DoIHoldOpponent) return;
		
		_captured_target.tag = _saved_tag;
		_captured_target.transform.parent = transform.root.parent;
		_captured_target.transform.localScale = _saved_scale;
		_captured_target.transform.rotation = _saved_rotation;
		
		Vector3 newPos = new Vector3(_captured_target.transform.position.x,0,0);
		_captured_target.transform.position = newPos;
		
		Knowledge_Mortal_Fighter knowledgeOpponent = _captured_target.GetComponent<Knowledge_Mortal_Fighter>();
		knowledgeOpponent.Action_Captured = false; 
		knowledgeOpponent.Action_Hit = true; 
		knowledgeOpponent.AmICaptured = false;
		
		_captured_target = null;
		
	}


	protected void goHitReaction()
	{
		//jks 피격 리액션 하기.
		forceResetFlags();
		Action_Hit = true;


		//jks 맞을 당시 공격 중이었다면, 쿨타임 처리. 
		if (_everGaveDamageForTheAttack)
		{
			//Log.jprint(gameObject + " C O O L T I M E  S T A R T ........... ");				
			startCoolTimeAndResetComboFlag();
			_everGaveDamageForTheAttack = false;
		}
		else
		{
			if (AttackCoolTimer != null && !AttackCoolTimer.IsCoolingInProgress)
				AttackCoolTimer.reset();
		}

	}


	public virtual float calcHitRate(Knowledge_Mortal_Fighter opponent)
	{
		ObscuredFloat hitRate;
//jks 2015.8.26 game design change.		float classRelationHitRate = calculate_ClassRelation_HitRate(opponent);
		hitRate = _criticalRate;//jks 2015.8.26 game design change. + classRelationHitRate;

		#if UNITY_EDITOR
		if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
		{
			//Log.print("공격자 : " + gameObject +  " 적중률: "+_criticalRate +  " +  클래스상성: "+  classRelationHitRate  + " = " + "  중간계산 적중률: " + hitRate);
			Log.print_always("공격자 : " + gameObject.name +  " 적중률: "+_criticalRate  + " = " + "  중간계산 적중률: " + hitRate);
		}
		#endif


		//jks 2016.3.14 : 
		#if UNITY_EDITOR
		float originalHitRate = hitRate;
		#endif

		//jks 적중율 스킬 버프 적용.
		float skillBuff_HitRate = BattleBase.Instance.getSkillBuff_CriticalUp(AllyID) - BattleBase.Instance.getSkillBuff_CriticalDown(AllyID, opponent);
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


	public virtual eHitType getFinalHitType(Knowledge_Mortal_Fighter opponent)
	{
		ObscuredFloat finalHitRate;

		finalHitRate = calcHitRate(opponent);

		if (finalHitRate < 0) finalHitRate = 0;
		
		eHitType hitType = judgeHitState(finalHitRate);

		//jks 2014.9.27  크리티컬은 스킬의 막지막 타에만 가능 하도록 수정. 
		if (hitType == eHitType.HT_CRITICAL)
		{
//2016.5.25			
//			if (! _skillCompleted)
//			{
//				hitType = eHitType.HT_GOOD;
//			}

			if ((IsBoss || (IsLeader && opponent.AllyID == eAllyID.Ally_Human_Opponent)) // 보스거나, 랭킹탑 적 리더 일 경우,
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
		}

//		if(TotalCombo > 2 && ComboCurrent == TotalCombo)
//		{
//			hitType = eHitType.HT_CRITICAL;
//		}

		return hitType;
	}



	public virtual int getReaction(eHitType hitType)
	{
		int hitReactionAnimID;

		if (hitType == eHitType.HT_CRITICAL)
		{
			hitReactionAnimID = getReactionAnimID(TotalCombo, hitType);
		}
		else 
		{
			hitReactionAnimID = getReactionAnimID(ComboRecent, hitType); 
		}

		return hitReactionAnimID;
	}


	public int getAttackPoint()
	{
		return AttackPoint;
	}


	protected virtual float GetRandomNumberForHitType()
	{
		return Random.Range(0.0f, 1.0f);
	}

	
	public virtual eHitType judgeHitState(float criticalHitRate)
	{
		ObscuredFloat sharedRate = (1 - criticalHitRate) * 0.25f;

		float randomNumber = GetRandomNumberForHitType();

		ObscuredFloat rate = criticalHitRate;

		//Log.print(gameObject + "          random number: " + randomNumber);
		#if UNITY_EDITOR
		if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
		{
			Log.print_always("공격자 : " + gameObject.name +  "CRITICAL 적중률: "+criticalHitRate +  " >  randomNumber: "+  randomNumber);
		}
		#endif

		if (randomNumber <= rate)
		{
			return eHitType.HT_CRITICAL;
		}
		else if (randomNumber < (rate += sharedRate))
		{
			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
			{
				Log.print_always("공격자 : " + gameObject.name +  " GOOD check 적중률: "+rate +  " >  randomNumber: "+  randomNumber);
			}
			#endif
			return eHitType.HT_GOOD;
		}
		else if (randomNumber < (rate += sharedRate))
		{
			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
			{
				Log.print_always("공격자 : " + gameObject.name +  " NICE check 적중률: "+rate +  " >  randomNumber: "+  randomNumber);
			}
			#endif
			return eHitType.HT_NICE;
		}
		else if (randomNumber < (rate += sharedRate))
		{
			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
			{
				Log.print_always("공격자 : " + gameObject.name +  " MISS check 적중률: "+rate +  " >  randomNumber: "+  randomNumber);
			}
			#endif
			return eHitType.HT_MISS;
		}
		else
		{
			return eHitType.HT_BAD;
		}
	}


	public override void updateHP(ObscuredInt delta, eHitType hitType)
	{
		ObscuredInt damagePoint = delta;

		//jks 2016.4.25 랭킹탑 전투 공격 배가치 적용.
		if (BattleBase.Instance.IsRankingTower)
		{
			damagePoint = (ObscuredInt)(delta * BattleBase.Instance.CurrentRankingTowerAttackBoost);

			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
			{
				Log.print_always("공격자 : " + gameObject.name +  " 랭킹탑 공격 배가치: " 
					+ BattleBase.Instance.CurrentRankingTowerAttackBoost +  " x  damage: " +delta + "  = " 
					+ (int)damagePoint);
			}
			#endif
					
		}
		else if (BattleBase.Instance.IsBossRaid && IsRaidBoss)  //jks 레이스보스 전에서 보스경우 시간이 지남에 따라 공격력 배가됨.
		{
			damagePoint = (ObscuredInt)(delta * BattleBase.Instance.CurrentRaidBossAttackBoost);

			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
			{
				Log.print_always("공격자 : " + gameObject.name +  " 레이드 시간 공격 배가치: " 
					+ BattleBase.Instance.CurrentRaidBossAttackBoost +  " x  damage: " +delta + "  = " 
					+ (int)damagePoint);
			}
			#endif
		}

		//jks 최종 HP 차이 수치 계산. 
		base.updateHP(damagePoint, hitType);


		#if UNITY_EDITOR
		if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
		{
			Log.print_always("   T A K E   D A M A G E  updateDamageUI().....  damagePoint: "+ damagePoint + "   hitType: " + hitType  + ": current su : " + _cur_hp);
		}
		#endif

		if (IsRaidBoss)
		{
			((BattleManager_BossRaid)BattleBase.Instance).TotalDamagesToRaidBoss = Max_HP - Current_HP; 
		}


		updateDamageUI(transform, damagePoint, hitType);

		shaderEffect_Hit_On();



		if (_cur_hp == 0)
		{
			//Log.jprint(gameObject + " is dead ! ! ! ! ! !.");

			//jks 죽지 않게 연출해야하는 보스 경우처리.
			if (IsBoss && BattleBase.Instance.isThisVictoryCondition(eBattleVictoryConditionIndex.BVC_BossHPReduction))
			{
				Action_DeathB = true;
				startSlowMotion(BattleTuning.Instance._slowMotionBossNPercentDamageTimeScale,
										  BattleTuning.Instance._slowMotionBossNPercentDamageDuration);
				
			}
			else
			{
				//jks slow motion effect: if boss' death
				if (IsBoss)
				{
					startSlowMotion(BattleTuning.Instance._slowMotionBossDeathTimeScale,
						BattleTuning.Instance._slowMotionBossDeathDuration);
				}

				Action_Death = true;

			}

//jks 2016.6.14 code moved to Behavior_Mortal_Fighter.cs to fix Mantis 1572 (피격애니매이션 중 공중에서 떨어질 때, 턱에 걸린 것 처럼 걸리면서 바닥으로 떨어짐.)
//			if (Action_Captured_InTheAir) //jks 공중스턴 당하다가 죽으면 지면으로 내림.
//				resetActionCapturedInTheAir();

		}
	}




	public void startSlowMotion(float timeScale, float duration)
	{
		//jks 거리 도달 미션은 슬로우 연출 안함.
		if (BattleBase.Instance.isThisVictoryCondition(eBattleVictoryConditionIndex.BVC_Distance))
			return;

		iTween.Stop(gameObject); //jks slow time 으로 들어가기 때문에 hit shake 중이면 중지 시킴.

		Time.timeScale = timeScale;

		Log.jprint(Time.time+" x x x x x x  startSlowMotion()  Time.timeScale = "+Time.timeScale);

		Invoke("resetTimeScale", duration);

		if (BattleUI.Instance() != null)
			BattleUI.Instance().gob_leaderHud.SetActive(false);
	}

	protected void resetTimeScale()
	{
		Time.timeScale = 1;
		Log.jprint(Time.time+"  o o o o o o   resetTimeScale()  Time.timeScale = "+Time.timeScale);
	}



	Transform _weaponEnd = null;
	GameObject _curWeapon = null;

	public GameObject Weapon { get { return _curWeapon;} set { _curWeapon = value;}}

	public Vector3 WeaponEndPosition
	{
		get
		{
			Vector3 endPosition;
			endPosition = transform.position;

			if (_right_foot == null || _left_foot == null || _right_hand == null || _left_hand == null)
				findWeaponBones();

			//jks if no weapon, dynamically get position of hand or foot bone.
			if (_curWeapon == null)
			{
				if (transform.forward.x > 0)
				{
					endPosition.x = Mathf.Max(Mathf.Max(_right_hand.position.x, _left_hand.position.x), Mathf.Max(_right_foot.position.x, _left_foot.position.x));
					endPosition.x = Mathf.Max(endPosition.x, (transform.position.x + Radius));
					//endPosition.x += Radius;
				}
				else
				{
					endPosition.x = Mathf.Min(Mathf.Min(_right_hand.position.x, _left_hand.position.x), Mathf.Min(_right_foot.position.x, _left_foot.position.x));
					endPosition.x = Mathf.Min(endPosition.x, (transform.position.x - Radius));
					//endPosition.x -= Radius;
				}

				endPosition.y = Mathf.Max(Mathf.Max(_right_hand.position.y, _left_hand.position.y), Mathf.Max(_right_foot.position.y, _left_foot.position.y));

				return endPosition;
			}

			//jks if we have weapon, get from tip bone postion and compare to body front
			if (_weaponEnd == null)
			{

				_weaponEnd = Utility.findTransformUsingKeyword(_curWeapon, "tip"); //jks if we have weapon, find tip.
				if (_weaponEnd == null)
				{
					Log.Warning(" Weapon '"+ _curWeapon + "' does not have a 'tip' child object.");
					_weaponEnd = Utility.findTransformUsingKeyword(gameObject, "bone_weapon_r"); //jks if no tip, use weapon bone for now.
				}
			}

			if (transform.forward.x > 0)
			{
				endPosition.x = Mathf.Max(_weaponEnd.position.x, (transform.position.x + Radius));
			}
			else
			{
				endPosition.x = Mathf.Min(_weaponEnd.position.x, (transform.position.x - Radius));
			}

			endPosition.y = Mathf.Max(Mathf.Max(_weaponEnd.position.y, transform.position.y + Height));

			return endPosition;
		}
	}


	public virtual bool checkHeightIfReachable(GameObject opponent)
	{
		//Knowledge_Mortal_Fighter knowledge = opponent.GetComponent<Knowledge_Mortal_Fighter>();

		//jks - even the player and the enemy are the same Ground type, can avoid attack if jumped high enough.          
		// if (knowledge.amIAirBorne() == amIAirBorne()) return true;

		Transform trfm = Utility.findTransformUsingKeyword(opponent, "Toe0Nub"); //lowest bone, i guess
		if (trfm == null)
		{
			trfm = Utility.findTransformUsingKeyword(opponent, "bone_weapon");
			if (trfm == null)
			{
				Log.Warning(gameObject + "No bone to find low position.");
				trfm = gameObject.transform;
			}
		}

		return WeaponEndPosition.y >= trfm.position.y;
	}


	//jks check if myHeight(y value) with volume(radius) is between fighters' toe and head. (used to check if projectile hit this fighter)
	public virtual bool checkHeightIfReachable(float myHeight, float radius)
	{
		//Knowledge_Mortal_Fighter knowledge = opponent.GetComponent<Knowledge_Mortal_Fighter>();
		
		//jks - even the player and the enemy are the same Ground type, can avoid attack if jumped high enough.          
		// if (knowledge.amIAirBorne() == amIAirBorne()) return true;

		float lowest = 0;
		float highest = 0;// opponent's

		Transform trfm = Utility.findTransformUsingKeyword(gameObject, "Toe"); //lowest bone, i guess
		if (trfm != null)
		{
			lowest = trfm.position.y;
			highest = lowest + gameObject.GetComponent<Knowledge_Mortal_Fighter>().Height;
		}
		else
		{
			trfm = Utility.findTransformUsingKeyword(gameObject, "spine");
			if (trfm == null)
			{
				Log.Warning(gameObject + "No bone to find low position.");
				trfm = gameObject.transform;
				lowest = trfm.position.y;
				highest = lowest + gameObject.GetComponent<Knowledge_Mortal_Fighter>().Height;
			}
			else
			{
				float halfHeight = gameObject.GetComponent<Knowledge_Mortal_Fighter>().Height * 0.5f;
				lowest = trfm.position.y - halfHeight;
				highest = trfm.position.y + halfHeight;
			}
		}
		
		return ((lowest < myHeight + radius) && (myHeight - radius < highest));
	}



	public virtual bool isLastDamageInSkill(float reactionDistanceOverride)
	{
		if (reactionDistanceOverride >= 1000)  	//jks  if last damage in combo
		{
			//jks 2015.4.27 :  if (TotalCombo == ComboCurrent) //jks  if last combo in skill
			//jks 2015.4.27 : 콤보 끝에 블랜딩 애니메이션이 들어가거나 뒤로 점프하는 애니메이션을 넣어 사용하는 경우로 인해 실제 데미지를 주는 마지막 공격인지 판단을 위해 추가된 LastCombo를  사용..
			if (LastCombo == ComboCurrent) //jks  if last combo in skill
				return true;
		}
		return false;
	}


	public bool isLastDamageInSkill(float reactionDistanceOverride, GameObject attacker)
	{
		if (reactionDistanceOverride >= 1000)  	//jks  if last damage in combo
		{
			Knowledge_Mortal_Fighter attackerKnowledge = attacker.GetComponent<Knowledge_Mortal_Fighter>();
			//jks if (attackerKnowledge.TotalCombo == attackerKnowledge.ComboCurrent) //jks  if last combo in skill
			//jks 2015.4.27 : 콤보 끝에 블랜딩 애니메이션이 들어가거나 뒤로 점프하는 애니메이션을 넣어 사용하는 경우로 인해 실제 데미지를 주는 마지막 공격인지 판단을 위해 추가된 LastCombo를  사용..
			if (attackerKnowledge.LastCombo == attackerKnowledge.ComboCurrent) //jks  if last combo in skill
				return true;
		}
		return false;
	}

	protected Renderer[] _renderer = null;
	protected void shaderEffect_Hit_On()
	{
		if (_renderer == null)
		{
			_renderer = GetComponentsInChildren<Renderer>();
		}
		if (_renderer == null) return;

		CancelInvoke("shaderEffect_Hit_Off");
		for(int k=0; k < _renderer.Length; k++)
		{
			if (_renderer[k] == null) continue;

			for(int j=0; j < _renderer[k].materials.Length; j++)
				_renderer[k].materials[j].SetFloat("_HitFX", 8);
		}

		Invoke("shaderEffect_Hit_Off", 0.15f);
	}

	public void shaderEffect_Hit_Off()
	{
		if (_renderer == null) return;

		for(int k=0; k < _renderer.Length; k++)
		{
			if (_renderer[k] == null) continue;

			//_renderer[k].material.SetFloat("_HitFX", 1); //jks reset

			for(int j=0; j < _renderer[k].materials.Length; j++)
				_renderer[k].materials[j].SetFloat("_HitFX", 1);
		}
	}

	//리더가 크리티컬 연타 맞으면 방어력 떨어지게 함.
	protected ObscuredInt _accumulated_critical_hit_count = 0;

	public override ObscuredInt takeDamage(ObscuredInt damagePoint, int reactionAnimID, eHitType hitType, eAttackType attackType, eWeaponType_ForAnimation weaponType, GameObject attacker,  float reactionDistanceOverride)
	{
		//Log.jprint(gameObject + ": takeDamage(); hitType : " + hitType);
		//Log.jprint(gameObject + ": takeDamage(); hitType : " + hitType + "    reactionAnimID : " + reactionAnimID);

		if (AllyID == eAllyID.Ally_Human_Me && BattleBase.Instance.IsInvincibleMode)
		{
			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
				Log.print_always(gameObject.name +" -------  팀원 무적 상태 ---------- damage : 0  ");
			#endif

			return 0;
		}

		if (damagePoint <= 0) 
		{
			Debug.LogError("damagePoint: " + damagePoint);
			return damagePoint;
		}

		Attacker = attacker;


		IsLastDamageInSkill = isLastDamageInSkill(reactionDistanceOverride, attacker); //jks reset	

//jks 2015.4.13 -		if (!IsLastDamageInSkill && hitType != eHitType.HT_CRITICAL) //jks 스킬 마지막 타이고 크리티컬이면 뒤로 밀리는 부분 생략.- (크리티컬 리액션은 root motion 사용하기 때문에 따로 미는 부분 생략.) 	
			applyReactionDistance(reactionDistanceOverride);


		//리더가 크리티컬 연타 맞으면 방어력 떨어지게 함.
		if (IsLeader)
		{
			if (hitType == eHitType.HT_CRITICAL)
			{
				if (_accumulated_critical_hit_count > 0 && _recentHitType == eHitType.HT_CRITICAL)
					_accumulated_critical_hit_count++;
				else
					_accumulated_critical_hit_count = 1;
			}
			else
			{
				_accumulated_critical_hit_count = 0;
			}
		}
		else
		{
			_accumulated_critical_hit_count = 0;
		}


		Knowledge_Mortal_Fighter knowledgeOpponent = attacker.GetComponent<Knowledge_Mortal_Fighter>();

		AppliedSkillBuff_UID = knowledgeOpponent.MyCurrentSkillBuff_UID;
			
		ObscuredInt defense = getDistributedDefense(knowledgeOpponent.DamageFrequency); // Defense;


		//jks 특정적 상대 시에 override base defense rate.
		if (_count_special_opponent > 0)
		{
			float special_defenseRate = getDefensdeRate_SpecialOpponent();
			if (special_defenseRate != float.MinValue)
			{
				defense = Mathf.RoundToInt(defense * special_defenseRate);
			}
		}



		//클래스 상성 적용 방어.
		if (IsLeader || knowledgeOpponent.IsLeader || TestOption.Instance()._classRelationBuffAll)
		{
			ObscuredInt classRelationDefensePoint = calculate_ClassRelation_DefensePoint(defense, knowledgeOpponent);

			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
				Log.print_always("  기본 방어:  "+ defense + "  + 상성 추가치: " + classRelationDefensePoint + "   new 방어력: " + (defense + classRelationDefensePoint));
			#endif

			defense = defense + classRelationDefensePoint;

		}


		int leaderBuffDefensePoint = getLeaderBuffDefenseUp(defense);
		if (leaderBuffDefensePoint > 0)
		{
			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
				Log.print_always("  기본 방어:  "+ defense + "  + 리더 버프 추가치: " + leaderBuffDefensePoint + "   new 방어력: " + (defense+leaderBuffDefensePoint));
			#endif

			defense += leaderBuffDefensePoint;
		}



		int leaderBuffDefenseDown = getLeaderBuffDefenseDown(defense);
		if (leaderBuffDefenseDown > 0)
		{
			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
				Log.print_always("  기본 방어:  "+ defense + "  + 리더 버프 감소치: " + leaderBuffDefenseDown + "   new 방어력: " + (defense-leaderBuffDefenseDown));
			#endif

			defense -= leaderBuffDefenseDown;
		}



		//리더가 크리티컬 연타 맞으면 방어력 떨어지게 함.
		if (IsLeader)
		{
			if (_accumulated_critical_hit_count > 2)
				defense = Mathf.RoundToInt(defense * 0.5f);
			else if (_accumulated_critical_hit_count > 1)
				defense = Mathf.RoundToInt(defense * 0.7f);

			#if UNITY_EDITOR
			if (_accumulated_critical_hit_count > 1 && TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
				Log.print_always("   리더가 크리티컬 연타 맞으면 방어력 떨어진 new 방어력:  " + defense);
			#endif
		}

		// 리더가 막기 할 경우 방어력 향상 적용.
		if (IsLeader)
		{
			defense = applyGuardUpDefenseBoost(defense, knowledgeOpponent.Class);
			hitShake();
		}

		_recentHitType = hitType;
		
		_animHitNumber = reactionAnimID;
		
		ObscuredInt finalDamage = getAttackPointByHitType(hitType, damagePoint); 
		ObscuredInt savedHitTypeAppliedDamage = finalDamage;

		#if UNITY_EDITOR
		if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
			Log.print_always("   T A K E   D A M A G E  -      피해자: " + gameObject.name + "  공격: " + finalDamage + " -  방어: " + defense  + "   =  최종 Damage : "+ (finalDamage-defense) );
		#endif


		//jks 2016.3.14 스킬 버프.
		if (BattleBase.Instance != null)
		{
			#if UNITY_EDITOR
			int originalDefense = defense;
			int originalDamage = finalDamage;
			#endif
			//jks 방어력 스킬 버프 적용.
			float buffRate_defense = BattleBase.Instance.getSkillBuff_DefenseUp(AllyID) - BattleBase.Instance.getSkillBuff_DefenseDown(AllyID, knowledgeOpponent);
			defense += Mathf.RoundToInt(defense * buffRate_defense);

			//jks 공격력 스킬 버프 적용.
			float buffRate_attack = BattleBase.Instance.getSkillBuff_AttackUp(AllyID) - BattleBase.Instance.getSkillBuff_AttackDown(AllyID, knowledgeOpponent);
			finalDamage += Mathf.RoundToInt(finalDamage * buffRate_attack);

			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
			{
				Log.print_always("   스킬버프 방어력 증감 적용       적용전: " + originalDefense + "      증감치: "+ (buffRate_defense * 100) + " %      적용후: "+ defense );
				Log.print_always("   스킬버프 공격력 증감 적용       적용전: " + originalDamage + "      증감치: "+ (buffRate_attack * 100) + " %      적용후: "+ finalDamage );
			}
			#endif
		}

		finalDamage -= defense;
		
		finalDamage = doMinimumDamageAdjustment(finalDamage, savedHitTypeAppliedDamage);

		//jks 2016.3.9 - 지금 데미지를 받고 있는 캐릭터가 스킬 공격 중일 경우, 피해 정도치 적용.
		if (Progress_SkillAction)
		{
			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
				Log.print_always(" 피해자도 공격 중인 스킬 속성으로 받는 데미지 보정:    -      피해자: " + gameObject.name + "  공격: " + finalDamage + " 에  " + DamagePenetration  + " % 만 적용  =  최종 Damage : "+ (Mathf.RoundToInt(finalDamage * DamagePenetration)) );
			#endif

			finalDamage = Mathf.RoundToInt(finalDamage * DamagePenetration);
		}
		else if (KnowledgeMultiSkill != null && KnowledgeMultiSkill.Progress_AnyAction)
		{
			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
				Log.print_always(" 피해자도 공격 중인 패턴/지원 스킬 속성으로 받는 데미지 보정:    -      피해자: " + gameObject.name + "  공격: " + finalDamage + " 에  " + DamagePenetration  + " % 만 적용  =  최종 Damage : "+ (Mathf.RoundToInt(finalDamage * KnowledgeMultiSkill.DamagePenetration)) );
			#endif
			finalDamage = Mathf.RoundToInt(finalDamage * KnowledgeMultiSkill.DamagePenetration);
		}


		updateHP(-finalDamage, hitType);



		//Log.jprint(gameObject + ": takeDamage(); finalDamage : " + finalDamage);
		//Log.jprint(gameObject + ": takeDamage(); current su : " + _cur_su);

		//_hit = true;

		//jsm - set damage fx (sound, particle)
//		eAttackType attackType = attackerKnow._attackType;
//		eWeaponType_ForAnimation weaponType = attackerKnow._weaponType_ForAnimation;

		if (ProjectileObject != null)
		{
			spawnDamageEffect( ProjectileObject, attackType, _recentHitType, weaponType );
		}
		else 
			spawnDamageEffect( attacker, attackType, _recentHitType, weaponType );
//		Debug.Log("owner : " + gameObject.name + " / type : " +  _hitType + " / attacker : " + attacker.name + " / attackType : " + attackType);

		//jsm_0319
		ProjectileObject = null;

//		//jsm_150831
//		if (_healthBar != null)
//		{
//			_healthBar.setHealth( (float) Current_HP / (float) Max_HP );
//			if (_healthBar.gameObject.activeSelf)
//				_healthBar.setHealth(Max_HP, Current_HP, finalDamage, _recentHitType);
//		}

		return finalDamage;
	}

	/// <summary>
	/// Hits the shake.- 피격되었지만 리액션하지 않는 조건상에 있을 때 흔들어 주기 위함.
	/// </summary>
	protected virtual void hitShake()
	{
		if (IsDead) return;

		if (Time.timeScale < 1) return ;

		if (IsBattleEnd) return;

		if (Action_Death || Action_DeathB)
		{
			iTween.Stop(gameObject);
			return;
		}

		//Log.jprint(gameObject +" *** *** *** *** hitShake() ...  _cur_hp : " + _cur_hp);

		iTween.ShakePosition(gameObject, iTween.Hash("x", BattleTuning.Instance._hitShake_range, 
													 "time", BattleTuning.Instance._hitShake_time, 
													 "easetype", BattleTuning.Instance._hitShake_easytype,
													 "ignoreTimeScale", false));
	}

	protected virtual int applyGuardUpDefenseBoost(int originalDefense, CardClass opponentClass)
	{
		return originalDefense; 
	}

	protected void applyReactionDistance(float reactionDistanceOverride)
	{
		if (reactionDistanceOverride == 0) return;
		if (AmICaptured) return;

	
		Vector3 forceDirection = -transform.forward;

		int dist = (int)reactionDistanceOverride;

		if (dist >= 1000) //jks bigger than 1000 means it's last skill signal, do not need here, so remove it.
		{
			dist -= 1000; // if greater than 1000,  dist-1000 is the actual distance to push.
		}

		if (dist > 9) //jks if it has projectile bone index in tens number.  
		{
			dist %= 10; //jks extract ones number only.
		}

		//Log.jprint(gameObject.name + "    reaction dist: "+ dist+ "     reactionDistanceOverride: "+ reactionDistanceOverride);
		Loco.pushObject(dist, forceDirection, 0.15f);	

	}



	public virtual void takeStun(int duration, int stunAnimNum)
	{
		Action_Stun = true;
		Anim_Stun = animStunReaction(stunAnimNum);
		CancelInvoke("stun_finishied"); //jks if previous stun is not finished, skip previous invoke and start new invoke to continue stun.
		Invoke("stun_finishied", duration);
		//Log.jprint(Time.time + "  :  " + gameObject + " + + + + + stun [duration]="+duration);
	}
	
	public virtual void takeStun(float duration, int stunAnimNum)
	{
		Action_Stun = true;
		Anim_Stun = animStunReaction(stunAnimNum);
		CancelInvoke("stun_finishied"); //jks if previous stun is not finished, skip previous invoke and start new invoke to continue stun.
		Invoke("stun_finishied", duration);
		//Log.jprint(Time.time + "  :  " + gameObject + " + + + + + stun [duration]="+duration);
	}
	
	void stun_finishied()
	{
//		forceResetFlags();

		// cancel previous skill
		if (KnowledgeMultiSkill != null)
			KnowledgeMultiSkill.forceResetComboFlags();  //jks stun 된 이후에 움직이지 않는 현상 수정.

		Action_Run = true;  //jks stun 된 이후에 움직이지 않는 현상 수정.


		Action_Stun = false;
		//Log.jprint(Time.time + "  :  " + gameObject + " - - - - - stun ");
	}
	
	protected virtual void updateDamageUI(Transform tran, ObscuredInt dmg, eHitType type)
	{
		if (_healthBar != null)
		{
			_healthBar.setHealth( (float) Current_HP / (float) Max_HP );
			if (_healthBar.gameObject.activeSelf)
				_healthBar.setHealth(Max_HP, Current_HP, dmg, type);
		}

		BattleBase.Instance.updateTotalHPForDisplay();

		if (BattleUI.Instance() != null)
			BattleUI.Instance().updateTotalHP();

		if (BattleUI.Instance() != null)
			BattleUI.Instance().updatePlayer2Card_gaugeHP();
	}


	public virtual void startAim()
	{
		updateProjectileTarget();

		if (!AmIHumanoid) return;

		AnimationOverride ao = GetComponent<AnimationOverride>();
		
		if (ao)
		{
			if (_projectileOriginalTarget)
			{
				//Log.jprint("startAim()");
				ao.aimBegin(getProjectileTargetPosition());
			}
		}
	}
	
	
	public void endAim()
	{
		AnimationOverride ao = GetComponent<AnimationOverride>();
		
		if (ao)
		{
			ao.aimEnd();
		}
	}
	
	public Vector3 getProjectileTargetPosition()
	{		
		return getProjectileTargetTransform().position;
	}


	public Transform getProjectileTargetTransform()
	{		
		if (_projectileOriginalTarget == null) return null;


		Transform spine = Utility.findTransformUsingKeyword(_projectileOriginalTarget, "spine1");
		
		if (spine) //jks humanoid
		{
			return spine;
		}
		
		//bone search priority    //jks fighter without spine1 bone.
		
		Transform weaponBone = Utility.findTransformUsingKeyword(_projectileOriginalTarget, "bone_weapon_1");
		if (weaponBone)
		{
			return weaponBone;
		}
		
		weaponBone = Utility.findTransformUsingKeyword(_projectileOriginalTarget, "bone_weapon_2");
		if (weaponBone)
		{
			return weaponBone;
		}
		
		weaponBone = Utility.findTransformUsingKeyword(_projectileOriginalTarget, "bone_weapon");
		if (weaponBone)
		{
			return weaponBone;
		}

		//Log.jprint(gameObject.name + "            _projectileOriginalTarget: "+ _projectileOriginalTarget + "         _projectileOriginalTarget.pos: " +  _projectileOriginalTarget.transform.position);

		
		return _projectileOriginalTarget.transform;
	}
	

	protected GameObject _projectileOriginalTarget;
	public GameObject ProjectileOriginalTarget { get { return _projectileOriginalTarget; } 
												 set { _projectileOriginalTarget = value;}}

	public void updateProjectileTarget()
	{
		//Log.jprint(gameObject + "     000     _projectileOriginalTarget: " + _projectileOriginalTarget);
		_projectileOriginalTarget = getCurrentTarget();
		//Log.jprint(gameObject + "     111     _projectileOriginalTarget: " + _projectileOriginalTarget);
	}


	public int getAttackPointByHitType(eHitType type, int attack)
	{
		#if UNITY_EDITOR
		if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
			Log.print_always("   T A K E   D A M A G E  -  hit type: "+ type + "   피해자: " + gameObject.name + "         damage: "+ attack + "  x   hitType 공격력 가중치  : "+ BattleBase.Instance.DamageRate(type) + "   finalDamage : "+ Mathf.RoundToInt(attack * BattleBase.Instance.DamageRate(type)));
		#endif
		//Log.jprint(" attack, damage rate : " + attack + " , " + BattleBase.Instance.DamageRate(type));
		return Mathf.RoundToInt(attack * BattleBase.Instance.DamageRate(type));
	}


	public int doMinimumDamageAdjustment(int attackPoint, int baseAttackPoint)
	{
		ObscuredInt finalAttack = attackPoint;

		ObscuredInt tenPercentOfBaseAttack = Mathf.RoundToInt(baseAttackPoint * 0.1f);

		if (finalAttack < tenPercentOfBaseAttack)
		{
			if (tenPercentOfBaseAttack < 1)
			{
				finalAttack = 1;
				#if UNITY_EDITOR
				if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
					Log.print_always("   T A K E   D A M A G E    - 피해자: " + gameObject.name + "   최소 damage 보정: hit type 적용된 damage의 10% damage 가 1 보다 낮아 최소 데미지 부여 : "+ finalAttack );
				#endif
			}
			else
			{
				finalAttack = tenPercentOfBaseAttack;
				#if UNITY_EDITOR
				if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
					Log.print_always("   T A K E   D A M A G E    - 피해자: " + gameObject.name + "   최소 damage 보정: damage 가 hit type 적용된 damage의 10% 보다 낮아, 상성 계산전 damage의 10% = 최종 Damage : "+ finalAttack );
				#endif
			}
		}
		else
		{
			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
				Log.print_always("   T A K E   D A M A G E    - 피해자: " + gameObject.name + "    최소 damage 보정 없음  =  최종 Damage : "+ finalAttack );
			#endif
		}


		return finalAttack;
	}


	public bool isSameCharacter(int characterType)
	{
		return _character_ID == characterType;
	}




	public virtual void aiMove() { }


	//jks 특정적 상대 시에 전투력 증/감 설정.
	public float getAttackRate_SpecialOpponent()
	{
		for (int i=0; i<_count_special_opponent; i++)
		{
			if (CharacterID == _specialOpponents[i]._characterID)
			{
				#if UNITY_EDITOR
				if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
					Log.print_always(gameObject.name +"!!!   Special defense rate 적용 : "+ _specialOpponents[i]._defense_rate  + "     자신 ID: "+ CharacterID + "   적 ID: " + _specialOpponents[i]._characterID);
				#endif

				return _specialOpponents[i]._defense_rate;
			}
		}
		return float.MinValue;
	}

	//jks 특정적 상대 시에 전투력 증/감 설정.
	public float getDefensdeRate_SpecialOpponent()
	{
		for (int i=0; i<_count_special_opponent; i++)
		{
			if (CharacterID == _specialOpponents[i]._characterID)
			{
				#if UNITY_EDITOR
				if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
					Log.print_always(gameObject.name +"!!!   Special attack 조정 rate 적용 : "+ _specialOpponents[i]._attack_rate  + "     자신 ID: "+ CharacterID + "   적 ID: " + _specialOpponents[i]._characterID);
				#endif

				return _specialOpponents[i]._attack_rate;
			}
		}
		return float.MinValue;
	}

	public virtual void setAttributesFromTable(Table_Character table)
	{
		Table_Character tbl = (Table_Character)table;

		_character_ID = tbl.ID;

		_body_type = tbl._body_type;
		_gender = tbl._gender;

		_radius = tbl._radius * 0.1f;
		_height = tbl._height * 0.1f; //jks if table value is 17, means _height = 1.7m

//jks obsolete		_animGroupByWeaponID = tbl._animGroupByWeaponID;

		_count_special_opponent = 0;

		//jks 특정적 상대 시에 전투력 증/감 설정.
		if (tbl._special_opponentID != 0)// && tbl._special_opponentID & 0xFF000000 == TABLE.TABLE_SPECIAL_OPPONENT)
		{
			Table_Special_Opponent oppTable = (Table_Special_Opponent)TableManager.GetContent(tbl._special_opponentID);	
			setAttributesFromTable(oppTable, _specialOpponents[0]);
			_count_special_opponent++;
		}
		//jks 특정적 상대 시에 전투력 증/감 설정.
		if (tbl._special_opponentID2 != 0)// && tbl._special_opponentID & 0xFF000000 == TABLE.TABLE_SPECIAL_OPPONENT)
		{
			Table_Special_Opponent oppTable = (Table_Special_Opponent)TableManager.GetContent(tbl._special_opponentID2);	
			setAttributesFromTable(oppTable, _specialOpponents[1]);
			_count_special_opponent++;
		}
		//jks 특정적 상대 시에 전투력 증/감 설정.
		if (tbl._special_opponentID3 != 0)// && tbl._special_opponentID & 0xFF000000 == TABLE.TABLE_SPECIAL_OPPONENT)
		{
			Table_Special_Opponent oppTable = (Table_Special_Opponent)TableManager.GetContent(tbl._special_opponentID3);	
			setAttributesFromTable(oppTable, _specialOpponents[2]);
			_count_special_opponent++;
		}

		//Log.jprint("Raction Table Reading: Critical : " + _reactionDistance[(int)eHitType.HT_CRITICAL] + "  Good : " + _reactionDistance[(int)eHitType.HT_GOOD]);
	}


	//jks 특정적 상대 시에 전투력 증/감 설정.
	protected void setAttributesFromTable(Table_Special_Opponent table, SpecialOpponent opp)
	{
		opp._characterID = table._characterID;
		opp._attack_rate = table._attack_rate * 0.01f;
		opp._defense_rate = table._defense_rate * 0.01f;
	}


	public virtual void setAttributesFromTable(Table_Enemy table)
	{
	}
	
	public virtual void setAttributesFromTable(Table_Spawn table)
	{
	}

	/// <summary>
	/// Sets the attributes from table. call this after skill and character table.
	/// </summary>
	/// <param name="table">Table.</param>
	public virtual void setAttributesFromTable(Table_AnimInfo table)
	{
		_opponentSearchingTime = table._opponent_searching_time * 0.1f;
		_teamMemberStartDistance = table._team_appear_offset * 0.1f;
		_leaderCatchUpDistance = table._leader_catchup_distance * 0.1f;
		_leaderCatchUpDistance_FightingTeamMember = table._front_team_catchup_distance * 0.1f;  //팀원이 앞에서 공격 중일 때 리더와 이 거리 이상 멀어지면 리더 전진 하게 함.

		_cameraTargetAttack = table._cam_target_attack;
		_cameraTargetIdle = table._cam_target_idle;
		_cameraTargetMove = table._cam_target_move;

		_speedRun = table._speed_run * 0.1f;
		_speedWalk = table._speed_walk * 0.1f;
		_speedWalkBack = table._speed_walkback * 0.1f;
		_speedWalkFast = table._speed_walkfast * 0.1f;

		_animSpeedRun = table._animspeed_run * 0.1f;
		_animSpeedWalk = table._animspeed_walk * 0.1f;
		_animSpeedWalkBack = table._animspeed_walkback * 0.1f;
		_animSpeedWalkFast = table._animspeed_walkfast * 0.1f;

	}


	public virtual void setAttributesFromTable(Table_Skill tbl)
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

		updateReaction(tbl._reactionTypeID);

		_weaponPathID = tbl._assetPathID_Weapon;
		//_weaponWeight = tbl._weight;

		//jks moved :  yield return StartCoroutine(installWeapon());
		//jks moved :  updateWeaponInfo();

		if (tbl._skill_info_attack_id > 0)
		{
			Table_Skill_Info_Attack tableInfo = (Table_Skill_Info_Attack)TableManager.GetContent(tbl._skill_info_attack_id);
			if (tableInfo != null)
				setAttributesFromTable(tableInfo);
		}
	}

	public virtual void setAttributesFromTable(Table_Skill_Info_Attack tbl)
	{
		_reactionOff = tbl._reaction_off != 0;
		_damagePenetration = tbl._damage_penetration * 0.01f;
		_buff_probability = tbl._buff_probability * 0.01f;	
		_buff_duration = tbl._buff_duration;		
		_attack_up = tbl._attack_up * 0.01f;			
		_defense_up = tbl._defense_up * 0.01f;		
		_critical_up = tbl._critical_up * 0.01f;		
		_attack_down = tbl._attack_down * 0.01f;		
		_defense_down = tbl._defense_down * 0.01f;		
		_critical_down = tbl._critical_down * 0.01f;		
		_skill_hit_only_buff_on = tbl._skill_hit_only_buff_on == 1;
	}

	public void updateReaction(int reactionTypeID)
	{
		Table_ReactionType tableReaction = (Table_ReactionType)TableManager.GetContent(reactionTypeID);
		setAttributesFromTable(tableReaction);
	}


	public void setAnim(Table_AnimInfo tblAnimInfo, Table_Skill tblSkill)
	{
		setAnimIdle(tblAnimInfo);
		setAnimWalk(tblAnimInfo);
		setAnimWalkback(tblAnimInfo);
		setAnimWalkbackTurn(tblAnimInfo);
		setAnimWalkfast(tblAnimInfo);
		setAnimRun(tblAnimInfo);
		setAnimProtect(tblAnimInfo);
		setAnimVictory(tblAnimInfo);
		setAnimInstallWeapon(tblAnimInfo);
		setAnimExhausted(tblAnimInfo);
		setAnimBlock(tblAnimInfo);
		setAnimCoolingJump(tblAnimInfo);
		setAnimCaptured_InTheAir(tblAnimInfo);

		setAnimCombo1(tblSkill._attack1); //jks combo1
		setAnimCombo2(tblSkill._attack2); //jks combo2
		setAnimCombo3(tblSkill._attack3); //jks combo3
		setAnimCombo4(tblSkill._attack4); //jks combo4
		setAnimCombo5(tblSkill._attack5); //jks combo5
		setAnimCombo6(tblSkill._attack6); //jks combo6
		setAnimCombo7(tblSkill._attack7); //jks combo6
		setAnimCombo8(tblSkill._attack8); //jks combo6
		
		setAnimSkillStaging(tblSkill._staging);
		
		setAnimPvpReady();
		setAnimPvpJustBeforeHit();
		setAnimInstallWeapon_Pre();
		//setAnimStun();
		setAnimCaptured();
		setAnimDeath();
		setAnimDeathB();
		setAnimPause();
	}



	public virtual void setAttributesFromTable(Table_ReactionType table)
	{
		Table_ReactionGroup tbl = (Table_ReactionGroup)TableManager.GetContent(table._reactionGroupID1);

		if (tbl != null)
		{
			_anim_reaction[0,0] = tbl._reactionID1;
			_anim_reaction[0,1] = tbl._reactionID2;
			_anim_reaction[0,2] = tbl._reactionID3;
			_anim_reaction[0,3] = tbl._reactionID4;
			_anim_reaction[0,4] = tbl._reactionID5;
			_anim_reaction[0,5] = tbl._reactionID6;
			_anim_reaction[0,6] = tbl._reactionID7;
		}

		if (table._reactionGroupID2 == 0) return;
		tbl = (Table_ReactionGroup)TableManager.GetContent(table._reactionGroupID2);
		if (tbl != null)
		{
			_anim_reaction[1,0] = tbl._reactionID1;
			_anim_reaction[1,1] = tbl._reactionID2;
			_anim_reaction[1,2] = tbl._reactionID3;
			_anim_reaction[1,3] = tbl._reactionID4;
			_anim_reaction[1,4] = tbl._reactionID5;
			_anim_reaction[1,5] = tbl._reactionID6;
			_anim_reaction[1,6] = tbl._reactionID7;
		}

		if (table._reactionGroupID3 == 0) return;
		tbl = (Table_ReactionGroup)TableManager.GetContent(table._reactionGroupID3);
		if (tbl != null)
		{
			_anim_reaction[2,0] = tbl._reactionID1;
			_anim_reaction[2,1] = tbl._reactionID2;
			_anim_reaction[2,2] = tbl._reactionID3;
			_anim_reaction[2,3] = tbl._reactionID4;
			_anim_reaction[2,4] = tbl._reactionID5;
			_anim_reaction[2,5] = tbl._reactionID6;
			_anim_reaction[2,6] = tbl._reactionID7;
		}


		if (table._reactionGroupID4 == 0) return;
		tbl = (Table_ReactionGroup)TableManager.GetContent(table._reactionGroupID4);
		if (tbl != null)
		{
			_anim_reaction[3,0] = tbl._reactionID1;
			_anim_reaction[3,1] = tbl._reactionID2;
			_anim_reaction[3,2] = tbl._reactionID3;
			_anim_reaction[3,3] = tbl._reactionID4;
			_anim_reaction[3,4] = tbl._reactionID5;
			_anim_reaction[3,5] = tbl._reactionID6;
			_anim_reaction[3,6] = tbl._reactionID7;
		}

		if (table._reactionGroupID5 == 0) return;
		tbl = (Table_ReactionGroup)TableManager.GetContent(table._reactionGroupID5);
		if (tbl != null)
		{
			_anim_reaction[4,0] = tbl._reactionID1;
			_anim_reaction[4,1] = tbl._reactionID2;
			_anim_reaction[4,2] = tbl._reactionID3;
			_anim_reaction[4,3] = tbl._reactionID4;
			_anim_reaction[4,4] = tbl._reactionID5;
			_anim_reaction[4,5] = tbl._reactionID6;
			_anim_reaction[4,6] = tbl._reactionID7;
		}

		if (table._reactionGroupID6 == 0) return;
		tbl = (Table_ReactionGroup)TableManager.GetContent(table._reactionGroupID6);
		if (tbl != null)
		{
			_anim_reaction[5,0] = tbl._reactionID1;
			_anim_reaction[5,1] = tbl._reactionID2;
			_anim_reaction[5,2] = tbl._reactionID3;
			_anim_reaction[5,3] = tbl._reactionID4;
			_anim_reaction[5,4] = tbl._reactionID5;
			_anim_reaction[5,5] = tbl._reactionID6;
			_anim_reaction[5,6] = tbl._reactionID7;
		}
	}




	public void updateWeaponInfo()
	{
		float legLength = Height * TestOption.Instance()._battle_leg_length_ratio;

		//jks 무기 없는 경우.
		if (_curWeapon == null) 
		{
			_weaponLength = legLength;
			return;
		}


		Transform curWeaponTransform = Utility.findTransformUsingKeyword(_curWeapon, "tip"); //jks if we have weapon, find tip.


		//jks 무기는 없지만 tip 본이 없는 경우. 예) arrow
		if (curWeaponTransform == null) 
		{
			_weaponLength = legLength;
			return;
		}


		//jks tip 본있는 무기인 경우.
		_weaponLength = Mathf.Abs(curWeaponTransform.localPosition.x);

		_weaponLength += legLength;
	}





	public virtual IEnumerator installWeapon()
	{
		if (_weaponPathID == 0) yield break; //jks means no weapon

		Table_Path tablePath = (Table_Path)TableManager.GetContent(_weaponPathID);

		if (_weaponAssetRef == null)
		{
			yield return StartCoroutine(ResourceManager.co_InstantiateFromBundle(tablePath._assetPath, result => _weaponObject = (GameObject)result));

			if (_weaponObject == null)
			{
				Debug.LogError("Can't find asset : "+ tablePath._assetPath);
			}
		}
		else
		{
			_weaponObject = (GameObject)Instantiate(_weaponAssetRef);
		}


		Weapon_Hold weapon = _weaponObject.GetComponent<Weapon_Hold>();

 		if (weapon == null)  //jks throw 타입 경우 예) 폭탄.  는 weapon script 없을 수 있음.
		{
			Debug.LogWarning("Can't find Weapon script on (throw 타입 무기 경우는 없을 수 있음.) "+ _weaponObject);
			yield break;
		}



		weapon.setPairItemPath(tablePath._assetPath);

		yield return StartCoroutine(weapon.install(gameObject));
		//weapon.hide(); 
		//_isWeaponVisible = false;

		//setWeaponVisibility(true); //jks 2014.11.3 initially show weapon

		//Log.jprint(gameObject.name + "   install weapon :  " + _weaponObject);
	}

	public override void showWeapon(bool bShow)
	{
		if (_weaponObject == null) return;
		
		Weapon_Hold weapon = _weaponObject.GetComponent<Weapon_Hold>();
		if (weapon != null)
			weapon.show(bShow);
	}

//	protected bool _isWeaponVisible = true;
//	public virtual void showWeapon(bool bShow)  //jks try show/hide weapon
//	{
//		if (_weaponObject == null) return;
//
////		if (IsTargetInShowWeaponRange)
////		{
////			bShow = true;
////		}
//		//jks - 2014.11.2  DESIGN CHANGE : setWeaponVisibility(bShow);
//		setWeaponVisibility(true);
//	}


//	protected void setWeaponVisibility(bool bShow)
//	{
//		if (_weaponObject == null) return;
//
//		if (_isWeaponVisible != bShow)
//		{
//			//Log.jprint(gameObject + "_isWeaponVisible: " + _isWeaponVisible + "  bShow: " + bShow);
//			_isWeaponVisible = bShow;
//
//			Weapon weapon = _weaponObject.GetComponent<Weapon>();
//			
//			if (weapon)
//			{
//				//Log.jprint(gameObject + " + - + - + - + - + weapon.show(" + bShow + ")");
//				weapon.show(bShow);
//
//				if (bShow)
//				{
//					//jks do IK weapon grab gesture
//					AnimCon.setArming(true, weapon.getWeaponType() == global::Weapon.eWeaponType.WT_Sword_Twin );
//					Invoke("finishArming", 0.5f);
//				}
//
//			}
//		}
//	}

	protected void finishArming()
	{
		AnimCon.setArming(false);
	}


	public override void setTrail(bool isTrail)
	{
		if (_weaponObject)
		{
			WeaponTrail wt = _weaponObject.GetComponentInChildren<WeaponTrail>();
			if (wt != null) 
			{
				wt.Emit = isTrail;
//				Knowledge_Mortal_Fighter_Main fighter = GetComponent<Knowledge_Mortal_Fighter_Main>();
//				if (fighter != null && GetComponent<Knowledge_Mortal_Fighter_Main>().IsLeader && wt.TrailObject != null)
//				{
//					wt.TrailObject.layer = LayerMask.NameToLayer("Fx");
//				}
			}
		}
	}


	protected virtual bool needToCatchUp()
	{
//		//jks 카메라 중심과의 거리 확인. 
//		if (Mathf.Abs(transform.position.x - CameraManager.Instance._targetCamera.transform.position.x) 
//		    > LeaderCatchUpDistance)
//			return true;

		//jks 가장 앞에서 공격 중인 팀원과의 거리 확인.
		Transform theTeamMember = BattleBase.Instance.getForemostFrontTeamMember();
		if (theTeamMember == null) return false;

		if (theTeamMember.position.x - transform.position.x > LeaderCatchUpDistance_FightingTeamMember)
			return true;

		return false;
	}


	public virtual bool shouldIRun()
	{
		return Action_Run;
	}

	public bool IsCameraAtFront
	{
		get {return CameraManager.Instance._targetCamera.transform.position.x > transform.position.x;}
	}

	//jks  거리 접점에 위치할 때  true와 false 계속 switching되어 떨리는 현상 제거. 일단 뛰면 최소 0.5초는 달리게 함.
	bool _runForAWhile = false;
	protected virtual float FollowContinueTime { get { return 0.5f;}}

	public virtual bool amIInView()
	{
		if (!IsBattleInProgress) return true;

		if (_runForAWhile) return false;

		if (needToCatchUp())
		{
			_runForAWhile = true;
			Invoke("resetRunForAWhile", FollowContinueTime);
			return false;
		}
		else
		{
			return true;
		}

	}

	public bool inRangeFromCam(float distance)
	{
		if (Mathf.Abs(transform.position.x - CameraManager.Instance._targetCamera.transform.position.x) < distance)
			return true;
		return false;
	}


	void resetRunForAWhile()
	{
		_runForAWhile = false;
	}




	public bool isInFrontOfMe(GameObject go)
	{
		//if ( BattleBase.Instance.HeadingRight )
		if (gameObject.transform.forward.x > 0)
		{
			if (go.transform.position.x > gameObject.transform.position.x)
				return true;
		}
		else
		{
			if (go.transform.position.x < gameObject.transform.position.x)
				return true;
		}
		
		return false;
	} 





	#region jsm
	public GameObject ProjectileObject
	{
		get; set;
	}

	public void spawnDamageEffect(GameObject attacker, eAttackType attackType, eHitType hitType, eWeaponType_ForAnimation weaponType)
	{
		if (IsDead) return;

//		switch(attackType)
//		{
//		case eAttackType.AT_Sword:
//		case eAttackType.AT_Fist:
//			break;
//		default:
//			return;
//		}

		FxManager fxManager = GetComponent<FxManager>();
		if (fxManager != null)
		{
			fxManager.spawnDamageFx( attacker, attackType, hitType, weaponType);
		}
	}

	public override void playSound(string key)
	{
		string[] flag = key.Split('/');
		
		if (flag[0].Equals("Weapon"))
		{
			if (_weaponObject)
			{
				FxManager manager = _weaponObject.GetComponent<FxManager>();
				if(manager)
					manager.play(key);
			}
		}
		else
		{
			FxManager manager = gameObject.GetComponent<FxManager>();
			if(manager)
				manager.play(key);
		}
		
	}
	
	public override void showEffect(string key)
	{
		FxManager fxm = GetComponent<FxManager>();
		if (fxm != null)
			fxm.spawn(key);
	}

	public override void spawnEffectAtTarget(string key)
	{
		FxManager fxm = GetComponent<FxManager>();
		if (fxm != null)
			fxm.spawnAtTargetTransform(key);
	}

	public override void deSpawnEffectAtTarget(string key)
	{
		FxManager fxm = GetComponent<FxManager>();
		if (fxm != null)
			fxm.deSpawnAtTargetTransform(key);
	}

	public GameObject WeaponObject
	{
		get { return _weaponObject; }
	}
    #endregion

    protected virtual void classRelTestFunc(Knowledge_Mortal_Fighter knowledgeOpponent)
    {
        if (!TestOption.Instance()._battlePopupUI)
            return;

        if (AllyID != eAllyID.Ally_Human_Me)
            return;

        int classRelation = 0;
        bool blockBuff = false;

        if (Progress_Action_Quick)
            return;
        if (KnowledgeMultiSkill != null && KnowledgeMultiSkill.Progress_AnyAction)
            return;

        if (IsLeader || knowledgeOpponent.IsLeader)
        {
            classRelation = CharacterClassRelation.getAttackRate(_class, knowledgeOpponent.Class);
        }

        if (BattleUI.Instance() != null)
        {
            if (IsLeader && BattleUI.Instance().ClassMatchOwner != -1)
            {
                blockBuff = true;
            }

            BattleUI.Instance().showClassRelation(_class, knowledgeOpponent.Class, classRelation, blockBuff, IsLeader);
        }
    }

	#region jsm
	/// <summary>
	/// 적 캐릭에 체력 게이지를 달아준다. 
	/// </summary>
	/// jsm_0818
	public DisplayHealthBar _healthBar = null;
	public void attachHPBar() //jks void Start()
	{
		if (!BattleBase.Instance.isSkillPreviewMode())
		{
			_healthBar = gameObject.AddComponent<DisplayHealthBar>();
            _healthBar.setClass((int)_class);
		}
	}
	#endregion
}
