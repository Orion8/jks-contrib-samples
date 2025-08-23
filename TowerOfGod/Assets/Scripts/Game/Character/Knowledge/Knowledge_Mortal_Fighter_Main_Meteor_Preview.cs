using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

public class Knowledge_Mortal_Fighter_Main_Meteor_Preview : Knowledge_Mortal_Fighter_Main_Meteor
{

	protected override void initializeBeforeUpdateBegin()
	{
		_cur_hp = _max_hp;		
		accordAttackDirection();
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

		
		
		//		if (IsLeader)
		//			Log.jprint(gameObject + "   doNextAction  c1: " + Action_Combo1 + " c2: " + Action_Combo2 + " c3: " + Action_Combo3 + " c4: " + Action_Combo4 + " c5: " + Action_Combo5 + " c6: " + Action_Combo6);
		
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
	

	
//	protected override void updateDamageUI(Transform transform, ObscuredInt damagePoint, eHitType hitType)
//	{
//		if (PvpBattleUI.Instance() != null)
//			PvpBattleUI.Instance().addDemageUI(transform, damagePoint, hitType);
//	}


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
