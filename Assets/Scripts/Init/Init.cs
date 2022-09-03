using UnityEngine;

public class Init : MonoBehaviour
{
	private float _delay = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		_delay-=Time.deltaTime;
		if(_delay<=0f)
		{
			SceneController.Instance.LoadScene(eScenes.MainMenu);
		}
    }
}
