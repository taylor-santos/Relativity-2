using UnityEngine;
using System.Collections;

public class Relativity_Rigidbody2 : MonoBehaviour {
	public GameObject Observer;
	public Vector3 velocity;
	public float γ;
	public double ProperTime;
	public double CoordinateTime;
	public Vector3 relativeVelocity;

	private Matrix4x4 boost;

	private Relativity_Observer obs;
	private GameObject scaler;
	
	public Vector4 currentPosition;
	public Vector4 realPosition;
	public Vector4 startingPosition;
	public Vector4 BoostedStartingPosition;
	
	void Start () {
		if (velocity.magnitude >= 1)
			velocity = velocity.normalized * (0.9999999f);
		obs = Observer.GetComponent<Relativity_Observer>();
		CoordinateTime = obs.CoordinateTime;
		Vector3 u = velocity;
		Vector3 v = obs.velocity;
		if (v.magnitude >= 1)
			v = v.normalized * (0.9999999f);
		if (u!=v)
		{
			relativeVelocity = Mathf.Sqrt(1- ((1-Mathf.Pow(u.magnitude,2))*(1-Mathf.Pow(v.magnitude,2)))/Mathf.Pow(1-Vector3.Dot(u,v),2)) * (u-v).normalized;
		}else{
			relativeVelocity = Vector3.zero;
		}
		if (relativeVelocity.magnitude >= 1)
			relativeVelocity = relativeVelocity.normalized * (0.9999999f);
		γ = 1f/Mathf.Sqrt(1-relativeVelocity.sqrMagnitude);

		startingPosition = new Vector4((float)CoordinateTime,(transform.position - Observer.transform.position).x,(transform.position - Observer.transform.position).y,(transform.position - Observer.transform.position).z);

		scaler = new GameObject();
		scaler.name = name;

		scaler.transform.position = Observer.transform.position;
		scaler.transform.LookAt(Observer.transform.position+relativeVelocity);

		transform.parent = scaler.transform;

		scaler.transform.localScale = new Vector3(1,1,1f/γ);
		boost = LorentzBoost(relativeVelocity.x,relativeVelocity.y,relativeVelocity.z);
		//if (u != v)
		//	startingPosition = (Vector4)boost.MultiplyVector(startingPosition);
	}	
	
	void Update () {
		if (velocity.magnitude >= 1)
			velocity = velocity.normalized * (0.9999999f);
		Vector3 u = velocity;
		Vector3 v = obs.velocity;
		if (v.magnitude >= 1)
			v = v.normalized * (0.9999999f);
		if (u!=v)
		{
			relativeVelocity = Mathf.Sqrt(1- ((1-Mathf.Pow(u.magnitude,2))*(1-Mathf.Pow(v.magnitude,2)))/Mathf.Pow(1-Vector3.Dot(u,v),2)) * (u-v).normalized;
		}else{
			relativeVelocity = Vector3.zero;
		}
		if (relativeVelocity.magnitude >= 1)
			relativeVelocity = relativeVelocity.normalized * (0.9999999f);
		γ = 1f/Mathf.Sqrt(1-relativeVelocity.sqrMagnitude);
		boost = LorentzBoost(relativeVelocity.x,relativeVelocity.y,relativeVelocity.z);
		CoordinateTime = obs.CoordinateTime;

		scaler.transform.localScale = Vector3.one;
		transform.parent = null;

		//currentPosition = startingPosition + (float)CoordinateTime * γ * relativeVelocity;
		currentPosition = startingPosition + new Vector4((float)CoordinateTime, (float)CoordinateTime * γ * relativeVelocity.x,(float)CoordinateTime * γ * relativeVelocity.y,(float)CoordinateTime * γ * relativeVelocity.z);
		//currentPosition = (Vector4)startingPosition + new Vector4(1f,velocity.x,velocity.y,velocity.z) * γ * (float)ProperTime;

		transform.position = Observer.transform.position + new Vector3(currentPosition.y,currentPosition.z,currentPosition.w)/* - (float)ProperTime / γ  * relativeVelocity*/;
		
		if (u!=v)
		{
			realPosition = (Vector4)boost.inverse.MultiplyVector(new Vector4((float)ProperTime,currentPosition.x,currentPosition.y,currentPosition.z));
		}
		else{
			realPosition = new Vector4((float)CoordinateTime,transform.position.x - Observer.transform.position.x,transform.position.y - Observer.transform.position.y,transform.position.z - Observer.transform.position.z);
		}
		scaler.transform.position = Observer.transform.position;
		scaler.transform.LookAt(Observer.transform.position + relativeVelocity);
		transform.parent = scaler.transform;
		scaler.transform.localScale = new Vector3(1,1,1f/γ);

		//Debug.Log(name + " -> " + new Vector3(realPosition.y,realPosition.z,realPosition.w) + " = " + new Vector3(transform.position.x - Observer.transform.position.x,transform.position.y - Observer.transform.position.y,transform.position.z - Observer.transform.position.z));
		ProperTime = boost.MultiplyVector(new Vector4((float)CoordinateTime,transform.position.x - Observer.transform.position.x,transform.position.y - Observer.transform.position.y,transform.position.z - Observer.transform.position.z)).x;
		//transform.localPosition = new Vector3(realPosition.y,realPosition.z,realPosition.w);
	
	}

	Matrix4x4 LorentzBoost(float βx, float βy, float  βz){

		float βSqr = Mathf.Pow(βx,2) + Mathf.Pow(βy,2) + Mathf.Pow(βz,2);
		float γ = 1f/Mathf.Sqrt(1f-βSqr);

		Matrix4x4 boost = new Matrix4x4();
		boost.SetRow(0,new Vector4(γ,		-βx*γ,						-βy*γ,						-βz*γ));
		boost.SetRow(1,new Vector4(-βx*γ,	(γ-1)*(βx*βx)/(βSqr) + 1,	(γ-1)*(βx*βy)/(βSqr),		(γ-1)*(βx*βz)/(βSqr)));
		boost.SetRow(2,new Vector4(-βy*γ,	(γ-1)*(βy*βx)/(βSqr),		(γ-1)*(βy*βy)/(βSqr) + 1,	(γ-1)*(βy*βz)/(βSqr)));
		boost.SetRow(3,new Vector4(-βz*γ,	(γ-1)*(βz*βx)/(βSqr),		(γ-1)*(βz*βy)/(βSqr),		(γ-1)*(βz*βz)/(βSqr) + 1));

		return boost;
	}
}
