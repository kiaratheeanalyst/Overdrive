using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryLineScriptingChapter1 : MonoBehaviour
{

	public float delay;
	public AudioSource audiosource;
        

	IEnumerator playWithDelay()
	{
		yield return new WaitForSeconds (delay);
		audiosource.Play();	
	}
	
	void Start()
	{
		StartCoroutine("playWithDelay");	
	}

}
