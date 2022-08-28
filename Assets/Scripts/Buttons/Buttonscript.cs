using UnityEngine;
using UnityEngine.UI;

public class Buttonscript : MonoBehaviour
{
	public eScenes sceneToLoad;
	private Button b;
    // Start is called before the first frame update
    void Start()
    {
		b = this.GetComponent<Button>();
		b.onClick.AddListener(onClick);
    }

	void onClick()
	{
		SceneController.Instance.LoadScene(sceneToLoad);
	}
}
