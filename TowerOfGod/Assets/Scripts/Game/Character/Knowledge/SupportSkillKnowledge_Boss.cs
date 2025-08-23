using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

public class SupportSkillKnowledge_Boss : SupportSkillKnowledge
{

	private static readonly byte one = (byte)0x01;


	#region Have SSkill flag
	protected byte _haveSSkill = 0;
	protected void setHaveSSkill(int index) { _haveSSkill |= (byte)(one << index); }
	protected void resetHaveSSkill() { _haveSSkill = 0; }
	public bool haveSSkill(int index) { return (_haveSSkill & (one << index)) > 0; }
	#endregion


	public override bool Progress_AnyAction { get 
		{ return 
			(Info[0] != null && Info[0]._mask_combo > 0) || 
			(Info[1] != null && Info[1]._mask_combo > 0) || 
			(Info[2] != null && Info[2]._mask_combo > 0) ||
			(Info[3] != null && Info[3]._mask_combo > 0) || 
			(Info[4] != null && Info[4]._mask_combo > 0) || 
			(Info[5] != null && Info[5]._mask_combo > 0);} }


	public override bool Progress_AnySkillAnimation { get 
		{ return
			(Info[0] != null && Info[0]._mask_combo_anim > 0) || 
			(Info[1] != null && Info[1]._mask_combo_anim > 0) || 
			(Info[2] != null && Info[2]._mask_combo_anim > 0) ||
			(Info[3] != null && Info[3]._mask_combo_anim > 0) || 
			(Info[4] != null && Info[4]._mask_combo_anim > 0) || 
			(Info[5] != null && Info[5]._mask_combo_anim > 0);} }



	protected override List<FighterActor> Opponents { get { return BattleBase.Instance.List_Ally; }}



	public override int AttackPoint 
	{ 
		get
		{
			ObscuredInt distributedAttackPoint = Knowledge._attackPoint;

			if (DamageFrequency < 0) //Info[ActiveSkillNum] == null
				return -1;

			if (DamageFrequency > 1)// if multiple attack skill?
			{
				distributedAttackPoint = Mathf.RoundToInt(Knowledge._attackPoint / DamageFrequency);  //jks 2015.11.23:  new calc
				distributedAttackPoint += Mathf.RoundToInt(distributedAttackPoint * BattleTuning.Instance._multipleAttackSkillAdjustmentFactor);
			}

			return distributedAttackPoint;
		}
	}


	public int DamageFrequency 
	{
		get 
		{ 
			int frequency = 1; 

			if (Info[ActiveSkillNum] == null) return -1;
						
			if (Info[ActiveSkillNum]._damage_frequency > 0)
			{
				frequency = Info[ActiveSkillNum]._damage_frequency; 
			}

			return frequency;
		} 
	}


	protected override void initVariables()
	{
//		_supportSkillInfo = new PerSkillStatus[6];

	}



	public override void startSkill_Support(int skillNum)
	{
		if (! Knowledge.CoolTimer.IsCoolingInProgress)
		{
//			Log.jprint(Time.time + "   startSkill_Support()    skillNum: "+ skillNum 
//				+ "   Cooltime: " + ((Knowledge_Mortal_Fighter_Bot)Knowledge).CurrentPattern_Cooltime
//				+ "   Repeat: " + ((Knowledge_Mortal_Fighter_Bot)Knowledge).CurrentPattern_Repeat);
			base.startSkill_Support(skillNum);
		}
	}




	public override ObscuredFloat calcHitRate(Knowledge_Mortal_Fighter opponent)
	{
		float hitRate;

		hitRate = calcHitRate_Base(opponent);

		#if UNITY_EDITOR
		if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
		{
			Log.print_always(" 적 보스 (패턴스킬) : " + gameObject.name + "  최종 적중률: " + hitRate);
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


		float finalDistToCheck = Mathf.Abs(transform.position.x - closestOpponent.transform.position.x) 
			- Knowledge.Radius - opponentKnowledge.Radius - Info[ActiveSkillNum]._weaponLength;

		if (finalDistToCheck > 0.3f)
			return;


		foreach(FighterActor ca in Opponents)
		{
			giveDamageTo_Melee(ca._go, finalDistToCheck, reactionDistanceOverride);
		}

		if (BattleBase.Instance.Protectee != null)
		{
			giveDamageTo_Melee(BattleBase.Instance.Protectee, finalDistToCheck, reactionDistanceOverride);
		}
	}




	private void giveDamageTo_Melee(GameObject go, float finalDistToCheck, float reactionDistanceOverride)
	{			

		{
			if (go == null) return;
			if (!go.activeSelf) return;

			//jks check damage range
			Knowledge_Mortal opponentKnowledge = go.GetComponent<Knowledge_Mortal>();
			if (opponentKnowledge == null) return;


			finalDistToCheck = Mathf.Abs(transform.position.x - go.transform.position.x) 
								- Knowledge.Radius - opponentKnowledge.Radius - Info[ActiveSkillNum]._weaponLength;


			if (go != Knowledge.getCurrentTarget())
			{
				if (finalDistToCheck > 0.1f + Info[ActiveSkillNum]._damageRange) 
					return;
			}

			if (Knowledge.checkHeightIfReachable(go) == false) return;

			Knowledge_Mortal_Fighter knowledgeOpponent = go.GetComponent<Knowledge_Mortal_Fighter>();


			#if UNITY_EDITOR
			if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
			{
				Log.print_always("   --------------------------------- 적 보스 Pattern Skill ---------------------------------");
			}
			#endif

			eHitType hitType = getFinalHitType(knowledgeOpponent);

			int hitReactionAnimID = getReaction(hitType);

			ObscuredInt finalAttack = AttackPoint;

			if (knowledgeOpponent.IsLeader || TestOption.Instance()._classRelationBuffAll)
			{
				ObscuredInt classRelationAttackPoint = Knowledge.calculate_ClassRelation_AttackPoint(AttackPoint, knowledgeOpponent);

				finalAttack = AttackPoint + classRelationAttackPoint;//jks 2015.8.26 no more:  + leaderBuffAttackPoint;//jks 2015.5.8 remove leader strategy-	 + leaderStrategyAttack;


				#if UNITY_EDITOR
				if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
				{
					//			Log.print(gameObject.name + ": giveDamage(); hitType : " + hitType + "    reactionAnimID : " + hitReactionAnimID);
					//Log.print("   --------------------------------- 팀원 ---------------------------------");
					if (BattleBase.Instance.LeaderTransform)
						Log.print_always("   현재 리더 클래스: "+ BattleBase.Instance.LeaderClass + "   :  " + BattleBase.Instance.LeaderTransform.gameObject.name);
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
					Log.print_always("   공격자 : " + gameObject.name + "  -->  피해자: " + knowledgeOpponent.name);
					Log.print_always("   피격 타입: " + hitType);
					Log.print_always("   G I V E  D A M A G E      기본 공격력: " + finalAttack);
				}
				#endif
			}


			Knowledge.SkillType = eSkillType.ST_Support;

			knowledgeOpponent.takeDamage(finalAttack, hitReactionAnimID, hitType, Info[ActiveSkillNum]._attackType, Info[ActiveSkillNum]._weaponType_ForAnimation, gameObject, reactionDistanceOverride);
		}

	}

	#region weapon

	protected GameObject[,] _weaponObjects_PatternSkill = new GameObject[6,2];  //jks 6 possible pattern skill with weapon, if pair weapon, need to have two space for each.
	public GameObject[,] WeaponObjects_PatternSkill { get { return _weaponObjects_PatternSkill; } }

	public override GameObject WeaponObject	{ get { return WeaponObjects_PatternSkill[ActiveSkillNum,0]; } }

	public override GameObject getWeaponObject(int skillNum)
	{ 
		return WeaponObjects_PatternSkill[skillNum,0]; 
	}


	protected IEnumerator instantiateWeapons_PatternSkill(Table_AIAttack tableAIAttack)
	{
		if (tableAIAttack._patternSequence == 0)
		{
			yield break;
		}

		Table_Skill tbl_skill;
		for (int k=0; k < tableAIAttack._numPatternSkill; k++)
		{
			tbl_skill = (Table_Skill)TableManager.GetContent ( tableAIAttack._bossPatternInfo[k]._skillID ); 
			yield return StartCoroutine(instantiateWeapon_PatternSkill(tbl_skill._assetPathID_Weapon, k));
		}
	}


	protected IEnumerator instantiateWeapon_PatternSkill(int weaponPathID, int skillNum)
	{
		if (weaponPathID == 0) yield break; //jks means no weapon


		GameObject weaponObj = null;

		//jks instantiate main weapon
		Table_Path tablePath = (Table_Path)TableManager.GetContent(weaponPathID);
		yield return StartCoroutine(ResourceManager.co_InstantiateFromBundle(tablePath._assetPath, result => weaponObj = (GameObject)result));

		if (weaponObj == null) Debug.LogError("Can't find asset : "+ tablePath._assetPath);

		WeaponObjects_PatternSkill[skillNum,0] = weaponObj;


		Weapon_Hold weaponHold = weaponObj.GetComponent<Weapon_Hold>();
		if (weaponHold == null)  //jks throw 타입 경우 예) 폭탄.  는 weapon script 없을 수 있음.
		{
			Debug.LogWarning("Can't find Weapon script on (throw 타입 무기 경우는 없을 수 있음.) "+ tablePath._assetPath);
			yield break;
		}


		//jks instantiate pair weapon
		if (weaponHold.IsPairWeapon) 
		{
			yield return StartCoroutine(ResourceManager.co_InstantiateFromBundle(tablePath._assetPath, obj => weaponObj = (GameObject)obj));
			if (weaponObj == null) Debug.LogError("Can't find asset : "+ tablePath._assetPath);

			weaponObj.tag = "Untagged";

			((Weapon_Hold_Sword_Twin)weaponHold).setPairSwordGo = weaponObj;

			WeaponObjects_PatternSkill[skillNum,1] = weaponObj;

			//jks - the pair item is just for visual. so remove script component if it has one.
			Weapon script = weaponObj.GetComponent<Weapon>();
			if (script != null)
			{
				Destroy(script);
			}		
		}

		weaponHold.show(false);
	}



	protected override void hideSSWeaponAll()
	{
		base.hideSSWeaponAll();

		hideSSWeapon(3);
		hideSSWeapon(4);
		hideSSWeapon(5);
	}


	public override void uninstallWeaponAll()
	{
		base.uninstallWeaponAll();

		uninstallWeapon(3);
		uninstallWeapon(4);
		uninstallWeapon(5);
	}

	#endregion



	bool _isBossPatternSkillReady = false;
	public bool IsBossPatternSkillReady { get { return _isBossPatternSkillReady; }}

	public IEnumerator setPatternSkillInfoFromTable(Table_AIAttack tableAIAttack)
	{

		if (tableAIAttack._patternSequence == 0)
		{
			resetHaveSSkill();
			yield break;
		}

		yield return StartCoroutine(instantiateWeapons_PatternSkill(tableAIAttack));
			
		Table_Skill tbl_skill;
		for (int k=0; k < tableAIAttack._numPatternSkill; k++)
		{
			if (tableAIAttack._bossPatternInfo[k]._skillID != 0)
			{
				tbl_skill = (Table_Skill)TableManager.GetContent ( tableAIAttack._bossPatternInfo[k]._skillID ); 
				setAttributesFromTable(k, tbl_skill);

//				_perSkillAttribuites[k]._coolTime = tableAIAttack._bossPatternInfo[k]._cool_time;
				setHaveSSkill(k);
			}
		}

		_isBossPatternSkillReady = true;
	}


	public void addLauncher(Table_AIAttack tableAIAttack)
	{
		if (tableAIAttack._patternSequence == 0) return;

		Table_Skill tbl_skill;
		for (int k=0; k < tableAIAttack._numPatternSkill; k++)
		{
			tbl_skill = (Table_Skill)TableManager.GetContent ( tableAIAttack._bossPatternInfo[k]._skillID ); 
			addComponents_PSkill_Launcher(gameObject, tbl_skill);
		}

	}


	public void addComponents_PSkill_Launcher(GameObject go, Table_Skill tbl_skill)
	{
		eAttackType attackType = (eAttackType)tbl_skill._attackType;
		int convertedSkillID = tbl_skill.ID;

		if (attackType == eAttackType.AT_Arrow || attackType == eAttackType.AT_Sinsu || 
			attackType == eAttackType.AT_Bullet)
		{
			if (GetComponent<Launcher_Projectile_SupportSkill>() != null) return; //jks don't add more than one same kind of launcher per characher.

			Launcher_Projectile launcher = go.AddComponent<Launcher_Projectile_SupportSkill>();
			launcher.initPrefabReference(convertedSkillID);
		}
		else if (attackType == eAttackType.AT_Stun)
		{
			if (GetComponent<Launcher_Projectile_SupportSkill_Stun>() != null) return;

			Launcher_Projectile_SupportSkill_Stun launcher = go.AddComponent<Launcher_Projectile_SupportSkill_Stun>();
			launcher.initPrefabReference(convertedSkillID);
		}
		else if (attackType == eAttackType.AT_Homing)
		{
			if (GetComponent<Launcher_Projectile_SupportSkill_Homing>() != null) return;

			Launcher_Projectile_SupportSkill_Homing launcher = go.AddComponent<Launcher_Projectile_SupportSkill_Homing>();
			launcher.initPrefabReference(convertedSkillID);
		}
		else if (attackType == eAttackType.AT_LivingWeapon)
		{
			if (GetComponent<Launcher_LivingWeapon_SupportSkill>() != null) return;

			Launcher_LivingWeapon launcher = go.AddComponent<Launcher_LivingWeapon_SupportSkill>();
			launcher.initPrefabReference(convertedSkillID);
		}
		else if (attackType == eAttackType.AT_Throwing)
		{
			if (GetComponent<Launcher_Throw_SupportSkill>() != null) return;

			Launcher_Throw launcher = go.AddComponent<Launcher_Throw_SupportSkill>();
			launcher.initPrefabReference(convertedSkillID);
		}
	}



	public void resetActionInfo()
	{
		if (IsFinalCombo)
			hideSSWeaponAndShowDefault();

		Knowledge.forceResetFlags();
		forceResetComboFlags();

		if (Knowledge.IsDead)
		{
			Knowledge.Action_Death = true;
		}
		else
		{
			if (Knowledge.IsBattleFailed)
			{
				Knowledge.Action_PostVictory = true;
			}

			doNextAction();
		}

		//jsm_0912 - hide trail
		Knowledge.setTrail(false);
	}


	public override void forceResetComboFlags()
	{
		base.forceResetComboFlags();

		reset_combo_Action(3);
		reset_combo_Action(4);
		reset_combo_Action(5);

		reset_combo_Animation(3);
		reset_combo_Animation(4);
		reset_combo_Animation(5);
	}


	public void doNextAction()
	{
		setNextComboActionFlag();

		if (NoNextComboAction) //jks if no next action,
		{
			processOnSSkillEnd();

			if (EverGaveDamage)
			{
				resetComboFlag(ActiveSkillNum);
				EverGaveDamage = false;
			}
		}
	}

	public void processOnSSkillEnd()
	{
		forceResetComboFlags();

		Knowledge.Action_Run = true;
	}


	public void resetComboFlag(int skillNum)
	{
		Knowledge.startCoolTimer();

		Info[skillNum]._skillCompleted = false; // reset
		ComboRecent = 0;// reset
		ComboCurrent = 0;// reset
	}
		



}
