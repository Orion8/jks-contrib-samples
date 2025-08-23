using UnityEngine;
using System.Collections;

public class AttackCoolTime_SupportSkill_1 : AttackCoolTime_SupportSkill 
{
	
	
	public override void reset()
	{
		_skillIndex = 1;
		
		base.reset();
	}
	
}
