using UnityEngine;
using System.Collections;

public class SkillBuffAgent : MonoBehaviour 
{
	protected float _attack_up = 0;			//버프시 증가할 아군의 공격치 %	
	protected float _defense_up = 0;		//버프시 증가시킬 아군의 방어력 %
	protected float _critical_up = 0;		//버프시 증가시킬 아군의 적중율 %
	protected float _attack_down = 0;		//버프시 감소시킬 상대의 공격치 %
	protected float _defense_down = 0;		//버프시 감소시킬 상대의 방어력 %
	protected float _critical_down = 0;		//버프시 감소시킬 상대의 적중율 %
	protected bool _skill_hit_only_buff_on = false;	//영역 감지 되어 적용 할 버프인지 판단.


	protected int _unique_ID = 0;
	public void setUniqueID(int id) { _unique_ID = id; }


	public float AttackUp { get { return _attack_up; } }
	public float DefenseUp { get { return _defense_up; } }
	public float CriticalUp { get { return _critical_up; } }


	public float AttackDown (Knowledge_Mortal_Fighter opponentKnowledge)
	{ 
		//jks 스킬 맞을 경우에만 활성화되는 스킬버프이고, 
		if (_skill_hit_only_buff_on)
		{
			//jks 해당 스킬에 맞은 캐릭터가 아니면,   버프 적용 안함.
			if (!opponentKnowledge.AppliedSkillBuff_UID_Queue.Contains(_unique_ID)) 
			{
				#if UNITY_EDITOR
				if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
				{
					foreach (int id in opponentKnowledge.AppliedSkillBuff_UID_Queue) 
						Log.print_always(opponentKnowledge.name+ " 피격된 스킬 버프 id: " + id);

					Log.print_always("  hit only type 스킬 버프  ->   스킬 버프 ID: "+_unique_ID + 
						"  가 맞은 상대의 맞은 스킬 버프 ID 들에 포함 되어 있지 않아 공격력 감소 적용 안됨.");					
				}
				#endif

				return 0;
			}
			else
			{
				#if UNITY_EDITOR
				if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
				{
					foreach (int id in opponentKnowledge.AppliedSkillBuff_UID_Queue) 
						Log.print_always(opponentKnowledge.name+ " 피격된 스킬 버프 id: " + id);

					Log.print_always("  hit only type 스킬 버프  ->   스킬 버프 ID: "+_unique_ID + 
						"   가 상대의 적용된 스킬 버프 ID 들 중에 포함되어 있어 공격력 감소 적용: "  + _attack_down);					
				}
				#endif

			}
		}
			
		return _attack_down; 	 
	}


	public float DefenseDown (Knowledge_Mortal_Fighter opponentKnowledge)
	{ 
		//jks 스킬 맞을 경우에만 활성화되는 스킬버프이고, 
		if (_skill_hit_only_buff_on)
		{
			//jks 해당 스킬에 맞은 캐릭터가 아니면,   버프 적용 안함.
			if (!opponentKnowledge.AppliedSkillBuff_UID_Queue.Contains(_unique_ID)) 
			{
				#if UNITY_EDITOR
				if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
				{
					foreach (int id in opponentKnowledge.AppliedSkillBuff_UID_Queue) 
						Log.print_always(opponentKnowledge.name+ " 피격된 스킬 버프 id: " + id);

					Log.print_always("  hit only type 스킬 버프  ->   스킬 버프 ID: "+_unique_ID + 
						"  가 맞은 상대의 맞은 스킬 버프 ID 들에 포함 되어 있지 않아 방어력 감소 적용 안됨.");					
				}
				#endif

				return 0;
			}
			else
			{
				#if UNITY_EDITOR
				if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
				{
					foreach (int id in opponentKnowledge.AppliedSkillBuff_UID_Queue) 
						Log.print_always(opponentKnowledge.name+ " 피격된 스킬 버프 id: " + id);

					Log.print_always("  hit only type 스킬 버프  ->   스킬 버프 ID: "+_unique_ID + 
						"   가 상대의 적용된 스킬 버프 ID 들 중에 포함되어 있어 방어력 감소 적용: "  + _defense_down);					
				}
				#endif

			}
		}

		return _defense_down; 	 
	}


	public float CriticalDown (Knowledge_Mortal_Fighter opponentKnowledge)
	{ 
		//jks 스킬 맞을 경우에만 활성화되는 스킬버프이고, 
		if (_skill_hit_only_buff_on)
		{
			//jks 해당 스킬에 맞은 캐릭터가 아니면,   버프 적용 안함.
			if (!opponentKnowledge.AppliedSkillBuff_UID_Queue.Contains(_unique_ID)) 
			{
				#if UNITY_EDITOR
				if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
				{
					foreach (int id in opponentKnowledge.AppliedSkillBuff_UID_Queue) 
						Log.print_always(opponentKnowledge.name+ " 피격된 스킬 버프 id: " + id);

					Log.print_always("  hit only type 스킬 버프  ->   스킬 버프 ID: "+_unique_ID + 
						"  가 맞은 상대의 맞은 스킬 버프 ID 들에 포함 되어 있지 않아 적중율 감소 적용 안됨.");					
				}
				#endif

				return 0;
			}
			else
			{
				#if UNITY_EDITOR
				if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
				{
					foreach (int id in opponentKnowledge.AppliedSkillBuff_UID_Queue) 
						Log.print_always(opponentKnowledge.name+ " 피격된 스킬 버프 id: " + id);

					Log.print_always("  hit only type 스킬 버프  ->   스킬 버프 ID: "+_unique_ID + 
						"   가 상대의 적용된 스킬 버프 ID 들 중에 포함되어 있어 적중율 감소 적용: "  + _critical_down);					
				}
				#endif

			}
		}

		return _critical_down; 	 
	}


	public void setAttributes(float attack_up, float defense_up, float critical_up, 
		float attack_down, float defense_down, float critical_down, float life_time, bool skill_hit_only_buff_on)
	{
		_attack_up = attack_up;	
		_defense_up = defense_up;
		_critical_up = critical_up;
		_attack_down = attack_down;
		_defense_down = defense_down;
		_critical_down = critical_down;
		_skill_hit_only_buff_on  = skill_hit_only_buff_on;

		Invoke("killMe", life_time);

	}
		



	public virtual void killMe()
	{
		#if UNITY_EDITOR
		if (TestOption.Instance() != null && TestOption.Instance()._showDamageCalculation)
		{
			Log.print_always(Time.time + "   버프고유ID: "+ _unique_ID +"   << << << << << << << << << << << << << <<    Skill Buff End  스킬 버프 종료. ");
			Log.print_always(
				" 공격력 up: "+_attack_up + " 공격력 down: "+_attack_down 
				+ " 방어력 up: "+_defense_up + " 방어력 down: "+_defense_down
				+ " 적중률 up: "+_critical_up + " 적중률 down: "+_critical_down + " 스킬에 피격된 경우만 적요하는 버프인지: " + _skill_hit_only_buff_on);
		}
		#endif
		Destroy(this);
	}

}
