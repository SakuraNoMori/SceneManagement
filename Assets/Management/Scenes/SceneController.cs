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

	[SerializeField] private Canvas _prefabLoadingScreen;
	private Canvas _loadingScreen;
	private Fader _fade;

	void Awake()
	{
		if(_instance != null && _instance != this)
		{
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
			_loadingScreen = Instantiate(_prefabLoadingScreen, null);
			SceneManager.MoveGameObjectToScene(_loadingScreen.gameObject, _management);
		}

	}

	private void Start()
	{
		_fade = _loadingScreen.GetComponent<Fader>();
	}


	/// <summary>
	/// Loads one or more scenes depending on sceneToLoad
	/// </summary>
	/// <param name="sceneToLoad">Scene to load</param>
	public void LoadScene(eScenes sceneToLoad)
	{
		// Check for unintended loading of init-scene
		if(sceneToLoad == eScenes.Init)
		{
			Debug.LogWarning("Tried to load Init-Scene.");
			return;
		}

		// Check if scene is already loaded
		if(!_activeScenes.Contains(sceneToLoad))
		{
			// Handle loading of MainMenu
			if(sceneToLoad == eScenes.MainMenu)
			{
				foreach(eScenes ID in _activeScenes)
				{
					SceneManager.UnloadSceneAsync((int)ID);
				}
				_activeScenes.Clear();
				StartCoroutine(AddScene(sceneToLoad, true));
				_activeScenes.Add(sceneToLoad);
			}
			// Handle loading of other scenes
			else
			{
				// Load wanted scene
				StartCoroutine(AddScene(sceneToLoad));
				_activeScenes.Add(sceneToLoad);             
				
				// if not already loaded, load UI-scene
				if(!_activeScenes.Contains(eScenes.GameUI))
				{
					StartCoroutine(AddScene(eScenes.GameUI, true));
					_activeScenes.Add(eScenes.GameUI);
				}

				// unload unneeded scenes
				for(int i = _activeScenes.Count - 1; i >= 0; i--)
				{
					if(_activeScenes[i] != eScenes.GameUI||_activeScenes[i]!=sceneToLoad)
					{
						SceneManager.UnloadSceneAsync((int)_activeScenes[i]);
						_activeScenes.Remove(_activeScenes[i]);
					}
				}



				Debug.Log("Fade from black");
				_fade.fade(0.5f, false);

			}
		}
		else
		{
			Debug.Log("Scene is already loaded");
		}
	}


	/// <summary>
	/// Function to add a new scene
	/// </summary>
	/// <param name="sceneID">The enum of the scene to add</param>
	/// <param name="activateImmediate">Whether loading of the scene should be ended automatically or via user-input<br/>If true, scene loads automatically, else needs user-input</param>
	/// <returns>No sensible return-value</returns>
	private IEnumerator AddScene(eScenes sceneID, bool activateImmediate = false)
	{

		if(!activateImmediate)
		{
			Debug.Log("Fade to black");
			_fade.fade(0.5f);

			while(_fade.Running)
			{
				yield return null;
			}
		}

		AsyncOperation loader = SceneManager.LoadSceneAsync((int)sceneID, LoadSceneMode.Additive);
		loader.allowSceneActivation = activateImmediate;

		while(!loader.isDone)
		{
			yield return null;

			if(loader.progress >= 0.9f && activateImmediate == false)
			{
				Debug.Log("Scene ready to load, press space");
				if(Input.GetKeyDown(KeyCode.Space))
				{
					loader.allowSceneActivation = true;
				}
			}
		}

		//if(!activateImmediate)
		//{
		//	Debug.Log("Fade from black");
		//	_fade.fade(0.5f, false);

		//	while(_fade.Running)
		//	{
		//		yield return null;
		//	}
		//}
	}

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
