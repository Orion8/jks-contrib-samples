using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

public class Knowledge_Mortal_Fighter_Main_Throw_PvP : Knowledge_Mortal_Fighter_Main_Throw
{
	void Awake()
	{
	}
	
	
	protected override void initializeBeforeUpdateBegin()
	{
		_cur_hp = _max_hp;		
		//InvokeRepeating("bossBorderLine", 1, 0.05f);//0.25f);
		AnimCon.setAnimatorCullingMode(AnimatorCullingMode.AlwaysAnimate);
	}


	public override bool IsBattleTimeStarted
	{ 
		get { return true; }
	}

	
	public override void setAttributesFromTable(Table_Skill tbl)
	{
		base.setAttributesFromTable(tbl);
		
		//jks override attack distance for pvp long distance attack to prevent to clamp distance range
		_attackDistanceMin = 0;   
		
		if (_attackDistanceMax > 3)
			_attackDistanceMax = 3;  
	}


	protected override float GetRandomNumberForHitType()
	{
		return Utility.GetRandom_0_1();
	}
	
	public override void startCoolTimer()
	{
		//		forceResetFlags();
		//		Action_WalkBack = true;
	}
	
	public override void updateHP(ObscuredInt damage, eHitType hitType)
	{
		int deltaDamage = damage; //jks to decrease total sinsu as much as individual card sinsu decrement.
		if (Current_HP < -damage)
		{
			deltaDamage = -Current_HP;
		}
		BattleBase.Instance.updateTotalHP(deltaDamage, AllyID);
		
		base.updateHP(deltaDamage, hitType);
		
		
		if (Inventory.Instance() != null)
		{
			if (AllyID == eAllyID.Ally_Human_Me)
			{
				Inventory.Instance().CSlot[_cardDeckIndex].Now_HP = Current_HP;
			}
			else
			{
				PvpBattleUI.Instance().TargetUser.CSlot[_cardDeckIndex].Now_HP = Current_HP;
			}
		}
		
		if (PvpBattleUI.Instance() && IsDead)
			PvpBattleUI.Instance().setDeadCard(AllyID == eAllyID.Ally_Human_Me, _cardDeckIndex);
	}
	
	
//	protected override void updateDamageUI(Transform transform, ObscuredInt damagePoint, eHitType hitType)
//	{
//		if (PvpBattleUI.Instance() != null)
//			PvpBattleUI.Instance().addDemageUI(transform, damagePoint, hitType);
//	}


	//jks 일반 전투에선 대상이 리더인 스킬 경우도 pvp 에는 리더가 없기 때문에 상대에게 다가가 스킬 사용.
	public override bool checkEnemyOnPath(bool bQuickSkill)
	{
		return getOpponentsToAttackWhileMoving(bQuickSkill); //check 10 meter for the initial scan.
	}

	
	
	public override void resetActionInfo()
	{
		if (Action_Hit) 
		{
			forceResetFlags();
			Action_Idle = true;
			
			return;//jks do nothing after hit in PVP
		}
		
		forceResetFlags();
		
		//jks after victory action, goto postVictory for "idle" animation
		if ((IsBattleVictorious && AllyID == eAllyID.Ally_Human_Me) ||
		    (IsBattleFailed && AllyID != eAllyID.Ally_Human_Me))
		{
			Action_PostVictory = true;
		}
		
		doNextAction();
		
	}
	
	
	protected override void doNextAction()
	{
		
		setNextComboActionFlag();

		
		//		if (IsLeader)
		//			Log.jprint(gameObject + "   doNextAction  c1: " + Action_Combo1 + " c2: " + Action_Combo2 + " c3: " + Action_Combo3 + " c4: " + Action_Combo4 + " c5: " + Action_Combo5 + " c6: " + Action_Combo6);
		
		if (NoNextComboAction) //jks if no next action,
		{
			processOnUseCardEnd();
			
			if (ComboRecent != 0) //jks if skill ever initiated?
			{
				//startCoolTimer();
				ComboRecent = 0;
				ComboCurrent = 0;
				//Log.jprint(gameObject + " C O O L T I M E  S T A R T : " + CardDeckIndex);				
			}
		}
		
	}
	
	public override void processOnUseCardEnd()
	{
		if(ComboCurrent == TotalCombo) //jks if projectile fire action
		{
			//Log.jprint("processAfterResetActionInfo() : activateSkillFinishedEvent()");
			activateSkillFinishedEvent();
		}
	}
	
	
	//	public override void showWeapon(bool bShow)  //jks in pvp, always show
	//	{
	//		setWeaponVisibility(true);
	//	}
	


	public override bool getOpponentsInScanDistance(bool bQuickSkill)
	{
		if (_pvp_target == null)
		{
			//showWeapon(false);
			//jks zoom out if no enemy.
			if (CameraManager.Instance.doIHaveCameraAttached(transform))
				setZoomOutState(true);//jks if no enemy. request zoom out.
			
			return false;
		}
		
		GameObject closestOpponent = _pvp_target;
		setTarget(closestOpponent);
		
		Knowledge_Mortal opponentKnowledge = closestOpponent.GetComponent<Knowledge_Mortal>();
		float distShell_AttackerAndClosestOpponent = Mathf.Abs(transform.position.x - closestOpponent.transform.position.x) - this.Radius - opponentKnowledge.Radius;
		
		//_isTargetInShowWeaponRange = !IsPvpVictim;// && (distShell_AttackerAndClosestOpponent < _attackDistanceMax + 4); //jks too show weapon if target is closer than 3 meter.
		//showWeapon(_isTargetInShowWeaponRange);
		
		if (_attackDistanceMax < distShell_AttackerAndClosestOpponent) 
		{
			//jks zoom out if no enemy.
			if (CameraManager.Instance.doIHaveCameraAttached(transform))
				setZoomOutState(true);//jks if no enemy. request zoom out.
			
			return false;
		}
		
		if (CameraManager.Instance.doIHaveCameraAttached(transform))
			setZoomOutState(false); //jks zoom in
		
		return true;
	}

	
	
	public override GameObject getOpponentsInScanDistance_WeaponPositionBased()
	{
		return null;
	}


//	#region Leader Buff
//	
//	public override int calculate_LeaderBuff_AttackPoint_Self(CardClass enemyClass)
//	{
//		return 0;
//	}
//	
//	public override float calculate_LeaderBuff_HitRate_Self(CardClass enemyClass)
//	{
//		return 0;
//	}
//	
//	#endregion
//


	protected override void pushPassedOpponents()
	{
		//jks do for 1 vs 1 
		GameObject curTarget = getCurrentTarget();
		if (curTarget == null) return;
		
		
		if (transform.forward.x > 0)
		{
			if (transform.position.x > curTarget.transform.position.x) //jks passed
			{
				keepOpponentAtWeaponEnd(curTarget, 0.3f);
			}
		}
		else
		{
			if (transform.position.x < curTarget.transform.position.x) //jks passed
			{
				keepOpponentAtWeaponEnd(curTarget, 0.3f);
			}
		}
		
	}
	
	
	protected override void pushPassedOpponentsWhenIMove()
	{
		GameObject curTarget = getCurrentTarget();
		if (curTarget == null) return;
		if (IsDead) return;
		
		
		if (transform.forward.x > 0)
		{
			if (transform.position.x > curTarget.transform.position.x) //jks passed
			{
				keepOpponentAtShellEnd(curTarget, 0.3f);
			}
		}
		else
		{
			if (transform.position.x < curTarget.transform.position.x) //jks passed
			{
				keepOpponentAtShellEnd(curTarget, 0.3f);
			}
		}
		
	}

	
	//jks 2015.5.8 remove leader strategy-		
//	#region Leader Strategy
//	
//	
//	public override int calculate_LeaderStrategy_AttackPoint()
//	{
//		return 0;
//	}
//	
//	public override float calculate_LeaderStrategy_HitRate()
//	{
//		return 0;
//	}
//	
//	#endregion
	
	
}
