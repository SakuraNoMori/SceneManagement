using System.Collections;

using UnityEngine;

public class Fader : MonoBehaviour
{
	[SerializeField] private CanvasRenderer _black;
	private bool _fadeToBlack = false;
	private float _curr = 0f;
	private float _progress = 0f;
	private float _duration = 0f;
	private Coroutine c;
	private bool _running = false;
	public bool Running { get => _running; }

	private void Awake()
	{
		_black.SetAlpha(0f);
		activeFaderCanvas(false);
	}

	public void fade(float duration, bool fadeToBlack = true)
	{
		if(_fadeToBlack != fadeToBlack)
		{
			if(duration <= float.Epsilon)
			{
				_black.SetAlpha(fadeToBlack ? 1f : 0f);
			}
			if(_running)
			{
				StopCoroutine(c);
				c = null;
			}
			_fadeToBlack = fadeToBlack;
			_duration = duration;
			_curr = _progress * _duration;
			_running = true;
			//Debug.Log("Fade " + (_fadeToBlack ? "to" : "from") + " black with duration " + _duration);
			c = StartCoroutine(fading());
		}
	}

	/// <summary>
	/// Function to create a fade effect
	/// </summary>
	private IEnumerator fading()
	{
		//Debug.Log("Started fading() with curr: "+_curr+", duration: "+_duration+", progress ="+_progress);
		while(_fadeToBlack ? _progress < 1f : _progress > 0f)
		{
			_curr += _fadeToBlack ? Time.deltaTime : -Time.deltaTime;
			_progress = _curr / _duration;
			//Debug.Log("Curr: "+_curr+"\tProg: "+_progress);
			_black.SetAlpha(_progress);
			yield return null;
		}
		_progress = _fadeToBlack ? 1f : 0f;
		_running = false;
	}

	public void activeFaderCanvas(bool active)
	{
		_black.gameObject.SetActive(active);
	}
}


