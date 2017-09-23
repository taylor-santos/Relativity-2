using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Relativity_ChargedObject : MonoBehaviour {
	public float charge;
	public bool drawFieldLines;
	public int fieldLineCount;
	public int fieldLineSegments = 100;
	public float accuracy = 15;

	private List<Relativity_ChargedObject> charges;
	public bool negative;
	//public bool drawFieldLines;
	//public List<GameObject> charges;
	// Use this for initialization
	void Start () {
		GameObject[] objects = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
		charges = new List<Relativity_ChargedObject>();
		foreach(GameObject obj in objects)
		{
			if (obj.GetComponent<Relativity_ChargedObject>() != null)
			{
				charges.Add(obj.GetComponent<Relativity_ChargedObject>());
			}
		}
		negative = GetComponent<Relativity_ChargedObject>().charge < 0;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (drawFieldLines)
		{
			for (int i=0; i<fieldLineCount; ++i)
			{
				Vector3 direction = Quaternion.AngleAxis(i * 360f/fieldLineCount,Vector3.forward) * Vector3.right;
				Vector3 pos = transform.position + direction * 0.55f;
				float dist = 0;
				for (int j=0;j<fieldLineSegments;++j)
				{
					direction = Vector3.zero;
					float minDist = int.MaxValue;
					foreach(Relativity_ChargedObject currCharge in charges)
					{
						if (currCharge.charge != 0)
						{
							Vector3 pos2 = currCharge.transform.position;
							float rSqr = (pos2-pos).sqrMagnitude;
							minDist = Mathf.Min(Mathf.Sqrt(rSqr),minDist);
							if (negative)
								direction += (pos2-pos).normalized * (currCharge.charge)/rSqr;
							else
								direction -= (pos2-pos).normalized * (currCharge.charge)/rSqr;
						}
					}
					Debug.DrawRay(pos,direction.normalized*(minDist/accuracy),Color.Lerp(Color.cyan,Color.cyan - new Color(0,0,0,1),1f/Mathf.Sqrt(direction.magnitude)/5),Time.fixedDeltaTime);
					if (dist > 1)
					{
						Debug.DrawRay(pos,Quaternion.AngleAxis(30,Vector3.forward)*(negative?direction.normalized:-direction.normalized)*0.2f,Color.Lerp(Color.cyan,Color.cyan - new Color(0,0,0,1),1f/Mathf.Sqrt(direction.magnitude)/5),Time.fixedDeltaTime);
						Debug.DrawRay(pos,Quaternion.AngleAxis(-30,Vector3.forward)*(negative?direction.normalized:-direction.normalized)*0.2f,Color.Lerp(Color.cyan,Color.cyan - new Color(0,0,0,1),1f/Mathf.Sqrt(direction.magnitude)/5),Time.fixedDeltaTime);
						dist = 0;
					}
					pos += direction.normalized*(minDist/accuracy);
					dist += minDist/accuracy;
					if (minDist < 0.5f)
						break;
				}
			}
		}
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
