//#define DEBUG_LOG
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using React;

//This is a little shortcut which creates an alias for a type, and makes out Action methods look much nicer.
using  Action = System.Collections.Generic.IEnumerator<React.NodeResult>;

public class Behavior_Mortal : Behavior
{

	protected Locomotion _locomotion = null;

	//jks send death event whoever want to know about it.
	public delegate void DeathEvent();
	public event DeathEvent _onDeath = null;
	
	public void registerDeathEvent(DeathEvent func)   { _onDeath += func; }
	public void unRegisterDeathEvent(DeathEvent func) { _onDeath -= func; }
	
	
	new protected Knowledge_Mortal Knowledge
	{
		get
		{
			if (_knowledge == null)
			{
				_knowledge = GetComponent<Knowledge>();
			}
			return (Knowledge_Mortal)_knowledge;
		}
	}
	

	protected override void initializeBeforeUpdateBegin()
	{
		base.initializeBeforeUpdateBegin();
		_locomotion = GetComponent<Locomotion>();
	}



	protected virtual void processDeath()
	{

		//Log.jprint(gameObject + "processDeath ......................");
		//tag = "TOG_Dead";
		
		CallDeathEvent(); //jks - send death event

		if (BattleUI.Instance() != null)
			BattleUI.Instance().renewEnemyInfo();

		hideHealthBar();
		destroyMe();
	}


	protected void hideHealthBar()
	{
		DisplayHealthBar healthBar = GetComponent<DisplayHealthBar>();
		if (healthBar != null)
			healthBar.activeBar( false );
	}

	protected void CallDeathEvent()
	{
		if(_onDeath != null) _onDeath(); //jks - send death event
	}


	protected virtual void destroyMe()
	{
		Destroy(gameObject, 2.8f);
	}


	#if DEBUG_LOG		
	void Update()
	{
		Log.print("Update...");
	}
	#endif


	#region REACT (BEHAVIOR TREE)

	public virtual Action battlePaused()
	{
		#if DEBUG_LOG		
		Log.print("battlePaused");
		#endif
		if (Knowledge.IsBattlePaused)
		{
			yield return React.NodeResult.Success;

		}
		yield return React.NodeResult.Failure;
	}
	
	public virtual Action battleVictory()
	{
		#if DEBUG_LOG		
		Log.print("battleVictory");
		#endif
		if (Knowledge.IsBattleVictorious)
		{
			yield return React.NodeResult.Success;

		}
		yield return React.NodeResult.Failure;
	}
	
	public virtual Action battleFailure()
	{
		#if DEBUG_LOG		
		Log.print("battleFailure");
		#endif
		if (Knowledge.IsBattleFailed)
		{
			yield return React.NodeResult.Success;

		}
		yield return React.NodeResult.Failure;
	}
	
	public virtual Action death()
	{
		#if DEBUG_LOG		
		Log.print("death");
		#endif
		yield return React.NodeResult.Failure;
	}

	public virtual Action deathB()
	{
		#if DEBUG_LOG		
		Log.print("death");
		#endif
		yield return React.NodeResult.Failure;
	}

	public virtual Action hit()
	{
		#if DEBUG_LOG		
		Log.print("hit");
		#endif
		yield return React.NodeResult.Failure;
	}
	
	public virtual Action knockDown()
	{
		#if DEBUG_LOG		
		Log.print("knockDown");
		#endif
		yield return React.NodeResult.Failure;
	}

	public virtual Action knockDown_Getup()
	{
		#if DEBUG_LOG		
		Log.print("knockDown_Getup");
		#endif
		yield return React.NodeResult.Failure;
	}
	
	public virtual Action jump()
	{
		#if DEBUG_LOG		
		Log.print("jump");
		#endif
		yield return React.NodeResult.Failure;
	}
	
	public virtual Action walkBack()
	{
		#if DEBUG_LOG		
		Log.print("walkBack");
		#endif
		yield return React.NodeResult.Failure;
	}
	
	public virtual Action walkFast()
	{
		#if DEBUG_LOG		
		Log.print("walkFast");
		#endif
		yield return React.NodeResult.Failure;
	}
	
	public virtual Action run()
	{
		#if DEBUG_LOG		
		Log.print("run");
		#endif
		yield return React.NodeResult.Failure;
	}
	
	public virtual Action sliding()
	{
		#if DEBUG_LOG		
		Log.print("sliding");
		#endif
		yield return React.NodeResult.Failure;
	}

	public virtual Action duckWalkFast()
	{
		#if DEBUG_LOG		
		Log.print("duckWalkFast");
		#endif
		yield return React.NodeResult.Failure;
	}

	public virtual Action sit()
	{
		#if DEBUG_LOG		
		Log.print("sit");
		#endif
		yield return React.NodeResult.Failure;
	}
	
	public virtual Action idle()
	{
		#if DEBUG_LOG		
		Log.print("idle");
		#endif
		yield return React.NodeResult.Failure;
	}

	public virtual Action walk()
	{
		#if DEBUG_LOG		
		Log.print("walk");
		#endif
		yield return React.NodeResult.Failure;
	}
	
	#endregion


}
