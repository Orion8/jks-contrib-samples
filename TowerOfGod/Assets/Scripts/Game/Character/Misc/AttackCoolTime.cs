using UnityEngine;
using System.Collections;

public class AttackCoolTime : MonoBehaviour 
{

	//jks send cool time finish event whoever want to know about it.
	public delegate void cooltimeFinishedEvent();
	public event cooltimeFinishedEvent _onCooltimeFinished = null;
	public void registerCooltimeFinishedEvent(cooltimeFinishedEvent func)   { _onCooltimeFinished += func; }
	public void unRegisterCooltimeFinishedEvent(cooltimeFinishedEvent func) { _onCooltimeFinished -= func; }


	public string _name = "";

	protected bool _enable = false;

	protected float _coolTime = 0;

	protected float _elapsedTime = 0;

	protected int _cardDeckIndex = -1;

	protected CoolTime[] _battleUICooltime;


	public int CardDeckIndex
	{
		set { _cardDeckIndex = value; }
	}

	public CoolTime[] BattleUICooltime
	{
		set { _battleUICooltime = value; }
	}

	public float CoolTime
	{
		get { return _coolTime; }
		set { 
			_coolTime = value; 
			//Log.jprint(_cardDeckIndex + "cool time = " + _coolTime);
		}
	}
	

	public float TimeLeft
	{
		get { return _coolTime - _elapsedTime; }
	}

	public bool IsEnabled
	{
		get { return _enable; }
	}

	public bool IsCoolingInProgress
	{
		get { return _enable && _elapsedTime > 0; }
	}

	public virtual void reset()
	{
//		if (_cardDeckIndex != -1)
//			Log.jprint(Time.time +"   "+ _name +"    xxxxxxxx        card: "+_cardDeckIndex +
//			           "    _elapsedTime: "+ _elapsedTime +"    _coolTime: "+ _coolTime);

		_enable = false;
		_elapsedTime = 0;

		if (_name.Contains("P1")) //jks if user player
		{
			BattleUI.Instance().endCoolTime(_cardDeckIndex); //BattleBase.Instance.doneUICoolTime(_cardDeckIndex);
//			Log.jprint(Time.time + "   cool time :::: reset timer <<<<<<<<<<<<<<......."+ _name);
			setCardUseFinishedFlag();
		}

//		if (_cardDeckIndex != -1)
//			Log.jprint(_cardDeckIndex + "   reset timer <<<<<<<<<<<<<<.......");
	}



	void setCardUseFinishedFlag()
	{
		GameObject fighter = BattleBase.Instance.List_Ally[_cardDeckIndex]._go;
		if (fighter == null) return;
			
		Knowledge_Mortal_Fighter_Main fighterKnow = fighter.GetComponent<Knowledge_Mortal_Fighter_Main>();
		if (fighterKnow == null) return;
			
		fighterKnow.IsCardUseFinishied = true;
	}



	public virtual void activateTimer()
	{
		if (! BattleBase.Instance.IsBattleInProgress) return;

		_enable = true;	
		_elapsedTime = 0;

		if (_name.Contains("P1")) //jks if user player
			BattleBase.Instance.showUICoolTime(_cardDeckIndex);

//		if (_cardDeckIndex == 0)
//			Log.jprint(Time.time+ "   "+ _name +"    oooooo       card: "+_cardDeckIndex + "   activate timer .........>>>>>>>>>>>>>>>");
	}
	
	public virtual void activateTimer(float time)
	{
		_enable = true;
		_coolTime = time;
		_elapsedTime = 0;

//		if (_cardDeckIndex == 0)
//			Log.jprint(Time.time +"   "+ _name +"    oooooo       card: "+_cardDeckIndex + "   activate timer -------->>>>>>>>>>>>>>> time: "+time);
	}
	


	float cooltimeToUse;
	protected virtual void Update()
	{
		if (! _enable) return;
		if (! BattleBase.Instance.IsBattleInProgress) return;
		if (BattleBase.Instance.IsIgnoreButtonTouch) return;
		if (BattleBase.Instance.IsDialogShowOff) return; //jks 보스 대화 중엔 쿨타임 갱신하지 않음.
		if (Time.timeScale == 0) return;

		_elapsedTime += Time.unscaledDeltaTime;


		if (_battleUICooltime != null && _cardDeckIndex >= 0)
		{
			if (_name.Contains("P1"))
				_battleUICooltime[_cardDeckIndex].NowCool = _elapsedTime;

			//Log.jprint(Time.time +"    gameObject: "+ gameObject.name +"    _cardDeckIndex: "+_cardDeckIndex +"    _elapsedTime: "+ _elapsedTime );
		}


		cooltimeToUse = _coolTime;

		if (_elapsedTime >= cooltimeToUse)
		{
			_onCooltimeFinished();
			reset();
			if (_battleUICooltime != null && _cardDeckIndex >= 0 && _name.Contains("P1"))
				_battleUICooltime[_cardDeckIndex].NowCool = 0;
		}
	}

	

}
