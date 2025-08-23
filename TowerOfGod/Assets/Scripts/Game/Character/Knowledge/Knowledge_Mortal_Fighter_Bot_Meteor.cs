using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;


public class Knowledge_Mortal_Fighter_Bot_Meteor : Knowledge_Mortal_Fighter_Bot
{

	//jks AttackInfo : delay in second to start damage
	//jks AttackInfo2 : damage duration in second.


	float _damage_duration;

	float _damage_interval;


	public override ObscuredInt DamageFrequency 
	{
		get 
		{ 
//jks 2016.1.5			
//			if (_comboPatterns.Count > 0)
//			{
//				int info2 = _comboPatterns[_comboPattern_current]._attackInfo2;
//				int info3 = _comboPatterns[_comboPattern_current]._attackInfo3;
//				calcDamageIntervalAndFrequency(info2, info3);
//			}

			if (_damage_frequency <= 0)
			{
				Log.Warning(gameObject.name + " _damage_frequency = "+ _damage_frequency);
				return 1;
			}
			return _damage_frequency; 
		} 
	}
	

	public override void setAttributesFromTable(Table_Skill tbl)
	{
		base.setAttributesFromTable(tbl);

		calcDamageIntervalAndFrequency(AttackInfo2, AttackInfo3);
	}


	protected void calcDamageIntervalAndFrequency(int info2, int info3)
	{
		_damage_duration = info2 * 0.01f;
		
		_damage_interval = info3 * 0.1f;
		if (_damage_interval == 0)
			_damage_interval = 0.05f; //jks give minimum interval
		
		_damage_frequency = Mathf.RoundToInt(_damage_duration / _damage_interval);
		if (_damage_frequency < 1)
			_damage_frequency = 1;

	}

	
	public override void giveDamage(float reactionDistanceOverride)
	{		
		if (KnowledgePSkill && KnowledgePSkill.Progress_AnyAction)
		{
			KnowledgePSkill.giveDamage(reactionDistanceOverride);
			return;
		}

		//jks 2016.3.14 skill buff 기능 추가.
		addSkillBuff();

		//		float delay = AttackInfo;
		//		Invoke("giveDamageDelayed", delay);
		_skillCompleted = false; //jks 2016.5.23 meteor 경우 항상 크리티컬 발생하는 이슈 방지.:   isLastDamageInSkill(reactionDistanceOverride);

		_damageCount=0;  //reset

		StartCoroutine(giveDamageDelayed(reactionDistanceOverride));
	}
	
	
	protected IEnumerator giveDamageDelayed(float reactionDistanceOverride)
	{
		float delay = AttackInfo;
		yield return new WaitForSeconds(delay);
		
		StartCoroutine(continuousDamage(reactionDistanceOverride));

		updateTotalGiveDamageCount();
	}
	
	
	protected float _startTime;
	protected int _damageCount=0;
	protected IEnumerator continuousDamage(float reactionDistanceOverride)
	{
		if (_damage_duration == 0) yield return null;

		_startTime = Time.time;
		
		float initialCenter = transform.position.x;
		while (Time.time - _startTime < _damage_duration) 
		{
			if (++_damageCount == DamageFrequency) //jks 2016.5.23 막타면,
				_skillCompleted = true;

			giveAreaDamageNow(reactionDistanceOverride, initialCenter);

			yield return new WaitForSeconds(_damage_interval);
		}
	}
	
	

	protected override void giveAreaDamageNow(float reactionDistanceOverride, float initialCenter)
	{
		if (CameraManager.Instance == null) return;

//		_opponentsInAttackRange.Clear(); //reset

//		bool found = HaveOpponentAhead(10 ,"Fighters", _opponentsInAttackRange);
//		if (!found) return;
		if (Opponents.Count <= 0) return;

		//Log.jprint(gameObject.name + "    1111111   damage center: "+ initialCenter);

		float areaCenter;
		bool headingRight = transform.forward.x > 0;
		if (headingRight)
		{
			areaCenter = initialCenter + _attackDistanceMax;
		}
		else
		{
			areaCenter = initialCenter - _attackDistanceMax;
		}

		//Log.jprint(gameObject.name + "    2222222   damage center: "+ areaCenter);

		#if UNITY_EDITOR
		//jks debugging
		_arealAttack_DamageCenter = transform.position;
		_arealAttack_DamageCenter.x = areaCenter;
		_arealAttack_ShowDamageRange = true;
		_arealAttack_Radius = _damageRange;
		#endif

		//foreach(GameObject go in _opponentsInAttackRange)
		foreach (FighterActor opponent in Opponents)
		{
			GameObject go = opponent._go;
			if (go == null) continue;
			if (!go.activeSelf) continue;

			giveDamageTo(go, areaCenter, reactionDistanceOverride);
		}

	}


	private void giveDamageTo(GameObject go, float areaCenter, float reactionDistanceOverride)
	{
		//jks check damage range
		Knowledge_Mortal goKnowledge = go.GetComponent<Knowledge_Mortal>();
		if (goKnowledge == null) return;

		//			float dist = Mathf.Abs(areaCenter - go.transform.position.x);		
		//			float distShell = dist - this.Radius - goKnowledge.Radius;
		//			if (distShell > _damageRange) continue;
		float distFromCenter = Mathf.Abs(areaCenter - go.transform.position.x) - goKnowledge.Radius;		
		if (distFromCenter > _damageRange) return;


		Knowledge_Mortal_Fighter knowledgeOpponent = go.GetComponent<Knowledge_Mortal_Fighter>();


		#if UNITY_EDITOR
		if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
		{
			Log.print_always("   --------------------------------- Area Damage ---------------------------------");
		}
		#endif
		eHitType hitType = getFinalHitType(knowledgeOpponent);

		int hitReactionAnimID = getReaction(hitType);

		ObscuredInt finalAttack = AttackPoint; 


		if (IsLeader || knowledgeOpponent.IsLeader || TestOption.Instance()._classRelationBuffAll)
		{
			ObscuredInt classRelationAttackPoint = calculate_ClassRelation_AttackPoint(AttackPoint, knowledgeOpponent);
			//jks 2015.8.26 no more: int leaderBuffAttackPoint = calculate_LeaderBuff_AttackPoint_Opponent();
			//jks 2015.5.8 remove leader strategy-				int leaderStrategyAttack = calculate_LeaderStrategy_AttackPoint();


			finalAttack = AttackPoint + classRelationAttackPoint;////jks 2015.8.26 no more:  + leaderBuffAttackPoint;//jks 2015.5.8 remove leader strategy-	 + leaderStrategyAttack;

			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
			{
				Log.print_always("   현재 리더 클래스: None ");
				Log.print_always("   공격자 : " + gameObject.name + "  -->  피해자: " + knowledgeOpponent.name);
				Log.print_always("   공격자 클래스 : " + Class + "  -->  피해자 클래스: " + knowledgeOpponent.Class + "   피격 타입: " + hitType);
				Log.print_always("   G I V E  D A M A G E      Original: "+ AttackPoint +   " = " + finalAttack);
			}
			#endif
		}
		else
		{
			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
			{
				Log.print_always("   공격자 : " + gameObject.name + "  -->  피해자: " + knowledgeOpponent.name);
				Log.print_always("   피격 타입: " + hitType);
				Log.print_always("   G I V E  D A M A G E     : " + finalAttack);
			}
			#endif
		}



		classRelTestFunc(knowledgeOpponent);
		knowledgeOpponent.takeDamage(finalAttack, hitReactionAnimID, hitType, AttackType, _weaponType_ForAnimation, gameObject, reactionDistanceOverride);

	}



}
