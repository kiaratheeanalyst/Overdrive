using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class PauseUI : MonoBehaviour
{

    [SerializeField] private Canvas PauseUIMenu;

    void Start()
    {
        PauseUIMenu.enabled = false;
	    Cursor.visible = false;
    }

    void Update ()
    {
	PauseMenuUIEnableMethod();
        Cursor.lockState = CursorLockMode.None;
    }

    void PauseMenuUIEnableMethod()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            PauseUIMenu.enabled = true;
            Cursor.lockState = CursorLockMode.None;
	    Cursor.visible = true;
        }
    }

    public void Continue()
    {
        if (PauseUIMenu.enabled = false)
        {
            return;
        }

        PauseUIMenu.enabled = false;
        Cursor.lockState = CursorLockMode.None;
	Cursor.visible = false;
    }  
    

        public void Retry()
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 0);
        }

        public void BackToLevelSelect()
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1);
        }
    }
