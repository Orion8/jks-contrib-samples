using UnityEngine;
using System.Collections;

public class EffectUnderFighter : MonoBehaviour 
{
	GameObject _owner = null;

	bool _isInitialized = false;


	public GameObject Owner
	{
		get { return _owner; }
		set { _owner = value;}
	}

	// one time init
	void Awake()
	{
		init();
	}

	void Update()
	{
		if (BattleManager.Instance.IsBattleEnd)
		{
			if (_isInitialized)
				despawnMe();
		}
	}

	// Pool manager callback
	public void OnSpawned()
	{
		init();
	}
	
//	// Pool manager callback
//	public void OnDespawned()
//	{
//		reset();
//	}


	public void OnDestroy()
	{
		Log.jprint(gameObject + "    OnDestroy()" );
	}


	void init()
	{
		if (_isInitialized) return;

		if (_owner == null)
			_owner = transform.parent.root.gameObject;

		Behavior_Mortal_Fighter behavior = _owner.GetComponent<Behavior_Mortal_Fighter>();
		if (behavior == null)
		{
			despawnMe();
			return;
		}

		behavior.registerDeathEvent(callbackWhenFighterIsDead);

		_isInitialized = true;
	}



	void despawnMe()
	{
		reset();
		MadPool.Instance().poolDestroy(transform);
	}



	void reset()
	{
		if (!_isInitialized) return;

		if (_owner)
		{
			Behavior_Mortal_Fighter behavior = _owner.GetComponent<Behavior_Mortal_Fighter>();
			if (behavior)
				behavior.unRegisterDeathEvent(callbackWhenFighterIsDead);
		}

		transform.parent = MadPool.Instance()._pool.transform;

		_owner = null;
		_isInitialized = false;
	}

	

	public void callbackWhenFighterIsDead()
	{
		if (_isInitialized)
			despawnMe();
	}




}
