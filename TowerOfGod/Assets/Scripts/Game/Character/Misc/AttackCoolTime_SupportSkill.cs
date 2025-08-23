using UnityEngine;
using System.Collections;

public class AttackCoolTime_SupportSkill : AttackCoolTime 
{
	protected int _skillIndex = -1; // 0 ~ 2  (three support skills)


	public override void reset()
	{
//		if (_skillIndex != -1)
//			Log.jprint(Time.time +"xxxxxxxx        _skillIndex: "+_skillIndex +"    _elapsedTime: "+ _elapsedTime +"    _coolTime: "+ _coolTime);

		_enable = false;
		_elapsedTime = 0;
		
		if (_battleUICooltime != null && _skillIndex >= 0)
		{
			_battleUICooltime[_skillIndex].NowCool = 0;
			BattleBase.Instance.doneUICoolTime_SupportSkill(_skillIndex);
		}
	}
	

//	public override void activateTimer()
//	{
//		_enable = true;
//		_elapsedTime = 0;
//		
//		if (_skillIndex != -1)
//			Log.jprint(Time.time +" sssssss       _skillIndex: "+_skillIndex + "   activate timer .........>>>>>>>>>>>>>>>");
//	}
//	
//	public override void activateTimer(float time)
//	{
//		_enable = true;
//		_coolTime = time;
//		_elapsedTime = 0;
//		
//		if (_skillIndex != -1)
//			Log.jprint(Time.time +" sssssss       _skillIndex: "+_skillIndex + "   activate timer -------->>>>>>>>>>>>>>> time: "+time);
//	}


	protected override void Update()
	{
		
		if (_enable && 
		    Time.timeScale > 0 &&
		    BattleBase.Instance.IsBattleInProgress)
		{
			_elapsedTime += Time.unscaledDeltaTime;
			
			if (_battleUICooltime != null && _skillIndex >= 0)
			{
				_battleUICooltime[_skillIndex].NowCool = _elapsedTime;
				//Log.jprint(" role skil cooltime Max: "+_coolTime+ "      _elased"+_elapsedTime);
			}
			
			
			if (_elapsedTime >= _coolTime)
			{
				reset();
				//Log.jprint(" role skil cooltime Max: "+_coolTime+ "      _elased"+_elapsedTime);
			}
		}
	}
}
