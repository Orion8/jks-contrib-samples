using UnityEngine;
using System.Collections;


/// <summary>
/// animation event 가 추가되어 있는 애니메이션을 전투가 아닌 컷씬등에 사용할 경우 이벤트 발생으로 인한 에러 제거 용도. - Dummy animation event receiver.
/// 주의 : 전투에 사용되는 캐릭텨 프리팹에 컴포넌트로 추가하면 안됨.
/// </summary>
public class DummyAnimationEventReceiver : MonoBehaviour 
{
	public void animEvent_LockOpponent(){}
	public void animEvent_UnlockOpponent(){}
	public void animEvent_Give_Damage(float reactionDistance){}
	public void animEvent_Give_Damage_QuickSkill(float reactionDistance){}
	public void animEvent_Action_Finished(){}
	public void animEvent_GuardUp_Finished(){}
	public void animEvent_PlaySound(string key){}
	public void animEvent_ShowEffect(string key){}
	public void anmiEvent_SpawnFx(string key){}
	public void animEvent_ShowTrail(){}
	public void animEvent_Zoom(float depth){}
	public void animEvent_SpawnFxAtTarget(string key){}
	public void animEvent_DespawnFxAtTarget(string key){}
	public void animEvent_SlowMotion_Finished(){}
	public void animEvent_Weapon_Hide(){}
	public void animEvent_Weapon_Show(){}
	public void animEvent_InstallWeapon(){}
	public void animEvent_Inven_EventEnd(){}
	public void animEvent_SkillStaging_Finished(){}
	public void animEvent_QuickCombo_Action_Finished(int combo) {}
	public void animEvent_QuickCombo_Transition(int combo) {}
	public void animEvent_Combo_InputWindow_Open(int combo){}
	public void animEvent_Combo_InputWindow_Close(int combo){}
	public void animEvent_audio(string key) { }
}
