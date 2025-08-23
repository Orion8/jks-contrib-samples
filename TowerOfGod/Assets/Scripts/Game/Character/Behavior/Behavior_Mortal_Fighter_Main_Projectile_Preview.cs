using UnityEngine;
using System.Collections;

public class Behavior_Mortal_Fighter_Main_Projectile_Preview : Behavior_Mortal_Fighter_Main_Projectile
{

	protected override void scanEnemy()
	{
		//Log.jprint(gameObject + " . . . . . . . Scan enemy ");
		
		bool found = Knowledge.checkEnemyOnPath(false);
		if (found)
		{
			StartCoroutine( Knowledge.startSkill() );
		}
	}
	
	protected override void scan()
	{
		if (Knowledge == null) return;
		
		if (Knowledge.Action_Run ||
		    Knowledge.Action_Walk ||
		    Knowledge.Action_WalkFast
		    )
		{
			scanEnemy();
		}
	}


}
