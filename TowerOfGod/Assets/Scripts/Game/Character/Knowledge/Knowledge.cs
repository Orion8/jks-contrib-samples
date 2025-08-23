using UnityEngine;
using System.Collections;


//jks agent's knowledge : Behavior Tree of the agent will make decision by evaluating agent knowledge.


public class Knowledge : MonoInit
{	
	//jks base knowledge of agents.
	public enum eBattleStatus { BATTLE_Pause, BATTLE_Pause_ActionOnly, BATTLE_InProgress, BATTLE_Victory, BATTLE_Failure };
	private static eBattleStatus _questState = eBattleStatus.BATTLE_InProgress;

	protected AnimationControl _animControl = null;

	protected static bool _bossDialogStagingMode = false;

	protected static bool _isBattleTimeStarted = false;
	
	public virtual bool IsBattleTimeStarted
	{ 
		get { return _isBattleTimeStarted; }
	}


	public AnimationControl AnimCon
	{
		get
		{
			if (_animControl == null)
			{
				_animControl = GetComponent<AnimationControl>();
			}
			return (AnimationControl)_animControl;
		}
	}
	

	public bool IsBattleVictorious
	{
		get 
		{ 
			return _questState == eBattleStatus.BATTLE_Victory;
		}
	}
	public bool IsBattleFailed
	{
		get 
		{ 
			return _questState == eBattleStatus.BATTLE_Failure;
		}
	}
	public bool IsBattleEnd
	{
		get
		{
			return IsBattleVictorious || IsBattleFailed;
		}
	}
	public bool IsBattlePaused
	{
		get 
		{ 
			return _questState == eBattleStatus.BATTLE_Pause;
		}
	}
	public bool IsBattleInProgress
	{
		get 
		{ 
			return _questState == eBattleStatus.BATTLE_InProgress;
		}
	}
	
	public bool IsBattlePaused_ActionOnly
	{
		get 
		{ 
			return _questState == eBattleStatus.BATTLE_Pause_ActionOnly;
		}
	}
	
	public bool IsBattleFinishedOrPaused
	{
		get
		{
			return IsBattleVictorious || IsBattleFailed || IsBattlePaused ;
		}
	}
	public bool IsBossDialogStagingMode
	{
		set { _bossDialogStagingMode = value; }
		get { return _bossDialogStagingMode; }
	}

	
	public eBattleStatus GetBattleState()
	{
		return _questState;
	}
	public virtual void SetBattleState(eBattleStatus qs)
	{
		_questState = qs;
	}





//	//jks - called from animation event


	public void animEvent_LockOpponent()
	{
		holdOpponent();
	}

	public void animEvent_UnlockOpponent()
	{
		releaseOpponent();
	}


	//jsm_0827
	public void animEvent_Give_Damage(float reactionDistance)  //jks 2014.9.3 - added reaction distance to set hit distance per hit. will be ignored if 0.
	{
//		if (gameObject.name.Contains("C"))
			Log.jprint(Time.time+ "    "+ gameObject.name +   "     E V E N T    D A M A G E.   ");

		giveDamage(reactionDistance);
	}

	public void animEvent_Give_Damage_QuickSkill(float reactionDistance) 
	{
		if (BattleTuning.Instance._showQuickSkillLog)
			Log.print(Time.time +  "   !!!!!   평타  Give_Damage 이벤트 ");
		giveDamage_QuickSkill(reactionDistance);
	}



	public bool _isResetActionInfoDone = false;
	//jsm_0827
	public void animEvent_Action_Finished()
	{
		Log.jprint(Time.time + "    "+ gameObject.name +"     E V E N T      F I N I S H I E D.   <<<<<<<<<  ");
		
		if (!_isResetActionInfoDone)
		{
			resetActionInfo();
			_isResetActionInfoDone = true;
		}

		animationFinished();
//test - releaseOpponent();
	}


	public void animEvent_GuardUp_Finished()
	{
		animationFinished_GuardUp();
	}


	//jsm - play sound
	public void animEvent_PlaySound(string key)
	{
//		playSound(key);
	}

	//jsm - show effect
	public void animEvent_ShowEffect(string key)
	{
		showEffect(key);
	}

	//jsm - play sound with effect
	public void anmiEvent_SpawnFx(string key)
	{
		playSound(key);
		showEffect(key);
	}

	public void animEvent_ShowTrail()
	{
//		setTrail(true);
	}

	public void animEvent_Zoom(float depth)
	{
//		if (BattleBase.Instance.IsPriorityTargetStrategyOn) return;

		setZoom(depth);
	}

	public void animEvent_SpawnFxAtTarget(string key)
	{
		//spawnEffectAtTarget(key);
	}

	public void animEvent_DespawnFxAtTarget(string key)
	{
		deSpawnEffectAtTarget(key);
	}

	public void animEvent_SlowMotion_Finished()
	{
		slowMotionFinished ();
	}

	public void animEvent_Weapon_Hide()
	{
		//Log.jprint(gameObject.name + "  -----h h h-----   animEvent_Weapon_Hide() ");
		showWeapon(false);
	}

	public void animEvent_Weapon_Show()
	{
		//Log.jprint(gameObject.name + "  -----s s s-----   animEvent_Weapon_Show() ");
		showWeapon(true);
	}

	public void animEvent_InstallWeapon()
	{
		Log.jprint(gameObject.name + "  ----------   animEvent_InstallWeapon() ");

		showWeaponForTheFirstTime();
	}





	const float _event_check_duration = 4.0f; 

//jks 2015.11.4 무기장착 연출 제거. --------------------------------------->
//	//jks 무기장착 애니 종료 이벤트.
	public void animEvent_Inven_EventEnd()
	{
//		_isAnimationPlaying_WeaponInstallEnd = false;
//
//		BattleBase.Instance.swapInvenCamera();
	}
//
//
//	//jks animation event 오지 않았을 경우 대비 처리. --------------------------------
//	bool _isAnimationPlaying_WeaponInstallEnd = false;
//
//	public void startAnimationEventDoubleCheck_InstallWeapon()
//	{
//		_isAnimationPlaying_WeaponInstallEnd = true;
//		Invoke("force_AnimEvent_Inven_EventEnd", _event_check_duration);  //jks 3초 후에도 이벤트 오지 않으면 강제 처리.
//	}
//
//	void force_AnimEvent_Inven_EventEnd()
//	{
//		if (_isAnimationPlaying_WeaponInstallEnd == false) return;
//
//		animEvent_Inven_EventEnd();
//	}
//	//jks -----------------------------------------------------------------------
// <---------------------------------------------------------------------




	//jks 클래스 매치 종료 이벤트.
	public void animEvent_SkillStaging_Finished()
	{
		skillStagingFinishied();  
	}







//jks 2015.11.4 보스액션 제거.
//	//jks 보스 연출 애니 종료 이벤트.
//	public void animEvent_BossAction_Finished()
//	{
//		BattleBase.Instance.IsSkillStagingInProgress = false;
//		BattleBase.Instance.PauseFighters (false);
//		BattleBase.Instance.UseCard_Disabled = false;
//
//		_isAnimationPlaying_BossAction = false;
//
//		bossActionFinishedProcess();
//
//		animEvent_Action_Finished();
//	}

//jks 2015.11.4 보스액션 제거.
//	//jks animation event 오지 않았을 경우 대비 처리. --------------------------------
//
//	bool _isAnimationPlaying_BossAction = false;
//
//	public void startAnimationEventDoubleCheck_BossAction()
//	{
//		_isAnimationPlaying_BossAction = true;
//		Invoke("force_AnimEvent_BossAction_Finished", _event_check_duration);  //jks 3초 후에도 이벤트 오지 않으면 강제 처리.
//	}
//
//	void force_AnimEvent_BossAction_Finished()
//	{
//		GameObject bossCam = GameObject.Find("Boss Event Camera");
//
//		if (_isAnimationPlaying_BossAction || bossCam != null) //jks 아직 anim event 가 안 불렸거나, boss cam 이 아직 살아 있으면, 강제로 연출 종료 처리.
//			animEvent_BossAction_Finished();
//	}
//	//jks -----------------------------------------------------------------------

//jks 2015.11.4 보스액션 제거.
//	void bossActionFinishedProcess()
//	{
//		GameObject bossCam = GameObject.Find("Boss Event Camera");
//		if (bossCam != null)
//		{
//			Utility.setLayerAllChildren(bossCam.transform.parent.gameObject, "Default") ; 
//			Utility.changeShader (gameObject, BattleBase.Instance._toonBasicOutline, true, "Effect"); //jks assetbundle friendly
//			Utility.changeOutline(gameObject, TestOption.Instance()._outline_color_boss, TestOption.Instance()._outline_width_boss);
//
//			Destroy (bossCam);
//			BattleBase.Instance.IsIgnoreButtonTouch = false;
//			BattleUI.Instance()._cam_enchant.enabled = true;
//			BattleManager.Instance.setEventBossScale(bossCam.transform.root.gameObject, true);
//		}
//	}


	//jks 2016.4.26 정교한 이벤트 처리를 위해 평타 전용 이벤트와 콤보 번호를 이벤트에 설정된 값 받게 갱신.
	public void animEvent_QuickCombo_Action_Finished(int combo)
	{
		//Log.jprint(Time.time + "    "+ gameObject.name +"     *  *  *  * animEvent_Action_Finished *  *  *  *  ");
		quickSkill_Action_Finished(combo);
	}

	public void animEvent_QuickCombo_Transition(int combo)
	{
		//Log.jprint(Time.time + "    "+ gameObject.name +"     *  *  *  * T R A N S I T I O N *  *  *  *  ");
		quickSkill_Combo_Transition(combo);
	}

	public void animEvent_Combo_InputWindow_Open(int combo)
	{
		//Log.jprint(Time.time + "    "+ gameObject.name +"     *  *  *  * INPUT W OPEN *  *  *  *  ");
		quickSkill_InputWindow_Open(combo);
	}
	
	public void animEvent_Combo_InputWindow_Close(int combo)
	{
		//Log.jprint(Time.time + "    "+ gameObject.name +"     *  *  *  * INPUT W CLOSE *  *  *  *  ");
		quickSkill_InputWindow_Close(combo);
	}

	
	public virtual void quickSkill_Action_Finished(int combo){}
	public virtual void quickSkill_Combo_Transition(int combo){}
	public virtual void quickSkill_InputWindow_Open(int combo){}
	public virtual void quickSkill_InputWindow_Close(int combo){}

	public virtual void showWeapon(bool bShow){}

	public virtual void skillStagingFinishied(){}

	public virtual void resetActionInfo(){}

	public virtual void animationFinished(){}

	public virtual void animationFinished_GuardUp(){}
	
	public virtual void giveDamage(float reactionDistanceOverride){}

	public virtual void giveDamage_QuickSkill(float reactionDistanceOverride){}

	public virtual void playSound(string key){}

	public virtual void showEffect(string key){}

	public virtual void setTrail(bool isTrail){}

	public virtual void setZoom(float depth){}

	public virtual void spawnEffectAtTarget(string key){}

    public virtual void deSpawnEffectAtTarget(string key){}

	public virtual void slowMotionFinished(){}

	public virtual void showWeaponForTheFirstTime(){}

	public virtual void holdOpponent(){}

	public virtual void releaseOpponent(){}

}
