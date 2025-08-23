using UnityEngine;
using System.Collections;



//jks 리더의 신수를 특정 시간동안 특정 수치만큼 최대치 이하 내에서 올려주는 무기.
public class Weapon_Throw_Sinsu : Weapon_Throw
{

	protected float _increase_rate; // increase rate % per second
	protected float _update_interval = 1;



	public void setAttributes(float rate, float interval)
	{
		_increase_rate = rate;
		_update_interval = interval + 0.01f;
	}
	

	protected override void activateWeapon()
	{
		base.activateWeapon();

		InvokeRepeating("applyWeaponPower", _update_interval, _update_interval);

		if (AllyID == eAllyID.Ally_Human_Me)
			BattleManager.Instance.showLeaderHealEffect(); //jks ui effect
	}


	protected override void applyWeaponPower() 
	{ 
		if (AllyID == eAllyID.Ally_Human_Me)
			BattleBase.Instance.recoverLeaderHP(_increase_rate);
		else if (AllyID == eAllyID.Ally_Human_Opponent)  //jks pvp
			BattleBase.Instance.recoverLeaderHP_P2(_increase_rate);
		else if (AllyID == eAllyID.Ally_Bot_Opponent && BattleBase.Instance.IsRankingTower)  //jks 랭킹탑 적팀.
			BattleBase.Instance.recoverLeaderHP_P2(_increase_rate);
		else //if (AllyID == eAllyID.Ally_Bot_Opponent)
			BattleBase.Instance.recoverBossHP(_increase_rate);
	}


	protected override void createEffect() 
	{
		if (BattleManager.Instance == null) return;

		_effectPrefab = MadPool.Instance().poolInstantiate(_effect_name);

		EffectPosition fxPos = _effectPrefab.GetComponent<EffectPosition>();
		if (fxPos == null)
			fxPos = _effectPrefab.gameObject.AddComponent<EffectPosition>();

		if (fxPos == null) return;
			
		if (_effect_placing == 1)
		{
			fxPos.MyTarget = getTargetCenterTransform();
		}
		else
		{
			fxPos.MyTarget = getTargetTransform();
		}
	}


	protected Transform getTargetCenterTransform()
	{
		Transform center = null;

		if (AllyID == eAllyID.Ally_Human_Me )
		{
			center = BattleManager.Instance.LeaderCenterTransform;
		}
		else if (AllyID == eAllyID.Ally_Human_Opponent)  //jks pvp
		{
			if (BattleBase.Instance.LeaderKnowledge_P2 != null)
				center = BattleBase.Instance.LeaderKnowledge_P2.CenterTransform;
		}
		else if (AllyID == eAllyID.Ally_Bot_Opponent && BattleBase.Instance.IsRankingTower)  //jks 랭킹탑 적팀.
		{
			if (BattleBase.Instance.LeaderKnowledge_P2 != null)
				center = BattleBase.Instance.LeaderKnowledge_P2.CenterTransform;
		}
		else //if (AllyID == eAllyID.Ally_Bot_Opponent)
		{
			if (BattleBase.Instance.FindBoss != null)
				center = BattleBase.Instance.FindBoss.GetComponent<Knowledge_Mortal_Fighter>().CenterTransform;
		}

		return center;
	}


	protected Transform getTargetTransform()
	{
		Transform target = null;

		if (AllyID == eAllyID.Ally_Human_Me )
		{
			target = BattleManager.Instance.LeaderTransform;
		}
		else if (AllyID == eAllyID.Ally_Human_Opponent)  //jks pvp
		{
			if (BattleBase.Instance.LeaderKnowledge_P2 != null)
				target = BattleBase.Instance.LeaderKnowledge_P2.transform;
		}
		else if (AllyID == eAllyID.Ally_Bot_Opponent && BattleBase.Instance.IsRankingTower)  //jks 랭킹탑 적팀.
		{
			if (BattleBase.Instance.LeaderKnowledge_P2 != null)
				target = BattleBase.Instance.LeaderKnowledge_P2.transform;
		}
		else //if (AllyID == eAllyID.Ally_Bot_Opponent)
		{
			if (BattleBase.Instance.FindBoss != null)
				target = BattleBase.Instance.FindBoss.GetComponent<Knowledge_Mortal_Fighter>().transform;
		}

		return target;
	}


}
