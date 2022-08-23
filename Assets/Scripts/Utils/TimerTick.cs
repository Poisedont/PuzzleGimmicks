using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// class use for counting time
/// </summary>
public class TimerCount
{
	bool m_useScale;
	bool m_active;
	float m_elapsed;

	public bool IsPlaying { get { return m_active; } }
	public TimerCount()
	{
		m_useScale = true;
		m_active = false;
		m_elapsed = 0f;
	}
	//================================================================================
	public void SetScaleTime(bool _isScaled)
	{
		m_useScale = _isScaled;
	}
	//================================================================================
	public void Reset()
	{
		m_active = false;
		m_elapsed = 0f;
	}
	//================================================================================
	public void Play()
	{
		m_active = true;
	}
	//================================================================================
	public void Pause()
	{
		m_active = false;
	}
	//================================================================================
	public void Update()
	{
		if (m_active)
		{
			m_elapsed += (m_useScale ? Time.deltaTime : Time.unscaledDeltaTime);
		}
	}
	//================================================================================
	public float ElapsedTime { get { return m_elapsed; } }
}
//================================================================================
/// <summary>
/// Class use for timer counting and event
/// </summary>
public class TimerTick : MonoBehaviour
{   
	/// <summary>
	/// Flag that indicates if the time-scale affects this Timer.
	/// </summary>
	public bool scale = true;
	/// <summary>
	/// Flag that indicates if this View is active.
	/// </summary>        
	public bool active = true;

	/// <summary>
	/// Duration of the timer in seconds per step.
	/// </summary>
	public float duration;

	/// <summary>
	/// Cycles before completion.
	/// </summary>
	[Tooltip("cycle before completion. set -1 if no need complete")]
	public int count;

	/// <summary>
	/// Elapsed time.
	/// </summary>
	[HideInInspector]
	public float elapsed;

	/// <summary>
	/// Current step.
	/// </summary>
	[HideInInspector]
	public int step;

	/// <summary>
	/// Restarts the timer.
	/// </summary>
	public void Restart()
	{
		elapsed = 0f;
		step = 0;
	}

	/// <summary>
	/// Activates the Timer.
	/// </summary>
	public void Play()
	{
		active = true;
	}

	/// <summary>
	/// Stops the Timer and reset its values.
	/// </summary>
	public void Stop()
	{
		active = false;
		Restart();
	}
	/// <summary>
	/// Pause the timer temporary
	/// </summary>
	public void Pause()
	{
		active = false;
	}

	// Use this for initialization
	void Start()
	{

	}
	
	/// <summary>
	/// Updates the timer logic.
	/// </summary>
	void Update()
	{
		if (!active)
		{
			return;
		}

		elapsed += scale ? Time.deltaTime : Time.unscaledDeltaTime;
		if (elapsed >= duration)
		{
			elapsed = 0f;
			
			if (m_onTimerStep != null)
			{
				m_onTimerStep();
			}
			step++;
			if (count >= 0 && step >= count)
			{
				active = false;
				if (m_onTimerComplete != null)
				{
					m_onTimerComplete();
				}
			}
		}
	}
	//================================================================================
	UnityAction m_onTimerStep;
	UnityAction m_onTimerComplete;

	public void SetOnTimerStep(UnityAction timerCallback)
	{
		m_onTimerStep = timerCallback;
	}
	public void SetOnTimerComplete(UnityAction timerCallback)
	{
		m_onTimerComplete = timerCallback;
	}

	public void UnregisterCallbacks()
	{
		m_onTimerComplete = null;
		m_onTimerStep = null;
	}
	//================================================================================
	
}
