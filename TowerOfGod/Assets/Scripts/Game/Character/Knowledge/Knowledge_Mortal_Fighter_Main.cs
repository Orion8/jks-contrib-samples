using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;


public class Knowledge_Mortal_Fighter_Main : Knowledge_Mortal_Fighter
{
	
	//jks send use card finish event whoever want to know about it.
	public delegate void SkillFinishedEvent(Knowledge_Mortal_Fighter_Main knowledge);
	public event SkillFinishedEvent _onSkillFinished = null;

	public void registerSkillFinishedEvent(SkillFinishedEvent func)   { _onSkillFinished += func; }
	public void unRegisterSkillFinishedEvent(SkillFinishedEvent func) { _onSkillFinished -= func; }
	

	protected bool _leader;
	public override bool IsLeader { get { return (_leader || _leader_P2); } }
	public virtual void setLeader(bool value) 
	{ 
		_leader = value; 
	} 

	protected bool _leader_P2;
	//public bool IsLeader_P2 { get { return _leader_P2; } }
//	public void setLeader_P2(bool value) 
//	{ 
//		_leader_P2 = value; 
//	} 

	protected override List<FighterActor> Opponents { get { return BattleBase.Instance.List_Enemy; }}


	protected Launcher _launcher;
	protected virtual IEnumerator setLauncher() { yield break; }


	public int[,] _anim_reaction_quickSkill = new int[6,7]{{0,0,0,0,0,0,0},{0,0,0,0,0,0,0},{0,0,0,0,0,0,0},{0,0,0,0,0,0,0},{0,0,0,0,0,0,0},{0,0,0,0,0,0,0}}; //jks _anim_reaction_quickSkill[1,] : combo2 reaction animation controller surfix number.

	protected int _currentCombo_QuickSkill;
	public int ComboCurrent_QuickSkill { get { return _currentCombo_QuickSkill ; } set {_currentCombo_QuickSkill  = value; } }

	protected bool _quickSkillInputFromUser = false;

	protected bool _quickSkillInputWindow_Open = false;

	protected float _attack_distance_max_QuickSkill = 1.0f;
	public float AttackDistanceMax_QuickSkill { get { return _attack_distance_max_QuickSkill; } }

	protected int _autoComboStart_QuickSkill = 4;  
	public bool IsAutoCombo_QuickSkill { get { return ComboCurrent_QuickSkill >= _autoComboStart_QuickSkill; }}

	//jks 평타 리더 AI
	protected bool _ai_auto_QuickSkill = false;
	public bool AI_AutoCombo_QuickSkill { get { return _ai_auto_QuickSkill ; } set {_ai_auto_QuickSkill  = value; } }

	protected int _totalCombo_QuickSkill;
	public int TotalCombo_QuickSkill { get { return _totalCombo_QuickSkill; } }


	protected ObscuredInt _attackPoint_QuickSkill;
	public ObscuredInt AttackPoint_QuickSkill { get { return _attackPoint_QuickSkill; }}

	protected ObscuredInt _attackPointFinal_QuickSkill;
	public ObscuredInt AttackPointFinal_QuickSkill { get { return _attackPointFinal_QuickSkill; }}

	
	protected static int _continuousHitCount_QuickSkill = 0;
	public override void incrementContinuousHitCount() 
	{ 
		if (!IsLeader || AllyID != eAllyID.Ally_Human_Me) return;

		_continuousHitCount_QuickSkill++; 
		BattleUI.Instance().offComboInfo();
		BattleUI.Instance().setComboInfo(_continuousHitCount_QuickSkill);
		Invoke("offcomboInfo", 2.5f);

		BattleBase.Instance.setLeaderBuffInvincibleMode(_continuousHitCount_QuickSkill);
	}


	public bool Progress_SupportSkillAnyAction { get { return KnowledgeSSkill && KnowledgeSSkill.Progress_AnyAction; }}
	public bool Progress_SupportSkillAnyAnimation { get { return KnowledgeSSkill && KnowledgeSSkill.Progress_AnySkillAnimation; }}

	//jks 쿨타임 중 적이 없을 경우 전진.
	protected bool _leaderCoolingRun = false;
	public bool LeaderCoolingRun { get { return _leaderCoolingRun ; } set {_leaderCoolingRun  = value; } }

	#region new leader buff

	protected LeaderBuffs CurrentLeaderBuff 
	{ 
		get 
		{ 
			Knowledge_Mortal_Fighter_Main myLeader = getLeader();
			if (myLeader == null)
				return null;
			
			return myLeader.MyLeaderBuff;
		}
	}


	public override int getLeaderBuffAttackUp(int curAttackPoint)
	{
//		if (AllyID != eAllyID.Ally_Human_Me)
//			return 0;

		if (CurrentLeaderBuff == null || CurrentLeaderBuff._attack_up.amount < 0)
			return 0;

		int attack_up =  Mathf.RoundToInt(curAttackPoint * BattleBase.Instance.CurrentLeaderBuff._attack_up.amount);

		#if UNITY_EDITOR
		if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
		{
			if (attack_up > 0)
			{
				if (BattleBase.Instance.LeaderGameObject == null)
					Log.print_always("  <<리더 버프>>  현재 리더: DEAD !"+" :  공격력 증가 없음.");				
				else
					Log.print_always("  <<리더 버프>>  현재 리더: "+BattleBase.Instance.LeaderGameObject.name +" :  공격력 증가  :   버프: "+ BattleBase.Instance.CurrentLeaderBuff._attack_up.amount 
						+ " *  현재 공격력: "+ curAttackPoint + " = 공격력 증가치: " + attack_up);
			}
		}
		#endif

		if (BattleBase.Instance.LeaderGameObject == null)
			return curAttackPoint;
		
		return  attack_up;
	}

	protected override int getLeaderBuffDefenseUp(int curDefensePoint) 
	{ 
//		if (AllyID != eAllyID.Ally_Human_Me)
//			return 0;

		if (CurrentLeaderBuff == null || CurrentLeaderBuff._defense_up.amount < 0)
			return 0;

		int defense_up =  Mathf.RoundToInt(curDefensePoint * BattleBase.Instance.CurrentLeaderBuff._defense_up.amount);

		#if UNITY_EDITOR
		if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
		{
			if (defense_up > 0)
			{
				if (BattleBase.Instance.LeaderGameObject == null)
					Log.print_always("  <<리더 버프>>  현재 리더: DEAD !"+" :  방어력 증가 없음.");				
				else
					Log.print_always("  <<리더 버프>>  현재 리더: "+BattleBase.Instance.LeaderGameObject.name +" :  방어력 증가  :   버프: "+ BattleBase.Instance.CurrentLeaderBuff._defense_up.amount 
						+ " *  현재 방어력: "+ curDefensePoint + " = 방어력 증가치: " + defense_up);				
			}
		}
		#endif

		if (BattleBase.Instance.LeaderGameObject == null)
			return curDefensePoint;

		return defense_up;
	}

	public override float getLeaderBuffCriticalUp(float curCritical) 
	{ 
//		if (AllyID != eAllyID.Ally_Human_Me)
//			return 0;

		if (CurrentLeaderBuff == null || CurrentLeaderBuff._critical_up.amount < 0)
			return 0;

		return curCritical * CurrentLeaderBuff._critical_up.amount; 
	}

	public override void updateLeaderBuffHPUp(bool hp_up, float exLeader_hp_up = 0f) 
	{ 
//		if (AllyID != eAllyID.Ally_Human_Me)
//			return;

		if (CurrentLeaderBuff == null || CurrentLeaderBuff._hp_up.amount < 0)
			return;


		if (hp_up)
		{
			int delta = Mathf.RoundToInt(Current_HP * CurrentLeaderBuff._hp_up.amount);
			Current_HP += delta; 

			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
			{
				Log.print_always(" 대상 : " + gameObject.name +  " 리더버프:  HP 증가치: " + delta  + " 최종 HP : " + Current_HP);
			}
			#endif
		}
		else
		{
			int delta = Mathf.RoundToInt(Current_HP * exLeader_hp_up);
			Current_HP -= delta; 

			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
			{
				Log.print_always(" 대상 : " + gameObject.name +  " 리더교체:  HP 감소치: " + delta  + " 최종 HP : " + Current_HP);
			}
			#endif
		}

	}

	protected override float getLeaderBuffSkillSpeed(float curSpeedRate) 
	{ 
//		if (AllyID != eAllyID.Ally_Human_Me)
//			return 0;
		
		if (CurrentLeaderBuff == null || CurrentLeaderBuff._skill_speed_up.amount < 0)
			return 0;
		
		return curSpeedRate * CurrentLeaderBuff._skill_speed_up.amount; 
	}

	public override float getLeaderBuffMoveSpeed() 
	{ 
//		if (AllyID != eAllyID.Ally_Human_Me)
//			return 0;

		if (CurrentLeaderBuff == null || CurrentLeaderBuff._move_speed_up.amount < 0)
			return 0;

		return CurrentLeaderBuff._move_speed_up.amount; 
	}

	public override int getLeaderBuffBossCriticalIgnoreCount() 
	{ 
//		if (AllyID != eAllyID.Ally_Human_Me)
//			return 0;
		
		if (CurrentLeaderBuff == null || CurrentLeaderBuff._ignore_boss_critical.amount < 0)
			return 0;

		return (int)CurrentLeaderBuff._ignore_boss_critical.amount; 
	}

	public override int getLeaderBuffInvincibleComboCount() 
	{ 
//		if (CurrentLeaderBuff == null || CurrentLeaderBuff._invincible_mode.amount < 0)
//			return 0;

		return (int)CurrentLeaderBuff._invincible_mode.amount; 
	}

	public override float getLeaderBuffInvinciblePeriod() 
	{ 
//		if (CurrentLeaderBuff == null || CurrentLeaderBuff._invincible_mode.amount < 0)
//			return 0;

		return CurrentLeaderBuff._invincible_mode.info; 
	}
		

	#endregion



	public override float AnimSpeed_Walk 
	{ 
		get { 
			return _animSpeedWalk + _animSpeedWalk * (_boostMovementSpeed + getLeaderBuffMoveSpeed ()); 
		}
	}
	public override float AnimSpeed_WalkBack 
	{ 
		get { 
			return _animSpeedWalkBack + _animSpeedWalkBack * (_boostMovementSpeed + getLeaderBuffMoveSpeed ()); 
		}
	}
	public override float AnimSpeed_WalkFast 
	{ 
		get { 
			return _animSpeedWalkFast + _animSpeedWalkFast * (_boostMovementSpeed +  getLeaderBuffMoveSpeed ()); 
		}
	}
	public override float AnimSpeed_Run 
	{ 
		get { 
			return _animSpeedRun + _animSpeedRun * (_boostMovementSpeed + getLeaderBuffMoveSpeed ()); 
		}
	}





	/// <summary>
	/// Gets the animation info ID. 캐릭터 조건에 따라 다른 애니메이션 정보를 가져오기 위해 조건으로 만들어진 ID 를 사용해서 테이블 항목을 찾음.
	/// 
	/// 100000의 자리수:  클래스 (1:탐, 2:낚,3:창, 4:등, 5:파)
	///
	///	10000의 자리수: 성별 (0:여, 1:남) 캐릭터테이블의 gender.
	///	
	///	1000, 100 자리 수:  체형: 	(0=small, 1=medium, 2=big, 3=huge, ...99)  캐릭터테이블의 body_type.
	///	
	///	10의자리 1의 자리:  무기타입 ( 스킬테이블의 weaponType )
	///	
	///	예) 2C300105 : 창지기 여자 성인 한손검 
	///	(2C는 이 테이블 공통)
	/// </summary>
	/// <returns>The animation info ID.</returns>
	public override int getAnimInfoID()
	{
		Table_Skill skillTable = (Table_Skill)TableManager.GetContent(SkillID);
		Table_Character characterTable = (Table_Character)TableManager.GetContent(CharacterID);

		string classType = ((int)this.Class).ToString();
		string gender = characterTable._gender.ToString();

		string bodyType = characterTable._body_type > 9 ? characterTable._body_type.ToString() : "0"+characterTable._body_type.ToString();
		string weaponAnimType = skillTable._weaponType > 9 ? skillTable._weaponType.ToString() : "0"+skillTable._weaponType.ToString();

		string idHex = classType + gender + bodyType + weaponAnimType;
		int id = (int)DataUtility.ParseHex(idHex);

		id += (int)TABLE.TABLE_ANIM_INFO;

		//Log.jprint("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ getAnimInfoID()  ID: " + id + "   in Hex : "+ idHex);

		return id;
	}

	
	void offcomboInfo()
	{
		BattleUI.Instance().offComboInfo();
	}

	public void resetContinuousHitCount() 
	{ 
		if (!IsLeader || AllyID != eAllyID.Ally_Human_Me) return;

		_continuousHitCount_QuickSkill = 0; 

		#if UNITY_EDITOR
		if (BattleTuning.Instance._showQuickSkillLog)
			Log.print_always(Time.time +"   R E S E T   평타 연타 카운트 !!!!!!!!!!!     ContinuousComboCount_QuickSkill = " + ContinuousComboCount_QuickSkill );
		#endif
	}


	public override eSkillType SkillType { get { return _skillType; } }


//	protected int getAttackPowerBoost_ByContinuousHit(float attackPoint)
//	{
//
//		if (_continuousHitCount_QuickSkill > 14)
//		{
//			BattleUI.Instance().offComboOption();
//			BattleUI.Instance().setComboOption(50);
//			Invoke("offcomboOption", 2.5f);
//			return (int)(attackPoint * 0.5f);
//		}
//		else if (_continuousHitCount_QuickSkill > 9)
//		{
//			BattleUI.Instance().offComboOption();
//			BattleUI.Instance().setComboOption(30);
//			Invoke("offcomboOption", 2.5f);
//			return (int)(attackPoint * 0.3f);
//		}
//		else if (_continuousHitCount_QuickSkill > 4)
//		{
//			BattleUI.Instance().offComboOption();
//			BattleUI.Instance().setComboOption(20);
//			Invoke("offcomboOption", 2.5f);
//			return (int)(attackPoint * 0.2f);
//		}
//		else 
//			return 0;
//	}

	protected void updateQuickComboUI_ByContinuousHit()
	{
		if (!IsLeader || AllyID != eAllyID.Ally_Human_Me)
			return;

		if (_continuousHitCount_QuickSkill > 14)
		{
			BattleUI.Instance().offComboOption();
			BattleUI.Instance().setComboOption(50);
			Invoke("offcomboOption", 2.5f);
		}
		else if (_continuousHitCount_QuickSkill > 9)
		{
			BattleUI.Instance().offComboOption();
			BattleUI.Instance().setComboOption(30);
			Invoke("offcomboOption", 2.5f);
		}
		else if (_continuousHitCount_QuickSkill > 4)
		{
			BattleUI.Instance().offComboOption();
			BattleUI.Instance().setComboOption(20);
			Invoke("offcomboOption", 2.5f);
		}
	}


	void offcomboOption()
	{
		BattleUI.Instance().offComboOption();
	}

	

	public int ContinuousComboCount_QuickSkill { get { return _continuousHitCount_QuickSkill; }}

	//jks 2016.4.21: Mantis1448 : 	public const int _ai_quick_skill_max_hit = 4; //jks 자동모드에서 평타 시작 후 연타 카운트가 4 를 채우면 스킬을 사용하게 함.


	protected int _anim_comboQuick1; //jks comboQuick1 - attack animation controller surfix number
	protected int _anim_comboQuick2; //jks comboQuick2 -
	protected int _anim_comboQuick3; //jks comboQuick3 -
	protected int _anim_comboQuick4; //jks comboQuick4 -
	protected int _anim_comboQuick5; //jks comboQuick5 -
	protected int _anim_comboQuick6; //jks comboQuick6 -

	public int Anim_ComboQuick1 						{ get { return _anim_comboQuick1; } }
	public int Anim_ComboQuick2 						{ get { return _anim_comboQuick2; } }
	public int Anim_ComboQuick3 						{ get { return _anim_comboQuick3; } }
	public int Anim_ComboQuick4 						{ get { return _anim_comboQuick4; } }
	public int Anim_ComboQuick5 						{ get { return _anim_comboQuick5; } }
	public int Anim_ComboQuick6 						{ get { return _anim_comboQuick6; } }

	public void setAnimComboQuick1(int attackID) 	{ _anim_comboQuick1 = getAnimHash("Combo" + attackID.ToString()); Log.nprint("- - - comboQuick1 - - - "+ "Combo" + attackID.ToString() + " hash: "+ _anim_comboQuick1); }
	public void setAnimComboQuick2(int attackID) 	{ _anim_comboQuick2 = getAnimHash("Combo" + attackID.ToString()); Log.nprint("- - - comboQuick2 - - - "+ "Combo" + attackID.ToString() + " hash: "+ _anim_comboQuick2); }
	public void setAnimComboQuick3(int attackID) 	{ _anim_comboQuick3 = getAnimHash("Combo" + attackID.ToString()); Log.nprint("- - - comboQuick3 - - - "+ "Combo" + attackID.ToString() + " hash: "+ _anim_comboQuick3); }
	public void setAnimComboQuick4(int attackID) 	{ _anim_comboQuick4 = getAnimHash("Combo" + attackID.ToString()); Log.nprint("- - - comboQuick4 - - - "+ "Combo" + attackID.ToString() + " hash: "+ _anim_comboQuick4); }
	public void setAnimComboQuick5(int attackID) 	{ _anim_comboQuick5 = getAnimHash("Combo" + attackID.ToString()); Log.nprint("- - - comboQuick5 - - - "+ "Combo" + attackID.ToString() + " hash: "+ _anim_comboQuick5); }
	public void setAnimComboQuick6(int attackID) 	{ _anim_comboQuick6 = getAnimHash("Combo" + attackID.ToString()); Log.nprint("- - - comboQuick6 - - - "+ "Combo" + attackID.ToString() + " hash: "+ _anim_comboQuick6); }



	protected bool _comboQuick1 = false;
	protected bool _comboQuick2 = false;
	protected bool _comboQuick3 = false;
	protected bool _comboQuick4 = false;
	protected bool _comboQuick5 = false;
	protected bool _comboQuick6 = false;

	protected bool _comboQuick1InProgress = false;
	protected bool _comboQuick2InProgress = false;
	protected bool _comboQuick3InProgress = false;
	protected bool _comboQuick4InProgress = false;
	protected bool _comboQuick5InProgress = false;
	protected bool _comboQuick6InProgress = false;

	public bool Action_ComboQuick1 { get { return _comboQuick1; } set {_comboQuick1 = value; } }
	public bool Action_ComboQuick2 { get { return _comboQuick2; } set {_comboQuick2 = value; } }
	public bool Action_ComboQuick3 { get { return _comboQuick3; } set {_comboQuick3 = value; } }
	public bool Action_ComboQuick4 { get { return _comboQuick4; } set {_comboQuick4 = value; } }
	public bool Action_ComboQuick5 { get { return _comboQuick5; } set {_comboQuick5 = value; } }
	public bool Action_ComboQuick6 { get { return _comboQuick6; } set {_comboQuick6 = value; } }

	public bool Progress_AnimComboQuick1 { get { return _comboQuick1InProgress; } set {_comboQuick1InProgress = value; } }
	public bool Progress_AnimComboQuick2 { get { return _comboQuick2InProgress; } set {_comboQuick2InProgress = value; } }
	public bool Progress_AnimComboQuick3 { get { return _comboQuick3InProgress; } set {_comboQuick3InProgress = value; } }
	public bool Progress_AnimComboQuick4 { get { return _comboQuick4InProgress; } set {_comboQuick4InProgress = value; } }
	public bool Progress_AnimComboQuick5 { get { return _comboQuick5InProgress; } set {_comboQuick5InProgress = value; } }
	public bool Progress_AnimComboQuick6 { get { return _comboQuick6InProgress; } set {_comboQuick6InProgress = value; } }


	public bool Progress_Anim_QuickSkill { get { return 	Progress_AnimComboQuick1 || Progress_AnimComboQuick2 || Progress_AnimComboQuick3 || Progress_AnimComboQuick4 || Progress_AnimComboQuick5 || Progress_AnimComboQuick6  ; } }
	
	public override bool Progress_Action_Quick { get { return 	Action_ComboQuick1 || Action_ComboQuick2 || Action_ComboQuick3 || Action_ComboQuick4 || Action_ComboQuick5 || Action_ComboQuick6 ; } }


	public override void resetActionComboQuickFlag()
	{
		#if UNITY_EDITOR
		if (BattleTuning.Instance._showQuickSkillLog)
			Log.print_always(Time.time + " R E S E T  평타  Action Flag()  =========================================");
		#endif
		Action_ComboQuick1 = Action_ComboQuick2 = Action_ComboQuick3 = Action_ComboQuick4 = Action_ComboQuick5 = Action_ComboQuick6 = false;
	}
	
	public override void resetProgressComboQuickFlag()
	{
		#if UNITY_EDITOR
		if (BattleTuning.Instance._showQuickSkillLog)
			Log.print_always(Time.time + " R E S E T  평타  Anim Flag()  --------------------------------------- ");
		#endif
		Progress_AnimComboQuick1 = Progress_AnimComboQuick2 = Progress_AnimComboQuick3 = Progress_AnimComboQuick4 = Progress_AnimComboQuick5 = Progress_AnimComboQuick6 = false;
	}


	public void resetQuickSkill()
	{
		#if UNITY_EDITOR
		if (BattleTuning.Instance._showQuickSkillLog)
			Log.print_always(Time.time + " R E S E T  평타    > > >   > > >   > > >   > > >   > > > ");
		#endif
		resetActionComboQuickFlag();
		resetProgressComboQuickFlag();
		ComboCurrent_QuickSkill = 0;
		AI_AutoCombo_QuickSkill = false;
	}


	protected int _attackType_QuickSkill;
	protected float _attackInfo_QuickSkill;
	protected int _attackInfo2_QuickSkill;
	protected string _attackInfo3_QuickSkill;
	

	public int AttackType_QuickSkill			{ get { return _attackType_QuickSkill; } }
	public float AttackInfo_QuickSkill			{ get { return _attackInfo_QuickSkill; } }
	public int AttackInfo2_QuickSkill 			{ get { return _attackInfo2_QuickSkill; } }
	public string AttackInfo3_QuickSkill		{ get { return _attackInfo3_QuickSkill; } }

	public override void quickSkill_Action_Finished(int combo)
	{
		//jks 2016.4.26  현재 액션의 이벤트가 아니면 무시.
		if (combo != _currentCombo_QuickSkill) return;

		#if UNITY_EDITOR
		if (BattleTuning.Instance._showQuickSkillLog)
		Log.print_always(Time.time +"   "+ gameObject.name +  " !!!!!  quickSkill_Action_Finished X X X X X X X X X X X X X X ");
		#endif

		if (!_isResetActionInfoDone)
		{
			resetActionInfo();
			_isResetActionInfoDone = true;
		}

		animationFinished();
	}

	public override void quickSkill_Combo_Transition(int combo)
	{
		//jks 2016.4.26  현재 액션의 이벤트가 아니면 무시.
		if (combo != _currentCombo_QuickSkill) return;

		#if UNITY_EDITOR
		if (BattleTuning.Instance._showQuickSkillLog)
		Log.print_always(Time.time +"   "+ gameObject.name +  " !!!!!  평타 quickSkill_Combo_Transition 이벤트  go to next combo   > > > ");
		#endif

		processAuto_Or_AI_QuickSkill();
	}


	public override void quickSkill_InputWindow_Open(int combo)
	{
		if (AmIAuto) return; //jks 자동이면 인풋 상과없음.

		//jks 2016.4.26  현재 액션의 이벤트가 아니면 무시.
		if (combo != _currentCombo_QuickSkill) return;

		#if UNITY_EDITOR
		if (BattleTuning.Instance._showQuickSkillLog)
		Log.print_always(Time.time +"   "+ gameObject.name +  " !!!!!  O P E N   평타 input 받기 시작 이벤트 =============== > > > ");
		#endif

		_quickSkillInputWindow_Open = true;
	}

	public override void quickSkill_InputWindow_Close(int combo)
	{
		if (AmIAuto) return; //jks 자동이면 인풋 상과없음.
	
		//jks 2016.4.26  현재 액션의 이벤트가 아니면 무시.
		if (combo != _currentCombo_QuickSkill) return;

		#if UNITY_EDITOR
		if (BattleTuning.Instance._showQuickSkillLog)
		Log.print_always(Time.time +"   "+ gameObject.name +  " !!!!!  C L O S E   < < < =============== 평타 input 받기 종료 이벤트 .");
		#endif

		if (_quickSkillInputWindow_Open)
			resetContinuousHitCount();

		_quickSkillInputWindow_Open = false;
	}


	protected bool _isCardUseFinishied = true;
	public bool IsCardUseFinishied{ get {return _isCardUseFinishied;} set {_isCardUseFinishied = value; }}


	protected int _cardDeckIndex;
	public int CardDeckIndex { get { return _cardDeckIndex; } set {_cardDeckIndex = value; } }

	protected bool _do_not_attack = false;
	public bool DoNotAttack { get { return _do_not_attack; } set {_do_not_attack = value; } }

	protected bool _pvp_victim = false;
	public bool IsPvpVictim { get { return _pvp_victim; } set {_pvp_victim = value; } }

	protected GameObject _pvp_target;
	public GameObject PvpTarget { get { return _pvp_target; } set {_pvp_target = value; } }

	protected float _pvp_home_position;
	public float PvpHomePosition { get { return _pvp_home_position; } set {_pvp_home_position = value; } }


	protected float _shortenTeamCooltime;
	public float ShortenTeamCooltime { get {return _shortenTeamCooltime;} set {_shortenTeamCooltime = value;}}


//	protected bool _role_skill = false;
//	public bool Action_RoleSkill { get { return _role_skill; } set {_role_skill = value; } }
//
//	protected bool _role_skill_activated = false;

	protected bool _guard_up = false;
	public bool Action_Guard_Up { get { return _guard_up; } set {_guard_up = value; } }


	protected bool _skill_staging = false;
	public bool Action_SkillStaging { get { return _skill_staging; } set {_skill_staging = value; } }

	public virtual SupportSkillKnowledge_Leader KnowledgeSSkill 
	{ 
		get 
		{ 
			return (SupportSkillKnowledge_Leader)base.KnowledgeMultiSkill;
		} 
	}


	public override eAttackType AttackType 
	{ 
		get 
		{ 
			if (KnowledgeSSkill && KnowledgeSSkill.Progress_AnyAction)
				return KnowledgeSSkill.AttackType;
			else
				return _attackType; 
		} 
	}


	public bool HaveSuppportSkill { get { return BattleBase.Instance.HaveSupportSkill; } }


	public virtual Knowledge_Mortal_Fighter_Main getLeader() 
	{
		return BattleBase.Instance.LeaderKnowledge;
	}



	protected override void initializeReference()
	{
		base.initializeReference();

		StartCoroutine(setLauncher());
	}

	protected override void initializeBeforeUpdateBegin()
	{
		base.initializeBeforeUpdateBegin();

		_walk = true;

		//for main characters, always animate
		AnimCon.setAnimatorCullingMode(AnimatorCullingMode.AlwaysAnimate);


	}


	void Update()
	{
		doNotLetPassThrough();
	}



	//jks 매치 스킬 연출 종료 이벤트 안 불리는 경우에 대한 안전 장치.
	public IEnumerator manual_SkillStaging_Finished()
	{
		yield return new WaitForSeconds(4.0f);

		animEvent_SkillStaging_Finished();
	}




	//jks staging finishied, resume battle
	public override void skillStagingFinishied()
	{
		//Log.jprint(Time.time + "- - - - - - - - -   M A T C H   C L A S S    : finishied      ");
//jks 2015.11.6 스킬 연출 제거관련:		BattleBase.Instance.PauseFighters(false);

		BattleBase.Instance.IsSkillStagingInProgress = false;

		if (Action_SkillStaging == false) return;

		BattleBase.Instance.PauseAlly(false, AllyID);
		Invoke("letOpponentsStartAttack", 3.0f); //jks 적의 공격 시작을 3초 지연 시킴.

		BattleBase.Instance.ProgressClassMatchAttack = true; //jks 클래스 매치 공격인지 확인.

		Progress_SkillStaging = false;
		Action_SkillStaging = false;

		BattleBase.Instance.UseCard_Disabled = false;

//		if (IsLeader)
//			leaderMove();
//
//		Action_Combo1 = true;

//jks 2015.11.6 스킬 연출 제거관련:		BattleBase.Instance.setEventCharacterScale(CardDeckIndex, true);

//jks 2015.11.6 스킬 연출 제거관련:		StartCoroutine(endSkilStaging());


		GameObject closestEnemy = BattleBase.Instance.findClosestOpponent(AllyID, transform.position.x);
		if (closestEnemy == null)
			return;
		setTarget(closestEnemy);

		//jks place hero at attack distance
		Vector3 stagingPosition = transform.position;

		Knowledge_Mortal enemyKnow = closestEnemy.GetComponent<Knowledge_Mortal>();

		//stagingPosition.x = closestEnemy.transform.position.x - (_attackDistanceMax+0.1f); //stagingPosition.x = closestEnemy.transform.position.x + (_attackDistanceMax+0.1f) * closestEnemy.transform.forward.x;
		stagingPosition.x = closestEnemy.transform.position.x - enemyKnow.Radius - Radius - (_attackDistanceMax+0.1f); //jks 볼륨 적용.

		Log.jprint(" Enemy info : " + closestEnemy.name + " : position x: "+ closestEnemy.transform.position.x + "  radius : " + enemyKnow.Radius + 
			       " Hero info : "  + gameObject.name   + " : position x: "+ transform.position.x              + "  radius : "+ Radius);

		Log.jprint(" match skill position: " + stagingPosition.x + " >  transform.position.x + Radius : "+ transform.position.x + Radius);
		if (stagingPosition.x > transform.position.x + Radius) //jks 멀리 있으면 hero 근접 시킴.
			transform.position = stagingPosition;

		PauseSpeedAdjustment = 1.0f;
		Action_Combo1 = true;
	}

//jks 2015.11.6 스킬 연출 제거관련:
//	IEnumerator endSkilStaging()
//	{
//		yield return StartCoroutine( BattleBase.Instance.swapSkillStagingCamera() );
//
//		//jks reset layer at the end to keep show hero on staging cam.
//		if (IsLeader)
//			Utility.setLayerAllChildren(gameObject, "Player");  
//		else
//			Utility.setLayerAllChildren(gameObject, "Default");
//
//	}

	protected void letOpponentsStartAttack()
	{
		BattleBase.Instance.ProgressClassMatchAttack = false;
		BattleBase.Instance.PauseOpponents(false, AllyID);
	}


	public override void animationFinished()
	{
//		resetRoleSkillFlag();

//		Log.jprint(gameObject.name + "   >  >  >  >  >   1: "+ Action_Combo1 + "  2:  "+ Action_Combo2 + "  3:  "+ Action_Combo3 + "  4:  "+ Action_Combo4 + "  5:  "+ Action_Combo5 + "  6:  "+ Action_Combo6);
//		Log.jprint(gameObject.name + "        animationFinished()        LastCombo: "+ LastCombo + "  ==  ComboCurrent :  "+ ComboCurrent);
//		//		if (TotalCombo == ComboCurrent)
//		if (LastCombo == ComboCurrent)
//		{
//			if (_coolTimer != null)
//				if (! _coolTimer.IsCoolingInProgress)
//				{
//					startCoolTimeAndResetComboFlag();
//					Log.jprint(gameObject.name + " >  >  >  >  > animationFinished start cool time ");
//				}
//		}

	}

//	protected void resetRoleSkillFlag()
//	{
//		if (isRoleSkillActive())
//		{
//			Action_RoleSkill = false;
//			Progress_RoleSkillAnimation = false;
//			_role_skill_activated = false;
//			Action_Idle = true;
//			if (_roles_skill_EverGaveDamage)
//				_coolTimer_GuardUp.activateTimer();
//		}
//	}



	protected override void processCooltimeFinished()
	{
		if (IsDead) return;

		//Log.jprint(Time.time + "  :  " + gameObject.name + "C O O L  T I M E   E N D: CardDeckIndex : " + CardDeckIndex); 

//		BattleBase.Instance.doneUICoolTime(CardDeckIndex);

		if (IsLeader)
		{
			GetComponent<Behavior_Mortal_Fighter_Main>().leaderStrategyLocomotion_AfterCooled();
		}
	}
//	protected override void processCooltimeFinished_RoleSkill()
//	{
//		if (IsDead) return;
//		
//		//Log.jprint(Time.time + "  :  " + gameObject.name + "C O O L  T I M E   E N D: CardDeckIndex : " + CardDeckIndex); 
//		
//		//BattleBase.Instance.doneUICoolTime_RoleSkill(CardDeckIndex);
//		
//	}
	


	public override void startCoolTimer()
	{
		if (_coolTimer.IsCoolingInProgress)
			return;

		_coolTimer.activateTimer();

//		if (name.Contains("M2"))
//			Log.jprint(gameObject.name + "$ $ $ startCoolTimer(): " + _coolTime + " sec"); 
		
		//jks play Idle/Cool animation during cooltime
		if (IsLeader)
		{
			//Log.jprint(gameObject.name + "+ + + + + + + + + : CardDeckIndex : " + CardDeckIndex);
//			BattleBase.Instance.showUICoolTime(CardDeckIndex);

			//jks stop freezing - forceResetFlags();
			Action_Idle = true;

//			Log.jprint(Time.time + "     $ $ $ startCoolTimer(): " + _coolTime + " sec" + "          "+gameObject); 
		}
	}



	void OnDisable()
	{
		//Log.jprint(gameObject.name + "- - - - OnDisable() : CardDeckIndex : " + CardDeckIndex);
		forceResetFlags();
		_idle = false;

		BattleBase.Instance.endUICoolTime(CardDeckIndex);
		BattleBase.Instance.endUICoolTime_GuardUp(CardDeckIndex);

	}



	public override void forceResetFlags()
	{
		base.forceResetFlags();

		if (KnowledgeSSkill && KnowledgeSSkill.Progress_AnyAction)
			KnowledgeSSkill.forceResetComboFlags();
	}

	public void forceResetFlagsAll()
	{
		forceResetFlags();
		_walkBackTurn = false;
	}

	public virtual void setAttributesFromTable(Table_SkillQuick tbl, float boostAttackPoint)
	{
		_totalCombo_QuickSkill = tbl._totalCombo;

		setAnimComboQuick1(tbl._combo1); //jks comboQuick1
		setAnimComboQuick2(tbl._combo2); //jks comboQuick2
		setAnimComboQuick3(tbl._combo3); //jks comboQuick3
		setAnimComboQuick4(tbl._combo4); //jks comboQuick4
		setAnimComboQuick5(tbl._combo5); //jks comboQuick5
		setAnimComboQuick6(tbl._combo6); //jks comboQuick6

		if (tbl._attackDistanceMax != 0)
			_attack_distance_max_QuickSkill = tbl._attackDistanceMax * 0.1f;

		_autoComboStart_QuickSkill = tbl._auto_combo_start;

		_attackPoint_QuickSkill =  tbl._attack_point + Mathf.RoundToInt(tbl._attack_point * boostAttackPoint);
		_attackPointFinal_QuickSkill = tbl._attack_point_final;

		if (BattleBase.Instance.IsRankingTower)
		{
			_attackPoint_QuickSkill = Mathf.RoundToInt(_attackPoint_QuickSkill * BattleTuning.Instance._rankingTower_attack_boost);
			_attackPointFinal_QuickSkill = Mathf.RoundToInt(_attackPointFinal_QuickSkill * BattleTuning.Instance._rankingTower_attack_boost);
		}
			
		_attackType_QuickSkill = tbl._attackType;
		_attackInfo_QuickSkill = tbl._info;
		_attackInfo2_QuickSkill = tbl._info2;
		_attackInfo3_QuickSkill = tbl._info3;

		if (_attackType_QuickSkill == 1) //projectile
		{
			Launcher_Projectile_Basic launcher = gameObject.GetComponent<Launcher_Projectile_Basic>();
			if (launcher == null)
				gameObject.AddComponent<Launcher_Projectile_Basic>();
		}

		Table_ReactionType tableReaction = (Table_ReactionType)TableManager.GetContent(tbl._reactionTypeID);
		setAttributesFromTable_QuickSkillReaction(tableReaction);
	}


	public void setAttributesFromTable_QuickSkillReaction(Table_ReactionType table)
	{
		Table_ReactionGroup tbl = (Table_ReactionGroup)TableManager.GetContent(table._reactionGroupID1);
		
		if (tbl != null)
		{
			_anim_reaction_quickSkill[0,0] = tbl._reactionID1;
			_anim_reaction_quickSkill[0,1] = tbl._reactionID2;
			_anim_reaction_quickSkill[0,2] = tbl._reactionID3;
			_anim_reaction_quickSkill[0,3] = tbl._reactionID4;
			_anim_reaction_quickSkill[0,4] = tbl._reactionID5;
			_anim_reaction_quickSkill[0,5] = tbl._reactionID6;
			_anim_reaction_quickSkill[0,6] = tbl._reactionID7;
		}
		
		if (table._reactionGroupID2 == 0) return;
		tbl = (Table_ReactionGroup)TableManager.GetContent(table._reactionGroupID2);
		if (tbl != null)
		{
			_anim_reaction_quickSkill[1,0] = tbl._reactionID1;
			_anim_reaction_quickSkill[1,1] = tbl._reactionID2;
			_anim_reaction_quickSkill[1,2] = tbl._reactionID3;
			_anim_reaction_quickSkill[1,3] = tbl._reactionID4;
			_anim_reaction_quickSkill[1,4] = tbl._reactionID5;
			_anim_reaction_quickSkill[1,5] = tbl._reactionID6;
			_anim_reaction_quickSkill[1,6] = tbl._reactionID7;
		}
		
		if (table._reactionGroupID3 == 0) return;
		tbl = (Table_ReactionGroup)TableManager.GetContent(table._reactionGroupID3);
		if (tbl != null)
		{
			_anim_reaction_quickSkill[2,0] = tbl._reactionID1;
			_anim_reaction_quickSkill[2,1] = tbl._reactionID2;
			_anim_reaction_quickSkill[2,2] = tbl._reactionID3;
			_anim_reaction_quickSkill[2,3] = tbl._reactionID4;
			_anim_reaction_quickSkill[2,4] = tbl._reactionID5;
			_anim_reaction_quickSkill[2,5] = tbl._reactionID6;
			_anim_reaction_quickSkill[2,6] = tbl._reactionID7;
		}
		
		
		if (table._reactionGroupID4 == 0) return;
		tbl = (Table_ReactionGroup)TableManager.GetContent(table._reactionGroupID4);
		if (tbl != null)
		{
			_anim_reaction_quickSkill[3,0] = tbl._reactionID1;
			_anim_reaction_quickSkill[3,1] = tbl._reactionID2;
			_anim_reaction_quickSkill[3,2] = tbl._reactionID3;
			_anim_reaction_quickSkill[3,3] = tbl._reactionID4;
			_anim_reaction_quickSkill[3,4] = tbl._reactionID5;
			_anim_reaction_quickSkill[3,5] = tbl._reactionID6;
			_anim_reaction_quickSkill[3,6] = tbl._reactionID7;
		}
		
		if (table._reactionGroupID5 == 0) return;
		tbl = (Table_ReactionGroup)TableManager.GetContent(table._reactionGroupID5);
		if (tbl != null)
		{
			_anim_reaction_quickSkill[4,0] = tbl._reactionID1;
			_anim_reaction_quickSkill[4,1] = tbl._reactionID2;
			_anim_reaction_quickSkill[4,2] = tbl._reactionID3;
			_anim_reaction_quickSkill[4,3] = tbl._reactionID4;
			_anim_reaction_quickSkill[4,4] = tbl._reactionID5;
			_anim_reaction_quickSkill[4,5] = tbl._reactionID6;
			_anim_reaction_quickSkill[4,6] = tbl._reactionID7;
		}
		
		if (table._reactionGroupID6 == 0) return;
		tbl = (Table_ReactionGroup)TableManager.GetContent(table._reactionGroupID6);
		if (tbl != null)
		{
			_anim_reaction_quickSkill[5,0] = tbl._reactionID1;
			_anim_reaction_quickSkill[5,1] = tbl._reactionID2;
			_anim_reaction_quickSkill[5,2] = tbl._reactionID3;
			_anim_reaction_quickSkill[5,3] = tbl._reactionID4;
			_anim_reaction_quickSkill[5,4] = tbl._reactionID5;
			_anim_reaction_quickSkill[5,5] = tbl._reactionID6;
			_anim_reaction_quickSkill[5,6] = tbl._reactionID7;
		}
	}


	public int getReactionAnimID_QuickSkill(int combo)
	{
		if(combo == 0) //jks HACK: safty check
			combo = 1;
		
		int reactionChoice;
		
		reactionChoice = Random.Range(0, 6);

		//Log.jprint(gameObject.name + "   reaction choice: "+ reactionChoice + "     reaciton anim #: "+ _anim_reaction[combo-1,reactionChoice]);
		
		return _anim_reaction_quickSkill[combo-1,reactionChoice]; //jks index 0 == combo1, index 1 == combo2, index 2 == combo3
	}



	public void setAttributesFromCardSlot(CardSlot slot)
	{
		if (BattleBase.Instance.IsBossRaid)
		{
			float adjustment = BattleBase.Instance.getConditionalBoost_Hp(Class)
							*  BattleBase.Instance.getConditionalDrop_Hp(Class);	
			
			if (adjustment != 1.0f)
				slot.setBossRaid_Conditional_HP_Adjustment(adjustment);
		}

		// [[ eom 캐릭터 스킨 버프 수치를 CardSlot > setSlotCheck() 에서 업데이트 하여 Card의 신수가 아닌 CardSlot의 신수를 사용하도록 함.
		_max_hp = slot.Max_HP;
		//_max_su = slot.CCard.MaxSinsu;
		// ]] eom 캐릭터 스킨 버프 수치를 CardSlot > setSlotCheck() 에서 업데이트 하여 Card의 신수가 아닌 CardSlot의 신수를 사용하도록 함.

		_cur_hp = slot.Now_HP;
		_attackPoint = slot.Atk;
		if (BattleBase.Instance.IsBossRaid)
		{
			float adjustment = BattleBase.Instance.getConditionalBoost_Attack(Class)
							*  BattleBase.Instance.getConditionalDrop_Attack(Class);
			_attackPoint = (int)(_attackPoint * adjustment);
		}

		_criticalRate = slot.Hit * 0.01f; //ㅈㅓㄱ ㅈㅜㅇ ㄹㅠㄹ
					
		_boostSkillSpeed = slot.Quickness; // 순발력 ?.. (ABI - Ability ??) 
		_boostMovementSpeed = slot.Agility;  // 민첩도(AGI - Agility) 
		_boostSupportAndQuickSkill = slot.Wisdom;
		//_coolTime = slot.CoolTime;

		if (BattleBase.Instance.IsBossRaid)
		{
			_boostSkillSpeed *= BattleBase.Instance.getConditionalBoost_Quickness(Class) 
							 * BattleBase.Instance.getConditionalDrop_Quickness(Class);
			_boostMovementSpeed *= BattleBase.Instance.getConditionalBoost_Agility(Class) 
								* BattleBase.Instance.getConditionalDrop_Agility(Class);
			_boostSupportAndQuickSkill *= BattleBase.Instance.getConditionalBoost_Intelligence(Class) 
									   * BattleBase.Instance.getConditionalDrop_Intelligence(Class);
			
		}

		Table_Skill tbl_skill = (Table_Skill)TableManager.GetContent ( slot.SCard.SkillID );  //jks - get skill table
		setAttributesFromTable(tbl_skill);

		Table_SkillQuick tbl = (Table_SkillQuick)TableManager.GetContent(slot.SCard.SkillQuickID); //jks - get 평타 table
		setAttributesFromTable(tbl, _boostSupportAndQuickSkill);


//		int convertedRollSkillID = TableManager.getConvertedID(TABLE.TABLE_LEADER_ROLE_SKILL, (int)slot.SCard.SkillType); 
//		Table_LeaderRoleSkill tableRollSkill = (Table_LeaderRoleSkill)TableManager.GetContent(convertedRollSkillID);
//		setAttributesFrom(tableRollSkill);

		int converted = TableManager.getConvertedID(TABLE.TABLE_GUARD_UP, (int)slot.CCard.Class); 
		Table_GuardUp tbl_guard = (Table_GuardUp)TableManager.GetContent(converted);
		setAttributesFrom(tbl_guard);

	}

	public void setAttributesFromCardSlot_SkillPreview_Offline(CardSlotSkillViewer slot)
	{
		_max_hp = 1000;

		_cur_hp =1000;
		_attackPoint = 100;
		_criticalRate = 0.5f; //적중율

		_boostSkillSpeed = 0; // 순발력 ?.. (ABI - Ability ??) 
		_boostMovementSpeed = 0;  // 민첩도(AGI - Agility) 
		_boostSupportAndQuickSkill = 0;
		//_coolTime = slot.CoolTime;

		Table_Skill tbl_skill = (Table_Skill)TableManager.GetContent ( slot._SCard._SkillID );  //jks - get skill table
		setAttributesFromTable(tbl_skill);

	}


//	#region leader role skill
//
//	protected float _pushDistance; 
//	protected float _roleSkillDuration; 
//	protected int _role_skill_attack_point;
//	protected float _role_skill_cooltime;
//	protected int _role_skill_sinsu_use;
//	protected SKILL_TYPE _role_skill_type;
//	protected float _role_skill_attack_distance;
//	protected float _role_skill_hit_range;
//
//	public SKILL_TYPE RoleSkillType { get { return _role_skill_type; }}
//	public int RoleSkillAttackPoint { get { return _role_skill_attack_point; }}
//	public float RoleSkillCooltime { get { return _role_skill_cooltime; }}
//
//	public float getCooltime_RoleSkill(CardSlot slot)
//	{
//		int convertedRollSkillID = TableManager.getConvertedID(TABLE.TABLE_LEADER_ROLE_SKILL, (int)slot.SCard.SkillType); //jks TODO remove test after finish.
//		Table_LeaderRoleSkill tableRollSkill = (Table_LeaderRoleSkill)TableManager.GetContent(convertedRollSkillID);
//		return tableRollSkill._cooltime * 0.1f;
//	}


//	1=Type T: 근접형  :  밀집분산역할
//	2=Type H: 지원형  : 공격차단역할
//	0=Type D: 원거리형  : 진로차단

//jks 2015.10.2 역할 스킬 제거.
//	protected void setAttributesFrom(Table_LeaderRoleSkill tbl)
//	{
//		setAnimRoleSkill( tbl._animation );
//
//		_role_skill_attack_point = tbl._attack_point;
//
//		_role_skill_cooltime = tbl._cooltime * 0.1f;
//
//		_role_skill_sinsu_use = tbl._sinsu_use;
//
//		_role_skill_type = (SKILL_TYPE) tbl._role_type;
//
//		_role_skill_attack_distance = tbl._attack_distance_max * 0.1f;
//
//		_role_skill_hit_range = tbl._hit_range * 0.1f;
//
//		switch (_role_skill_type)
//		{
//		case SKILL_TYPE.TYPE_T: //근접형  : 밀집분산역할
//			_pushDistance = tbl._info * 0.1f;
//			break;
//		case SKILL_TYPE.TYPE_H: //지원형  : 공격차단역할
//		case SKILL_TYPE.TYPE_D: //원거리형 : 진로차단
//			_roleSkillDuration = tbl._info * 0.1f;
//			break;
//		}
//	}


//	public void consumeSinsuForRoleSkillUse()
//	{
//		updateSinsu(-_role_skill_sinsu_use);
//		if (BattleUI.Instance() != null)
//			BattleUI.Instance().addDemageUI(transform, 0, eHitType.HT_NONE);
//	}
//
//	public bool isRoleSkillActive()
//	{
//		return _role_skill_activated;
//	}

//	public void activateRoleSkill()
//	{
//		_role_skill_activated = true;
//
//		_roles_skill_EverGaveDamage = false;
//
//		if (BattleManager.Instance.findEnemiesNear(gameObject, _role_skill_attack_distance))
//		{
//			StartCoroutine( startSkill() );
//		}
//		else
//		{
//			Action_Run = true;
//		}
//	}

//	public bool IsRoleSkillWall
//	{
//		get { return (_role_skill_type == SKILL_TYPE.TYPE_D); }  // 원거리형  진로차단 ?
//	}
//
//
//	public override bool AmIWall
//	{
//		get { return IsRoleSkillWall; }
//	}

//	#endregion


#region GuardUp

	protected bool _guard_up_key_released = true;
	protected bool _guard_up_time_min_passed = false; //jks 막기 실행 후 최소 막기 시간이 경과 했는지 확인. (막기 애니 한사이클 보여줌)
	protected float _guard_up_time_max;
	protected float _guard_up_cooltime = 1.5f;
	protected float [] _guard_up_defense_boost = new float[6]{0,0,0,0,0,0}; //jks index 0  is not used  1: 탐색꾼,...

	float guardUpDefenseBoost(CardClass classType) { return _guard_up_defense_boost[(int)classType]; }

	public float GuardUpCooltime { get { return _guard_up_cooltime; }}
	public float GuardUpTimeMax { get { return _guard_up_time_max; }}
	public bool GuardUpTimeMinPassed { get { return _guard_up_time_min_passed; }}
	public bool GuardUpKeyReleased { get { return _guard_up_key_released; } set { _guard_up_key_released = value;}}



//	public bool isGuardUpActive()
//	{
////		return _role_skill_activated;
//		return false; //for now
//	}



	public void activateGuardUp(bool val)
	{
		if (val)
		{
			_guard_up_time_min_passed = false;
			CancelInvoke("timeUpGuardUp");
			Invoke("timeUpGuardUp", GuardUpTimeMax);
		}
		else
		{
			//Action_Hit = false;
			forceResetFlags();
			startCoolTimer_GuardUp();
			Progress_GuardUpAnimation = false;
		}

		Action_Guard_Up = val;
	}


	public override void startCoolTimer_GuardUp()
	{
		_coolTimer_GuardUp.activateTimer(GuardUpCooltime);
		//Log.jprint(gameObject.name + "$ $ $ startCoolTimer(): " + _coolTime + " sec"); 
		
		//play Idle/Cool animation during cooltime
		//jks stop freezing - forceResetFlags();
		Action_Idle = true;
	}



	protected void setAttributesFrom(Table_GuardUp tbl)
	{
		setAnimGuardUp( (int)_class );

		_guard_up_cooltime = tbl._cool_time * 0.1f;

		_guard_up_defense_boost[1] = tbl._defense_search * 0.01f;
		_guard_up_defense_boost[2] = tbl._defense_fish * 0.01f;
		_guard_up_defense_boost[3] = tbl._defense_spear * 0.01f;
		_guard_up_defense_boost[4] = tbl._defense_light * 0.01f;
		_guard_up_defense_boost[5] = tbl._defense_wave * 0.01f;

		_guard_up_time_max = tbl._guard_time_max * 0.1f;  //jks 막기 최대 지속 시간
	}


	void timeUpGuardUp()
	{
		if (Action_Guard_Up == false) return;

		BattleBase.Instance.endGuardUp();
	}


	protected override int applyGuardUpDefenseBoost(int originalDefense, CardClass opponentClass)
	{
		if (! Action_Guard_Up) return originalDefense; // 막기 중이 아니면, 

		ObscuredInt newDefense = originalDefense;
		ObscuredFloat boost = guardUpDefenseBoost(opponentClass);
		newDefense += Mathf.RoundToInt( (float)originalDefense * boost );

		#if UNITY_EDITOR
		if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
				Log.print_always("   리더 막기 방어력 상승 적용       적용전: " + originalDefense + "      상승치: "+ (boost * 100) + " %      적용후: "+ newDefense );
		#endif

		return newDefense; 
	}


	public override void animationFinished_GuardUp()
	{
		_guard_up_time_min_passed = true;

		if (GuardUpKeyReleased && Action_Guard_Up) //jks 최소시간 이벤트 발생 시 이미 유저가 버튼을 놓아 버린 상태면, 막기 바로 중단.
		{
			BattleManager.Instance.endGuardUp();
		}
			
	}


#endregion



	public void activateSupportSkill(int skillNum)
	{
//		forceResetFlags();
		if (KnowledgeSSkill)
			KnowledgeSSkill.startSkill_Support(skillNum);

	}





	public override bool checkEnemyOnPath(bool bQuickSkill)
	{
//		if (isRoleSkillActive())
//		{
//			return BattleManager.Instance.findEnemiesNear(gameObject, _role_skill_attack_distance);
//		}
//		else
		{
			return getOpponentsToAttackWhileMoving(bQuickSkill);
		}
	}


	



	public override bool canUseSkill()
	{
		if (! base.canUseSkill()) return false;

		//jks 버튼 터치 무시 flag 이 켜있으면, 지금 스킬 사용 못 함.
		if (BattleBase.Instance.IsIgnoreButtonTouch) return false;  //jks 원거리 스킬은 보스대화 거리에서 공격으로 들어 갈 수 있으므로 거리가 가까워지면 공격 할 수 없게 함.

		//jks 보스 대사 중이면, 지금 스킬 사용 못 함.
		if (IsLeader && _bossDialogStagingMode) return false;

		if (Progress_SupportSkillAnyAnimation) return false;

		//jks 2016.4.21 Mantis[1449]리더 고유 스킬 카드의 사용을 1 순위로.	-	if (Action_Guard_Up || Progress_GuardUpAnimation) return false;

		if (BattleBase.Instance.IsSkillStagingInProgress) //jks 체인스킬연출 중,
		{
			if (AllyID == eAllyID.Ally_Human_Me) // 유저팀 이면,
			{
				if (!IsLeader) return false; // 팀원은 스킬 못하게 함.
			}
			else
			{
				return false; // 상대 팀은 모두 스킬 사용 못 함.
			}
		}

		//jks 2016.4.21 Mantis[1449]리더 고유 스킬 카드의 사용을 1 순위로.  -   if (Progress_Action_Quick || AI_AutoCombo_QuickSkill) return false;
	
		if (GetComponent<Animator>().IsInTransition(0))
		{
			//Log.jprint(" avoid skill start.     B L E N D I N G");
			return false;  //jks to avoid start skill while previous blending. (in case fast click/touch input)
		}

		return  true;
	}





	/// <summary>
	/// 평타 사용할 수 있는지 확인.
	/// </summary>
	/// <returns><c>true</c>, if use quick skill was caned, <c>false</c> otherwise.</returns>
	public virtual bool canUseQuickSkill()
	{
		//jks 포즈면,  사용 못 함.
		if (MadPauser.Instance.TimeState == eTimeState.TIME_PAUSE) return false;

		if (!IsLeader) return false;

		//jks 전투 중이 아니면, 사용 못 함.
		if (!IsBattleInProgress) return false;

		//jks 입력 무시 중이면, 사용 못 함.
		if (BattleBase.Instance.IsIgnoreButtonTouch) return false; //jks 원거리 스킬은 보스대화 거리에서 공격으로 들어 갈 수 있으므로 거리가 가까워지면 공격 할 수 없게 함.

		//jks 보스 대화 중이면, 사용 못 함.
		if (_bossDialogStagingMode) return false;


		//jks 클래스 매치 스킬 중이면,  사용 못 함.
		if (BattleBase.Instance.IsSkillStagingInProgress) return false;
		if (BattleBase.Instance.ProgressClassMatchAttack) return false;


		//jks 이미 스킬 시작 되었으면, 지금 스킬 사용 못 함.
		if (Progress_SkillAction || Progress_SkillAnimation)
		{
			//jks 무한달리기 방지. 
			if (BattleBase.Instance.IsPlay_AutoLeader && amIMoving()) 
			{
				forceResetFlags();
				Action_Run = true;
				return true;
			}

			return false;
		}


		//jks 2016.4.21: Mantis1448 : 		//jks 자동모드에서 평타콤보가 요구치(4) 만족되면, 스킬 나가게하도록 평타 시작 방지.
		//jks 2016.4.21: Mantis1448 : 		if (AmIAuto && ContinuousComboCount_QuickSkill >= _ai_quick_skill_max_hit) return false;


		//jks to avoid start skill while previous blending. (in case fast click/touch input)
		if (GetComponent<Animator>().IsInTransition(0)) return false;


		//jks 무한달리기 방지. 
		if (BattleBase.Instance.IsPlay_AutoLeader && amIMoving()) 
		{
			forceResetFlags();
			Action_Run = true;
			return true;
		}



		return true;
	}




//	#region Leader Buff
//	
//	public override int calculate_LeaderBuff_AttackPoint_Self(CardClass enemyClass)
//	{
//		float rate = CharacterClassRelation.getAttackRate_Buff_Self(BattleBase.Instance.LeaderClass, enemyClass) * 0.01f;
//		
//		return Mathf.RoundToInt(_attackPoint * rate);
//	}
//	
//	public override float calculate_LeaderBuff_HitRate_Self(CardClass enemyClass)
//	{
//		float buffHitRate = CharacterClassRelation.getCriticalRate_Buff_Self(BattleBase.Instance.LeaderClass, enemyClass) * 0.01f;
//		
//		return buffHitRate;
//	}
//
//	#endregion

//jks 2015.5.8 remove leader strategy-	
//	#region Leader Strategy
//
//
//	public override int calculate_LeaderStrategy_AttackPoint()
//	{
//		if (!IsLeader) return 0;
//
//		eLeaderStrategy leader_strategy = BattleBase.Instance.LeaderStrategyType;
//		float rate = LeaderStrategy.getLeaderStrategyValues(leader_strategy, eSubjectType.ST_Self, LeaderStrategy.eValueType.VT_Attack) * 0.01f;
//		
//		return Mathf.RoundToInt(_attackPoint * rate);
//	}
//
//	public override float calculate_LeaderStrategy_HitRate()
//	{
//		if (!IsLeader) return 0;
//
//		eLeaderStrategy leader_strategy = BattleBase.Instance.LeaderStrategyType;
//		float rate = LeaderStrategy.getLeaderStrategyValues(leader_strategy, eSubjectType.ST_Self, LeaderStrategy.eValueType.VT_Critical) * 0.01f;
//		
//		return rate;
//	}
//
//	#endregion


	public float getHitRateBoost_ByContinuousQuickComboCount()
	{
		if (ContinuousComboCount_QuickSkill > 2)
			return 0.1f;
		else if (ContinuousComboCount_QuickSkill > 4)
			return 0.2f;
		else if (ContinuousComboCount_QuickSkill > 9)
			return 0.3f;
		else if (ContinuousComboCount_QuickSkill > 13)
			return 0.4f;

		return 0;
	}

	
	public override float calcHitRate(Knowledge_Mortal_Fighter opponent)
	{
		float hitRate;
		//jks 2015.8.26 no more leader buff:  float buffHitRate = calculate_LeaderBuff_HitRate_Self(opponent.Class);
		//jks 2015.5.8 remove leader strategy-	float leaderStrategy = calculate_LeaderStrategy_HitRate();

	
		hitRate = base.calcHitRate(opponent);//jks 2015.8.26 no more leader buff:  + buffHitRate; //jks 2015.5.8 remove leader strategy-	 + leaderStrategy;


		float hitRateBoost = 0, hitRateBoost_Buff = 0;

		if (AllyID == eAllyID.Ally_Human_Me)
		{
			hitRateBoost = hitRate * getHitRateBoost_ByContinuousQuickComboCount();

			hitRateBoost_Buff = getLeaderBuffCriticalUp(hitRate);

			hitRate += hitRateBoost + hitRateBoost_Buff;
		}

		//Log.jprint("********* calcHitRate()    buff: "+ buffHitRate+"    leader strategy: "+leaderStrategy);

		#if UNITY_EDITOR
		if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
		{
			//jks 2015.8.26 no more leader buff:    Log.print(" 팀원 : " + gameObject.name +  " 추가 적중률 ->   버프: "+buffHitRate  + " = " + "  최종 적중률: " + hitRate);
			Log.print_always(" 팀원 : " + gameObject.name +  " 추가 적중률 ->   평타연속콤보: "+hitRateBoost +  "+ 리더버프: "+hitRateBoost_Buff  + " = " + "  최종 적중률: " + hitRate);
		}
		#endif


		return hitRate;
	}
	
	

	public override eHitType getFinalHitType(Knowledge_Mortal_Fighter opponent)
	{
		eHitType hitType;

		//2016.5.25	제거
//		//jks 2015.12.29 :맨티스 1069: 보스 타입을 제외한 일반 적에게는 '1회 공격'이 들어가는 경우를 제외, 모든 스킬의 마지막타는 무조건 크리티컬 처리
//		if (! opponent.IsBoss && TotalCombo > 1 && _skillCompleted)
//		{
//			hitType = eHitType.HT_CRITICAL;
//		}
//		else 

		//jks if class match, force set hit type to critical.
		if (BattleUI.Instance() != null && BattleUI.Instance().ClassMatchOwner == CardDeckIndex) 
		{
			hitType = eHitType.HT_CRITICAL;
		}
		else
		{
			hitType = base.getFinalHitType(opponent);
		}


		//jsm
		if (hitType == eHitType.HT_CRITICAL && !BattleBase.Instance.IsPVP) //jsm_0323 - 리더가 아니어도 크리티컬 표시 되도록 수정.
		{
			if (BattleBase.Instance.CriticalCount < CameraManager.Instance.MaxCritical)
			{
				BattleBase.Instance.CriticalCount ++;
			}
		}

		return hitType;
	}


//	protected bool _roles_skill_EverGaveDamage = false;
//
//	//jks 근접형  :  밀집분산역할
//	public void roleSkill_PushAway()
//	{
//		bool foundEnemy = BattleManager.Instance.findEnemiesNear(gameObject, _role_skill_hit_range, _opponentsInAttackRange);
//		if (! foundEnemy) return;
//		
//		//		if (gameObject.name.Contains("C"))
//		//			Log.jprint(Time.time+": "+ gameObject.name + ". # . # . closestOpponent: " + closestOpponent);
//		_roles_skill_EverGaveDamage = true;
//
//		foreach(GameObject go in _opponentsInAttackRange)
//		{
//			if (go == null) return;
//
//			//if (checkHeightIfReachable(go) == false) continue;
//			
//			Knowledge_Mortal_Fighter knowledgeOpponent = go.GetComponent<Knowledge_Mortal_Fighter>();
//			
//			eHitType hitType = (Random.Range(0,100) < 30) ? eHitType.HT_GOOD : eHitType.HT_CRITICAL;
//
//			int reactionChoice;
//
//			if (hitType == eHitType.HT_CRITICAL)
//			{
//				reactionChoice = Random.Range(4, 6);  //jks critical reaction only
//			}
//			else 
//			{
//				reactionChoice = Random.Range(0, 2);
//			}
//
//			int hitReactionAnimID = _anim_reaction[0, reactionChoice];
//
//            classRelTestFunc(knowledgeOpponent);
//
//            SkillType = eSkillType.ST_Role;
//            knowledgeOpponent.takeDamage(RoleSkillAttackPoint, hitReactionAnimID, hitType, AttackType, _weaponType_ForAnimation, gameObject, Random.Range(3,6));
//		}
//	}


//	//jks 지원형 :  공격차단역할
//	public void roleSkill_Stun()
//	{
//		bool foundEnemy = BattleManager.Instance.findEnemiesNear(gameObject, _role_skill_hit_range, _opponentsInAttackRange);
//		if (! foundEnemy) return;
//		
//		//		if (gameObject.name.Contains("C"))
//		//			Log.jprint(Time.time+": "+ gameObject.name + ". # . # . closestOpponent: " + closestOpponent);
//		_roles_skill_EverGaveDamage = true;
//
//		foreach(GameObject go in _opponentsInAttackRange)
//		{
//			if (go == null) return;
//			
//			//if (checkHeightIfReachable(go) == false) continue;
//			
//			Knowledge_Mortal_Fighter knowledgeOpponent = go.GetComponent<Knowledge_Mortal_Fighter>();
//
//			SkillType = eSkillType.ST_Role;
//			knowledgeOpponent.takeStun(_roleSkillDuration, 1);
//		}
//	}




//	public virtual void giveDamage_RoleSkill()
//	{
//		switch(RoleSkillType)
//		{
//			case SKILL_TYPE.TYPE_T:  // 근접형  :  밀집분산역할
//				roleSkill_PushAway();
//				break;
//			case SKILL_TYPE.TYPE_H:  // 지원형   공격차단역할
//				roleSkill_Stun();
//				break;
//		}
//	}

	//jks 평타 damage
	public override void giveDamage_QuickSkill(float reactionDistanceOverride)
	{

		bool isLastDamage = reactionDistanceOverride >= 1000;

		//jks 평타 리더 AI
		if (isLastDamage)
			AI_AutoCombo_QuickSkill = false;


		GameObject closestOpponent = getOpponentsInScanDistance_WeaponPositionBased();
		if (closestOpponent == null) return;

		Knowledge_Mortal_Fighter opponentKnowledge = closestOpponent.GetComponent<Knowledge_Mortal_Fighter>();		
		float finalDistToCheck = Mathf.Abs(transform.position.x - closestOpponent.transform.position.x) - this.Radius - opponentKnowledge.Radius - _weaponLength;
		
		if (finalDistToCheck > 0.3f) return;

		//jks increase quick skill continuous hit count       
		incrementContinuousHitCount();

		#if UNITY_EDITOR
		if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation) Log.print_always("   --------------------------------- 평타 (연속누적 : "+ _continuousHitCount_QuickSkill + ")   . . . . . . . . . . . . . . . . . ");
		#endif

		int hitReactionAnimID = getReactionAnimID_QuickSkill(ComboCurrent_QuickSkill);

		ObscuredInt finalAttack = Mathf.RoundToInt((isLastDamage ? _attackPointFinal_QuickSkill : _attackPoint_QuickSkill) * 0.25f); //jks 평타 공격력은 4로 나누어 사용.

		updateQuickComboUI_ByContinuousHit();
//		int continuousHitBoost = getAttackPowerBoost_ByContinuousHit(finalAttack);
//		finalAttack += continuousHitBoost;

		int classRelation = BattleBase.Instance.compareClassRelation(Class, opponentKnowledge.Class);


		if (classRelation == 1)
			finalAttack = finalAttack * 2;
		
		#if UNITY_EDITOR
		if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
		{
			Log.print_always("   평타 공격자 : " + gameObject.name + "  -->  피해자: " + closestOpponent.name);
			Log.print_always("   공격자 클래스 : " + Class + "  -->  피해자 클래스: " + opponentKnowledge.Class);
			if (isLastDamage)
			{
				if (classRelation == 1)
					Log.print_always("   G I V E  D A M A G E      평타 막타 공격력: " + _attackPointFinal_QuickSkill + "  ->   x 2 (클래스상성 우위)  =  " + finalAttack);
				else
					Log.print_always("   G I V E  D A M A G E      평타 막타 공격력: " + _attackPointFinal_QuickSkill + "  =  " + finalAttack);
			}
			else
			{
				if (classRelation == 1)
					Log.print_always("   G I V E  D A M A G E      평타 기본 공격력: " + _attackPoint_QuickSkill +  "  ->   x 2 (클래스상성 우위)  =  " + finalAttack);
				else
					Log.print_always("   G I V E  D A M A G E      평타 기본 공격력: " + _attackPoint_QuickSkill +  "  =  " + finalAttack);
			}
		}
    #endif

    	_skillType = eSkillType.ST_Quick;

        classRelTestFunc(opponentKnowledge);
        opponentKnowledge.takeDamage(finalAttack, hitReactionAnimID, eHitType.HT_GOOD, AttackType, _weaponType_ForAnimation, gameObject, reactionDistanceOverride);
	}




	public override void giveDamage(float reactionDistanceOverride)
	{		
		if (CameraManager.Instance == null) return;


		//jks 2016.3.11 skill buff 기능 추가.
		addSkillBuff();

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
		
		_everGaveDamageForTheAttack = true;


//		if (gameObject.name.Contains("C"))
//			Log.jprint(Time.time+": "+ gameObject.name + ". . . . . giveDamage(): ");

		_skillCompleted = isLastDamageInSkill(reactionDistanceOverride);


		GameObject closestOpponent = getOpponentsInScanDistance_WeaponPositionBased();
		if (closestOpponent == null) return;

//		if (gameObject.name.Contains("C"))
//			Log.jprint(Time.time+": "+ gameObject.name + ". # . # . closestOpponent: " + closestOpponent);

		Knowledge_Mortal opponentKnowledge = closestOpponent.GetComponent<Knowledge_Mortal>();



		#if UNITY_EDITOR
		//jks debugging gizmo -----------------------
		_meleeAttack_ShowAttackRange = true;
		_meleeAttack_Center_Opp = closestOpponent.transform.position;
		_meleeAttack_Radius_Opp = opponentKnowledge.Radius;
		//jks debugging gizmo -----------------------
		#endif


//		//jks - body based
//		float distShell_AttackerAndClosestOpponent = Mathf.Abs(transform.position.x - closestOpponent.transform.position.x) - this.Radius - opponentKnowledge.Radius;
//		//jks - weapon based
//		float distWeapon_AttackerAndClosestOpponent = Mathf.Abs(WeaponEndPosition.x  - closestOpponent.transform.position.x) - opponentKnowledge.Radius;
//		//jks 무기를 뒤로 휘두르는 경우 몸보다 더 뒤에 위치하기 때문에  이 경우는 몸 위치로 계산. 
//		float finalDistToCheck = Mathf.Min(distShell_AttackerAndClosestOpponent, distWeapon_AttackerAndClosestOpponent);
		float finalDistToCheck = Mathf.Abs(transform.position.x - closestOpponent.transform.position.x) - this.Radius - opponentKnowledge.Radius - _weaponLength;

		if (finalDistToCheck > 0.3f)
			return;


		//jks - 공격 중 적에게 최대공격가능거리보다 가까워지면, 가까워진 거리를 와 damage range를 기준으로 공격 받을 적 판단하기위한 값.
		//float damageDistance = Mathf.Min(_attackDistanceMax, finalDistToCheck);

		//foreach(GameObject go in _opponentsInAttackRange)
		foreach(FighterActor ea in Opponents)
		{
			GameObject go = ea._go;

			if (go == null) continue;
			if (!go.activeSelf) continue;

			//jks check damage range
			opponentKnowledge = go.GetComponent<Knowledge_Mortal>();
			if (opponentKnowledge == null) continue;


//			//jks - body based
//			distShell_AttackerAndClosestOpponent = Mathf.Abs(transform.position.x - go.transform.position.x) - this.Radius - opponentKnowledge.Radius;
//			//jks - weapon based
//			distWeapon_AttackerAndClosestOpponent = Mathf.Abs(WeaponEndPosition.x  - go.transform.position.x) - opponentKnowledge.Radius;
//			//jks 무기를 뒤로 휘두르는 경우 몸보다 더 뒤에 위치하기 때문에  이 경우는 몸 위치로 계산. 
//			finalDistToCheck = Mathf.Min(distShell_AttackerAndClosestOpponent, distWeapon_AttackerAndClosestOpponent);
			finalDistToCheck = Mathf.Abs(transform.position.x - go.transform.position.x) - this.Radius - opponentKnowledge.Radius - _weaponLength;


//			if (gameObject.name.Contains("C"))
//			{
//				Log.jprint("GGGGGGG  "+ transform.position.x + " : " + WeaponEndPosition.x + " : " + closestOpponent.transform.position.x);
//				Log.jprint(Time.time + ": " + gameObject.name + " found as closest: " + closestOpponent + " GGGGGGG " + "finalDistToCheck: " + finalDistToCheck + " < ? " + damageDistance); 
//			}

//			if (go == getCurrentTarget())
//			{
//				if (finalDistToCheck > damageDistance + _weaponLength)  //jks for the current target, give generous(+ weapon length) check
//					continue;
//			}
//			else 
//			{
//				if (finalDistToCheck > damageDistance + _weaponLength + _damageRange) 
//					continue;
//			}
			if (go != getCurrentTarget())
			{
				if (finalDistToCheck > 0.1f + _damageRange) 
					continue;
			}

			if (checkHeightIfReachable(go) == false) continue;

			Knowledge_Mortal_Fighter knowledgeOpponent = go.GetComponent<Knowledge_Mortal_Fighter>();


			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
			{
				Log.print_always("   --------------------------------- 팀원 ---------------------------------");
			}
			#endif

			eHitType hitType = getFinalHitType(knowledgeOpponent);
			
			int hitReactionAnimID = getReaction(hitType);

			ObscuredInt finalAttack = AttackPoint;

			if (IsLeader || knowledgeOpponent.IsLeader || TestOption.Instance()._classRelationBuffAll)
			{
				ObscuredInt classRelationAttackPoint = calculate_ClassRelation_AttackPoint(AttackPoint, knowledgeOpponent);
				ObscuredInt leaderBuffAttackPoint = getLeaderBuffAttackUp(finalAttack);
				//jks 2015.5.8 remove leader strategy-				int leaderStrategyAttack = calculate_LeaderStrategy_AttackPoint();
				

				finalAttack +=  classRelationAttackPoint + leaderBuffAttackPoint;//jks 2015.5.8 remove leader strategy-	 + leaderStrategyAttack;
				
				
				#if UNITY_EDITOR
				if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
				{
					//			Log.print(gameObject.name + ": giveDamage(); hitType : " + hitType + "    reactionAnimID : " + hitReactionAnimID);
					//Log.print("   --------------------------------- 팀원 ---------------------------------");
					if (BattleBase.Instance.LeaderTransform)
						Log.print_always("   현재 리더 클래스: "+ BattleBase.Instance.LeaderClass + "   :  " + BattleBase.Instance.LeaderTransform.gameObject.name);
					Log.print_always("   공격자 : " + gameObject.name + "  -->  피해자: " + knowledgeOpponent.name);
					Log.print_always("   공격자 클래스 : " + Class + "  -->  피해자 클래스: " + knowledgeOpponent.Class);
					Log.print_always("   G I V E  D A M A G E (leader)    기본 공격력: " + AttackPoint + 
									 "  +  클래스상성 공격력: " + classRelationAttackPoint + 
									 "  +  리더버프 공격력: " + leaderBuffAttackPoint + "  =  " + finalAttack);
				}
				#endif

			}
			else
			{
				#if UNITY_EDITOR
				if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
				{
					Log.print_always("   공격자 : " + gameObject.name + "  -->  피해자: " + knowledgeOpponent.name);
					Log.print_always("   G I V E  D A M A G E      기본 공격력: " + finalAttack);
				}
				#endif

			}


			if (BattleUI.Instance() != null)
			{
				if (IsLeader && BattleUI.Instance().ClassMatchOwner != -1)
				{
					float classMatchBoost = Inventory.Instance().CSlot[CardDeckIndex].SBlock.Value * 0.01f;
					finalAttack += Mathf.RoundToInt(finalAttack * classMatchBoost);

					#if UNITY_EDITOR
					if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
					{
						Log.print_always(" > > >  M A T C H  < < <        "   + Inventory.Instance().CSlot[CardDeckIndex].SBlock.Value +  
										 " % 증가    =   공격력:  " + finalAttack);
					}
					#endif
				}
			}


            BattleBase.Instance.incrementHitTypeCount(CardDeckIndex, hitType);

            //jks for non-boss : slow motion effect: condition = last combo, critical, opponent death.
            /* jsm_0415 - disable slow motion
			if (_skillCompleted && hitType == eHitType.HT_CRITICAL && !knowledgeOpponent.IsBoss)
			{
				if (knowledgeOpponent.CurrentSinsu < finalAttack) //jks means death
				{
					Time.timeScale = 0.1f;
					Invoke("resetTimeScale", 0.1f);
				}
			}
			*/

            classRelTestFunc(knowledgeOpponent);

			_skillType = eSkillType.ST_Card;
            knowledgeOpponent.takeDamage(finalAttack, hitReactionAnimID, hitType, AttackType, _weaponType_ForAnimation, gameObject, reactionDistanceOverride);
		}

		//jsm_0911 - slow motion
//		if (IsLeader && CameraManager.Instance.IsEventScene)
//		{
//			if (Time.timeScale != 1.0f) 
//			{
//				Time.timeScale = 1.0f;
//			}
//		}
	}







    

    #region Leader Switching

    int _HP_LossSinceLeader = 0;
	int _criticalCountSinceLeader = 0;


	public void resetCriticalCountSinceLeader()
	{
		_criticalCountSinceLeader = 0;
	}
	
	public void updateCriticalCountSinceLeader(eHitType hitType)
	{
		if(IsLeader && hitType == eHitType.HT_CRITICAL)
		{
			_criticalCountSinceLeader++;
		}
	}

	public bool isCriticalCountEnough()
	{
		return _criticalCountSinceLeader >= BattleTuning.Instance._leaderSwitchConditionCritical;
	}



	public void resetHPLossSinceLeader()
	{
		_HP_LossSinceLeader = 0;
	}

	public void updateHPLossSinceLeader(int delta)
	{
		if(IsLeader && delta < 0)
		{
			_HP_LossSinceLeader += -delta;
		}
	}

	public bool isHPLossMoreThanLimitPercent()
	{
		return (float)_HP_LossSinceLeader/(float)_max_hp > BattleTuning.Instance._leaderSwitchConditionHPLoss;
	}


	public override void updateHP(ObscuredInt delta, eHitType hitType)
	{
		base.updateHP(delta, hitType);

		if (Inventory.Instance() != null)
		{
			if (AllyID == eAllyID.Ally_Human_Me)
			{
				Inventory.Instance().CSlot[_cardDeckIndex].Now_HP = Current_HP;
			}
		}

		if (IsDead)
		{
			if (BattleUI.Instance() != null && AllyID == eAllyID.Ally_Human_Me)
				BattleUI.Instance().setClassMatchSkillTarget(false, CardDeckIndex);

			if (KnowledgeSSkill)
			{
				KnowledgeSSkill.hideSSWeaponAndShowDefault();
				KnowledgeSSkill.uninstallWeaponAll();
			}
		}

		updateHPLossSinceLeader(delta);
	}
	#endregion



	public override void takeStun(int duration, int stunAnimNum)
	{
		base.takeStun(duration, stunAnimNum);
		BattleBase.Instance.showUICoolTime_Stun(CardDeckIndex, duration);
	}
	public override void takeStun(float duration, int stunAnimNum)
	{
		base.takeStun(duration, stunAnimNum);
		BattleBase.Instance.showUICoolTime_Stun(CardDeckIndex, duration);
	}

	public override bool IsClassMatching
	{
		get { return BattleUI.Instance() != null && BattleUI.Instance().ClassMatchOwner == CardDeckIndex; }
	}



	public override ObscuredInt takeDamage(ObscuredInt damagePoint, int reactionAnimID, eHitType hitType, eAttackType attackType, 
		eWeaponType_ForAnimation weaponType, GameObject attacker,  float reactionDistanceOverride)
	{
		
		//jks 평타 리더 AI
		//		if (IsLeader && Progress_Action_Quick)
		//		{
		//			AI_AutoCombo_QuickSkill = false; // reset
		//		}

		if (IsClassMatching && AllyID == eAllyID.Ally_Human_Me) return 0;
		if (Action_SkillStaging) return 0;    

		if (IsLeader && BattleManager.Instance.IsSkillStagingInProgress)//jks 클래스 매치 스킬 중에는 리더가 데미지 받지 않게 합.
			return 0;

		//jks 20164.21 Mantis1448 :
		/*
		//jks 2016.1.11  자동리더가 쿨타임 중 맞으면, 평타 공격 시작. 
		if (IsLeader && isCoolingInProgress() &&
			!Progress_SkillAction && !AI_AutoCombo_QuickSkill)
		{
			if (BattleBase.Instance.IsPlay_AutoLeader || //jks 유저 자동 리더 이거나,
				AllyID != eAllyID.Ally_Human_Me) //jks 랭킹탑 상대 Bot 이면,
			{
				_rollDiceForAttackType = false;
				AI_AutoCombo_QuickSkill = true;
				Action_Run = true;
				Log.jprint(Time.time + "   " + gameObject.name + " - - - 쿨타임 중 공격 받아 평타 공격 전환. - - -   ");
			}
		}
		*/



		if (AllyID == eAllyID.Ally_Human_Me && BattleBase.Instance.IsInvincibleMode)
		{
			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
				Log.print_always(gameObject.name +" -------  팀원 무적 상태 ---------- damage : 0  ");
			#endif

			return 0;
		}



		//Log.print(gameObject.name + "< < < < <  : takeDamage(); from monster..." + "hitType : " + hitType);
		_recentHitType = hitType; ///jks save recent hit type

		IsLastDamageInSkill = isLastDamageInSkill(reactionDistanceOverride, attacker); //jks reset	

		//jks 2015.4.13 -		if (!IsLastDamageInSkill && hitType != eHitType.HT_CRITICAL) //jks 스킬 마지막 타이고 크리티컬이면 뒤로 밀리는 부분 생략.- (크리티컬 리액션은 root motion 사용하기 때문에 따로 미는 부분 생략.) 	
		applyReactionDistance(reactionDistanceOverride);


		if (hitType == eHitType.HT_BAD)
		{
			//			if (BattleUI.Instance() != null)
			//				BattleUI.Instance().addDemageUI(transform, 0, hitType);

			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
				Log.print_always("   N O     D A M A G E  -  hit type: "+ hitType + "   피해자: " + gameObject.name);
			#endif
			//jks move back little bit, even though no damage (just make it look natural.)
			//			Vector3 forceDirection = -transform.forward;
			//			Loco.pushObject(0.3f, forceDirection, 0.15f);	
			//Log.jprint(gameObject.name + "^^^^   ^^^^^   ^^^   Loco.pushObject(0.2f, forceDirection, 0.15f); ");

			//jsm - show bad effect
			//			eAttackType attackType = attacker.GetComponent<Knowledge_Mortal_Fighter>().AttackType;
			//			eWeaponType_ForAnimation weaponType = attacker.GetComponent<Knowledge_Mortal_Fighter>()._weaponType_ForAnimation;
			base.spawnDamageEffect( attacker, attackType, hitType, weaponType);
			return 0;
		}


		damagePoint = base.takeDamage(damagePoint, reactionAnimID, hitType, attackType, 
			weaponType,  attacker, reactionDistanceOverride);



		if (checkReaction(hitType)) //리액션 할 수 있는 조건..
		{
			if (AttackCoolTimer != null && !AttackCoolTimer.IsCoolingInProgress && !_everGaveDamageForTheAttack)
			{
				AttackCoolTimer.reset();
			}


			if (IsLeader)
			{
				resetContinuousHitCount();
				resetQuickSkill();
			}


			if (KnowledgeSSkill && KnowledgeSSkill.Progress_AnyAction)
			{
				KnowledgeSSkill.processCooltime();	
				//jks deactivate support skill weapon
				KnowledgeSSkill.hideSSWeaponAndShowDefault();
			}


			if (_animHitNumber > 0)
				goHitReaction();


			//jks 혹시 무기가 hide 된 경우 일 수 있으니 스킬 취소되는 이 시점에 다시 show 함.
			showWeapon(true);

			if (DoIHoldOpponent)
			{
				releaseOpponent();
			}
			//Log.jprint(gameObject.name + "^^^^   ^^^^^   ^^^   takeDamage() Action_Hit = true; ");
		}
		else
		{
			hitShake();
		}


		if (IsLeader && AllyID == eAllyID.Ally_Human_Me && hitType == eHitType.HT_CRITICAL)
		{
			resetContinuousHitCount();
			resetQuickSkill();
		}


		//jsm - camera zoom
		setZoom(0);
		//jsm - camera shake
		if (hitType == eHitType.HT_CRITICAL || hitType == eHitType.HT_GOOD)
		{
			if (!BattleBase.Instance.IsSkillStagingInProgress && TestOption.Instance()._isUsePlayerCamShake) 
				CameraShake.Shake(1, Vector3.one, new Vector3(0.1f, 0.1f, 0.1f), 0.2f, 70.0f, 0.2f, 1.0f, false);
		}


		updateCriticalCountSinceLeader(hitType);


		return damagePoint;
	}


	//jks 레이드 보스의 공격 받기.
	public ObscuredInt takeDamage_Blitz(ObscuredInt damagePoint, int reactionAnimID, eHitType hitType, GameObject attacker)
	{
		if (IsClassMatching && AllyID == eAllyID.Ally_Human_Me) return 0; 
		if (Action_SkillStaging) return 0;    //jks 2016.6.23 매치스킬 연출 중이면 damage 받지 않게 함.

		if (IsLeader && BattleManager.Instance.IsSkillStagingInProgress)//jks 클래스 매치 스킬 중에는 리더가 데미지 받지 않게 합.
			return 0;
				

		_recentHitType = hitType; ///jks save recent hit type


		IsLastDamageInSkill = true;


		if (hitType == eHitType.HT_BAD)
		{
			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
				Log.print_always("takeDamage_Blitz   N O     D A M A G E  -  hit type: "+ hitType + "   피해자: " + gameObject.name);
			#endif
			return 0;
		}


		ObscuredInt finalDamage = damage_Blitz(damagePoint, reactionAnimID, hitType, attacker);

	
		if (IsLeader && AllyID == eAllyID.Ally_Human_Me && hitType == eHitType.HT_CRITICAL)
		{
			resetContinuousHitCount();
			resetQuickSkill();
		}

		updateCriticalCountSinceLeader(hitType);

		return finalDamage;
	}


	//jks 레이드 보스의 공격 받기.
	private ObscuredInt damage_Blitz(ObscuredInt damagePoint, int reactionAnimID, eHitType hitType, GameObject attacker)
	{
		if (damagePoint <= 0)
			return 0;

		Attacker = attacker;


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

		ObscuredInt defense = getDistributedDefense(knowledgeOpponent.DamageFrequency);

		//jks 리더 버프 
		int leaderBuffDefensePoint = getLeaderBuffDefenseUp(defense);
		if (leaderBuffDefensePoint > 0)
		{
			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
				Log.print_always("takeDamage_Blitz  기본 방어:  "+ defense + "  + 리더 버프 추가치: " + leaderBuffDefensePoint + "   new 방어력: " + (defense+leaderBuffDefensePoint));
			#endif

			defense += leaderBuffDefensePoint;
		}
			
		int leaderBuffDefenseDown = getLeaderBuffDefenseDown(defense);
		if (leaderBuffDefenseDown > 0)
		{
			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
				Log.print_always("takeDamage_Blitz  기본 방어:  "+ defense + "  + 리더 버프 감소치: " + leaderBuffDefenseDown + "   new 방어력: " + (defense-leaderBuffDefenseDown));
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
				Log.print_always("takeDamage_Blitz   리더가 크리티컬 연타 맞으면 방어력 떨어진 new 방어력:  " + defense);
			#endif
		}


		_recentHitType = hitType;

		ObscuredInt finalDamage = damagePoint; //jks 2016.6.28 기획요청.		ObscuredInt finalDamage = getAttackPointByHitType(hitType, damagePoint); 
		ObscuredInt savedHitTypeAppliedDamage = finalDamage;

//		#if UNITY_EDITOR
//		if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
//			Log.print_always("takeDamage_Blitz   T A K E   D A M A G E  -      피해자: " + gameObject.name + "  공격: " + finalDamage + " -  방어: " + defense  + "   =  최종 Damage : "+ (finalDamage-defense) );
//		#endif


		//jks 2016.3.14 스킬 버프.
//		if (BattleBase.Instance != null)
//		{
//			#if UNITY_EDITOR
//			//int originalDefense = defense;
//			int originalDamage = finalDamage;
//			#endif
//			//jks 방어력 스킬 버프 적용.
////			float buffRate_defense = BattleBase.Instance.getSkillBuff_DefenseUp(AllyID) - BattleBase.Instance.getSkillBuff_DefenseDown(AllyID, knowledgeOpponent);
////			defense += Mathf.RoundToInt(defense * buffRate_defense);
//
//			//jks 공격력 스킬 버프 적용.
//			float buffRate_attack = BattleBase.Instance.getSkillBuff_AttackUp(AllyID) - BattleBase.Instance.getSkillBuff_AttackDown(AllyID, knowledgeOpponent);
//			finalDamage += Mathf.RoundToInt(finalDamage * buffRate_attack);
//
//			#if UNITY_EDITOR
//			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
//			{
//				//Log.print_always("takeDamage_Blitz   스킬버프 방어력 증감 적용       적용전: " + originalDefense + "      증감치: "+ (buffRate_defense * 100) + " %      적용후: "+ defense );
//				Log.print_always("takeDamage_Blitz   스킬버프 공격력 증감 적용       적용전: " + originalDamage + "      증감치: "+ (buffRate_attack * 100) + " %      적용후: "+ finalDamage );
//			}
//			#endif
//		}

//jks 2016.6.28 기획요청.		finalDamage -= defense;
		finalDamage = doMinimumDamageAdjustment(finalDamage, savedHitTypeAppliedDamage);


		#if UNITY_EDITOR
		if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
		{
			Log.print_always("takeDamage_Blitz   최소 공격력 적용        적용전: " + savedHitTypeAppliedDamage + "       적용후: finalDamage = "+ finalDamage );
		}
		#endif


		updateHP(-finalDamage, hitType);


//		if (checkReaction(hitType)) //리액션 할 수 있는 조건..
		if (gameObject.activeSelf)  //레이드 보스의 blitz 공격에 대한 리액션은 나와 있는 캐릭터들만.
		{
			if (AttackCoolTimer != null && !AttackCoolTimer.IsCoolingInProgress && !_everGaveDamageForTheAttack)
			{
				AttackCoolTimer.reset();
			}


			if (IsLeader)
			{
				resetContinuousHitCount();
				resetQuickSkill();
			}


			if (KnowledgeSSkill && KnowledgeSSkill.Progress_AnyAction)
			{
				KnowledgeSSkill.processCooltime();	
				//jks deactivate support skill weapon
				KnowledgeSSkill.hideSSWeaponAndShowDefault();
			}

			_animHitNumber = reactionAnimID;

			if (_animHitNumber > 0)
				goHitReaction();


			//jks 혹시 무기가 hide 된 경우 일 수 있으니 스킬 취소되는 이 시점에 다시 show 함.
			showWeapon(true);

//			if (DoIHoldOpponent)
//			{
//				releaseOpponent();
//			}
			//Log.jprint(gameObject.name + "^^^^   ^^^^^   ^^^   takeDamage() Action_Hit = true; ");
		}

		return finalDamage;
	}





	/// <summary>
	/// Hits the shake.- 피격되었지만 리액션하지 않는 조건상에 있을 때 흔들어 주기 위함.
	/// </summary>
	protected override void hitShake()
	{
		if (IsDead) return;

		if (Time.timeScale < 1) return ;

		if (IsBattleEnd) return;

		if (Action_Death || Action_DeathB)
		{
			iTween.Stop(gameObject);
			return;
		}

		iTween.ShakePosition(gameObject, iTween.Hash("x", BattleTuning.Instance._hitShake_range_team, 
													 "time", BattleTuning.Instance._hitShake_time_team, 
													 "easetype", BattleTuning.Instance._hitShake_easytype_team,
													 "ignoreTimeScale", false));
	}




//	protected virtual bool checkReaction(eHitType hitType)
//	{
//		if (hitType == eHitType.HT_CRITICAL)		// 크리티컬이면 무조건 리액션 함.
//			return true;
//		else if (Progress_SkillAction || Action_Run)// 스킬 사용 중이거나 달리는 중이면 리액션 안함.	
//			return false;
//		else if (IsLeader && (KnowledgeSSkill && KnowledgeSSkill.Progress_AnySkillAnimation)) //리더 보조스킬 사용 중이면 리액션 안함.
//			return false;
//		else if (Progress_Action_Quick) //평타일 사용 중에 리액션 안함.
//			return false;
//		else
//			return true;
//	}
	protected virtual bool checkReaction(eHitType hitType)
	{
		//jks 2016.3.9 실행 중인 스킬 info 에 리액션 하지 않는 것으로 설정되어 있으면, 크리티컬이라도 리액션 하지 않음.
		if (Progress_SkillAction && ReactionOff)
			return false;			

		//jks 2016.3.14 리더지원 스킬과 보스패턴 스킬도 reaction off 설정 적용.
		if (Progress_SupportSkillAnyAction && KnowledgeSSkill.ReactionOff)
			return false;

		if (hitType == eHitType.HT_CRITICAL)		// 크리티컬이면 무조건 리액션 함.
			return true;
			
		return false;
	}


	
	public void activateSkillFinishedEvent()
	{
		_onSkillFinished(this); //jks - send SkillFinished event
	}



	protected override void setNextComboActionFlag()
	{
			 if (ComboCurrent == 1  && TotalCombo > 1 ) { Action_Combo2  = true; }
		else if (ComboCurrent == 2  && TotalCombo > 2 ) { Action_Combo3  = true; } //jsm - reset zoom value 
		else if (ComboCurrent == 3  && TotalCombo > 3 ) { Action_Combo4  = true; }
		else if (ComboCurrent == 4  && TotalCombo > 4 ) { Action_Combo5  = true; }
		else if (ComboCurrent == 5  && TotalCombo > 5 ) { Action_Combo6  = true; }
		else if (ComboCurrent == 6  && TotalCombo > 6 ) { Action_Combo7  = true; }
		else if (ComboCurrent == 7  && TotalCombo > 7 ) { Action_Combo8  = true; }
	}

	//jks 평타 콤보 연결
	protected void setNextComboActionFlag_QuickSkill()
	{
		_quickSkillInputWindow_Open = false; //reset

		//if (!IsTargetDead)
		{
			#if UNITY_EDITOR
			if (BattleTuning.Instance._showQuickSkillLog)
				Log.print_always("     TotalCombo_QuickSkill :  "+ TotalCombo_QuickSkill);
			#endif

			if (ComboCurrent_QuickSkill == 1 && TotalCombo_QuickSkill > 1)
			{	
				#if UNITY_EDITOR
				if (BattleTuning.Instance._showQuickSkillLog)
					Log.print_always(Time.time + "     * * go  ->  combo 2 시작    (평타 input 받기 종료)");
				#endif
				Action_ComboQuick2 = true;
				return; 
			}
			else if (ComboCurrent_QuickSkill == 2 && TotalCombo_QuickSkill > 2)
			{
				#if UNITY_EDITOR
				if (BattleTuning.Instance._showQuickSkillLog)
					Log.print_always(Time.time + "     * * *  go  ->  combo 3 시작    (평타 input 받기 종료)  ");
				#endif
				Action_ComboQuick3 = true;
				return;
			}
			else if (ComboCurrent_QuickSkill == 3 && TotalCombo_QuickSkill > 3)
			{
				#if UNITY_EDITOR
				if (BattleTuning.Instance._showQuickSkillLog)
					Log.print_always(Time.time + "     * * * * *  go  ->  combo 4 시작    (평타 input 받기 종료)  ");
				#endif
				Action_ComboQuick4 = true;
				return;
			}
			else if (ComboCurrent_QuickSkill == 4 && TotalCombo_QuickSkill > 4)
			{
				#if UNITY_EDITOR
				if (BattleTuning.Instance._showQuickSkillLog)
					Log.print_always(Time.time + "     * * * * * * *  go  ->  combo 5 시작    (평타 input 받기 종료)  ");
				#endif
				Action_ComboQuick5 = true;
				return;
			}
			else if (ComboCurrent_QuickSkill == 5 && TotalCombo_QuickSkill > 5)
			{
				#if UNITY_EDITOR
				if (BattleTuning.Instance._showQuickSkillLog)
					Log.print_always(Time.time + "     * * * * * * * * * * go  ->  combo 6 시작    (평타 input 받기 종료)  ");
				#endif
				Action_ComboQuick6 = true;
				return;
			}
		}
	}



	public override void resetActionInfo()
	{
		//jks if support skill action finished
		if (KnowledgeSSkill && KnowledgeSSkill.Progress_AnyAction)
		{
			KnowledgeSSkill.resetActionInfo();
		}
		else
		{
			base.resetActionInfo();
		}

	}


	public virtual void resetQuickSkillAndStartMainSkill()
	{
		resetContinuousHitCount();
		StartCoroutine( startSkill() );
		resetQuickSkill();
	}

	protected virtual void checkForSkillStart()
	{
		//jks 쿨타임 중이 아니면,
		if (AttackCoolTimer != null && !AttackCoolTimer.IsCoolingInProgress)
		{
			AttackCoolTimer.reset();

			//jks 평타 리더 AI
			if (IsLeader && AmIAuto) // leader ? auto ?
			{
				if (AI_AutoCombo_QuickSkill) // 평타 중에,
				{
					#if UNITY_EDITOR
					if (BattleTuning.Instance._showQuickSkillLog)
						Log.print_always(gameObject.name + "     자동모드에서 평타 연타 카운트 : " + ContinuousComboCount_QuickSkill);
					#endif

					//jks 20164.21 Mantis1448 :						if (ContinuousComboCount_QuickSkill >= _ai_quick_skill_max_hit) //jks 자동모드에서 평타 시작 후 연타 카운트가 5 를 채우면 스킬을 사용하게 함.
					if (ComboCurrent_QuickSkill >= TotalCombo_QuickSkill) // 평타 마지막 콤보 끝나면,
					{
						#if UNITY_EDITOR
						if (BattleTuning.Instance._showQuickSkillLog)
							Log.print_always(gameObject.name + "     자동모드에서 평타 시작 후 평타 콤보 완료 되면 스킬 사용. ");//jks 20164.21 Mantis1448 : Log.print_always("     자동모드에서 평타 시작 후 연타 카운트 4 이상이면 스킬 사용. ");
						#endif

						resetQuickSkillAndStartMainSkill();
					}
					//jks 20164.21 Mantis1448 :							else
					//jks 20164.21 Mantis1448 :								_rollDiceForAttackType = false;
				}
				else if (! Progress_SkillAction) //스킬 사용 중이 아니면,
				{
					//resetQuickSkillAndStartMainSkill();
					Action_Run = true;
				}
			}
		}

	}


//jks 20164.21 Mantis1448 :	public bool _rollDiceForAttackType = true;

	protected override void doNextAction()
	{
		//processOnResetActionInfo();

		setNextComboActionFlag();
		

//		if (IsLeader)
//			Log.jprint(gameObject.name + "   doNextAction  c1: " + Action_Combo1 + " c2: " + Action_Combo2 + " c3: " + Action_Combo3 + " c4: " + Action_Combo4 + " c5: " + Action_Combo5 + " c6: " + Action_Combo6);

		if (IsLeader)
		{
			if (Action_InstallWeapon_Pre)
			{
				Action_InstallWeapon = true;
				Action_InstallWeapon_Pre = false;
				return;
			}

//			if (BattleBase.Instance.IsTeamRushFirstSkillUsed)
//				return;
		}
		 
		if (NoNextComboAction) //jks if no next action,
		{
//			if (_recentHitType == eHitType.HT_CRITICAL)
//				Log.jprint(" H I T : 	"+ _recentHitType +"    !  !  !");

			processOnUseCardEnd();


			//if (ComboRecent != 0)  //jks if skill ever initiated? and i m not the leader, then start skill cooltime
			//if (ComboRecent != 0 && !IsLeader)  //jks if skill ever initiated? and i m not the leader, then start skill cooltime
			//Log.jprint(gameObject.name + " doNextAction ........... _everGaveDamageForTheAttack: " + _everGaveDamageForTheAttack);				
			if (_everGaveDamageForTheAttack)
			{
				//Log.jprint(gameObject.name + " C O O L T I M E  S T A R T ........... ");				
				startCoolTimeAndResetComboFlag();
				_everGaveDamageForTheAttack = false;
			}
			else
			{
				checkForSkillStart();
			}

//jks 20164.21 Mantis1448 :			_rollDiceForAttackType = true; //reset


			//Log.jprint(gameObject.name + "          _rollDiceForAttackType: " + _rollDiceForAttackType);

//			if (IsAutoCombo_QuickSkill || AI_AutoCombo_QuickSkill)  //jks 평타 리더 AI
//			{
//				if (BattleTuning.Instance._showQuickLog)
//					Log.print(Time.time + "     o o o o  start auto combo 자동 콤보 시작  ");
//				setNextComboActionFlag_QuickSkill(); //jks 평타 콤보 연결
//			}



			if (IsLeader && KnowledgeSSkill)
			{
				if (KnowledgeSSkill.Progress_AnyAction)
				{
					KnowledgeSSkill.processCooltime();	

					//jks deactivate support skill weapon
					KnowledgeSSkill.hideSSWeaponAndShowDefault();
				}
			}


//			if (IsLeader && AttackCoolTimer_GuardUp)
//			{
//				if (isGuardUpActive() && !_roles_skill_EverGaveDamage && !AttackCoolTimer_GuardUp.IsCoolingInProgress)
//				{
//					AttackCoolTimer_GuardUp.reset();
//				}
//			}


//			if (_skillCompleted)  //jks if skill completed, then start skill cooltime
//			{
//				//Log.jprint(gameObject.name + " C O O L T I M E  S T A R T ........... _skillCompleted: " + _skillCompleted);				
//				startCoolTimeAndResetComboFlag();
//				return;
//			}
//
//			if (!isCoolingInProgress() && _recentHitType == eHitType.HT_CRITICAL) //jks 2014.9.25 if recently attacked by opponent with critical hit type, then start skill cooltime
//			{
//				//Log.jprint(gameObject.name + " C O O L T I M E  S T A R T ........... _skillCompleted: " + _skillCompleted);				
//				startCoolTimeAndResetComboFlag();
//				return;
//			}
		}

	}

	protected void processAuto_Or_AI_QuickSkill()
	{
		if (IsAutoCombo_QuickSkill || AI_AutoCombo_QuickSkill)  //jks 평타 리더 AI
		{
			//jks 2016.4.21: Mantis1448 : if (AI_AutoCombo_QuickSkill && ContinuousComboCount_QuickSkill >= _ai_quick_skill_max_hit)//jks even though the combo is auto, in ai mode, do not exceed max continuous hit.
			if (AI_AutoCombo_QuickSkill && ComboCurrent_QuickSkill >= TotalCombo_QuickSkill)
			{
				//jks 2016.4.21: Mantis1448 : resetContinuousHitCount();
				resetQuickSkill();
				return;
			}

			#if UNITY_EDITOR
			if (BattleTuning.Instance._showQuickSkillLog)
				Log.print_always(Time.time + "     o o o o  start auto combo 자동 콤보 시작  ");
			#endif
			setNextComboActionFlag_QuickSkill(); //jks 평타 콤보 연결
		}
	}


	protected override void keepOpponentAtWeaponEnd(GameObject opponent, float penetration)
	{
		if (KnowledgeSSkill && KnowledgeSSkill.Progress_AnyAction)
		{
			KnowledgeSSkill.keepOpponentAtWeaponEnd(opponent, penetration);
			return;
		}

		base.keepOpponentAtWeaponEnd(opponent, penetration);
	}





//	protected void startCoolTimeAndResetComboFlag()
//	{
//		startCoolTimer();
//		
//		_recentHitType = eHitType.HT_NONE;
//		_skillCompleted = false; // reset
//		ComboRecent = 0;
//		ComboCurrent = 0;
//	}

	//jks clean root motion movement z and y direction
	public void resetPosition()
	{
		Vector3 resetPosition = transform.position;
		resetPosition.y = 0;
		resetPosition.z = 0;
		transform.position = resetPosition;
	}


	protected void leaderMove()
	{
		//if (Progress_Skill) 
		{
			forceResetFlags();
		}

		if (AmIAuto)
		{
			Action_Run = true;
		}
		else
		{
			Action_Walk = false;
			Action_Idle = true;
		}
	}

	public virtual void processOnUseCardEnd()
	{
		if (IsLeader)
		{
			leaderMove();

			checkEnemy();

			Action_WalkBackTurn = false; //jks reset
		}
		else
		{
			//if (!ImmediateAttack)
			//Log.jprint(gameObject.name + "   0 0 0    Action_WalkBackTurn : "+Action_WalkBackTurn+"       Action_WalkBack: "+Action_WalkBack);
			if (Action_WalkBackTurn)
			{
				forceResetFlags();
				Action_WalkBack = true;
				Action_WalkBackTurn = false;
				//Log.jprint(gameObject.name + "   1 1 1    Action_WalkBackTurn : "+Action_WalkBackTurn+"       Action_WalkBack: "+Action_WalkBack);
			}
			else
			{
				forceResetFlags();
				Action_WalkBackTurn = true;
				//Log.jprint(gameObject.name + "   2 2 2    Action_WalkBackTurn : "+Action_WalkBackTurn+"       Action_WalkBack: "+Action_WalkBack);
			}

			//Log.jprint(gameObject.name +"000000  WalkBack !!!");
			
			activateSkillFinishedEvent();
		}
			

		if (BattleUI.Instance() != null)
		//jsm_0327 - 클래스 매치 스킬 시전중이었다면 스킬 발동 플래그 해제
		if (BattleUI.Instance().ClassMatchOwner == CardDeckIndex)
		{
			BattleUI.Instance().ClassMatchOwner = -1;
			BattleManager.Instance.attachCameraToLeader();
		}
//		Debug.Log("========= processOnUseCardEnd : " + gameObject.name);
	}





	public void startOrContinueQuickSkill()
	{
		//jks if i am in the middle of quick skill
		if (Progress_Action_Quick || Progress_Anim_QuickSkill)
		{
			if (_quickSkillInputWindow_Open) //jks if in input accepting period, start next combo immediately.
			{
				if (!IsAutoCombo_QuickSkill)  //jks if auto, ignore input.
				{
					#if UNITY_EDITOR
					if (BattleTuning.Instance._showQuickSkillLog)
						Log.print_always(Time.time + "   ---------   수동 콤보 들어가는 중 input combo.   ");
					#endif
					setNextComboActionFlag_QuickSkill();
				}
				else
				{
					#if UNITY_EDITOR
					if (BattleTuning.Instance._showQuickSkillLog)
						Log.print_always(Time.time + "   x x x x x   자동 콤보  ->  인풋 무시 auto combo -> ignore input  x x x x x   ");
					#endif
				}
			}
		}
		else
		{
			#if UNITY_EDITOR
			if (BattleTuning.Instance._showQuickSkillLog)
				Log.print_always(Time.time + "     *  go  ->  combo 1 시작 .. (평타 input 받기 종료) ");
			#endif

			_quickSkillInputWindow_Open = false; //reset

			Action_ComboQuick1 = true;
		}
	}


	public override IEnumerator startSkill()
	{
		if (canUseSkill() == false)
		{
			yield break;
		}


		if (BattleUI.Instance() != null)
		{
			bool isClassMatch = BattleUI.Instance().isClassMatchSkill(CardDeckIndex); //jks save value before "startClassMatchSkill" called.

			if (IsLeader && !CameraManager.Instance.IsBossDead)
			{
				//jsm_0325 - 자동 공격 중 리더 스킬 발동 시 클래스 매치 이벤트를 위해 
				if ( isClassMatch )
				{
					BattleUI.Instance().startClassMatchSkill( CardDeckIndex );
					//						jks 스킬 연출 시작.
					yield return StartCoroutine(BattleBase.Instance.showSkillStaging(CardDeckIndex));
				}else{
					//						BattleUI.Instance().setClassIconForMatchEvent( CardDeckIndex ); //150611_jsm - 기존 블럭 매치 시스템 사용 안함.
					BattleUI.Instance().checkBlock( CardDeckIndex );
				}
			}
		}

		Action_Combo1 = true;

		if (IsLeader)
		{
			GameObject leader_target = getClosestEnemy();
			BattleBase.Instance.updateLeaderTargetKnowledge(leader_target);

			BattleBase.Instance.showUICoolTime(CardDeckIndex);

			StartCoroutine(resetCooltime_CheckForAbnormalCase(CardDeckIndex));

			//jsm_0210 - 피버 이벤트를 버튼 발동 형식으로 변경.
			//			if (BattleBase.Instance.CriticalCount >= CameraManager.Instance.MaxCritical &&
			//			    !BattleBase.Instance.DialogBossAppeared)	
			//			{
			//
			//				CameraManager.Instance.startEventScene();
			//			}
		}
//		else
//		{
//			if (BattleTuning.Instance._teamMemberStaging && !BattleBase.Instance.IsRankingTower)
//			{
//				if (BattleBase.Instance.IsTeamRush && ! BattleBase.Instance.IsTeamRushFirstSkillUsed)
//				{
//					Log.jprint (" --------HOT HOT HOT--------- S T A R T   team rush... "); 
//					BattleBase.Instance.IsTeamRushFirstSkillUsed = true; //set
//
//					BattleBase.Instance.LeaderKnowledge.Action_Run = false;
//				}
//			}
//		}
	}


	//jks 2016.4.12 :  카드비활성화는 되었는데 쿨타임 시작하지 않고 카드 비활성화 풀리지 않는 현상. - 카페유저 제보는 되었지만,  재발생 안되어, 발생했다고 가정한 경우에 대한 처리.
	protected IEnumerator resetCooltime_CheckForAbnormalCase(int slotIndex)
	{
		//jks 포즈나 연출 중이면 기다림.
		while (! IsBattleInProgress || BattleBase.Instance.IsIgnoreButtonTouch || 
				BattleBase.Instance.IsDialogShowOff || Time.timeScale == 0) 
			yield return null;

		//jks 카드 터치 후 6초가 지나면 확인.
		yield return new WaitForSeconds(6);

		//jks 죽은 카드면 무시.
		if (BattleBase.Instance.isCardDead(slotIndex)) yield break;

		//jks 스킬 중이면 무시.
		if (Progress_SkillAction) yield break;

		//jks 쿨타임 시작했으면 무시.
		if (isCoolingInProgress()) yield break;

		CoolTimer.reset();
	}


	
//	public virtual void startSkillPreview()
//	{
//		if (! _canStartSkillPreview) return;  //jks to avoid start skill while previous blending. (in case fast click/touch input)
//
//		_canStartSkillPreview = false;
//
//		//Log.jprint(gameObject.name + "  0 0 0 0     startSkill initiated   > > > > > > > > >    "  + Time.time);
//
//		base.startSkill();
//	}

	public virtual void startSkillPreview()
	{
		if (! _canStartSkillPreview || Action_Run || AnimCon.isInTransition() || Progress_SkillAction) return;  //jks to avoid start skill while previous blending. (in case fast click/touch input)
		_canStartSkillPreview = false;

		//forceResetFlags();
		Action_Run = true;
	}




	protected bool _zoomOutFlag_Prev = false;
	
	protected void setZoomOutState(bool zoomOut)
	{
		//150720_jsm - true 면 적이 없음. 최대 줌 아웃 / false 면 적 발견. 줌 인
		CameraManager.Instance.event_DoZoomOut(zoomOut);
		/*
		if (zoomOut != _zoomOutFlag_Prev)
		{
			_zoomOutFlag_Prev = zoomOut;

			CameraManager.Instance.event_DoZoomOut(zoomOut);
		}
		*/
	}



	public override GameObject getOpponentsInScanDistance_WeaponPositionBased()
	{
		GameObject found = base.getOpponentsInScanDistance_WeaponPositionBased();
		
		//jks zoom out if no enemy.
		if (CameraManager.Instance != null && CameraManager.Instance.doIHaveCameraAttached(transform))
			setZoomOutState(found == null);//jks if no enemy. request zoom out.
		
		return found;
	}




	public override bool shouldIJumpBackToKeepDistance()
	{
		//jks 리더가 아니면, 자동으로 뒤로 후퇴하여 거리 확보하지 않음.
		if (!IsLeader)
		{
			Action_CoolingJump = false;
			return false;
		}

		//jks 이미 점프 중이면, 넘어감.
		if (Progress_CoolingJump) return false;

		Action_CoolingJump = false; //reset

		//jks 수동모드이면, 자동으로 뒤로 후퇴하여 거리 확보하지 않음.
		if (!AmIAuto) return false;
		
		//jks 공격 중이면, 자동으로 뒤로 후퇴하여 거리 확보하지 않음.
		if (Progress_SkillAnimation || Progress_SkillAction) return false;
		
		//jks 보스 대화 연출 중이면, 자동으로 뒤로 후퇴하여 거리 확보하지 않음.
		if (BattleBase.Instance.IsBossDialogStagingMode) return false;
		
		if (_target_attack == null) return false;

		if (_jumpBackDelay) return false;
		
		Knowledge_Mortal_Fighter targetKnow = _target_attack.GetComponent<Knowledge_Mortal_Fighter>();
		
		if (! targetKnow.Progress_SkillAction) return false;
		if (targetKnow.WeaponSlowMovement < 0.7f || targetKnow.SkillSpeedAdjustment < 0.7f) return false;  //jks 만약 상대가 slow 스킬에 걸려있는 경우는 상대가 공격이지만 뒤로 피할 필요없음.  
		
		if (! targetKnow.IsCloseAttack) return false;

		if ( (Action_Run || Action_Walk) && isTooCloseToFlee()) return false;


		if (!TooDangerous) //jks 위험상태가 아닌 경우에,  계속 위험상태가 아닐지 확인.
		{
			if (JumpBackCount == 0)
				if (amIInDamageRange()) //jks 적이 가깝고,
			{
				//if (_target_attack != null && _target_attack.GetComponent<Knowledge_Mortal_Fighter>().Progress_Action) //jks  적이 공격 중이면,
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
		
		_jumpBackDelay = true;
		Invoke("resetJumpBackDelay", 1.5f); //jks 후진 최소 interval 1.5초. 

		Action_CoolingJump = true;

		return true;
	}

	


	public override bool shouldIJumpBack()
	{
		return
//				(
//					IsLeader //jks only for leader
//					&& (BattleManager.Instance.Strategy != eBattleStrategy.BS_Default) //jks - do if not manual mode
//					//jks - do during cooltime
//					&& (AttackCoolTimer.IsCoolingInProgress || Progress_CoolingJump)  //jks even cool time is finished earlier, keep playing cooling jump animation.
//					&& ! CoolingJumpDone
//					&& (! isFarEnoughNotToJumpBack() || Progress_CoolingJump)  //jks for user character, if too far from target, skip cooling jump
//				)
//				|| 
				shouldIJumpBackToKeepDistance()

				;
	}

	public virtual bool manualJumpBack()
	{
		//jks - do if user touch back button
		return 
			(
				IsLeader && 
				BattleManager.Instance.ForceMoveLeaderBackward &&
				! IsBossDialogStagingMode &&
				! BattleManager.Instance.IsIgnoreButtonTouch &&
				! Progress_SkillAction && //jks 리더 스킬 터치하자 마자 후진 넣을 경우 ForceMoveLeaderBackward가 true가 되더라도, 여기서 막음.
				! Progress_Action_Quick &&
				! Progress_SupportSkillAnyAction
			);
	}

	public override bool shouldIRun()
	{
		return
			!Progress_SkillAnimation
			&& !BattleBase.Instance.ProgressClassMatchAttack
			&&
			(
				Action_Run 
				|| (IsLeader && BattleBase.Instance.ForceMoveLeaderRun && !AmIAuto)  //jks 수동 리더 조작 이동.
				|| (IsLeader && LeaderCoolingRun && AmIAuto) //jks 자동 리더가 쿨링 중 적이 없어 전진.
				|| (IsLeader && !amIInView())
			);
	}


	public override bool isCoolingInProgress()
	{
		if (_coolTimer == null) return true;

		return _coolTimer.IsCoolingInProgress;
	}
	



	public override void showWeaponForTheFirstTime()
	{
//jks 2015.11.4 무기장착 제거.		if (!BattleBase.Instance.IsWeaponInstallationStagingInProgress)
			BattleManager.Instance.IsIgnoreButtonTouch = false;

		if (_weaponObject == null) return;

		Weapon_Hold weapon = _weaponObject.GetComponent<Weapon_Hold>();

		if (weapon == null) return;

		weapon.show(true);
	}


	public override IEnumerator installWeapon()
	{
		if (_weaponPathID == 0) yield break; //jks means no weapon
		
		
		Table_Path tablePath = (Table_Path)TableManager.GetContent(_weaponPathID);
		//jks_weaponObject = Utility.instantiateObjectSimple(ResourceManager.GetResource(tablePath._assetPath)) as GameObject;
		//yield return StartCoroutine(ResourceManager.co_InstantiateFromBundle(tablePath._assetPath, result => _weaponObject = (GameObject)result, 30));

		Vector3 position = new Vector3(0,0,0);

		//jks 스킬프리뷰에서 첫 프레임에 보이지 않게 하기 위함. //////////////  >>>>
		if (BattleBase.Instance.isSkillPreviewMode())
			position.x = 1000;
		//jks 스킬프리뷰에서 첫 프레임에 보이지 않게 하기 위함. //////////////  <<<<<

		yield return StartCoroutine(ResourceManager.co_InstantiateFromBundle(tablePath._assetPath, position, Quaternion.identity, result => _weaponObject = (GameObject)result, 30));

		if (_weaponObject == null)
		{
			Debug.LogError("Can't find asset : "+ tablePath._assetPath);
		}
		
		Weapon_Hold weapon = _weaponObject.GetComponent<Weapon_Hold>();

		if (weapon == null)  //jks throw 타입 경우 예) 폭탄.  는 weapon script 없을 수 있음.
		{
			Debug.LogWarning("Can't find Weapon script on (throw 타입 무기 경우는 없을 수 있음.) "+ tablePath._assetPath);

			Destroy(_weaponObject);

			yield break;
		}

		//jks 스킬프리뷰에서 첫 프레임에 보이지 않게 하기 위함. //////////////  >>>>
		if (BattleBase.Instance.isSkillPreviewMode())
		{
			weapon.show(false);
			_weaponObject.transform.position += new Vector3 (-1000, 0, 0);
		}
		//jks 스킬프리뷰에서 첫 프레임에 보이지 않게 하기 위함. //////////////  <<<<<
			
		weapon.setPairItemPath(tablePath._assetPath);
		yield return StartCoroutine(weapon.install(gameObject));
		//weapon.hide(); 
		//_isWeaponVisible = false;

		if (IsLeader)
			weapon.hide();
		//Log.jprint(gameObject.name + "   install weapon :  " + _weaponObject);
	}



	public override bool getOpponentsInScanDistance(bool bQuickSkill)
	{
//1126		if (BattleBase.Instance.EnemyToKill == 0) 
//		{
//			//showWeapon(false);
//			return false;
//		}

		//jks - 가장 가까운 적을 기준으로 damage range 적용을 위한 준비.
		GameObject closestOpponent = BattleBase.Instance.findClosestOpponent(AllyID, transform.position.x);
		setTarget(closestOpponent);

		if (closestOpponent == null)
		{
			return false;
		}

		Knowledge_Mortal_Fighter opponentKnowledge = closestOpponent.GetComponent<Knowledge_Mortal_Fighter>();
		
		float distShell_AttackerAndClosestOpponent = Mathf.Abs(transform.position.x - closestOpponent.transform.position.x) - this.Radius - opponentKnowledge.Radius;
		
		eBotType opponentType = opponentKnowledge.getBotType();
		if (IsLeader)
		{	
//jks 2015.11.4 보스액션 제거.
//			if (opponentType == eBotType.BT_Boss || opponentType == eBotType.BT_Boss_Story || opponentType == eBotType.BT_Boss_Raid) //jks - if i m leader and closest opponent is boss
//			{
//				//jks - check if boss action is not done,  do not attack.
//				if (((Knowledge_Mortal_Fighter_Bot)opponentKnowledge).WaitForBossAction)
//					return false;
//			}
		}
		else
		{
			//jks - if opponent is Boss and leader is behind me , do not attack the boss yet.
			if (opponentType == eBotType.BT_Boss_Story || opponentType == eBotType.BT_Boss_Leader)
			{
				if (distShell_AttackerAndClosestOpponent < _attackDistanceMax 
				    && BattleBase.Instance.LeaderTransform
				    && (! BattleBase.Instance.IsBossDialogFinished)
					//jks 2015.11.4 보스액션 제거.  || ((Knowledge_Mortal_Fighter_Bot)opponentKnowledge).WaitForBossAction) //jks - do only before dialog
				    )
				{
					if (transform.position.x > BattleBase.Instance.LeaderTransform.position.x)
					{
						forceResetFlags();  	Action_WalkBackTurn = true;
						return false;
					}
				}
			}
//jks 2015.11.4 보스액션 제거.  
//			else if (opponentType == eBotType.BT_Boss)
//			{
//				if (distShell_AttackerAndClosestOpponent < _attackDistanceMax 
//				    && ((Knowledge_Mortal_Fighter_Bot)opponentKnowledge).WaitForBossAction
//				    )
//				{
//					//if (transform.position.x > BattleBase.Instance.LeaderTransform.position.x)
//					{
//						forceResetFlags();  	Action_WalkBackTurn = true;
//						return false;
//					}
//				}
//			}
		}



		
		//_isTargetInShowWeaponRange = (distShell_AttackerAndClosestOpponent < _attackDistanceMax + 4); //jks too show weapon if target is closer than 3 meter.
		//showWeapon(_isTargetInShowWeaponRange);


		float finalAttackDistanceMax = _attackDistanceMax;

		if (bQuickSkill)
			finalAttackDistanceMax = _attack_distance_max_QuickSkill;  //jks 평타 전용 공격 거리.



		if (finalAttackDistanceMax < distShell_AttackerAndClosestOpponent) return false;
		
		
		
		
		return true;
	}



	protected override void LateUpdate()
	{
		if (IsBattleEnd) return;
		
		if (Progress_SkillAnimation
			|| (KnowledgeSSkill && KnowledgeSSkill.Progress_AnyAction)
			|| Progress_Action_Quick)
		{
			//Profiler.BeginSample("000");
			pushOpponentWhenIAttack();
			//Profiler.EndSample();
		}
		else if (Action_Run || Action_Walk || Action_WalkFast || BattleBase.Instance.ForceMoveLeaderRun || LeaderCoolingRun)
		{
			//GameObject target = getCurrentTarget();
			pushPassedOpponentsWhenIMove();
			
		}

		pauseProcess();

		
//jks 2015.11.4 보스액션 제거.		checkBossAction();
	}



	protected override void pushPassedOpponents()
	{
		//if (AmICaptured) return;  // if i m captured, do not push.
		if (IsDead) return;
		if (AmICaptured) return;
		
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



	protected override void pushPassedOpponentsWhenIMove()
	{
		if (IsDead) return;
		if (!IsBattleTimeStarted) return;
		if (AmICaptured) return;


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

}
