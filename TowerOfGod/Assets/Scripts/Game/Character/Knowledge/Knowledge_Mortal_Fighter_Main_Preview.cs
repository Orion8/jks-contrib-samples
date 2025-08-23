using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

public class Knowledge_Mortal_Fighter_Main_Preview : Knowledge_Mortal_Fighter_Main 
{

	protected override void initializeBeforeUpdateBegin()
	{
		_cur_hp = _max_hp;		
		accordAttackDirection();
		findWeaponBones();

		//InvokeRepeating("bossBorderLine", 1, 0.25f);
	}

	public override bool IsBattleTimeStarted
	{ 
		get { return true; }
	}

	
	public override void startCoolTimer()
	{
		//		forceResetFlags();
		//		Action_WalkBack = true;
	}
	
	public override void updateHP(ObscuredInt damage, eHitType hitType)
	{
	}


	public override bool canUseSkill()
	{
		return 	true;
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
		
		setActionFlag_Preview();


		if (NoNextComboAction) //jks if no next action,
		{
//			forceResetFlags();
//			Action_Idle = true;
//			resetPosition();

			//if (ComboCurrent != 0)  
			if (ComboRecent != 0) //jks if skill ever initiated?
			{
				//				startCoolTimer();
				ComboRecent = 0;
				ComboCurrent = 0;
				//Log.jprint(gameObject + " C O O L T I M E  S T A R T : " + CardDeckIndex);				
				processOnUseCardEnd();
			}
		}
		
	}
	
	public override void processOnUseCardEnd()
	{
		activateSkillFinishedEvent();
	}



//	public override void showWeapon(bool bShow)  //jks in preview, always show
//	{
//		setWeaponVisibility(true);
//	}



	
	
	public override void giveDamage(float reactionDistanceOverride)
	{		
//		if (CameraManager.Instance == null) return;

		_skillCompleted = isLastDamageInSkill(reactionDistanceOverride);

		GameObject closestOpponent = getOpponentsInScanDistance_WeaponPositionBased();
		if (closestOpponent == null) return;
		
		Knowledge_Mortal opponentKnowledge = closestOpponent.GetComponent<Knowledge_Mortal>();

		//jks - body based
		float distShell_AttackerAndClosestOpponent = Mathf.Abs(transform.position.x - closestOpponent.transform.position.x) - this.Radius - opponentKnowledge.Radius;
		
		//jks - weapon based
		float distWeapon_AttackerAndClosestOpponent = Mathf.Abs(WeaponEndPosition.x  - closestOpponent.transform.position.x) - opponentKnowledge.Radius;
		
		//jks 무기를 뒤로 휘두르는 경우 몸보다 더 뒤에 위치하기 때문에  이 경우는 몸 위치로 계산. 
		float finalDistToCheck = Mathf.Min(distShell_AttackerAndClosestOpponent, distWeapon_AttackerAndClosestOpponent);
		
		//jks - 공격 중 적에게 최대공격가능거리보다 가까워지면, 가까워진 거리를 와 damage range를 기준으로 공격 받을 적 판단하기위한 값.
		float damageDistance = Mathf.Min(_attackDistanceMax, finalDistToCheck) + _damageRange;

		GameObject go = closestOpponent;

		//jks check damage range
		opponentKnowledge = go.GetComponent<Knowledge_Mortal>();
		if (opponentKnowledge == null) return;
		//jks - body based
		distShell_AttackerAndClosestOpponent = Mathf.Abs(transform.position.x - go.transform.position.x) - this.Radius - opponentKnowledge.Radius;
		//jks - weapon based
		distWeapon_AttackerAndClosestOpponent = Mathf.Abs(WeaponEndPosition.x  - go.transform.position.x) - opponentKnowledge.Radius;
		//jks 무기를 뒤로 휘두르는 경우 몸보다 더 뒤에 위치하기 때문에  이 경우는 몸 위치로 계산. 
		finalDistToCheck = Mathf.Min(distShell_AttackerAndClosestOpponent, distWeapon_AttackerAndClosestOpponent);


		if (go == getCurrentTarget())
		{
			if (finalDistToCheck > damageDistance + _weaponLength)  //jks for the current target, give generous(+ weapon length) check
				return;
		}
		else 
		{
			if (finalDistToCheck > damageDistance) 
				return;
		}

		Knowledge_Mortal_Fighter knowledgeOpponent = go.GetComponent<Knowledge_Mortal_Fighter>();
		
		eHitType hitType = getFinalHitType(knowledgeOpponent);
		
		int hitReactionAnimID = getReaction(hitType);


		ObscuredInt classRelationAttackPoint = calculate_ClassRelation_AttackPoint(_attackPoint, knowledgeOpponent);

		ObscuredInt finalAttack = _attackPoint + classRelationAttackPoint;
		
		BattleBase.Instance.incrementHitTypeCount(CardDeckIndex, hitType);
		SkillType = eSkillType.ST_Card;
		knowledgeOpponent.takeDamage(finalAttack, hitReactionAnimID, hitType, AttackType, _weaponType_ForAnimation, gameObject, reactionDistanceOverride);
	}

	
//	protected override void updateDamageUI(Transform transform, ObscuredInt damagePoint, eHitType hitType)
//	{
//		//Log.jprint(gameObject + "hitType : " + hitType);
//		if (PvpBattleUI.Instance() != null)
//			PvpBattleUI.Instance().addDemageUI(transform, damagePoint, hitType);
//	}
	
	
	
	public override eHitType getFinalHitType(Knowledge_Mortal_Fighter opponent)
	{
		eHitType hitType = eHitType.HT_GOOD; //jks 프리뷰에서는 크리티컬 나가지 않게 함.

		return hitType;
	}



	public override IEnumerator startSkill()
	{
		if (Progress_SkillAnimation) yield break;
		
		Action_Combo1 = true;
	}


	public override bool getOpponentsInScanDistance(bool bQuickSkill)
	{
		//GameObject closestOpponent = findOpponentCloseEnoughToAttack();
		GameObject closestOpponent = GameObject.Find("Dummy");
		setTarget(closestOpponent);

		if (closestOpponent == null) return false;
		
		Knowledge_Mortal opponentKnowledge = closestOpponent.GetComponent<Knowledge_Mortal>();
		float distShell_AttackerAndClosestOpponent = Mathf.Abs(transform.position.x - closestOpponent.transform.position.x) - this.Radius - opponentKnowledge.Radius;
		
		//_isTargetInShowWeaponRange = !IsPvpVictim;// && (distShell_AttackerAndClosestOpponent < _attackDistanceMax + 4); //jks too show weapon if target is closer than 3 meter.
		//showWeapon(_isTargetInShowWeaponRange);
		
		if (_attackDistanceMax < distShell_AttackerAndClosestOpponent)
			return false;

		return true;
	}


	protected override void pushPassedOpponents()
	{
		GameObject go = GameObject.Find("Dummy");
		setTarget(go);
		{
			if (go == null) return;
			if (go == _captured_target) return; // do not push whatever i hold.
			
			Knowledge_Mortal_Fighter goKnowledge = go.GetComponent<Knowledge_Mortal_Fighter>();
			if (goKnowledge == null) return;
			if (goKnowledge.AmIWall) return;
			if (goKnowledge.AmICaptured) return;
			
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
		GameObject go = GameObject.Find("Dummy");
		setTarget(go);
		{
			if (go == null) return;
			if (go == _captured_target) return; // do not push whatever i hold.
			
			Knowledge_Mortal_Fighter goKnowledge = go.GetComponent<Knowledge_Mortal_Fighter>();
			if (goKnowledge == null) return;
			if (goKnowledge.AmIWall) return;
			if (goKnowledge.AmICaptured) return;
			
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

}
