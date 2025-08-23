using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class LeaderStrategy
{

//jks 2015.5.8 remove leader strategy-	

//	static Dictionary<string, int> _dic_values = new Dictionary<string, int> ();
//
//	static bool isInitilized = false;
//
//
//	public enum eValueType
//	{
//		VT_Attack = 4,
//		VT_Critical = 5,
//		VT_SinsuUseup = 6,
//		VT_SinsuRecovery = 7,		
//	}
//
//
//	public static void initDictionary()
//	{
//		if (isInitilized) return;
//		
//		loadLeaderStrategyInfoFromTable();
//		isInitilized = true;
//	}
//	
//
//
//
//	static void loadLeaderStrategyInfoFromTable()
//	{
//		int convertedID;
//		int value;
//
//
//		for (int sIndex=1; sIndex <= (int)eLeaderStrategy.LS_AllOut; sIndex++)
//		{
//			convertedID = TableManager.getConvertedID(TABLE.TABLE_LEADER_STRATEGY, sIndex);
//			Table_LeaderStrategy table = (Table_LeaderStrategy)TableManager.GetContent ( convertedID ); 
//
//			value = table._leader_attack;  		addToDictionary(sIndex, eSubjectType.ST_Self, eValueType.VT_Attack, value);
//			value = table._leader_critical;  	addToDictionary(sIndex, eSubjectType.ST_Self, eValueType.VT_Critical, value);
//
//			value = table._enemy_attack;  		addToDictionary(sIndex, eSubjectType.ST_Opponent, eValueType.VT_Attack, value);
//			value = table._enemy_critical;  	addToDictionary(sIndex, eSubjectType.ST_Opponent, eValueType.VT_Critical, value);
//
//			value = table._move_sinsu_useup;  	addToDictionary(sIndex, eSubjectType.ST_Self, eValueType.VT_SinsuUseup, value);
//
//			value = table._team_sinsu_recovery; addToDictionary(sIndex, eSubjectType.ST_Self, eValueType.VT_SinsuRecovery, value);
//
//		}
//
//	}
//
//
//	static string getKey(eLeaderStrategy strategy_type, eSubjectType subject_type, eValueType value_type)
//	{
//		int strategy = (int)strategy_type;
//		int subject = (int)subject_type;
//		int value_t = (int)value_type;
//		
//		string key = strategy.ToString() + subject.ToString() + value_t.ToString();
//		
//		return key;
//	}
//
//
//	static void addToDictionary(int strategyID, eSubjectType subject, eValueType valueType, int value)
//	{
//		
//		string key = getKey((eLeaderStrategy)strategyID, subject, valueType);
//		
//		// add to critical dictionary
//		_dic_values.Add(key, value);
//
//	}
//	
//
//	public static int getLeaderStrategyValues(eLeaderStrategy strategyType, eSubjectType subject, eValueType valueType)
//	{
//		int value;
//		string key = getKey(strategyType, subject, valueType);
//
//		_dic_values.TryGetValue(key, out value);
//
//		return value;
//	}
//
//
//	public static int getSinsuUseup(eLeaderStrategy strategyType)
//	{
//		int value;
//		string key = getKey(strategyType, eSubjectType.ST_Self, eValueType.VT_SinsuUseup);
//		
//		_dic_values.TryGetValue(key, out value);
//		
//		return value;
//	}
//	
//
//	public static int getSinsuRecovery(eLeaderStrategy strategyType)
//	{
//		int value;
//		string key = getKey(strategyType, eSubjectType.ST_Self, eValueType.VT_SinsuRecovery);
//		
//		_dic_values.TryGetValue(key, out value);
//		
//		return value;
//	}
	




}
