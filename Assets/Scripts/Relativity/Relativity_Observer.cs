using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Relativity_Observer : MonoBehaviour {
	public bool FreezeTime = true;
	public Vector3 velocity;
	public Vector3 acceleration;
	public float CoordinateTime = 0;
	public List<Vector4> properAccelerations;
	public float TimeScale = 1;
	public KeyCode PauseKey = KeyCode.Space;
	public KeyCode SlowTime = KeyCode.LeftArrow;
	public KeyCode SpeedTime = KeyCode.RightArrow;
	private Vector3 initialProperVelocity;

	public List<GameObject> charges;
	// Use this for initialization
	void Start () {
		GameObject[] objects = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
		charges = new List<GameObject>();
		foreach(GameObject obj in objects)
		{
			if (obj.GetComponent<Relativity_ChargedObject>() != null)
			{
				charges.Add(obj);
			}
		}
		initialProperVelocity = velocity / Mathf.Sqrt(1f - velocity.sqrMagnitude);
	}
	
	// Update is called once per frame
	void Update () {
		if (velocity.magnitude >= 0.999999f)
			velocity = velocity.normalized * (0.999999f);
		if (Input.GetKeyDown(PauseKey)){
			FreezeTime = !FreezeTime;
		}
		if (acceleration != Vector3.zero){
			Vector3 properVelocity = initialProperVelocity + (float)CoordinateTime * acceleration;
			velocity = properVelocity / Mathf.Sqrt(1f + properVelocity.sqrMagnitude);
		}
		
		/*
		if (Input.GetKeyDown(SlowTime))
		{
			TimeScale -= 0.2f;
		}
		if (Input.GetKeyDown(SpeedTime))
		{
			TimeScale += 0.2f;
		}
		*/
		if (!FreezeTime){
			CoordinateTime += Time.deltaTime * TimeScale;
		}
		
	}

	void FixedUpdate (){
		Shader.SetGlobalVector("_Observer_Velocity", velocity);
		Shader.SetGlobalFloat("_Observer_Time", CoordinateTime);
	}

	void OnGUI() {
        GUI.Label(new Rect(10, 10, 100, 20), "Time Scale: " + (TimeScale).ToString("#.0"));
        TimeScale = GUI.HorizontalSlider(new Rect(25, 25, 100, 30), TimeScale, -10, 10);
    }

    void OnApplicationQuit() {
		Shader.SetGlobalFloat("_Coordinate_Time", 0);
    }
}
