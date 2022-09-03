using System.Collections;

using UnityEngine;

public class Fader : MonoBehaviour
{
	[SerializeField] private CanvasRenderer _black;
	private bool _fadeToBlack;
	private float _curr=0f;
	private float _progress=0f;
	private float _duration=0f;
	Coroutine c;
	private bool _running=false;
	public bool Running{ get => _running; }

	private void Awake()
	{
		_black.SetAlpha(0f);
	}

	public void fade(float duration, bool fadeToBlack = true)
	{
		if(_fadeToBlack != fadeToBlack)
		{
			if(_running)
			{
				StopCoroutine(c);
			}
			_fadeToBlack = fadeToBlack;
			_duration = duration;
			_curr = _progress * _duration;
			_running = true;
			c = StartCoroutine(fading());
		}
	}

	private IEnumerator fading()
	{
		while(_fadeToBlack ? _progress < 1f : _progress > 0f)
		{
			_curr += _fadeToBlack ? Time.deltaTime : -Time.deltaTime;
			_progress = _curr / _duration;
			_black.SetAlpha(_progress);
			yield return null;
		}
		_running = false;
	}
}


