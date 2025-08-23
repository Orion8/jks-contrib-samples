using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;


public class Knowledge_Mortal_Fighter_Main_Meteor_RankingTower : Knowledge_Mortal_Fighter_Main_Meteor
{
	public override SupportSkillKnowledge_Leader KnowledgeSSkill { get { return null; } }

	public override void setLeader(bool value) { _leader_P2 = value; } 

	public override Knowledge_Mortal_Fighter_Main getLeader() 
	{
		return BattleBase.Instance.LeaderKnowledge_P2;
	}


	protected override List<FighterActor> Opponents { get { return BattleBase.Instance.List_Ally; } }


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
			//Log.jprint(gameObject + "   0 0 0    Action_WalkBackTurn : "+Action_WalkBackTurn+"       Action_WalkBack: "+Action_WalkBack);
			if (Action_WalkBackTurn)
			{
				forceResetFlags();
				Action_WalkBack = true;
				Action_WalkBackTurn = false;
				//Log.jprint(gameObject + "   1 1 1    Action_WalkBackTurn : "+Action_WalkBackTurn+"       Action_WalkBack: "+Action_WalkBack);
			}
			else
			{
				forceResetFlags();
				Action_WalkBackTurn = true;
				//Log.jprint(gameObject + "   2 2 2    Action_WalkBackTurn : "+Action_WalkBackTurn+"       Action_WalkBack: "+Action_WalkBack);
			}

			//jks 2016.4.22 랭킹전 상대 팀원 스킬 후 뒤로 들어가게 수정.			Action_Run = true;

			activateSkillFinishedEvent();
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
//		if (Action_Hit) yield break;//jks if (_hitReactionInProgress) return;
		if (BattleBase.Instance.IsSkillStagingInProgress) yield break;
//		if (Progress_Action_Quick && ContinuousComboCount_QuickSkill < _ai_quick_skill_max_hit) yield break;
		
//		if (name.Contains("M2"))
//			Log.jprint  (gameObject.name + "  Start skill xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
		
		Action_Combo1 = true;

		if (IsLeader)
		{
			GameObject leader_target = getClosestEnemy();
			if (leader_target)
				BattleBase.Instance.LeaderKnowledgeTarget_P2 = leader_target.GetComponent<Knowledge_Mortal_Fighter_Main>();
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
	
	//jks for scan
	public override bool getOpponentsInScanDistance(bool bQuickSkill)
	{
		
		if (! IsBattleInProgress)	//		if (BattleBase.Instance.EnemyToKill == 0) 
		{
			return false;
		}
		
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
		
		//if (_attackDistanceMax + Random.Range(-0.5f, 0) < distShell_AttackerAndClosestOpponent) return false;
		if (_attackDistanceMax < distShell_AttackerAndClosestOpponent) return false;

		return true;
	}
	
	
	
	
	
	// for give damage
	public override GameObject getOpponentsInScanDistance_WeaponPositionBased()
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
		
		if (!(IsLivingWeapon && ComboCurrent >= 1)) //jks - if summoned character,  do not check distance if combo started to finish attack.
			if (_attackDistanceMax + _weaponLength < finalDistToCheck) return null;
		
		return closestOpponent;
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

		if (NoNextComboAction) //jks if no next action,
		{			
			processOnUseCardEnd();				 
			
			if (_everGaveDamageForTheAttack)
			{
				startCoolTimeAndResetComboFlag();
				_everGaveDamageForTheAttack = false;
			}
			else
			{
				checkForSkillStart();
			}
		}
	}


	public override void giveDamage(float reactionDistanceOverride)
	{		
		//jks 2016.3.14 skill buff 기능 추가.
		addSkillBuff();

		_everGaveDamageForTheAttack = true;
		
		_skillCompleted = false; //jks 2016.5.23 meteor 경우 항상 크리티컬 발생하는 이슈 방지.:   isLastDamageInSkill(reactionDistanceOverride);

		StartCoroutine(giveDamageDelayed(reactionDistanceOverride));
		
	}



	public override eHitType getFinalHitType(Knowledge_Mortal_Fighter opponent)
	{
		float finalHitRate;
		
		finalHitRate = calcHitRate(opponent);
		
		if (finalHitRate < 0) finalHitRate = 0;
		
		eHitType hitType = judgeHitState(finalHitRate);
		
		//jks 2014.9.27  크리티컬은 스킬의 막지막 타에만 가능 하도록 수정. 
		if (hitType == eHitType.HT_CRITICAL)
		{
			//2016.5.25	제거
//			if (! _skillCompleted)
//			{
//				hitType = eHitType.HT_GOOD;
//			}

			if ((IsLeader) // 랭킹탑 적 리더 일 경우,
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
		
		return hitType;
	}
	
	
	
	public override float calcHitRate(Knowledge_Mortal_Fighter opponent)
	{
		float hitRate;
		//jks 2015.8.27 		float buffHitRate = calculate_LeaderBuff_HitRate_Opponent();
		//jks 2015.5.8 remove leader strategy-	float leaderStrategy = calculate_LeaderStrategy_HitRate();
		
		hitRate = base.calcHitRate(opponent);//jks 2015.8.27  + buffHitRate; //jks 2015.5.8 remove leader strategy-	 + leaderStrategy;
		//Log.jprint("********* calcHitRate()    buff: "+ buffHitRate+"    leader strategy: "+leaderStrategy);

		float hitRateDebuff = getLeaderBuffCriticalDown (hitRate);

		hitRate -= hitRateDebuff;

		#if UNITY_EDITOR
		if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
		{
			Log.print_always(" P2 : " + gameObject.name +  "     리더버프: -"+hitRateDebuff  + " = " + "  최종 적중률: " + hitRate);
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
