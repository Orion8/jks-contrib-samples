using UnityEngine;
using System.Collections;
using CodeStage.AntiCheat.ObscuredTypes;

public enum eHitType 
{
	HT_NONE,
	HT_CRITICAL,
	HT_GOOD,
	HT_NICE,
	HT_MISS,
	HT_BAD,
    HT_HEAL
}

public enum eAllyID
{
	Ally_Bot_Opponent, 		//jks bot enemy
	Ally_Human_Me, 			//jks client player
	Ally_Human_Opponent  	//jks network player
}

public class Knowledge_Mortal : Knowledge
{
	public eAllyID _ally_ID = 0;

//	protected float[] _reactionDistance;
	protected float _radius;
	protected float _height;
	//protected float _weight;
//jks obsolete	protected int _animGroupByWeaponID;
	protected ObscuredInt _max_hp;
	protected ObscuredInt _cur_hp;

	protected Transform _objectCenter;

	protected eHitType _recentHitType;



	public eHitType RecentHitType { get { return _recentHitType; } set {_recentHitType = value;}}

	public ObscuredBool IsDead { get { return _cur_hp <= 0; } }

	public ObscuredInt Current_HP { get { return _cur_hp; } set {_cur_hp = value;}}
	public ObscuredInt Max_HP { get { return _max_hp; } set {_max_hp = value;}}

	public float Height { get { return _height; } }
	public float Radius { get { return _radius; } }

	//jsm
	protected GameObject _attacker;
	public GameObject Attacker 
	{
		get { return _attacker; }
		set { _attacker = value; }
	}

	public Vector3 CenterPosition
	{
		get
		{
			if (_objectCenter == null)
			{
				_objectCenter = Utility.findTransformUsingKeyword(gameObject, "spine");
				if (_objectCenter == null)
				{
					_objectCenter = Utility.findTransformUsingKeyword(gameObject, "bone_weapon");
					if (_objectCenter == null)
					{
						Log.Warning(gameObject + "No bone to find center position. Used gameObject position for now.");
						_objectCenter = gameObject.transform;
					}
				}
			}
			return _objectCenter.position;
		}
	}

	public Transform CenterTransform
	{
		get
		{
			if (_objectCenter == null)
			{
				_objectCenter = Utility.findTransformUsingKeyword(gameObject, "spine");
				if (_objectCenter == null)
				{
					_objectCenter = Utility.findTransformUsingKeyword(gameObject, "bone_weapon");
					if (_objectCenter == null)
					{
						Log.Warning(gameObject + "No bone to find center position. Used gameObject position for now.");
						_objectCenter = gameObject.transform;
					}
				}
			}
			return _objectCenter;
		}
	}

	public Transform CenterTransformForDamage
	{
		get
		{
			if (_objectCenter == null)
			{
				_objectCenter = Utility.findTransformUsingKeyword(gameObject, "Pelvis");
				if (_objectCenter == null)
				{
					_objectCenter = Utility.findTransformUsingKeyword(gameObject, "bone_weapon");
					if (_objectCenter == null)
					{
						Log.Warning(gameObject + "No bone to find center position. Used gameObject position for now.");
						_objectCenter = gameObject.transform;
					}
				}
			}
			return _objectCenter;
		}
	}
	


	public eAllyID AllyID
	{
		get { return _ally_ID; }
		set { _ally_ID = value; }
	}
	
	public bool IsAlly (Knowledge_Mortal subject)
	{ 
		return _ally_ID == subject.AllyID;
	}
	

	public virtual bool AmIWall
	{
		get { return false; }
	}


//	public virtual float getReactionDistance()
//	{
//		Log.jprint("reactionDistance : " + _reactionDistance[(int)_hitType]);
//		return _reactionDistance[(int)_hitType];
//	}



	public virtual void updateHP(ObscuredInt delta, eHitType hitType)
	{
		_cur_hp += delta;

		if (_cur_hp < 0)
			_cur_hp = 0;

		if (_cur_hp > _max_hp)
			_cur_hp = _max_hp;
	}



	public virtual void SetRadius(float radius)
	{
		_radius = radius;
	}


	public virtual int takeDamage(ObscuredInt damage)
	{
		if (damage <= 0) return damage;

		updateHP(-damage, eHitType.HT_GOOD);

		if (IsDead)
		{
			//jks do death action
		}	
		return damage;
	}

	public virtual ObscuredInt takeDamage(ObscuredInt damagePoint, int reactionAnimID, eHitType hitType, eAttackType attackType, eWeaponType_ForAnimation weaponType, GameObject attacker, float reactionDistanceOverride)
	{
		return damagePoint;
	}
	
	public virtual bool IsClassMatching
	{
		get { return false; }
	}



}
