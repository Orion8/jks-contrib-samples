using UnityEngine;
using System.Collections;

public class AttackCoolTime_GuardUp : AttackCoolTime 
{

	//jks send cool time finish event whoever want to know about it.
//	public delegate void cooltimeFinishedEvent2();
//	public event cooltimeFinishedEvent2 _onCooltimeFinished2 = null;
//	public void registerCooltimeFinishedEvent2(cooltimeFinishedEvent2 func)   { _onCooltimeFinished2 += func; }
//	public void unRegisterCooltimeFinishedEvent2(cooltimeFinishedEvent2 func) { _onCooltimeFinished2 -= func; }



	public override void reset()
	{
		_enable = false;
		_elapsedTime = 0;

		if (_battleUICooltime != null && _cardDeckIndex >= 0)
		{
			_battleUICooltime[_cardDeckIndex].NowCool = 0;
			BattleBase.Instance.doneUICoolTime_GuardUp(_cardDeckIndex);
		}
	}


	public override void activateTimer()
	{
		_enable = true;	
		_elapsedTime = 0;

		if (_name.Contains("P1")) //jks if user player
			BattleBase.Instance.showUICoolTime_GuardUp(_cardDeckIndex);

//		if (_cardDeckIndex != -1)
//			Log.jprint(Time.time+ "   "+ _name +"    oooooo       card: "+_cardDeckIndex + "   activate timer .........>>>>>>>>>>>>>>>");
	}
	
	public override void activateTimer(float time)
	{
		_enable = true;
		_coolTime = time;
		_elapsedTime = 0;

		if (_name.Contains("P1")) //jks if user player
			BattleBase.Instance.showUICoolTime_GuardUp(_cardDeckIndex);


//		if (_cardDeckIndex != -1)
//			Log.jprint(Time.time +"   "+ _name +"    oooooo       card: "+_cardDeckIndex + "   activate timer -------->>>>>>>>>>>>>>> time: "+time);
	}

	
	protected override void Update()
	{
		 
		if (_enable && 
		    Time.timeScale > 0 &&
		    BattleBase.Instance.IsBattleInProgress)
		{
			_elapsedTime += Time.unscaledDeltaTime;

			if (_battleUICooltime != null && _cardDeckIndex >= 0)
			{
				_battleUICooltime[_cardDeckIndex].NowCool = _elapsedTime;
				//Log.jprint(" role skil cooltime Max: "+_coolTime+ "      _elased"+_elapsedTime);
			}


			if (_elapsedTime >= _coolTime)
			{
				//_onCooltimeFinished2();
				reset();
				//Log.jprint(" role skil cooltime Max: "+_coolTime+ "      _elased"+_elapsedTime);
			}
		}
	}

	

}
