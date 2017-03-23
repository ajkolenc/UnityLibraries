using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : CustomYieldInstruction {

	static Queue<Timer> timerPool = new Queue<Timer>();

	/// <summary>
	/// Create a new timer that can either be yielded on or queried for the current completion percent. Uses a pooled Timer where available to avoid garbage collection
	/// </summary>
	/// <param name="duration">The duration of the timer.</param>
	/// <param name="useRealtime">If set to <c>true</c>, use realtime instead of game time.</param>
	/// <param name="onTick">An action to be executed each frame. The parameter is a value between 0 and 1 that represents the percent completion of the timer. If present, assumes that the timer is yielded upon.</param>
	public static Timer Create(float duration, bool useRealtime, System.Action<float> onTick = null){
		Timer t = null;
		if (timerPool.Count > 0) {
			t = timerPool.Dequeue ();
			t.Duration = duration;
			t.UsingRealtime = useRealtime;
			t.tick = onTick;
			t.StartTime = t.getTime ();
			t.DoTick ();
		}
		else {
			t = new Timer (duration, useRealtime, onTick);
			t.pooled = true;
		}
		return t;
	}

	public static Timer Create(float duration, System.Action<float> onTick = null){
		return Create (duration, false, onTick);
	}

	/// <summary>
	/// Releases a timer back into the pool. This is automatically called if the timer is yielded upon
	/// </summary>
	/// <param name="t">The timer to release</param>
	public static void Release(Timer t){
		timerPool.Enqueue (t);
		t.pooled = true;
	}

	public float StartTime {
		get;
		private set;
	}

	public bool UsingRealtime {
		get;
		private set;
	}

	public float Duration {
		get;
		private set;
	}

	public bool IsFinished {
		get {
			return (getTime() - StartTime) >= Duration;
		}
	}

	System.Action<float> tick;
	bool pooled = false;

	/// <summary>
	/// Create a new timer that can either be yielded on or queried for the current completion percent
	/// </summary>
	/// <param name="duration">The duration of the timer.</param>
	/// <param name="useRealtime">If set to <c>true</c>, use realtime instead of game time.</param>
	/// <param name="onTick">An action to be executed each frame. The parameter is a value between 0 and 1 that represents the percent completion of the timer. If present, assumes that the timer is yielded upon.</param>
	public Timer(float duration, bool useRealtime, System.Action<float> onTick = null){
		Duration = duration;
		UsingRealtime = useRealtime;
		StartTime = getTime ();
		tick = onTick;
		DoTick ();
	}

	public Timer(float duration, System.Action<float> onTick = null) : this(duration, false, onTick) {
	}

	float getTime(){
		return UsingRealtime ? Time.realtimeSinceStartup : Time.time;
	}

	/// <summary>
	/// Does this timer keep waiting? If not, release it back into the Timer pool
	/// </summary>
	public override bool keepWaiting {
		get {
			bool stop = DoTick ();
			if (stop && pooled) {
				Release (this);
			}
			return !stop;
		}
	}

	/// <summary>
	/// Ticks the timer and executes onTick function if present
	/// </summary>
	/// <returns><c>true</c>, if the timer is finished, <c>false</c> otherwise.</returns>
	public bool DoTick(){
		float t = Mathf.Clamp01((getTime () - StartTime) / Duration);
		if (tick != null) {
			tick (t);
		}
		return t >= 1f;
	}
}
