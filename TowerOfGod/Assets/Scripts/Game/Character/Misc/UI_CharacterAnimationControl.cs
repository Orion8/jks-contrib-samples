using UnityEngine;
using System.Collections;

public class UI_CharacterAnimationControl : MonoBehaviour 
{
	protected static int walkState = Animator.StringToHash("Base Layer.Walk");	
	protected static int idleState = Animator.StringToHash("Base Layer.Idle");	
	protected static int walkState1 = Animator.StringToHash("Base Layer.Walk1");	
	protected static int walkState2 = Animator.StringToHash("Base Layer.Walk2");	
	protected static int walkState3 = Animator.StringToHash("Base Layer.Walk3");	
	protected static int walkState4 = Animator.StringToHash("Base Layer.Walk4");	

	
	protected static int Idle_Bam = Animator.StringToHash("Base Layer.Idle_Bam");
	protected static int Idle_Hawreyn = Animator.StringToHash("Base Layer.Idle_Hawreyn");
	protected static int Idle_Yuri = Animator.StringToHash("Base Layer.Idle_Yuri");
	protected static int Idle_Sunwonara = Animator.StringToHash("Base Layer.Idle_Sunwonara");
	protected static int Idle_Paraqul = Animator.StringToHash("Base Layer.Idle_Paraqul");	

	protected static int Active_Bam = Animator.StringToHash("Base Layer.Bam_Active");
	protected static int Active_Hawreyn = Animator.StringToHash("Base Layer.Hawreyn_Active");
	protected static int Active_Yuri = Animator.StringToHash("Base Layer.Yuri_Active");
	protected static int Active_Sunwonara = Animator.StringToHash("Base Layer.Sunwonara_Active");
	protected static int Active_Paraqul = Animator.StringToHash("Base Layer.Paraqul_Active");

	//-------------------------------------------------------

	protected static int Idle_Test = Animator.StringToHash("Base Layer.Idle_Test");
	protected static int Idle_Ran01 = Animator.StringToHash("Base Layer.Idle_Ran01");
	protected static int Idle_Ran02 = Animator.StringToHash("Base Layer.Idle_Ran02");
	protected static int Idle_Ran03 = Animator.StringToHash("Base Layer.Idle_Ran03");
	protected static int Idle_Ran04 = Animator.StringToHash("Base Layer.Idle_Ran04");
	protected static int Idle_Ran05 = Animator.StringToHash("Base Layer.Idle_Ran05");
	protected static int Idle_Ran06 = Animator.StringToHash("Base Layer.Idle_Ran06");
	protected static int Idle_Ran07 = Animator.StringToHash("Base Layer.Idle_Ran07");
	protected static int Idle_Ran08 = Animator.StringToHash("Base Layer.Idle_Ran08");




	//--------------------------------------------- IDLE_ANIMATION ------------------------------------------//	

	protected static int Idle_Slot01_01 = Animator.StringToHash("Base Layer.Idle_Slot01_01");
	protected static int Idle_Slot02_01 = Animator.StringToHash("Base Layer.Idle_Slot02_01");
	protected static int Idle_Slot03_01 = Animator.StringToHash("Base Layer.Idle_Slot03_01");
	protected static int Idle_Slot04_01 = Animator.StringToHash("Base Layer.Idle_Slot04_01");
	protected static int Idle_Slot05_01 = Animator.StringToHash("Base Layer.Idle_Slot05_01");

	
	protected static int Idle_Slot01_02 = Animator.StringToHash("Base Layer.Idle_Slot01_02");
	protected static int Idle_Slot02_02 = Animator.StringToHash("Base Layer.Idle_Slot02_02");
	protected static int Idle_Slot03_02 = Animator.StringToHash("Base Layer.Idle_Slot03_02");
	protected static int Idle_Slot04_02 = Animator.StringToHash("Base Layer.Idle_Slot04_02");
	protected static int Idle_Slot05_02 = Animator.StringToHash("Base Layer.Idle_Slot05_02");

	
	protected static int Idle_Slot01_03 = Animator.StringToHash("Base Layer.Idle_Slot01_03");
	protected static int Idle_Slot02_03 = Animator.StringToHash("Base Layer.Idle_Slot02_03");
	protected static int Idle_Slot03_03 = Animator.StringToHash("Base Layer.Idle_Slot03_03");
	protected static int Idle_Slot04_03 = Animator.StringToHash("Base Layer.Idle_Slot04_03");
	protected static int Idle_Slot05_03 = Animator.StringToHash("Base Layer.Idle_Slot05_03");
	
	
	protected static int Idle_Slot01_04 = Animator.StringToHash("Base Layer.Idle_Slot01_04");
	protected static int Idle_Slot02_04 = Animator.StringToHash("Base Layer.Idle_Slot02_04");
	protected static int Idle_Slot03_04 = Animator.StringToHash("Base Layer.Idle_Slot03_04");
	protected static int Idle_Slot04_04 = Animator.StringToHash("Base Layer.Idle_Slot04_04");
	protected static int Idle_Slot05_04 = Animator.StringToHash("Base Layer.Idle_Slot05_04");

	
	protected static int Idle_Slot01_05 = Animator.StringToHash("Base Layer.Idle_Slot01_05");
	protected static int Idle_Slot02_05 = Animator.StringToHash("Base Layer.Idle_Slot02_05");
	protected static int Idle_Slot03_05 = Animator.StringToHash("Base Layer.Idle_Slot03_05");
	protected static int Idle_Slot04_05 = Animator.StringToHash("Base Layer.Idle_Slot04_05");
	protected static int Idle_Slot05_05 = Animator.StringToHash("Base Layer.Idle_Slot05_05");


	//--------------------------------------------- WALK_ANIMATION ------------------------------------------//	
	
	protected static int Walk_Slot01_01 = Animator.StringToHash("Base Layer.Walk_Slot01_01");
	protected static int Walk_Slot02_01 = Animator.StringToHash("Base Layer.Walk_Slot02_01");
	protected static int Walk_Slot03_01 = Animator.StringToHash("Base Layer.Walk_Slot03_01");
	protected static int Walk_Slot04_01 = Animator.StringToHash("Base Layer.Walk_Slot04_01");
	protected static int Walk_Slot05_01 = Animator.StringToHash("Base Layer.Walk_Slot05_01");
	
	protected static int Walk_Slot01_02 = Animator.StringToHash("Base Layer.Walk_Slot01_02");
	protected static int Walk_Slot02_02 = Animator.StringToHash("Base Layer.Walk_Slot02_02");
	protected static int Walk_Slot03_02 = Animator.StringToHash("Base Layer.Walk_Slot03_02");
	protected static int Walk_Slot04_02 = Animator.StringToHash("Base Layer.Walk_Slot04_02");
	protected static int Walk_Slot05_02 = Animator.StringToHash("Base Layer.Walk_Slot05_02");
	
	protected static int Walk_Slot01_03 = Animator.StringToHash("Base Layer.Walk_Slot01_03");
	protected static int Walk_Slot02_03 = Animator.StringToHash("Base Layer.Walk_Slot02_03");
	protected static int Walk_Slot03_03 = Animator.StringToHash("Base Layer.Walk_Slot03_03");
	protected static int Walk_Slot04_03 = Animator.StringToHash("Base Layer.Walk_Slot04_03");
	protected static int Walk_Slot05_03 = Animator.StringToHash("Base Layer.Walk_Slot05_03");
	
	protected static int Walk_Slot01_04 = Animator.StringToHash("Base Layer.Walk_Slot01_04");
	protected static int Walk_Slot02_04 = Animator.StringToHash("Base Layer.Walk_Slot02_04");
	protected static int Walk_Slot03_04 = Animator.StringToHash("Base Layer.Walk_Slot03_04");
	protected static int Walk_Slot04_04 = Animator.StringToHash("Base Layer.Walk_Slot04_04");
	protected static int Walk_Slot05_04 = Animator.StringToHash("Base Layer.Walk_Slot05_04");
	
	protected static int Walk_Slot01_05 = Animator.StringToHash("Base Layer.Walk_Slot01_05");
	protected static int Walk_Slot02_05 = Animator.StringToHash("Base Layer.Walk_Slot02_05");
	protected static int Walk_Slot03_05 = Animator.StringToHash("Base Layer.Walk_Slot03_05");
	protected static int Walk_Slot04_05 = Animator.StringToHash("Base Layer.Walk_Slot04_05");
	protected static int Walk_Slot05_05 = Animator.StringToHash("Base Layer.Walk_Slot05_05");
	
	
	//--------------------------------------------- ACTIVE_ANIMATION ------------------------------------------//	
	
	protected static int Active_Slot01_01 = Animator.StringToHash("Base Layer.Active_Slot01_01");
	protected static int Active_Slot02_01 = Animator.StringToHash("Base Layer.Active_Slot02_01");
	protected static int Active_Slot03_01 = Animator.StringToHash("Base Layer.Active_Slot03_01");
	protected static int Active_Slot04_01 = Animator.StringToHash("Base Layer.Active_Slot04_01");
	protected static int Active_Slot05_01 = Animator.StringToHash("Base Layer.Active_Slot05_01");
	
	
	protected static int Active_Slot01_02 = Animator.StringToHash("Base Layer.Active_Slot01_02");
	protected static int Active_Slot02_02 = Animator.StringToHash("Base Layer.Active_Slot02_02");
	protected static int Active_Slot03_02 = Animator.StringToHash("Base Layer.Active_Slot03_02");
	protected static int Active_Slot04_02 = Animator.StringToHash("Base Layer.Active_Slot04_02");
	protected static int Active_Slot05_02 = Animator.StringToHash("Base Layer.Active_Slot05_02");
	
	
	//--------------------------------------------- RAN_ANIMATION ------------------------------------------//	
	
	protected static int Ran_Slot01_01 = Animator.StringToHash("Base Layer.Ran_Slot01_01");
	protected static int Ran_Slot02_01 = Animator.StringToHash("Base Layer.Ran_Slot02_01");
	protected static int Ran_Slot03_01 = Animator.StringToHash("Base Layer.Ran_Slot03_01");
	protected static int Ran_Slot04_01 = Animator.StringToHash("Base Layer.Ran_Slot04_01");
	protected static int Ran_Slot05_01 = Animator.StringToHash("Base Layer.Ran_Slot05_01");
	
	
	protected static int Ran_Slot01_02 = Animator.StringToHash("Base Layer.Ran_Slot01_02");
	protected static int Ran_Slot02_02 = Animator.StringToHash("Base Layer.Ran_Slot02_02");
	protected static int Ran_Slot03_02 = Animator.StringToHash("Base Layer.Ran_Slot03_02");
	protected static int Ran_Slot04_02 = Animator.StringToHash("Base Layer.Ran_Slot04_02");
	protected static int Ran_Slot05_02 = Animator.StringToHash("Base Layer.Ran_Slot05_02");
	
	public void forceWalk()
	{
		if (this.name == "0") {
			GetComponent<AnimationControl>().startAnimation(Walk_Slot01_01, 0.1f);
		}
		else if (this.name == "1") {
			GetComponent<AnimationControl>().startAnimation(Walk_Slot02_01, 0.1f);
		}
		else if (this.name == "2") {
			GetComponent<AnimationControl>().startAnimation(Walk_Slot03_01, 0.1f);
		}
		else if (this.name == "3") {
			GetComponent<AnimationControl>().startAnimation(Walk_Slot04_01, 0.1f);
		}
		else if (this.name == "4") {
			GetComponent<AnimationControl>().startAnimation(Walk_Slot05_01, 0.1f);
		}
		else {
			GetComponent<AnimationControl>().startAnimation(walkState, 0.1f);
		}
	}

	public void forceRanIdle() {
		if (this.name == "0")	
			GetComponent<AnimationControl>().startAnimation(Ran_Slot01_01, 0.1f);
		else if (this.name == "1")
			GetComponent<AnimationControl>().startAnimation(Ran_Slot02_01, 0.1f);
		else if (this.name == "2")
			GetComponent<AnimationControl>().startAnimation(Ran_Slot03_01, 0.1f);
		else if (this.name == "3")
			GetComponent<AnimationControl>().startAnimation(Ran_Slot04_01, 0.1f);
		else if (this.name == "4")
			GetComponent<AnimationControl>().startAnimation(Ran_Slot05_01, 0.1f);
	}

	public void forceRanIdle2() {
		if (this.name == "0")	
			GetComponent<AnimationControl>().startAnimation(Ran_Slot01_02, 0.1f);
		else if (this.name == "1")
			GetComponent<AnimationControl>().startAnimation(Ran_Slot02_02, 0.1f);
		else if (this.name == "2")
			GetComponent<AnimationControl>().startAnimation(Ran_Slot03_02, 0.1f);
		else if (this.name == "3")
			GetComponent<AnimationControl>().startAnimation(Ran_Slot04_02, 0.1f);
		else if (this.name == "4")
			GetComponent<AnimationControl>().startAnimation(Ran_Slot05_02, 0.1f);
	}
	
	public void forceIdle()
	{
		//if (gameObject.name == "sdbam") {
			GetComponent<AnimationControl>().startAnimation(idleState, 0.1f);
		//	return;
		//}
		//else {
		//	GetComponent<AnimationControl>().startAnimation(Idle_Test, 0.1f);
		//}
		/*
		Table_Character characterTable = (Table_Character)TableManager.GetContent(Inventory.Instance().CSlot[int.Parse(this.gameObject.name)].CCard.ModelIndex); //jks get character table
		Table_Path tablePath = (Table_Path)TableManager.GetContent (characterTable._pathID ); //jks - get character prefab path
		
		switch(tablePath._assetPath.ToString()) {
			case "Prefabs/Characters/Bam_01/C_Bam_01":
				GetComponent<AnimationControl>().startAnimation(Idle_Bam, 0.1f);
				break;
			case "Prefabs/Characters/HawReyn_02/CM_HawReyn_02":
				GetComponent<AnimationControl>().startAnimation(Idle_Hawreyn, 0.1f);
				break;
			case "Prefabs/Characters/Yuri_01/CM_Yuri":
				GetComponent<AnimationControl>().startAnimation(Idle_Sunwonara, 0.1f);
				break;
			case "Prefabs/Characters/Sunwonara_01/CM_Sunwonara_01":
				GetComponent<AnimationControl>().startAnimation(Idle_Sunwonara, 0.1f);
				break;
			case "Prefabs/Characters/Paraqul_01/CM_Paraqul_01":
				GetComponent<AnimationControl>().startAnimation(Idle_Paraqul, 0.1f);
				break;
			default:
				GetComponent<AnimationControl>().startAnimation(idleState, 0.1f);
				break;
		}

*/
	}

	public void forceActive()
	{
		switch(this.gameObject.name) {
			case "0":
				GetComponent<AnimationControl>().startAnimation(Active_Slot01_01, 0.1f);
				break;
			case "1":
				GetComponent<AnimationControl>().startAnimation(Active_Slot02_01, 0.1f);
				break;
			case "2":
				GetComponent<AnimationControl>().startAnimation(Active_Slot03_01, 0.1f);
				break;
			case "3":
				GetComponent<AnimationControl>().startAnimation(Active_Slot04_01, 0.1f);
				break;
			case "4":
				GetComponent<AnimationControl>().startAnimation(Active_Slot05_01, 0.1f);
				break;
		}
	}
	
	public void forceActive2()
	{
		switch(this.gameObject.name) {
		case "0":
			GetComponent<AnimationControl>().startAnimation(Active_Slot01_02, 0.1f);
			break;
		case "1":
			GetComponent<AnimationControl>().startAnimation(Active_Slot02_02, 0.1f);
			break;
		case "2":
			GetComponent<AnimationControl>().startAnimation(Active_Slot03_02, 0.1f);
			break;
		case "3":
			GetComponent<AnimationControl>().startAnimation(Active_Slot04_02, 0.1f);
			break;
		case "4":
			GetComponent<AnimationControl>().startAnimation(Active_Slot05_02, 0.1f);
			break;
		}
	}

	public void forceWalk1()
	{
		if (this.name == "0") {
			GetComponent<AnimationControl>().startAnimation(Walk_Slot01_02, 0.1f);
		}
		else if (this.name == "1") {
			GetComponent<AnimationControl>().startAnimation(Walk_Slot02_02, 0.1f);
		}
		else if (this.name == "2") {
			GetComponent<AnimationControl>().startAnimation(Walk_Slot03_02, 0.1f);
		}
		else if (this.name == "3") {
			GetComponent<AnimationControl>().startAnimation(Walk_Slot04_02, 0.1f);
		}
		else if (this.name == "4") {
			GetComponent<AnimationControl>().startAnimation(Walk_Slot05_02, 0.1f);
		}
	}
	
	public void forceWalk2()
	{
		if (this.name == "0") {
			GetComponent<AnimationControl>().startAnimation(Walk_Slot01_03, 0.1f);
		}
		else if (this.name == "1") {
			GetComponent<AnimationControl>().startAnimation(Walk_Slot02_03, 0.1f);
		}
		else if (this.name == "2") {
			GetComponent<AnimationControl>().startAnimation(Walk_Slot03_03, 0.1f);
		}
		else if (this.name == "3") {
			GetComponent<AnimationControl>().startAnimation(Walk_Slot04_03, 0.1f);
		}
		else if (this.name == "4") {
			GetComponent<AnimationControl>().startAnimation(Walk_Slot05_03, 0.1f);
		}
	}
	
	public void forceWalk3()
	{
		if (this.name == "0") {
			GetComponent<AnimationControl>().startAnimation(Walk_Slot01_04, 0.1f);
		}
		else if (this.name == "1") {
			GetComponent<AnimationControl>().startAnimation(Walk_Slot02_04, 0.1f);
		}
		else if (this.name == "2") {
			GetComponent<AnimationControl>().startAnimation(Walk_Slot03_04, 0.1f);
		}
		else if (this.name == "3") {
			GetComponent<AnimationControl>().startAnimation(Walk_Slot04_04, 0.1f);
		}
		else if (this.name == "4") {
			GetComponent<AnimationControl>().startAnimation(Walk_Slot05_04, 0.1f);
		}
	}
	
	public void forceWalk4()
	{
		if (this.name == "0") {
			GetComponent<AnimationControl>().startAnimation(Walk_Slot01_05, 0.1f);
		}
		else if (this.name == "1") {
			GetComponent<AnimationControl>().startAnimation(Walk_Slot02_05, 0.1f);
		}
		else if (this.name == "2") {
			GetComponent<AnimationControl>().startAnimation(Walk_Slot03_05, 0.1f);
		}
		else if (this.name == "3") {
			GetComponent<AnimationControl>().startAnimation(Walk_Slot04_05, 0.1f);
		}
		else if (this.name == "4") {
			GetComponent<AnimationControl>().startAnimation(Walk_Slot05_05, 0.1f);
		}
	}

	public void forceIdleSlot() {

		if (GetComponent<Animator>().GetBool("Idle1")) {
			
			if (this.name == "0") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot01_01, 0.1f);
			}
			else if (this.name == "1") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot02_01, 0.1f);
			}
			else if (this.name == "2") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot03_01, 0.1f);
			}
			else if (this.name == "3") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot04_01, 0.1f);
			}
			else if (this.name == "4") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot05_01, 0.1f);
			}

		}
		else if (GetComponent<Animator>().GetBool("Idle2")) {
			
			if (this.name == "0") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot01_02, 0.1f);
			}
			else if (this.name == "1") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot02_02, 0.1f);
			}
			else if (this.name == "2") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot03_02, 0.1f);
			}
			else if (this.name == "3") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot04_02, 0.1f);
			}
			else if (this.name == "4") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot05_02, 0.1f);
			}

		}
		else if (GetComponent<Animator>().GetBool("Idle3")) {
			
			if (this.name == "0") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot01_03, 0.1f);
			}
			else if (this.name == "1") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot02_03, 0.1f);
			}
			else if (this.name == "2") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot03_03, 0.1f);
			}
			else if (this.name == "3") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot04_03, 0.1f);
			}
			else if (this.name == "4") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot05_03, 0.1f);
			}

		}
		else if (GetComponent<Animator>().GetBool("Idle4")) {
			
			if (this.name == "0") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot01_04, 0.1f);
			}
			else if (this.name == "1") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot02_04, 0.1f);
			}
			else if (this.name == "2") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot03_04, 0.1f);
			}
			else if (this.name == "3") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot04_04, 0.1f);
			}
			else if (this.name == "4") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot05_04, 0.1f);
			}

		}
		else if (GetComponent<Animator>().GetBool("Idle5")) {
			
			if (this.name == "0") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot01_05, 0.1f);
			}
			else if (this.name == "1") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot02_05, 0.1f);
			}
			else if (this.name == "2") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot03_05, 0.1f);
			}
			else if (this.name == "3") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot04_05, 0.1f);
			}
			else if (this.name == "4") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot05_05, 0.1f);
			}

		}
	}

	public void forceIdleSlot(int value) {

		if (value >= 80) {
			if (this.name == "0") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot01_01, 0.1f);
			}
			else if (this.name == "1") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot02_01, 0.1f);
			}
			else if (this.name == "2") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot03_01, 0.1f);
			}
			else if (this.name == "3") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot04_01, 0.1f);
			}
			else if (this.name == "4") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot05_01, 0.1f);
			}
		}
		else if (value >= 60) {
			if (this.name == "0") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot01_02, 0.1f);
			}
			else if (this.name == "1") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot02_02, 0.1f);
			}
			else if (this.name == "2") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot03_02, 0.1f);
			}
			else if (this.name == "3") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot04_02, 0.1f);
			}
			else if (this.name == "4") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot05_02, 0.1f);
			}
		}
		else if (value >= 40) {
			if (this.name == "0") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot01_03, 0.1f);
			}
			else if (this.name == "1") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot02_03, 0.1f);
			}
			else if (this.name == "2") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot03_03, 0.1f);
			}
			else if (this.name == "3") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot04_03, 0.1f);
			}
			else if (this.name == "4") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot05_03, 0.1f);
			}
		}
		else if (value >= 20) {
			if (this.name == "0") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot01_04, 0.1f);
			}
			else if (this.name == "1") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot02_04, 0.1f);
			}
			else if (this.name == "2") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot03_04, 0.1f);
			}
			else if (this.name == "3") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot04_04, 0.1f);
			}
			else if (this.name == "4") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot05_04, 0.1f);
			}
		}
		else {
			if (this.name == "0") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot01_05, 0.1f);
			}
			else if (this.name == "1") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot02_05, 0.1f);
			}
			else if (this.name == "2") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot03_05, 0.1f);
			}
			else if (this.name == "3") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot04_05, 0.1f);
			}
			else if (this.name == "4") {
				GetComponent<AnimationControl>().startAnimation(Idle_Slot05_05, 0.1f);
			}
		}
	}

	public void setIdleFlag(int Index) {

		for (int i = 1; i < 6; i++) {
			GetComponent<Animator>().SetBool("Idle" + Index.ToString(), false);
		}

		GetComponent<Animator>().SetBool("Idle" + Index.ToString(), true);
	}

}

