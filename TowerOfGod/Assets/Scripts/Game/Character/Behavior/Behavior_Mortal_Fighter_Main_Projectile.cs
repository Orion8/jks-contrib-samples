using UnityEngine;
using System.Collections;

public class Behavior_Mortal_Fighter_Main_Projectile : Behavior_Mortal_Fighter_Main
{


	new public Knowledge_Mortal_Fighter_Main_Projectile Knowledge
	{
		get
		{
			if (_knowledge == null)
			{
				_knowledge = GetComponent<Knowledge_Mortal_Fighter_Main_Projectile>();
			}
			return (Knowledge_Mortal_Fighter_Main_Projectile) _knowledge;
		}
	}


//	protected override void setCombo2AdditionalProcess()
//	{
//		//Log.jprint(gameObject + "setCombo2AdditionalProcess ");
//
//		Invoke("shootProjectile", 0.2f);
//	}
//	
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



	protected override bool cancelAttackIfNoEnemy()
	{
		if (BattleManager.Instance.isSkillPreviewMode()) return true;

		//Log.jprint("Projectile :: cancelAttackIfNoEnemy()");
		bool found = Knowledge.checkEnemyOnPath(false);
		if (!found)
		{
			if (Knowledge.IsLeader)
			{
				//forceWalk();
				if (Knowledge.AmIAuto)
				{
					forceRun();
				}
				else
				{
					forceIdle();//forceWalk();
				}
			}
			else
			{
//				if (Knowledge.ImmediateAttack)
//				{
//					forceIdle();    //jks no enemy and immediate, idle
//				}
//				else
				{
					forceWalkBack(); //jks no enemy , so walkback
				}
				Knowledge.resetCoolTimer(); //BattleBase.Instance.endUICoolTime(Knowledge.CardDeckIndex);//jks card use canceled, so let the card available.
				Knowledge.activateSkillFinishedEvent();  //jks activate leader
			}

			//클래스 매치 스킬 시전중이었다면 스킬 발동 플래그 해제
			if (BattleUI.Instance() != null)
			if (BattleUI.Instance().ClassMatchOwner == Knowledge.CardDeckIndex)
			{
				BattleUI.Instance().ClassMatchOwner = -1;
			}
			Knowledge.endAim();
		}
		
		_isDelayedInvoked_cancelAttackIfNoEnemy = false;

		return !found;  //jks is canceled?
	}


//	protected override void comboRootMotion()
//	{
//		AnimCon.applyRootMotion(false);
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
