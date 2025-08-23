using UnityEngine;
using System.Collections;
public class Knowledge_Mortal_Fighter_Bot_Protectee : Knowledge_Mortal_Fighter_Bot 

{


	public override bool IsProtectee { get { return true; }}
		

	
	public override void initSentinel() 
	{
		//do nothing because a protectee should not attack opponent.
	}



	public override void setAttributesFromTable(Table_Spawn tableSpawn)
	{
		base.setAttributesFromTable(tableSpawn);

		_AIMoveType = eAIMoveType.AMT_Stay;
		Action_Idle = true; //jks default
	}



	public override bool shouldIRun()
	{
		Action_Idle = true; //jks default
		return (!amIInView() && IsCameraAtFront);
	}

		
	public override bool shouldIJumpBack()
	{
		Action_Idle = true; //jks default
		return (!amIInView() && !IsCameraAtFront) ;
	}

	protected override bool needToCatchUp()
	{

		//jks 카메라 중심과의 거리 확인. 
//		if (Mathf.Abs(transform.position.x - (CameraManager.Instance._targetCamera.transform.position.x)) 
		if (Mathf.Abs(transform.position.x - (BattleBase.Instance.LeaderTransform.position.x))  
			> BattleTuning.Instance._protectee_catchup_distance) // range to keep
//		    > 3.0f) // range to keep
			return true;
		

		return false;
	}

	protected override float FollowContinueTime { get { return 0.7f;}}


}
