﻿using UnityEngine;
using System.Collections;

public class PufferBehavior : MonoBehaviour {
	private float distFromMeemoToActivateTrigger = 7.0f;
	private float timer = 0.0f;
	private const float MAX_TIME = 5.0f;
	public Animator anim;
	public float bounce_force = 5f;

	#region sound support
	private bool audioIsEnabled = false;
	private float soundTimer = 0.0f;
	#endregion

	#region state support
	public enum PufferState {
		Little,
		Puffed
	}
	public PufferState currentState;
	#endregion

	// Use this for initialization
	void Start () {
		currentState = PufferState.Little;
		anim = GetComponentInParent<Animator> ();
	}

	// Update is called once per frame
	void Update () {
		Canvas gameOverCanvas = GameObject.Find ("GameOverCanvas").GetComponent<Canvas> ();
		if (!gameOverCanvas.enabled) {
			Hero_Interaction meemo = GameObject.FindGameObjectWithTag ("Player").GetComponent<Hero_Interaction> ();
			switch (currentState) {
			case PufferState.Little:
				if (Vector3.Distance (meemo.transform.position, transform.position) < distFromMeemoToActivateTrigger) {
					//Debug.Log (currentState);
					anim.SetBool ("bool",true);
					if (!audioIsEnabled)
						audioIsEnabled = true;
					else {
						soundTimer += Time.deltaTime;
						if (soundTimer > 0.03f) {
							GetComponent<AudioSource> ().Play ();
							currentState = PufferState.Puffed;
						}
					}
				}
				break;
			case PufferState.Puffed:
				if (Vector3.Distance (meemo.transform.position, transform.position) < distFromMeemoToActivateTrigger) {
					timer = 0.0f;
				} else {
					if (timer >= MAX_TIME) {
						currentState = PufferState.Little;
						//Debug.Log (currentState);
						anim.SetBool ("bool",false);
					} else {
						timer += Time.deltaTime;
					}
				}
				break;
			}
		}


	}
}
