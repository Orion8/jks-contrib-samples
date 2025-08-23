using UnityEngine;
using System.Collections;

public class Knowledge_Mortal_Fighter_Main_LivingWeaponUser : Knowledge_Mortal_Fighter_Main
{


	protected override IEnumerator setLauncher()
	{
		Launcher_LivingWeapon[] launchers = GetComponents<Launcher_LivingWeapon>();

		while (launchers.Length == 0)
		{
			yield return null;
			launchers = GetComponents<Launcher_LivingWeapon>();
		}

		foreach (Launcher_LivingWeapon launcher in launchers )
		{
			if (launcher.LauncherTypeName == "Launcher_LivingWeapon")
				{_launcher = launcher; break;}
		}
		
		
		if (_launcher == null) 
		{
			Log.Warning("Can not find Launcher_LivingWeapon component. ");
		}
		
	}



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

//		//jks 역할 스킬 인가?
//		if (Action_RoleSkill)  
//		{
//			giveDamage_RoleSkill();
//			return;
//		}
//		//jks 지원 스킬 인가?
//		else 
		if (KnowledgeSSkill && KnowledgeSSkill.Progress_AnyAction)
		{
			KnowledgeSSkill.giveDamage(reactionDistanceOverride);
			return;
		}
		
		_everGaveDamageForTheAttack = true;

		_skillCompleted = isLastDamageInSkill(reactionDistanceOverride);
		//Log.jprint(". . . . . giveDamage(): " + _attackType);
//		Launcher_LivingWeapon launcher = GetComponent<Launcher_LivingWeapon>();
//		if (launcher == null) 
//		{
//			Log.Warning("Can not find Launcher_LivingWeapon component. ");
//			return;
//		}
		
//		if (_target_attack)
		{
			//Log.jprint("giveDamage()");
			SkillType = eSkillType.ST_Card;
			StartCoroutine(_launcher.spawn(_target_attack));//jks launcher.spawn(_target_attack);
		}

	}
	



}
