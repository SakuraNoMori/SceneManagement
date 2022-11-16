using UnityEngine;
using UnityEngine.EventSystems;

public class Init : MonoBehaviour
{
	private float _delay = 1.0f;

	[SerializeField] private Camera _camPrefab;
	[SerializeField] private Canvas _prefabFaderCanvas;

	private Camera _cam;
	private Canvas _faderCanvas;

	private bool _waiting = true;

	private void Awake()
	{
		// Instantiating all necessary objects
		GameObject go = new GameObject("Managers");

		_cam = Instantiate(_camPrefab);
		_cam.name ="CameraMain";

		_faderCanvas = Instantiate(_prefabFaderCanvas, null);
		_faderCanvas.name = "FaderCanvas";

		// Creating components
		go.AddComponent<SceneController>();
		go.AddComponent<EventSystem>();
		go.AddComponent<StandaloneInputModule>();
	}

	// Start is called before the first frame update
	void Start()
	{
		SceneController.Instance.registerCam(_cam);
		SceneController.Instance.registerFader(_faderCanvas);
	}

	// Update is called once per frame
	void Update()
	{
		_delay -= Time.deltaTime;
		if(_delay <= 0f&&_waiting)
		{
			SceneController.Instance.LoadScene(eScenes.MainMenu);
			_waiting = false;
		}
	}
}
