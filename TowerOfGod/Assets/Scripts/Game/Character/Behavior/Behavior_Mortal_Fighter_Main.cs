//#define DEBUG_LOG

using UnityEngine;
using System.Collections;
using React;

//This is a little shortcut which creates an alias for a type, and makes out Action methods look much nicer.
using  Action = System.Collections.Generic.IEnumerator<React.NodeResult>;



public class Behavior_Mortal_Fighter_Main : Behavior_Mortal_Fighter
{


	new public Knowledge_Mortal_Fighter_Main Knowledge
	{
		get
		{
			if (_knowledge == null)
			{
				_knowledge = GetComponent<Knowledge>();
			}
			return (Knowledge_Mortal_Fighter_Main) _knowledge;
		}
	}

	public override void attachCameraAndSetTarget()
	{
		if (Knowledge.AllyID != eAllyID.Ally_Human_Me) return;

		CameraManager camMan = CameraManager.Instance;
		if (camMan == null) return;

		if (! gameObject.activeSelf) return;
		
		int targetObjIndex;
		if (Knowledge.Progress_SkillAction 
		    || Knowledge.Progress_Action_Quick
		    || (Knowledge.KnowledgeSSkill != null && Knowledge.KnowledgeSSkill.Progress_AnyAction))
			targetObjIndex = Knowledge.CameraTargetAttack;

		else if (Loco.CurrentSpeed > 0)
			targetObjIndex = Knowledge.CameraTargetMove;

		else 
			targetObjIndex = Knowledge.CameraTargetIdle;
		

		camMan.setTarget(transform, targetObjIndex);
		BattleBase.Instance.CameraFocusedCardIndex = Knowledge.CardDeckIndex;
	}


	//jks - main character death
	protected override void processDeath()
	{
		//Log.jprint(gameObject + ":  processDeath .............CallDeathEvent().........");
		CallDeathEvent();
		if (BattleUI.Instance() != null)
			BattleUI.Instance().setPlayerDead(Knowledge.CardDeckIndex);

		//jks if dead and camera attached, then attach camera to leader.
		if (!Knowledge.IsLeader)
		{
			if (CameraManager.Instance.doIHaveCameraAttached(transform))
			{
				//Log.jprint("$ $ $ $ new fix . . . . . . . . . . . . . . . ");
				BattleManager.Instance.attachCameraToLeader();
			}
		}

		BattleBase.Instance.resetAllCoolTime_SupportSkill();

		hideHealthBar();
		destroyMe();
	}


//	void OnEnable()
//	{
//		RegisterInput(true);
//	}
//	
//	void OnDisable()
//	{
//		RegisterInput(false);
//	}



//	protected virtual void RegisterInput(bool value)
//	{
//		if (BattleBase.Instance.isSkillPreviewMode()) return;
//
//		if (value)
//		{
//			Gesture.Instance().SetLayerMask = (1 << LayerMask.NameToLayer("EnemySelect")); 
//			InputManager.onSingleTapEvent += onSingleTap; 
//			//jks - UI design changed - InputManager.onSwipeEvent += onSwipe;
//		}
//		else
//		{
//			InputManager.onSingleTapEvent -= onSingleTap;
//			//jks - UI design changedInputManager.onSwipeEvent -= onSwipe;
//		}
//	}
//
//	void onSingleTap(Vector2 pos, Transform trans)
//	{
//		if (trans == null) return;
//		if (!BattleBase.Instance.IsPriorityTargetStrategyOn) return;
//		if (CameraManager.Instance.IsEventScene) return;
//		//Debug.Log("[Touch : " + pos.x + " , " + pos.y + "]");
//		//Log.jprint(gameObject + "   onTouch() trans: " + trans.parent.gameObject);
//
//		BattleBase.Instance.PriorityTarget = trans.parent.gameObject;
//	}

	//jks - removed because UI design changed.  
//	void onSwipe(SwipeInfo gi)
//	{
//		//Debug.Log("[onSwipe : " + gi.getDirection() + "]");
//
//		if (!Knowledge.IsLeader) return;
//
//		if(gi.getDirection() == SwipeInfo.Direction.Right)
//		{
//			if (Knowledge.Action_Walk)
//			{
//				forceWalkFast();
//			}
//			else if (Knowledge.Action_WalkFast)
//			{
//				forceRun();
//			}
//		}
//		else if(gi.getDirection() == SwipeInfo.Direction.Left)
//		{
//			if (Knowledge.Action_WalkFast)
//			{
//				forceWalk();
//			}
//			else if (Knowledge.Action_Run)
//			{
//				forceWalkFast();
//			}
//		}
//	}

	protected override void Update()
	{
		if (Knowledge == null) return;
		if (Knowledge.Progress_SkillAnimation) return;
		if (Knowledge.HoldUpdate) return; //jks  to hold behavior update() summoned bot , between addcomponents and knowledge initial value setup.
		if (!BattleBase.Instance.IsBattleInProgress) return;
		if (!Knowledge.IsBattleTimeStarted) return;
		if (!BattleBase.Instance.IsPVP 
			&& 
			(BattleBase.Instance.EnemyToKill <= 0 && BattleBase.Instance.isThisVictoryCondition(eBattleVictoryConditionIndex.BVC_KillBoss))
		   )
			return; //jks story staging
		
		scan();

	}


	//jks 20164.21 Mantis1448 :
	protected virtual void decideQuickOrMainSkill()
	{
		if (BattleBase.Instance.IsPlay_AutoLeader)
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
						Knowledge.LeaderCoolingRun = true;
					}
					else if (!Knowledge.AI_AutoCombo_QuickSkill)
					{	//jks 20164.21 Mantis1448 : 메인스킬 쿨 중이면, 평타 시작.
						Knowledge.AI_AutoCombo_QuickSkill = true;
						Knowledge.Action_Run = true;
						Log.jprint(Time.time+ "   - - - Quick (평타) 공격 시작. - - -   " + gameObject);
					}
				}
				else
				{
					Knowledge.LeaderCoolingRun = true; //jks 쿨타임 중 생성된 적이 없으면 뛰기. (쿨타임 중 적이 없는데 정지하고 있는 상황 제거.)
				}
			}
		}
		else
		{
			Knowledge.LeaderCoolingRun = false; //jks 리더 자동이 아니면 쿨타임 런 중지.
		}

	}


	protected override void scan()
	{
		if (Knowledge == null || 
		    BattleBase.Instance.IsDialogShowOff || 
		    //jks - 2015.4.28 let switched leader run		    ( Knowledge.IsLeader && BattleBase.Instance.IsStrategy_Default && !Knowledge.IsClassMatching && !CameraManager.Instance.IsFeverTime && !Knowledge.isRoleSkillActive()) || 
		    Knowledge.Action_InstallWeapon || 
		    Knowledge.Action_InstallWeapon_Pre)
		{
			//Log.Warning(" Knowledge is null : "+ gameObject);
			return;
		}


//		//jks 자동 리더는 팀모드 시작되면 모드 종료까지 공격 안함.
//		if (BattleTuning.Instance._teamMemberStaging && BattleBase.Instance.IsTeamRush)
////jks 12.24 자동에서도 적용. -   && !BattleBase.Instance.IsPlay_AutoTeam)
//		{
//			if (Knowledge.IsLeader && BattleBase.Instance.IsPlay_AutoLeader)
//			{
//				// 뛰고 있는 중이고 적을 만나면 정지하게함.
//				if(Knowledge.Action_Run)
//				{
//					bool found = Knowledge.checkEnemyOnPath(false);
//					if (found)
//					{
//						Knowledge.Action_Run = false;
//					}
//				}
//				
//				return;
//			}
//		}



		//jks 평타 리더 AI
		if (Knowledge.IsLeader) // leader ?
		{
			decideQuickOrMainSkill();
		}
		else
		{
			Knowledge.AI_AutoCombo_QuickSkill = false;
		}




		if (Knowledge.Action_Run ||
		    Knowledge.Action_Walk ||
		    Knowledge.Action_WalkFast ||
		    (Knowledge.AmIAuto && Knowledge.Action_Idle) ||  //jks even during idle if i am auto, scan opponent.
			(Knowledge.IsLeader && BattleBase.Instance.IsPlay_AutoLeader && Knowledge.LeaderCoolingRun) //jks 쿨타임 중 뛰고 있는지 ?
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

	public void scanEnemyToAttack()
	{
		scanEnemy();
	}


	//리더인경우 타겟팅된 대상의 transform을 셋팅 
	protected virtual void setForCamLockOn()
	{
		GameObject target_cur = Knowledge.getCurrentTarget();
		if (Knowledge.IsLeader && target_cur)
			CameraManager.Instance.lockOnTarget( target_cur.transform );
	}


	protected override void scanEnemy()
	{
		//Log.jprint(gameObject + " . . . . . . . Scan enemy ");
			
		if (BattleBase.Instance.IsSkillStagingInProgress) return;

		bool found = Knowledge.checkEnemyOnPath(false);

		//jks 자동 플레이에서 쿨타임 중이기 때문에 달리고 있는 리더 경우, 적을 만나면 정지하고 팀원을 출격시킴.
		if (found && Knowledge.IsLeader) // leader ?
		{
			if (BattleBase.Instance.IsPlay_AutoLeader && Knowledge.LeaderCoolingRun)
			{
				Knowledge.LeaderCoolingRun = false;  //jks 적을 만나면 정지.
				if (BattleBase.Instance.IsPlay_AutoTeam)
					BattleBase.Instance.autoUseCardWithBiggestSinsu();				
			}
		}

		if (!Knowledge.canUseSkill()) return;
				


		//jks - 2015.4.28 let switched leader run, but stops in front of enemy. 
		if (Knowledge.IsLeader && found && BattleManager.Instance.IsPlay_Manual)
		{
			//jks if not class matching. if not fever mode. if not role skill active. => stop
			if (!Knowledge.IsClassMatching)
			{
				Knowledge.Action_Run = false;
				Knowledge.Action_Idle = true;
				return;
			}
		}

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
					Invoke("cancelAttackIfNoEnemy", Knowledge.OpponentSearchingTime);
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


	protected virtual void startQuickSkill()
	{
		BattleBase.Instance.startLeaderQuickSkill();
	}


	protected void scanEnemy_QuickSkill()
	{
		if (!Knowledge.canUseQuickSkill()) return;
		
		//Log.jprint(gameObject + " . . . > . . . Scan enemy QuickSkill");
		
		
		
		bool found = Knowledge.checkEnemyOnPath(Knowledge.AI_AutoCombo_QuickSkill);
		
		//		if (gameObject.name.Contains("C3"))
		//		{
		//			if (Knowledge.AI_AutoCombo_QuickSkill && found)
		//				Log.jprint(gameObject + "  --------------   AI_AutoCombo_QuickSkill: "+ Knowledge.AI_AutoCombo_QuickSkill + "      found: "+ found);
		//			else
		//				Log.jprint(gameObject + "         AI_AutoCombo_QuickSkill: "+ Knowledge.AI_AutoCombo_QuickSkill + "      found: "+ found);
		//		}
		
		

		if (found)
		{
			//jks 평타 리더 AI
			startQuickSkill();

			//리더인경우 타겟팅된 대상의 transform을 셋팅 
			setForCamLockOn();
		}
		else 
		{
			if (!Knowledge.Progress_Action_Quick)  
			{
				// if we have enemy ahead, run
				if (Knowledge.isEnemyAppeared())
				{
					Knowledge.Action_Run = true;
				}
			}
		}
		
		
	}


	#region strategy dependant locomotion
	//jks obsolete
//	public void leaderStrategyLocomotion_AfterCooled()
//	{
//		if (Knowledge.Action_Hit) return; //jks let reaction finish first before move
//		if (Knowledge.Progress_CoolingJump) return; //jks let cooling jump action complete
//
//		if (BattleBase.Instance.LeaderStrategyType == eLeaderStrategy.LS_Caution)
//		{
//			forceWalk();
//		}
//		else if (BattleBase.Instance.LeaderStrategyType == eLeaderStrategy.LS_SinsuFirst)
//		{
//			float ratio = (float)Knowledge.CurrentSinsu / (float)Knowledge.MaxSinsu;
//			if (ratio < 0.4f)
//				forceWalk();
//			else
//				forceWalkFast();
//		}
//		else
//		{
//			forceRun();
//		}
//	}
	public void leaderStrategyLocomotion_AfterCooled()
	{
		if (Knowledge.Action_Hit) return; //jks let reaction finish first before move
		if (Knowledge.Progress_CoolingJump) return; //jks let cooling jump action complete
		
		if (! Knowledge.AmIAuto 
//			||(BattleTuning.Instance._teamMemberStaging && BattleBase.Instance.IsTeamRush && !BattleBase.Instance.IsPlay_AutoTeam))
			)
		{
			Knowledge.Action_Idle = true;
			Knowledge.Action_Walk = false;
		}
		else
		{
			Knowledge.Action_Run = true;  //forceRun();
		}
	}

	//jks obsolete
//	protected void leaderStrategyLocomotion_IfEnemyAppeared()
//	{
//		if (Knowledge.Action_Hit) return; //jks let reaction finish first before move
//
//		if (!Knowledge.IsLeader)
//		{
//			forceRun();
//			return;
//		}
//
//		//jks leader
//
//		if (BattleBase.Instance.LeaderStrategyType == eLeaderStrategy.LS_Caution)
//		{
//			forceWalk();
//		}
//		else
//		{
//			forceRun();
//		}
//	}
	protected void leaderStrategyLocomotion_IfEnemyAppeared()
	{
		if (Knowledge.Action_Hit) return; //jks let reaction finish first before move
		
		if (!Knowledge.IsLeader)
		{
			Knowledge.Action_Run = true;//forceRun();
			return;
		}
		
		//jks leader
		
		//if (Knowledge.AmIAuto && !BattleBase.Instance.IsTeamRushFirstSkillUsed)
		if (Knowledge.AmIAuto)
		{
			Knowledge.Action_Run = true;//forceRun();
		}
		else
		{
			Knowledge.Action_Idle = true;
			Knowledge.Action_Walk = false;
		}
	}

	 
	
	protected void leaderStrategyLocomotion_SwichLeader()
	{
		if (Knowledge.Progress_SkillAnimation) return; //jks do not move if in attack action

//		if (BattleBase.Instance.LeaderStrategyType == eLeaderStrategy.LS_Caution)
//		{
//			//Log.jprint("****** switch leader- walk");
//			forceWalk();
//		}
//		else if (BattleBase.Instance.LeaderStrategyType == eLeaderStrategy.LS_SinsuFirst)
//		{
//			//Log.jprint("****** switch leader- walk fast");
//			forceWalkFast();
//		}
//		else
//		{
//			//Log.jprint("****** switch leader- run");
//			forceRun();
//		}

		if(Knowledge.IsLeader && (Knowledge.Action_InstallWeapon || Knowledge.Action_InstallWeapon_Pre)) //jks  만약, 리더가 무기 장착 중이면 아래 forceRun() / forceIdle() 건너 뛰어,  flag 리셋되지 않게하여 계속 무기장착 액션 진행 되게 함.
			return;

//jks - 2015.4.28 let switched leader run		if (BattleBase.Instance.Strategy == eBattleStrategy.BS_AutoTeamAndLeader)
		//jks 2016.1.1 리더 변경 시 자동일 경우만 전진함.
		if (Knowledge.AmIAuto)
		{
			//Log.jprint("****** switch leader- R U N");
			forceRun();
		}
		else
		{
			forceIdle();
		}

//jks - 2015.4.28 let switched leader run
//		else
//		{
//			Log.jprint("****** switch leader- I D L E");
//			forceIdle(); //forceWalk();
//		}

	}
	
	#endregion	

	protected override bool cancelAttackIfNoEnemy()
	{
		if (BattleManager.Instance.isSkillPreviewMode()) return true;
		if (BattleManager.Instance.IsDialogShowOff) return true;

		//Log.jprint("''''''''''''''''''''''Main :: cancelAttackIfNoEnemy()");
		bool found = Knowledge.checkEnemyOnPath(false);
		if (!found)
		{
			if (!Knowledge.IsLeader)
			{
//				if (Knowledge.ImmediateAttack)
//				{
//					forceIdle();
//				}
//				else

				if (!Knowledge.Action_WalkBackTurn && !Knowledge.Action_WalkBack)
				{
					forceWalkBack();

					Knowledge.resetCoolTimer(); //jks BattleBase.Instance.endUICoolTime(Knowledge.CardDeckIndex);//jks card use canceled, so let the card available.
					Knowledge.activateSkillFinishedEvent();  //jks activate leader
				}
					
				 
			}

			if (BattleUI.Instance() != null)
			{
				//클래스 매치 스킬 시전중이었다면 스킬 발동 플래그 해제
				if (BattleUI.Instance().ClassMatchOwner == Knowledge.CardDeckIndex)
				{
					BattleUI.Instance().ClassMatchOwner = -1;
				}
			}
		}

		_isDelayedInvoked_cancelAttackIfNoEnemy = false;

		return !found;  //jks is canceled?
	}

	
	public void leadNow()
	{
		//Log.jprint  (gameObject + "  leadNow()");

		Knowledge.setLeader(true);

		Invoke("leaderStrategyLocomotion_SwichLeader", 0.1f);  //jks delay for full activation
	}
	
	
	public void leadNot()
	{
		//Log.jprint  (gameObject + "  leadNot()");

		Knowledge.setLeader(false);
		forceWalkBack();//forceIdle();
	}
	
	
	public void deployCardActor()
	{
//		if(name.Contains("M2"))
//			Log.jprint  (gameObject + "  deployCardActor() oooooooooooooooooooooooooooooooooooooooooo");
		CancelInvoke(); //jks cancel previously invoked "cancelAttackIfNoEnemy" function.
		Invoke("forceRun", 0.1f);  //jks delay for full activation
	}


	public void letLeaderWalk()
	{
		//Log.jprint  (gameObject + "W A L K    W A L K    W A L K    letLeaderWalk()");
		//Invoke("forceWalk", 0.1f);  //jks delay for full activation
		if (! Knowledge.Progress_SkillAnimation)
			forceIdle(); //forceWalk();
	}


	protected virtual void animate_WalkBackTurn()
	{
		#if DEBUG_LOG_ANIMATION		
		Log.jprint(gameObject.name +"    animate_WalkBackTurn      "+ AnimationBlendingInfo.Instance._blend_walkback);
		#endif
		AnimCon.applyRootMotion(true);
		AnimCon.startAnimation(Knowledge.Anim_WalkBackTurn, AnimationBlendingInfo.Instance._blend_walkback, Loco.AnimSpeed_WalkBack * MovementPlayRate, 0);
		resetComboCurrent();
		Knowledge._isResetActionInfoDone = false;

		Knowledge.endAim();
		
		//Knowledge.showWeapon(false);
		Loco.setCurrentSpeedToZero();
	}




	//jks behavior tree action
	#region REACT (BEHAVIOR TREE)


	//jks “신수 전략”은 더 이상 사용 안함. //	public bool isSinsuRefillMode()
//	{
//		return BattleBase.Instance.IsSinsuRefillMode;
//	}





//	public virtual Action sinsuRefillMode_WalkBack()
//	{
//		//if (BattleBase.Instance.isEnemyInView())
//		if (! BattleBase.Instance.isEnemyFarEnoughForSinsuRefill())  //jks show sinsu refill UI quicker.
//		{
//			#if DEBUG_LOG		
//			Log.jprint (gameObject + "   sinsuRefillMode_WalkBack");
//			#endif
//			if (!AnimCon.isAnimPlaying(Knowledge.Anim_WalkBack))
//			{
//				Knowledge.forceResetFlags();
//				animate_WalkBack();
//			}
//			
//			yield return React.NodeResult.Success;
//			
//		}
//		yield return React.NodeResult.Failure;
//	}


	public override Action hit()
	{
		if (Knowledge.Action_Hit)  
		{	
			if (Knowledge.amIMelee() && Knowledge.IsLeader)//jks 근접공격 리더가,
			{

				if (Knowledge.manualJumpBack() //jks 수동 후퇴이거나,
					|| Knowledge.shouldIRun()  //jks 수동 전진이거나,
					|| Knowledge.Progress_Action_Quick) //jks 평타 시작했거나,
				{
					Knowledge.Action_Hit = false; //reset  //jks , hit 반응 무시.
					yield return React.NodeResult.Failure;
				}
			}

			if (!Knowledge.Progress_HitReaction)
			{			
				animate_Hit();
			}

			yield return React.NodeResult.Success;
		}

		yield return React.NodeResult.Failure;
	}	



	public virtual Action exhausted()
	{
		if (Knowledge.IsLeader)
		{
			#if DEBUG_LOG		
			Log.jprint (gameObject + "   exhausted");
			#endif
			if (!AnimCon.isAnimPlaying(Knowledge.Anim_Exhausted))
			{
				Knowledge.forceResetFlags();
				animate_Exhausted();
				//if (BattleUI.Instance() != null)
				//	StartCoroutine( BattleUI.Instance().setSinsuPop());
				GetComponent<AnimationOverride>().lookBegin(CameraManager.Instance.CameraTransform);
			}
		}

		yield return React.NodeResult.Success;
	}



//	//jks 리더일 경우 cooling 들어 가기전에 뒤로 점프하는 동작
//	public override Action coolingJump()
//	{	
//		//jks by user input
//		if (Knowledge.forceJumpBack())
//		{
//			if (!AnimCon.isAnimPlaying(Knowledge.Anim_CoolingJump))
//			{
//				Knowledge.forceResetFlags();
//				animate_CoolingJump();
//			}
//			
//			#if DEBUG_LOG		
//			Log.jprint (gameObject + "   cooling jump");
//			#endif
//			yield return React.NodeResult.Success;
//
//		}
//		else if (Knowledge.shouldIJumpBack()) 
//		{
//			if (!AnimCon.isAnimPlaying(Knowledge.Anim_CoolingJump))
//			{
//				//Knowledge.forceResetFlags();
//				animate_CoolingJump();
//			}
//			
//			if (!Knowledge.Progress_CoolingJump) //jks if jump back action is finished
//			{
//				Knowledge.CoolingJumpDone = true;
//			}
//			
//			#if DEBUG_LOG		
//			Log.jprint (gameObject + "   cooling jump");
//			#endif
//			yield return React.NodeResult.Success;
//		}
//
//		yield return React.NodeResult.Failure;
//	}


	//jks 리더일 경우 cooling 들어 가기전에 뒤로 점프하는 동작
	public override Action coolingJump()
	{	
		//jks by user input
		if (Knowledge.manualJumpBack())
		{
			if (!AnimCon.isAnimPlaying(Knowledge.Anim_CoolingJump))
			{
				Knowledge.forceResetFlags();
				animate_CoolingJump();
			}

			#if DEBUG_LOG		
			Log.jprint (gameObject + "   cooling jump");
			#endif
			yield return React.NodeResult.Success;

		}
		else
		{
			if (Knowledge.IsLeader
				&& Knowledge.AttackCoolTimer.IsCoolingInProgress 
				&& !Knowledge.LeaderCoolingRun 
				&& Knowledge.amIInView())
			{
				Knowledge.shouldIJumpBack();

				if (Knowledge.Action_CoolingJump)
				{
					if (!Knowledge.Progress_CoolingJump && !AnimCon.isAnimPlaying(Knowledge.Anim_CoolingJump))
					{
						//Knowledge.forceResetFlags();
						animate_CoolingJump();
					}
					yield return React.NodeResult.Success;
				}
			}
		}

		yield return React.NodeResult.Failure;
	}



	public override Action cooling()
	{
		if (Knowledge.IsLeader)
		{
			if ( Knowledge.AttackCoolTimer.IsCoolingInProgress 
				&& !Knowledge.LeaderCoolingRun 				//jks 자동 리더 쿨링 중  적이 없어 뛰어야 하는 상황 아니면,
				&& !BattleBase.Instance.ForceMoveLeaderRun  //jks 수동 리더 이동 아니면,
				&& Knowledge.amIInView()
				&& !Knowledge.AI_AutoCombo_QuickSkill 		//jks 쿨링 중 공격 받아 평타로 가는 경우 아니면,
			)
			{
				#if DEBUG_LOG		
				Log.jprint (gameObject + "   cooling");
				#endif
				if (!AnimCon.isAnimPlaying(Knowledge.Anim_Exhausted))
				{
					//Knowledge.forceResetFlags();
					animate_Exhausted();
				}
				
				yield return React.NodeResult.Success;
				
			}
		}
		else
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



	public virtual Action walkBackTurn()
	{
		//Log.jprint ("Knowledge.Action_Walk: "+ Knowledge.Action_Walk);
		if (Knowledge.Action_WalkBackTurn && !Knowledge.Progress_SkillAnimation)  //jks - let attack finish it's action before start move.
		{			
			#if DEBUG_LOG		
			Log.jprint (gameObject + "   walk back");
			#endif			
			
			if (!AnimCon.isAnimPlaying(Knowledge.Anim_WalkBackTurn))
			{
				animate_WalkBackTurn();
			}
			
//			BattleBase.Instance.accordHeading(gameObject); //jks if leader moving direction changed, walkback character should change  
			
			//jks Hack: in case, if walk back character has camera focus, set focus to leader.
			//if (Knowledge.CardDeckIndex == BattleBase.Instance.CameraFocusedCardIndex)
			if (CameraManager.Instance.doIHaveCameraAttached(transform))
			{
				BattleBase.Instance.attachCameraToTheForemostFront(gameObject);
			}
			
			yield return React.NodeResult.Success;
		}
		
		yield return React.NodeResult.Failure;
	}
	
	

	public override Action walkBack()
	{
		//Log.jprint ("Knowledge.Action_Walk: "+ Knowledge.Action_Walk);
		if (Knowledge.Action_WalkBack && !Knowledge.Progress_SkillAnimation)  //jks - let attack finish it's action before start move.
		{			
			#if DEBUG_LOG		
			Log.jprint (gameObject + "   walk back");
			#endif			

			if (!AnimCon.isAnimPlaying(Knowledge.Anim_WalkBack))
			{
				animate_WalkBack();
			}

//			BattleBase.Instance.accordHeading(gameObject); //jks if leader moving direction changed, walkback character should change  

			//jks Hack: in case, if walk back character has camera focus, set focus to leader.
			//if (Knowledge.CardDeckIndex == BattleBase.Instance.CameraFocusedCardIndex)
			if (CameraManager.Instance.doIHaveCameraAttached(transform))
			{
				BattleBase.Instance.attachCameraToTheForemostFront(gameObject);
			}

			yield return React.NodeResult.Success;
		}
		
		yield return React.NodeResult.Failure;
	}


	public override Action run()
	{
		if (Knowledge.shouldIRun()) 
		{			
			#if DEBUG_LOG		
			Log.jprint (gameObject + "    run");
			#endif
			
			if (!AnimCon.isAnimPlaying(Knowledge.Anim_Run) || MovementPlayRate != _movementPlayRate_Prev)
			{
				//Log.jprint (gameObject + "    call animate_Run");
				animate_Run();
				//				if (! BattleBase.Instance.isSkillPreviewMode())
				//					setCameraTarget(CameraManager.TargetState.Walk);
				//150803_jsm - 카메라 중심점 타겟팅 방식 자동으로 변경
//				if (Knowledge.IsLeader)
//					setCameraTarget(CameraManager.TargetState.Walk_fast);
				if (Knowledge.IsLeader)
				{
					Knowledge.resetContinuousHitCount();
					Knowledge.Action_Hit = false;  //jks 2015.12.22 수동이동이 리액션 보다 우세.
				}
			}
			
			//scanEnemy();
			
			yield return React.NodeResult.Success;
		}
		
		yield return React.NodeResult.Failure;
	}	


//	public virtual Action roleSkill()
//	{
//		#if DEBUG_LOG		
//			Log.jprint(gameObject + "  V I S I T : Action roleSkill()");
//		#endif
//		if (Knowledge.Action_RoleSkill) 
//		{	
//			if (!Knowledge.Progress_RoleSkillAnimation)
//			{			
//				animate_roleSkill();
//			}
//			
//			yield return React.NodeResult.Success;
//		}
//		yield return React.NodeResult.Failure;
//	}	


	public virtual Action guardUp()
	{
		#if DEBUG_LOG		
			Log.jprint(gameObject + "  V I S I T : Action roleSkill()");
		#endif
		if (Knowledge.Action_Guard_Up && Knowledge.IsLeader) 
		{	
			if (!Knowledge.Progress_GuardUpAnimation)
			{			
				animate_GuardUp();
			}
			
			yield return React.NodeResult.Success;
		}
		yield return React.NodeResult.Failure;
	}	


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
			
			if (!AnimCon.isAnimPlaying(Knowledge.Anim_Idle))
			{
				animate_Idle();

				if (Knowledge.IsLeader)
				{
					//150803_jsm - 카메라 중심점 타겟팅 방식 자동으로 변경
//					setCameraTarget(CameraManager.TargetState.Walk_fast);

					Knowledge.AI_AutoCombo_QuickSkill = false;

				}
			}
			
			yield return React.NodeResult.Success;
		}
		yield return React.NodeResult.Failure;
	}	


	
//	protected virtual void animate_roleSkill()
//	{		
//		#if DEBUG_LOG_ANIMATION
//		Log.jprint(gameObject.name +"     animate_roleSkill      "+ AnimationBlendingInfo.Instance._blend_block);
//		#endif
//		AnimCon.applyRootMotion(false);
//		AnimCon.startAnimation(Knowledge.Anim_RoleSkill, AnimationBlendingInfo.Instance._blend_combo1, 1, 0);
//		Knowledge.Progress_RoleSkillAnimation = true;
//		//if (Knowledge.RoleSkillType == SKILL_TYPE.TYPE_T)
//			Knowledge._isResetActionInfoDone = false;
//		
//		//Knowledge.showWeapon(false);
//		Loco.setCurrentSpeedToZero();
//	}


	protected virtual void animate_GuardUp()
	{		
		AnimCon.applyRootMotion(false);
		AnimCon.startAnimation(Knowledge.Anim_GuardUp, AnimationBlendingInfo.Instance._blend_combo1, 1, 0);
		Knowledge.Progress_GuardUpAnimation = true;

		Knowledge._isResetActionInfoDone = false;
		
		Loco.setCurrentSpeedToZero();
	}




	public virtual Action skillStaging()
	{
		#if DEBUG_LOG		
		Log.jprint(gameObject + "  V I S I T : Action skillStaging()");
		#endif
		if (Knowledge.Action_SkillStaging) 
		{	
			if (!Knowledge.Progress_SkillStaging)
			{			
				animate_skillStaging();
			}
			
			yield return React.NodeResult.Success;
		}
		yield return React.NodeResult.Failure;
	}	
	



	protected virtual void animate_skillStaging()
	{		
		#if DEBUG_LOG_ANIMATION
		//if (gameObject.name.Contains("C") == false)
		Log.jprint(Time.time + "  " + gameObject.name +"   animate_skill_staging()     Knowledge.SkillStagingFactor: "+ Knowledge.SkillStagingFactor );
		#endif
		Log.nprint(gameObject.name +" :   Anim_Skill_Staging ");
		
		AnimCon.applyRootMotion(false);

		AnimCon.startAnimation(Knowledge.Anim_Skill_Staging, 0, 1, 0);

		Knowledge.Progress_SkillStaging = true;
		Knowledge._isResetActionInfoDone = false;
		if (gameObject.name.Contains("C3") == false)
			Log.jprint(Time.time + "  " + gameObject.name +"   animate_skillStaging()     _isResetActionInfoDone: "+ Knowledge._isResetActionInfoDone );
		
		Loco.setCurrentSpeedToZero();
	}
	

	bool IsSupportSkillInProgress { get { return Knowledge.KnowledgeSSkill && Knowledge.KnowledgeSSkill.Progress_Action ; }}


	public virtual bool comboQuick1()
	{
		if (Knowledge.Action_ComboQuick1) 
		{			
			if (Knowledge.Progress_SkillAction //jks if skill is activated, do not start QuickSkill. (in BT, QuickSkill is prior to cooltime, but cooltime is prior to Skill, and Skill is prior to QuickSkill, so give priority to Skill over QuickSkill here.)
			    || IsSupportSkillInProgress)
			{
				Knowledge.Action_ComboQuick1 = false;
				return false;
			}

			if (!Knowledge.Progress_AnimComboQuick1)
			{
				animate_ComboQuick1();
			}
			
			return true;
		}
		
		return false;
	}	
	
	
	public virtual bool comboQuick2()
	{
		if (Knowledge.Action_ComboQuick2) 
		{			
			if (Knowledge.Progress_SkillAction //jks if skill is activated, do not start QuickSkill
				|| IsSupportSkillInProgress)
			{
				Knowledge.Action_ComboQuick2 = false;
				return false;
			}

			if (!Knowledge.Progress_AnimComboQuick2)
			{
				animate_ComboQuick2();
			}
			
			return true;
		}
		
		return false;
	}	
	
	
	public virtual bool comboQuick3()
	{
		if (Knowledge.Action_ComboQuick3) 
		{			
			if (Knowledge.Progress_SkillAction //jks if skill is activated, do not start QuickSkill
				|| IsSupportSkillInProgress)
			{
				Knowledge.Action_ComboQuick3 = false;
				return false;
			}
			
			if (!Knowledge.Progress_AnimComboQuick3)
			{
				animate_ComboQuick3();
			}
			
			return true;
		}
		
		return false;
	}	
	
	
	public virtual bool comboQuick4()
	{
		if (Knowledge.Action_ComboQuick4) 
		{			
			if (Knowledge.Progress_SkillAction //jks if skill is activated, do not start QuickSkill
			    || IsSupportSkillInProgress)
			{
				Knowledge.Action_ComboQuick4 = false;
				return false;
			}
			
			if (!Knowledge.Progress_AnimComboQuick4)
			{
				animate_ComboQuick4();
			}
			
			return true;
		}
		
		return false;
	}	
	
	
	public virtual bool comboQuick5()
	{
		if (Knowledge.Action_ComboQuick5) 
		{			
			if (Knowledge.Progress_SkillAction //jks if skill is activated, do not start QuickSkill
			    || IsSupportSkillInProgress)
			{
				Knowledge.Action_ComboQuick5 = false;
				return false;
			}
			
			if (!Knowledge.Progress_AnimComboQuick5)
			{
				animate_ComboQuick5();
			}
			
			return true;
		}
		
		return false;
	}	
	
	
	public virtual bool comboQuick6()
	{
		if (Knowledge.Action_ComboQuick6) 
		{			
			if (Knowledge.Progress_SkillAction //jks if skill is activated, do not start QuickSkill
			    || IsSupportSkillInProgress)
			{
				Knowledge.Action_ComboQuick6 = false;
				return false;
			}
			
			if (!Knowledge.Progress_AnimComboQuick6)
			{
				animate_ComboQuick6();
			}
			
			return true;
		}
		
		return false;
	}	
	

	#endregion




	protected override void resetComboCurrent()
	{
		base.resetComboCurrent();

		Knowledge.ComboCurrent_QuickSkill = 0;
	}




	#region Animation

	protected float SkillQuick_AniPlayRate
	{
		get 
		{
			return SkillAniPlayRate; //return 1.0f;//return Knowledge.calculate_SkillAnimationPlayRate();
		}
	}


	protected virtual void animate_ComboQuick1()
	{	
		#if UNITY_EDITOR	
		if (BattleTuning.Instance._showQuickSkillLog)
			Log.print_always(Time.time + "    === animation start:   combo 1  애니메이션 시작.  ");
		#endif

		#if DEBUG_LOG_ANIMATION
		//if (gameObject.name.Contains("C") == false)
		Log.jprint(Time.time + "  " + gameObject.name +"   animate_ComboQuick1()     Knowledge.Anim_Combo10: "+ Knowledge.Anim_ComboQuick1 + "    SkillAniPlayRate : "+SkillAniPlayRate);
		#endif
		Log.nprint(gameObject.name +" :   animate_ComboQuick1() ");
		
		comboRootMotion();
		AnimCon.startAnimation(Knowledge.Anim_ComboQuick1, AnimationBlendingInfo.Instance._blend_comboQuick1, SkillQuick_AniPlayRate, 0);
		Knowledge.Progress_AnimComboQuick1 = true;
		Knowledge.ComboCurrent_QuickSkill = 1;
		//Knowledge.ComboRecent = Knowledge.ComboCurrent;
		Knowledge._isResetActionInfoDone = false;

		//Knowledge.showWeapon(true);
		Loco.setCurrentSpeedToZero();
		attachCameraAndSetTarget();

		//jks 리더 평타 계속 진행 될 때 깊이 위치 값 보정.
		if (Knowledge.IsLeader)
		{
			Vector3 newPos = Knowledge.transform.position;
			newPos.z = BattleBase.Instance.getPlayerTrackDepth(Knowledge.CardDeckIndex);
			Knowledge.transform.position = newPos;
		}
	}

	protected virtual void animate_ComboQuick2()
	{		
		#if UNITY_EDITOR	
		if (BattleTuning.Instance._showQuickSkillLog)
		Log.print_always(Time.time +"   "+ gameObject.name + "    === animation start:   combo 2  애니메이션 시작.  ");
		#endif

		#if DEBUG_LOG_ANIMATION
		//if (gameObject.name.Contains("C") == false)
		Log.jprint(Time.time + "  " + gameObject.name +"   animate_ComboQuick2()     Knowledge.Anim_Combo10: "+ Knowledge.Anim_ComboQuick2 + "    SkillAniPlayRate : "+SkillAniPlayRate);
		#endif
		Log.nprint(gameObject.name +" :   animate_ComboQuick2() ");
		
		comboRootMotion();
		AnimCon.startAnimation(Knowledge.Anim_ComboQuick2, AnimationBlendingInfo.Instance._blend_comboQuick2, SkillQuick_AniPlayRate, 0);
		Knowledge.Progress_AnimComboQuick2 = true;
		Knowledge.ComboCurrent_QuickSkill = 2;
		//Knowledge.ComboRecent = Knowledge.ComboCurrent;
		Knowledge._isResetActionInfoDone = false;
		
		//Knowledge.showWeapon(true);
		Loco.setCurrentSpeedToZero();
	}

	protected virtual void animate_ComboQuick3()
	{		
		#if UNITY_EDITOR	
		if (BattleTuning.Instance._showQuickSkillLog)
		Log.print_always(Time.time +"   "+ gameObject.name +  "    === animation start:   combo 3  애니메이션 시작.  ");
		#endif

		#if DEBUG_LOG_ANIMATION
		//if (gameObject.name.Contains("C") == false)
		Log.jprint(Time.time + "  " + gameObject.name +"   animate_ComboQuick3()     Knowledge.Anim_Combo10: "+ Knowledge.Anim_ComboQuick3 + "    SkillAniPlayRate : "+SkillAniPlayRate);
		#endif
		Log.nprint(gameObject.name +" :   animate_ComboQuick3() ");
		
		comboRootMotion();
		AnimCon.startAnimation(Knowledge.Anim_ComboQuick3, AnimationBlendingInfo.Instance._blend_comboQuick3, SkillQuick_AniPlayRate, 0);
		Knowledge.Progress_AnimComboQuick3 = true;
		Knowledge.ComboCurrent_QuickSkill = 3;
		//Knowledge.ComboRecent = Knowledge.ComboCurrent;
		Knowledge._isResetActionInfoDone = false;
		
		//Knowledge.showWeapon(true);
		Loco.setCurrentSpeedToZero();
	}
	
	protected virtual void animate_ComboQuick4()
	{		
		#if UNITY_EDITOR	
		if (BattleTuning.Instance._showQuickSkillLog)
		Log.print_always(Time.time + "   "+ gameObject.name + "    === animation start:   combo 4  애니메이션 시작.  ");
		#endif

		#if DEBUG_LOG_ANIMATION
		//if (gameObject.name.Contains("C") == false)
		Log.jprint(Time.time + "  " + gameObject.name +"   animate_ComboQuick4()     Knowledge.Anim_Combo10: "+ Knowledge.Anim_ComboQuick4 + "    SkillAniPlayRate : "+SkillAniPlayRate);
		#endif
		Log.nprint(gameObject.name +" :   animate_ComboQuick4() ");
		
		comboRootMotion();
		AnimCon.startAnimation(Knowledge.Anim_ComboQuick4, AnimationBlendingInfo.Instance._blend_comboQuick4, SkillQuick_AniPlayRate, 0);
		Knowledge.Progress_AnimComboQuick4 = true;
		Knowledge.ComboCurrent_QuickSkill = 4;

		//Knowledge.ComboRecent = Knowledge.ComboCurrent;
		Knowledge._isResetActionInfoDone = false;
		
		//Knowledge.showWeapon(true);
		Loco.setCurrentSpeedToZero();
	}
	
	protected virtual void animate_ComboQuick5()
	{		
		#if UNITY_EDITOR	
		if (BattleTuning.Instance._showQuickSkillLog)
		Log.print_always(Time.time +"   "+ gameObject.name +  "    === animation start:   combo 5  애니메이션 시작.  ");
		#endif

		#if DEBUG_LOG_ANIMATION
		//if (gameObject.name.Contains("C") == false)
		Log.jprint(Time.time + "  " + gameObject.name +"   animate_ComboQuick5()     Knowledge.Anim_Combo10: "+ Knowledge.Anim_ComboQuick5 + "    SkillAniPlayRate : "+SkillAniPlayRate);
		#endif
		Log.nprint(gameObject.name +" :   animate_ComboQuick5() ");
		
		comboRootMotion();
		AnimCon.startAnimation(Knowledge.Anim_ComboQuick5, AnimationBlendingInfo.Instance._blend_comboQuick5, SkillQuick_AniPlayRate, 0);
		Knowledge.Progress_AnimComboQuick5 = true;
		Knowledge.ComboCurrent_QuickSkill = 5;
		//Knowledge.ComboRecent = Knowledge.ComboCurrent;
		Knowledge._isResetActionInfoDone = false;
		
		//Knowledge.showWeapon(true);
		Loco.setCurrentSpeedToZero();
	}
	
	protected virtual void animate_ComboQuick6()
	{		
		#if UNITY_EDITOR	
		if (BattleTuning.Instance._showQuickSkillLog)
		Log.print_always(Time.time +"   "+ gameObject.name +  "    === animation start:   combo 6  애니메이션 시작.  ");
		#endif

		#if DEBUG_LOG_ANIMATION
		//if (gameObject.name.Contains("C") == false)
		Log.jprint(Time.time + "  " + gameObject.name +"   animate_ComboQuick6()     Knowledge.Anim_Combo10: "+ Knowledge.Anim_ComboQuick6 + "    SkillAniPlayRate : "+SkillAniPlayRate);
		#endif
		Log.nprint(gameObject.name +" :   animate_ComboQuick6() ");
		
		comboRootMotion();
		AnimCon.startAnimation(Knowledge.Anim_ComboQuick6, AnimationBlendingInfo.Instance._blend_comboQuick6, SkillQuick_AniPlayRate, 0);
		Knowledge.Progress_AnimComboQuick6 = true;
		Knowledge.ComboCurrent_QuickSkill = 6;
		//Knowledge.ComboRecent = Knowledge.ComboCurrent;
		Knowledge._isResetActionInfoDone = false;
		
		//Knowledge.showWeapon(true);
		Loco.setCurrentSpeedToZero();
	}
	

	
	#endregion

	



}
