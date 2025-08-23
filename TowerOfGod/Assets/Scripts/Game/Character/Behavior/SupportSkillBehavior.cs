using UnityEngine;
using System.Collections;

public class SupportSkillBehavior : MonoBehaviour 
{
	SupportSkillKnowledge _knowledgeSS = null;
	Knowledge_Mortal_Fighter _knowledge = null;
	Locomotion _locomotion = null;
	AnimationControl _animControl = null;

	public SupportSkillKnowledge KnowledgeSS
	{
		get
		{
			if (_knowledgeSS == null)
			{
				_knowledgeSS = GetComponent<SupportSkillKnowledge>();
			}
			return (SupportSkillKnowledge) _knowledgeSS;
		}
	}

	public Knowledge_Mortal_Fighter Knowledge
	{
		get
		{
			if (_knowledge == null)
			{
				_knowledge = GetComponent<Knowledge_Mortal_Fighter>();
			}
			return _knowledge;
		}
	}


	protected float SkillAniPlayRate
	{
		get 
		{
			return Knowledge.calculate_SkillAnimationPlayRate();
		}
	}


	public Locomotion_Mortal_Fighter Loco
	{
		get
		{
			if (_locomotion == null)
			{
				_locomotion = GetComponent<Locomotion>();
				if (_locomotion == null) Log.Warning("Missing Locomotion component.");
			}
			return (Locomotion_Mortal_Fighter) _locomotion;
		}
	}


	protected AnimationControl AnimCon
	{
		get
		{
			if (_animControl == null)
			{
				_animControl = GetComponent<AnimationControl>();
			}
			return _animControl;
		}
	}




	protected virtual void comboRootMotion()
	{
		AnimCon.applyRootMotion(true);

		//jsm - trail set by weapon type
		if (Knowledge.AttackType == eAttackType.AT_Sword)
		{
			Knowledge.setTrail(true);
		}
	}


	



	#region Animation

	protected void animate_Combo1()
	{		
		Log.nprint(gameObject.name +" :  SS animate_Combo1 ");
		
		comboRootMotion();
		AnimCon.startAnimation(KnowledgeSS.Anim_Combo1, AnimationBlendingInfo.Instance._blend_combo1, SkillAniPlayRate, 0);  //jks call "play()" instead of "crossfade()" by giving small blending time.
		KnowledgeSS.Progress_Anim_Combo1 = true;
		KnowledgeSS.ComboCurrent = 1;
		KnowledgeSS.ComboRecent = KnowledgeSS.ComboCurrent;
		Knowledge._isResetActionInfoDone = false;
		Loco.setCurrentSpeedToZero();
		GetComponent<Behavior_Mortal_Fighter>().attachCameraAndSetTarget();
	}

	protected void animate_Combo2()
	{		
		Log.nprint(gameObject.name +" :  SS animate_Combo2 ");
		
		comboRootMotion();
		AnimCon.startAnimation(KnowledgeSS.Anim_Combo2, AnimationBlendingInfo.Instance._blend_combo2, SkillAniPlayRate, 0);
		KnowledgeSS.Progress_Anim_Combo2 = true;
		KnowledgeSS.ComboCurrent = 2;
		KnowledgeSS.ComboRecent = KnowledgeSS.ComboCurrent;
		Knowledge._isResetActionInfoDone = false;
		Loco.setCurrentSpeedToZero();
	}
	
	protected void animate_Combo3()
	{		
		Log.nprint(gameObject.name +" :  SS animate_Combo3 ");
		
		comboRootMotion();
		AnimCon.startAnimation(KnowledgeSS.Anim_Combo3, AnimationBlendingInfo.Instance._blend_combo3, SkillAniPlayRate, 0);
		KnowledgeSS.Progress_Anim_Combo3 = true;
		KnowledgeSS.ComboCurrent = 3;
		KnowledgeSS.ComboRecent = KnowledgeSS.ComboCurrent;
		Knowledge._isResetActionInfoDone = false;
		Loco.setCurrentSpeedToZero();
	}
	
	protected void animate_Combo4()
	{		
		Log.nprint(gameObject.name +" :  SS animate_Combo4 ");
		
		comboRootMotion();
		AnimCon.startAnimation(KnowledgeSS.Anim_Combo4, AnimationBlendingInfo.Instance._blend_combo4, SkillAniPlayRate, 0);
		KnowledgeSS.Progress_Anim_Combo4 = true;
		KnowledgeSS.ComboCurrent = 4;
		KnowledgeSS.ComboRecent = KnowledgeSS.ComboCurrent;
		Knowledge._isResetActionInfoDone = false;
		Loco.setCurrentSpeedToZero();
	}
	
	protected void animate_Combo5()
	{		
		Log.nprint(gameObject.name +" :  SS animate_Combo5 ");
		
		comboRootMotion();
		AnimCon.startAnimation(KnowledgeSS.Anim_Combo5, AnimationBlendingInfo.Instance._blend_combo5, SkillAniPlayRate, 0);
		KnowledgeSS.Progress_Anim_Combo5 = true;
		KnowledgeSS.ComboCurrent = 5;
		KnowledgeSS.ComboRecent = KnowledgeSS.ComboCurrent;
		Knowledge._isResetActionInfoDone = false;
		Loco.setCurrentSpeedToZero();
	}
	
	protected void animate_Combo6()
	{		
		Log.nprint(gameObject.name +" :  SS animate_Combo6 ");
		
		comboRootMotion();
		AnimCon.startAnimation(KnowledgeSS.Anim_Combo6, AnimationBlendingInfo.Instance._blend_combo6, SkillAniPlayRate, 0);
		KnowledgeSS.Progress_Anim_Combo6 = true;
		KnowledgeSS.ComboCurrent = 6;
		KnowledgeSS.ComboRecent = KnowledgeSS.ComboCurrent;
		Knowledge._isResetActionInfoDone = false;
		Loco.setCurrentSpeedToZero();
	}
	
	protected void animate_Combo7()
	{		
		Log.nprint(gameObject.name +" :  SS animate_Combo7 ");
		
		comboRootMotion();
		AnimCon.startAnimation(KnowledgeSS.Anim_Combo7, AnimationBlendingInfo.Instance._blend_combo7, SkillAniPlayRate, 0);
		KnowledgeSS.Progress_Anim_Combo7 = true;
		KnowledgeSS.ComboCurrent = 7;
		KnowledgeSS.ComboRecent = KnowledgeSS.ComboCurrent;
		Knowledge._isResetActionInfoDone = false;
		Loco.setCurrentSpeedToZero();
	}
	
	protected void animate_Combo8()
	{		
		Log.nprint(gameObject.name +" :  SS animate_Combo8 ");
		
		comboRootMotion();
		AnimCon.startAnimation(KnowledgeSS.Anim_Combo8, AnimationBlendingInfo.Instance._blend_combo8, SkillAniPlayRate, 0);
		KnowledgeSS.Progress_Anim_Combo8 = true;
		KnowledgeSS.ComboCurrent = 8;
		KnowledgeSS.ComboRecent = KnowledgeSS.ComboCurrent;
		Knowledge._isResetActionInfoDone = false;
		Loco.setCurrentSpeedToZero();
	}
	

	#endregion



	#region Behavior Tree


	public bool combo1()
	{
		if (KnowledgeSS && KnowledgeSS.Action_Combo1) 
		{			
			if (!KnowledgeSS.Progress_Anim_Combo1)
			{
				animate_Combo1();
			}
			return true;
		}
		return false;
	}


	public bool combo2()
	{
		if (KnowledgeSS && KnowledgeSS.Action_Combo2) 
		{			
			if (!KnowledgeSS.Progress_Anim_Combo2)
			{
				animate_Combo2();
			}
			return true;
		}
		return false;
	}


	public bool combo3()
	{
		if (KnowledgeSS && KnowledgeSS.Action_Combo3) 
		{			
			if (!KnowledgeSS.Progress_Anim_Combo3)
			{
				animate_Combo3();
			}
			return true;
		}
		return false;
	}
	

	public bool combo4()
	{
		if (KnowledgeSS && KnowledgeSS.Action_Combo4) 
		{			
			if (!KnowledgeSS.Progress_Anim_Combo4)
			{
				animate_Combo4();
			}
			return true;
		}
		return false;
	}



	public bool combo5()
	{
		if (KnowledgeSS && KnowledgeSS.Action_Combo5) 
		{			
			if (!KnowledgeSS.Progress_Anim_Combo5)
			{
				animate_Combo5();
			}
			return true;
		}
		return false;
	}


	public bool combo6()
	{
		if (KnowledgeSS && KnowledgeSS.Action_Combo6) 
		{			
			if (!KnowledgeSS.Progress_Anim_Combo6)
			{
				animate_Combo6();
			}
			return true;
		}
		return false;
	}



	public bool combo7()
	{
		if (KnowledgeSS && KnowledgeSS.Action_Combo7) 
		{			
			if (!KnowledgeSS.Progress_Anim_Combo7)
			{
				animate_Combo7();
			}
			return true;
		}
		return false;
	}



	public bool combo8()
	{
		if (KnowledgeSS && KnowledgeSS.Action_Combo8) 
		{			
			if (!KnowledgeSS.Progress_Anim_Combo8)
			{
				animate_Combo8();
			}
			return true;
		}
		return false;
	}


	#endregion

		

}
