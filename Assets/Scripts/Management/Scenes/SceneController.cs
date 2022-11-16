using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// enums for each scene, values need to be the same as in Unitys buildsettings
/// </summary>
public enum eScenes { Init, MainMenu, GameUI, Scene1, Scene2, Scene3 };

public class SceneController : MonoBehaviour
{
	// Singleton
	private static SceneController _instance;
	public static SceneController Instance { get => _instance; }

	private string _nameManagement = "Management";
	private Scene _management;
	private List<eScenes> _activeScenes = new();
	private bool _isChanging = false;
	private bool _isLoading = false;

	private Canvas _faderCanvas;
	private Fader _faderScript;

	private GameObject _cam;

	private readonly List<eScenes> _immediateLoading = new List<eScenes>();
	private readonly List<eScenes> _needsUI = new List<eScenes>();


	void Awake()
	{
		if(_instance != null && _instance != this)
		{
			Debug.LogWarning("Second SceneController was created!");
			Destroy(this.gameObject);
			return;
		}
		if(_instance == null)
		{
			_instance = this;
			_activeScenes.Add(eScenes.Init);

			// set physicsmode for management to None, since it doesn't need any physics
			CreateSceneParameters csp = new();
			csp.localPhysicsMode = LocalPhysicsMode.None;
			// Create and save reference to Managementscene
			_management = SceneManager.CreateScene(_nameManagement, csp);
			SceneManager.MoveGameObjectToScene(this.gameObject, _management);
		}
	}

	private void Start()
	{
		_immediateLoading.Add(eScenes.MainMenu);
		_needsUI.Add(eScenes.Scene1);
		_needsUI.Add(eScenes.Scene2);
		_needsUI.Add(eScenes.Scene3);
	}

	/// <summary>
	/// Loads one or more scenes depending on sceneToLoad
	/// </summary>
	/// <param name="sceneToLoad">Scene to load</param>
	/// <param name="fadeDuration">Duration of fade-effect. Only used when loading a scene that is not MainMenu</param>
	public void LoadScene(eScenes sceneToLoad, float fadeDuration = 0f)
	{
		// Check for unintended loading of init-scene
		if(sceneToLoad == eScenes.Init)
		{
			Debug.LogWarning("Tried to load Init-Scene.");
			return;
		}
		if(_isChanging)
		{
			return;
		}
		_isChanging = true;

		// Check if scene is already loaded
		if(!_activeScenes.Contains(sceneToLoad))
		{
			// Handle loading of MainMenu
			if(sceneToLoad == eScenes.MainMenu)
			{
				StartCoroutine(_loadLevel(eScenes.MainMenu, 0f));
			}
			// Handle loading of other scenes
			else
			{
				StartCoroutine(_loadLevel(sceneToLoad, fadeDuration));
			}
		}
		else
		{
			Debug.Log("Scene is already loaded");
		}
	}

	/// <summary>
	/// Function to load a specific scene with fade-in/-out
	/// </summary>
	/// <param name="sceneToLoad">Scene to load</param>
	/// <param name="fadeDuration">Duration of fading</param>
	private IEnumerator _loadLevel(eScenes sceneToLoad, float fadeDuration = 0.5f)
	{
		_faderScript.activeFaderCanvas(true);
		_faderScript.fade(fadeDuration);

		do
		{
			yield return null;
		} while(_faderScript.Running);

		SceneManager.SetActiveScene(_management);
		moveToManagement();

		// unload unneeded scenes
		for(int i = _activeScenes.Count - 1; i >= 0; i--)
		{
			if(_activeScenes[i] != sceneToLoad)
			{
				if(_activeScenes[i] == eScenes.GameUI && _needsUI.Contains(sceneToLoad))
				{
					continue;
				}
				SceneManager.UnloadSceneAsync((int)_activeScenes[i]);
				_activeScenes.Remove(_activeScenes[i]);
			}
		}

		// if not already loaded, load UI-scene
		if(!_activeScenes.Contains(eScenes.GameUI) && _needsUI.Contains(sceneToLoad))
		{
			StartCoroutine(AddScene(eScenes.GameUI, true));
			_activeScenes.Add(eScenes.GameUI);
		}

		// Load wanted scene
		StartCoroutine(AddScene(sceneToLoad, _immediateLoading.Contains(sceneToLoad)));
		_activeScenes.Add(sceneToLoad);

		do
		{
			yield return null;
		} while(_isLoading);

		_faderScript.fade(fadeDuration, false);
		moveFromManagement(sceneToLoad);

		do
		{
			yield return null;
		} while(_faderScript.Running);
		_faderScript.activeFaderCanvas(false);

		yield return new WaitForSecondsRealtime(0.02f);
		SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex((int)sceneToLoad));
		_isChanging = false;
	}

	/// <summary>
	/// Function to add a new scene
	/// </summary>
	/// <param name="sceneID">The enum of the scene to add</param>
	/// <param name="activateImmediate">Whether loading of the scene should be ended automatically or via user-input<br/>If true, scene loads automatically, else needs user-input</param>
	/// <returns>No sensible return-value</returns>
	private IEnumerator AddScene(eScenes sceneID, bool activateImmediate = false)
	{
		AsyncOperation loader = SceneManager.LoadSceneAsync((int)sceneID, LoadSceneMode.Additive);
		loader.allowSceneActivation = activateImmediate;
		_isLoading = !activateImmediate;
		while(!loader.isDone)
		{
			yield return null;

			if(loader.progress >= 0.9f && activateImmediate == false)
			{
				Debug.Log("Scene ready to load, press backspace");
				if(Input.GetKeyDown(KeyCode.Backspace))
				{
					loader.allowSceneActivation = true;
					_isLoading = false;
				}
			}
		}
	}

	private void moveToManagement()
	{
		SceneManager.MoveGameObjectToScene(_cam, _management);
	}

	private void moveFromManagement(eScenes scene)
	{
		SceneManager.MoveGameObjectToScene(_cam, SceneManager.GetSceneByBuildIndex((int)scene));
	}

	#region Functions for registering objects
	public void registerCam(Camera newCam)
	{
		_cam = newCam.gameObject;
		SceneManager.MoveGameObjectToScene(_cam, _management);
	}

	public void registerFader(Canvas fader)
	{
		_faderCanvas = fader;
		// Create and move Loadingscreen
		SceneManager.MoveGameObjectToScene(_faderCanvas.gameObject, _management);
		_faderScript = _faderCanvas.GetComponent<Fader>();

	}

	#endregion
	/*
	 //This script lets you load a Scene asynchronously. It uses an asyncOperation to calculate the progress and outputs the current progress to Text (could also be used to make progress bars).

//Attach this script to a GameObject
//Create a Button (Create>UI>Button) and a Text GameObject (Create>UI>Text) and attach them both to the Inspector of your GameObject
//In Play Mode, press your Button to load the Scene, and the Text changes depending on progress. Press the space key to activate the Scene.
//Note: The progress may look like it goes straight to 100% if your Scene doesnft have a lot to load.

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AsyncOperationProgressExample : MonoBehaviour
{
    public Text m_Text;
    public Button m_Button;

    void Start()
    {
        //Call the LoadButton() function when the user clicks this Button
        m_Button.onClick.AddListener(LoadButton);
    }

    void LoadButton()
    {
        //Start loading the Scene asynchronously and output the progress bar
        StartCoroutine(LoadScene());
    }

    IEnumerator LoadScene()
    {
        yield return null;

        //Begin to load the Scene you specify
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("Scene3");
        //Don't let the Scene activate until you allow it to
        asyncOperation.allowSceneActivation = false;
        Debug.Log("Pro :" + asyncOperation.progress);
        //When the load is still in progress, output the Text and progress bar
        while (!asyncOperation.isDone)
        {
            //Output the current progress
            m_Text.text = "Loading progress: " + (asyncOperation.progress * 100) + "%";

            // Check if the load has finished
            if (asyncOperation.progress >= 0.9f)
            {
                //Change the Text to show the Scene is ready
                m_Text.text = "Press the space bar to continue";
                //Wait to you press the space key to activate the Scene
                if (Input.GetKeyDown(KeyCode.Space))
                    //Activate the Scene
                    asyncOperation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
	 */
}
