using UnityEngine;
using System.Collections;

public class Weapon_Hold_Sword : Weapon_Hold
{
	

	
	protected override void installMain(GameObject hero)
	{

		base.installMain(hero);

		//jks install
//		Log.jprint("weapon : " + gameObject + "  scale : " + transform.localScale);
//		Log.jprint("weapon owner : " + character);
		Transform weaponSlot = Utility.findTransformUsingKeyword(hero, "bone_weapon_r");

		if (weaponSlot == null)
		{
			transform.position = new Vector3(0,-1000,0); //jks hack for now
		}
		else
		{
//		Log.jprint("weaponSlot : " + weaponSlot.name + "bone scale : " + weaponSlot.localScale);
		Utility.attachGameObjectToTransform(gameObject, weaponSlot);	
//		Log.jprint("AFTER install weapon : " + gameObject + "  scale : " + transform.localScale);
		}

	}
	
	
	public override void unInstall()
	{
		//jks place item on the ground.
		transform.parent = null;
		transform.rotation = Quaternion.identity;
		transform.position = new Vector3(transform.position.x, 0.05f, transform.position.z);
		base.unInstall();
	}
	
	public override void unInstall_SS()
	{
		//jks place item on the ground.
		transform.parent = null;
		transform.rotation = Quaternion.identity;
		transform.position = new Vector3(transform.position.x, 0.05f, transform.position.z);
		base.unInstall_SS();
	}


}
