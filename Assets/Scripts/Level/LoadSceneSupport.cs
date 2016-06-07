﻿using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
// for SceneManager

public class LoadSceneSupport : MonoBehaviour {

	public string LevelName = null;

    public Button mStart;
    public Button mExit;
    public Button mCredit;
	public Button mTutorial;
    public Canvas creditCanvas;

	// Use this for initialization
	void Start () {
        creditCanvas.enabled = false;
        // Workflow assume:
        //      m-Buttons: are dragged/placed from UI
        // add in listener
        mStart.onClick.AddListener(
                () => {                     // Lamda operator: define an annoymous function
					Time.timeScale = 1;
                	LoadScene("Jump");
                });
        mExit.onClick.AddListener(
                () => {                     // Lamda operator: define an annoymous function
                    Application.Quit();
                });
        mCredit.onClick.AddListener(
                () => {                     // Lamda operator: define an annoymous function
                    creditCanvas.enabled = true;
                });
		mTutorial.onClick.AddListener (
				() => {
					Time.timeScale = 1;
					LoadScene ("TutorialScene");
				});

    }

	public void gotoMainMenu(){
		LoadScene ("Menu");
	}

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if(creditCanvas.enabled == true)
            {
                creditCanvas.enabled = false;
            }
        }
    }
    
	void LoadScene(string theLevel) {
        SceneManager.LoadScene(theLevel);
	}
}