using UnityEngine;
using System.Collections;

public class Relativity_PropagateLight : MonoBehaviour {
	public GameObject Observer;
	public double coordinateTimeStart;
	public float segmentLength = 0.1f;

	private Relativity_Observer obs;
	private LineRenderer lr;
	private Relativity_Rigidbody rb;
	// Use this for initialization
	void Start () {
		obs = Observer.GetComponent<Relativity_Observer>();
		//coordinateTimeStart = obs.CoordinateTime;
		lr = GetComponent<LineRenderer>();
		rb = GetComponent<Relativity_Rigidbody>();
		rb.Observer = Observer;
	}
	
	// Update is called once per frame
	void Update () {
		rb.Velocity = obs.velocity;
		float radius = Mathf.Abs((float)(obs.CoordinateTime-coordinateTimeStart));
		transform.localScale = Vector3.one * radius * 2;
		float theta = 0;
		
		int segmentCount = Mathf.Min((int)(2*Mathf.PI*radius/segmentLength),100);
		float thetaIncrement = 2*Mathf.PI/segmentCount;
		lr.positionCount = segmentCount+1;
		Vector3 prevPos = transform.position + Vector3.right * radius;
		lr.SetPosition(0,prevPos);
		for (int i=0; i<segmentCount; ++i)
		{
			theta += thetaIncrement;
			float x = radius * Mathf.Cos(theta);
            float y = radius * Mathf.Sin(theta);  
			Vector3 nextPos = transform.position + new Vector3(x,y,0);
			lr.SetPosition(i+1,nextPos);
			//Debug.DrawLine(prevPos,nextPos,Color.white,Time.deltaTime);
			prevPos = nextPos;
		}
		//transform.localScale = new Vector3(2,2,0) * Mathf.Max((float)(obs.CoordinateTime-coordinateTimeStart),0) + new Vector3(0,0,0.0001f);
		//lightComponent.range =  Mathf.Max((float)(obs.CoordinateTime-coordinateTimeStart),0);
	}
}
