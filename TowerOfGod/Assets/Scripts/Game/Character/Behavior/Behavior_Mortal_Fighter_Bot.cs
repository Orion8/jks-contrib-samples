using UnityEngine;
using System.Collections;
using React;

//This is a little shortcut which creates an alias for a type, and makes out Action methods look much nicer.
using  Action = System.Collections.Generic.IEnumerator<React.NodeResult>;


public class Behavior_Mortal_Fighter_Bot : Behavior_Mortal_Fighter
{

	new public Knowledge_Mortal_Fighter_Bot Knowledge
	{
		get
		{
			if (_knowledge == null)
			{
				_knowledge = GetComponent<Knowledge>();
			}
			return (Knowledge_Mortal_Fighter_Bot) _knowledge;
		}
	}


	protected override void animate_Victory()
	{
		#if DEBUG_LOG		
		Log.jprint(gameObject.name +"animate_Victory");
		#endif

		Time.timeScale = 1;

		AnimCon.applyRootMotion(true);
		AnimCon.startAnimation(Knowledge.Anim_Victory, 0.1f, 1, 0);
		resetComboCurrent();
		
		Knowledge.endAim();
		
		//Knowledge.showWeapon(false);
		Loco.setCurrentSpeedToZero();
		//Log.jprint(gameObject + " Loco.setCurrentSpeedToWalk()");
	}




	protected virtual void animate_AccumulatedHitReaction()
	{		
		#if DEBUG_LOG_ANIMATION
		//Log.jprint(Time.time + "  " + gameObject.name +"  *  animate_AccumulatedHitReaction    "  + AnimationBlendingInfo.Instance._blend_hit);
		#endif

		AnimCon.applyRootMotion(true);

		AnimCon.startAnimation(Knowledge.Anim_AccumulatedHitReaction, AnimationBlendingInfo.Instance._blend_hit, 1, 0);  // crossfade()
		resetComboCurrent();
		Knowledge.Progress_AccumulatedHitReaction = true;
		
		Knowledge._isResetActionInfoDone = false;
		
		Knowledge.endAim();
		
		//jks do not hide weapon if hit -- Knowledge.showWeapon(false);
		Loco.setCurrentSpeedToZero();
	}
	
//jks 2015.11.4 보스액션 제거.	---------------------->
//	protected virtual void animate_ActionBoss()
//	{		
//
//		AnimCon.applyRootMotion(true);
//		
//		AnimCon.startAnimation(Knowledge.animActionBoss(), AnimationBlendingInfo.Instance._blend_hit, 1, 0);  // crossfade()
//		Knowledge.startAnimationEventDoubleCheck_BossAction();
//
//		resetComboCurrent();
//		Knowledge.Progress_ActionBoss = true;
//		
//		Knowledge._isResetActionInfoDone = false;
//		
//		Knowledge.endAim();
//		
//		//jks do not hide weapon if hit -- Knowledge.showWeapon(false);
//		Loco.setCurrentSpeedToZero();
//	}
//jks <----------------------------------------------



	//jks behavior tree action
	#region BehaviorTree  


	public override Action battleVictory()
	{
		#if DEBUG_LOG		
		if (gameObject.name.Contains("Dummy") == false)
			Log.jprint(gameObject + "  V I S I T : Action battleVictory()");
		#endif
		if (Knowledge.IsBattleVictorious )  //유저 관점에서의 승리.
		{
			if (Knowledge.AllyID == eAllyID.Ally_Human_Me) 
			{
				if (!AnimCon.isAnimPlaying(Knowledge.Anim_Victory))
				{
					animate_Victory();
				}
			} 
			else
			{
				if (Knowledge.IsBoss && 
					BattleBase.Instance.isThisVictoryCondition(eBattleVictoryConditionIndex.BVC_BossHPReduction))
				{
					Knowledge.Action_DeathB = true;  //jks 보스가 죽지않아도 이길 수 있는 mission 일 경우 유저 승리 시 보스의 액션은 Action_DeathB.
					Knowledge.startSlowMotion(BattleTuning.Instance._slowMotionBossNPercentDamageTimeScale,
											  BattleTuning.Instance._slowMotionBossNPercentDamageDuration);
				}
				else
				{
					//jks for alive minions
					//forceIdle();
					Knowledge.Current_HP = 0; //jks boss die, so you do.
					Knowledge.forceResetFlags();
					Knowledge.Action_Death = true;
				}
			}

			yield return React.NodeResult.Success;
			
		}
		yield return React.NodeResult.Failure;
	}


	public override Action death()
	{
		if (Knowledge.Action_Death) 
		{		
			//jks > > > > > > > > > >for story staging only  - end game but no death animation
			if (Knowledge.Max_HP == 0) 
			{
				if (!Knowledge.Progress_Death)
				{
					Knowledge.Progress_Death = true;

					animate_Idle();
					BattleBase.Instance.addDeathCount_Enemy_For_StoryStaging();
				}
				yield return React.NodeResult.Success;
			}
			//jks < < < < < < < < < < for story staging only
			else
			{
				if (Knowledge.Progress_Death)
				{
					if (!AnimCon.isAnimPlaying(Knowledge.Anim_Death))
					{
						animate_Death();
					}
				}
				else
				{
					if (!AnimCon.isAnimPlaying(Knowledge.Anim_Death))
					{
						Knowledge.Progress_Death = true;
						
						processDeath();
						animate_Death();
					}
				}
				
				yield return React.NodeResult.Success;
			}
		}
		yield return React.NodeResult.Failure;
	}

	/// <summary>
	/// DeathB : 보스가 죽지 않아도 승리할 수 있는 mission의 경우 유저 승리 시 보스액션. 
	/// </summary>
	public override Action deathB()
	{
		if (Knowledge.Action_DeathB) 
		{		
			if (!Knowledge.Progress_DeathB)
			{
				if (!AnimCon.isAnimPlaying(Knowledge.Anim_DeathB))
				{
					animate_DeathB();
				}
			}

			yield return React.NodeResult.Success;
		}

		yield return React.NodeResult.Failure;
	}


	public virtual bool accumulatedHitReaction()
	{
		#if DEBUG_LOG		
		Log.jprint(gameObject + "  V I S I T :  accumulatedHitReaction()");
		#endif
		if (Knowledge.Action_AccumulatedHitReaction)
		{
			if (!Knowledge.Progress_AccumulatedHitReaction)
			{			
				animate_AccumulatedHitReaction();
			}

			return true;
		}
		return false;
	}

//jks 2015.11.4 보스액션 제거.------------------------>
//	public virtual bool actionBoss()
//	{
//		#if DEBUG_LOG		
//		Log.jprint(gameObject + "  V I S I T :  actionBoss()");
//		#endif
//		if (Knowledge.Action_Boss)
//		{
//			if (!Knowledge.Progress_ActionBoss)
//			{			
//				animate_ActionBoss();
//			}
//			
//			return true;
//		}
//		return false;
//	}
//jks <--------------------------------------------	
	


	public override Action idle()
	{
		#if DEBUG_LOG		
		if (gameObject.name.Contains("Dummy") == false)
			Log.jprint(gameObject + "  V I S I T : Action Idle()");
		#endif
		if (Knowledge.Action_Idle)
		{			
			#if DEBUG_LOG		
			Log.jprint(gameObject + "   Idle");
			#endif
			
			if (!AnimCon.isAnimPlaying(Knowledge.Anim_Idle))
			{
				animate_Idle();
			}
			
			yield return React.NodeResult.Success;
		}
		yield return React.NodeResult.Failure;
	}	

	//default
	public override Action walkFast()
	{
		#if DEBUG_LOG		
		if (gameObject.name.Contains("Dummy") == false)
			Log.jprint(gameObject + "  V I S I T : Action walkFast()");
		#endif
		//if (Knowledge.Action_WalkFast && !Knowledge.Progress_Skill) 
		{			
			#if DEBUG_LOG		
			Log.jprint (gameObject + "   walkFast");
			#endif
			
			if (!AnimCon.isAnimPlaying(Knowledge.Anim_WalkFast))
			{
				animate_WalkFast();
				//150803_jsm - 카메라 중심점 타겟팅 방식 자동으로 변경
//				setCameraTarget(CameraManager.TargetState.Walk);
			}
			
			//scanEnemy();
			
			yield return React.NodeResult.Success;
		}
		
		yield return React.NodeResult.Failure;
	}	




	//jks 리더일 경우 cooling 들어 가기전에 뒤로 점프하는 동작 . Bot 일 경우 동시에 공격 중인 적이 이 수 이상일 때만 허용.
	//bool _coolingJumpDone = false;
	public override Action coolingJump()
	{
		if (Knowledge.shouldIJumpBack())
		{
			if (Knowledge.IsLivingWeapon) // summoned character type weapon, won't cool down, will be destroyed instead after attack.
			{
				Knowledge.Action_Paused = true; // do nothing
				Destroy(gameObject, 0.8f);
				yield return React.NodeResult.Success;
			}

			if (!AnimCon.isAnimPlaying(Knowledge.Anim_CoolingJump))
			{
				Knowledge.forceResetFlags();
				animate_CoolingJump();
			}
			
			if (!Knowledge.Progress_CoolingJump) //jks if jump back action is finished
			{
				Knowledge.CoolingJumpDone = true;
			}
			
			#if DEBUG_LOG		
			Log.jprint (gameObject + "   cooling jump");
			#endif
			yield return React.NodeResult.Success;
		}
		
		Knowledge.CoolingJumpDone = false; //jks reset
		yield return React.NodeResult.Failure;
	}

//	//jks 리더일 경우 cooling 들어 가기전에 뒤로 점프하는 동작 . Bot 일 경우 동시에 공격 중인 적이 이 수 이상일 때만 허용.
//	//bool _coolingJumpDone = false;
//	public override Action coolingJump()
//	{
//		Knowledge.shouldIJumpBack();
//
//		if (Knowledge.Action_CoolingJump)
//		{
//			if (Knowledge.IsLivingWeapon) // summoned character type weapon, won't cool down, will be destroyed instead after attack.
//			{
//				Knowledge.Action_Paused = true; // do nothing
//				Destroy(gameObject, 0.8f);
//				yield return React.NodeResult.Success;
//			}
//
//			if (!Knowledge.Progress_CoolingJump)
//			{
//				Knowledge.forceResetFlags();
//				animate_CoolingJump();
//				Knowledge.CoolingJumpDone = true;
//			}
//				
//			yield return React.NodeResult.Success;
//		}
//
//		Knowledge.CoolingJumpDone = false; //jks reset
//		yield return React.NodeResult.Failure;
//	}

	

	#endregion


	float getWaveSignalPosition()
	{
		if (transform.forward.x > 0)
		{
			return transform.position.x - 2;
		}
		else
		{
			return transform.position.x + 2;
		}
	}


	//jks - enemy death
	protected override void processDeath()
	{
		if (Knowledge.AllyID != eAllyID.Ally_Human_Me)
		{
//jks- 2015.7.17  remove - no need anymore
//			if (BattleBase.Instance.Strategy == eBattleStrategy.BS_Default)
//				CameraManager.Instance.setTarget(transform);

			//150720_jsm - 카메라 락온 상태에서 디졌을 경우 카메라 타겟을 리더로 변경.
			if (CameraManager.Instance.doIHaveCameraAttached ( transform ))
				BattleBase.Instance.attachCameraToTheForemostFront( BattleBase.Instance.LeaderTransform.gameObject );


			if (Knowledge.IsMinion == false) //jks - if minion, ignore death count since minion is not included in total bot count.
				BattleBase.Instance.addDeathCount_Enemy();
			if (Knowledge.WaveSignalOn)
				BattleBase.Instance.waveSignalReady(getWaveSignalPosition());
			Inventory.Instance().checkMonQuest(Knowledge.BotID);

			if (Knowledge.IsBoss)
			{
				BattleBase.Instance.postProcessBossDeath();
				BattleBase.Instance.updateAchievedVictoryCondition(eBattleVictoryConditionIndex.BVC_KillBoss);
			}

			if (Knowledge.IsGoalTarget)
				BattleBase.Instance.updateAchievedVictoryCondition(eBattleVictoryConditionIndex.BVC_KillTargetObject);

			BattleBase.Instance.updateKillCount();

			if (Knowledge.BotType == eBotType.BT_Blockage)
				BattleBase.Instance.updateKillBlockageCount();

			if (Knowledge != null && Knowledge.Attacker != null)
			{
				Knowledge_Mortal_Fighter_Main knowAttacker = Knowledge.Attacker.GetComponent<Knowledge_Mortal_Fighter_Main>();
				if (knowAttacker != null)
				{
					eSkillType skillType = knowAttacker.SkillType; //공격받은 스킬 체크

					//jsm - 리더에게 죽었고 리더가 상성 우위이고 일반 스킬에 죽었다면 이펙트 표시.
					if (Knowledge.Attacker == BattleBase.Instance.LeaderGameObject && skillType == eSkillType.ST_Card)
					{
						int type = BattleManager.Instance.isLeaderSuperiorClass(Knowledge.Class);
						if (type == 1)
							BattleUI.Instance ().leaderRelationEffectOn ();
					}
				}
			}
		}

		if (Knowledge.IsProtectee)
		{
			BattleBase.Instance.updateCurrentCondition_Fail(eBattleFailConditionIndex.BFC_Protection);
		}

		hideHealthBar();
		destroyMe();
	}


	protected override void destroyMe()
	{
//jks- 2015.7.17  remove - no need anymore
//		if (BattleBase.Instance.Strategy == eBattleStrategy.BS_Default)
//			Invoke("releaseCameraFocus", 2.0f);
		Destroy(gameObject, 2.8f);
	}


//jks- 2015.7.17  remove - no need anymore
//	void releaseCameraFocus()
//	{
//		BattleBase.Instance.attachCameraToLeader();
//	}

	protected override void Update()
	{
		//		if (gameObject.name.Contains("Dummy") == false)
		//			Log.jprint(gameObject.name +" - = - = - Update() - = - = -" );
		
		if (Knowledge == null) return;

		if (Knowledge.IsBoss)
		{
			if (Knowledge.KnowledgePSkill != null)
			{
				if (Knowledge.HavePatternSkill)
				{
					if (!Knowledge.KnowledgePSkill.IsBossPatternSkillReady)
						return;
				}
			}
		}

		if (Knowledge.HoldUpdate) return; //jks  to hold behavior update() summoned bot , between addcomponents and knowledge initial value setup.

		if (! BattleBase.Instance.isSkillPreviewMode())
		{
			if (!BattleBase.Instance.IsBattleInProgress) return;
			if (!Knowledge.IsBattleTimeStarted) return;
		}

		scan();
		
	}

}
