using UnityEngine;
using System.Collections;
//This is a little shortcut which creates an alias for a type, and makes out Action methods look much nicer.
using  Action = System.Collections.Generic.IEnumerator<React.NodeResult>;

public class Behavior_Mortal_Fighter_Main_LivingWeaponUser_RankingTower : Behavior_Mortal_Fighter_Main_LivingWeaponUser 
{

	protected override void setForCamLockOn() { }

	protected bool _coolingMoveDone = false;

	//	protected bool _coolingMoveDone = false;
	
	#region Behavior Tree
	/*
	public override Action cooling()
	{
		//		if ( Knowledge.IsLeader)
		{
			if ( Knowledge.AttackCoolTimer.IsCoolingInProgress 
			    //			    && Knowledge.inRangeFromCam(2)
			    )
			{
				if (!AnimCon.isAnimPlaying(Knowledge.Anim_Exhausted))
				{
					Knowledge.forceResetFlags();
					animate_Exhausted();
				}
				
				yield return React.NodeResult.Success;
				
			}
		}
		//		else
		//		{
		//			if ( Knowledge.AttackCoolTimer != null && Knowledge.AttackCoolTimer.IsCoolingInProgress)
		//			{
		//				if (_coolingMoveDone)
		//				{
		//					if (!AnimCon.isAnimPlaying(Knowledge.Anim_Exhausted))
		//					{
		//						Knowledge.forceResetFlags();
		//						animate_Exhausted();
		//					}
		//					
		//					yield return React.NodeResult.Success;
		//				}
		//				else
		//				{
		//					Knowledge.Action_WalkBack = true;
		//				}
		//			
		//			}
		//		}
		
		Knowledge.CoolingJumpDone = false; //jks reset
		
		yield return React.NodeResult.Failure;
	}
	*/

	public override Action cooling()
	{
		if (Knowledge.IsLeader)
		{
			if ( Knowledge.AttackCoolTimer.IsCoolingInProgress )
			{
				if (!Knowledge.IsLeader || !BattleBase.Instance.ForceMoveLeaderRun_P2)
				{
					if (!AnimCon.isAnimPlaying(Knowledge.Anim_Exhausted))
					{
						Knowledge.forceResetFlags();
						animate_Exhausted();
					}
				}

				yield return React.NodeResult.Success;

			}
		}
		else  //jks 2016.4.22 랭킹전 상대 팀원 스킬 후 뒤로 들어가게 수정.
		{
			//jks this happens, do not know the reason yet.
			if ( Knowledge.AttackCoolTimer.IsCoolingInProgress && Knowledge.Action_Run)
			{
				Knowledge.forceResetFlags();
				Knowledge.Action_WalkBackTurn = true;

				yield return React.NodeResult.Failure;
			}
		}

		Knowledge.CoolingJumpDone = false; //jks reset

		yield return React.NodeResult.Failure;
	}

	
//	public override Action walkBackTurn()
//	{
//		if (Knowledge.Action_WalkBackTurn && !Knowledge.Progress_SkillAnimation)  //jks - let attack finish it's action before start move.
//		{			
//			if (!AnimCon.isAnimPlaying(Knowledge.Anim_WalkBackTurn))
//			{
//				animate_WalkBackTurn();
//			}
//			
//			yield return React.NodeResult.Success;
//		}
//		
//		yield return React.NodeResult.Failure;
//	}
//	
//	
//	public override Action walkBack()
//	{
//		//Log.jprint ("Knowledge.Action_Walk: "+ Knowledge.Action_Walk);
//		if (Knowledge.Action_WalkBack && !Knowledge.Progress_SkillAnimation)  //jks - let attack finish it's action before start move.
//		{			
//			#if DEBUG_LOG		
//			Log.jprint (gameObject + "   walk back");
//			#endif			
//			
//			if (!AnimCon.isAnimPlaying(Knowledge.Anim_WalkBack))
//			{
//				animate_WalkBack();
//			}
//			
//			//			if ( ! Knowledge.inRangeFromCam(2))
//			//			{
//			//				_coolingMoveDone = true;
//			//				Knowledge.Action_WalkBack = false;
//			//			}
//			
//			yield return React.NodeResult.Success;
//		}
//		
//		yield return React.NodeResult.Failure;
//	}
	
	#endregion





//	protected bool canUseQuickSkill()
//	{
//		if (! Knowledge.IsLeader) return false;
//		if (Knowledge.Progress_SkillAction || Knowledge.Progress_SkillAnimation) return false;
//		
//		if (BattleBase.Instance.IsBossDialogStagingMode) return false;
//		if (!BattleBase.Instance.IsBattleInProgress) return false; 
//		if (MadPauser.Instance.TimeState == eTimeState.TIME_PAUSE) return false;
//		if (BattleBase.Instance.IsIgnoreButtonTouch) return false;
//		
//		return true;
//	}
	
	
	protected void startLeaderQuickSkill_P2()  
	{ 
		if (BattleTuning.Instance._showQuickSkillLog)
			Log.print(Time.time + "     * * * * * * * * * * P2 평타  * * * * * * * * * * ");
		
		if (! Knowledge.canUseQuickSkill()) return;		
		
		//jks to avoid start skill while previous blending. (in case fast click/touch input)
		if (Knowledge.GetComponent<Animator>().IsInTransition(0)) return;
		
		Knowledge.startOrContinueQuickSkill();
	}
	
	
	protected override void startQuickSkill()
	{
		startLeaderQuickSkill_P2();
	}




	//jks 20164.21 Mantis1448 :
	protected override void decideQuickOrMainSkill()
	{
		if (! Knowledge.AttackCoolTimer.IsCoolingInProgress) //jks 쿨타임 중이 아니고,
		{
			if (! Knowledge.Progress_Action_Quick )
			{
				Knowledge.Action_Run = true;
				Knowledge.AI_AutoCombo_QuickSkill = false;
			}
		}
		else
		{
			GameObject closestOpponent = BattleBase.Instance.findClosestOpponent(Knowledge.AllyID, transform.position.x);
			if (closestOpponent != null)
			{
				Knowledge_Mortal_Fighter opponentKnowledge = closestOpponent.GetComponent<Knowledge_Mortal_Fighter>();

				float dist = Mathf.Abs(transform.position.x - closestOpponent.transform.position.x) - Knowledge.Radius - opponentKnowledge.Radius;

				if (dist > Knowledge._attackDistanceMax + 2.5f) //jks 거리 내에 적이 없으면 뛰기.
				{
					//Log.jprint(gameObject + " - - - 쿨타임 중이고, 거리 내에 적이 없어 뛰기. - - -   ");
					BattleBase.Instance.ForceMoveLeaderRun_P2 = true;
				}
				else if (!Knowledge.AI_AutoCombo_QuickSkill)
				{	//jks 20164.21 Mantis1448 : 메인스킬 쿨 중이면, 평타 시작.
					Knowledge.AI_AutoCombo_QuickSkill = true;
					Knowledge.Action_Run = true;
					Log.jprint(Time.time+ "   - - - Quick (평타) 공격 시작. - - -   " + gameObject);
				}
			}
		}
	}

	protected override void scan()
	{
		if (Knowledge == null || 
		    BattleBase.Instance.IsDialogShowOff || 
		    Knowledge.Action_InstallWeapon || 
		    Knowledge.Action_InstallWeapon_Pre)
		{
			return;
		}
		
		//jks 평타 리더 AI
		if (Knowledge.IsLeader) // leader ?
		{
			//jks 20164.21 Mantis1448 :
			decideQuickOrMainSkill();
		}
		else
		{
			Knowledge.AI_AutoCombo_QuickSkill = false;
		}
		
		
		
		if (Knowledge.Action_Run ||
		    Knowledge.Action_Walk ||
		    Knowledge.Action_WalkFast ||
		    Knowledge.Action_Idle ||
			(Knowledge.IsLeader && BattleBase.Instance.ForceMoveLeaderRun_P2) //jks 쿨타임 중 뛰고 있는지 ?
		    )
		{
			if ( Knowledge.AI_AutoCombo_QuickSkill) //jks 평타 결정이면,
			{
				if (Knowledge.ComboCurrent_QuickSkill == 0) 
				{
					scanEnemy_QuickSkill();
				}
			}
			else 
			{
				scanEnemy();
			}
		}
	}
	
	
	
	protected override void scanEnemy()
	{
		if (BattleBase.Instance.IsSkillStagingInProgress) return;

		bool found = Knowledge.checkEnemyOnPath(false);

		//jks 쿨타임 중이기 때문에 달리고 있는 리더 경우, 적을 만나면 정지하고 팀원을 출격시킴.
		if (found && Knowledge.IsLeader) // leader ?
		{
			if (BattleBase.Instance.ForceMoveLeaderRun_P2)
			{
				BattleBase.Instance.ForceMoveLeaderRun_P2 = false;  //jks 적을 만나면 정지.
				BattleBase.Instance.autoUseCardWithBiggestSinsu_P2();				
			}
		}

		if (!Knowledge.canUseSkill()) return;

		
		if (found)
		{
			StartCoroutine( Knowledge.startSkill() );	
			CancelInvoke("cancelAttackIfNoEnemy");
			_isDelayedInvoked_cancelAttackIfNoEnemy = false;
			
			//리더인경우 타겟팅된 대상의 transform을 셋팅 
			setForCamLockOn();
		}
		else 
		{
			if (!Knowledge.IsLeader)
			{
				//Log.jprint(gameObject + "Enemy not found  . . . . . . . . ");
				if (!_isDelayedInvoked_cancelAttackIfNoEnemy)
				{
					_isDelayedInvoked_cancelAttackIfNoEnemy = true;
					//Log.jprint(gameObject + ".. Invoke cancelAttackIfNoEnemy : " + BattleBase.Instance.GetEnemySearchingTime);
					Invoke("cancelAttackIfNoEnemy", BattleBase.Instance.GetEnemySearchingTime_P2);
				}
			}
			// if leader
			else if (!Knowledge.Progress_SkillAnimation)  
			{
				// if we have enemy ahead, run
				if (Knowledge.isEnemyAppeared())
				{
					leaderStrategyLocomotion_IfEnemyAppeared();
				}
			}
		}
		
		
	}
	
	

	
	//jks - enemy death
	protected override void processDeath()
	{
		if (Knowledge.AllyID != eAllyID.Ally_Human_Me)
		{
			if (BattleUI.Instance() != null)
			{
				//jks P2 UI :  
				BattleUI.Instance().setPlayerDead_P2(Knowledge.CardDeckIndex);			
			}

			//150720_jsm - 카메라 락온 상태에서 디졌을 경우 카메라 타겟을 리더로 변경.
			if (CameraManager.Instance.doIHaveCameraAttached ( transform ))
				BattleBase.Instance.attachCameraToTheForemostFront( BattleBase.Instance.LeaderTransform.gameObject );
			

			BattleBase.Instance.addDeathCount_Enemy();
			
			if (BattleBase.Instance.EnemyToKill == 0)
			{
				Log.jprint(gameObject + " is dead ! ! ! ! ! !  GO SLOW MOTION.");

				Knowledge.startSlowMotion(BattleTuning.Instance._slowMotionBossDeathTimeScale,
					BattleTuning.Instance._slowMotionBossDeathDuration);

				if (BattleUI.Instance() != null)
					BattleUI.Instance().gob_leaderHud.SetActive(false);
			}

			//			if (Knowledge.WaveSignalOn)
			//				BattleBase.Instance.waveSignalReady(getWaveSignalPosition());
			//			Inventory.Instance().checkMonQuest(Knowledge.BotID);

			if (Knowledge.IsLeader)
				BattleBase.Instance.switchLeader_P2_leader_death();

		}

		hideHealthBar();
		destroyMe();
	}
	
	
	protected override bool cancelAttackIfNoEnemy()
	{
		//Log.jprint("Projectile :: cancelAttackIfNoEnemy()");
		bool found = Knowledge.checkEnemyOnPath(false);
		if (!found)
		{
			if (Knowledge.IsLeader)
			{
				forceRun();
			}
			else
			{
				forceWalkBack(); //jks no enemy , so walkback
				Knowledge.activateSkillFinishedEvent();  //jks activate leader
			}
			
			Knowledge.endAim();
		}
		
		_isDelayedInvoked_cancelAttackIfNoEnemy = false;
		
		return !found;  //jks is canceled?
	}


	//default
	public override Action idle()
	{
		#if DEBUG_LOG		
		if (gameObject.name.Contains("Dummy") == false)
		Log.jprint(gameObject + "  V I S I T : Action Idle()");
		#endif
		//if (Knowledge.Action_Idle)
		{			
			#if DEBUG_LOG		
			Log.jprint(gameObject + "   Idle");
			#endif

			Knowledge.Action_Run = true;

			//			if (!AnimCon.isAnimPlaying(Knowledge.Anim_Idle))
			//			{
			//				animate_Idle();
			//			}

			yield return React.NodeResult.Success;
		}
		yield return React.NodeResult.Failure;
	}	

}
