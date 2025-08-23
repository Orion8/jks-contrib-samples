using UnityEngine;
using System.Collections;


//jks 던져 발동 시키는 무기들의 base
public class Weapon_Throw : Weapon 
{
	protected eAllyID _ally_ID;


	protected float _effect_length; 			//jks- in sec.
	protected int _effect_placing; 			
	protected bool _effect_repeat; 			

	protected float _life_span; 			//jks- in sec.
	protected float _activation_delay;	//jks- duratin between launching and activating

	protected string _effect_name;

	protected Transform _effectPrefab;


	public virtual bool IsSlowTrap { get { return false; }}


	public eAllyID AllyID
	{
		get { return _ally_ID; }
		set { _ally_ID = value; }
	}
	
	public string EffectName
	{
		get { return _effect_name; }
		set { _effect_name = value; }
	}

	public bool IsAlly (Knowledge_Mortal subject)
	{ 
		return _ally_ID == subject.AllyID;
	}


	#region initialize


	void Start()
	{
		Init(_owner.GetComponent<Knowledge_Mortal_Fighter>());
	}


	protected virtual void Init(Knowledge_Mortal ownerKnowledge)
	{
		_owner = ownerKnowledge.gameObject;
		_ally_ID = ownerKnowledge.AllyID;

		Invoke("activateWeapon",_activation_delay);

		Invoke("killMe", _life_span);
	}


	public void setAttributes_base(float effect_length, int effect_placing, bool effect_repeat,  float life_span, float activation_delay)
	{
		_effect_length = effect_length;
		_effect_placing = effect_placing;
		_effect_repeat = effect_repeat;

		_life_span = life_span;
		_activation_delay = activation_delay;
	}


	#endregion



	#region update


	protected virtual void activateWeapon() 
	{ 
		if (_effect_repeat)
			InvokeRepeating("createEffect", 0, _effect_length);
		else
			createEffect();
		
		InvokeRepeating("updateEffectPosition", 0, 0.1f);

		if (!BattleBase.Instance.isSkillPreviewMode())
			applyWeaponPower();
	}


	protected virtual void applyWeaponPower() { }


	protected virtual void createEffect() 
	{
		if (BattleManager.Instance == null) return;
		
		_effectPrefab = MadPool.Instance().poolInstantiate(_effect_name);
		
		EffectPosition fxPos = _effectPrefab.GetComponent<EffectPosition>();
		if (fxPos == null)
			fxPos = _effectPrefab.gameObject.AddComponent<EffectPosition>();
		
		if (_effect_placing == 1)
		{
			fxPos.MyTarget = BattleManager.Instance.LeaderCenterTransform;
		}
		else
		{
			if (BattleManager.Instance.LeaderGameObject == null) 
				fxPos.MyTarget = null;
			else
				fxPos.MyTarget = BattleManager.Instance.LeaderGameObject.transform;
		}
	}


	protected virtual void updateEffectPosition() 
	{
		if (BattleManager.Instance != null) return;
		
		EffectPosition fxPos = _effectPrefab.GetComponent<EffectPosition>();
		if (fxPos == null)
			fxPos = _effectPrefab.gameObject.AddComponent<EffectPosition>();
		
		if (_effect_placing == 1)
		{
			fxPos.MyTarget = BattleManager.Instance.LeaderCenterTransform;
		}
		else
		{
			if (BattleManager.Instance.LeaderGameObject == null) 
				fxPos.MyTarget = null;
			else
				fxPos.MyTarget = BattleManager.Instance.LeaderGameObject.transform;
		}
	}


	#endregion



	public virtual void killMe()
	{
		if (_effectPrefab != null && _effectPrefab.gameObject.activeSelf)
		{
//			Log.jprint(Time.time + "   poolDestroy: _effectPrefab: "+_effectPrefab);
			MadPool.Instance().poolDestroy(_effectPrefab);
		}

		Destroy(gameObject);
	}




}
