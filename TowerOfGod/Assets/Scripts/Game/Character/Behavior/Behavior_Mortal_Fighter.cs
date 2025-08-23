//#define DEBUG_LOG
//#define DEBUG_LOG_ANIMATION
//#define OLD
using UnityEngine;
using System.Collections;
using React;

//This is a little shortcut which creates an alias for a type, and makes out Action methods look much nicer.
using  Action = System.Collections.Generic.IEnumerator<React.NodeResult>;



public class Behavior_Mortal_Fighter : Behavior_Mortal
{

	protected bool _isDelayedInvoked_cancelAttackIfNoEnemy = false;

	new public Knowledge_Mortal_Fighter Knowledge
	{
		get
		{
			if (_knowledge == null)
			{
				_knowledge = GetComponent<Knowledge>();
			}
			return (Knowledge_Mortal_Fighter) _knowledge;
		}
	}
	
	
	public Locomotion_Mortal_Fighter Loco
	{
		get
		{
			if (_locomotion == null)
			{
				_locomotion = GetComponent<Locomotion>();
				if (_locomotion == null) Log.Warning("Missing Locomotion component.");
			}
			return (Locomotion_Mortal_Fighter) _locomotion;
		}
	}

	 
	protected virtual void scan()
	{
		if (Knowledge == null || BattleManager.Instance.IsDialogShowOff)
			return;

		if (Knowledge.Action_Run ||
		    Knowledge.Action_Walk ||
		    Knowledge.Action_WalkFast ||
		    Knowledge.Action_Idle
		    )
		{
			scanEnemy();
		}
	}

	protected virtual void Update()
	{
//		if (gameObject.name.Contains("Dummy") == false)
//			Log.jprint(gameObject.name +" - = - = - Update() - = - = -" );

		if (Knowledge == null) return;
		if (Knowledge.HoldUpdate) return; //jks  to hold behavior update() summoned bot , between addcomponents and knowledge initial value setup.
		if (!BattleBase.Instance.IsBattleInProgress) return;
		if (!Knowledge.IsBattleTimeStarted) return;

		scan();

	}

	protected virtual void resetComboCurrent()
	{
		Knowledge.ComboCurrent = 0;
	}

	protected void turnToFaceCamera()
	{
		transform.rotation = Quaternion.Euler(0, 180, 0);
	}


	public virtual void attachCameraAndSetTarget()
	{
		return;
	}


	protected virtual void animate_Victory()
	{
		#if DEBUG_LOG_ANIMATION		
		Log.jprint(gameObject.name +" *********** animate_Victory      "+AnimationBlendingInfo.Instance._blend_victory);
		#endif

		Time.timeScale = 1;

		Knowledge.cancelRepeatingInvokes();
		
		//jks facing camera
		turnToFaceCamera();

		AnimCon.applyRootMotion(true);
		AnimCon.startAnimation(Knowledge.Anim_Victory, AnimationBlendingInfo.Instance._blend_victory, 1, 0);
		//AnimCon.startAnimation(Knowledge.Anim_Victory, 0.1f, 1, 0);
		resetComboCurrent();
		
		Knowledge.endAim();
		
		//Knowledge.showWeapon(false);
		Loco.setCurrentSpeedToZero();
		//Log.jprint(gameObject + " Loco.setCurrentSpeedToWalk()");
	}


	protected virtual void animate_Walk()
	{
		#if DEBUG_LOG_ANIMATION		
		Log.jprint(gameObject.name +" ********** animate_Walk     "+AnimationBlendingInfo.Instance._blend_walk);
		#endif
		AnimCon.applyRootMotion(false);

		float aniPlayRate = Knowledge.AnimSpeed_Walk * MovementPlayRate;
		AnimCon.startAnimation(Knowledge.Anim_Walk, AnimationBlendingInfo.Instance._blend_walk, aniPlayRate, 0);

		resetComboCurrent();

		_movementPlayRate_Prev = MovementPlayRate;

		Knowledge.endAim();

		//Knowledge.showWeapon(false);
		Loco.setCurrentSpeedToWalk();
		//Log.jprint(gameObject + " Loco.setCurrentSpeedToWalk()");
		if (Knowledge.IsLeader)
			attachCameraAndSetTarget();
	}
	
	protected virtual void animate_WalkBack()
	{
		#if DEBUG_LOG_ANIMATION		
		Log.jprint(gameObject.name +"    animate_WalkBack      "+ AnimationBlendingInfo.Instance._blend_walkback);
		#endif
		AnimCon.applyRootMotion(false);

		float aniPlayRate = Knowledge.AnimSpeed_WalkBack * MovementPlayRate;
		AnimCon.startAnimation(Knowledge.Anim_WalkBack, AnimationBlendingInfo.Instance._blend_walkback, aniPlayRate, 0);

		resetComboCurrent();

		Knowledge.endAim();
		
		//Knowledge.showWeapon(false);
		Loco.setCurrentSpeedToWalkBack();

		if (Knowledge.IsLeader)
			attachCameraAndSetTarget();
	}
	
	protected virtual void animate_WalkFast()
	{		
		#if DEBUG_LOG_ANIMATION
		Log.jprint(gameObject.name +"    animate_WalkFast      "+AnimationBlendingInfo.Instance._blend_walkfast);
		#endif
		AnimCon.applyRootMotion(false);

		float aniPlayRate = Knowledge.AnimSpeed_WalkFast * MovementPlayRate;
		AnimCon.startAnimation(Knowledge.Anim_WalkFast, AnimationBlendingInfo.Instance._blend_walkfast, aniPlayRate, 0);

		resetComboCurrent();

		_movementPlayRate_Prev = MovementPlayRate;

		Knowledge.endAim();
		
		//Knowledge.showWeapon(false);
		Loco.setCurrentSpeedToWalkFast();
		
		if (Knowledge.IsLeader)
			attachCameraAndSetTarget();
	}
	
	protected virtual void animate_Run()
	{		
		#if DEBUG_LOG_ANIMATION
		Log.jprint(gameObject.name +"     animate_Run       "+ AnimationBlendingInfo.Instance._blend_run);
		#endif

//		if (Knowledge.IsLeader)
//			Log.jprint(" leader run     leader run     leader run     leader run     leader run");

		AnimCon.applyRootMotion(false);

		float aniPlayRate = Knowledge.AnimSpeed_Run * MovementPlayRate;
		AnimCon.startAnimation(Knowledge.Anim_Run, AnimationBlendingInfo.Instance._blend_run, aniPlayRate, 0);

		resetComboCurrent();

		_movementPlayRate_Prev = MovementPlayRate;

		Knowledge.endAim();
		
		//Knowledge.showWeapon(false);
		Loco.setCurrentSpeedToRun();
		
		if (Knowledge.IsLeader)
			attachCameraAndSetTarget();
	}


	protected virtual void comboRootMotion()
	{
		AnimCon.applyRootMotion(true);

//		Debug.Log("comboRootMotion : " + Knowledge._attackType);
//		Debug.Log("TotalCombo : " + Knowledge.TotalCombo + " / current : " + (Knowledge.ComboCurrent + 1));

		//jsm - trail set by weapon type
		if (Knowledge.AttackType == eAttackType.AT_Sword)
		{
			Knowledge.setTrail(true);
		}

		//jsm_0921 - change leader slot pressed state
		if (BattleUI.Instance() != null && BattleUI.Instance().IsLeaderPressed)
		{
			BattleUI.Instance().leaderSlotPressState(false);
		}

		//jsm_0210 - 피버 이벤트를 버튼 발동 형식으로 변경.
//		if (BattleBase.Instance.AttackPoint >= CameraManager.Instance.MaxAttackPoint && !CameraManager.Instance.IsEventScene)
//		{
//			CameraManager.Instance.startEventScene();
//		}
	}

	protected float SkillAniPlayRate
	{
		get 
		{
			return Knowledge.calculate_SkillAnimationPlayRate();
		}
	}
	
	protected float MovementPlayRate
	{
		get 
		{
			return Knowledge.calculate_MovementPlayRate();
		}
	}
	

	protected virtual void animate_Block()
	{		
		#if DEBUG_LOG_ANIMATION
		Log.jprint(gameObject.name +"     animate_Block      "+ AnimationBlendingInfo.Instance._blend_block);
		#endif
		AnimCon.applyRootMotion(false);
		AnimCon.startAnimation(Knowledge.Anim_Block, AnimationBlendingInfo.Instance._blend_block, 1, 0);
		Knowledge.Progress_Block = true;
		Knowledge.ComboCurrent = -1;
		Knowledge._isResetActionInfoDone = false;

		//Knowledge.showWeapon(false);
		Loco.setCurrentSpeedToZero();
	}
	

	protected virtual void animate_Combo1()
	{		
		#if DEBUG_LOG_ANIMATION
		if (gameObject.name.Contains("Dummy") == false)
			Log.jprint(Time.time + "  " + gameObject.name +"   animate_Combo1      "+AnimationBlendingInfo.Instance._blend_combo1 + "    SkillAniPlayRate : "+SkillAniPlayRate);
		#endif

		Log.nprint(gameObject.name +" :   Combo1 ");

		comboRootMotion();
		//AnimCon.startAnimation(Knowledge.Anim_Combo1, 0, SkillAniPlayRate, 0);  //jks call "play()" instead of "crossfade()" by giving small blending time.
		AnimCon.startAnimation(Knowledge.Anim_Combo1, AnimationBlendingInfo.Instance._blend_combo1, SkillAniPlayRate, 0);  //jks call "play()" instead of "crossfade()" by giving small blending time.
		Knowledge.Progress_Anim_Combo1 = true;
		Knowledge.ComboCurrent = 1;
		Knowledge.ComboRecent = Knowledge.ComboCurrent;
		Knowledge._isResetActionInfoDone = false;

		//Knowledge.showWeapon(true);
		Loco.setCurrentSpeedToZero();
		//Log.jprint(gameObject.name +"animate_Combo1 - 8");

		
		if (Knowledge.IsLeader)
			attachCameraAndSetTarget();

	}
	
	protected virtual void animate_Combo2()
	{		
		#if DEBUG_LOG_ANIMATION
		if (gameObject.name.Contains("Dummy") == false)
			Log.jprint(Time.time + "  " + gameObject.name +"   animate_Combo2      "+AnimationBlendingInfo.Instance._blend_combo2 + "    SkillAniPlayRate : "+SkillAniPlayRate);
		#endif
		Log.nprint(gameObject.name +" :   Combo2 ");

		comboRootMotion();
		//AnimCon.startAnimation(Knowledge.Anim_Combo2, 0.1f, SkillAniPlayRate, 0);
		AnimCon.startAnimation(Knowledge.Anim_Combo2, AnimationBlendingInfo.Instance._blend_combo2, SkillAniPlayRate, 0);
		Knowledge.Progress_Anim_Combo2 = true;
		Knowledge.ComboCurrent = 2;
		Knowledge.ComboRecent = Knowledge.ComboCurrent;
		Knowledge._isResetActionInfoDone = false;

		//Knowledge.showWeapon(true);
		Loco.setCurrentSpeedToZero();
	}
	
	protected virtual void animate_Combo3()
	{		
		#if DEBUG_LOG_ANIMATION
		//if (gameObject.name.Contains("Dummy") == false)
			Log.jprint(Time.time + "  " + gameObject.name +"   animate_Combo3()     Knowledge.Anim_Combo3: "+ Knowledge.Anim_Combo3 + "    SkillAniPlayRate : "+SkillAniPlayRate);
		#endif
		Log.nprint(gameObject.name +" :   Combo3 ");
		
		comboRootMotion();
		//AnimCon.startAnimation(Knowledge.Anim_Combo3, 0.15f, SkillAniPlayRate, 0);
		AnimCon.startAnimation(Knowledge.Anim_Combo3, AnimationBlendingInfo.Instance._blend_combo3, SkillAniPlayRate, 0);
		Knowledge.Progress_Anim_Combo3 = true;
		Knowledge.ComboCurrent = 3;
		Knowledge.ComboRecent = Knowledge.ComboCurrent;
		Knowledge._isResetActionInfoDone = false;

		//Knowledge.showWeapon(true);
		Loco.setCurrentSpeedToZero();
	}

	protected virtual void animate_Combo4()
	{		
		#if DEBUG_LOG_ANIMATION
		////if (gameObject.name.Contains("C") == false)
			Log.jprint(Time.time + "  " + gameObject.name +"   animate_Combo4()     Knowledge.Anim_Combo4: "+ Knowledge.Anim_Combo4 + "    SkillAniPlayRate : "+SkillAniPlayRate);
		#endif
		Log.nprint(gameObject.name +" :   Combo4 ");
		
		comboRootMotion();
		//AnimCon.startAnimation(Knowledge.Anim_Combo4, 0.15f, SkillAniPlayRate, 0);
		AnimCon.startAnimation(Knowledge.Anim_Combo4, AnimationBlendingInfo.Instance._blend_combo4, SkillAniPlayRate, 0);
		Knowledge.Progress_Anim_Combo4 = true;
		Knowledge.ComboCurrent = 4;
		Knowledge.ComboRecent = Knowledge.ComboCurrent;
		Knowledge._isResetActionInfoDone = false;

		//Knowledge.showWeapon(true);
		Loco.setCurrentSpeedToZero();
	}
	
	
	protected virtual void animate_Combo5()
	{		
		#if DEBUG_LOG_ANIMATION
		//if (gameObject.name.Contains("C") == false)
			Log.jprint(Time.time + "  " + gameObject.name +"   animate_Combo5()     Knowledge.Anim_Combo5: "+ Knowledge.Anim_Combo5 + "    SkillAniPlayRate : "+SkillAniPlayRate);
		#endif
		Log.nprint(gameObject.name +" :   Combo5 ");
		
		comboRootMotion();
		//AnimCon.startAnimation(Knowledge.Anim_Combo5, 0.15f, SkillAniPlayRate, 0);
		AnimCon.startAnimation(Knowledge.Anim_Combo5, AnimationBlendingInfo.Instance._blend_combo5, SkillAniPlayRate, 0);
		Knowledge.Progress_Anim_Combo5 = true;
		Knowledge.ComboCurrent = 5;
		Knowledge.ComboRecent = Knowledge.ComboCurrent;
		Knowledge._isResetActionInfoDone = false;

		//Knowledge.showWeapon(true);
		Loco.setCurrentSpeedToZero();
	}
	
	
	protected virtual void animate_Combo6()
	{		
		#if DEBUG_LOG_ANIMATION
		//if (gameObject.name.Contains("C") == false)
			Log.jprint(Time.time + "  " + gameObject.name +"   animate_Combo6()     Knowledge.Anim_Combo6: "+ Knowledge.Anim_Combo6 + "    SkillAniPlayRate : "+SkillAniPlayRate);
		#endif
		Log.nprint(gameObject.name +" :   Combo6 ");
		
		comboRootMotion();
		//AnimCon.startAnimation(Knowledge.Anim_Combo6, 0.15f, SkillAniPlayRate, 0);
		AnimCon.startAnimation(Knowledge.Anim_Combo6, AnimationBlendingInfo.Instance._blend_combo6, SkillAniPlayRate, 0);
		Knowledge.Progress_Anim_Combo6 = true;
		Knowledge.ComboCurrent = 6;
		Knowledge.ComboRecent = Knowledge.ComboCurrent;
		Knowledge._isResetActionInfoDone = false;
		
		//Knowledge.showWeapon(true);
		Loco.setCurrentSpeedToZero();
	}
	
	
	
	protected virtual void animate_Combo7()
	{		
		#if DEBUG_LOG_ANIMATION
		//if (gameObject.name.Contains("C") == false)
			Log.jprint(Time.time + "  " + gameObject.name +"   animate_Combo7()     Knowledge.Anim_Combo7: "+ Knowledge.Anim_Combo7 + "    SkillAniPlayRate : "+SkillAniPlayRate);
		#endif
		Log.nprint(gameObject.name +" :   Combo7 ");
		
		comboRootMotion();
		//AnimCon.startAnimation(Knowledge.Anim_Combo7, 0.15f, SkillAniPlayRate, 0);
		AnimCon.startAnimation(Knowledge.Anim_Combo7, AnimationBlendingInfo.Instance._blend_combo7, SkillAniPlayRate, 0);
		Knowledge.Progress_Anim_Combo7 = true;
		Knowledge.ComboCurrent = 7;
		Knowledge.ComboRecent = Knowledge.ComboCurrent;
		Knowledge._isResetActionInfoDone = false;

		//Knowledge.showWeapon(true);
		Loco.setCurrentSpeedToZero();
	}
	
	
	protected virtual void animate_Combo8()
	{		
		#if DEBUG_LOG_ANIMATION
		//if (gameObject.name.Contains("C") == false)
			Log.jprint(Time.time + "  " + gameObject.name +"   animate_Combo8()     Knowledge.Anim_Combo8: "+ Knowledge.Anim_Combo8 + "    SkillAniPlayRate : "+SkillAniPlayRate);
		#endif
		Log.nprint(gameObject.name +" :   Combo8 ");
		
		comboRootMotion();
		//AnimCon.startAnimation(Knowledge.Anim_Combo8, 0.15f, SkillAniPlayRate, 0);
		AnimCon.startAnimation(Knowledge.Anim_Combo8, AnimationBlendingInfo.Instance._blend_combo8, SkillAniPlayRate, 0);
		Knowledge.Progress_Anim_Combo8 = true;
		Knowledge.ComboCurrent = 8;
		Knowledge.ComboRecent = Knowledge.ComboCurrent;
		Knowledge._isResetActionInfoDone = false;
		
		//Knowledge.showWeapon(true);
		Loco.setCurrentSpeedToZero();
	}

	
	

	

	
	

	protected virtual void animate_Hit()
	{		
		#if DEBUG_LOG_ANIMATION
		//Log.jprint(Time.time + "  " + gameObject.name +"  *  animate_Hit    "  + AnimationBlendingInfo.Instance._blend_hit);
		#endif
//		if (Knowledge.HitType == eHitType.HT_CRITICAL)
//		{
//			AnimCon.applyRootMotion(true);
//		}
//		else
//		{
//			AnimCon.applyRootMotion(true);
//		}

		AnimCon.applyRootMotion(Knowledge.IsLastDamageInSkill || Knowledge.RecentHitType == eHitType.HT_CRITICAL);

		//Log.jprint(gameObject + "         animate_Hit()  anim name hash: "+ Knowledge.animHitReaction());

		AnimCon.startAnimation(Knowledge.animHitReaction(), AnimationBlendingInfo.Instance._blend_hit, 1, 0);  // crossfade()
		resetComboCurrent();
		Knowledge.Progress_HitReaction = true;

		Knowledge._isResetActionInfoDone = false;

		Knowledge.endAim();

		//jks do not hide weapon if hit -- Knowledge.showWeapon(false);
		Loco.setCurrentSpeedToZero();
	}



	protected virtual void animate_Death()
	{		
		#if DEBUG_LOG_ANIMATION
		Log.jprint(gameObject.name +"animate_Death    "  + AnimationBlendingInfo.Instance._blend_death );
		#endif
		AnimCon.applyRootMotion(true);
		AnimCon.startAnimation(Knowledge.Anim_Death, AnimationBlendingInfo.Instance._blend_death, 1, 0);
		Knowledge.Progress_Death = true;

		Knowledge.endAim();
		//Knowledge.showWeapon(false);
		Loco.setCurrentSpeedToZero();

		//jks 2016.6.14 code moved from Knowledge_Mortal_Fighter.cs to fix Mantis 1572
		if (Knowledge.Action_Captured_InTheAir) //jks 공중스턴 당하다가 죽으면 지면으로 내림.
			Knowledge.resetActionCapturedInTheAir();
		
	}



	protected virtual void animate_DeathB()
	{		
		AnimCon.applyRootMotion(true);
		AnimCon.startAnimation(Knowledge.Anim_DeathB, AnimationBlendingInfo.Instance._blend_death, 1, 0);
		Knowledge.Progress_DeathB = true;

		Knowledge.endAim();
		//Knowledge.showWeapon(false);
		Loco.setCurrentSpeedToZero();
	}


	protected virtual void animate_Pause()
	{		
		#if DEBUG_LOG_ANIMATION
		if (gameObject.name.Contains("Dummy") == false)
		#endif
		
		AnimCon.applyRootMotion(false);
		AnimCon.startAnimation(Knowledge.Anim_Pause, AnimationBlendingInfo.Instance._blend_idle, 1, 0);
		resetComboCurrent();
		
		Knowledge.endAim();
		//Knowledge.showWeapon(false);
		Loco.setCurrentSpeedToZero();
	}


	protected virtual void animate_StagingPause()
	{		
		#if DEBUG_LOG_ANIMATION
			Log.jprint(gameObject.name +"********** animate_StagingPause    " + 0 );
		#endif
		
		AnimCon.applyRootMotion(false);
		AnimCon.startAnimation(Knowledge.Anim_Idle, 0, 1, 0); //jks - blending time = 0 to for staging
		resetComboCurrent();
		
		Knowledge.endAim();
		//Knowledge.showWeapon(false);
		Loco.setCurrentSpeedToZero();
	}

	
	protected virtual void animate_Idle()
	{		
		#if DEBUG_LOG_ANIMATION
		if (gameObject.name.Contains("Dummy") == false)
			Log.jprint(gameObject.name +"********** animate_Idle    " + AnimationBlendingInfo.Instance._blend_idle );
		#endif
		
		AnimCon.applyRootMotion(false);
		AnimCon.startAnimation(Knowledge.Anim_Idle, AnimationBlendingInfo.Instance._blend_idle, 1, 0); //jks Play() to avoid skill preview block by fast multiple touch.
		resetComboCurrent();

		Knowledge.endAim();
		//Knowledge.showWeapon(false);
		Loco.setCurrentSpeedToZero();


		//jks in case if cooling jump action finish event is skipped.
		if (Knowledge.Progress_CoolingJump)
			Knowledge.resetActionInfo();
		
		if (Knowledge.IsLeader)
			attachCameraAndSetTarget();
	}

	protected virtual void animate_PvpReady()
	{		
		#if DEBUG_LOG_ANIMATION
		Log.jprint(Time.time +" : "+ gameObject.name +"********** animate_PvpReady");
		#endif
		
		AnimCon.applyRootMotion(false);
		AnimCon.startAnimation(Knowledge.Anim_PvpReady, 0.1f, 1, 0); 
		resetComboCurrent();
		
		Knowledge.endAim();
		//Knowledge.showWeapon(false);
		Loco.setCurrentSpeedToZero();
	}


	protected virtual void animate_PvpJustBeforeHit()
	{		
		#if DEBUG_LOG_ANIMATION
		Log.jprint(Time.time +" : "+ gameObject.name +"********** animate_PvpJustBeforeHit");
		#endif
		
		AnimCon.applyRootMotion(false);
		AnimCon.startAnimation(Knowledge.Anim_PvpJustBeforeHit, 0.1f, 1, 0); 
		resetComboCurrent();
		
		Knowledge.endAim();
		//Knowledge.showWeapon(false);
		Loco.setCurrentSpeedToZero();
	}
	

	protected virtual void animate_InstallWeapon()
	{		
		#if DEBUG_LOG_ANIMATION
		Log.jprint(Time.time +" : "+ gameObject.name +"********** animate_InstallWeapon    ");
		#endif
		
		AnimCon.applyRootMotion(false);

		AnimCon.startAnimation(Knowledge.Anim_InstallWeapon, 0.1f, 1, 0); 
//jks 2015.11.4 무기장착 연출 제거.		Knowledge.startAnimationEventDoubleCheck_InstallWeapon();

		resetComboCurrent();
		Knowledge._isResetActionInfoDone = false;

		Knowledge.endAim();
		//Knowledge.showWeapon(false);
		Loco.setCurrentSpeedToZero();
	}


	protected virtual void animate_InstallWeapon_Pre()
	{		
		#if DEBUG_LOG_ANIMATION
		Log.jprint(Time.time +" : "+ gameObject.name +"********** animate_InstallWeapon_Pre    ");
		#endif
		
		AnimCon.applyRootMotion(false);
		AnimCon.startAnimation(Knowledge.Anim_InstallWeapon_Pre, 0.1f, 1, 0);
		resetComboCurrent();
		Knowledge._isResetActionInfoDone = false;
		
		Knowledge.endAim();
		//Knowledge.showWeapon(false);
		Loco.setCurrentSpeedToZero();

		Invoke("showLeader", 0.05f);
	}
	
	void showLeader()
	{
		BattleBase.Instance.showLeader(true);
		if (BattleTuning.Instance._useWeaponAction)
			Knowledge.showWeapon(false);
	}



	protected virtual void animate_Captured_InTheAir()
	{		
		#if DEBUG_LOG_ANIMATION
		Log.jprint(gameObject+"********** animate_Stun   " + AnimationBlendingInfo.Instance._blend_stun);
		#endif

		AnimCon.applyRootMotion(false);
		AnimCon.startAnimation(Knowledge.Anim_Captured_InTheAir, 0.2f, 1, 0); 
		resetComboCurrent();

		Knowledge.endAim();
		//Knowledge.showWeapon(false);
		Loco.setCurrentSpeedToZero();
	}
		

	protected virtual void animate_Captured()
	{		
		#if DEBUG_LOG_ANIMATION
		Log.jprint(gameObject.name +"********** animate_Stun   " + AnimationBlendingInfo.Instance._blend_stun);
		#endif
		
		AnimCon.applyRootMotion(false);
		AnimCon.startAnimation(Knowledge.Anim_Captured, 0.2f, 1, 0); 
		resetComboCurrent();
		
		Knowledge.endAim();
		//Knowledge.showWeapon(false);
		Loco.setCurrentSpeedToZero();
	}
	

	protected virtual void animate_Stun()
	{		
		#if DEBUG_LOG_ANIMATION
		Log.jprint(gameObject.name +"********** animate_Stun   " + AnimationBlendingInfo.Instance._blend_stun);
		#endif
		
		AnimCon.applyRootMotion(false);
		AnimCon.startAnimation(Knowledge.Anim_Stun, AnimationBlendingInfo.Instance._blend_stun, 1, 0); 
		resetComboCurrent();
		
		Knowledge.endAim();
		//Knowledge.showWeapon(false);
		Loco.setCurrentSpeedToZero();
	}

	protected virtual void animate_Exhausted()
	{		
		#if DEBUG_LOG_ANIMATION
		Log.jprint(gameObject.name +"     animate_Exhausted");
		#endif
		AnimCon.applyRootMotion(false);
		AnimCon.startAnimation(Knowledge.Anim_Exhausted, 0.1f, 1, 0);
		resetComboCurrent();
		
		Knowledge.endAim();
		//Knowledge.showWeapon(false);
		Loco.setCurrentSpeedToZero();

		if (Knowledge.IsLeader)
			attachCameraAndSetTarget();
	}


	protected virtual void animate_CoolingJump()
	{
		#if DEBUG_LOG_ANIMATION
		if (gameObject.name.Contains("C"))
			Log.jprint(gameObject.name +"    animate_CoolingJump  " + AnimationBlendingInfo.Instance._blend_coolingjump);
		#endif

		AnimCon.applyRootMotion(true);
		AnimCon.startAnimation(Knowledge.Anim_CoolingJump, AnimationBlendingInfo.Instance._blend_coolingjump,  MovementPlayRate, 0);
		resetComboCurrent();

		Knowledge.Progress_CoolingJump = true;
		Knowledge._isResetActionInfoDone = false;

		Knowledge.endAim();
		//Knowledge.showWeapon(false);
		Loco.setCurrentSpeedToZero();
	}


//	protected virtual void scanEnemy()
//	{
//		//Log.jprint(gameObject + "   Scan enemy ");
//		if (!Knowledge.canUseSkill()) return;
//		
//		bool found = Knowledge.checkEnemyOnPath(false);
//		if (found)
//		{
//			Knowledge.startSkill();
//		}
//		else if (Knowledge.Progress_Skill)
//		{
//			//Log.jprint(gameObject + "Enemy not found  . . . . . . . . ");
//			if (!_isDelayedInvoked_cancelAttackIfNoEnemy)
//			{
//				Invoke("cancelAttackIfNoEnemy", 4.0f);
//				_isDelayedInvoked_cancelAttackIfNoEnemy = true;
//			}
//		}
//	}

	protected virtual void scanEnemy()
	{
		//Log.jprint(gameObject + "Behavior_Mortal_Fighter :: ScanEnemy() ");
		if (!Knowledge.canUseSkill()) return;
		
		bool found = Knowledge.checkEnemyOnPath(false);
		if (found)
		{
			StartCoroutine( Knowledge.startSkill() );
		}
	}

	
	protected virtual bool cancelAttackIfNoEnemy()
	{
		bool found = Knowledge.checkEnemyOnPath(false);
		if (!found)
		{
			forceWalk();
			Knowledge.endAim();
		}

		_isDelayedInvoked_cancelAttackIfNoEnemy = false;
		
		return !found;  //jks is canceled?
	}

	public void CancelInvoke_cancelAttackIfNoEnemy()
	{
		CancelInvoke("cancelAttackIfNoEnemy");
		_isDelayedInvoked_cancelAttackIfNoEnemy = false;
	}

	public void CancelAllInvokes()
	{
		CancelInvoke();
		_isDelayedInvoked_cancelAttackIfNoEnemy = false;
	}


//	protected virtual void setCameraTarget(CameraManager.TargetState state)
//	{
//	}

//	protected virtual void setCombo2AdditionalProcess()
//	{
//	}

	

	//jks ignore hit reaction and all other actions, just run now.
	public void forceRun()
	{
		Knowledge.forceResetFlags();
		Knowledge.Action_Run = true;
	}
	
	public void forceWalk()
	{
		Knowledge.forceResetFlags();
		Knowledge.Action_Walk = true;
		//Log.jprint  (gameObject + "  forceWalk()");
	}
	
	public void forceWalkBack()
	{
		Knowledge.forceResetFlags();
		Knowledge.Action_WalkBackTurn = true;
		//Log.jprint(gameObject.name +"9999999  WalkBack !!!");
		//Log.jprint  (gameObject + "  forceWalkFast()");
	}
	
	public void forceWalkFast()
	{
		Knowledge.forceResetFlags();
		Knowledge.Action_WalkFast = true;
		//Log.jprint  (gameObject + "  forceWalkFast()");
	}
	
	public void forceIdle()
	{
		Knowledge.forceResetFlags();
		Knowledge.Action_Idle = true;
		//Log.jprint  (gameObject + "  forceIdle()");
	}




	#region REACT (BEHAVIOR TREE)


	public virtual Action postVictory()  //jks for idle after victory action
	{
		#if DEBUG_LOG		
		if (gameObject.name.Contains("Dummy") == false)
			Log.jprint(gameObject + "  V I S I T : Action postVictory()");
		#endif
//jks		if (Knowledge.Action_PostVictory)
//		{
//			#if DEBUG_LOG		
//			Log.jprint (gameObject + "   postVictory");
//			#endif
//			if (!AnimCon.isAnimPlaying(Knowledge.Anim_Idle))
//			{
//				animate_Idle();
//			}
//			
//			yield return React.NodeResult.Success;
//			
//		}
		yield return React.NodeResult.Failure;
	}



	public override Action battleVictory()
	{
		#if DEBUG_LOG		
		if (gameObject.name.Contains("Dummy") == false)
			Log.jprint(gameObject + "  V I S I T : Action battleVictory()");
		#endif
		if (Knowledge.IsBattleVictorious && Knowledge.AllyID == eAllyID.Ally_Human_Me)
		{
			#if DEBUG_LOG		
			Log.jprint (gameObject + "   battleVictory");
			#endif
			if (!AnimCon.isAnimPlaying(Knowledge.Anim_Victory))
			{
				//Log.jprint (gameObject + "    call Anim_Victory");
				animate_Victory();
			}
			
			yield return React.NodeResult.Success;
			
		}
		yield return React.NodeResult.Failure;
	}


	public override Action battlePaused()
	{
		#if DEBUG_LOG		
		if (gameObject.name.Contains("Dummy") == false)
			Log.jprint(gameObject + "  V I S I T : Action battlePaused()");
		#endif
		if (Knowledge.IsBattlePaused || Knowledge.Action_Paused ||
			BattleBase.Instance.ProgressBossDialog2)
		{
			#if DEBUG_LOG		
			Log.jprint (gameObject + "   battlePaused");
			#endif
			if (!AnimCon.isAnimPlaying(Knowledge.Anim_Idle))
			{
				animate_StagingPause();
			}

			yield return React.NodeResult.Success;
			
		}
		yield return React.NodeResult.Failure;
	}
	

	public override Action battleFailure()
	{
		#if DEBUG_LOG		
		if (gameObject.name.Contains("Dummy") == false)
			Log.jprint(gameObject + "  V I S I T : Action battleFailure()");
		#endif
		if (Knowledge.IsBattleFailed)
		{
			#if DEBUG_LOG		
			Log.jprint (gameObject + "   battleFailure");
			#endif

			if (Knowledge.AllyID == eAllyID.Ally_Human_Me)  //jks time out 이나 친구 캐릭터가 남아 있고 패배 할 때 취할 애니메이션.
			{
				if (!AnimCon.isAnimPlaying(Knowledge.Anim_Exhausted))
				{
					//Log.jprint (gameObject + "    call animate_Run");
					animate_Exhausted();
				}
				
				yield return React.NodeResult.Success;
			}
			else
			{
				if (!AnimCon.isAnimPlaying(Knowledge.Anim_Victory))
				{
					//Log.jprint (gameObject + "    call animate_Run");
					animate_Victory();
				}
				
				yield return React.NodeResult.Success;
			}
				
			
		}
		yield return React.NodeResult.Failure;
	}

	//jks “신수 전략”은 더 이상 사용 안함. 
//	public virtual Action sinsuRefillMode()
//	{
//		#if DEBUG_LOG		
//		if (gameObject.name.Contains("Dummy") == false)
//			Log.jprint(gameObject + "  V I S I T : Action sinsuRefillMode()");
//		#endif
//		if (BattleBase.Instance.IsSinsuRefillMode)
//		{
//			#if DEBUG_LOG		
//			Log.jprint (gameObject + "   IsSinsuRefillMode");
//			#endif
//			if (!AnimCon.isAnimPlaying(Knowledge.Anim_Idle))
//			{
//				//Log.jprint (gameObject + "    call animate_Run");
//				Knowledge.forceResetFlags();
//				animate_Idle();
//			}
//			
//			yield return React.NodeResult.Success;
//			
//		}
//		yield return React.NodeResult.Failure;
//	}



	public virtual Action stagingMode() //jks for boss dialog, ..
	{
		#if DEBUG_LOG		
		if (gameObject.name.Contains("Dummy") == false)
			Log.jprint(gameObject + "  V I S I T : Action stagingMode()");
		#endif
		if (Knowledge.IsBossDialogStagingMode)
		{
			#if DEBUG_LOG		
			Log.jprint (gameObject + "   stagingMode");
			#endif
			if (!AnimCon.isAnimPlaying(Knowledge.Anim_Idle))
			{
				//Log.jprint (gameObject + "    call animate_Run");
				Knowledge.forceResetFlags();
				animate_Idle();
			}
			
			yield return React.NodeResult.Success;
			
		}
		yield return React.NodeResult.Failure;
	}


	public virtual Action coolingJump()
	{
		yield return React.NodeResult.Failure;
	}


	public override Action walk()
	{
		#if DEBUG_LOG		
		if (gameObject.name.Contains("Dummy") == false)
			Log.jprint(gameObject + "  V I S I T : Action walk()");
		#endif
		//Log.jprint (gameObject + "   Knowledge.Action_Walk: "+ Knowledge.Action_Walk);
		if (Knowledge.Action_Walk && !Knowledge.Progress_SkillAnimation && !Knowledge.isCoolingInProgress())  //jks - let attack finish it's action before start move.
		{			
			#if DEBUG_LOG		
			Log.jprint (gameObject + "   walk");
			#endif

			if (AnimCon == null)
			{
				Log.jprint(gameObject + "   AnimCon == null");
			}
			if (Knowledge == null)
			{
				Log.jprint(gameObject + "   Knowledge == null");
			}
			
			if (!AnimCon.isAnimPlaying(Knowledge.Anim_Walk) || MovementPlayRate != _movementPlayRate_Prev)
			{
				animate_Walk();
				//150803_jsm - 카메라 중심점 타겟팅 방식 자동으로 변경
//				setCameraTarget(CameraManager.TargetState.Walk);
			}
			
			//scanEnemy();
			
			yield return React.NodeResult.Success;
		}
		
		yield return React.NodeResult.Failure;
	}	
	
	
	public override Action walkFast()
	{
		#if DEBUG_LOG		
		if (gameObject.name.Contains("Dummy") == false)
			Log.jprint(gameObject + "  V I S I T : Action walkFast()");
		#endif
		if (Knowledge.Action_WalkFast && !Knowledge.Progress_SkillAnimation) 
		{			
			#if DEBUG_LOG		
			Log.jprint (gameObject + "   walkFast");
			#endif
			
			if (!AnimCon.isAnimPlaying(Knowledge.Anim_WalkFast) || MovementPlayRate != _movementPlayRate_Prev)
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


	protected float _movementPlayRate_Prev = 1; //jks to check if slow skill finishied while opponents' move animation already played with slow skill rate.
	
	public override Action run()
	{
		#if DEBUG_LOG		
		if (gameObject.name.Contains("Dummy") == false)
			Log.jprint(gameObject + "  V I S I T : Action run()");
		#endif
		if (Knowledge.shouldIRun() && !Knowledge.Progress_SkillAnimation && !Knowledge.isCoolingInProgress()) 
		{			
			#if DEBUG_LOG		
			if (gameObject.name.Contains("Dummy") == false)
				Log.jprint (gameObject + "    run");
			#endif
			
			if (!AnimCon.isAnimPlaying(Knowledge.Anim_Run) || MovementPlayRate != _movementPlayRate_Prev)
			{
				//Log.jprint (gameObject + "    call animate_Run");
				animate_Run();
				//				if (! BattleBase.Instance.isSkillPreviewMode())
//					setCameraTarget(CameraManager.TargetState.Walk);
			}
			
			//scanEnemy();
			
			yield return React.NodeResult.Success;
		}
		
		yield return React.NodeResult.Failure;
	}	



	public virtual Action Block()
	{
		#if DEBUG_LOG		
		if (gameObject.name.Contains("Dummy") == false)
			Log.jprint(gameObject + "  V I S I T : Action Block()");
		#endif
		if (Knowledge.Action_Block) 
		{	
			if (!Knowledge.Progress_Block)
			{			
				animate_Block();
			}
			
			yield return React.NodeResult.Success;
		}
		yield return React.NodeResult.Failure;
	}	
	
	

	public virtual Action combo1()
	{
		#if DEBUG_LOG		
		if (gameObject.name.Contains("Dummy") == false)
			Log.jprint(gameObject + "  V I S I T : Action combo1()");
		#endif
		if (Knowledge.Action_Combo1) 
		{			
			if (!Knowledge.Progress_Anim_Combo1)
			{
//jks - 전투 중 캐릭터 멈추는 현상. -->				if (! AnimCon.isAnimPlaying(Knowledge.Anim_Combo1))
				{
//					if (Knowledge.IsLeader)
//						Log.jprint(Time.time + "    animate_Combo1();    " + gameObject);
					animate_Combo1();
				}
			}

			#if DEBUG_LOG		
			if (gameObject.name.Contains("Dummy") == false)
				Log.jprint(gameObject + "   Action combo1()");
			#endif
			yield return React.NodeResult.Success;
		}
		
		yield return React.NodeResult.Failure;
	}	
	

	public virtual Action combo2()
	{
		#if DEBUG_LOG		
		if (gameObject.name.Contains("Dummy") == false)
			Log.jprint(gameObject + "  V I S I T : Action combo2()");
		#endif
		if (Knowledge.Action_Combo2) 
		{			
			if (!Knowledge.Progress_Anim_Combo2)
			{
//jks - 전투 중 캐릭터 멈추는 현상. -->				if (! AnimCon.isAnimPlaying(Knowledge.Anim_Combo2))
				{
					animate_Combo2();
				}
			}
			
			#if DEBUG_LOG		
			if (gameObject.name.Contains("Dummy") == false)
				Log.jprint(gameObject + "   Action combo2()");
			#endif
			yield return React.NodeResult.Success;
		}
		
		yield return React.NodeResult.Failure;
	}	
	
	
	public virtual Action combo3()
	{
		#if DEBUG_LOG		
		if (gameObject.name.Contains("Dummy") == false)
			Log.jprint(gameObject + "  V I S I T : Action combo3()");
		#endif
		if (Knowledge.Action_Combo3) 
		{			
			if (!Knowledge.Progress_Anim_Combo3)
			{
//jks - 전투 중 캐릭터 멈추는 현상. -->				if (! AnimCon.isAnimPlaying(Knowledge.Anim_Combo3))
				{
					animate_Combo3();
				}
			}
			
			#if DEBUG_LOG		
			if (gameObject.name.Contains("Dummy") == false)
				Log.jprint(gameObject + "   Action combo3()");
			#endif
			yield return React.NodeResult.Success;
		}
		
		yield return React.NodeResult.Failure;
	}	
	
	
	public virtual Action combo4()
	{
		#if DEBUG_LOG		
		if (gameObject.name.Contains("Dummy") == false)
			Log.jprint(gameObject + "  V I S I T : Action combo4()");
		#endif
		if (Knowledge.Action_Combo4) 
		{			
			if (!Knowledge.Progress_Anim_Combo4)
			{
//jks - 전투 중 캐릭터 멈추는 현상. -->				if (! AnimCon.isAnimPlaying(Knowledge.Anim_Combo4))
				{
					animate_Combo4();
				}
			}
			
			#if DEBUG_LOG		
			if (gameObject.name.Contains("Dummy") == false)
				Log.jprint(gameObject + "   Action combo4()");
			#endif
			yield return React.NodeResult.Success;
		}
		
		yield return React.NodeResult.Failure;
	}	
	
	
	
	public virtual Action combo5()
	{
		#if DEBUG_LOG		
		if (gameObject.name.Contains("Dummy") == false)
			Log.jprint(gameObject + "  V I S I T : Action combo5()");
		#endif
		if (Knowledge.Action_Combo5) 
		{			
			if (!Knowledge.Progress_Anim_Combo5)
			{
//jks - 전투 중 캐릭터 멈추는 현상. -->				if (! AnimCon.isAnimPlaying(Knowledge.Anim_Combo5))
				{
					animate_Combo5();
				}
			}
			
			#if DEBUG_LOG		
			if (gameObject.name.Contains("Dummy") == false)
				Log.jprint(gameObject + "   Action combo5()");
			#endif
			yield return React.NodeResult.Success;
		}
		
		yield return React.NodeResult.Failure;
	}	
	
	
	
	public virtual Action combo6()
	{
		#if DEBUG_LOG		
		if (gameObject.name.Contains("Dummy") == false)
			Log.jprint(gameObject + "  V I S I T : Action combo6()");
		#endif
		if (Knowledge.Action_Combo6) 
		{			
			if (!Knowledge.Progress_Anim_Combo6)
			{
//jks - 전투 중 캐릭터 멈추는 현상. -->				if (! AnimCon.isAnimPlaying(Knowledge.Anim_Combo6))
				{
					animate_Combo6();
				}
			}
			
			#if DEBUG_LOG		
			if (gameObject.name.Contains("Dummy") == false)
				Log.jprint(gameObject + "   Action combo6()");
			#endif
			yield return React.NodeResult.Success;
		}
		
		yield return React.NodeResult.Failure;
	}	


	public virtual Action combo7()
	{
		#if DEBUG_LOG		
		if (gameObject.name.Contains("Dummy") == false)
			Log.jprint(gameObject + "  V I S I T : Action combo7()");
		#endif
		if (Knowledge.Action_Combo7) 
		{			
			if (!Knowledge.Progress_Anim_Combo7)
			{
//jks - 전투 중 캐릭터 멈추는 현상. -->				if (! AnimCon.isAnimPlaying(Knowledge.Anim_Combo7))
				{
					animate_Combo7();
				}
			}
			
			#if DEBUG_LOG		
			if (gameObject.name.Contains("Dummy") == false)
				Log.jprint(gameObject + "   Action combo7()");
			#endif
			yield return React.NodeResult.Success;
		}
		
		yield return React.NodeResult.Failure;
	}	


	public virtual Action combo8()
	{
		#if DEBUG_LOG		
		if (gameObject.name.Contains("Dummy") == false)
			Log.jprint(gameObject + "  V I S I T : Action combo8()");
		#endif
		if (Knowledge.Action_Combo8) 
		{			
			if (!Knowledge.Progress_Anim_Combo8)
			{
//jks - 전투 중 캐릭터 멈추는 현상. -->				if (! AnimCon.isAnimPlaying(Knowledge.Anim_Combo8))
				{
					animate_Combo8();
				}
			}
			
			#if DEBUG_LOG		
			if (gameObject.name.Contains("Dummy") == false)
				Log.jprint(gameObject + "   Action combo8()");
			#endif
			yield return React.NodeResult.Success;
		}
		
		yield return React.NodeResult.Failure;
	}	
		



	public virtual bool capturedInTheAir()
	{
		if (Knowledge.Action_Captured_InTheAir)
		{
			if (!AnimCon.isAnimPlaying(Knowledge.Anim_Captured_InTheAir))
			{
				animate_Captured_InTheAir();
			}
			return true;
		}
		return false;
	}


	public virtual bool captured()
	{
		#if DEBUG_LOG		
		if (gameObject.name.Contains("Dummy") == false)
			Log.jprint(gameObject + "  V I S I T : Action_Captured()");
		#endif
		if (Knowledge.Action_Captured)
		{
			if (!AnimCon.isAnimPlaying(Knowledge.Anim_Captured))
			{
				animate_Captured();
			}
			return true;
		}
		return false;
	}


	public virtual bool stun()
	{
		#if DEBUG_LOG		
		if (gameObject.name.Contains("Dummy") == false)
			Log.jprint(gameObject + "  V I S I T : Action stun()");
		#endif
		if (Knowledge.Action_Stun)
		{
			if (!AnimCon.isAnimPlaying(Knowledge.Anim_Stun))
			{
				animate_Stun();
			}
			return true;
		}
		return false;
	}


	public virtual bool pvpReady()
	{
		if (Knowledge.Action_PvpReady)
		{
			if (!AnimCon.isAnimPlaying(Knowledge.Anim_PvpReady))
			{
				animate_PvpReady();
			}
			return true;
		}
		return false;
	}
	
	//jks  Pvp  에서 맞을 차례가 된 캐릭터가 탁격 받기 전 취할 동작.
	public virtual bool pvpJustBeforeHit()
	{
		if (Knowledge.Action_PvpJustBeforeHit)
		{
			if (!AnimCon.isAnimPlaying(Knowledge.Anim_PvpJustBeforeHit))
			{
				animate_PvpJustBeforeHit();
			}
			return true;
		}
		return false;
	}
	
	
	public virtual bool installWeapon_Pre()
	{
		#if DEBUG_LOG		
		if (gameObject.name.Contains("Dummy") == false)
			Log.jprint(gameObject + "  V I S I T : Action installWeapon()");
		#endif
		if (Knowledge.Action_InstallWeapon_Pre)
		{
			if (!AnimCon.isAnimPlaying(Knowledge.Anim_InstallWeapon_Pre))
			{
				animate_InstallWeapon_Pre();
			}
			return true;
		}
		return false;
	}
	
	
	public virtual bool installWeapon()
	{
		#if DEBUG_LOG		
		if (gameObject.name.Contains("Dummy") == false)
			Log.jprint(gameObject + "  V I S I T : Action installWeapon()");
		#endif
		if (Knowledge.Action_InstallWeapon)
		{
			if (!AnimCon.isAnimPlaying(Knowledge.Anim_InstallWeapon))
			{
				animate_InstallWeapon();
			}
			return true;
		}
		return false;
	}
	
	
	public override Action hit()
	{
		#if DEBUG_LOG		
		if (gameObject.name.Contains("Dummy") == false)
			Log.jprint(gameObject + "  V I S I T : Action hit()");
		#endif
		if (Knowledge.Action_Hit) 
		{	
			//#if DEBUG_LOG		
			//Log.jprint(Time.time +" : "+ gameObject + " Action hit() ");
			//#endif
			if (!Knowledge.Progress_HitReaction)
			{			
//jks this functionality moved in "takeDamage()"..				Vector3 forceDirection = -transform.forward;
//				Loco.pushObject(Knowledge.getReactionDistance(), forceDirection, 0.15f);	
				//Log.jprint(gameObject + "AAAAAAAAAA no play it now - " + Knowledge.animHitReaction());
				animate_Hit();
			}
			
			yield return React.NodeResult.Success;
		}
		yield return React.NodeResult.Failure;
	}	
	


	public override Action death()
	{
		if (Knowledge.Action_Death) 
		{		
			if (Knowledge.Progress_Death)
			{
				if (!AnimCon.isAnimPlaying(Knowledge.Anim_Death))
				{
					//if (Knowledge.HitType != eHitType.HT_CRITICAL) //jks if do not play death if i m already down by critical damage.
					{
						animate_Death();
					}
				}
			}
			else
			{
				#if DEBUG_LOG		
				Log.jprint(gameObject + "   death");
				#endif
				
				if (!AnimCon.isAnimPlaying(Knowledge.Anim_Death))
				{
					Knowledge.Progress_Death = true;
					processDeath();

					//if (Knowledge.HitType != eHitType.HT_CRITICAL) //jks if do not play death if i m already down by critical damage.
					{
						animate_Death();
					}
				}
			}
			
			yield return React.NodeResult.Success;
		}
		yield return React.NodeResult.Failure;
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
			
			if (!AnimCon.isAnimPlaying(Knowledge.Anim_Idle))
			{
				animate_Idle();
			}
			
			yield return React.NodeResult.Success;
		}
		yield return React.NodeResult.Failure;
	}	



	public virtual Action cooling()
	{
		if (Knowledge.AttackCoolTimer.IsCoolingInProgress)
		{
			#if DEBUG_LOG		
			Log.jprint (gameObject + "   cooling");
			#endif
			if (!AnimCon.isAnimPlaying(Knowledge.Anim_Exhausted))
			{
				Knowledge.forceResetFlags();
				animate_Exhausted();
			}
			
			yield return React.NodeResult.Success;
			
		}
		
		Knowledge.CoolingJumpDone = false; //jks reset
		
		yield return React.NodeResult.Failure;
	}



	#endregion

}
