using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

public class Knowledge_Mortal_Fighter_Main_Meteor_PvP : Knowledge_Mortal_Fighter_Main_Meteor
{
	void Awake()
	{
	}


	protected override void initializeBeforeUpdateBegin()
	{
		_cur_hp = _max_hp;		
		AnimCon.setAnimatorCullingMode(AnimatorCullingMode.AlwaysAnimate);
	}

	// in pvp, class matching is not used.
	public override bool IsClassMatching
	{
		get { return false; }
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



	public override int getReactionAnimID(int combo, eHitType hitType)
	{
		if(combo == 0) //jks HACK: safty check
			combo = 1;
		
		int reactionChoice;
		
		if (hitType == eHitType.HT_CRITICAL)
		{
			//reactionChoice = Random.Range(4, 6);  //jks critical reaction only
			float randomNum = Utility.GetRandom_0_1();
			if (randomNum < 0.33f) reactionChoice = 4;
			else if (randomNum < 0.66f) reactionChoice = 5;
			else reactionChoice = 6;
			
		}
		else if (hitType == eHitType.HT_MISS || hitType == eHitType.HT_BAD)
		{   //jks protect motion
			return _anim_protect; //jks     reactionChoice = 3;  //jks block reaction only
		}
		else 
		{
			//reactionChoice = Random.Range(0, 2);
			float randomNum = Utility.GetRandom_0_1();
			if (randomNum < 0.33f) reactionChoice = 0;
			else if (randomNum < 0.66f) reactionChoice = 1;
			else reactionChoice = 2;
		}
		
		//Log.jprint(gameObject + "   reaction choice: "+ reactionChoice + "     reaciton anim #: "+ _anim_reaction[combo-1,reactionChoice]);
		
		return _anim_reaction[combo-1,reactionChoice]; //jks index 0 == combo1, index 1 == combo2, index 2 == combo3
	}




	public override void startCoolTimer()
	{
		//		forceResetFlags();
		//		Action_WalkBack = true;
	}
	
	protected override float GetRandomNumberForHitType()
	{
		return Utility.GetRandom_0_1();
	}

	public override void updateHP(ObscuredInt damage, eHitType hitType)
	{
		int deltaDamage = damage; //jks to decrease total sinsu as much as individual card sinsu decrement.
		if (Current_HP < -damage)
		{
			deltaDamage = -Current_HP;
		}
		BattleBase.Instance.updateTotalHP(deltaDamage, AllyID);
		
		base.updateHP(damage, hitType);		

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
			//if (ComboCurrent != 0)  
			if (ComboRecent != 0) //jks if skill ever initiated?
			{
				//				startCoolTimer();
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
		
//		_isTargetInShowWeaponRange = !IsPvpVictim;// && (distShell_AttackerAndClosestOpponent < _attackDistanceMax + 4); //jks too show weapon if target is closer than 3 meter.
//		showWeapon(_isTargetInShowWeaponRange);
		
		if (_attackDistanceMax < distShell_AttackerAndClosestOpponent)
		{
			//jks zoom out if no enemy.
			if (CameraManager.Instance.doIHaveCameraAttached(transform))
				setZoomOutState(true);//jks if no enemy. request zoom out.
			
			return false;
		}		
		
		if (CameraManager.Instance.doIHaveCameraAttached(transform))
			setZoomOutState(false);//jks found enemy. request zoom in.

		return true;
	}



	public override GameObject getOpponentsInScanDistance_WeaponPositionBased()
	{
		if (_pvp_target == null)
		{
			//showWeapon(false);
			//jks zoom out if no enemy.
			if (CameraManager.Instance.doIHaveCameraAttached(transform))
				setZoomOutState(true);//jks if no enemy. request zoom out.
			
			return null;
		}
		
		GameObject closestOpponent = _pvp_target;
		setTarget(closestOpponent);
		
		Knowledge_Mortal opponentKnowledge = closestOpponent.GetComponent<Knowledge_Mortal>();
		
		//jks - body based
		float distShell_AttackerAndClosestOpponent = Mathf.Abs(transform.position.x - closestOpponent.transform.position.x) - this.Radius - opponentKnowledge.Radius;
		
		//jks - weapon based
		float distWeapon_AttackerAndClosestOpponent = Mathf.Abs(WeaponEndPosition.x  - closestOpponent.transform.position.x) - opponentKnowledge.Radius;
		
		//jks 무기를 뒤로 휘두르는 경우 몸보다 더 뒤에 위치하기 때문에  이 경우는 몸 위치로 계산. 
		float finalDistToCheck = Mathf.Min(distShell_AttackerAndClosestOpponent, distWeapon_AttackerAndClosestOpponent);
		
		//		if (gameObject.name.Contains("C"))
		//		{
		//			Log.jprint("+++++++++  "+ transform.position.x + " : " + WeaponEndPosition.x + " : " + closestOpponent.transform.position.x);
		//			Log.jprint(Time.time + ": " + gameObject + " found as closest: " + closestOpponent + " ++++++++++++ " + "finalDistToCheck: " + finalDistToCheck + " < ? " + _attackDistanceMax); 
		//		}
		
		if (_attackDistanceMax + _weaponLength < finalDistToCheck) 
		{
			//jks zoom out if no enemy.
			if (CameraManager.Instance.doIHaveCameraAttached(transform))
				setZoomOutState(true);//jks if no enemy. request zoom out.
			
			return null;
		}
		
		
		if (CameraManager.Instance.doIHaveCameraAttached(transform))
			setZoomOutState(false);//jks found enemy. request zoom in.

		
		return closestOpponent;
	}


	protected override void giveAreaDamageNow(float reactionDistanceOverride, float initialCenter)
	{
		if (CameraManager.Instance == null) return;
		
//		_opponentsInAttackRange.Clear(); //reset
		
//		bool found = HaveOpponentAhead(10 ,"Fighters", _opponentsInAttackRange);		
//		if (!found) return;
		if (BattleBase.Instance.List_Enemy.Count <= 0) return;

//		GameObject closestOpponent = Utility.findClosestGameObjectAmongPool(gameObject, _opponentsInAttackRange);

		GameObject closestOpponent = BattleBase.Instance.findClosestOpponent(AllyID, transform.position.x);
		if (closestOpponent == null) return;

		Knowledge_Mortal_Fighter_Main knowledgeOpponent = closestOpponent.GetComponent<Knowledge_Mortal_Fighter_Main>();
		if (knowledgeOpponent == null) return;

		//jks for selective damage
		if (! knowledgeOpponent.IsPvpVictim) return;

		setTarget(closestOpponent);

		
//		float areaCenter;
//		bool headingRight = transform.forward.x > 0;
//		if (headingRight)
//		{
//			areaCenter = transform.position.x + _attackDistanceMax;
//		}
//		else
//		{
//			areaCenter = transform.position.x - _attackDistanceMax;
//		}
		
		
		{
			//jks check damage range
//			float dist = Mathf.Abs(areaCenter - closestOpponent.transform.position.x);		
//			float distShell = dist - this.Radius - goKnowledge.Radius;
//			if (distShell > _damageRange) return;
			

			eHitType hitType = getFinalHitType(knowledgeOpponent);
			
			int hitReactionAnimID = getReaction(hitType);
			ObscuredInt classRelationAttackPoint = calculate_ClassRelation_AttackPoint(AttackPoint, knowledgeOpponent);
			//jks 2015.8.26 no more: int leaderBuffAttackPoint = calculate_LeaderBuff_AttackPoint_Opponent();
			//jks 2015.5.8 remove leader strategy-				int leaderStrategyAttack = calculate_LeaderStrategy_AttackPoint();
			
			
			ObscuredInt finalAttack = AttackPoint + classRelationAttackPoint;//jks 2015.8.26 no more:  + leaderBuffAttackPoint; //jks 2015.5.8 remove leader strategy-	 + leaderStrategyAttack;

			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
			{
				Log.print_always("   --------------------------------- PVP ---------------------------------");
				Log.print_always("   현재 리더 클래스: None ");
				Log.print_always("   공격자 : " + gameObject + "  -->  피해자: " + knowledgeOpponent);
				Log.print_always("   공격자 클래스 : " + Class + "  -->  피해자 클래스: " + knowledgeOpponent.Class + "   피격 타입: " + hitType);
				Log.print_always("   G I V E  D A M A G E      Original: "+ AttackPoint + "    + 클래스 상성 공격력: "+classRelationAttackPoint+" = " + finalAttack);
				Log.print_always("   PVP 카드숫자 우위 공격치 배수: " + PvpAttackBoost + "  x  "+ finalAttack + "  =  " + (PvpAttackBoost * finalAttack));
			}
			#endif
			
			finalAttack *= PvpAttackBoost;
			
			//임의 공격력 부스트
			finalAttack *= 4;

			knowledgeOpponent.takeDamage(finalAttack, hitReactionAnimID, hitType, AttackType, _weaponType_ForAnimation, gameObject, reactionDistanceOverride);
		}
		
	}




	
//	protected override void updateDamageUI(Transform transform, ObscuredInt damagePoint, eHitType hitType)
//	{
//		if (PvpBattleUI.Instance() != null)
//			PvpBattleUI.Instance().addDemageUI(transform, damagePoint, hitType);
//	}
	
	
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
