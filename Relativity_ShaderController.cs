using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relativity_ShaderController : MonoBehaviour {
	public GameObject Observer;
	public Vector3 velocity;
	public bool useProperVelocity;
	public float ProperTimeOffset;
	public List<Vector4> ProperAccelerations;
	public List<Vector4> AccelPoints;
	public float ProperTime;
	public Vector4 CoordinateCurrentEvent;
	public Vector4 ObserverCurrentEvent;
	public Vector3 ProperVelocity;
	private bool shaderMat = false;
	private Material mat;
	private Relativity_Observer observer_script;
	private SkinnedMeshRenderer MR;
	// Use this for initialization
	void Start () {
		if (!Observer){
			Observer = Camera.main.gameObject;
		}
		Renderer r = GetComponent<Renderer>();
		if (r != null)
			mat = r.material;
		if (mat != null){
			if (mat.shader == Shader.Find("Relativity/Relativity_Shader"))
				shaderMat = true;
		}
		observer_script = Observer.GetComponent<Relativity_Observer>();
		MR = GetComponent<SkinnedMeshRenderer>();
		if (MR){
			MR.updateWhenOffscreen = true;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (shaderMat){
			Vector3 observer_velocity = -observer_script.velocity;
			Vector3 relative_proper_velocity;
			if (useProperVelocity){
				Vector3 observer_proper_velocity = observer_velocity / Mathf.Sqrt(1f - observer_velocity.sqrMagnitude);
				relative_proper_velocity = ProperVelAdd(velocity, observer_proper_velocity);
			}else{
				Vector3 relative_velocity = RelVelAdd(velocity, observer_velocity);
				relative_proper_velocity = relative_velocity / Mathf.Sqrt(1f - relative_velocity.sqrMagnitude);
			}
	
			mat.SetVector("_Proper_Velocity", relative_proper_velocity);
			for (int i=AccelPoints.Count; i<ProperAccelerations.Count; ++i){
				if (AccelPoints.Count == 0)
					AccelPoints.Add(Vector4.zero);
				else
					AccelPoints.Add(AccelPoints[i-1]);
			}
			if (ProperAccelerations.Count > 0 && AccelPoints.Count > 0){
				mat.SetVectorArray("_accelerations", ProperAccelerations.ToArray());
				mat.SetVectorArray("_accel_positions", AccelPoints.ToArray());
			}
			mat.SetFloat("_Proper_Time_Offset", ProperTimeOffset);
		}
		DrawPath();
		CalculateCurrentState((float)observer_script.CoordinateTime);
	}

	void CalculateCurrentState(float coordinate_time){
		Vector3 current_velocity = velocity;
		Vector3 current_proper_velocity = velocity;
		if (useProperVelocity){
			current_velocity = velocity / Mathf.Sqrt(1f + velocity.sqrMagnitude);
		}else{
			current_proper_velocity = velocity / Mathf.Sqrt(1f - velocity.sqrMagnitude);
		}
		Vector3 observer_velocity = -observer_script.velocity;
		Vector3 observer_proper_velocity = observer_velocity / Mathf.Sqrt(1f - observer_velocity.sqrMagnitude);

		//current_proper_velocity = ProperVelAdd(observer_proper_velocity, current_proper_velocity);
		current_proper_velocity = current_proper_velocity;
		current_velocity = current_proper_velocity / Mathf.Sqrt(1f + current_proper_velocity.sqrMagnitude);

		Matrix4x4 boost = LorentzBoost(-current_velocity);
		Matrix4x4 obs_boost = LorentzBoost(observer_velocity);
		Vector4 proper_starting_event = CombineTemporalAndSpacial(-ProperTimeOffset, transform.position);
		CoordinateCurrentEvent = boost * proper_starting_event;
		ObserverCurrentEvent = obs_boost * coordinate_current_event;
		

		ProperTime = GetTemporalComponent(proper_starting_event) + ProperTimeOffset;
		
		if (coordinate_time > GetTemporalComponent(CoordinateCurrentEvent)){
			for (int i=0; i<ProperAccelerations.Count; ++i){
				Vector3 a = GetSpacialComponents(ProperAccelerations[i]);
				Vector3 u_0 = current_proper_velocity;
				float proper_duration = GetTemporalComponent(ProperAccelerations[i]);

				Vector3 pos = (Vector3)AccelPoints[i];
				float L = Vector3.Dot(pos, a)/a.magnitude;

				if (L <= 1f/a.magnitude){
					float b = 1f/(1f-a.magnitude*L);
					a *= b;
					proper_duration /= b;
				}else{
					a = new Vector3(0,0,0);
					proper_duration = 0;
				}
				if (a.magnitude > 0){
					float coordinate_duration = (Vector3.Dot(a,u_0)*(float)Cosh(a.magnitude*proper_duration) + a.magnitude*Mathf.Sqrt(1+u_0.sqrMagnitude)*(float)Sinh(a.magnitude*proper_duration) - Vector3.Dot(a,u_0))/a.sqrMagnitude;
					if (coordinate_time > GetTemporalComponent(CoordinateCurrentEvent) + coordinate_duration){
						current_velocity = (a*coordinate_duration+u_0)/Mathf.Sqrt(1 + (a*coordinate_duration+u_0).sqrMagnitude);
						float t = coordinate_duration;
						Vector3 displacement = (
						                        a*a.magnitude*(Mathf.Sqrt(1+(a*t+u_0).sqrMagnitude)-Mathf.Sqrt(1+u_0.sqrMagnitude)) - 
						                        Vector3.Cross(Vector3.Cross(u_0, a),-a)*
						                        (
						                       	    Mathf.Log(Vector3.Dot(a,u_0) + a.magnitude*Mathf.Sqrt(1+u_0.sqrMagnitude)) - 
						                       	    Mathf.Log(a.sqrMagnitude*t+Vector3.Dot(a,u_0)+a.magnitude*Mathf.Sqrt(1+(a*t+u_0).sqrMagnitude))
						                        )
						                       ) / Mathf.Pow(a.magnitude,3);
						CoordinateCurrentEvent += CombineTemporalAndSpacial(t, displacement);
						current_proper_velocity += a*t;
						ProperTime += proper_duration;
					}else{
						coordinate_duration = coordinate_time - GetTemporalComponent(CoordinateCurrentEvent);
						float t = coordinate_duration;
						Vector3 displacement = (
						                        a*a.magnitude*(Mathf.Sqrt(1+(a*t+u_0).sqrMagnitude)-Mathf.Sqrt(1+u_0.sqrMagnitude)) - 
						                        Vector3.Cross(Vector3.Cross(u_0, a),-a)*
						                        (
						                       	    Mathf.Log(Vector3.Dot(a,u_0) + a.magnitude*Mathf.Sqrt(1+u_0.sqrMagnitude)) - 
						                       	    Mathf.Log(a.sqrMagnitude*t+Vector3.Dot(a,u_0)+a.magnitude*Mathf.Sqrt(1+(a*t+u_0).sqrMagnitude))
						                        )
						                       ) / Mathf.Pow(a.magnitude,3);
						CoordinateCurrentEvent += CombineTemporalAndSpacial(t, displacement);
						current_proper_velocity += a*t;
						ProperTime += (
								            Mathf.Sqrt(1f/(1+(a*coordinate_duration+u_0).sqrMagnitude))*Mathf.Sqrt(1+(a*coordinate_duration+u_0).sqrMagnitude)*Mathf.Log(Vector3.Dot(a,a*coordinate_duration+u_0)+a.magnitude*Mathf.Sqrt(1+(a*coordinate_duration+u_0).sqrMagnitude)) - 
								            Mathf.Sqrt(1/(1+u_0.sqrMagnitude))*Mathf.Sqrt(1+u_0.sqrMagnitude)*Mathf.Log(Vector3.Dot(a,u_0) + a.magnitude*Mathf.Sqrt(1+u_0.sqrMagnitude))
								           ) / a.magnitude;
						break;
					}
				}else{
					float coordinate_duration = proper_duration / Mathf.Sqrt(1f - current_velocity.sqrMagnitude);
					if (coordinate_time > GetTemporalComponent(CoordinateCurrentEvent) + coordinate_duration){
						Vector3 displacement = current_velocity * coordinate_duration;
						ProperTime += proper_duration;
						CoordinateCurrentEvent += CombineTemporalAndSpacial(coordinate_duration, displacement);
					}else{
						coordinate_duration = coordinate_time - GetTemporalComponent(CoordinateCurrentEvent);
						Vector3 displacement = current_velocity * coordinate_duration;
						CoordinateCurrentEvent += CombineTemporalAndSpacial(coordinate_duration, displacement);
						ProperTime += coordinate_duration * Mathf.Sqrt(1f - current_velocity.sqrMagnitude);
					}
				}
			}
		}
		ProperTime += (coordinate_time - GetTemporalComponent(CoordinateCurrentEvent)) * Mathf.Sqrt(1f - current_velocity.sqrMagnitude);
		CoordinateCurrentEvent += (coordinate_time - GetTemporalComponent(CoordinateCurrentEvent)) * CombineTemporalAndSpacial(1, current_velocity);
		ProperVelocity = current_proper_velocity;
		if (MR){
			Bounds newBounds = MR.localBounds;
			newBounds.center = transform.InverseTransformPoint(GetSpacialComponents(CoordinateCurrentEvent));
			MR.localBounds = newBounds;
		}
	}

	void DrawPath()
	{
		float coordinate_time = (float)observer_script.CoordinateTime;
		Vector3 current_velocity = velocity;
		Vector3 current_proper_velocity = velocity;
		if (useProperVelocity){
			current_velocity = velocity / Mathf.Sqrt(1f + velocity.sqrMagnitude);
		}else{
			current_proper_velocity = velocity / Mathf.Sqrt(1f - velocity.sqrMagnitude);
		}
		Vector3 observer_velocity = -observer_script.velocity;
		Vector3 observer_proper_velocity = observer_velocity / Mathf.Sqrt(1f - observer_velocity.sqrMagnitude);

		current_proper_velocity = ProperVelAdd(observer_proper_velocity, current_proper_velocity);
		current_velocity = current_proper_velocity / Mathf.Sqrt(1f + current_proper_velocity.sqrMagnitude);
		//Matrix4x4 Λ = WignerBoost(v_observer,v_object);
		Matrix4x4 boost = LorentzBoost(-current_velocity);
		Vector4 proper_starting_event = CombineTemporalAndSpacial(ProperTimeOffset, transform.position);
		Vector4 coordinate_current_event = boost * CombineTemporalAndSpacial(-ProperTimeOffset, GetSpacialComponents(proper_starting_event));
		
		Debug.DrawRay(setZAxis(GetSpacialComponents(coordinate_current_event),GetTemporalComponent(coordinate_current_event) - coordinate_time), -setZAxis(current_velocity,1)*10000, new Color(0.3f, 0.3f, 0.3f));

		float proper_time = 0;

		for (int i=0; i<ProperAccelerations.Count; ++i){
			Vector3 a = GetSpacialComponents(ProperAccelerations[i]);
			float proper_duration = GetTemporalComponent(ProperAccelerations[i]);
			Vector3 pos = (Vector3)AccelPoints[i] - GetSpacialComponents(proper_starting_event) + transform.position;
			float L = Vector3.Dot(pos, a)/a.magnitude;
			if (L <= 1f/a.magnitude){
				float b = 1f/(1f-a.magnitude*L);
				a *= b;
				proper_duration /= b;
			}else{
				a = new Vector3(0,0,0);
				proper_duration = 0;
			}

			Vector3 u_0 = current_velocity / Mathf.Sqrt(1 - current_velocity.sqrMagnitude);

			Color col = i%2==0 ? Color.green : Color.cyan;
			if (a.magnitude > 0){
				float coordinate_duration = (Vector3.Dot(a,u_0)*(float)Cosh(a.magnitude*proper_duration) + a.magnitude*Mathf.Sqrt(1+u_0.sqrMagnitude)*(float)Sinh(a.magnitude*proper_duration) - Vector3.Dot(a,u_0))/a.sqrMagnitude;
				float proper_time_next = proper_time + proper_duration;
				while(proper_time < proper_time_next){
					u_0 = current_velocity / Mathf.Sqrt(1 - current_velocity.sqrMagnitude);
					float dotProd = Vector3.Dot(setZAxis(u_0,1).normalized, setZAxis(u_0 + a * proper_duration,1).normalized);
					float T = 0.5f;
					if (proper_time % 0.5f != 0){
						T = proper_time % 0.5f;
					}
					T = Mathf.Min(proper_time_next - proper_time, T);

					float t = (Vector3.Dot(a,u_0)*(float)Cosh(a.magnitude*T) + a.magnitude*Mathf.Sqrt(1+u_0.sqrMagnitude)*(float)Sinh(a.magnitude*T) - Vector3.Dot(a,u_0))/a.sqrMagnitude;
					Vector4 coordinate_prev_event = coordinate_current_event;
					Vector3 prev_proper_velocity = current_proper_velocity;
					if ((proper_time + T)%0.5f == 0)
						Debug.DrawRay(setZAxis(GetSpacialComponents(coordinate_prev_event),GetTemporalComponent(coordinate_prev_event) - coordinate_time), Vector3.up, col);
					else
						Debug.DrawRay(setZAxis(GetSpacialComponents(coordinate_prev_event),GetTemporalComponent(coordinate_prev_event) - coordinate_time), Vector3.up, Color.red);
					Vector3 displacement = (
					                        a*a.magnitude*(Mathf.Sqrt(1+(a*t+u_0).sqrMagnitude)-Mathf.Sqrt(1+u_0.sqrMagnitude)) - 
					                        Vector3.Cross(Vector3.Cross(u_0, a),-a)*
					                        (
					                       	    Mathf.Log(Vector3.Dot(a,u_0) + a.magnitude*Mathf.Sqrt(1+u_0.sqrMagnitude)) - 
					                       	    Mathf.Log(a.sqrMagnitude*t+Vector3.Dot(a,u_0)+a.magnitude*Mathf.Sqrt(1+(a*t+u_0).sqrMagnitude))
					                        )
					                       ) / Mathf.Pow(a.magnitude,3);
					
					coordinate_current_event += CombineTemporalAndSpacial(t, displacement);
					current_velocity = (a*t+u_0)/Mathf.Sqrt(1 + (a*t+u_0).sqrMagnitude);
					current_proper_velocity = current_velocity / Mathf.Sqrt(1 - current_velocity.sqrMagnitude);
					proper_time += T;
					Debug.DrawLine(	setZAxis(GetSpacialComponents(coordinate_prev_event),GetTemporalComponent(coordinate_prev_event) - coordinate_time), 
									setZAxis(GetSpacialComponents(coordinate_current_event),GetTemporalComponent(coordinate_current_event) - coordinate_time),
									col);
				}
			}else{
				Vector4 coordinate_prev_event = coordinate_current_event;
				float coordinate_duration = proper_duration / Mathf.Sqrt(1f - current_velocity.sqrMagnitude);
				Vector3 displacement = current_velocity * coordinate_duration;
				coordinate_current_event += CombineTemporalAndSpacial(coordinate_duration, displacement);
				proper_time += proper_duration;
				Debug.DrawLine(	setZAxis(GetSpacialComponents(coordinate_prev_event),GetTemporalComponent(coordinate_prev_event) - coordinate_time), 
								setZAxis(GetSpacialComponents(coordinate_current_event),GetTemporalComponent(coordinate_current_event) - coordinate_time),
								col);
			}
		}
		Debug.DrawRay(setZAxis(GetSpacialComponents(coordinate_current_event),GetTemporalComponent(coordinate_current_event) - (float)coordinate_time), setZAxis(current_velocity,1)*10000, Color.white);
	}

	Vector4 CombineTemporalAndSpacial(float t, Vector3 p)
	{
		return new Vector4(t, p.x, p.y, p.z);
	}

	Vector3 RelVelAdd(Vector3 u, Vector3 v)
	{
		float γu = 1f/Mathf.Sqrt(1-u.sqrMagnitude);
		return 1f/(1 + Vector3.Dot(u,v)) * ((1 + γu/(1+γu)*Vector3.Dot(u,v))*u + (1f/γu)*v);
	}

	Vector3 ProperVelAdd(Vector3 u, Vector3 v){
		float Bu = 1f/Mathf.Sqrt(1 + u.sqrMagnitude);
		float Bv = 1f/Mathf.Sqrt(1 + v.sqrMagnitude);
		return u + v + u * (Bu/(1+Bu) * Vector3.Dot(u,v) + (1-Bv)/Bv);
	}

	double Sinh(double x)
	{
		return (System.Math.Pow(System.Math.E, x) - System.Math.Pow(System.Math.E, -x))/2;
	}

	double Cosh(double x)
	{
		return (System.Math.Pow(System.Math.E, x) + System.Math.Pow(System.Math.E, -x))/2;
	}

	double Tanh(double x)
	{
		return Sinh(x)/Cosh(x);
	}

	double ATanh(double x)
	{
		return (System.Math.Log(1 + x) - System.Math.Log(1 - x))/2;
	}

	double ACosh(double x)
	{
		return System.Math.Log(x + System.Math.Sqrt(System.Math.Pow(x,2) - 1));
	}

	double ASinh(double x)
	{
		return System.Math.Log(x + System.Math.Sqrt(1 + System.Math.Pow(x,2)));
	}

	Matrix4x4 WignerBoost(Vector3 u, Vector3 v)
	{
		double γu = 1.0/System.Math.Sqrt(1-u.sqrMagnitude);
		double γv = 1.0/System.Math.Sqrt(1-v.sqrMagnitude);
		double γ = γv * γu * (1 + Vector3.Dot(v,u)); 
		Matrix4x4 rot = RotationMatrix4(u, v);
		Vector3 a = (float)γ*RelVelAdd(u,v);
		Matrix4x4 boost = LorentzBoost(a/(float)γ);
		return rot * boost;
	}

	Matrix4x4 RotationMatrix4(Vector3 u, Vector3 v)
	{
		if (Vector3.Cross(u,v).magnitude == 0)
			return Matrix4x4.identity;
		Matrix4x4 rot = new Matrix4x4();
		float γu = 1f/Mathf.Sqrt(1-u.sqrMagnitude);
		float γv = 1f/Mathf.Sqrt(1-v.sqrMagnitude);
		float γ = γv * γu * (1 + Vector3.Dot(v,u)); 
		float cosθ = (Mathf.Pow(1 + γ + γu + γv,2)) / ((1+γ) * (1+γu) * (1+γv)) - 1;
		float sinθ = Mathf.Sqrt(1 - Mathf.Pow(cosθ,2));
		Vector3 e = Vector3.Cross(u,v).normalized;
		rot.SetRow(0, new Vector4(1,  0,                              0,                              0                              ));
		rot.SetRow(1, new Vector4(0,  cosθ + e.x*e.x*(1 - cosθ),      e.x*e.y*(1 - cosθ) - e.z*sinθ,  e.x*e.z*(1 - cosθ) + e.y*sinθ  ));
		rot.SetRow(2, new Vector4(0,  e.y*e.x*(1 - cosθ) + e.z*sinθ,  cosθ + e.y*e.y*(1 - cosθ),      e.y*e.z*(1 - cosθ) - e.x*sinθ  ));
		rot.SetRow(3, new Vector4(0,  e.z*e.x*(1 - cosθ) - e.y*sinθ,  e.z*e.y*(1 - cosθ) + e.x*sinθ,  cosθ + e.z*e.z*(1 - cosθ)      ));
		return rot;
	}

	Matrix4x4 LorentzBoost(Vector3 v)
	{
		float βx = v.x;
		float βy = v.y;
		float βz = v.z;
		float βSqr = v.sqrMagnitude;

		if (βSqr == 0f)
		{
			return Matrix4x4.identity;
		}

		float γ = 1f/Mathf.Sqrt(1f-βSqr);

		Matrix4x4 boost = new Matrix4x4();
		boost.SetRow(0,new Vector4(γ,      -βx*γ,                     -βy*γ,                     -βz*γ));
		boost.SetRow(1,new Vector4(-βx*γ,  (γ-1)*(βx*βx)/(βSqr) + 1,  (γ-1)*(βx*βy)/(βSqr),      (γ-1)*(βx*βz)/(βSqr)));
		boost.SetRow(2,new Vector4(-βy*γ,  (γ-1)*(βy*βx)/(βSqr),      (γ-1)*(βy*βy)/(βSqr) + 1,  (γ-1)*(βy*βz)/(βSqr)));
		boost.SetRow(3,new Vector4(-βz*γ,  (γ-1)*(βz*βx)/(βSqr),      (γ-1)*(βz*βy)/(βSqr),      (γ-1)*(βz*βz)/(βSqr) + 1));

		return boost;
	}

	Vector3 GetSpacialComponents(Vector4 v)
	{
		return new Vector3(v.y,v.z,v.w);
	}

	float GetTemporalComponent(Vector4 v)
	{
		return v.x;
	}

	Vector3 setZAxis(Vector3 v, float z){
		v.z += z;
		return v;
	}
}
