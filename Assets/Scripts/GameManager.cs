using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
	private static GameManager _instance;

    public bool showCursorMode;
    public CursorLockMode lockCursorMode;


	public static GameManager instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = GameObject.Find("GameManager").GetComponent<GameManager>();
			}
			return _instance;
		}
	}


	void Start ()
	{
        // Init the cursor behaviour
        Cursor.lockState = lockCursorMode;
        Cursor.visible = showCursorMode;

        Screen.fullScreen = false;
	}
	
	void Update ()
	{

	}

    public void OnApplicationQuit()
    {
        WorldManager.instance.deleteWorld();
    }
}
