
using UnityEngine;
using System.Collections;
using CodeStage.AntiCheat.ObscuredTypes;

public class Knowledge_Mortal_Fighter_Main_Projectile : Knowledge_Mortal_Fighter_Main 
{



	protected override IEnumerator setLauncher()
	{

		//Launcher_Projectile[] launchers = GetComponents<Launcher_Projectile>();

		Launcher_Projectile foundLauncher = null;

		while (foundLauncher == null)
		{
			yield return new WaitForEndOfFrame();

			Launcher_Projectile[] launchers = GetComponents<Launcher_Projectile>();

			if (AttackType == eAttackType.AT_Stun)
				foundLauncher = findLauncher(launchers, "Launcher_Projectile_Stun");
			else if (AttackType == eAttackType.AT_Homing)
				foundLauncher = findLauncher(launchers, "Launcher_Projectile_Homing");
			else
				foundLauncher = findLauncher(launchers, "Launcher_Projectile_Basic");
		}

		_launcher = foundLauncher;

	}


	Launcher_Projectile findLauncher(Launcher_Projectile[] launchers, string launcherName)
	{
		Launcher_Projectile found = null;	

		for (int i=0; i < launchers.Length; i++)
		{
			if (launchers[i].LauncherTypeName == launcherName)
			{
				found = launchers[i]; 
				break;
			}
		}
		return found;
	}


	public override eHitType getFinalHitType(Knowledge_Mortal_Fighter opponent)
	{
		float finalHitRate;
		
		finalHitRate = calcHitRate(opponent);
		
		if (finalHitRate < 0) finalHitRate = 0;
		
		eHitType hitType = judgeHitState(finalHitRate);

		if (BattleUI.Instance() != null)
			if (BattleUI.Instance().ClassMatchOwner == CardDeckIndex)
				hitType = eHitType.HT_CRITICAL;

		//jsm
		if (hitType == eHitType.HT_CRITICAL && IsLeader) 
		{
			if (BattleBase.Instance.CriticalCount < CameraManager.Instance.MaxCritical)
			{
				BattleBase.Instance.CriticalCount ++;
			}
		}

		return hitType;
	}
	


	public override int getReaction(eHitType hitType)  //jks TODO : is this function ever called?
	{
		int hitReactionAnimID;
		
		if (hitType == eHitType.HT_CRITICAL)
		{
			hitReactionAnimID = getReactionAnimID(3, hitType);
		}
		else 
		{
			int index = ComboCurrent;
			if (ComboCurrent == 3)
			{
				index = 2;  //jks avoid always critical hit by combo number since combo3 is shooting on projectile
			}
			hitReactionAnimID = getReactionAnimID(index, hitType); 
		}

		return hitReactionAnimID;
	}


	

//	protected override bool isAttackerSuperiorClass(CardClass attacker, CardClass victim)
//	{
//		return _isSuperiorClass_Distance[(int)attacker, (int)victim] == 1;
//	}

	public override bool amIMelee()
	{
		return false;
	}

	public override bool canIAttackFlyingOpponent()
	{
		return true;
	}
	




//	public override void takeDamage(int damagePoint, int reactionAnimID, eHitType hitType, GameObject attacker)
//	{
//		base.takeDamage(damagePoint,reactionAnimID, hitType, attacker);
//
//		endAim();
//	}

	public override bool checkEnemyOnPath(bool bQuickSkill)
	{
//		if (isGuardUpActive())
//		{
//			return BattleManager.Instance.findEnemiesNear(gameObject, _role_skill_attack_distance);
//		}
//		else
		{
			return getOpponentsToAttackWhileMoving(bQuickSkill);
		}
	}



	public override bool getOpponentsInScanDistance(bool bQuickSkill)
	{
//1126		if (BattleBase.Instance.EnemyToKill == 0) 
//		{
//			//showWeapon(false);
//			return false;
//		}

		float scanDistance = _attackDistanceMax;

		if (bQuickSkill && AttackType_QuickSkill == 0) // if 평타? and melee type ?
			scanDistance = 1.0f;
		
		//jks - 가장 가까운 적을 기준으로 damage range 적용을 위한 준비.
		GameObject closestOpponent = BattleBase.Instance.findClosestOpponent(AllyID, transform.position.x);
		setTarget(closestOpponent);
		
		if (closestOpponent == null) 
		{
			if (IsLivingWeapon && ! BattleBase.Instance.isInCameraView(gameObject))
			{
				Action_Paused = true; // do nothing
				Destroy(gameObject, 0.5f);
			}
			return false;
		}

		Knowledge_Mortal_Fighter opponentKnowledge = closestOpponent.GetComponent<Knowledge_Mortal_Fighter>();
		
		float distShell_AttackerAndClosestOpponent = Mathf.Abs(transform.position.x - closestOpponent.transform.position.x) - this.Radius - opponentKnowledge.Radius;
		

		eBotType opponentType = opponentKnowledge.getBotType();

		if (IsLeader)
		{	
//jks 2015.11.4 보스액션 제거.
//			if (opponentType == eBotType.BT_Boss || opponentType == eBotType.BT_Boss_Story || opponentType == eBotType.BT_Boss_Raid)//jks - if i m leader and closest opponent is boss,
//			{
//				//jks - check if boss action is not done,  do not attack.
//				if (((Knowledge_Mortal_Fighter_Bot)opponentKnowledge).WaitForBossAction)
//					return false;
//			}
		}
		else
		{
			//jks - if opponent is Boss and leader is behind me , do not attack the boss yet.
			if (opponentType == eBotType.BT_Boss_Story 
				|| opponentType == eBotType.BT_Boss_Leader )
			{
				if (distShell_AttackerAndClosestOpponent < scanDistance 
				    && BattleBase.Instance.LeaderTransform
				    && ! BattleBase.Instance.IsBossDialogFinished  //jks - do only before dialog
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
//					&& ((Knowledge_Mortal_Fighter_Bot)opponentKnowledge).WaitForBossAction
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

		
		//_isTargetInShowWeaponRange = (distShell_AttackerAndClosestOpponent < scanDistance + 4); //jks too show weapon if target is closer than 3 meter.
		//showWeapon(_isTargetInShowWeaponRange);


		float finalAttackDistanceMax = _attackDistanceMax;

		if (bQuickSkill)
			finalAttackDistanceMax = _attack_distance_max_QuickSkill;  //jks 평타 전용 공격 거리.


		if (finalAttackDistanceMax < distShell_AttackerAndClosestOpponent) return false;

		
		
		
		return true;
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
//		Launcher_Projectile launcher;
//
//		if (AttackType == eAttackType.AT_Stun)
//			launcher = GetComponent<Launcher_Projectile_Stun>();
//		else if (AttackType == eAttackType.AT_Homing)
//			launcher = GetComponent<Launcher_Projectile_Homing>();
//		else
//			launcher = GetComponent<Launcher_Projectile_Basic>();
//
//
//		if (launcher == null) 
//		{
//			Log.Warning("Can not find Launcher_Projectile component. ");
//			return;
//		}


		updateProjectileTarget();
		if (_projectileOriginalTarget == null)
		{
			_projectileOriginalTarget = BattleBase.Instance.findClosestOpponent(AllyID, transform.position.x);
			setTarget(_projectileOriginalTarget);
			//updateProjectileTarget();
		}

		if (_projectileOriginalTarget == null)
		{
			if (IsLeader && !BattleBase.Instance.IsPlay_AutoLeader)
			{
				//jks  수동 리더가 발사체 스킬 사용하면 , air shot 발사되게 하기 위해 이 시점에 return 하지 않음.
			}
			else
			{
				return;
			}
		}

		startAim();

//jks 2015.12.09: 일반spawner 생성된 적 있을 수 있음.		if (BattleBase.Instance.EnemyToKill != 0) 
		{
			//Log.jprint("giveDamage()");
			SkillType = eSkillType.ST_Card;
			StartCoroutine( _launcher.spawn(_projectileOriginalTarget, AttackInfo, AttackType, _weaponType_ForAnimation, reactionDistanceOverride));
		}

	}




	protected ObscuredInt _projectile_Damage_QuickSkill;
	public ObscuredInt Projectile_Damage_QuickSkill { get { return _projectile_Damage_QuickSkill; }}

	public override void giveDamage_QuickSkill(float reactionDistanceOverride)
	{
		if ( AttackType_QuickSkill == 1 ) // 1: 발사체 0: melee
		{
			Launcher_Projectile launcher = GetComponent<Launcher_Projectile_Basic>();
			if (launcher == null) {Log.Warning("Can not find Launcher_Projectile component. "); return;}

			updateProjectileTarget();
			if (_projectileOriginalTarget == null)
			{
				_projectileOriginalTarget = BattleBase.Instance.findClosestOpponent_QuickSkill(transform.position.x);
				setTarget(_projectileOriginalTarget);
				//updateProjectileTarget();
			}

			if (_projectileOriginalTarget == null) return;

			startAim();


			bool isLastDamage = reactionDistanceOverride >= 1000;
//jks2016.1.8			int finalAttack = (isLastDamage ? _attackPointFinal_QuickSkill : _attackPoint_QuickSkill);
			ObscuredInt finalAttack = Mathf.RoundToInt((isLastDamage ? _attackPointFinal_QuickSkill : _attackPoint_QuickSkill) * 0.25f); //jks 평타 공격력은 4로 나누어 사용.


			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
			{
				Log.print_always("   --------------------------------- 평타 발사체 타격 예정 ---------------------------------");
				Log.print_always("   공격자 : " + gameObject.name + "  --> 예정 피해자: " + _projectileOriginalTarget.name);
				
				if (isLastDamage) Log.print_always("  막타 공격력: " + _attackPointFinal_QuickSkill); else Log.print("  기본 공격력: " + _attackPoint_QuickSkill);
//				Log.print("  연속타 가산점: " + getAttackPowerBoost_ByContinuousHit(finalAttack));
			}
			#endif

//			finalAttack += getAttackPowerBoost_ByContinuousHit(finalAttack);
			updateQuickComboUI_ByContinuousHit();

			Knowledge_Mortal_Fighter opponentKnowledge = _projectileOriginalTarget.GetComponent<Knowledge_Mortal_Fighter>();

			int classRelation = BattleBase.Instance.compareClassRelation(Class, opponentKnowledge.Class);

			_projectile_Damage_QuickSkill = finalAttack;

			if (classRelation == 1)
			{
				_projectile_Damage_QuickSkill = finalAttack * 2;
			}

			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
			{
				if (classRelation == 1)
					Log.print_always("   공격자 클래스 : " + Class + "  -->  피해자 클래스: " + opponentKnowledge.Class + " =>    클래스 우위 !!  피격 X2   ==>  예정 최종 공격력: " + _projectile_Damage_QuickSkill);
				else
					Log.print_always("   공격자 클래스 : " + Class + "  -->  피해자 클래스: " + opponentKnowledge.Class + " =>    클래스 우위 아님  ==>  예정 최종 공격력: " + _projectile_Damage_QuickSkill);
			}
			#endif


			_skillType = eSkillType.ST_Quick;
			StartCoroutine(launcher.spawn_QuickSkill(_projectileOriginalTarget, AttackInfo_QuickSkill, AttackType, _weaponType_ForAnimation, reactionDistanceOverride));
		}
		else
		{
			_skillType = eSkillType.ST_Quick;
			base.giveDamage_QuickSkill(reactionDistanceOverride); // go melee attack
		}


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

//			if (BattleTuning.Instance._teamMemberStaging 
//				&& BattleBase.Instance.IsTeamRush 
//				&& !BattleBase.Instance.IsPlay_AutoTeam)
//				return;
		}


		//		if (IsLeader)
		//			Log.jprint(gameObject.name + "   doNextAction  c1: " + Action_Combo1 + " c2: " + Action_Combo2 + " c3: " + Action_Combo3 + " c4: " + Action_Combo4 + " c5: " + Action_Combo5 + " c6: " + Action_Combo6);
		
		if (NoNextComboAction) //jks if no next action,
		{	
			
			processOnUseCardEnd();

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



//			if (ComboRecent != 0)  //jks if skill ever initiated? , then start skill cooltime
//				//if (ComboRecent != 0 && !IsLeader)  //jks if skill ever initiated? and i m not the leader, then start skill cooltime
//			{
//				//Log.jprint(gameObject.name + " C O O L T I M E  S T A R T ........... _skillCompleted: " + _skillCompleted);				
//				startCoolTimeAndResetComboFlag();
//				return;
//			}
//			
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

		
	public override void processOnUseCardEnd()
	{
		if (IsLeader)
		{
			leaderMove();

			checkEnemy();

			Action_WalkBackTurn = false;
		}
		else
		{
			//if (!ImmediateAttack)
//			forceResetFlags();
//			Action_WalkBackTurn = true;
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



			//Log.jprint(gameObject.name +"111111  WalkBack !!!");

			//Log.jprint("processAfterResetActionInfo() : ComboCurrent: " + ComboCurrent + "   TotalCombo: " + TotalCombo);
			if(ComboCurrent == TotalCombo) //jks if projectile fire action
			{
				//Log.jprint("processAfterResetActionInfo() : activateSkillFinishedEvent()");
				endAim();
				activateSkillFinishedEvent();
			}
//			else
//			{
//				forceResetFlags();
//				Action_Walk = true;
//				//Action_Idle = false;
//			}
		}

		if (BattleUI.Instance().ClassMatchOwner == CardDeckIndex)
		{
			BattleUI.Instance().ClassMatchOwner = -1;
			BattleManager.Instance.attachCameraToLeader();
		}
//		Debug.Log("========= processOnUseCardEnd : " + gameObject.name + " / owner : " + BattleUI.Instance().ClassMatchOwner + " / index : " + CardDeckIndex);
	}




//	/// <summary>
//	/// jks -  in case character collide monster do not push forward.  give more distance for shooter
//	/// </summary>
//	protected override void doNotLetPassThrough()
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
//			float dist = Mathf.Abs(transform.position.x - go.transform.position.x);
//			float distShell = dist - this.Radius - goKnowledge.Radius;
//			
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
//
//
//		}
//	}


	//jks 2015.12.22 : 아군은 크리티컬일 때만 hit 리액션 - 
//	protected override bool checkReaction(eHitType hitType)
//	{
//		//평타 경우
//		if (Progress_Action_Quick) 
//		{
//			if (hitType == eHitType.HT_MISS || hitType == eHitType.HT_BAD) // 원거리 경우에는  공격 miss 일 경우에만 리액션 안함.
//				return false;
//			else
//				return true;
//		}
//
//
//		//jks 스킬 경우
//
//		if (IsLeader)
//		{
//			if (IsLastDamageInSkill || !Progress_SkillAction )
//				return true;
//			else
//				return false;
//		}
//		else
//		{
//			if (IsLastDamageInSkill || !Progress_SkillAction )
//			{
//				if (hitType == eHitType.HT_CRITICAL) //jks 막타라도 팀원이 나오다 맞으면 크리티컬만 아니면 리액션 하지 않게
//					return true;
//				else
//					return false;
//			}
//			else
//				return false;
//		}
//	}




//	public override bool shouldIJumpBackToKeepDistance()
//	{
//		Log.jprint(gameObject.name + "    !Progress_Skill: "+ !Progress_Skill+ "    isTooCloseToAttack: "+ isTooCloseToAttack() + "    CoolingJumpDone(): "+ CoolingJumpDone + "    JumpBackCount :"+ JumpBackCount);
//		return
//			(
//				IsLeader 
//				&& !Progress_Skill
//				//jks - do if too close to launch projectile
//				&& isTooCloseToAttack()
//				&& CoolingJumpDone
//				&& JumpBackCount < Random.Range(3,7)  //jks don't do it forever
//				&& !BattleBase.Instance.IsStagingMode //jks if boss talk staging
//			);
//	}


//	protected override bool isTooCloseToAttack()
//	{
//		GameObject target = getCurrentTarget();
//		
//		if (target == null) return false;
//		
//		float dist = Mathf.Abs(transform.position.x - target.transform.position.x);
//		float distShell = dist - Radius - target.GetComponent<Knowledge_Mortal_Fighter>().Radius;
//		
//		if (distShell < _attackDistanceMax * 1.5f)
//			return true;
//		
//		return false;
//	}




//	public override bool shouldIJumpBack()
//	{
////		bool shouldJump = base.shouldIJumpBack();
//		bool shouldKeepDistance = shouldIJumpBackToKeepDistance();
//
////		if (shouldKeepDistance)
////		{
////			Log.jprint(gameObject.name + "OOOOOOOOOOOOOOOOO    shouldJump: " + shouldJump + "      shouldKeepDistance: "+ shouldKeepDistance);
////		}
////		else
////		{
////			Log.jprint(gameObject.name + "XXXXXXXXXXXXXXXXX    shouldJump: " + shouldJump + "      shouldKeepDistance: "+ shouldKeepDistance);
////		}
//
////		return shouldJump || shouldKeepDistance;
//		return shouldKeepDistance;
//	}





}
