using UnityEngine;
using System.Collections;

public class Weapon_Hold : Weapon
{
	
	protected bool _installed = false;

	public virtual void putBack() {}
	public virtual void setPairItemPath(string path) {}

	public virtual IEnumerator install(GameObject hero)
	{
		//jks uninstall previously possessed weapon first
		Weapon_Hold weapon =  hero.GetComponentInChildren<Weapon_Hold>();
		if (weapon != null)
		{
			weapon.unInstall();
		}
		
		_owner = hero;
		_installed = true;
		
		hero.GetComponent<Knowledge_Mortal_Fighter>().Weapon = gameObject;
		
		installMain(hero);

		yield break;
	}

	protected virtual void installMain(GameObject hero) 
	{
		_owner = hero;
	}

	

	public virtual void unInstall()
	{
		_owner = null;
		_installed = false;
	}

	/// <summary>
	/// Install weapon for support skill.
	/// </summary>
	public virtual void install_SS(GameObject hero)
	{
		installMain(hero);
	}

	public virtual void unInstall_SS()
	{
		_owner = null;
		_installed = false;
	}

	
	//jks use it when you need to change body(upper) parts of Bam character.
	public virtual void putAside()
	{
		gameObject.transform.parent = gameObject.transform.root;
	}
	

	//	bool _isFirstSetup = true;
	public virtual void show(bool bShow)
	{
		//		Debug.Log("_isFirstSetup : " + _isFirstSetup + " / owner : " + bShow);


		Renderer[] renderers = GetComponentsInChildren<Renderer>();

		if (renderers != null && renderers.Length > 0) 
		{
			foreach (Renderer rend in renderers)
			{
				rend.enabled = bShow;
			}
		}
	
		if (bShow) 
		{
			if (_owner)
				_owner.GetComponent<Knowledge_Mortal_Fighter>().WeaponObject_Current = gameObject;
		}


//		if (_owner && _owner.name.Contains("C"))
//			Log.jprint(_owner.name + " _______________show weapon_____ : " + bShow + "      current weapon: " + _owner.GetComponent<Knowledge_Mortal_Fighter>().WeaponObject_Current);


		WeaponCaseEffect effect = gameObject.GetComponent<WeaponCaseEffect>();
		if (effect == null) return;

		if (bShow) 
			effect.showOpenEffect();
		else 
			effect.showRemoveEffect();


		//		if (_isFirstSetup)
		//		{
		//			_isFirstSetup = false;
		//			return;
		//		}
	}


	public virtual void hide() //jks for install hide only
	{
		Renderer[] renderers = GetComponentsInChildren<Renderer>();
		
		
		if (renderers != null && renderers.Length > 0) 
		{
			foreach (Renderer rend in renderers)
			{
				rend.enabled = false;
			}
		}
	}

}
