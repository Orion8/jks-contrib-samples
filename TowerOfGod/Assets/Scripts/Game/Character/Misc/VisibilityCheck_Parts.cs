using UnityEngine;
using System.Collections;

public class VisibilityCheck_Parts : VisibilityCheck 
{


	protected override GameObject getCharacterGameObject()
	{
		return transform.parent.parent.gameObject;
	}


}
