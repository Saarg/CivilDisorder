using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Steamworks;

public class SceneOptions : MonoBehaviour {

	[SerializeField] GameManager gameManager;
	[SerializeField] Button createButton;

	int curTrack = 1;
	public void OnChangeWold(int scene) {
		StartCoroutine(LoadAsyncScene(scene + 1));

        SteamUserStats.SetAchievement("START_THE_GAME");                
        SteamUserStats.SetStat("START_THE_GAME", 1);
        SteamUserStats.StoreStats();
	}

	IEnumerator LoadAsyncScene(int scene)
    {
		createButton.gameObject.SetActive(false);
		AsyncOperation asyncLoad = SceneManager.UnloadSceneAsync(curTrack);
		while (!asyncLoad.isDone)
        {
            yield return null;
        }
		curTrack = scene;
        gameManager.track = scene;
        gameManager.curTrack = scene;
        asyncLoad = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
		createButton.gameObject.SetActive(true);		
    }

    void Start()
    {
        SceneManager.LoadScene(1, LoadSceneMode.Additive);
    }
}
