using UnityEngine;
using System.Collections;

public class VisibilityCheck : MonoBehaviour 
{
	void OnBecameInvisible() 
	{
		//Log.print(gameObject + " OnBecameInvisible");

//		GameObject go = getCharacterGameObject();

		//Knowledge_Mortal_Fighter_Main knowledge = go.GetComponent<Knowledge_Mortal_Fighter_Main>();
//		go.SetActive(false);

//		if (knowledge)
//		{
//			if (!knowledge.IsLeader)
//			{
//				//if (knowledge.Action_Idle)
//				//if (!knowledge.IsLeader)
//					go.SetActive(false);
//			}
//		}
	}


	protected virtual GameObject getCharacterGameObject()
	{
		return transform.parent.gameObject;
	}

}
