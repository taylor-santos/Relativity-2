using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Relativity_RigidAcceleration : MonoBehaviour {
	public Vector3 Front;
	public float T;
	public Vector3 A;
	public Vector3 P0;

	public float L;
	public float a;

	private Relativity_Rigidbody rb;
	// Use this for initialization
	void Start () {
		rb = GetComponent<Relativity_Rigidbody>();
		P0 = transform.position;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (rb != null)
		{
			Vector3 pos = Front - P0;
			L = Vector3.Dot(pos, A) / A.magnitude;
			if (L <= 1f/A.magnitude)
			{
				rb.Proper_Accelerations = new List<Vector4>();
				a = 1/(1-A.magnitude * L);
				Vector4 accel = new Vector4(T*(1-A.magnitude*L), A.x*a, A.y*a, A.z*a);
				rb.Proper_Accelerations.Add(accel);
			}else{
				rb.Proper_Accelerations = new List<Vector4>();
			}
		}
	}
}
