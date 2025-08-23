using System.Collections;
using System.Collections.Generic;
using UnityEngine;



//public enum eClassRelationApplicationType
//{
//	CRAT_ShortDistance = 0,
//	CRAT_LongDistance = 1,
//	CRAT_LeaderBuff = 2,
//}

//jks 2015.8.27 no more leader buff.
//public enum eSubjectType
//{
//	ST_Self = 0,
//	ST_Opponent = 1
//}


public enum eAttackerClassVariation
{
	// index for melee skills
	ACV_Seacher = 1,
	ACV_Fisherman = 2,
	ACV_Lighthouse = 3,
	ACV_Waveuser = 4,
	ACV_Spearuser = 5,
	ACV_RaidBoss = 6,
	ACV_Max = 7,

//	// index for long distance skills
//	AV_Seacher_long = 6,
//	AV_Fisherman_long = 7,
//	AV_Lighthouse_long = 8,
//	AV_Waveuser_long = 9,
//	AV_Waveuser_long_sinsu = 10,
//	AV_Spearuser_long = 11,
//	AV_Spearuser_long_sinsu = 12,
}




public static class CharacterClassRelation
{

	static Dictionary<string, int> _dic_critical = new Dictionary<string, int> ();

	static Dictionary<string, int> _dic_attack = new Dictionary<string, int> ();

	static bool isInitilized = false;
	

	static CardClass getAttackerClass(int classID)
	{
		if (classID == (int)eAttackerClassVariation.ACV_Seacher) 
			return CardClass.CLASS_SEARCHER;

		if (classID == (int)eAttackerClassVariation.ACV_Fisherman) 
			return CardClass.CLASS_FISHERMAN;

		if (classID == (int)eAttackerClassVariation.ACV_Lighthouse) 
			return CardClass.CLASS_LIGHTHOUSE_KEEPER;

		if (classID == (int)eAttackerClassVariation.ACV_Waveuser) 
			return CardClass.CLASS_WAVE_USER;

		if (classID == (int)eAttackerClassVariation.ACV_Spearuser) 
			return CardClass.CLASS_SPEAR_USER;

		if (classID == (int)eAttackerClassVariation.ACV_RaidBoss) 
			return CardClass.CLASS_RAID_BOSS;


		Debug.LogError("Wrong character class ID: " + classID);

		return CardClass.CLASS_NONE;
	}

	public static void initDictionary()
	{
		if (isInitilized) return;

		loadClassRelationInfoFromTable();
//jks 2015.8.27 no more leader buff.    loadLeaderBuffInfoFromTable();
		isInitilized = true;
	}


	static void loadClassRelationInfoFromTable()
	{
		int convertedID;
		int attackRate;
		int defenseRate;
//		eAttackType victimAttackType;

		int number_class = ((int)eAttackerClassVariation.ACV_Max)-1;
		for (int classID=1; classID <= number_class; classID++)
		{
			convertedID = TableManager.getConvertedID(TABLE.TABLE_CLASS_RELATION, classID);
			Table_ClassRelation table = (Table_ClassRelation)TableManager.GetContent ( convertedID ); 


//			//jks load melee change info
//			if (classID <= (int)eAttackerClassVariation.AV_Spearuser) // 5
//			{
//				attackType = eAttackType.AT_Sword; //jks represent melee skill
//			}
//			// long distance
//			else
//			{
//				if (classID == (int)eAttackerClassVariation.AV_Spearuser_long_sinsu || classID == (int)eAttackerClassVariation.AV_Waveuser_long_sinsu)
//				{
//					attackType = eAttackType.AT_Sinsu; //jks represent long distance skill
//				}
//				else
//				{
//					attackType = eAttackType.AT_Arrow; //jks represent long distance skill
//				}
//			}

//			victimAttackType = eAttackType.AT_Sword;

			if (table != null)
			{
				defenseRate = table._searcher_defense;  		addToDictionary(classID, CardClass.CLASS_SEARCHER, defenseRate, _dic_critical);
				attackRate = table._searcher_attack;  			addToDictionary(classID, CardClass.CLASS_SEARCHER, attackRate, _dic_attack);

				defenseRate = table._fisherman_defense;  		addToDictionary(classID, CardClass.CLASS_FISHERMAN, defenseRate, _dic_critical);
				attackRate = table._fisherman_attack;  			addToDictionary(classID, CardClass.CLASS_FISHERMAN, attackRate, _dic_attack);

				defenseRate = table._lighthouse_defense;  		addToDictionary(classID, CardClass.CLASS_LIGHTHOUSE_KEEPER, defenseRate, _dic_critical);
				attackRate = table._lighthouse_attack;  		addToDictionary(classID, CardClass.CLASS_LIGHTHOUSE_KEEPER, attackRate, _dic_attack);

				defenseRate = table._waveuser_defense;  		addToDictionary(classID, CardClass.CLASS_WAVE_USER, defenseRate, _dic_critical);
				attackRate = table._waveuser_attack;  			addToDictionary(classID, CardClass.CLASS_WAVE_USER, attackRate, _dic_attack);

				defenseRate = table._spearuser_defense;  		addToDictionary(classID, CardClass.CLASS_SPEAR_USER, defenseRate, _dic_critical);
				attackRate = table._spearuser_attack;  			addToDictionary(classID, CardClass.CLASS_SPEAR_USER, attackRate, _dic_attack);

				defenseRate = table._raidboss_defense;  		addToDictionary(classID, CardClass.CLASS_RAID_BOSS, defenseRate, _dic_critical);
				attackRate = table._raidboss_attack;  			addToDictionary(classID, CardClass.CLASS_RAID_BOSS, attackRate, _dic_attack);
			}


//			victimAttackType = eAttackType.AT_Sinsu;
//
//			defenseRate = table._waveuser_sinsu_defense;  	addToDictionary(attackType, victimAttackType, classID, CardClass.CLASS_WAVE_USER, defenseRate, eSubjectType.ST_Self, _dic_critical);
//			attackRate = table._waveuser_sinsu_attack;  	addToDictionary(attackType, victimAttackType, classID, CardClass.CLASS_WAVE_USER, attackRate, eSubjectType.ST_Self, _dic_attack);
//			
//			defenseRate = table._spearuser_sinsu_defense;  	addToDictionary(attackType, victimAttackType, classID, CardClass.CLASS_SPEAR_USER, defenseRate, eSubjectType.ST_Self, _dic_critical);
//			attackRate = table._spearuser_sinsu_attack;  	addToDictionary(attackType, victimAttackType, classID, CardClass.CLASS_SPEAR_USER, attackRate, eSubjectType.ST_Self, _dic_attack);
		}


		//jks load attack change info
	}

//jks 2015.8.27 no more leader buff.
//	static void loadLeaderBuffInfoFromTable()
//	{
//		int convertedID;
//		int attackRate;
//		int criticalRate;
//
//		
//		for (int classID=1; classID <= (int)eAttackerClassVariation.AV_Spearuser; classID++)
//		{
//			convertedID = TableManager.getConvertedID(TABLE.TABLE_BUFF, classID);
//			Table_Buff table = (Table_Buff)TableManager.GetContent ( convertedID ); 
//			
//		
//			criticalRate = table._searcher_critical_self;  		addToDictionaryBuff(classID, CardClass.CLASS_SEARCHER, criticalRate, eSubjectType.ST_Self, _dic_critical);
//			criticalRate = table._searcher_critical_opponent;  	addToDictionaryBuff(classID, CardClass.CLASS_SEARCHER, criticalRate, eSubjectType.ST_Opponent, _dic_critical);
//			attackRate = table._searcher_attack_self;  			addToDictionaryBuff(classID, CardClass.CLASS_SEARCHER, attackRate, eSubjectType.ST_Self, _dic_attack);
//			attackRate = table._searcher_attack_opponent;  		addToDictionaryBuff(classID, CardClass.CLASS_SEARCHER, attackRate, eSubjectType.ST_Opponent, _dic_attack);
//			
//			criticalRate = table._fisherman_critical_self;  	addToDictionaryBuff(classID, CardClass.CLASS_FISHERMAN, criticalRate, eSubjectType.ST_Self, _dic_critical);
//			criticalRate = table._fisherman_critical_opponent; 	addToDictionaryBuff(classID, CardClass.CLASS_FISHERMAN, criticalRate, eSubjectType.ST_Opponent, _dic_critical);
//			attackRate = table._fisherman_attack_self;  		addToDictionaryBuff(classID, CardClass.CLASS_FISHERMAN, attackRate, eSubjectType.ST_Self, _dic_attack);
//			attackRate = table._fisherman_attack_opponent;  	addToDictionaryBuff(classID, CardClass.CLASS_FISHERMAN, attackRate, eSubjectType.ST_Opponent, _dic_attack);
//			
//			criticalRate = table._lighthouse_critical_self;  	addToDictionaryBuff(classID, CardClass.CLASS_LIGHTHOUSE_KEEPER, criticalRate, eSubjectType.ST_Self, _dic_critical);
//			criticalRate = table._lighthouse_critical_opponent; addToDictionaryBuff(classID, CardClass.CLASS_LIGHTHOUSE_KEEPER, criticalRate, eSubjectType.ST_Opponent, _dic_critical);
//			attackRate = table._lighthouse_attack_self;  		addToDictionaryBuff(classID, CardClass.CLASS_LIGHTHOUSE_KEEPER, attackRate, eSubjectType.ST_Self, _dic_attack);
//			attackRate = table._lighthouse_attack_opponent;  	addToDictionaryBuff(classID, CardClass.CLASS_LIGHTHOUSE_KEEPER, attackRate, eSubjectType.ST_Opponent, _dic_attack);
//			
//			criticalRate = table._waveuser_critical_self;  		addToDictionaryBuff(classID, CardClass.CLASS_WAVE_USER, criticalRate, eSubjectType.ST_Self, _dic_critical);
//			criticalRate = table._waveuser_critical_opponent; 	addToDictionaryBuff(classID, CardClass.CLASS_WAVE_USER, criticalRate, eSubjectType.ST_Opponent, _dic_critical);
//			attackRate = table._waveuser_attack_self;  			addToDictionaryBuff(classID, CardClass.CLASS_WAVE_USER, attackRate, eSubjectType.ST_Self, _dic_attack);
//			attackRate = table._waveuser_attack_opponent;  		addToDictionaryBuff(classID, CardClass.CLASS_WAVE_USER, attackRate, eSubjectType.ST_Opponent, _dic_attack);
//			
//			criticalRate = table._spearuser_critical_self;  	addToDictionaryBuff(classID, CardClass.CLASS_SPEAR_USER, criticalRate, eSubjectType.ST_Self, _dic_critical);
//			criticalRate = table._spearuser_critical_opponent; 	addToDictionaryBuff(classID, CardClass.CLASS_SPEAR_USER, criticalRate, eSubjectType.ST_Opponent, _dic_critical);
//			attackRate = table._spearuser_attack_self;  		addToDictionaryBuff(classID, CardClass.CLASS_SPEAR_USER, attackRate, eSubjectType.ST_Self, _dic_attack);
//			attackRate = table._spearuser_attack_opponent;  	addToDictionaryBuff(classID, CardClass.CLASS_SPEAR_USER, attackRate, eSubjectType.ST_Opponent, _dic_attack);
//
//		}
//	}

	
	static void addToDictionary(int classID, CardClass victimClass, int value, Dictionary<string, int> dictionaryToUse)
	{
		CardClass attackerClass = getAttackerClass(classID);

		string key = getKey(attackerClass, victimClass);

		//Log.jprint("............. addToDictionary ............. Key: " + key + " value: " + value);

		
		// add to critical dictionary
		dictionaryToUse.Add(key, value);
	}
	
	
	
	//jks 2015.8.27 no more leader buff.
//	static void addToDictionaryBuff(int classID, CardClass victimClass, int value, eSubjectType subject, Dictionary<string, int> dictionaryToUse)
//	{
//		CardClass leaderClass = getAttackerClass(classID);
//		
//		string key = getKey(subject, leaderClass, victimClass);
//		
//		// add to critical dictionary
//		dictionaryToUse.Add(key, value);
//
////		if (subject == eSubjectType.ST_Self)
////			Log.jprint(">>> ~~~~~~~~~~~~~~~~ Buff Attack Self: Key: " + key + "    value: " + value);
//	}

	


//	static bool isLongDistance(eAttackType skillType)
//	{
//		return 
//		skillType == eAttackType.AT_Arrow || 
//		skillType == eAttackType.AT_Bullet ||
//		skillType == eAttackType.AT_Sinsu ||
//		skillType == eAttackType.AT_Meteor ||
//		skillType == eAttackType.AT_Stun;
//	}
	
	//jks for attack
	static string getKey(CardClass class_attacker, CardClass class_victim)
	{
		int attacker = (int)class_attacker;
		int victim = (int)class_victim;
//		int is_sinsu_skill = attacker_skill_type == eAttackType.AT_Sinsu ? 1 : 0;
//		int is_victim_skill_sinsu = victim_skill_type == eAttackType.AT_Sinsu ? 1 : 0;
//		int is_buff = attacker_skill_type == eAttackType.AT_BUFF ? 1 : 0;

//		eClassRelationApplicationType applicationType;
//		
//		if (isLongDistance(attacker_skill_type))
//		{
//			applicationType = eClassRelationApplicationType.CRAT_LongDistance;
//		}
//		else
//		{
//			applicationType = eClassRelationApplicationType.CRAT_ShortDistance;
//		}

//		string key = applicationType.ToString() + 
//					 attacker.ToString() + victim.ToString() + 
//					 is_sinsu_skill.ToString()+ is_victim_skill_sinsu.ToString() + is_buff.ToString();
		string key = attacker.ToString() + victim.ToString();

		return key;
	}



	//jks 2015.8.27 no more leader buff.
//	//jks for buff
//	static string getKey(eSubjectType subject_type, CardClass class_attacker, CardClass class_victim)
//	{
//		int subject = (int)subject_type;
//		int attacker = (int)class_attacker;
//		int victim = (int)class_victim;
//
//		string key = subject.ToString() + attacker.ToString() + victim.ToString();
//		
//		return key;
//	}
	
	

	//jks for skill attack
	public static int getAttackRate(CardClass class_attacker, CardClass class_victim)
	{
		int value;

		string key = getKey (class_attacker, class_victim);

		_dic_attack.TryGetValue(key, out value);


		//Log.jprint("^^^^^^^^^^^^ get attack rate ^^^^^^^^^^^^ Key: " + key + " value: " + value);

		return value;
	}

	//jks for skill attack
	public static int getDefenseRate(CardClass class_attacker, CardClass class_victim)
	{
		int value;
		
		string key = getKey (class_attacker, class_victim);
		
		_dic_critical.TryGetValue(key, out value);
		
		return value;
	}



//	//jks for buff
//	public static int getAttackRate_Buff_Self(CardClass class_leader, CardClass class_victim)
//	{
//		int value;
//		
//		string key = getKey (eSubjectType.ST_Self, class_leader, class_victim);
//		
//		_dic_attack.TryGetValue(key, out value);
//
//		//Log.jprint("~~~~~~~~~~~~~~~~ Buff Attack Self: Key: " + key + "    value: " + value);
//		
//		return value;
//	}
//	
//	//jks for buff
//	public static int getAttackRate_Buff_Opponent(CardClass class_leader, CardClass class_victim)
//	{
//		int value;
//		
//		string key = getKey (eSubjectType.ST_Opponent, class_leader, class_victim);
//		
//		_dic_attack.TryGetValue(key, out value);
//		
//		return value;
//	}
//
//	//jks for buff
//	public static int getCriticalRate_Buff_Self(CardClass class_leader, CardClass class_victim)
//	{
//		int value;
//		
//		string key = getKey (eSubjectType.ST_Self, class_leader, class_victim);
//		
//		_dic_critical.TryGetValue(key, out value);
//		
//		return value;
//	}
//
//	
//	//jks for buff
//	public static int getCriticalRate_Buff_Opponent(CardClass class_leader, CardClass class_victim)
//	{
//		int value;
//		
//		string key = getKey (eSubjectType.ST_Opponent, class_leader, class_victim);
//		
//		_dic_critical.TryGetValue(key, out value);
//		
//		return value;
//	}
	

	
	//jks return 1: stronger, 0: same, -1: weaker
	public static int checkClassRelation(CardClass class_attacker, CardClass class_victim)
	{
		int result = 0;

		int attackTo = getAttackRate(class_attacker, class_victim);
		int attackFrom = getAttackRate(class_victim, class_attacker);

		int defenseTo = getDefenseRate(class_attacker, class_victim);
		int defenseFrom = getDefenseRate(class_victim, class_attacker);

		bool isAttackRateStrong = attackTo > 0 || attackFrom < 0;
		bool isDefenseRateStrong = defenseTo > 0 || defenseFrom < 0;

		bool isAttackRateWeaker = attackTo < 0 || attackFrom > 0;
		bool isDefenseRateWeaker = defenseTo < 0 || defenseFrom > 0;

		if ( isAttackRateStrong || isDefenseRateStrong ) 
			result = 1;
		else if (isAttackRateWeaker || isDefenseRateWeaker)
			result = -1;

		return result;
	}









}
