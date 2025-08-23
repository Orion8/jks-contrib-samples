using UnityEngine;
using System.Collections;

public class Weapon_Hold_Sword_Twin : Weapon_Hold_Sword
{

	protected GameObject _pairSwordGo;
	public GameObject setPairSwordGo { set { _pairSwordGo = value; }}

	public override bool IsPairWeapon { get { return true; }}
	
		
	public override IEnumerator install(GameObject hero)
	{
		yield return StartCoroutine( base.install(hero));
		yield return StartCoroutine( installPair(hero));
	}

	public override void install_SS(GameObject hero)
	{
		base.install_SS(hero);
		installPair_SS(hero);
	}

	protected void installPair_SS(GameObject hero)
	{
		Transform slot = Utility.findTransformUsingKeyword(hero, "bone_weapon_l");
		if (slot == null) return;

		Utility.attachGameObjectToTransform(_pairSwordGo, slot);

		//jks - the pair item is just for visual. so remove script component if it has one.
		Weapon script = _pairSwordGo.GetComponent<Weapon>();
		if (script != null)
		{
			Destroy(script);
		}		
	}



	protected virtual IEnumerator installPair(GameObject hero)
	{
		//jks install pair
		//Log.jprint("_pairItemPath : " + _pairItemPath);
		Transform slot = Utility.findTransformUsingKeyword(hero, "bone_weapon_l");
		if (slot == null)
		{
			yield break;
		}


		GameObject pairItem = null;

		GameObject weaponRef = hero.GetComponent<Knowledge_Mortal_Fighter>()._weaponAssetRef;

		if (weaponRef == null)
		{
			Vector3 position = new Vector3(0,0,0);

			//jks 스킬프리뷰에서 첫 프레임에 보이지 않게 하기 위함. //////////////  >>>>
			if (BattleBase.Instance.isSkillPreviewMode())
				position.x = 1000;
			//jks 스킬프리뷰에서 첫 프레임에 보이지 않게 하기 위함. //////////////  <<<<<


			yield return StartCoroutine(ResourceManager.co_InstantiateFromBundle(_pairItemPath, position, Quaternion.identity, obj => pairItem = (GameObject)obj));

			if (pairItem == null)
			{
				Debug.LogError("Can't find asset : "+ _pairItemPath);
			}

			//jks 스킬프리뷰에서 첫 프레임에 보이지 않게 하기 위함. //////////////  >>>>
			if (BattleBase.Instance.isSkillPreviewMode())
			{
				setPairSwordGo = pairItem;
				show(false);
				pairItem.transform.position += new Vector3 (-1000, 0, 0);
			}
			//jks 스킬프리뷰에서 첫 프레임에 보이지 않게 하기 위함. //////////////  <<<<<

		}
		else
		{
			pairItem = (GameObject)Instantiate(weaponRef);
		}



		pairItem.tag = "Untagged";

		Utility.attachGameObjectToTransform(pairItem, slot);
		_pairSwordGo = pairItem;
		
		//jks - the pair item is just for visual. so remove script component if it has one.
		Weapon script = pairItem.GetComponent<Weapon>();
		if (script != null)
		{
			Destroy(script);
		}		
	}




	
	public override void unInstall()
	{
		//jks remove pair item
		if (_pairSwordGo) 			
		{
			Destroy(_pairSwordGo);
		}

		//jks place item on the ground.
		base.unInstall();
	}

	public override void unInstall_SS()
	{
		//jks uninstall pair item.
		if (_pairSwordGo) 			
		{
			//jks place item on the ground.
			_pairSwordGo.transform.parent = null;
			_pairSwordGo.transform.rotation = Quaternion.identity;
			_pairSwordGo.transform.position = new Vector3(transform.position.x, 0.05f, transform.position.z);
		}

		base.unInstall_SS();
	}

	
	
	//jks use it when you need to change body(upper) parts of Bam character.
	public override void putAside()
	{
		base.putAside();
		if ( _pairSwordGo != null)
		{
			_pairSwordGo.transform.parent = _pairSwordGo.transform.root;
		}
	}
	
	
	public override void putBack()
	{
		GameObject character = gameObject.transform.root.gameObject;
		installMain(character);		

		if ( _pairSwordGo != null)
		{
			GameObject hero = gameObject.transform.root.gameObject;
			Transform slot = Utility.findTransformUsingKeyword(hero, "Bone_Weapon_L");
			Utility.attachGameObjectToTransform(_pairSwordGo, slot);
		}
	}
	
	public override void setPairItemPath(string path)
	{
		_pairItemPath = path;
	}


	public override void show(bool bShow)
	{
		base.show(bShow);

		if (_pairSwordGo)
		{
			Renderer[] renderers = _pairSwordGo.GetComponentsInChildren<Renderer>();
			
			if (renderers != null && renderers.Length > 0) 
			{
				foreach (Renderer rend in renderers)
				{
					rend.enabled = bShow;
				}
			}

		}
	}

	public override void hide() //jks for install hide only
	{
		base.hide();

		if (_pairSwordGo)
		{
			Renderer[] renderers = _pairSwordGo.GetComponentsInChildren<Renderer>();

			if (renderers != null && renderers.Length > 0) 
			{
				foreach (Renderer rend in renderers)
				{
					rend.enabled = false;
				}
			}

		}
	}

}
