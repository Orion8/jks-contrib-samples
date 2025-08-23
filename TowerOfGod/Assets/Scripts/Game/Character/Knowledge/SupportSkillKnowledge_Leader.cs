using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

public class SupportSkillKnowledge_Leader : SupportSkillKnowledge 
{

	public override SupportSkillInfo[] SskillInfo { get { return BattleManager.Instance.SupportSkillInfo; }}

	protected override List<FighterActor> Opponents { get { return BattleBase.Instance.List_Enemy; }}


	bool IsFitClass { get { return Knowledge.Class == BattleBase.Instance.SupportSkillSet_FitClass ; }}

	protected override float Cooltime
	{ 
		get 
		{ 
			if (IsFitClass)
				return SskillInfo[ActiveSkillNum]._coolTime;
			else
				return SskillInfo[ActiveSkillNum]._coolTime + BattleBase.Instance.SupportSkillSet_PenaltyCooltime;
		} 
	}

	public override int AttackPoint 
	{ 	
		get { 
			//jks wisdom 속성 적용.
			ObscuredInt point = SskillInfo[ActiveSkillNum]._attackPoint + Mathf.RoundToInt(SskillInfo[ActiveSkillNum]._attackPoint * Knowledge._boostSupportAndQuickSkill);

			//jks 클래스 적합도 적용.
			if (IsFitClass)
				return point;
			else
				return point - Mathf.RoundToInt(BattleBase.Instance.SupportSkillSet_PenaltyAttack * (float)point);
		} 
	}


	protected override void initVariables()
	{
//		_supportSkillInfo = new PerSkillStatus[3];

	}


	public override void startSkill_Support(int skillNum)
	{
		if (Knowledge.IsLeader && canUseSkill_Support(skillNum))
		{
			base.startSkill_Support(skillNum);

			processCooltime();
			BattleBase.Instance.showUICoolTime_SupportSkill(skillNum, Cooltime);

			StartCoroutine(resetCooltime_CheckIfCooltimeIsNotStarted(skillNum));  //jks 2016.4.28

		}
	}


	//jks 2016.4.28 : 리더 지원스킬 사용 후 쿨타임 시작하지 않는 경우(스킬 종료되기 전에 상태 변경으로 action finish 이벤트 오지 않는 경우.) 대한 처리.
	protected IEnumerator resetCooltime_CheckIfCooltimeIsNotStarted(int skillNum)
	{
		//jks 포즈나 연출 중이면 기다림.
		while (! BattleBase.Instance.IsBattleInProgress || BattleBase.Instance.IsIgnoreButtonTouch || 
			BattleBase.Instance.IsDialogShowOff || Time.timeScale == 0) 
			yield return null;

		//jks 스킬 터치 후 4 초가 지나면 확인.
		yield return new WaitForSeconds(4);

		//jks 스킬 중이면 무시.
		if (isActionProgress(skillNum)) yield break;

		processCooltime(skillNum);

	}




	public override ObscuredFloat calcHitRate(Knowledge_Mortal_Fighter opponent)
	{
		ObscuredFloat hitRate;
		//jks 2015.8.27 		float buffHitRate = Knowledge.calculate_LeaderBuff_HitRate_Self(opponent.Class);

		hitRate = calcHitRate_Base(opponent);//jks 2015.8.27  + buffHitRate; //jks 2015.5.8 remove leader strategy-	 + leaderStrategy;


		ObscuredFloat hitRateBoost = 0, hitRateBoost_Buff = 0;

		Knowledge_Mortal_Fighter_Main knowMain = (Knowledge_Mortal_Fighter_Main) Knowledge;

		hitRateBoost = hitRate * knowMain.getHitRateBoost_ByContinuousQuickComboCount();

		hitRateBoost_Buff = Knowledge.getLeaderBuffCriticalUp(hitRate);

		hitRate += hitRateBoost + hitRateBoost_Buff;

		//Log.jprint("********* calcHitRate()    buff: "+ buffHitRate+"    leader strategy: "+leaderStrategy);

		#if UNITY_EDITOR
		if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
		{
			//jks 2015.8.26 no more leader buff:    Log.print(" 팀원 : " + gameObject.name +  " 추가 적중률 ->   버프: "+buffHitRate  + " = " + "  최종 적중률: " + hitRate);
			Log.print_always(" 팀원(지원스킬) : " + gameObject.name +  " 추가 적중률 ->   평타연속콤보: "+hitRateBoost +  "+ 리더버프: "+hitRateBoost_Buff  + " = " + "  최종 적중률: " + hitRate);
		}
		#endif



		return hitRate;
	}




	protected override void giveDamage_Melee(float reactionDistanceOverride)
	{			

		GameObject closestOpponent = getOpponentsInScanDistance_WeaponPositionBased();
		if (closestOpponent == null) return;

		//jks 2016.3.14 skill buff 기능 추가.
		addSkillBuff();

		Knowledge_Mortal opponentKnowledge = closestOpponent.GetComponent<Knowledge_Mortal>();


		float finalDistToCheck = Mathf.Abs(transform.position.x - closestOpponent.transform.position.x) - Knowledge.Radius - opponentKnowledge.Radius - Info[ActiveSkillNum]._weaponLength;

		if (finalDistToCheck > 0.3f)
			return;

		foreach(FighterActor ea in Opponents)
		{
			GameObject go = ea._go;

			if (go == null) continue;
			if (!go.activeSelf) continue;

			//jks check damage range
			opponentKnowledge = go.GetComponent<Knowledge_Mortal>();
			if (opponentKnowledge == null) continue;


			finalDistToCheck = Mathf.Abs(transform.position.x - go.transform.position.x) - Knowledge.Radius - opponentKnowledge.Radius - Info[ActiveSkillNum]._weaponLength;


			if (go != Knowledge.getCurrentTarget())
			{
				if (finalDistToCheck > 0.1f + Info[ActiveSkillNum]._damageRange) 
					continue;
			}

			if (Knowledge.checkHeightIfReachable(go) == false) continue;

			Knowledge_Mortal_Fighter knowledgeOpponent = go.GetComponent<Knowledge_Mortal_Fighter>();


			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
			{
				Log.print_always("   --------------------------------- 리더 지원 Skill ---------------------------------");
			}
			#endif

			eHitType hitType = getFinalHitType(knowledgeOpponent);

			int hitReactionAnimID = getReaction(hitType);

			ObscuredInt finalAttack = AttackPoint;

			if (TestOption.Instance()._classRelationBuffAll)
			{
				ObscuredInt classRelationAttackPoint = Knowledge.calculate_ClassRelation_AttackPoint(AttackPoint, knowledgeOpponent);

				finalAttack = AttackPoint + classRelationAttackPoint;//jks 2015.8.26 no more:  + leaderBuffAttackPoint;//jks 2015.5.8 remove leader strategy-	 + leaderStrategyAttack;


				#if UNITY_EDITOR
				if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
				{
					Log.print_always("   공격자 : " + gameObject.name + "  -->  피해자: " + knowledgeOpponent.name);
					Log.print_always("   공격자 클래스 : " + Knowledge.Class + "  -->  피해자 클래스: " + knowledgeOpponent.Class);
					Log.print_always("   G I V E  D A M A G E      기본 공격력: " + AttackPoint + "  +  클래스상성 공격력: " + classRelationAttackPoint +  "  =  " + finalAttack);
				}
				#endif
			}
			else
			{
				#if UNITY_EDITOR
				if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
				{
					Log.print_always("   G I V E  D A M A G E      기본 공격력: " + finalAttack);
				}
				#endif
			}

			Knowledge_Mortal_Fighter_Main knowMain = (Knowledge_Mortal_Fighter_Main) Knowledge;
			BattleBase.Instance.incrementHitTypeCount(knowMain.CardDeckIndex, hitType);

			Knowledge.SkillType = eSkillType.ST_Support;

			knowledgeOpponent.takeDamage(finalAttack, hitReactionAnimID, hitType, Info[ActiveSkillNum]._attackType, Info[ActiveSkillNum]._weaponType_ForAnimation, gameObject, reactionDistanceOverride);
		}

	}




	public void doNextAction()
	{
		setNextComboActionFlag();

		if (NoNextComboAction) //jks if no next action,
		{
			processOnSSkillEnd();
			processCooltime();
		}
	}

	public void processOnSSkillEnd()
	{
		forceResetComboFlags();

		if (Knowledge.AmIAuto)
		{
			Knowledge.Action_Run = true;
		}
		else
		{
			Knowledge.Action_Run = false;
			Knowledge.Action_Walk = false;
			Knowledge.Action_Idle = true;
		}
	}

	public void processCooltime()
	{
		if (_everGaveDamageForTheAttack[ActiveSkillNum])
		{
			//Log.jprint(gameObject.name + " C O O L T I M E  S T A R T ........... ");				
			startCoolTimeAndResetComboFlag(ActiveSkillNum);
			_everGaveDamageForTheAttack[ActiveSkillNum] = false;
		}
		else
		{
			resetCoolTimer(ActiveSkillNum);
		}
	}


	public void processCooltime(int skillNum)
	{
		if (_everGaveDamageForTheAttack[skillNum])
		{
			//Log.jprint(gameObject.name + " C O O L T I M E  S T A R T ........... ");				
			startCoolTimeAndResetComboFlag(skillNum);
			_everGaveDamageForTheAttack[skillNum] = false;
		}
		else
		{
			resetCoolTimer(skillNum);
		}
	}


	public void resetActionInfo()
	{
		if (IsFinalCombo)
			hideSSWeaponAndShowDefault();

		forceResetComboFlags();

		if (Knowledge.IsDead)
		{
			Knowledge.Action_Death = true;
		}
		else
		{
			//jks after victory action, goto postVictory for "idle" animation
			if ((Knowledge.IsBattleVictorious && Knowledge.AllyID == eAllyID.Ally_Human_Me) ||
				(Knowledge.IsBattleFailed && Knowledge.AllyID != eAllyID.Ally_Human_Me))
			{
				Knowledge.Action_PostVictory = true;
			}

			doNextAction();
		}

		//jsm_0912 - hide trail
		Knowledge.setTrail(false);

	}


}
