using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectMenu : MonoBehaviour
{
    //Probably Only two buttons - may add more.


	public void ClearProgressSure ()
	{
        	PlayerPrefs.SetInt("levelReached", 1);
	}

	public void Back ()
	{
        	SceneManager.LoadScene(0);
	}
}
