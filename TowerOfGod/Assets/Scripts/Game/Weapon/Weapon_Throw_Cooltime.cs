using UnityEngine;
using System.Collections;


//jks 팀원들의 쿨타임을 특정 시간동안 짧게 해주는 무기.
public class Weapon_Throw_Cooltime : Weapon_Throw
{

	protected float _rate; // shorten _rate % of the cooltime.

	protected Transform[] _effectPrefabs = new Transform[6];

	public void setAttributes(float rate)
	{
		_rate = rate;
	}

	

	protected override void applyWeaponPower() 
	{ 
		BattleBase.Instance.adjustTeamCooltime(_rate, AllyID, getOwner());
	}
	
	

	public override void killMe()
	{
		//jks reset
		BattleBase.Instance.adjustTeamCooltime(0, AllyID, getOwner()); 
		CancelInvoke("createEffect");
		CancelInvoke("updateEffectPosition");


		base.killMe();
	}


	protected override void createEffect() 
	{
		if (BattleBase.Instance == null) return;
		
		BattleBase.Instance.createTeamCooltimeSkillEffect(_effectPrefabs, _effect_name, _effect_placing);
	}
	
	
	protected override void updateEffectPosition() 
	{
		if (BattleBase.Instance != null) return;

		BattleBase.Instance.updateTeamCooltimeSkillEffect(_effectPrefabs, _effect_name, _effect_placing);


	}


}
