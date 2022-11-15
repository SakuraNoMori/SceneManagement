using UnityEngine;

public class Init : MonoBehaviour
{
	private float _delay = 1.0f;

	[SerializeField] private Camera _camPrefab;
	[SerializeField] private Canvas _prefabFaderCanvas;

	private Camera _cam;
	private Canvas _faderCanvas;

	private GameObject go;

	private void Awake()
	{
		// Instantiating all necessary objects
		go = new GameObject("Managers");

		_cam = Instantiate(_camPrefab);

		_faderCanvas = Instantiate(_prefabFaderCanvas, null);
		_faderCanvas.name = "FaderCanvas";

		// Creating components
		go.AddComponent<SceneController>();
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
		if(_delay <= 0f)
		{
			SceneController.Instance.LoadScene(eScenes.MainMenu);
		}
	}
}
