using UnityEngine;
using System.Collections;
using CodeStage.AntiCheat.ObscuredTypes;

public class Knowledge_Mortal_Fighter_Main_Meteor : Knowledge_Mortal_Fighter_Main
{
	//jks AttackInfo : delay in second to start damage

	//jks AttackInfo2 : damage duration in second. (table)1 = (code)0.01  
	float _damage_duration;

	float _damage_interval;

	

	public override void setAttributesFromTable(Table_Skill tbl)
	{
		base.setAttributesFromTable(tbl);
		
		_damage_duration = AttackInfo2 * 0.01f;

		_damage_interval = AttackInfo3 * 0.1f;
		if (_damage_interval == 0)
			_damage_interval = 0.05f; //jks give minimum interval


		_damage_frequency = Mathf.RoundToInt(_damage_duration / _damage_interval);
		if (_damage_frequency < 1)
			_damage_frequency = 1;


		#if UNITY_EDITOR
		if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
		{
			Log.print_always("   M E T E O R   T Y P E   setAttributesFromTable() ");
			Log.print_always("   _damage_duration " + _damage_duration + " /  _damage_interval: " + _damage_interval  + "  = "+_damage_frequency);
		}
		#endif


	}


	public override bool amIMelee()
	{
		return false;
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


//		Log.jprint(Time.time + "  Give Damage Meteor ");
		
		_everGaveDamageForTheAttack = true;

		_skillCompleted = false; //jks 2016.5.23 meteor 경우 항상 크리티컬 발생하는 이슈 방지.:   isLastDamageInSkill(reactionDistanceOverride);

		_damageCount=0;  //reset

		StartCoroutine(giveDamageDelayed(reactionDistanceOverride));

	}


	protected IEnumerator giveDamageDelayed(float reactionDistanceOverride)
	{
		float delay = AttackInfo;
		yield return new WaitForSeconds(delay);

		StartCoroutine(continuousDamage(reactionDistanceOverride));
	}


	protected float _startTime;
	protected int _damageCount=0;
	protected IEnumerator continuousDamage(float reactionDistanceOverride)
	{
		if (_damage_duration == 0) yield return null;

		_startTime = Time.time;

		float initialCenter = transform.position.x;

		while (Time.time - _startTime < _damage_duration) 
		{
			if (++_damageCount == DamageFrequency) //jks 2016.5.23 막타면,
				_skillCompleted = true;

			//Log.jprint(Time.time +"  _damage_duration:"+_damage_duration+"   _damage_interval: "+"   DamageFrequency: "+ DamageFrequency + "  _damageCount: "+ _damageCount);

			giveAreaDamageNow(reactionDistanceOverride, initialCenter);

			yield return new WaitForSeconds(_damage_interval);

#if UNITY_EDITOR
			//jks debugging 
			_arealAttack_ShowDamageRange = false;
#endif
		}

		//jks this prevents playing animation completely. so can't get finish event. comment -> //processOnUseCardEnd(); 

	}


	protected override void doNextAction()
	{
		//processOnResetActionInfo();
		
		setNextComboActionFlag();


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


		//		if (IsLeader)
		//			Log.jprint(gameObject.name + "   doNextAction  c1: " + Action_Combo1 + " c2: " + Action_Combo2 + " c3: " + Action_Combo3 + " c4: " + Action_Combo4 + " c5: " + Action_Combo5 + " c6: " + Action_Combo6);
		
		if (NoNextComboAction) //jks if no next action,
		{
			//			if (_recentHitType == eHitType.HT_CRITICAL)
			//				Log.jprint(" H I T : 	"+ _recentHitType +"    !  !  !");
			
			processOnUseCardEnd();				 

//			if (ComboRecent != 0)  //jks if skill ever initiated
//			{
//				startCoolTimeAndResetComboFlag();
//				return;
//			}

			if (_everGaveDamageForTheAttack)
			{
//				Log.jprint (Time.time +  "   !  start cool time  !    " + gameObject);

				startCoolTimeAndResetComboFlag();
				_everGaveDamageForTheAttack = false;
			}
			else
			{
				checkForSkillStart();
			}

			//jks 20164.21 Mantis1448 :			_rollDiceForAttackType = true; //reset


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


	protected override void giveAreaDamageNow(float reactionDistanceOverride, float initialCenter)
	{
		if (CameraManager.Instance == null) return;
		
//		_opponentsInAttackRange.Clear(); //reset
		
		//jks bool found = HaveOpponentAhead(10 ,"Fighters", _opponentsInAttackRange);	
		//jks if (!found) return;
		if (Opponents.Count <= 0) return;
		
		//Log.jprint(gameObject.name + "    1111111   damage center: "+ initialCenter);
		
		float areaCenter;
		bool headingRight = transform.forward.x > 0;
		if (headingRight)
		{
			areaCenter = initialCenter + _attackDistanceMax;
		}
		else
		{
			areaCenter = initialCenter - _attackDistanceMax;
		}
		
		//Log.jprint(gameObject.name + "    2222222   damage center: "+ areaCenter);
		
		#if UNITY_EDITOR
		//jks debugging
		_arealAttack_DamageCenter = transform.position;
		_arealAttack_DamageCenter.x = areaCenter;
		_arealAttack_ShowDamageRange = true;
		_arealAttack_Radius = _damageRange;
		#endif
		
		//jks foreach(GameObject go in _opponentsInAttackRange)
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
			if (distFromCenter > _damageRange) continue;
			
			
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
			
			if (IsLeader || knowledgeOpponent.IsLeader || TestOption.Instance()._classRelationBuffAll)
			{
				ObscuredInt classRelationAttackPoint = calculate_ClassRelation_AttackPoint(AttackPoint, knowledgeOpponent);
				//jks 2015.8.26 no more: int leaderBuffAttackPoint = calculate_LeaderBuff_AttackPoint_Opponent();
				//jks 2015.5.8 remove leader strategy-				int leaderStrategyAttack = calculate_LeaderStrategy_AttackPoint();
				
				
				finalAttack = AttackPoint + classRelationAttackPoint;//jks 2015.8.26 no more:  + leaderBuffAttackPoint;//jks 2015.5.8 remove leader strategy-	 + leaderStrategyAttack;
				
				#if UNITY_EDITOR
				if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
				{
					Log.print_always("   현재 리더 클래스: None ");
					Log.print_always("   공격자 : " + gameObject.name + "  -->  피해자: " + knowledgeOpponent.name);
					Log.print_always("   공격자 클래스 : " + Class + "  -->  피해자 클래스: " + knowledgeOpponent.Class + "   피격 타입: " + hitType);
					Log.print_always("   G I V E  D A M A G E      Original: "+ AttackPoint + "    + 클래스 상성 공격력: "+classRelationAttackPoint+  " = " + finalAttack);
				}
				#endif
			}
			else
			{
				#if UNITY_EDITOR
				if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
				{
					Log.print_always("   피격 타입: " + hitType);
					Log.print_always("   G I V E  D A M A G E     : " + finalAttack);
				}
				#endif
			}


			if (BattleUI.Instance() != null)
			{
				if (IsLeader && BattleUI.Instance().ClassMatchOwner != -1)
				{
					ObscuredFloat classMatchBoost = Inventory.Instance().CSlot[CardDeckIndex].SBlock.Value * 0.01f;
					finalAttack += Mathf.RoundToInt(finalAttack * classMatchBoost);
					
					#if UNITY_EDITOR
					if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
					{
						Log.print_always(" > > >  M A T C H  < < <        "   + Inventory.Instance().CSlot[CardDeckIndex].SBlock.Value +  " % 증가    =   공격력:  " + finalAttack);
					}
					#endif
				}
			}
            
            classRelTestFunc(knowledgeOpponent);

			SkillType = eSkillType.ST_Card;
            knowledgeOpponent.takeDamage(finalAttack, hitReactionAnimID, hitType, AttackType, _weaponType_ForAnimation, gameObject, reactionDistanceOverride);
		}
		
	}

}
