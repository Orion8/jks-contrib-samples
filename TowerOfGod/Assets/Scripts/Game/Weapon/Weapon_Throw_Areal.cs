using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;



public class Weapon_Throw_Areal : Weapon_Throw 
{

	protected float _radius;				//jks- specify region to be influenced

	protected float _reactionDistanceOverride;

	protected List<GameObject> _opponentsInRadius = new List<GameObject>();
	protected List<GameObject> _opponentsNotInRadius = new List<GameObject>();

	protected string _tagNameToSearch = "Fighters";

	public void setAttributes(float radius)
	{
		_radius = radius;
	}

	protected void resetList()
	{
		_opponentsInRadius.Clear();
		_opponentsNotInRadius.Clear();
	}


	protected override void activateWeapon()
	{
		base.activateWeapon();

		StartCoroutine(updateOpponentsInRadius());
	}


	
	//jks update opponents in active area.
	IEnumerator updateOpponentsInRadius()
	{
		while (true)
		{
			updateList();
			yield return new WaitForSeconds(0.1f);
		}
	}


	protected void updateList()
	{
		resetList();

//		GameObject[] gosInWorld = GameObject.FindGameObjectsWithTag(_tagNameToSearch);
//		if (gosInWorld.Length == 0) return;

		List<FighterActor> fighters;

		if (AllyID == eAllyID.Ally_Human_Me)
			fighters = BattleBase.Instance.List_Enemy;
		else 
			fighters = BattleBase.Instance.List_Ally;

		float xMin = transform.position.x - _radius;
		float xMax = transform.position.x + _radius;

		findOpponentsInArea(xMin, xMax, fighters, _opponentsInRadius, _opponentsNotInRadius);


		#if UNITY_EDITOR
		// debugging
		_arealAttack_ShowDamageRange = true;
		_arealAttack_DamageCenter = transform.position;
		_arealAttack_Radius = _radius;
		#endif
	}


	protected void findOpponentsInArea(float xMin, float xMax, List<FighterActor> opponents, 
		List<GameObject> gosInRange, List<GameObject> gosNotInRange)
	{
		//Reset
		gosInRange.Clear();
		gosNotInRange.Clear();

		foreach(FighterActor opponent in opponents)
		{
			if (opponent._go == null) continue;

			Vector3 goPos = opponent._go.transform.position;

			float radius = opponent._go.GetComponent<Knowledge_Mortal_Fighter>().Radius;

			if ((goPos.x + radius) < xMin || (goPos.x - radius) > xMax)
				gosNotInRange.Add(opponent._go);
			else
				gosInRange.Add(opponent._go);
		}
	}




	protected void setFlagInList(List<GameObject> list, Action<GameObject> setWeaponFunctionFlag)
	{
		if (_owner == null) return;
			
		Knowledge_Mortal_Fighter ownerKnow = _owner.GetComponent<Knowledge_Mortal_Fighter>();

//		if (BattleBase.Instance.isSkillPreviewMode()) return;
		foreach(GameObject go in list)
		{
			if (go == null) continue;
			if (!go.activeSelf) continue;

			if (ownerKnow.IsAlly(go.GetComponent<Knowledge_Mortal_Fighter>()))
				continue;
			
			setWeaponFunctionFlag(go);
		}
	}


	public override void killMe()
	{
		// debugging
		#if UNITY_EDITOR
		_arealAttack_ShowDamageRange = false;
		#endif

		updateList();

		base.killMe();
	}


	protected override void createEffect() 
	{
		_effectPrefab = MadPool.Instance().poolInstantiate(_effect_name, transform.position, transform.rotation);
	}


	protected override void updateEffectPosition() {}


	#if UNITY_EDITOR
	protected bool _arealAttack_ShowDamageRange = false;
	Vector3 _arealAttack_DamageCenter = new Vector3();
	float _arealAttack_Radius;
	Color _arealAttack_Color = new Color(0.7f, 0.7f, 0.1f, 0.5f);
	void OnDrawGizmos() 
	{
		if (_arealAttack_ShowDamageRange)
		{
			Gizmos.color = _arealAttack_Color;
			Gizmos.DrawSphere(_arealAttack_DamageCenter, _arealAttack_Radius);
			
			//Log.jprint(gameObject + "    QQQQQQ   damage center: "+ _arealAttack_DamageCenter);
		}
	}
	#endif


	protected virtual void refreshAnimationForSlowReset(Knowledge_Mortal_Fighter know)
	{
//		if (know.AmIManualLeader) return;
//		if (know.Progress_SkillAction) return;

//		Log.jprint(Time.time+ "    " + know.gameObject.name +  "    refreshAnimation()  SkillAction:  " + know.Progress_SkillAction
//			+  "    run:  " + know.Action_Run);

//		know.forceResetFlags();
//		know.Action_Run = true;

		know.AnimCon.setAnimationSpeed(1.0f);
	}



	protected void releaseSlowedOpponent()
	{
		List<FighterActor> fighters;

		if (AllyID == eAllyID.Ally_Human_Me)
			fighters = BattleBase.Instance.List_Enemy;
		else 
			fighters = BattleBase.Instance.List_Ally;

		if (BattleBase.Instance.isSkillPreviewMode()) return;
		foreach(FighterActor fighter in fighters)
		{
			if (fighter._go == null) continue;

			Knowledge_Mortal_Fighter fighterKnow = fighter._go.GetComponent<Knowledge_Mortal_Fighter>();

			if (fighterKnow == null) continue;

			fighterKnow.SkillSpeedAdjustment = 1;
			fighterKnow.WeaponSlowMovement = 1;

			//Log.jprint(Time.time + "  target: "+ fighter._go + "     released ........" );

			refreshAnimationForSlowReset(fighterKnow);
		}

	}



	protected virtual void giveDamage(GameObject target)
	{
		if (_owner == null) return;
		if (target == null) return;

		Knowledge_Mortal_Fighter knowledgeOpponent = target.GetComponent<Knowledge_Mortal_Fighter>();
		Knowledge_Mortal_Fighter ownerKnowledge = _owner.GetComponent<Knowledge_Mortal_Fighter>();

		if (ownerKnowledge == null) return;
		if (knowledgeOpponent == null) return;


		eHitType hitType = eHitType.HT_MISS; 
		hitType = ownerKnowledge.getFinalHitType(knowledgeOpponent);

		ObscuredInt finalAttack = ownerKnowledge.getAttackPoint();

		ObscuredInt leaderBuffAttackPoint;
		if (ownerKnowledge.AllyID == eAllyID.Ally_Human_Me)
		{
			leaderBuffAttackPoint = ownerKnowledge.getLeaderBuffAttackUp(finalAttack);
			if (leaderBuffAttackPoint > 0)
			{
				#if UNITY_EDITOR
				if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
					Log.print_always("  기본공격력:  "+ finalAttack + "  + 리더 버프 추가치: " + leaderBuffAttackPoint + "   new 공격력: " + (finalAttack+leaderBuffAttackPoint));
				#endif

				finalAttack += leaderBuffAttackPoint;
			}
		}


		if (ownerKnowledge.IsLeader || knowledgeOpponent.IsLeader || TestOption.Instance()._classRelationBuffAll)
		{
			ObscuredInt classRelationAttackPoint = ownerKnowledge.calculate_ClassRelation_AttackPoint(ownerKnowledge.AttackPoint, knowledgeOpponent);

			finalAttack += classRelationAttackPoint; 

			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
			{
				Log.print_always(Time.time + "   --------------------------------- 발사체 타격 ---------------------------------");
				if (BattleBase.Instance.LeaderTransform)
					Log.print_always("   현재 리더 클래스: "+ BattleBase.Instance.LeaderClass + "   :  " + BattleBase.Instance.LeaderTransform.gameObject);
				Log.print_always("   공격자 : " + _owner + "  -->  피해자: " + knowledgeOpponent);
				Log.print_always("   공격자 클래스 : " + ownerKnowledge.Class + "  -->  피해자 클래스: " + knowledgeOpponent.Class + "   피격 타입: " + hitType);
				Log.print_always("   G I V E  D A M A G E      공격력: " + (finalAttack-classRelationAttackPoint) + "  +  클래스상성 공격력: " + classRelationAttackPoint + "  =  " + finalAttack);


				if (ownerKnowledge.PvpAttackBoost > 1)
				{
					Log.print_always("   PVP 카드숫자 우위 공격치 배수: " + ownerKnowledge.PvpAttackBoost + "  x  "+ finalAttack + "  =  " + (ownerKnowledge.PvpAttackBoost * finalAttack));
				}
			}
			#endif

		}
		else
		{
			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
			{
				Log.print_always("   --------------------------------- 발사체 타격 ---------------------------------");
				Log.print_always("   공격자 : " + _owner + "  -->  피해자: " + knowledgeOpponent);
				Log.print_always("   피격 타입: " + hitType);
				Log.print_always("   G I V E  D A M A G E      기본 공격력: " + finalAttack);

				if (ownerKnowledge.PvpAttackBoost > 1)
				{
					Log.print_always("   PVP 카드숫자 우위 공격치 배수: " + ownerKnowledge.PvpAttackBoost + "  x  "+ finalAttack + "  =  " + (ownerKnowledge.PvpAttackBoost * finalAttack));
				}
			}
			#endif

		}



		if (ownerKnowledge.AllyID == eAllyID.Ally_Human_Me)
		{
			if (BattleUI.Instance() != null)
			{
				Knowledge_Mortal_Fighter_Main mainKnow = _owner.GetComponent<Knowledge_Mortal_Fighter_Main>();
				if (mainKnow != null)
				if (mainKnow.IsLeader && BattleUI.Instance().ClassMatchOwner != -1)
				{
					float classMatchBoost = Inventory.Instance().CSlot[mainKnow.CardDeckIndex].SBlock.Value * 0.01f;
					finalAttack += Mathf.RoundToInt(finalAttack * classMatchBoost);

					#if UNITY_EDITOR
					if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
					{
						Log.print_always(" > > >  M A T C H  < < <        "   + Inventory.Instance().CSlot[mainKnow.CardDeckIndex].SBlock.Value +  " % 증가    =   공격력:  " + finalAttack);
					}
					#endif
				}
			}

		}


		finalAttack = damageAdjustment(finalAttack);


		int reactionID = ownerKnowledge.getReaction(hitType);

		knowledgeOpponent.takeDamage(finalAttack, reactionID, hitType, ownerKnowledge.AttackType, 
			ownerKnowledge._weaponType_ForAnimation, ownerKnowledge.gameObject, _reactionDistanceOverride);

		//					if (hitType != eHitType.HT_BAD  &&  hitType != eHitType.HT_MISS)
		//						startAdditionalSkillFunction(knowledgeOpponent , ownerKnowledge);


	}


	protected virtual int damageAdjustment(int curDamage)
	{
		return curDamage;
	}


}
