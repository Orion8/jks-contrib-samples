using UnityEngine;
using System.Collections;

public class EffectPosition : MonoBehaviour 
{
	Transform _target = null;


	public Transform MyTarget
	{
		get { return _target; }
		set { _target = value;}
	}


	// Update is called once per frame
	void Update () 
	{
		if (MyTarget == null) return;

		transform.position = MyTarget.position;

	}
}
