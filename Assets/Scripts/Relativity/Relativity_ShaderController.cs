using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relativity_ShaderController : MonoBehaviour {
	public float T_Test;
	public GameObject Observer;
	public Vector3 velocity;
	public bool useProperVelocity;
	public float ProperTimeOffset;
	public List<Vector4> ProperAccelerations;
	public List<Vector4> AccelPoints;
	public float ProperTime;
	public Matrix4x4 Boost;
	public Vector4 CoordinateCurrentEvent;
	public Vector4 ObserverCurrentEvent;
	public Vector3 ProperVelocity;
	private bool shaderMat = false;
	private Material mat;
	private Relativity_Observer observer_script;
	private SkinnedMeshRenderer MR;
	public Vector3 v;
	public float b;
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
		v = RelVelAdd(new Vector3(0.213f,0.874f,0)*0.655f, new Vector3(-0.353f,0.497f,0)*0.61f);
		b = v.magnitude;
		v = v.normalized;
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
			if (useProperVelocity)
			{
				mat.SetVector("_Proper_Velocity", velocity);
			}else{
				mat.SetVector("_Proper_Velocity", velocity / Mathf.Sqrt(1f - velocity.sqrMagnitude));
			}
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
		Boost = LorentzBoost(velocity);
		//DrawPath();
		DrawPath();
		//CalculateCurrentState((float)observer_script.CoordinateTime);
		CalculateCurrentState((float)observer_script.CoordinateTime);
	}
	void CalculateCurrentState(float coordinate_time){
		Vector3 coordinate_velocity = velocity;
		Vector3 coordinate_proper_velocity = velocity;
		if (useProperVelocity){
			coordinate_velocity = velocity / Mathf.Sqrt(1f + velocity.sqrMagnitude);
		}else{
			coordinate_proper_velocity = velocity / Mathf.Sqrt(1f - velocity.sqrMagnitude);
		}
		Vector3 observer_velocity = -observer_script.velocity;
		Vector3 observer_proper_velocity = observer_velocity / Mathf.Sqrt(1f - observer_velocity.sqrMagnitude);

		Vector3 current_proper_velocity = ProperVelAdd(observer_proper_velocity, coordinate_proper_velocity);
		Vector3 current_velocity = current_proper_velocity / Mathf.Sqrt(1f + current_proper_velocity.sqrMagnitude);

		Matrix4x4 boost = LorentzBoost(-coordinate_velocity); //Object -> coordinate
		Matrix4x4 obs_boost = LorentzBoost(-observer_velocity); //Coordinate -> observer
		Vector4 proper_starting_event = CombineTemporalAndSpacial(-ProperTimeOffset, transform.position);
		Vector4 coordinate_current_event = boost * proper_starting_event;
		Vector4 observer_current_event = obs_boost * coordinate_current_event;

		ProperTime = 0;

		for (int i=0; i<ProperAccelerations.Count; ++i){
			if (GetTemporalComponent(observer_current_event) < coordinate_time){
				Vector3 a = GetSpacialComponents(ProperAccelerations[i]);
				float proper_duration = GetTemporalComponent(ProperAccelerations[i]);
				Color col = i%2==0 ? Color.red : Color.green;
				if (a.magnitude > 0){
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
					Vector3 u_0 = coordinate_proper_velocity;
				
					float coordinate_duration = getDuration(a, u_0, proper_duration);
					Vector4 coordinate_prev_event = coordinate_current_event;

					Vector3 displacement = getDisplacement(a, u_0, coordinate_duration);
					Vector4 new_coordinate_event = coordinate_current_event + CombineTemporalAndSpacial(coordinate_duration, displacement);
					Vector4 new_observer_event = obs_boost * new_coordinate_event;
					Vector3 new_proper_velocity = coordinate_proper_velocity + a*coordinate_duration;
					Vector3 new_current_proper_velocity = ProperVelAdd(observer_proper_velocity, new_proper_velocity);
					//Debug.DrawRay(setZAxis(GetSpacialComponents(new_observer_event),GetTemporalComponent(new_observer_event) - coordinate_time), setZAxis(new_current_proper_velocity / Mathf.Sqrt(1f + new_current_proper_velocity.sqrMagnitude),1), Color.white);
					/*
					if (GetTemporalComponent(new_observer_event) < coordinate_time){
						coordinate_proper_velocity = new_proper_velocity;
						coordinate_velocity = coordinate_proper_velocity / Mathf.Sqrt(1f + coordinate_proper_velocity.sqrMagnitude);
						ProperTime += proper_duration;
						coordinate_current_event = new_coordinate_event;
						observer_current_event = new_observer_event;
					}else{
						while (GetTemporalComponent(observer_current_event) < coordinate_time){
							proper_duration = 0.1f;
							u_0 = coordinate_proper_velocity;
							coordinate_duration = getDuration(a, u_0, proper_duration);
							displacement = getDisplacement(a, u_0, coordinate_duration);
							coordinate_current_event += CombineTemporalAndSpacial(coordinate_duration, displacement);
							observer_current_event = obs_boost * coordinate_current_event;
							coordinate_proper_velocity += a*coordinate_duration;
							ProperTime += proper_duration;
						}
						break;
					}
					*/
					if (GetTemporalComponent(new_observer_event) > coordinate_time)
					{
						coordinate_duration = coordinate_time - GetTemporalComponent(coordinate_current_event);
						proper_duration = getProperDuration(a, u_0, coordinate_duration);
						displacement = getDisplacement(a, u_0, coordinate_duration);
						new_coordinate_event = coordinate_current_event + CombineTemporalAndSpacial(coordinate_duration, displacement);
						new_observer_event = obs_boost * new_coordinate_event;
						new_proper_velocity = coordinate_proper_velocity + a*coordinate_duration;
						new_current_proper_velocity = ProperVelAdd(observer_proper_velocity, new_proper_velocity);
					}
					coordinate_proper_velocity = new_proper_velocity;
					coordinate_velocity = coordinate_proper_velocity / Mathf.Sqrt(1f + coordinate_proper_velocity.sqrMagnitude);
					ProperTime += proper_duration;
					coordinate_current_event = new_coordinate_event;
					observer_current_event = new_observer_event;				
				}else{
					float coordinate_duration = proper_duration / Mathf.Sqrt(1f + coordinate_proper_velocity.sqrMagnitude);
					Vector3 displacement = coordinate_velocity * coordinate_duration;
					coordinate_current_event += CombineTemporalAndSpacial(coordinate_duration, displacement);
					ProperTime += proper_duration;
					Vector4 observer_prev_event = observer_current_event;
					observer_current_event = obs_boost * coordinate_current_event;
				}
			}else{
				break;
			}
		}
		current_proper_velocity = ProperVelAdd(observer_proper_velocity, coordinate_proper_velocity);
		current_velocity = current_proper_velocity / Mathf.Sqrt(1f + current_proper_velocity.sqrMagnitude);
		ProperTime += (coordinate_time - GetTemporalComponent(observer_current_event)) * Mathf.Sqrt(1f - current_velocity.sqrMagnitude);
		observer_current_event += (coordinate_time - GetTemporalComponent(observer_current_event)) * CombineTemporalAndSpacial(1, current_velocity);
		Debug.DrawRay(setZAxis(GetSpacialComponents(observer_current_event),GetTemporalComponent(observer_current_event) - (float)observer_script.CoordinateTime), Vector3.up, Color.green);
		Debug.DrawRay(setZAxis(GetSpacialComponents(observer_current_event),GetTemporalComponent(observer_current_event) - (float)observer_script.CoordinateTime), setZAxis(current_velocity,1)*0.5f, Color.blue);
		ProperVelocity = current_proper_velocity;
		if (MR){
			Bounds newBounds = MR.localBounds;
			newBounds.center = transform.InverseTransformPoint(GetSpacialComponents(observer_current_event));
			MR.localBounds = newBounds;
		}

	}

	void DrawPath(){
		float coordinate_time = (float)observer_script.CoordinateTime;

		Vector3 coordinate_velocity = velocity;
		Vector3 coordinate_proper_velocity = velocity;
		if (useProperVelocity){
			coordinate_velocity = velocity / Mathf.Sqrt(1f + velocity.sqrMagnitude);
		}else{
			coordinate_proper_velocity = velocity / Mathf.Sqrt(1f - velocity.sqrMagnitude);
		}
		Vector3 observer_velocity = -observer_script.velocity;
		Vector3 observer_proper_velocity = observer_velocity / Mathf.Sqrt(1f - observer_velocity.sqrMagnitude);

		Vector3 current_proper_velocity = ProperVelAdd(observer_proper_velocity, coordinate_proper_velocity);
		Vector3 current_velocity = current_proper_velocity / Mathf.Sqrt(1f + current_proper_velocity.sqrMagnitude);

		Matrix4x4 boost = LorentzBoost(-coordinate_velocity); //Object -> coordinate
		Matrix4x4 obs_boost = LorentzBoost(-observer_velocity); //Coordinate -> observer
		Vector4 proper_starting_event = CombineTemporalAndSpacial(-ProperTimeOffset, transform.position);
		Vector4 coordinate_current_event = boost * proper_starting_event;
		Vector4 observer_current_event = obs_boost * coordinate_current_event;
		
		Debug.DrawRay(setZAxis(GetSpacialComponents(observer_current_event),GetTemporalComponent(observer_current_event) - coordinate_time), -setZAxis(current_velocity,1)*10000, Color.cyan);

		float proper_time = 0;

		for (int i=0; i<ProperAccelerations.Count; ++i){
			Vector3 a = GetSpacialComponents(ProperAccelerations[i]);
			float proper_duration = GetTemporalComponent(ProperAccelerations[i]);
			Color col = i%2==0 ? Color.red : Color.green;
			if (a.magnitude > 0){
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
				Vector3 u_0 = coordinate_proper_velocity;
				Vector4 prev_observer_event = observer_current_event;
				Vector3 prev_proper_velocity = ProperVelAdd(observer_proper_velocity, coordinate_proper_velocity);
				float coordinate_duration = getDuration(a, u_0, proper_duration);
				float proper_time_next = proper_time + proper_duration;

				float increment = 1f;
				while(proper_time < proper_time_next){
					u_0 = coordinate_proper_velocity;
					float dotProd = Vector3.Dot(setZAxis(u_0,1).normalized, setZAxis(u_0 + a * proper_duration,1).normalized);
					float T = increment;
					if (proper_time % increment != 0){
						T = proper_time % increment;
					}
					T = Mathf.Min(proper_time_next - proper_time, T);

					float t = getDuration(a, u_0, T);
					Vector4 coordinate_prev_event = coordinate_current_event;

					Vector3 displacement = getDisplacement(a, u_0, t);
					
					coordinate_current_event += CombineTemporalAndSpacial(t, displacement);
					//coordinate_proper_velocity = ProperVelAdd(coordinate_proper_velocity, a*t);
					coordinate_proper_velocity += a*t;
					coordinate_velocity = coordinate_proper_velocity / Mathf.Sqrt(1f + coordinate_proper_velocity.sqrMagnitude);
					proper_time += T;

					Vector4 observer_prev_event = observer_current_event;
					observer_current_event = obs_boost * coordinate_current_event;
					Debug.DrawLine(	setZAxis(GetSpacialComponents(observer_prev_event), GetTemporalComponent(observer_prev_event) - coordinate_time), 
									setZAxis(GetSpacialComponents(observer_current_event), GetTemporalComponent(observer_current_event) - coordinate_time),
									col);
				}
				float observer_duration = GetTemporalComponent(observer_current_event - prev_observer_event);
				Vector3 disp = getDisplacement(a, prev_proper_velocity, observer_duration);
				Vector4 new_observer_event = prev_observer_event + CombineTemporalAndSpacial(observer_duration, disp);

			}else{
				float coordinate_duration = proper_duration / Mathf.Sqrt(1f + coordinate_proper_velocity.sqrMagnitude);
				Vector3 displacement = coordinate_velocity * coordinate_duration;
				coordinate_current_event += CombineTemporalAndSpacial(coordinate_duration, displacement);
				proper_time += proper_duration;
				Vector4 observer_prev_event = observer_current_event;
				observer_current_event = obs_boost * coordinate_current_event;
				Debug.DrawLine(	setZAxis(GetSpacialComponents(observer_prev_event), GetTemporalComponent(observer_prev_event) - coordinate_time), 
								setZAxis(GetSpacialComponents(observer_current_event), GetTemporalComponent(observer_current_event) - coordinate_time),
								col);
			}
		}
		current_proper_velocity = ProperVelAdd(observer_proper_velocity, coordinate_proper_velocity);
		current_velocity = current_proper_velocity / Mathf.Sqrt(1f + current_proper_velocity.sqrMagnitude);
		Debug.DrawRay(setZAxis(GetSpacialComponents(observer_current_event), GetTemporalComponent(observer_current_event) - (float)coordinate_time), setZAxis(current_velocity,1)*10000, Color.yellow);
	}

	Vector3 getDisplacement(Vector3 a, Vector3 u_0, float t){
		return
		(
		    a*a.magnitude
		    *
		    (
		        Mathf.Sqrt(
		            1 + (a*t + u_0).sqrMagnitude
		        )
		        -
		        Mathf.Sqrt(
		            1 + u_0.sqrMagnitude
		        )
		    )
		    - 
		    Vector3.Cross(
		        Vector3.Cross(
		            u_0, 
		            a
		        ),
		        -a
		    )
		    *
		    (
		        Mathf.Log(
		            Vector3.Dot(
		                a,
		                u_0
		            )
		            + 
		            a.magnitude
		            *
		            Mathf.Sqrt(
		                1 + u_0.sqrMagnitude
		            )
		        )
		        - 
		        Mathf.Log(
		            a.sqrMagnitude*t
		            +
		            Vector3.Dot(
		                a,
		                u_0
		            )
		            +
		            a.magnitude
		            *
		            Mathf.Sqrt(
		                1 + (a*t + u_0).sqrMagnitude
		            )
		        )
		    )
		) 
		/ 
		Mathf.Pow(
		    a.magnitude,
		    3
		);
	}

	float getDuration(Vector3 a, Vector3 u_0, float T){
		return
		(
		    Vector3.Dot(
		        a,
		        u_0
		    )
		    *
		    (float)Cosh(
		        a.magnitude*T
		    )
		    + 
		    a.magnitude
		    *
		    Mathf.Sqrt(
		        1 + u_0.sqrMagnitude
		    )
		    *
		    (float)Sinh(
		        a.magnitude*T
		    )
		    -
		    Vector3.Dot(
		        a,
		        u_0
		    )
		)
		/
		a.sqrMagnitude;
	}

	float getProperDuration(Vector3 a, Vector3 u_0, float t){
		return
		(
		    Mathf.Sqrt(
		        1f
		        /
		        (
		            1 
		            + 
		            (
		                a*t + u_0
		            ).sqrMagnitude
		        )
		    )
		    *
		    Mathf.Sqrt(
		        1f
		        +
		        (
		            a*t
		            +
		            u_0
		        ).sqrMagnitude
		    )
		    *
		    Mathf.Log(
		        Vector3.Dot(
		            a,
		            a*t + u_0
		        )
		        +
		        a.magnitude
		        *
		        Mathf.Sqrt(
		            1
		            +
		            (
		                a*t + u_0
		            ).sqrMagnitude
		        )
		    ) 
		    - 
		    Mathf.Sqrt(
		        1f
		        /
		        (
		            1 + u_0.sqrMagnitude
		        )
		    )
		    *
		    Mathf.Sqrt(
		        1f + u_0.sqrMagnitude
		    )
		    *
		    Mathf.Log(
		        Vector3.Dot(
		            a,
		            u_0
		        ) 
		        + 
		        a.magnitude
		        *
		        Mathf.Sqrt(
		            1f + u_0.sqrMagnitude
		        )
		    )
		) 
		/ 
		a.magnitude;
	}

	Vector4 CombineTemporalAndSpacial(float t, Vector3 p)
	{
		return new Vector4(t, p.x, p.y, p.z);
	}

	Vector3 RelVelAdd(Vector3 v, Vector3 u)
	{
		//float γu = 1f/Mathf.Sqrt(1-u.sqrMagnitude);
		//return 1f/(1 + Vector3.Dot(u,v)) * ((1 + γu/(1+γu)*Vector3.Dot(u,v))*u + (1f/γu)*v);
		return 1f/(1+Vector3.Dot(u,v))*(v + u*Mathf.Sqrt(1f+v.sqrMagnitude) + (1f/(Mathf.Sqrt(1+v.sqrMagnitude))/(1+1f/Mathf.Sqrt(1+v.sqrMagnitude)))*Vector3.Dot(u,v)*v);
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
