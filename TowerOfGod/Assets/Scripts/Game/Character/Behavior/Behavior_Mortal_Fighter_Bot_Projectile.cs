using UnityEngine;
using System.Collections;

public class Behavior_Mortal_Fighter_Bot_Projectile : Behavior_Mortal_Fighter_Bot
{
//	protected override void setCombo2AdditionalProcess()
//	{
//		Invoke("shootProjectile", 0.2f);
//	}
//	
////	protected void shootProjectile()
////	{
////		Knowledge.Action_Combo3 = true;
////	}
//	protected void shootProjectile()
//	{
//		if (Knowledge.ComboCurrent != 2) return;  //jks must be combo2(aim) state to shoot now.
//		
//		//if (cancelAttackIfNoEnemy()) return; //jks cancel if no target.
//		
//		Knowledge.Progress_Combo2 = false;
//		Knowledge.Action_Combo2 = false;
//		Knowledge.Action_Combo3 = true;
//	}




	protected override void animate_Combo1()
	{		
		base.animate_Combo1();
		
		Knowledge.startAim();
	}
	

	
	//jks charge
	//public override Action combo()
	
	//jks aim
	//public override Action combo2()
	
	//jks shoot
	//public override Action combo3()

}
