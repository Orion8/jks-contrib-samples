using UnityEngine;
using System.Collections;


//jks 팀원들의 스킬액션 speed 를 특정 시간동안 빠르게 해주는 skill.
public class Weapon_Throw_TeamFastSkill : Weapon_Throw
{

	protected float _rate;


	public void setAttributes(float rate)
	{
		_rate = rate;
	}
	



	
	protected override void applyWeaponPower() 
	{ 
		BattleManager.Instance.adjustSkillSpeed(_rate, AllyID);
	}
	
	



	public override void killMe()
	{
		//jks reset
		BattleManager.Instance.adjustSkillSpeed(1, AllyID); 

		base.killMe();
	}
	

}
