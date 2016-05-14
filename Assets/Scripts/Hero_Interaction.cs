﻿using UnityEngine;
using System.Collections;

public class Hero_Interaction : MonoBehaviour {
	private Canvas gameOverCanvas;
	CameraBehavior globalBehavior;

    public float max_speed = 2f;
    public float air_speed = 0.1f;
    private Rigidbody2D rigid_body;
	public Vector3 mSize;

	#region healthbar support
	private const int MAX_HEALTH = 3;
	public int health = MAX_HEALTH;
	HealthBar_interaction health_bar;
	#endregion

	#region jump support
    bool grounded = false;
    public Transform ground_check;
    float ground_radius = 0.3f;
    public LayerMask what_is_ground;
	#endregion

	#region bubble support
	private float meemoSpeed = 5f;
	public BubbleBehaviour bubble;
	public bool isInBubble;
	private bool isFacingRight;
	#endregion

	#region starpower support
	private const float MAX_STAR_TIMER = 1f;
	private float star_timer = MAX_STAR_TIMER; // get 1 second of power up
	private StarBar_interaction star_bar = null;
	private bool is_using_power = false;
	#endregion

	#region meemostate support
	public enum MeemoState
	{
		Normal,
		Bubble,
		Hurt
	}
	public MeemoState current_state;
	#endregion

	// Use this for initialization
	void Start () {
		this.globalBehavior = GameObject.Find("Main Camera").GetComponent<CameraBehavior>();
		mSize = GetComponent<Renderer> ().bounds.size;
		this.health_bar = GameObject.Find ("HealthBar").GetComponent<HealthBar_interaction> ();
        this.rigid_body = this.GetComponent<Rigidbody2D>();
		isInBubble = false;
		isFacingRight = true;
		this.star_bar = GameObject.Find ("StarBar").GetComponent<StarBar_interaction> ();
		gameOverCanvas = GameObject.Find ("GameOverCanvas").GetComponent<Canvas> ();
		gameOverCanvas.enabled = false;		// The GameOverCanvas has to be initially enabled on the Unity UI
		current_state = MeemoState.Normal;
    }

    void FixedUpdate () {
		this.change_direction ();
		/// Interaction with bubble
		if (is_using_power) {
			fly ();
		}
		this.ClampToCamera ();
		this.CheckDeath ();
		/// End interaction with bubble
    }

	void Update() {
		if (Input.GetKey ("space") && !this.grounded && this.star_timer > 0f) {
			is_using_power = true;
		}
		else {
			is_using_power = false;
		}
		switch (this.current_state) {
			case MeemoState.Bubble:
				if (Input.GetAxis ("Horizontal") != 0f) { // When meemo is controlling the horizontal direction
					this.move_in_bubble ();
				} else { // When meemo is following bubble
					this.follow_in_bubble ();
				}
				break;
			case MeemoState.Normal:
				this.grounded = Physics2D.OverlapCircle (this.ground_check.position, this.ground_radius, this.what_is_ground);
				this.rigid_body.velocity = new Vector2 (Input.GetAxis ("Horizontal") * max_speed, this.rigid_body.velocity.y);
				break;
		}
	}
		
	// Currently unused
//    void Jump ()
//    {
//        this.GetComponent<Rigidbody2D>().AddForce(new Vector2(0, 10f), ForceMode2D.Impulse);
//    }
	#region starpower support
	void fly () {
		this.star_timer -= Time.fixedDeltaTime;
		this.rigid_body.AddForce (new Vector2 (5f, 20f), ForceMode2D.Force);
		star_bar.UpdateStarBarSize (this.star_timer);
	}

	public void ResetStarPower() {
		this.star_timer = 1f;
		star_bar.UpdateStarBarSize (this.star_timer);
	}
	#endregion

	#region direction support
	private void change_direction() {
		if (Input.GetAxis ("Horizontal") < 0f && isFacingRight) {
			transform.localScale = new Vector3 (-.3f, .3f, 1f);
			isFacingRight = false;
		}

		if (Input.GetAxis ("Horizontal") > 0f && !isFacingRight) {
			transform.localScale = new Vector3 (.3f, .3f, 1f);
			isFacingRight = true;
		}
	}
	#endregion

	#region bubble support
	// Update position of bubble following sine curve
	private void FollowSineCurve(){
		float newY = bubble.transform.position.y + BubbleBehaviour.bubbleSpeed * Time.deltaTime;
		float newX = bubble.initpos.x + GetXValue (newY); 
		bubble.transform.position = new Vector3 (newX, newY, 0f);
	}
		
	// Calculate the x value for bubble movement
	private float GetXValue(float y){
		CameraBehavior globalBehaviour = GameObject.Find ("Main Camera").GetComponent<CameraBehavior> ();
		float sinFreqScale = bubble.sinOsc * 2f * (Mathf.PI) / globalBehaviour.WorldMax.y;
		return bubble.sinAmp * (Mathf.Sin(y * sinFreqScale));
	}
	#endregion

	#region Bubble support
	private void move_in_bubble() {
		float bnewY = bubble.transform.position.y + 0.03f;// bubble floats
		float bnewX = transform.position.x + Input.GetAxis ("Horizontal") * (air_speed);
		bubble.transform.position = new Vector3 (bnewX, bnewY, 0f);
		bubble.initpos.x = bnewX;

		transform.position = new Vector3 (bnewX, bnewY - 0.2f, transform.position.z);
	}

	private void follow_in_bubble() {
		FollowSineCurve();
		// update meemo's position to bubble'e sine curve
		transform.position = new Vector3 (bubble.transform.position.x - 0.05f,
			bubble.transform.position.y - GetComponent<Renderer> ().bounds.size.y / 2f + 0.5f, bubble.transform.position.z);
	}
	#endregion

	#region camera support
	private void ClampToCamera() {
		// Handle when hero collided with the bottom bound of the window (die)
		CameraBehavior.WorldBoundStatus status = globalBehavior.ObjectCollideWorldBound (GetComponent<Renderer> ().bounds);
		Vector3 pos = globalBehavior.mCamera.WorldToViewportPoint (transform.position);
		Vector3 backgroundSize = GameObject.Find ("backgroundImage").GetComponent<Renderer> ().bounds.size;
		pos.x = Mathf.Clamp (pos.x, 0.03f, 1f - (mSize.x / backgroundSize.x)); //(1f / backgroundSize.x * mSize.x / 2f));
		pos.y = Mathf.Clamp (pos.y, 0.035f, 1f - (mSize.y / backgroundSize.y));
		transform.position = globalBehavior.mCamera.ViewportToWorldPoint (pos);
		if (transform.position.y - mSize.y/2f <= globalBehavior.globalyMin)
		{
			// Destroy Meemo
			// TimeScale = 0;
			// Panel is active
			Destroy (GameObject.Find("Meemo"));
			Time.timeScale = 0;
			gameOverCanvas.enabled = true;
		}
	}

	private void CheckDeath() {
		if (transform.position.y - mSize.y/2f <= globalBehavior.globalyMin)
		{
			// Destroy Meemo
			// TimeScale = 0;
			// Panel is active
			Die();
		}
	}

	private void Die() {
		Destroy (GameObject.Find("Meemo"));
		Time.timeScale = 0;
		gameOverCanvas.enabled = true;
	}
	#endregion
}
