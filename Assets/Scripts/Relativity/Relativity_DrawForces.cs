using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Relativity_DrawForces : MonoBehaviour {
	public GameObject Observer;
	public bool DrawEForce;
	public bool DrawBForce;
	public Vector3 EForce;
	public Vector3 BForce;
	public Vector3 LorentzForce;
	public float drawScale = 100;
	
	public Relativity_Observer obs;
	public Relativity_ChargedObject thisCharge;
	public List<Relativity_ChargedObject> charges;

	private float timer;
	// Use this for initialization
	void Start () {
		obs = Observer.GetComponent<Relativity_Observer>();
		GameObject[] objects = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
		charges = new List<Relativity_ChargedObject>();
		foreach(GameObject obj in objects)
		{
			if (obj.GetComponent<Relativity_ChargedObject>() != null && obj.transform != transform)
			{
				charges.Add(obj.GetComponent<Relativity_ChargedObject>());
			}
		}
		thisCharge = GetComponent<Relativity_ChargedObject>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (thisCharge != null)
		{
			EForce = Vector3.zero;
			BForce = Vector3.zero;

			Vector3 v = Vector3.zero;
			if (GetComponent<Relativity_Rigidbody>() != null)
			{
				v = GetComponent<Relativity_Rigidbody>().Relative_Velocity;
			}

			Vector3 pos = transform.position;

			Vector3 BField = Vector3.zero;
			foreach(Relativity_ChargedObject currCharge in charges)
			{
				Vector3 pos2 = currCharge.transform.position;
				float rSqr = (pos-pos2).sqrMagnitude;
				EForce += (pos-pos2).normalized * (currCharge.charge)/(4*Mathf.PI*rSqr);

				Vector3 u = Vector3.zero;
				if (currCharge.gameObject.GetComponent<Relativity_Rigidbody>() != null)
				{
					u = currCharge.gameObject.GetComponent<Relativity_Rigidbody>().Relative_Velocity;
				}
				//Vector3 relativeVelocity = Mathf.Sqrt(1- ((1-Mathf.Pow(u.magnitude,2))*(1-Mathf.Pow(v.magnitude,2)))/Mathf.Pow(1-Vector3.Dot(u,v),2)) * (u-v).normalized;
				BField += currCharge.charge * Vector3.Cross(u,(pos-pos2))/(4*Mathf.PI*Mathf.Pow((pos-pos2).magnitude,3));
			}

			if (v.magnitude > 0)
				BForce = Vector3.Cross(v,BField);
			
			if (GetComponent<Relativity_Rigidbody>() != null && GetComponent<Relativity_Rigidbody>().enabled)
			{
				BForce /= (float)GetComponent<Relativity_Rigidbody>().γ;
				EForce /= (float)GetComponent<Relativity_Rigidbody>().γ;
			}

			LorentzForce = BForce + EForce;

			if (DrawEForce)
			{
				Debug.DrawRay(pos,EForce*drawScale,Color.blue - new Color(0,0,0,0.4f),Time.fixedDeltaTime);
					Debug.DrawRay(pos+EForce*drawScale,Quaternion.AngleAxis(150,Vector3.forward)*EForce*drawScale/5,Color.blue,Time.fixedDeltaTime);
					Debug.DrawRay(pos+EForce*drawScale,Quaternion.AngleAxis(-150,Vector3.forward)*EForce*drawScale/5,Color.blue,Time.fixedDeltaTime);
			}

		//	Debug.DrawRay(pos,LorentzForce*100,Color.green - new Color(0,0,0,0.4f),Time.fixedDeltaTime);
		//		Debug.DrawRay(pos+LorentzForce*100,Quaternion.AngleAxis(150,Vector3.Cross(LorentzForce,Vector3.up))*LorentzForce*20,Color.green,Time.fixedDeltaTime);
		//		Debug.DrawRay(pos+LorentzForce*100,Quaternion.AngleAxis(-150,Vector3.Cross(LorentzForce,Vector3.up))*LorentzForce*20,Color.green,Time.fixedDeltaTime);

			if (DrawBForce)
			{
				Debug.DrawRay(pos,BForce*drawScale,Color.red - new Color(0,0,0,0.4f),Time.fixedDeltaTime);
					Debug.DrawRay(pos+BForce*drawScale,Quaternion.AngleAxis(150,Vector3.Cross(BForce,Vector3.up))*BForce*drawScale/5,Color.red,Time.fixedDeltaTime);
					Debug.DrawRay(pos+BForce*drawScale,Quaternion.AngleAxis(-150,Vector3.Cross(BForce,Vector3.up))*BForce*drawScale/5,Color.red,Time.fixedDeltaTime);
			}
			/*
			if (timer >= 0.5f)
			{
				timer -= 0.5f;
				Debug.DrawRay(pos,force*5,Color.blue,20);
			}
			*/
			//GetComponent<Rigidbody>().AddForce(force);
		}
		timer += Time.fixedDeltaTime;
	}

	Matrix4x4 LorentzBoost(float βx, float βy, float  βz)
	{
		float βSqr = Mathf.Pow(βx,2) + Mathf.Pow(βy,2) + Mathf.Pow(βz,2);
		float γ = 1f/Mathf.Sqrt(1f-βSqr);

		Matrix4x4 boost = new Matrix4x4();
		boost.SetRow(0,new Vector4(γ,      -βx*γ,                     -βy*γ,                     -βz*γ));
		boost.SetRow(1,new Vector4(-βx*γ,  (γ-1)*(βx*βx)/(βSqr) + 1,  (γ-1)*(βx*βy)/(βSqr),      (γ-1)*(βx*βz)/(βSqr)));
		boost.SetRow(2,new Vector4(-βy*γ,  (γ-1)*(βy*βx)/(βSqr),      (γ-1)*(βy*βy)/(βSqr) + 1,  (γ-1)*(βy*βz)/(βSqr)));
		boost.SetRow(3,new Vector4(-βz*γ,  (γ-1)*(βz*βx)/(βSqr),      (γ-1)*(βz*βy)/(βSqr),      (γ-1)*(βz*βz)/(βSqr) + 1));

		return boost;
	}
}
