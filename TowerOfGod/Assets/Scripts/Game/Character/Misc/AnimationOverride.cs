using UnityEngine;
using System.Collections;





public class AnimationOverride : MonoBehaviour 
{
	
	protected Animator _animator;
	protected Knowledge_Mortal_Fighter _knowledge;
	protected GameObject _proxyTargetGO = null;


	GameObject ProxyTargetGO
	{
		get
		{
			if (_proxyTargetGO == null)
			{
				_proxyTargetGO = new GameObject("Projectile Proxy Target");
				_proxyTargetGO.transform.parent = transform;
			}

			return _proxyTargetGO;
		}
	}

	void Start ()
	{
		_animator = GetComponent<Animator>();	
		_aimWeight = 0;
		_lookWeight = 0;
		_knowledge = GetComponent<Knowledge_Mortal_Fighter>();
	}


	void LateUpdate() //jks use "LateUpdate()" to override bone rotation which was set in animation update.
	{
		updateAim();

		updateLook();
	}

	

	#region aim

	bool _aimNow = false;
	float _aimWeight = 0;					// the amount to transition when using head look
	float _aimSmoother = 4f;				// a smoothing setting for camera motion
	Transform _objToAimAt = null;						// a transform to Lerp the camera to during head look
	Transform _spineBone;
	/// The local axis of the Transform that you want to be aimed at IKPosition.
	Vector3 axis = Vector3.forward; //jks - brought from "Final IK" plugin.


	public void aimBegin(Transform target)
	{
		if (_knowledge == null) return;
		//Log.jprint(gameObject.name + " + + + + aimBegin 1 ");
		if (Utility.getNthDigit( _knowledge.AttackInfo3, 1) == 1) return; //jks- means parallel projectile, so do not adjust spine

		_objToAimAt = target;
		_aimWeight = 0;
		_aimNow = true;
	}

	public void aimBegin(Vector3 target)
	{
		if (_knowledge == null) return;
		//Log.jprint(gameObject.name + " + + + + aimBegin 2 ");
		if (Utility.getNthDigit( _knowledge.AttackInfo3, 1) == 1) return; //jks- means parallel projectile, so do not adjust spine

		_objToAimAt = ProxyTargetGO.transform;
		_objToAimAt.position = target;
		_aimWeight = 0;
		_aimNow = true;
	}
	
	public void aimEnd()
	{
		//Log.jprint(gameObject.name + " - - - - aimEnd  ");
		_aimNow = false;
		_aimWeight = 0;
	}


	void updateAim()
	{
		if(_objToAimAt == null) return;

		update_aim_weight();

		rotate_spine();
	}


	void update_aim_weight()
	{
		if (!_aimNow && _aimWeight == 0) return;
		
		if(_aimNow)
		{
			_aimWeight = Mathf.Lerp(_aimWeight,1f,Time.deltaTime*_aimSmoother);
		}
		// else, return to using animation for the head by lerping back to 0 for look at weight
		else if (_aimWeight > 0.01f)
		{
			_aimWeight = Mathf.Lerp(_aimWeight,0f,Time.deltaTime*_aimSmoother);
		}
		else 
		{
			_aimWeight = 0;
		}
	}
	

	void rotate_spine()
	{
		if (_aimWeight == 0) return;

		RotateToTarget(_objToAimAt.position, SpineBone, _aimWeight);
	}
	
	
	// Rotating bone to get transform aim closer to target  //jks - brought from "Final IK" plugin.
	void RotateToTarget(Vector3 targetPosition, Transform bone, float weight) 
	{
		Quaternion rotationOffset = Quaternion.FromToRotation(transformAxis, targetPosition - getAimBasePosition());

		//jks clamp between -0.3 ~ 0.3 to avoid unnatural bending look.
		if (rotationOffset.z < -0.3f)
			rotationOffset.z = -0.3f;
		else if (rotationOffset.z > 0.3f)
			rotationOffset.z = 0.3f;
		
		bone.rotation = Quaternion.Lerp(Quaternion.identity, rotationOffset, weight) * bone.rotation;	

		//Log.jprint("++++++++++  RotateToTarget()    rotationOffset: " + rotationOffset);


		//Log.jprint("++++++++++  RotateToTarget()    targetPosition: " + targetPosition);
	}


	public Transform SpineBone
	{
		get 
		{
			if (_spineBone == null)
			{
				_spineBone = Utility.findTransformUsingKeyword(gameObject, "spine1");
				if (_spineBone == null)
				{
					Debug.LogWarning(gameObject + "Can not find spine bone to rotate for aiming. ! ");
				}
			}
			
			return _spineBone;
		}
	}
	
	//jks - brought from "Final IK" plugin.
	public Vector3 transformAxis 
	{
		get 
		{
			return transform.rotation * axis;
		}
	}


//	Launcher_Projectile Launcher_Projectile = null;

//jks	Vector3 getAimBasePosition()
//	{
//		if (Launcher_Projectile == null)
//		{
//			Launcher_Projectile = GetComponent<Launcher_Projectile>();
//		}
//
//		if (Launcher_Projectile == null)
//		{
//			return transform.position;
//		}
//
//		return Launcher_Projectile.SpawnPosition;
//	}
	Vector3 getAimBasePosition()
	{
		return SpineBone.position;
	}

	#endregion aim





	#region look

	bool _lookNow = false;
	float _lookWeight = 0;					// the amount to transition when using head look
	float _lookSmoother = 5f;				// a smoothing setting for camera motion
	Transform _objToLookAt = null;						// a transform to Lerp the camera to during head look

	public void lookBegin(Transform target)
	{
		_objToLookAt = target;
		_lookNow = true;
	}

	public void lookEnd()
	{
		_lookNow = false;
	}


	void updateLook()
	{
		if (_objToLookAt == null) return;

		update_look_weight();

		rotate_head();
	}
	

	void update_look_weight()
	{
		if (!_lookNow && _lookWeight == 0) return;

		if(_lookNow)
		{
			_lookWeight = Mathf.Lerp(_lookWeight,1f,Time.deltaTime*_lookSmoother);
		}
		// else, return to using animation for the head by lerping back to 0 for look at weight
		else if (_lookWeight > 0.01f)
		{
			_lookWeight = Mathf.Lerp(_lookWeight,0f,Time.deltaTime*_lookSmoother);
		}
		else 
		{
			_lookWeight = 0;
		}
	}


	void rotate_head()
	{
		if (_lookWeight == 0) return;

		_animator.SetLookAtWeight(_lookWeight);					// set the Look At Weight - amount to use look at IK vs using the head's animation
		// ...set a position to look at with the head, and use Lerp to smooth the look weight from animation to IK (see line 54)
		_animator.SetLookAtPosition(_objToLookAt.position);
	}

	#endregion look	





	//jks to use this callback, check on "IK Pass" at Layer widget of Animator window.
//	void OnAnimatorIK(int layerIndex) 
//	{
//		if (_objToAimAt == null) return;
//		if (_lookWeight < 0.01f) return;
//
//		Quaternion handRotation = Quaternion.LookRotation(_objToAimAt.position - transform.position);
//		_animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, _lookWeight);
//		_animator.SetIKRotation(AvatarIKGoal.LeftHand, handRotation);
//
//		_animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, _lookWeight);		
//		_animator.SetIKPosition(AvatarIKGoal.LeftHand, _objToAimAt.localPosition);
//	}





}
