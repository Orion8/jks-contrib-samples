using UnityEngine;
using System.Collections;

public class Knowledge_Mortal_Fighter_Bot_LivingWeaponUser : Knowledge_Mortal_Fighter_Bot
{


	public override void setAttributesFromTable(Table_Skill tbl)
	{
		base.setAttributesFromTable(tbl);
		
		_summon_position_x = AttackInfo3 * 0.1f;
	}


	public override void giveDamage(float reactionDistanceOverride)
	{		
		if (KnowledgePSkill && KnowledgePSkill.Progress_AnyAction)
		{
			KnowledgePSkill.giveDamage(reactionDistanceOverride);
			return;
		}

		_skillCompleted = isLastDamageInSkill(reactionDistanceOverride);
		//Log.jprint(". . . . . giveDamage(): " + _attackType);
		Launcher_LivingWeapon launcher = GetComponent<Launcher_LivingWeapon>();
		if (launcher == null) 
		{
			Log.Warning("Can not find Launcher_LivingWeapon component. ");
			return;
		}
		
		if (_target_attack)
		{
			//Log.jprint("giveDamage()");
			StartCoroutine(launcher.spawn(_target_attack));//jks launcher.spawn(_target_attack);
		}
	}


}
