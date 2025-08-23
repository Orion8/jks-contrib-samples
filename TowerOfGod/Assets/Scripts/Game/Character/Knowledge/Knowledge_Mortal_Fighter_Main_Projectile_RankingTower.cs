using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

public class Knowledge_Mortal_Fighter_Main_Projectile_RankingTower : Knowledge_Mortal_Fighter_Main_Projectile
{
	protected override List<FighterActor> Opponents { get { return BattleBase.Instance.List_Ally; } }

	public override SupportSkillKnowledge_Leader KnowledgeSSkill { get { return null; } }
	
	public override void setLeader(bool value) { _leader_P2 = value; } 


	public override Knowledge_Mortal_Fighter_Main getLeader() 
	{
		return BattleBase.Instance.LeaderKnowledge_P2;
	}


	public override bool manualJumpBack()
	{
		return false;
	}

	public override void updateHP(ObscuredInt delta, eHitType hitType)
	{
		base.updateHP(delta, hitType);


		if (BattleUI_RankingTower.Instance != null)
		{
			if (AllyID != eAllyID.Ally_Human_Me)
			{
				BattleUI_RankingTower.Instance.TargetUser.CSlot[_cardDeckIndex].Now_HP = Current_HP;
			}
		}


		if (_cur_hp == 0)
		{
			//Log.jprint(gameObject.name + " is dead ! ! ! ! ! !.");

//			//jks slow motion effect: if last opponent is dead.
//			if (BattleBase.Instance.EnemyToKill == 1)
//			{
//				startSlowMotion(BattleTuning.Instance._slowMotionBossDeathTimeScale,
//					BattleTuning.Instance._slowMotionBossDeathDuration);
//				
//				if (BattleUI.Instance() != null)
//					BattleUI.Instance().gob_leaderHud.SetActive(false);
//			}

			Action_Death = true;
		}
		
//		shaderEffect_Hit_On();
		
//		BattleBase.Instance.updateTotalHPForDisplay();
	}






	public override void processOnUseCardEnd()
	{

		if (IsLeader)
		{
			leaderMove();
			
			checkEnemy();
			
		}
		else
		{
			//jks 2016.4.22 랭킹전 상대 팀원 스킬 후 뒤로 들어가게 수정.
			if (Action_WalkBackTurn)
			{
				forceResetFlags();
				Action_WalkBack = true;
				Action_WalkBackTurn = false;
			}
			else
			{
				forceResetFlags();
				Action_WalkBackTurn = true;
			}

			//jks 2016.4.22 랭킹전 상대 팀원 스킬 후 뒤로 들어가게 수정.       Action_Run = true;

			if(ComboCurrent == TotalCombo) //jks if projectile fire action
			{
				endAim();
				activateSkillFinishedEvent();
			}
		}
		
	}



	public override void incrementContinuousHitCount() 
	{ 
		if (!IsLeader) return;

		_continuousHitCount_QuickSkill++; 

		BattleBase.Instance.setLeaderBuffInvincibleMode_P2(_continuousHitCount_QuickSkill);
	}



	public override IEnumerator startSkill()
	{
		if (canUseSkill() == false)
		{
			yield break;
		}


//		if (Progress_SkillAnimation) yield break;
//		if (Action_Hit) yield break; //jks if (_hitReactionInProgress) return;
		if (BattleBase.Instance.IsSkillStagingInProgress) yield break;
		
//		if (name.Contains("M2"))
//			Log.jprint  (gameObject.name + "  Start skill xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");

		Action_Combo1 = true;

		if (IsLeader)
		{
			GameObject leader_target = getClosestEnemy();
			if (leader_target)
				BattleBase.Instance.LeaderKnowledgeTarget_P2 = leader_target.GetComponent<Knowledge_Mortal_Fighter_Main>();
			
			//				BattleBase.Instance.showUICoolTime(CardDeckIndex);
		}
	}
	


	bool isInvalidToPush(GameObject go)
	{
		if (go == null) return true;
		if (!go.activeSelf) return true;
		if (go == _captured_target) return true; // do not push whatever i hold.
		
		Knowledge_Mortal_Fighter goKnowledge = go.GetComponent<Knowledge_Mortal_Fighter>();
		if (goKnowledge == null) return true;
		if (goKnowledge.AllyID == AllyID) return true;
		if (goKnowledge.IsDead) return true;
		if (goKnowledge.AmIWall) return true;
		if (goKnowledge.AmICaptured) return true;
		
		return false;
	}
	
	protected override void pushPassedOpponents()
	{
		if (IsDead) return;
		if (AmICaptured) return;
		
		foreach(FighterActor ca in Opponents) 
		{
			GameObject go = ca._go;
			
			if (isInvalidToPush(go)) continue;
			
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
		
		foreach(FighterActor ca in Opponents)
		{
			GameObject go = ca._go;
			
			if (isInvalidToPush(go)) continue;
			
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



	public override bool getOpponentsInScanDistance(bool bQuickSkill)
	{
		if (! IsBattleInProgress)	//		if (BattleBase.Instance.EnemyToKill == 0) 
		{
			return false;
		}
		
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

		if (scanDistance < distShell_AttackerAndClosestOpponent) return false;
		

		return true;
	}




	protected override void LateUpdate()
	{
		if (Progress_SkillAnimation
			|| (KnowledgeSSkill && KnowledgeSSkill.Progress_AnyAction)
			|| Progress_Action_Quick)
		{
			pushOpponentWhenIAttack();
		}
		else if (Action_Run || Action_Walk || Action_WalkFast || BattleBase.Instance.ForceMoveLeaderRun_P2)
		{
			pushPassedOpponentsWhenIMove();
		}

		pauseProcess();
	}



	protected override void doNextAction()
	{

		setNextComboActionFlag();

		//		if (IsLeader_P2 && Action_InstallWeapon_Pre)
		//		{
		//			Action_InstallWeapon = true;
		//			Action_InstallWeapon_Pre = false;
		//			return;
		//		}

		if (NoNextComboAction) //jks if no next action,
		{

			processOnUseCardEnd();

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

		}
	}





	/// <summary>
	/// jks -  in case character collide monster do not push forward.  give more distance for shooter
	/// </summary>
	protected override void doNotLetPassThrough()
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
			
			//Log.jprint( "  dist:" + dist + "  _radius: " + _radius + "  go._radius: " + go.GetComponent<Knowledge_Mortal>()._radius);
			//Log.jprint("_attackDistanceMin " + _attackDistanceMin + " < distShell: " + distShell + " < _attackDistanceMax: " + _attackDistanceMax);
			//float checkDist = Radius + Radius + 0.05f;
			if (distShell < 0) 
			{			
				Vector3 newPos = go.transform.position;
				//newPos.x = transform.position.x + (this.Radius + goKnowledge.Radius + checkDist) * transform.forward.x;
				newPos.x = transform.position.x + (this.Radius + goKnowledge.Radius) * transform.forward.x;
				go.transform.position = newPos;
				
				MoveSpeedChangeRate = 0.05f;
				
				//				if (go.name.Contains("C"))
				//					Log.jprint(go + "  ~ ~ ~ ~ ~   doNotLetPassThrough()     position: "+ go.transform.position);
			}
		}
	}







	public override void giveDamage(float reactionDistanceOverride)
	{		
		//jks 2016.3.14 skill buff 기능 추가.
		addSkillBuff();

		_everGaveDamageForTheAttack = true;


		_skillCompleted = isLastDamageInSkill(reactionDistanceOverride);

		//Log.jprint(". . . . . giveDamage(): " + _attackType);
		Launcher_Projectile launcher;

		if (AttackType == eAttackType.AT_Stun)
			launcher = GetComponent<Launcher_Projectile_Stun>();
		else if (AttackType == eAttackType.AT_Homing)
			launcher = GetComponent<Launcher_Projectile_Homing>();
		else
			launcher = GetComponent<Launcher_Projectile_Basic>();


		if (launcher == null) 
		{
			Log.Warning("Can not find Launcher_Projectile component. ");
			return;
		}


		updateProjectileTarget();
		if (_projectileOriginalTarget == null)
		{
			_projectileOriginalTarget = BattleBase.Instance.findClosestOpponent(AllyID, transform.position.x);
			setTarget(_projectileOriginalTarget);
			//updateProjectileTarget();
		}

		if (_projectileOriginalTarget == null) return;


		startAim();

//		if (BattleBase.Instance.EnemyToKill != 0) 
		{
			SkillType = eSkillType.ST_Card;
			StartCoroutine( launcher.spawn(_projectileOriginalTarget, AttackInfo, AttackType, _weaponType_ForAnimation, reactionDistanceOverride));
		}

	}





	public override float calcHitRate(Knowledge_Mortal_Fighter opponent)
	{
		float hitRate;

		hitRate = base.calcHitRate(opponent);

		float hitRateDebuff = getLeaderBuffCriticalDown (hitRate);

		hitRate -= hitRateDebuff;

		#if UNITY_EDITOR
		if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
		{
			Log.print_always(" P2 : " + gameObject.name +  " 리더버프: -"+hitRateDebuff  + " = " + "  최종 적중률: " + hitRate);
		}
		#endif

		return hitRate;
	}



	public override bool shouldIRun()
	{
		return
			!Progress_SkillAnimation
			&&
			(
				Action_Run 
				|| (IsLeader && BattleBase.Instance.ForceMoveLeaderRun_P2)
				|| (IsLeader && !inRangeFromCam(2))
			);
	}


	public override void takeStun(int duration, int stunAnimNum)
	{
		Action_Stun = true;
		Anim_Stun = animStunReaction(stunAnimNum);
		CancelInvoke("stun_finishied"); //jks if previous stun is not finished, skip previous invoke and start new invoke to continue stun.
		Invoke("stun_finishied", duration);
		//Log.jprint(Time.time + "  :  " + gameObject.name + " + + + + + stun [duration]="+duration);
	}

	public override void takeStun(float duration, int stunAnimNum)
	{
		Action_Stun = true;
		Anim_Stun = animStunReaction(stunAnimNum);
		CancelInvoke("stun_finishied"); //jks if previous stun is not finished, skip previous invoke and start new invoke to continue stun.
		Invoke("stun_finishied", duration);
		//Log.jprint(Time.time + "  :  " + gameObject.name + " + + + + + stun [duration]="+duration);
	}


}
