
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;



public enum eBotType
{
	BT_None = 0,
	BT_Plain = 1,
	BT_Boss = 2,
	BT_Boss_Story = 3,
	BT_Boss_obolete = 4,   //jks 더 이상 사용하지 않아 "BT_Boss" 로 취급.
	BT_OperatingWeapon = 5,
	BT_Blockage = 6,
	BT_Boss_Leader = 7,
	BT_Boss_Raid = 8, //jks 레이드 보스.
}


public enum eAIMoveType
{
	AMT_Stay=0,
	AMT_Walk=1,
	AMT_WalkFast=2,
	AMT_Run=3
}


public class Knowledge_Mortal_Fighter_Bot : Knowledge_Mortal_Fighter
{

	protected float[] _counterAttackRate = new float[6];

	protected bool _counterAttackInProgress = false;

	protected float _scanDistance = -1;

	protected eAIMoveType _AIMoveType;

	protected float _moveDelay;

	protected bool _counterAttackOn;
	
//	protected bool _passThrough = true;

	protected eBotType _botType;
	protected bool _isGoalTarget = false;

	protected int _dialogID;
	protected int _dialogID2;
	protected int _dialog2_trigger_condition;

	public int Dialog2_Trigger_Condition { get { return _dialog2_trigger_condition; }}

//jks 2015.11.4 보스액션 제거.	protected int _bossAction;

	protected int _botID;

	protected int _populationTolerance;

	protected int _reaction_rate;  // 스킬/평타 공격을 맞을 때, 리액션 할지에 대한 판정 확률.
	protected int _reaction_rate_quickskill_melee; //jks 근거리 평타 확율.
	protected int _reaction_rate_quickskill_distant; //jks 원거리 평타 확율.
	protected int _reaction_judge_start_combo_qs; //평타 맞을 경우, 몇번 combo 이상 부터 reaction rate 판정을 적용 할 것인지 설정.

	protected bool _attackWhenOpponentRest;

	protected bool _jumpBackIfPossible;

	protected bool _waveSignal;
	protected bool _do_not_attack;

	protected bool _accumulatedHitReaction;
	protected int _anim_accumulatedHitReaction; 
	protected bool _accumulatedHitReactionInProgress = false;

//jks 2015.11.4 보스액션 제거.	protected bool _actionBossInProgress = false;
//jks 2015.11.4 보스액션 제거.	protected bool _actionBoss;

	protected List<int> _patternSequence = new List<int>();  //jks 2015.9.28 : protected int _patternSequence = 0;  	 //jks from AI table : 콤보패턴의 반복 순서.
	protected int _patternSequenceIndex; //jks current pattern (n th index)

//jks 2016.1.5	protected List<ComboPatternSet> _comboPatterns = new List<ComboPatternSet>();  //jks from AI table : combo 가 순차적으로 나가지 않고, 여기에 정해진 순서대로 나감.

	protected List<BossPatternInfo> _bossPatternInfo = new List<BossPatternInfo>();

	protected int[] _patternSkillID = new int[6];

	protected int _comboPattern_current; //jks current pattern    //protected ComboPatternSet _comboPattern_current; //jks current pattern

	protected bool _minion = false;

	public bool HavePatternSkill { get { return _patternSequence.Count > 0; }}

	public int[] PatternSkillID
	{
		get { return _patternSkillID; }
	}


	//jks protected SupportSkillKnowledge_Boss _knowledgePatternSkill;
	public SupportSkillKnowledge_Boss KnowledgePSkill 
	{ 
		get 
		{ 
			return (SupportSkillKnowledge_Boss) base.KnowledgeMultiSkill;
		}
	}
	

	protected override List<FighterActor> Opponents 
	{ 
		get 
		{ 
			if (AllyID == eAllyID.Ally_Human_Me)  //jks summoned weapon has "know...bot" but could be Ally_Human_Me.
				return BattleBase.Instance.List_Enemy;
			return BattleBase.Instance.List_Ally; 
		}
	}


	public override bool IsNotAttacking 
	{ get 
		{
			return ! Progress_SkillAction
				&& ! KnowledgePSkill.Progress_Action;
		}
	}


	public eBotType BotType
	{
		get { return _botType; }
	}

	public override bool IsBoss
	{
		get { return _botType == eBotType.BT_Boss || _botType == eBotType.BT_Boss_Raid 
			|| _botType == eBotType.BT_Boss_Story || _botType == eBotType.BT_Boss_Leader; }
	}
		
	public override bool IsRaidBoss
	{
		get { return _botType == eBotType.BT_Boss_Raid; }
	}

	public virtual bool IsGoalTarget
	{
		get { return _isGoalTarget; } set { _isGoalTarget = value; }
	}
	


	public bool IsMinion
	{
		get { return _minion; } set { _minion = value; }
	}
	
	public int DialogID
	{
		get { return _dialogID; }
	}
	public int DialogID2
	{
		get { return _dialogID2; }
	}

//jks 2015.11.4 보스액션 제거.
//	public int BossActionAnimNumber
//	{
//		get { return _bossAction; }
//	}
	
	//	public bool PassThrough
//	{
//		set { _passThrough = value; }
//		get { return _passThrough; }
//	}
	
	public int BotID
	{
		set { _botID = value; }
		get { return _botID; }
	}
	
	public int PopulationTolerance
	{
		get { return _populationTolerance; }
	}

	public int ReactionRate
	{
		get { return _reaction_rate; }
	}
	public int ReactionRate_QuickSkill_Melee
	{
		get { return _reaction_rate_quickskill_melee; }
	}
	public int ReactionRate_QuickSkill_Distant
	{
		get { return _reaction_rate_quickskill_distant; }
	}
	public float ReactionJudgeStartCombo_QS //평타 전용
	{
		get { return _reaction_judge_start_combo_qs; }
	}


	public bool AttackWhenOpponentRest
	{
		get { return _attackWhenOpponentRest; }
	}
	
	public bool WaveSignalOn
	{
		get { return _waveSignal; }
	}

	public bool DoNotAttack
	{
		get { return _do_not_attack; }
	}

	private int _rage_start_point = 5;
	public int RageStartPoint { get { return _rage_start_point; } }


	private int _total_giveDamage_count = 0;
	public int TotalGivedamageCount { get { return _total_giveDamage_count; }}
	/// <summary>
	/// Increments the total give damage count.  소환된 무기가 스킬에 정해진 공격횟수를 채우면 사라지게 하기 위함.
	/// </summary>
	protected void updateTotalGiveDamageCount() 
	{ 
		if (! IsLivingWeapon ) return;

		_total_giveDamage_count ++; 

		if (_total_giveDamage_count >= DamageFrequency) // summoned character type weapon, won't cool down, will be destroyed instead after attack.
		{
			Action_Paused = true; // do nothing
			Destroy(gameObject, 0.8f);
		}
	}

	public override eBotType getBotType() { return BotType; }

	public bool Action_AccumulatedHitReaction { get { return _accumulatedHitReaction; } set {_accumulatedHitReaction = value; } }
	public int Anim_AccumulatedHitReaction 	{ get { return _anim_accumulatedHitReaction; } set { _anim_accumulatedHitReaction = value;}}

	public int animAccumulatedHitReaction(int animNum)
	{
		int anim_info_id = getAnimInfoID(); //jks character table 과 skill table 의 set attribues 하는 부분이 콜된 이후 콜해야 함. 
		Table_AnimInfo animInfoTable = (Table_AnimInfo)TableManager.GetContent(anim_info_id);
		int animForGeneric = animInfoTable._anim_generic_accumulated_hit_reaction;

		if (animForGeneric != 0)
			return getAnimHash("Reaction" + animForGeneric.ToString());
		else
			return getAnimHash("AccumulatedHitReaction" + animNum);
	}

	public bool Progress_AccumulatedHitReaction { get { return _accumulatedHitReactionInProgress;} set { _accumulatedHitReactionInProgress = value; } }





//jks 2015.11.4 보스액션 제거.	public bool Action_Boss { get { return _actionBoss;} set {_actionBoss = value;}}
//jks 2015.11.4 보스액션 제거.	public bool Progress_ActionBoss { get { return _actionBossInProgress;} set { _actionBossInProgress = value; } }
//	public int animActionBoss()
//	{
//		return getAnimHash("BossAction" + BossActionAnimNumber);
//	}




	/// <summary>
	/// Gets the animation info ID. 캐릭터 조건에 따라 다른 애니메이션 정보를 가져오기 위해 조건으로 만들어진 ID 를 사용해서 테이블 항목을 찾음.
	/// 
	/// 100000의 자리수:  6
	///	10000의 자리수: 2
	///	
	///	1000, 100 자리 수:  체형: 	(0=small, 1=medium, 2=big, 3=huge, ...99)  캐릭터테이블의 body_type.
	///	
	///	10의자리 1의 자리:  무기타입 ( 스킬테이블의 weaponType )
	///	
	///	예) 2C620105 : monster 성인 한손검 
	///	(2C는 이 테이블 공통)
	/// </summary>
	/// <returns>The animation info ID.</returns>
	public override int getAnimInfoID()
	{
		Table_Skill skillTable = (Table_Skill)TableManager.GetContent(SkillID);
		Table_Character characterTable = (Table_Character)TableManager.GetContent(CharacterID);

		string classType = "6"; //jks 팀원이 아닌 경우는 클래스와 성별은 무시함.
		string gender = "2";

		string bodyType = characterTable._body_type > 9 ? characterTable._body_type.ToString() : "0"+characterTable._body_type.ToString();
		string weaponAnimType = skillTable._weaponType > 9 ? skillTable._weaponType.ToString() : "0"+skillTable._weaponType.ToString();

		string idHex = classType + gender + bodyType + weaponAnimType;
		int id = (int)DataUtility.ParseHex(idHex);

		id += (int)TABLE.TABLE_ANIM_INFO;

		//Log.jprint("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ getAnimInfoID()  ID: " + id + "   in Hex : "+ idHex);

		return id;
	}




	// called from Behavior Tree
	public bool shouldIWaitForChance()
	{
		if (IsProtectee) return false;

		// cooltime attacker ?         // if i m alone, ignore ai 
		if (_attackWhenOpponentRest && BattleManager.Instance.EnemyToKill > 1)
		{
			// do I have target
			if (_target_attack == null) 
				return false;
			
			// is my target in cooltime ?
			if (_target_attack.GetComponent<Knowledge_Mortal_Fighter_Main>().isCoolingInProgress()) 
			{
				Action_Run = true;

				return false;
			}

			Action_Idle = true;

			return true;
		}


		return false;
	}


	void makePatternSkillIDList(Table_AIAttack tableAIAttack)
	{
		for (int k=0; k < tableAIAttack._bossPatternInfo.Length ; k++)
			_patternSkillID[k] = tableAIAttack._bossPatternInfo[k]._skillID;
	}


	void makePatternSequenceList(Table_AIAttack tableAIAttack)
	{
		_patternSequence.Clear();

		if (tableAIAttack._patternSequence <= 0) return;

		_haveSkillPattern = true;

		for(int k = 1; true; k++)
		{
			int nth_pattern = Utility.getNthDigit(tableAIAttack._patternSequence, k);
			if (nth_pattern == 0) //no more sequence from here.
				break;

			_patternSequence.Add(nth_pattern);


			//jks if any repeat
			int repeat = tableAIAttack._bossPatternInfo[nth_pattern-1]._repeat; // index: 0~5  and  nth_pattern: 1~6  

			for (int j=0; j<repeat; j++)
				_patternSequence.Add(nth_pattern);

			//Log.jprint("nth_pattern : "+ nth_pattern + " repeat: "+ repeat );

		}

		//Log.jprint("tableAIAttack._patternSequence : "+ tableAIAttack._patternSequence );

		for(int k=0; k < _patternSequence.Count; k++)
				Log.jprint(" List : " + _patternSequence[k]);
	}



	public override void setAttributesFromTable(Table_Spawn tableSpawn)
	{
		//jks set AI properties
		Table_AIAttack tableAIAttack = (Table_AIAttack)TableManager.GetContent ( tableSpawn._ai_attackID );
		_coolTime = tableAIAttack._coolTime * 0.1f; //jks fixed point conversion (0 ~ 25.5 sec)
		_criticalRate = tableAIAttack._criticalRate * 0.01f;

		_scanDistance = tableAIAttack._scanDistance * 0.1f;
		initSentinel(); //jks initial call must be called after _scanDistance has value from table.

		_AIMoveType = (eAIMoveType)tableAIAttack._defaultMove;
		_moveDelay = tableAIAttack._moveDelay; //jks  second
		_counterAttackOn = tableAIAttack._counterAttack != 0;
		_populationTolerance = tableAIAttack._populationTolerance;
		_reaction_rate = tableAIAttack._reactionRate;
		_reaction_rate_quickskill_melee = tableAIAttack._reactionRate_QuickSkill % 10000 / 100;
		_reaction_rate_quickskill_distant = tableAIAttack._reactionRate_QuickSkill % 100;

		_reaction_judge_start_combo_qs = tableAIAttack._reactionJudgeStartComboForQuickSkill;
		_attackWhenOpponentRest = (1 == Utility.getNthDigit(tableAIAttack._on_off_info, 1));
		_jumpBackIfPossible = (1 == Utility.getNthDigit(tableAIAttack._on_off_info, 2));
		_waveSignal = (1 == Utility.getNthDigit(tableAIAttack._on_off_info, 3));
		_do_not_attack = (1 == Utility.getNthDigit(tableAIAttack._on_off_info, 4));



		//ai : combo pattern 
		makePatternSequenceList(tableAIAttack);  //jks : _patternSequence = tableAIAttack._patternSequence;

		makePatternSkillIDList(tableAIAttack);

		if (IsRaidBoss)
		{
			SupportSkillKnowledge_Boss_Raid know_pskill = gameObject.AddComponent<SupportSkillKnowledge_Boss_Raid>();//jks 보스패턴스킬 없어도 추가

			if (tableAIAttack._numPatternSkill > 0)
			{
				StartCoroutine(know_pskill.setPatternSkillInfoFromTable(tableAIAttack));
				know_pskill.addLauncher(tableAIAttack);
			}

			gameObject.AddComponent<SupportSkillBehavior>();  //jks 보스패턴스킬 없어도 추가 (behavior tree 관련)
		}
		else if (IsBoss)
		{
			SupportSkillKnowledge_Boss know_pskill = gameObject.AddComponent<SupportSkillKnowledge_Boss>();//jks 보스패턴스킬 없어도 추가

			if (tableAIAttack._numPatternSkill > 0)
			{
				StartCoroutine(know_pskill.setPatternSkillInfoFromTable(tableAIAttack));
				know_pskill.addLauncher(tableAIAttack);
			}

			gameObject.AddComponent<SupportSkillBehavior>();  //jks 보스패턴스킬 없어도 추가 (behavior tree 관련)
		}


		if(tableAIAttack._patternSequence > 0)
		{
			for (int k=0; k < 6; k++)
			{
//jks 2016.1.5				_comboPatterns.Add(tableAIAttack._comboPatterns[k]);
				Log.jprint(gameObject.name + "    k : "+ k + "   set attri..  tableAIAttack._bossPatternInfo[k]._cool_time: "+tableAIAttack._bossPatternInfo[k]._cool_time);
				_bossPatternInfo.Add(tableAIAttack._bossPatternInfo[k]);
			}

			initComboPatternSys();
			updateCurrentComboPattern();
		}


		if (_counterAttackOn)
		{
			//jks set counter attack probability.
			Table_AICounterAtk tableAIcounterAtk = (Table_AICounterAtk)TableManager.GetContent ( tableSpawn._ai_counterAtkID ); 
			_counterAttackRate[(int)eHitType.HT_CRITICAL] = tableAIcounterAtk._critical * 0.01f;
			_counterAttackRate[(int)eHitType.HT_GOOD] = tableAIcounterAtk._good * 0.01f;
			_counterAttackRate[(int)eHitType.HT_NICE] = tableAIcounterAtk._nice * 0.01f;
			_counterAttackRate[(int)eHitType.HT_MISS] = tableAIcounterAtk._miss * 0.01f;
			_counterAttackRate[(int)eHitType.HT_BAD] = 1.0f;
		}
	}


	public override void setAttributesFromTable(Table_Enemy enemyTable)
	{
		base.setAttributesFromTable(enemyTable);

		_botType = (eBotType)enemyTable._type;

		if (_botType == eBotType.BT_Boss_obolete) //jks2016.5.26 사용하지 않는 보스 타입이라,  테이블에 들어 있으면 BT_Boss 로 대체.
			_botType = eBotType.BT_Boss;

		_dialogID = enemyTable._dialogID;
		_dialogID2 = enemyTable._dialogID2;
		_dialog2_trigger_condition = enemyTable._dialog2_trigger_condition;

//jks 2015.11.4 보스액션 제거.		_bossAction = enemyTable._bossAction;
		//_defense = (enemyTable._defense * 100 / (enemyTable._defense + 100)) * 0.01f;
		_defense = enemyTable._defense;
		 
		_max_hp = IsRaidBoss ? BattleManager.Instance.SavedRaidBossCurrentHP : (ObscuredInt)enemyTable._max_HP;
		_cur_hp = _max_hp;

		_level = enemyTable._level;

		_attackPoint = enemyTable._attack;

		_class = (CardClass)enemyTable._class;

		if (IsBoss)
		{
			BattleBase.Instance.BossName = enemyTable._str_Name; //jks 이름 저장.
		}

		/*
		if (_healthBar != null) {
			if ((eBotType)enemyTable._type != eBotType.BT_Boss_Story && (eBotType)enemyTable._type != eBotType.BT_Boss_Raid && (eBotType)enemyTable._type != eBotType.BT_Boss) {
				_healthBar.setHelth (MaxSinsu, CurrentSinsu, 0);
				Vector3 barPosition = transform.localPosition;
				barPosition.y = _height;
				_healthBar.transform.localPosition = barPosition;
			}
			else {
				Destroy(_healthBar);
				_healthBar = null;
			}
		}
		*/

		//Log.jprint(gameObject.name + "________________criticalRate: " + _criticalRate);

	}


	public override void aiMove()
	{
		Action_Idle = false;

		//Log.jprint(gameObject.name + "_______________AIMoveType" + _AIMoveType);
		
		if (_AIMoveType == eAIMoveType.AMT_Stay)
		{
			Action_Idle = true;
		}
		else if (_AIMoveType == eAIMoveType.AMT_Walk)
		{
			Action_Walk = true;
		}
		else if (_AIMoveType == eAIMoveType.AMT_WalkFast)
		{
			Action_WalkFast = true;
		}
		else if (_AIMoveType == eAIMoveType.AMT_Run)
		{
			Action_Run = true;
		}
	}


	protected void aiMove_initialMove()
	{
		Action_Idle = true;
		
		//Log.jprint(gameObject.name + "_______________AIMoveType" + _AIMoveType);
		
		if (_AIMoveType != eAIMoveType.AMT_Stay)
		{
			Invoke("aiMove", _moveDelay);
		}
	}
	
	
	protected override void initializeBeforeUpdateBegin()
	{
		base.initializeBeforeUpdateBegin();
		aiMove_initialMove();

		AnimCon.setAnimatorCullingMode(AnimatorCullingMode.BasedOnRenderers);
		
//		if (!PassThrough || IsBoss)
//			InvokeRepeating("bossBorderLine", 1, 0.1f);//0.25f);

//		Utility.setLayerAllChildren(gameObject, "Enemy");
		if (IsBoss)
		{
			InvokeRepeating("checkMinionSpawnerActivation", 1, 1); //StartCoroutine(checkMinionSpawnerActivation());
		}
	}



	void OnEnable()
	{
		initSentinel();
		InvokeRepeating("accordAttackDirection", 0.1f, 0.5f);
	}

	void OnDisable()
	{
		//CancelInvoke("searchTarget");
		CancelInvoke("accordAttackDirection");
	}


	public virtual void initSentinel() 
	{
		//Log.jprint(gameObject.name + "....... _AIMoveType: " + _AIMoveType + "  _scanDistance: " + _scanDistance);
		if (_AIMoveType == eAIMoveType.AMT_Stay && _scanDistance > 0)
		{
			InvokeRepeating("searchTarget", 0.1f, 0.5f);
		}
	}

	//jks give staying monster a sentinel function to approach and attack target which is in _scanDistance. 
	protected void searchTarget()
	{
		if (IsBossDialogStagingMode) return; //jks 대화 중일때는 적을 찾지않고, 이동 상태 콘트롤 하지 않음.

		if (IsBoss && HavePatternSkill) //jks if 보스이고 패턴스킬 사용일 경우,
		{
			if (KnowledgePSkill.Progress_AnySkillAnimation)
				return;
		}

		if (IsLivingWeapon)
		{
			if (BattleBase.Instance.IsIgnoreButtonTouch) //jks 턴이 바뀌는 중이면, 그전에 사용한 소환무기는 제거.
			{
				Action_Paused = true; // do nothing
				Destroy(gameObject);
			}
		}

		//Log.jprint(gameObject.name + "................searchTarget");
		bool found = isOpponentsInDistance(_scanDistance);
		
		if (found)
		{
			Action_Idle = false;

			if (IsLivingWeapon)
			{
				Action_Run = true;
			}
			else
			{
				aiMove();
			}
		}
		else if (_AIMoveType == eAIMoveType.AMT_Stay)
		{
			Action_Idle = true;
			Action_Walk = false;
		}
	}


	public override bool shouldIJumpBackToKeepDistance()
	{
		if (_jumpBackIfPossible == false) return false;

		//jks 공격 중이면, 자동으로 뒤로 후퇴하여 거리 확보하지 않음.
		if (Progress_SkillAnimation || Progress_SkillAction) return false;

		//jks 보스 대화 연출 중이면, 자동으로 뒤로 후퇴하여 거리 확보하지 않음.
		if (BattleBase.Instance.IsBossDialogStagingMode) return false;

		if (_target_attack == null) return false;

		Knowledge_Mortal_Fighter targetKnow = _target_attack.GetComponent<Knowledge_Mortal_Fighter>();

		if (! targetKnow.Progress_SkillAction) return false;

		if (! targetKnow.IsCloseAttack) return false;

		if ( (Action_Run || Action_Walk) && isTooCloseToFlee()) return false;

		//		if (!AttackCoolTimer.IsCoolingInProgress) return false;


		if (!TooDangerous) //jks 위험상태가 아닌 경우에,  계속 위험상태가 아닐지 확인.
		{
			if (JumpBackCount == 0)
			if (amIInDamageRange()) //jks 적이 가깝고,
			{
				if (_target_attack != null && _target_attack.GetComponent<Knowledge_Mortal_Fighter>().Progress_SkillAction) //jks  적이 공격 중이면,
				{
					TooDangerous = true; //jks  위험상태로 전환.
				}
			}
		}
		else if (JumpBackCount > 10 && !amIInDamageRange()) //jks 일단 위험상태로 전환되었으면, count를 하여 멀어져도 지속하게 함.
		{
			//reset
			TooDangerous = false;
			JumpBackCount = 0;
		}

		//jks 위험한 상태가 아니면, 자동으로 뒤로 후퇴하여 거리 확보하지 않음.
		if (!TooDangerous) return false;


		JumpBackCount++;


		return true;
	}


//	public override bool shouldIJumpBackToKeepDistance()
//	{
//		if ((AttackCoolTimer.IsCoolingInProgress || Progress_CoolingJump) 
//			&& !CoolingJumpDone
//			&& getTotalBotsInFrontOfMe() >= PopulationTolerance)
//		{
//			Action_CoolingJump = true;
//			return true;
//		}
//
//		//jks 이미 점프 중이면, 넘어감.
//		if (Progress_CoolingJump) return false;
//
//		Action_CoolingJump = false; //reset
//
//		if (_jumpBackIfPossible == false) return false;
//
//		//jks 공격 중이면, 자동으로 뒤로 후퇴하여 거리 확보하지 않음.
//		if (Progress_SkillAnimation || Progress_SkillAction) return false;
//		
//		//jks 보스 대화 연출 중이면, 자동으로 뒤로 후퇴하여 거리 확보하지 않음.
//		if (BattleBase.Instance.IsBossDialogStagingMode) return false;
//
//		if (_target_attack == null) return false;
//
//		if (_jumpBackDelay) return false;
//
//
//		Knowledge_Mortal_Fighter targetKnow = _target_attack.GetComponent<Knowledge_Mortal_Fighter>();
//		
//		if (! targetKnow.Progress_SkillAction) return false;
//		
//		if (! targetKnow.IsCloseAttack) return false;
//
//		if ( (Action_Run || Action_Walk) && isTooCloseToFlee()) return false;
//
//
//		if (!TooDangerous) //jks 위험상태가 아닌 경우에,  계속 위험상태가 아닐지 확인.
//		{
//			if (JumpBackCount == 0)
//				if (amIInDamageRange()) //jks 적이 가깝고,
//			{
//				if (_target_attack != null && _target_attack.GetComponent<Knowledge_Mortal_Fighter>().Progress_SkillAction) //jks  적이 공격 중이면,
//				{
//					TooDangerous = true; //jks  위험상태로 전환.
//				}
//			}
//		}
//		else if (JumpBackCount > 10 && !amIInDamageRange()) //jks 일단 위험상태로 전환되었으면, count를 하여 멀어져도 지속하게 함.
//		{
//			//reset
//			TooDangerous = false;
//			JumpBackCount = 0;
//		}
//		
//		//jks 위험한 상태가 아니면, 자동으로 뒤로 후퇴하여 거리 확보하지 않음.
//		if (!TooDangerous) return false;
//		
//		
//		JumpBackCount++;
//		
//		_jumpBackDelay = true;
//		Invoke("resetJumpBackDelay", 1.5f); //jks 후진 최소 interval 1.5초. 
//
//		Action_CoolingJump = true;
//
//		return true;
//	}



	public int getTotalBotsInFrontOfMe()
	{
		int botsAheadCount = 0;
		
		foreach(FighterActor ea in BattleBase.Instance.List_Enemy)
		{
			GameObject go = ea._go;
			if (go == null) continue;
			if (!go.activeSelf) continue;

			Knowledge_Mortal goKnowledge = go.GetComponent<Knowledge_Mortal>();
			if (goKnowledge == null) continue;
			if (goKnowledge.AllyID != eAllyID.Ally_Bot_Opponent) continue;
			if (goKnowledge.IsDead) continue;
			if (!isInFrontOfMe(go)) continue;
			
			botsAheadCount++;
		}
		
		return botsAheadCount;
	}



	public override bool shouldIJumpBack()
	{
		if (BattleBase.Instance.isSkillPreviewMode()) return false;

		if (IsLivingWeapon) 
			return AttackCoolTimer.IsCoolingInProgress;

		return
			(
				(AttackCoolTimer.IsCoolingInProgress || Progress_CoolingJump)  //jks even cool time is finished earlier, keep playing cooling jump animation.
				&& !CoolingJumpDone
				&& getTotalBotsInFrontOfMe() >= PopulationTolerance
			)
			||
			shouldIJumpBackToKeepDistance();
	}

	

	public override bool checkEnemyOnPath(bool bQuickSkill)
	{
		return getOpponentsToAttackWhileMoving(bQuickSkill); //check 10 meter for the initial scan.
	}


	protected float AttackDistanceMax
	{
		get 
		{
			if (IsBoss && HavePatternSkill)
			{
				return KnowledgePSkill.AttackDistanceMax;
			}
			else
			{
				return _attackDistanceMax;
			}
				
		}
	}




	protected override void processCooltimeFinished()
	{
		//Log.jprint(gameObject.name + "C O O L  T I M E   E N D"); 
		aiMove();
	}


	public override bool isLastDamageInSkill(float reactionDistanceOverride)
	{
		if (reactionDistanceOverride >= 1000)  	//jks  if last damage in combo
		{
//jks 2016.1.5
//			if (_haveSkillPattern)
//			{
//				if (IsLastComboInCurrentPattern)
//					return true;
//			}
//			else
			{
				//jks if (TotalCombo == ComboCurrent)
				//jks 2015.4.27 : 콤보 끝에 블랜딩 애니메이션이 들어가거나 뒤로 점프하는 애니메이션을 넣어 사용하는 경우로 인해 실제 데미지를 주는 마지막 공격인지 판단을 위해 추가된 LastCombo를  사용..

				if (LastCombo == ComboCurrent) //jks  if last combo in skill
					return true;
			}
		}
		return false;
	}
	



	#region AI combo pattern


	public override int TotalCombo 				
	{ 
		get 
		{ 
//jks 2016.1.5
//			if (IsBoss && _haveSkillPattern)
//				return CurrentPattern_TotalCombo; //jks 보스는 전용 공격 패턴내의 공격 수를 사용.
//			else
				return _totalCombo; 
		} 
	}

	public override int LastCombo
	{ 
		get 
		{ 
//jks 2016.1.5
//			if (IsBoss && _comboPatterns.Count > 0)
//				return CurrentPattern_TotalCombo; //jks 보스는 last combo 개념 없이 total combo 와 항상 같이 하게 함.
//			else
				return _lastCombo; 
		} 
	}


	private bool _haveSkillPattern = false;

//jks 2016.1.5
//	protected bool IsLastComboInCurrentPattern
//	{
//		get { return getCombo(ComboCurrent + 1) == 0; }
//	}

//jks 2016.1.5
//	protected ComboPatternSet CurrentComboPattern
//	{
//		get { return _comboPatterns[_comboPattern_current]; }
//	}


//jks 2016.1.5
//	public int CurrentPattern_TotalCombo
//	{
//		get 
//		{ 
//			if (_comboPatterns.Count > 0)
//				return (int)_comboPatterns[_comboPattern_current]._total_combo;
//			else
//				return 0;
//		}
//
//	}


//jks 2016.1.5
//	public int CurrentPattern_ReactionTypeID
//	{
//		get 
//		{ 
//			if (_comboPatterns.Count > 0)
//				return (int)_comboPatterns[_comboPattern_current]._reationTypeID;
//			else
//				return 0;
//		}
//
//	}




//jks 2015.12.10 보스공격 패턴도 적 테이블의 공격력 사용.	public int CurrentPattern_AttackPoint
//	{
//		get 
//		{ 
//			if (_bossPatternInfo.Count > 0)
//				return (int)_bossPatternInfo[_comboPattern_current]._attack_point; 
//			else
//				return 0;
//		}
//
//	}
	public int CurrentPattern
	{
		get 
		{
			return _comboPattern_current; 
		}
	}

	public int CurrentPattern_Repeat
	{
		get 
		{ 
			if (_bossPatternInfo.Count > 0)
				return (int)_bossPatternInfo[_comboPattern_current]._repeat; 
			else
				return 0;
		}
	}

	public float CurrentPattern_Cooltime
	{
		get 
		{ 
			if (_bossPatternInfo.Count > 0)
				return _bossPatternInfo[_comboPattern_current]._cool_time; 
			else
				return 0;
		}
	}


	public override ObscuredInt DamageFrequency 
	{
		get 
		{ 
			int frequency = 1; 

//			if (_comboPatterns.Count > 0)
//			{
//				frequency = (int)_comboPatterns[_comboPattern_current]._total_givedamge;
//			}
//
			if (_damage_frequency > 0)
			{
				frequency = _damage_frequency; 
			}

			return frequency;
		} 
	}


	public override float ProjectileRadius 	
	{ 
		get 
		{ 
			if (IsBoss && HavePatternSkill)
			{
				return KnowledgePSkill.ProjectileRadius;
			}
			else
			{
				return _projectileRadius; 
			}
		} 
	}



	protected void initComboPatternSys()
	{
		_patternSequenceIndex = 0; 	//jks current pattern (n th index)

		_comboPattern_current = 0; //_comboPattern_current = _comboPatterns[0];
	}


//	//jks get pattern to use. one out of six patterns.
//	protected int getNextPattern()
//	{
//		int pattern = Utility.getNthDigit(_patternSequence, _patternSequenceIndex);
//
//		if (pattern == 0) // if reached end of pattern sequence, start from beginning.
//		{
//			_patternSequenceIndex = 1; // reset
//			pattern =  Utility.getNthDigit(_patternSequence, _patternSequenceIndex);
//		}
//
//		_patternSequenceIndex ++;
//
//		return (pattern - 1);  // make 1 ~ 6  -->  0 ~ 5  for patterns list
//	}


	//jks get pattern to use. one out of six patterns.
	protected int getNextPattern()
	{
		if (_patternSequenceIndex >= _patternSequence.Count) // if reached end of pattern sequence, start from beginning.
		{
			_patternSequenceIndex = 0; // reset
		}

		int pattern = _patternSequence[_patternSequenceIndex];

		_patternSequenceIndex ++;

		return (pattern - 1);  // make 1 ~ 6  -->  0 ~ 5  for patterns list
	}


//jks 2016.1.5
//	//jks get combo(action) to start in the pattern.
//	protected int getCombo(int comboIndex)
//	{
//		int combo = 1;
//		switch(comboIndex)
//		{
//			case 1:  combo = CurrentComboPattern._attack1;  break;
//			case 2:  combo = CurrentComboPattern._attack2;  break;
//			case 3:  combo = CurrentComboPattern._attack3;  break;
//			case 4:  combo = CurrentComboPattern._attack4; 	break;
//			case 5:  combo = CurrentComboPattern._attack5;  break;
//			case 6:  combo = CurrentComboPattern._attack6;  break;
//			case 7:  combo = CurrentComboPattern._attack7;  break;
//			case 8:  combo = CurrentComboPattern._attack8;  break;
//		}
//
//		Log.nprint("------------getCombo(): "+ combo + "        combo index: "+ comboIndex + "      TotalCombo : "+ TotalCombo);
//
//		return combo;
//	}


	protected void updateCurrentComboPattern()
	{
		_comboPattern_current = getNextPattern();
//jks 2016.1.5		updateAnimCombo();

		Log.nprint("------------_comboPattern_current: "+ _comboPattern_current);
	}



	public override void animationFinished ()
	{
		//if (!isCoolingInProgress() && _recentHitType == eHitType.HT_CRITICAL)   //jks 2014.9.25 if skill is completed OR recent hit type is critical
		if (TotalCombo == ComboCurrent)
		{
			if (_coolTimer != null)
				if (! _coolTimer.IsCoolingInProgress)
					startCoolTimeAndResetComboFlag();
		}

	}



	public override IEnumerator startSkill()
	{
		if (Progress_SkillAnimation) yield break;
		if (Action_Hit) yield break;//jks if (_hitReactionInProgress) return;


		if (IsBoss && _haveSkillPattern)  //jks 만약 보스이고 스킬패턴을 가지고 있으면, 패턴 선택과 애니 셋업.
		{
			if (KnowledgePSkill.Progress_AnyAction) 
				yield break;

			if (CoolTimer.IsCoolingInProgress)
				yield break;


			updateCurrentComboPattern();
//jks 2016.1.5			updateReaction(CurrentPattern_ReactionTypeID);
			KnowledgePSkill.startSkill_Support(_comboPattern_current);
		}
		else
		{
			Action_Combo1 = true;
		}

	}


//	public override void resetActionInfo()
//	{
//		//jks if pattern skill action finished
////		if (KnowledgePSkill && KnowledgePSkill.Progress_AnyAction)
//		if (IsBoss && HavePatternSkill)
//		{
//			forceResetFlags();
//			KnowledgePSkill.resetActionInfo();
//		}
//		else
//		{
//			base.resetActionInfo();
//		}
//
//	}

	public override void resetActionInfo()
	{
		//jks if support skill action finished
		if (KnowledgePSkill && KnowledgePSkill.Progress_AnyAction)
		{
			KnowledgePSkill.resetActionInfo();
		}
		else
		{
			base.resetActionInfo();
		}

	}




	protected override void doNextAction()
	{
		//jks trigger combo
		//Log.nprint(gameObject.name + "doNextAction()       ComboCurrent : " + ComboCurrent + "   TotalCombo : " + TotalCombo + "   _haveSkillPattern: "+_haveSkillPattern);


		if (_haveSkillPattern)
		{
			//jks 2016.1.5			setActionFlag_Pattern();
		}
		else
		{
			setNextComboActionFlag();
		}


		if (NoNextComboAction) //jks if no next action,
		{
//jks 2015.12.10			if (_haveSkillPattern)
//				updateCurrentComboPattern();

			aiMove();

			//if (ComboCurrent != 0) //jks if skill ever initiated? 
			//if (_skillCompleted || (!isCoolingInProgress() && _recentHitType == eHitType.HT_CRITICAL) )  //jks 2014.9.25 if skill is completed OR recent hit type is critical
			if ( _everGaveDamageForTheAttack || (!isCoolingInProgress() && _recentHitType == eHitType.HT_CRITICAL) )  //jks 2016.5.23 : _
			{
//				startCoolTimer();
//
//				_skillCompleted = false;

//				if (IsLivingWeapon)
//					Log.jprint(gameObject.name + "    start cool time: " + AttackCoolTimer.CoolTime);
				startCoolTimeAndResetComboFlag();
			}
		}

	}
	

//jks 2016.1.5
//	public void updateAnimCombo()
//	{
//		Log.nprint(gameObject.name + "   set boss skill anim :   pattern #: "+ _comboPattern_current);
//		setAnimCombo1(CurrentComboPattern._attack1); //jks combo1
//		setAnimCombo2(CurrentComboPattern._attack2); //jks combo2
//		setAnimCombo3(CurrentComboPattern._attack3); //jks combo3
//		setAnimCombo4(CurrentComboPattern._attack4); //jks combo4
//		setAnimCombo5(CurrentComboPattern._attack5); //jks combo5
//		setAnimCombo6(CurrentComboPattern._attack6); //jks combo6
//		setAnimCombo7(CurrentComboPattern._attack7); //jks combo6
//		setAnimCombo8(CurrentComboPattern._attack8); //jks combo6
//	}



//jks 2016.1.5
//	protected virtual void setActionFlag_Pattern()
//	{
//		if (ComboCurrent == 1  && !IsLastComboInCurrentPattern ) { if (getOpponentsToAttack()) Action_Combo2  = true; }
//		else if (ComboCurrent == 2  && !IsLastComboInCurrentPattern ) { if (getOpponentsToAttack()) Action_Combo3  = true; else setZoom(0); } //jsm - reset zoom value 
//		else if (ComboCurrent == 3  && !IsLastComboInCurrentPattern ) { if (getOpponentsToAttack()) Action_Combo4  = true; else setZoom(0); }
//		else if (ComboCurrent == 4  && !IsLastComboInCurrentPattern ) { if (getOpponentsToAttack()) Action_Combo5  = true; else setZoom(0); }
//		else if (ComboCurrent == 5  && !IsLastComboInCurrentPattern ) { if (getOpponentsToAttack()) Action_Combo6  = true; else setZoom(0); }
//		else if (ComboCurrent == 6  && !IsLastComboInCurrentPattern ) { if (getOpponentsToAttack()) Action_Combo7  = true; else setZoom(0); }
//		else if (ComboCurrent == 7  && !IsLastComboInCurrentPattern ) { if (getOpponentsToAttack()) Action_Combo8  = true; else setZoom(0); }
//	}


	protected override void setNextComboActionFlag()
	{
		if (ComboCurrent == -1 && TotalCombo > 1) { if (getOpponentsToAttack()) Action_Combo1 = true;}
		else if (ComboCurrent == 1  && TotalCombo > 1 ) { if (getOpponentsToAttack()) Action_Combo2  = true; }
		else if (ComboCurrent == 2  && TotalCombo > 2 ) { if (getOpponentsToAttack()) Action_Combo3  = true; else setZoom(0); } //jsm - reset zoom value 
		else if (ComboCurrent == 3  && TotalCombo > 3 ) { if (getOpponentsToAttack()) Action_Combo4  = true; else setZoom(0); }
		else if (ComboCurrent == 4  && TotalCombo > 4 ) { if (getOpponentsToAttack()) Action_Combo5  = true; else setZoom(0); }
		else if (ComboCurrent == 5  && TotalCombo > 5 ) { if (getOpponentsToAttack()) Action_Combo6  = true; else setZoom(0); }
		else if (ComboCurrent == 6  && TotalCombo > 6 ) { if (getOpponentsToAttack()) Action_Combo7  = true; else setZoom(0); }
		else if (ComboCurrent == 7  && TotalCombo > 7 ) { if (getOpponentsToAttack()) Action_Combo8  = true; else setZoom(0); }
	}


	#endregion





	void destroyMe()
	{
		Destroy(gameObject);
	}
	

	public override bool canUseSkill()
	{
		if (_bossDialogStagingMode) return false;

		if (Action_AccumulatedHitReaction) return false;

		if (_scanDistance <= 0) return false;

		if (DoNotAttack) return false;


		return base.canUseSkill();

	}

	

	public override float calcHitRate(Knowledge_Mortal_Fighter opponent)
	{
		ObscuredFloat hitRate;
		//jks 2015.8.27 float buffHitRate = calculate_LeaderBuff_HitRate_Opponent();
		//jks 2015.5.8 remove leader strategy-	float leaderStrategy = calculate_LeaderStrategy_HitRate();

		hitRate = base.calcHitRate(opponent);//jks 2015.8.27 :  + buffHitRate; //jks 2015.5.8 remove leader strategy-	 + leaderStrategy;
		//Log.jprint("********* calcHitRate()    buff: "+ buffHitRate+"    leader strategy: "+leaderStrategy);

		ObscuredFloat hitRateDebuff = getLeaderBuffCriticalDown (hitRate);

		hitRate -= hitRateDebuff;

		#if UNITY_EDITOR
		if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
		{
			Log.print_always(" 적 : " + gameObject.name +  "   + 리더버프: -"+hitRateDebuff  + " = " + "  최종 적중률: " + hitRate);
		}
		#endif

		return hitRate;
	}


	protected bool checkReactionRate(Knowledge_Mortal_Fighter opponetKnowledge)
	{
		int pick = Random.Range(0, 99);

		// hit by "quick skill".
		if (opponetKnowledge.Progress_Action_Quick)
		{
			if (opponetKnowledge.ComboCurrent >= ReactionJudgeStartCombo_QS)
			{
				return opponetKnowledge.amIMelee() ? (pick <= ReactionRate_QuickSkill_Melee) : (pick <= ReactionRate_QuickSkill_Distant);
			}
		}
		else // hit by "skill".
		{
			return pick <= ReactionRate;
		}

		return false;
	}




//	public override int AttackPoint 
//	{ 
//		get
//		{
//			if (_count_special_opponent > 0)
//			{
//				float specialAttackRate = getAttackRate_SpecialOpponent();
//				if (specialAttackRate == float.MinValue)
//					return _attackPoint;
//
//				return (int)(_attackPoint * specialAttackRate); //jks use special attack.
//			}
//			else
//			{
//				if (IsBoss && CurrentPattern_AttackPoint > 0)
//				{
//					return CurrentPattern_AttackPoint;
//				}
//				else
//					return _attackPoint;  //jks use original.
//			}
//		}
//	}


	public override ObscuredInt AttackPoint 
	{ 
		get
		{
			ObscuredInt distributedAttackPoint = _attackPoint;
			
			if (DamageFrequency > 1)// if multiple attack skill?
			{
				distributedAttackPoint = Mathf.RoundToInt(_attackPoint / DamageFrequency);  //jks 2015.11.23:  new calc
				distributedAttackPoint += Mathf.RoundToInt(distributedAttackPoint * BattleTuning.Instance._multipleAttackSkillAdjustmentFactor);
			}
			
			if (_count_special_opponent > 0)
			{
				ObscuredFloat specialAttackRate = getAttackRate_SpecialOpponent();
				if (specialAttackRate == float.MinValue)
					return distributedAttackPoint;
				
				return Mathf.RoundToInt(distributedAttackPoint * specialAttackRate); //jks use special attack.
			}
			else
			{
//jks 2015.12.10				if (IsBoss && CurrentPattern_AttackPoint > 0)
//				{
//					return CurrentPattern_AttackPoint;
//				}
//				else
					return distributedAttackPoint;
			}
		}
	}




	public override void giveDamage(float reactionDistanceOverride)
	{		

		if (KnowledgePSkill && KnowledgePSkill.Progress_AnyAction)
		{
			KnowledgePSkill.giveDamage(reactionDistanceOverride);
			return;
		}

		_everGaveDamageForTheAttack = true;
		
		if (CameraManager.Instance == null) return;

		//jks 2016.3.14 skill buff 기능 추가.
		addSkillBuff();

		
		//		if (gameObject.name.Contains("C"))
		//			Log.jprint(Time.time+": "+ gameObject.name + ". . . . . giveDamage(): ");
		
		_skillCompleted = isLastDamageInSkill(reactionDistanceOverride);
		
		GameObject closestOpponent = getOpponentsInScanDistance_WeaponPositionBased();
		if (closestOpponent == null) return;
		
		//		if (gameObject.name.Contains("C"))
		//			Log.jprint(Time.time+": "+ gameObject.name + ". # . # . closestOpponent: " + closestOpponent);
	
		updateTotalGiveDamageCount();

		Knowledge_Mortal opponentKnowledge = closestOpponent.GetComponent<Knowledge_Mortal>();


		#if UNITY_EDITOR
		//jks debugging gizmo -----------------------
		_meleeAttack_ShowAttackRange = true;			
		_meleeAttack_Center_Opp = closestOpponent.transform.position;
		_meleeAttack_Radius_Opp = opponentKnowledge.Radius;
		//jks debugging gizmo -----------------------
		#endif

		
		
		float finalDistToCheck = Mathf.Abs(transform.position.x - closestOpponent.transform.position.x) - this.Radius - opponentKnowledge.Radius - _weaponLength;


		//Log.jprint(gameObject.name + "      ~~~~~~~~~~~~ finalDistToCheck: " + finalDistToCheck);

		
		if (finalDistToCheck > 0.3f)
			return;
		
		//jks		foreach(GameObject go in _opponentsInAttackRange)
		foreach(FighterActor ca in Opponents)
		{
			giveDamageTo(ca._go, finalDistToCheck, reactionDistanceOverride);
		}

		if (BattleBase.Instance.Protectee != null)
		{
			giveDamageTo(BattleBase.Instance.Protectee, finalDistToCheck, reactionDistanceOverride);
		}
	}



	private void giveDamageTo(GameObject go, float finalDistToCheck, float reactionDistanceOverride)
	{
		if (go == null) return;
		if (!go.activeSelf) return;
		
		//jks check damage range
		Knowledge_Mortal opponentKnowledge = go.GetComponent<Knowledge_Mortal>();
		if (opponentKnowledge == null) return;
		
		finalDistToCheck = Mathf.Abs(transform.position.x - go.transform.position.x) - this.Radius - opponentKnowledge.Radius - _weaponLength;
		
		
		if (go != getCurrentTarget())
		{
			if (finalDistToCheck > 0.1f + _damageRange) 
				return;
		}
		
		if (checkHeightIfReachable(go) == false) return;
		
		//Log.jprint(go + "      xxxxxxxxxxxx take damage ");
		
		
		Knowledge_Mortal_Fighter knowledgeOpponent = go.GetComponent<Knowledge_Mortal_Fighter>();
		
		#if UNITY_EDITOR
		if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
		{
			Log.print_always("   --------------------------------- 적 ---------------------------------");
		}
		#endif
		
		eHitType hitType = getFinalHitType(knowledgeOpponent);
		
		int hitReactionAnimID = getReaction(hitType);
		
		ObscuredInt finalAttack = AttackPoint;
		
		if (knowledgeOpponent.IsLeader || TestOption.Instance()._classRelationBuffAll)
		{
			ObscuredInt classRelationAttackPoint = calculate_ClassRelation_AttackPoint(AttackPoint, knowledgeOpponent);
			//jks 2015.8.26 no more: int leaderBuffAttackPoint = calculate_LeaderBuff_AttackPoint_Opponent();
			//jks 2015.5.8 remove leader strategy-				int leaderStrategyAttack = calculate_LeaderStrategy_AttackPoint();
			
			finalAttack = AttackPoint + classRelationAttackPoint;//jks 2015.8.26 no more:  + leaderBuffAttackPoint;//jks 2015.5.8 remove leader strategy-	 + leaderStrategyAttack;
			
			
			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
			{
				//Log.print("   --------------------------------- 적 ---------------------------------");
				if (BattleBase.Instance.LeaderTransform)
					Log.print_always("   현재 리더 클래스: "+ BattleBase.Instance.LeaderClass + "   :  " + BattleBase.Instance.LeaderTransform.gameObject.name);
				Log.print_always("   공격자 : " + gameObject.name + "  -->  피해자: " + knowledgeOpponent.name);
				Log.print_always("   공격자 클래스 : " + Class + "  -->  피해자 클래스: " + knowledgeOpponent.Class + "   피격 타입: " + hitType);
				Log.print_always("   G I V E  D A M A G E      기본 공격력: " + AttackPoint + "  +  클래스상성 공격력: " + classRelationAttackPoint + "  =  " + finalAttack);
			}
			#endif
		}
		else
		{
			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
			{
				Log.print_always("   공격자 : " + gameObject.name + "  -->  피해자: " + knowledgeOpponent.name);
				Log.print_always("   피격 타입: " + hitType);
				Log.print_always("   G I V E  D A M A G E      기본 공격력: " + finalAttack);
			}
			#endif
		}
		
		knowledgeOpponent.takeDamage(finalAttack, hitReactionAnimID, hitType, AttackType, _weaponType_ForAnimation, gameObject, reactionDistanceOverride);

	}
	



	
	public override ObscuredInt takeDamage(ObscuredInt damagePoint, int reactionAnimID, eHitType hitType, eAttackType attackType, eWeaponType_ForAnimation weaponType, GameObject attacker,  float reactionDistanceOverride)
	{
		if (IsDead) return damagePoint;

		if (AllyID == eAllyID.Ally_Human_Me && BattleBase.Instance.IsInvincibleMode)
		{
			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
				Log.print_always(gameObject.name +" -------  팀원 무적 상태 ---------- damage : 0  ");
			#endif

			return 0;
		}


//jks 2015.11.4 보스액션 제거.		if (IsBoss && !_bossActionDone) return 0;

//jks 2015.07.28: design change: no more no damage period advantage.
//		if (_counterAttackInProgress)
//		{
//			#if UNITY_EDITOR
//			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
//				Log.print_always("   N O     D A M A G E  -  Counter attack in progress...   피해자: " + gameObject);
//			#endif
//			return damagePoint;
//		}

		_recentHitType = hitType; ///jks save recent hit type

		//jsm_0915
		Attacker = attacker; // <--- Knowledge_Mortal_Fighter::takeDamage 로 이동. //jks

		IsLastDamageInSkill = isLastDamageInSkill(reactionDistanceOverride, attacker); //jks reset	

//jks 2015.4.13 -		if (!IsLastDamageInSkill && hitType != eHitType.HT_CRITICAL) //jks 스킬 마지막 타이고 크리티컬이면 뒤로 밀리는 부분 생략.- (크리티컬 리액션은 root motion 사용하기 때문에 따로 미는 부분 생략.) 	
		applyReactionDistance(reactionDistanceOverride);


		Knowledge_Mortal_Fighter opponentKnowledge = attacker.GetComponent<Knowledge_Mortal_Fighter>();
		AppliedSkillBuff_UID = opponentKnowledge.MyCurrentSkillBuff_UID;

		if (hitType == eHitType.HT_BAD)
		{
//			if ( ! IsLivingWeapon && !IsMinion) 
//				if (BattleUI.Instance() != null)
//					BattleUI.Instance().addDemageUI(transform, 0, hitType);

			//jsm - show bad effect
//			eAttackType attackType = opponentKnowledge.AttackType;
//			eWeaponType_ForAnimation weaponType = opponentKnowledge._weaponType_ForAnimation;
			base.spawnDamageEffect( attacker, attackType, hitType, weaponType);

			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
				Log.print_always("   N O     D A M A G E  -  hit type: "+ hitType + "   피해자: " + gameObject.name);
			#endif

			return 0;
		}

		if (checkAdditionalSkillFunction(hitType, attacker))
		{
			damagePoint += Mathf.RoundToInt((float)damagePoint * 0.5f);
			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
				Log.print_always("   추가 스킬 기능으로 인한 공격 50% 가산 damagePoint : "+ damagePoint + "   피해자: " + gameObject.name);
			#endif
		}

		damagePoint = base.takeDamage(damagePoint, reactionAnimID, hitType, attackType, weaponType, attacker, reactionDistanceOverride);

		//if (IsBoss && Progress_Skill) return; //jks if Boss and while doing skill action, do not make it be hit. 

		//jks 승리 조건 갱신.
		if (IsBoss)
		{
			if (BattleBase.Instance.isThisVictoryCondition(eBattleVictoryConditionIndex.BVC_BossHPReduction) ||
				BattleBase.Instance.haveBossDialog2(BattleBase.Instance.StoryBoss)
			)
			BattleBase.Instance.updateBossHPReduction(this);
		}


		//jks 클래스 매치 공격 받은 경우.
		if (BattleBase.Instance.ProgressClassMatchAttack) //jks 클래스 매치 공격이면 , 리액션.
		{
			if (hitType == eHitType.HT_CRITICAL &&
				BattleBase.Instance.ProgressClassMatchAttack &&
				_animHitNumber > 0
			)
				goHitReaction();
			else
				hitShake();		
		}
		//jks 반격 할 수 있는지 확인.
		else if (	checkCounterAttack(hitType) && 
					checkEnemyOnPath(false) &&
					! isCoolingInProgress())
		{
			if (!Action_Block && !AmICaptured)
			{
				if (IsBoss)
				{
					if (! KnowledgePSkill.Progress_AnyAction)
					{
						Action_Block = true;
					}
				}
				else
				{
					Action_Block = true;
				}


				hitShake();

				return 0;
			}
		}
		else
		{
			//jks 2015.12.22 : 보스는 크리티컬일 때만 hit 리액션하고, 일반 적은 막타에서 리액션 하게 수정.
			if (IsBoss)
			{
				if ((hitType == eHitType.HT_CRITICAL &&
					! KnowledgePSkill.Progress_AnyAction) && 
					!(Progress_SkillAction && ReactionOff) &&  //jks 2016.3.9 실행 중인 스킬 info 에 리액션 하지 않는 것으로 설정되어 있으면, 크리티컬이라도 리액션 하지 않음.
					!(KnowledgeMultiSkill != null && KnowledgeMultiSkill.Progress_Action && KnowledgeMultiSkill.ReactionOff) && //jks 2016.3.14 리더지원 스킬과 보스패턴 스킬도 reaction off 설정 적용.
					_animHitNumber > 0
				)
				{
					if (!Action_AccumulatedHitReaction)
					{
						goHitReaction();
					}
				}
				else
					hitShake();		
			}
			else
			{
				if (
					(IsLastDamageInSkill 					// 막타 이거나,
					|| !Progress_SkillAction 				// 스킬 사용 중이 아니거나,
					|| AttackCoolTimer.IsCoolingInProgress  // 쿨링 중이거나,
					|| Action_Idle 							// idle 중이거나,
					|| checkReactionRate(opponentKnowledge))
					
					&& !(Progress_SkillAction && ReactionOff)  //jks 2016.3.9 실행 중인 스킬 info 에 리액션 하지 않는 것으로 설정되어 있으면, 크리티컬이라도 리액션 하지 않음.
					&& !(KnowledgeMultiSkill != null && KnowledgeMultiSkill.Progress_Action && KnowledgeMultiSkill.ReactionOff //jks 2016.3.14 리더지원 스킬과 보스패턴 스킬도 reaction off 설정 적용.
						&& _animHitNumber > 0
					)
				)// 
				{
					if (!Action_AccumulatedHitReaction)
					{
						goHitReaction();
					}
				}
				else
				{
					hitShake();
				}
			}

			if (DoIHoldOpponent)
			{
				releaseOpponent();
			}

			
//jks 2015.12.10			if (_haveSkillPattern)
//				updateCurrentComboPattern();
		}

		//Log.jprint(gameObject.name + "^^^^^^^^^^^^^^^^^ takeDamage() hit type : " + hitType);


//		if ( ! IsLivingWeapon)
//			updateDamageUI(transform, damagePoint, hitType);

		//140326 jsm
		if (hitType == eHitType.HT_CRITICAL || hitType == eHitType.HT_GOOD)
		{
			if (!BattleBase.Instance.IsSkillStagingInProgress) 
			{
				if (IsBoss)
				{
					if (TestOption.Instance()._isUseBossCamShake)
						CameraShake.Shake(1, Vector3.one, new Vector3(0.1f, 0.1f, 0.1f), 0.2f, 70.0f, 0.2f, 1.0f, false);
				}else{
					if (TestOption.Instance()._isUseMobCamShake)
						CameraShake.Shake(1, Vector3.one, new Vector3(0.1f, 0.1f, 0.1f), 0.2f, 70.0f, 0.2f, 1.0f, false);
				}
					
			}
			if (hitType == eHitType.HT_CRITICAL)
				BattleBase.Instance.TotalCritical ++;
		}


		alignPositionZ(transform, attacker.transform.position.z);


		return damagePoint;
	}





	public override void startCoolTimer()
	{
		if (_coolTimer.IsCoolingInProgress)
			return;

		float cool = _coolTime;

//		if (IsBoss)
//			Log.jprint(gameObject.name + "   $ $ $ startCoolTimer()  pattern : "+ _comboPattern_current 
//				+ "    CurrentPattern_Cooltime: " + CurrentPattern_Cooltime + " sec" + " repeat : " + CurrentPattern_Repeat); 

		if (IsBoss && HavePatternSkill)
		{
			cool = CurrentPattern_Cooltime;
		}

		_coolTimer.activateTimer(cool);


		//play Idle/Cool animation during cooltime
		//jks stop freezing - forceResetFlags();
		Action_Idle = true;
	}



	protected override void updateDamageUI(Transform tran, ObscuredInt dmg, eHitType type)
	{
		if (IsMinion) return;

		base.updateDamageUI(tran, dmg, type);
	}


	protected bool checkAdditionalSkillFunction(eHitType hitType, GameObject attacker)
	{
		if (hitType != eHitType.HT_CRITICAL) return false;

		SkillAgent[] sas = GetComponents<SkillAgent>();

		if (sas.Length == 0) return false;

		SkillAgent theAgent = null;
		foreach (SkillAgent sa in sas)
		{
			if (sa.IsReadyToGiveExtraDamage) //jks 추가 데미지 조건이 되는 agent 가 하나라도 발견되면, 
			{
				theAgent = sa;
				break;
			}
		}

		if (theAgent == null) return false;

		//jks do extra damage
		Action_Hit = false;
		Action_AccumulatedHitReaction = true;

		Anim_AccumulatedHitReaction = animAccumulatedHitReaction(theAgent.AnimNum_AccumulatedHitReaction);

		Log.jprint(Time.time + "   Anim_AccumulatedHitReaction hash: "+ Anim_AccumulatedHitReaction + "    anim num: "+ theAgent.AnimNum_AccumulatedHitReaction);

		//jks reset count
		theAgent.resetHitCount();


		return true;
	}


	public override void forceResetFlags()
	{
//		if (IsBoss)
//			Log.jprint(Time.time +" : "+ gameObject.name + " ****************       forceResetFlags()");

		base.forceResetFlags();

		_accumulatedHitReaction = false;
		_accumulatedHitReactionInProgress = false;


		if (KnowledgePSkill && KnowledgePSkill.Progress_AnyAction)
			KnowledgePSkill.forceResetComboFlags();

//jks 2015.11.4 보스액션 제거.		_actionBoss = false;
//jks 2015.11.4 보스액션 제거.		_actionBossInProgress = false;
	}



	protected virtual void alignPositionZ(Transform trm, float z)
	{
		Vector3 newPosition = trm.position;
		newPosition.z = z;
		trm.position = newPosition;
	}


	//jks 2015.07.28: design change: no more no damage period advantage.
//	protected void resetCounterAttackFlag()
//	{
//		_counterAttackInProgress = false;
//		//Log.jprint(Time.time + " : " +  gameObject.name + " < < < < < < < < < Counter Attack : Block End  +++++++++++++++");
//	}



	protected bool checkCounterAttack(eHitType hitType)
	{
		if (!_counterAttackOn) return false;

		if (Progress_SkillAnimation) return false;

		float randomNumber = Random.Range(0.0f, 1.0f);

		if (randomNumber < _counterAttackRate[(int)hitType])
		{
			//Log.jprint(gameObject.name + "checkCounterAttack() : success.");
			return true;
		}

		return false;
	}


	protected bool isInCameraView()
	{
		return Mathf.Abs(transform.position.x - CameraManager.Instance._targetCamera.transform.position.x) < 7;
	}



	public override bool getOpponentsInScanDistance(bool bQuickSkill)
	{
		if (BattleBase.Instance.EnemyToKill == 0 && BattleBase.Instance.isThisVictoryCondition(eBattleVictoryConditionIndex.BVC_KillBoss))
		{
			//showWeapon(false);
			return false;
		}

		GameObject closestOpponent = null;

		if (BattleBase.Instance.IsPVP)
		{
			closestOpponent = getCurrentTarget();
		}
		else
		{
//			if (gameObject.name.Contains("FX"))
//				Log.jprint(gameObject.name + "        findClosestOpponent()   AllyID: " + AllyID);
			closestOpponent = BattleBase.Instance.findClosestOpponent(AllyID, transform.position.x);
		}

		setTarget(closestOpponent);

		if (closestOpponent == null) 
		{
			if (IsLivingWeapon && !isInCameraView())
			{
				Action_Paused = true; // do nothing
				Destroy(gameObject, 0.5f);
			}
			return false;
		}

		Knowledge_Mortal_Fighter opponentKnowledge = closestOpponent.GetComponent<Knowledge_Mortal_Fighter>();
		
		float distShell_AttackerAndClosestOpponent = Mathf.Abs(transform.position.x - closestOpponent.transform.position.x) - this.Radius - opponentKnowledge.Radius;
		
		
		//_isTargetInShowWeaponRange = (distShell_AttackerAndClosestOpponent < _attackDistanceMax + 4); //jks too show weapon if target is closer than 3 meter.
		//showWeapon(_isTargetInShowWeaponRange);
		
		//if (_attackDistanceMax + Random.Range(-0.5f, 0) < distShell_AttackerAndClosestOpponent) return false;
		if (AttackDistanceMax < distShell_AttackerAndClosestOpponent) return false;

		//Log.jprint(gameObject.name + "      ____________ distShell: " + distShell_AttackerAndClosestOpponent);

		return true;
	}


	public bool EverGaveDamage
	{  
		get {
			if ( HavePatternSkill )
				return KnowledgePSkill.EverGaveDamage;
			else
				return _everGaveDamageForTheAttack;
		}
	}

	protected void checkMinionSpawnerActivation()
	{
		if (EverGaveDamage)
		{
			//jks if the boss has a minion spawner, activate it now.
			MinionSpawner mSpawner = GetComponent<MinionSpawner>();
			if (mSpawner != null)
				mSpawner.activate();
	
			CancelInvoke("checkMinionSpawnerActivation");
		}
	}

//jks 2015.11.4 보스액션 제거.--------------------------------------------------->
//	protected bool _bossActionDone = false;
//
//	public bool WaitForBossAction { get { return ! _bossActionDone; }}
//
//	protected override void checkBossAction()
//	{
//		if (_bossActionDone) return;
//
//		if (! BattleBase.Instance.HaveStoryBossAndDialogPassed) return; //jks boss dialog 끝나길 기다림.
//
//		if (!IsBoss || BossActionAnimNumber == 0)
//		{
//			_bossActionDone = true;
//			return;
//		}
//
//		float dist = Mathf.Abs(transform.position.x - BattleBase.Instance.LeaderTransform.position.x);
//		
//		float distShell = dist - Radius - BattleBase.Instance.LeaderTransform.GetComponent<Knowledge_Mortal_Fighter>().Radius;
//
//		if (distShell < 2.2f)
//		{
//			if (BattleBase.Instance.CanStartBossAction)
//			{
//				//BattleBase.Instance.IsIgnoreButtonTouch = true;
//				BattleBase.Instance.IsSkillStagingInProgress = true;
//				BattleBase.Instance.PauseFighters(true);
//				
//				//jks start staging action
//				forceResetFlags();
//				Action_Boss = true;
//				_bossActionDone = true;
//				setBossActionCamera();
//				//jks reset flag that did not get "onTouchUp" event because of the boss action. (this prevent leader run after boss action.
//				BattleBase.Instance.ForceMoveLeaderRun = false;
//				//jks if the boss has a minion spawner, activate it now.
//				MinionSpawner mSpawner = GetComponent<MinionSpawner>();
//				if (mSpawner != null)
//					mSpawner.activate();
//			}
//			else
//			{
//				forceResetFlags();
//				Action_Idle = true;
//			}
//		}
//	}
//jks <---------------------------------------------------------------------

//jks 2015.11.4 보스액션 제거.
//	//jsm_150703
//	GameObject _gob_bossActionCam;
//	GameObject _gob_bossActionCamRoot;
//	protected void setBossActionCamera()
//	{
//		Utility.setLayerAllChildren(gameObject, "Boss");
//		//Utility.changeShader (gameObject, "Toon/Basic", true, "Effect");
//		Utility.changeShader (gameObject, BattleBase.Instance._toonBasic, true, "Effect");
//
//		_gob_bossActionCamRoot = new GameObject();
//		_gob_bossActionCamRoot.name = "Boss Event Camera";
//		_gob_bossActionCamRoot.transform.parent = transform;
//		_gob_bossActionCamRoot.transform.position = new Vector3(-0.01f,0,-0.01f);
//		_gob_bossActionCamRoot.transform.localPosition = Vector3.zero;
//		_gob_bossActionCamRoot.transform.localRotation = Quaternion.Euler(0, 0, 0);
//		float scalePer = GetComponent<Knowledge_Mortal_Fighter>().Height / 1.8f;
//		_gob_bossActionCamRoot.transform.localScale = new Vector3(scalePer, scalePer, scalePer);
//
//		_gob_bossActionCam = Instantiate(BattleUI.Instance()._gob_swapCameraForBoss) as GameObject;
//		Camera bossCam = _gob_bossActionCam.GetComponent<Camera>();
//		bossCam.cullingMask = 1 << LayerMask.NameToLayer ("Boss") | 1 << LayerMask.NameToLayer ("EventFx");
//		
//		_gob_bossActionCam.transform.parent = _gob_bossActionCamRoot.transform;
//		_gob_bossActionCam.transform.position = Vector3.zero;
//		
//		setEventCameraClip( _gob_bossActionCam );
//		BattleUI.Instance ()._isCutEffLabel = true;
//		BattleUI.Instance()._cam_enchant.enabled = false;
//		BattleBase.Instance.IsIgnoreButtonTouch = true;
//
//		BattleManager.Instance.setEventBossScale(gameObject, false);
//		CutEff.Instance.waitForInitEff ();
//	}

//jks 2015.11.4 보스액션 제거.
//    protected void setEventCameraClip(GameObject obj)
//    {
//        AnimationClip clip = null;
//        Table_Enemy tableEnemy = (Table_Enemy)TableManager.GetContent(BotID);
//
//        Animator anime = obj.GetComponent<Animator>();
//        RuntimeAnimatorController myController = anime.runtimeAnimatorController;
//        AnimatorOverrideController myOverrideController = new AnimatorOverrideController();
//        myOverrideController.name = "camera walk";
//        myOverrideController.runtimeAnimatorController = myController;
//        StartCoroutine(ResourceManager.co_GetResourceFromBundle("AnimationClip/BossAction/bossAction_" + tableEnemy._bossAction, result =>
//        {
//            clip = (AnimationClip)result;
//            myOverrideController[myOverrideController.clips[0].originalClip.name] = clip;
//            anime.runtimeAnimatorController = myOverrideController;
//        }));
//		obj.camera.fieldOfView = 24.0f;
//	}

//	protected void setBossCamFov(float fov)
//	{
//		_gob_bossActionCam.GetComponent<Camera>().fieldOfView = fov;
//	}


	protected override void LateUpdate()
	{
		if (Progress_SkillAnimation
			|| (KnowledgePSkill && KnowledgePSkill.Progress_AnyAction) )
		{
			//Profiler.BeginSample("000");
			pushOpponentWhenIAttack();
			//Profiler.EndSample();
		}
//		else if (Action_Run || Action_Walk || Action_WalkFast)
//		{
//			//GameObject target = getCurrentTarget();
//			pushPassedOpponentsWhenIMove();
//
//		}

		pauseProcess();

		//jks 2015.11.4 보스액션 제거.		checkBossAction();
	}



	protected override void keepOpponentAtWeaponEnd(GameObject opponent, float penetration)
	{
		if (KnowledgePSkill && KnowledgePSkill.Progress_AnyAction)
		{
			KnowledgePSkill.keepOpponentAtWeaponEnd(opponent, penetration);
			return;
		}

		base.keepOpponentAtWeaponEnd(opponent, penetration);
	}




	protected override void pushPassedOpponents()
	{
		//if (AmICaptured) return;  // if i m captured, do not push.
		if (IsDead) return;
		if (AmICaptured) return;

		if (IsLivingWeapon)
			pushPassedOpponents_summonedWeapon();
		else
		{
			foreach(FighterActor ca in Opponents)
			{
				GameObject go = ca._go;
				
				if (go == null) continue;
				if (!go.activeSelf) continue;
				if (go == _captured_target) continue; // do not push whatever i hold.
				
				Knowledge_Mortal_Fighter goKnowledge = go.GetComponent<Knowledge_Mortal_Fighter>();
				if (goKnowledge == null) continue;
				if (goKnowledge.AllyID == AllyID) continue;
				if (goKnowledge.IsDead) continue;
				if (goKnowledge.AmIWall) continue;
				if (goKnowledge.AmICaptured) continue;
				
				if (transform.forward.x > 0)
				{
					if (transform.position.x > go.transform.position.x) //jks passed
					{
						keepOpponentAtWeaponEnd(go, 0.3f);
					}
				}
				else
				{
					if (transform.position.x < go.transform.position.x) //jks passed
					{
						keepOpponentAtWeaponEnd(go, 0.3f);
					}
				}
			}
		}
	}


	protected override void pushPassedOpponentsWhenIMove()
	{
		if (IsDead) return;
		if (!IsBattleTimeStarted) return;
		if (AmICaptured) return;
		
		if (IsLivingWeapon)
			pushPassedOpponentsWhenIMove_summonedWeapon();
		else
		{
			foreach(FighterActor ca in Opponents)
			{
				GameObject go = ca._go;
				
				if (go == null) continue;
				if (!go.activeSelf) continue;
				if (go == gameObject) continue;
				if (go == _captured_target) continue; // do not push whatever i hold.
				
				Knowledge_Mortal_Fighter goKnowledge = go.GetComponent<Knowledge_Mortal_Fighter>();
				if (goKnowledge == null) continue;
				if (goKnowledge.AllyID == AllyID) continue;
				if (goKnowledge.IsDead) continue;
				if (goKnowledge.AmIWall) continue;
				if (goKnowledge.AmICaptured) continue;
				
				if (transform.forward.x > 0)
				{
					if (transform.position.x > go.transform.position.x) //jks passed
					{
						keepOpponentAtShellEnd(go, 0.3f);
					}
				}
				else
				{
					if (transform.position.x < go.transform.position.x) //jks passed
					{
						keepOpponentAtShellEnd(go, 0.3f);
					}
				}
			}
		}
	}




	protected void pushPassedOpponents_summonedWeapon()
	{
		//if (AmICaptured) return;  // if i m captured, do not push.
		if (IsDead) return;
		if (AmICaptured) return;
		
		if (BattleBase.Instance.isSkillPreviewMode())
		{
			GameObject targetObject =  GameObject.Find("Dummy");
			if (transform.forward.x > 0)
			{
				if (transform.position.x > targetObject.transform.position.x) //jks passed
				{
					keepOpponentAtWeaponEnd(targetObject, 0.3f);
				}
			}
			else
			{
				if (transform.position.x < targetObject.transform.position.x) //jks passed
				{
					keepOpponentAtWeaponEnd(targetObject, 0.3f);
				}
			}
			return;
		}

		foreach(FighterActor ea in Opponents)
		{
			GameObject go = ea._go;
			
			if (go == null) continue;
			if (!go.activeSelf) continue;
			if (go == gameObject) continue;
			if (go == _captured_target) continue; // do not push whatever i hold.
			
			Knowledge_Mortal_Fighter goKnowledge = go.GetComponent<Knowledge_Mortal_Fighter>();
			if (goKnowledge == null) continue;
			if (goKnowledge.AllyID == AllyID) continue;
			if (goKnowledge.IsDead) continue;
			if (goKnowledge.AmIWall) continue;
			if (goKnowledge.AmICaptured) continue;
			
			if (transform.forward.x > 0)
			{
				if (transform.position.x > go.transform.position.x) //jks passed
				{
					keepOpponentAtWeaponEnd(go, 0.3f);
				}
			}
			else
			{
				if (transform.position.x < go.transform.position.x) //jks passed
				{
					keepOpponentAtWeaponEnd(go, 0.3f);
				}
			}
		}
	}


	protected void pushPassedOpponentsWhenIMove_summonedWeapon()
	{
		if (IsDead) return;
		if (!IsBattleTimeStarted) return;
		if (AmICaptured) return;

		if (BattleBase.Instance == null) return;


		if (BattleBase.Instance.isSkillPreviewMode())
		{
			GameObject targetObject =  GameObject.Find("Dummy");
			if (targetObject == null) return;

			if (transform.forward.x > 0)
			{
				if (transform.position.x > targetObject.transform.position.x) //jks passed
				{
					keepOpponentAtShellEnd(targetObject, 0.3f);
				}
			}
			else
			{
				if (transform.position.x < targetObject.transform.position.x) //jks passed
				{
					keepOpponentAtShellEnd(targetObject, 0.3f);
				}
			}
			return;
		}

		
		foreach(FighterActor ea in Opponents)
		{
			GameObject go = ea._go;
			
			if (go == null) continue;
			if (!go.activeSelf) continue;
			if (go == gameObject) continue;
			if (go == _captured_target) continue; // do not push whatever i hold.
			
			Knowledge_Mortal_Fighter goKnowledge = go.GetComponent<Knowledge_Mortal_Fighter>();
			if (goKnowledge == null) continue;
			if (goKnowledge.AllyID == AllyID) continue;
			if (goKnowledge.IsDead) continue;
			if (goKnowledge.AmIWall) continue;
			if (goKnowledge.AmICaptured) continue;
			
			if (transform.forward.x > 0)
			{
				if (transform.position.x > go.transform.position.x) //jks passed
				{
					keepOpponentAtShellEnd(go, 0.3f);
				}
			}
			else
			{
				if (transform.position.x < go.transform.position.x) //jks passed
				{
					keepOpponentAtShellEnd(go, 0.3f);
				}
			}
		}
	}



	public override void playSound(string key)
	{
		base.playSound(key);
		
	}
	
	public override void showEffect(string key)
	{
		base.showEffect(key);
	}

	public override void setTrail(bool isTrail)
	{
		base.setTrail(isTrail);
	}

	public override void spawnEffectAtTarget(string key)
	{
		base.spawnEffectAtTarget(key);
	}

	public override void deSpawnEffectAtTarget(string key)
	{
		base.deSpawnEffectAtTarget(key);
	}

}
