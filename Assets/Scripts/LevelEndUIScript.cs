using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelEndUIScript : MonoBehaviour
{

    [SerializeField] private Canvas levelUIMenu;

    public GameManager gameManager;

    void Start()
    {
        levelUIMenu.enabled = false;
	    Cursor.visible = false;
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerLayer"))
        {
            gameManager.WinLevel();
            levelUIMenu.enabled = true;
            Cursor.lockState = CursorLockMode.None;
	    Cursor.visible = true;
        }
   }

    public void BossOneGot()
    {
        {
            gameManager.WinLevel();
            levelUIMenu.enabled = true;
            Cursor.lockState = CursorLockMode.None;
	    Cursor.visible = true;
        }
   }



        public void Next()
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
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