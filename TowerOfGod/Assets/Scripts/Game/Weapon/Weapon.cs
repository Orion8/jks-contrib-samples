using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour 
{
	public enum eWeaponType
	{
		WT_None,
		WT_Sword,			//jks - short or medium size sword.
		WT_Sword_Twin,		//jks - two short or medium size swords for each hand.
		WT_Sword_Long,		
		WT_Spear,
		WT_Bow,
	}	
	
	
	protected GameObject _owner = null;
	protected Knowledge_Mortal_Fighter _ownerKnowledge = null;
	public Knowledge_Mortal_Fighter OwnerKnowledge 
	{ 
		get 
		{ 
			if (_ownerKnowledge == null)
				_ownerKnowledge = _owner.GetComponent<Knowledge_Mortal_Fighter>();
			
			return _ownerKnowledge; 
		}
	}

	protected string _pairItemPath = string.Empty;


	public GameObject getOwner()
	{
		return _owner;
	}


	public void setOwner(GameObject newOwner)
	{
		_owner = newOwner;
	}
	
	
	public virtual bool IsPairWeapon { get { return false; }}




}
