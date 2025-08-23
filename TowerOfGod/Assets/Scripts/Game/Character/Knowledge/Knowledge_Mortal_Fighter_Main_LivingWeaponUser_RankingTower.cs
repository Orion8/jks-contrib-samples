using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Knowledge_Mortal_Fighter_Main_LivingWeaponUser_RankingTower : Knowledge_Mortal_Fighter_Main_RankingTower
{
	protected override List<FighterActor> Opponents { get { return BattleBase.Instance.List_Ally; } }

	public override void setAttributesFromTable(Table_Skill tbl)
	{
		base.setAttributesFromTable(tbl);
		
		_summon_position_x = AttackInfo3 * 0.1f;
	}
	


	public override bool amIMelee()
	{
		return false;
	}
		



	public override void giveDamage(float reactionDistanceOverride)
	{		
		_everGaveDamageForTheAttack = true;
		
		_skillCompleted = isLastDamageInSkill(reactionDistanceOverride);

		Launcher_LivingWeapon launcher = GetComponent<Launcher_LivingWeapon>();
		if (launcher == null) 
		{
			Log.Warning("Can not find Launcher_LivingWeapon component. ");
			return;
		}

		SkillType = eSkillType.ST_Card;
		StartCoroutine(launcher.spawn(_target_attack));
	}



	public override void incrementContinuousHitCount() 
	{ 
		if (!IsLeader) return;

		_continuousHitCount_QuickSkill++; 

		BattleBase.Instance.setLeaderBuffInvincibleMode_P2(_continuousHitCount_QuickSkill);
	}

}
