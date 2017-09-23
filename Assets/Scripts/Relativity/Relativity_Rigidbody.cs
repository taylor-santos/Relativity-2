using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Relativity_Rigidbody : MonoBehaviour {
	public GameObject Observer;
	public Vector3 Velocity;
	public double Proper_Time_Offset;
	public List<Vector4> Proper_Accelerations;
	public Vector3 Relative_Velocity;
	public double γ;
	public double Proper_Time;
	public bool Draw_World_Line = false;
	public Vector4 coordinate_current_event;

	public Vector4 proper_starting_event;
	private double coordinate_time;
	private Relativity_Observer observer_script;
	private GameObject scaler;
	private GameObject rotater;
	public bool shaderMat = false;
	private Material mat;

	void Start () {
		if (Velocity.magnitude >= 0.9999999f)
			Velocity = Velocity.normalized * (0.9999999f);
		observer_script = Observer.GetComponent<Relativity_Observer>();
		coordinate_time = observer_script.CoordinateTime;
		
		scaler = new GameObject();
		scaler.name = name + " - Scaler";

		rotater = new GameObject();
		rotater.name = name + " - Rotater";

		proper_starting_event = CombineTemporalAndSpacial(-(float)Proper_Time_Offset, transform.position);
		//mat = GetComponent<Renderer>().material;
		//if (mat.shader == Shader.Find("Relativity/Relativity_Shader"))
		//	shaderMat = true;
	}

	void FixedUpdate()
	{
		coordinate_time = observer_script.CoordinateTime;
		
		Vector3 v_observer = -observer_script.velocity;
		if (v_observer.magnitude >= 0.9999999f)
			v_observer = v_observer.normalized * (0.9999999f);
		double γ_observer = LorentzFactor(v_observer);

		Vector3 v_object = Velocity;
		if (v_object.magnitude >= 0.9999999f)
			v_object = v_object.normalized * (0.9999999f);

		//Relative_Velocity = RelVelAdd(v_observer, v_object);
		//γ = 1f/Mathf.Sqrt(1f - Relative_Velocity.sqrMagnitude);

		Matrix4x4 Λ_starting = WignerBoost(v_observer,v_object);
		//Matrix4x4 object_boost = LorentzBoost(v_object);
		proper_starting_event = CombineTemporalAndSpacial(-(float)Proper_Time_Offset, GetSpacialComponents(proper_starting_event));
		Vector4 coordinate_zero_proper_time_event = Λ_starting.inverse * proper_starting_event;
		coordinate_current_event = coordinate_zero_proper_time_event;
		Relative_Velocity = RelVelAdd(v_observer, v_object);
		Proper_Time = GetTemporalComponent(proper_starting_event) + (float)Proper_Time_Offset;
		
		if (coordinate_time > GetTemporalComponent(coordinate_zero_proper_time_event))
		{
			for (int i=0; i<Proper_Accelerations.Count; ++i)
			{
				Vector3 a = GetSpacialComponents(Proper_Accelerations[i]);
				float T = GetTemporalComponent(Proper_Accelerations[i]);
				if (a.magnitude > 0){
					Vector3 u_0 = Relative_Velocity / Mathf.Sqrt(1 - Relative_Velocity.sqrMagnitude);
					float t = (Vector3.Dot(a,u_0)*(float)Cosh(a.magnitude*T) + a.magnitude*Mathf.Sqrt(1+u_0.sqrMagnitude)*(float)Sinh(a.magnitude*T) - Vector3.Dot(a,u_0))/a.sqrMagnitude;

					if ((float)coordinate_time > GetTemporalComponent(coordinate_current_event)+t){
						Vector3 displacement = (
						                        a*a.magnitude*(Mathf.Sqrt(1+(a*t+u_0).sqrMagnitude)-Mathf.Sqrt(1+u_0.sqrMagnitude)) - 
						                        Vector3.Cross(Vector3.Cross(u_0, a),-a)*
						                        (
						                       	    Mathf.Log(Vector3.Dot(a,u_0) + a.magnitude*Mathf.Sqrt(1+u_0.sqrMagnitude)) - 
						                       	    Mathf.Log(a.sqrMagnitude*t+Vector3.Dot(a,u_0)+a.magnitude*Mathf.Sqrt(1+(a*t+u_0).sqrMagnitude))
						                        )
						                       ) / Mathf.Pow(a.magnitude,3);
						coordinate_current_event += CombineTemporalAndSpacial(t, displacement);
						Relative_Velocity = (a*t+u_0)/Mathf.Sqrt(1 + (a*t+u_0).sqrMagnitude);
						Proper_Time += (
							            Mathf.Sqrt(1f/(1+(a*t+u_0).sqrMagnitude))*Mathf.Sqrt(1+(a*t+u_0).sqrMagnitude)*Mathf.Log(Vector3.Dot(a,a*t+u_0)+a.magnitude*Mathf.Sqrt(1+(a*t+u_0).sqrMagnitude)) - 
							            Mathf.Sqrt(1/(1+u_0.sqrMagnitude))*Mathf.Sqrt(1+u_0.sqrMagnitude)*Mathf.Log(Vector3.Dot(a,u_0) + a.magnitude*Mathf.Sqrt(1+u_0.sqrMagnitude))
							           ) / a.magnitude;
					}else{
						t = (float)coordinate_time - GetTemporalComponent(coordinate_current_event);
						Vector3 displacement = (
						                        a*a.magnitude*(Mathf.Sqrt(1+(a*t+u_0).sqrMagnitude)-Mathf.Sqrt(1+u_0.sqrMagnitude)) - 
						                        Vector3.Cross(Vector3.Cross(u_0, a),-a)*
						                        (
						                       	    Mathf.Log(Vector3.Dot(a,u_0) + a.magnitude*Mathf.Sqrt(1+u_0.sqrMagnitude)) - 
						                       	    Mathf.Log(a.sqrMagnitude*t+Vector3.Dot(a,u_0)+a.magnitude*Mathf.Sqrt(1+(a*t+u_0).sqrMagnitude))
						                        )
						                       ) / Mathf.Pow(a.magnitude,3);
						coordinate_current_event += CombineTemporalAndSpacial(t, displacement);
						Relative_Velocity = (a*t+u_0)/Mathf.Sqrt(1 + (a*t+u_0).sqrMagnitude);
						Proper_Time += (
							            Mathf.Sqrt(1f/(1+(a*t+u_0).sqrMagnitude))*Mathf.Sqrt(1+(a*t+u_0).sqrMagnitude)*Mathf.Log(Vector3.Dot(a,a*t+u_0)+a.magnitude*Mathf.Sqrt(1+(a*t+u_0).sqrMagnitude)) - 
							            Mathf.Sqrt(1/(1+u_0.sqrMagnitude))*Mathf.Sqrt(1+u_0.sqrMagnitude)*Mathf.Log(Vector3.Dot(a,u_0) + a.magnitude*Mathf.Sqrt(1+u_0.sqrMagnitude))
							           ) / a.magnitude;
						break;
					}
				}else{
					float coordinate_duration = T / Mathf.Sqrt(1f - Relative_Velocity.sqrMagnitude);
					if ((float)coordinate_time > GetTemporalComponent(coordinate_current_event)+coordinate_duration){
						Vector3 displacement = Relative_Velocity * coordinate_duration;
						coordinate_current_event += CombineTemporalAndSpacial(coordinate_duration, displacement);
						Proper_Time += T;
					}else{
						coordinate_duration = (float)coordinate_time - GetTemporalComponent(coordinate_current_event);
						Vector3 displacement = Relative_Velocity * coordinate_duration;
						coordinate_current_event += CombineTemporalAndSpacial(coordinate_duration, displacement);
						//Proper_Time += coordinate_duration / Mathf.Sqrt(1f - current_relative_velocity.sqrMagnitude);
						break;
					}
				}
			}
		}
		

		double γ_object = 1.0/System.Math.Sqrt(1 - v_object.sqrMagnitude);
		//Relative_Velocity = RelVelAdd(v_observer, v_object);
		γ = 1f/Mathf.Sqrt(1f - Relative_Velocity.sqrMagnitude);
		Matrix4x4 Λ_current = WignerBoost(v_observer,v_object);

		Proper_Time += (coordinate_time - GetTemporalComponent(coordinate_current_event)) / γ;
		coordinate_current_event += ((float)coordinate_time - GetTemporalComponent(coordinate_current_event)) * CombineTemporalAndSpacial(1, Relative_Velocity);

		//Proper_Time = GetTemporalComponent(Λ_current * coordinate_current_event) - GetTemporalComponent(proper_starting_event);

		scaler.transform.localScale = Vector3.one;
		rotater.transform.parent = null;
		rotater.transform.localScale = Vector3.one;
		rotater.transform.rotation = Quaternion.identity;
		transform.parent = null;
		transform.position = GetSpacialComponents(coordinate_current_event);
		
		scaler.transform.position = transform.position;
		scaler.transform.LookAt(scaler.transform.position + Relative_Velocity);

		double cosθ = (System.Math.Pow(1 + γ + γ_observer + γ_object,2)) / ((1+γ) * (1+γ_observer) * (1+γ_object)) - 1;
		double θ = System.Math.Acos(cosθ);

		rotater.transform.position = transform.position;
		transform.parent = rotater.transform;

		rotater.transform.rotation = Quaternion.AngleAxis(-(float)θ * Mathf.Rad2Deg, Vector3.Cross(v_observer,v_object).normalized);
		rotater.transform.parent = scaler.transform;
		scaler.transform.localScale = new Vector3(1,1,1f/(float)γ);

		if (Draw_World_Line)
			DrawPath();
	}

	void DrawPath()
	{
		Vector3 v_observer = -observer_script.velocity;
		if (v_observer.magnitude >= 0.9999999f)
			v_observer = v_observer.normalized * (0.9999999f);
		double γ_observer = LorentzFactor(v_observer);

		Vector3 v_object = Velocity;
		if (v_object.magnitude >= 0.9999999f)
			v_object = v_object.normalized * (0.9999999f);

		Vector3 current_relative_velocity = RelVelAdd(v_observer,v_object);
		Matrix4x4 Λ = WignerBoost(v_observer,v_object);
		proper_starting_event = CombineTemporalAndSpacial((float)Proper_Time_Offset, GetSpacialComponents(proper_starting_event));
		Vector4 coordinate_current_event = Λ.inverse * CombineTemporalAndSpacial(-(float)Proper_Time_Offset, GetSpacialComponents(proper_starting_event));
		
		Debug.DrawRay(setZAxis(GetSpacialComponents(coordinate_current_event),GetTemporalComponent(coordinate_current_event) - (float)coordinate_time), -setZAxis(current_relative_velocity,1)*10000, new Color(0.3f, 0.3f, 0.3f));

		float curr_proper_time = (float)Proper_Time;

		for (int i=0; i<Proper_Accelerations.Count; ++i){
			Vector3 a = GetSpacialComponents(Proper_Accelerations[i]);
			Vector3 u_0 = current_relative_velocity / Mathf.Sqrt(1 - current_relative_velocity.sqrMagnitude);
			float proper_duration = GetTemporalComponent(Proper_Accelerations[i]);
			Color col = i%2==0 ? Color.green : Color.cyan;
			if (a.magnitude > 0){
				float coordinate_duration = (Vector3.Dot(a,u_0)*(float)Cosh(a.magnitude*proper_duration) + a.magnitude*Mathf.Sqrt(1+u_0.sqrMagnitude)*(float)Sinh(a.magnitude*proper_duration) - Vector3.Dot(a,u_0))/a.sqrMagnitude;
				int steps = (int)(proper_duration*10);

				for (int j=0; j<steps; j++){
					float T = proper_duration/steps;
					u_0 = current_relative_velocity / Mathf.Sqrt(1 - current_relative_velocity.sqrMagnitude);
					float t = (Vector3.Dot(a,u_0)*(float)Cosh(a.magnitude*T) + a.magnitude*Mathf.Sqrt(1+u_0.sqrMagnitude)*(float)Sinh(a.magnitude*T) - Vector3.Dot(a,u_0))/a.sqrMagnitude;
					Vector4 coordinate_prev_event = coordinate_current_event;
					
					
					Vector3 displacement = (
					                        a*a.magnitude*(Mathf.Sqrt(1+(a*t+u_0).sqrMagnitude)-Mathf.Sqrt(1+u_0.sqrMagnitude)) - 
					                        Vector3.Cross(Vector3.Cross(u_0, a),-a)*
					                        (
					                       	    Mathf.Log(Vector3.Dot(a,u_0) + a.magnitude*Mathf.Sqrt(1+u_0.sqrMagnitude)) - 
					                       	    Mathf.Log(a.sqrMagnitude*t+Vector3.Dot(a,u_0)+a.magnitude*Mathf.Sqrt(1+(a*t+u_0).sqrMagnitude))
					                        )
					                       ) / Mathf.Pow(a.magnitude,3);
					
					coordinate_current_event += CombineTemporalAndSpacial(t, displacement);
					current_relative_velocity = (a*t+u_0)/Mathf.Sqrt(1 + (a*t+u_0).sqrMagnitude);
					
					Debug.DrawLine(	setZAxis(GetSpacialComponents(coordinate_prev_event),GetTemporalComponent(coordinate_prev_event) - (float)coordinate_time), 
									setZAxis(GetSpacialComponents(coordinate_current_event),GetTemporalComponent(coordinate_current_event) - (float)coordinate_time),
									col);
				}
			}else{
				Vector4 coordinate_prev_event = coordinate_current_event;
				float coordinate_duration = proper_duration / Mathf.Sqrt(1f - current_relative_velocity.sqrMagnitude);
				Vector3 displacement = current_relative_velocity * coordinate_duration;
				coordinate_current_event += CombineTemporalAndSpacial(coordinate_duration, displacement);
				Debug.DrawLine(	setZAxis(GetSpacialComponents(coordinate_prev_event),GetTemporalComponent(coordinate_prev_event) - (float)coordinate_time), 
									setZAxis(GetSpacialComponents(coordinate_current_event),GetTemporalComponent(coordinate_current_event) - (float)coordinate_time),
									col);
			}
		}
		Debug.DrawRay(setZAxis(GetSpacialComponents(coordinate_current_event),GetTemporalComponent(coordinate_current_event) - (float)coordinate_time), setZAxis(current_relative_velocity,1)*10000, Color.white);
		for (int i=0; i<60; ++i){
			Vector4 c_event = getCoordinateEventAtProperTime(i);
			Debug.DrawRay(	setZAxis(GetSpacialComponents(c_event),GetTemporalComponent(c_event) - (float)coordinate_time), Vector3.up, Color.white);
		}
	}

	Vector4 getCoordinateEventAtProperTime(float T){
		Vector3 v_observer = -observer_script.velocity;
		if (v_observer.magnitude >= 0.9999999f)
			v_observer = v_observer.normalized * (0.9999999f);
		double γ_observer = LorentzFactor(v_observer);

		Vector3 v_object = Velocity;
		if (v_object.magnitude >= 0.9999999f)
			v_object = v_object.normalized * (0.9999999f);

		Matrix4x4 Λ_starting = WignerBoost(v_observer,v_object);
		Vector4 coordinate_zero_proper_time_event = Λ_starting.inverse * CombineTemporalAndSpacial(-(float)Proper_Time_Offset, GetSpacialComponents(proper_starting_event));;
		Vector4 coordinate_current_event = coordinate_zero_proper_time_event;
		Vector3 Relative_Velocity = RelVelAdd(v_observer, v_object);
		float Proper_Time = GetTemporalComponent(proper_starting_event) - (float)Proper_Time_Offset;
		
		if (T > Proper_Time)
		{
			for (int i=0; i<Proper_Accelerations.Count; ++i)
			{
				Vector3 a = GetSpacialComponents(Proper_Accelerations[i]);
				Vector3 u_0 = Relative_Velocity / Mathf.Sqrt(1 - Relative_Velocity.sqrMagnitude);
				float proper_duration = GetTemporalComponent(Proper_Accelerations[i]);
				if (a.magnitude > 0){
					if (T > Proper_Time+proper_duration){
						float t = (Vector3.Dot(a,u_0)*(float)Cosh(a.magnitude*proper_duration) + a.magnitude*Mathf.Sqrt(1+u_0.sqrMagnitude)*(float)Sinh(a.magnitude*proper_duration) - Vector3.Dot(a,u_0))/a.sqrMagnitude;
						Vector3 displacement = (
						                        a*a.magnitude*(Mathf.Sqrt(1+(a*t+u_0).sqrMagnitude)-Mathf.Sqrt(1+u_0.sqrMagnitude)) - 
						                        Vector3.Cross(Vector3.Cross(u_0, a),-a)*
						                        (
						                       	    Mathf.Log(Vector3.Dot(a,u_0) + a.magnitude*Mathf.Sqrt(1+u_0.sqrMagnitude)) - 
						                       	    Mathf.Log(a.sqrMagnitude*t+Vector3.Dot(a,u_0)+a.magnitude*Mathf.Sqrt(1+(a*t+u_0).sqrMagnitude))
						                        )
						                       ) / Mathf.Pow(a.magnitude,3);
						coordinate_current_event += CombineTemporalAndSpacial(t, displacement);
						Relative_Velocity = (a*t+u_0)/Mathf.Sqrt(1 + (a*t+u_0).sqrMagnitude);
						Proper_Time += proper_duration;
					}else{
						proper_duration = T - (float)Proper_Time;
						float t = (Vector3.Dot(a,u_0)*(float)Cosh(a.magnitude*proper_duration) + a.magnitude*Mathf.Sqrt(1+u_0.sqrMagnitude)*(float)Sinh(a.magnitude*proper_duration) - Vector3.Dot(a,u_0))/a.sqrMagnitude;
						Vector3 displacement = (
						                        a*a.magnitude*(Mathf.Sqrt(1+(a*t+u_0).sqrMagnitude)-Mathf.Sqrt(1+u_0.sqrMagnitude)) - 
						                        Vector3.Cross(Vector3.Cross(u_0, a),-a)*
						                        (
						                       	    Mathf.Log(Vector3.Dot(a,u_0) + a.magnitude*Mathf.Sqrt(1+u_0.sqrMagnitude)) - 
						                       	    Mathf.Log(a.sqrMagnitude*t+Vector3.Dot(a,u_0)+a.magnitude*Mathf.Sqrt(1+(a*t+u_0).sqrMagnitude))
						                        )
						                       ) / Mathf.Pow(a.magnitude,3);
						coordinate_current_event += CombineTemporalAndSpacial(t, displacement);
						Relative_Velocity = (a*t+u_0)/Mathf.Sqrt(1 + (a*t+u_0).sqrMagnitude);
						Proper_Time += proper_duration;
						break;
					}
				}else{
					if (T > Proper_Time+proper_duration){
						float coordinate_duration = proper_duration / Mathf.Sqrt(1f - Relative_Velocity.sqrMagnitude);
						Vector3 displacement = Relative_Velocity * coordinate_duration;
						coordinate_current_event += CombineTemporalAndSpacial(coordinate_duration, displacement);
						Proper_Time += proper_duration;
					}else{
						proper_duration = T - (float)Proper_Time;
						float coordinate_duration = proper_duration / Mathf.Sqrt(1f - Relative_Velocity.sqrMagnitude);
						Vector3 displacement = Relative_Velocity * coordinate_duration;
						coordinate_current_event += CombineTemporalAndSpacial(coordinate_duration, displacement);
						Proper_Time += proper_duration;
						break;
					}
				}
			}
		}
		float γ = 1f/Mathf.Sqrt(1f - Relative_Velocity.sqrMagnitude);

		float remaining_proper_time = T - (float)Proper_Time;
		float remaining_coordinate_time = remaining_proper_time * γ;
		coordinate_current_event += remaining_coordinate_time * CombineTemporalAndSpacial(1, Relative_Velocity);
		return coordinate_current_event;
	}

	double LorentzFactor(Vector3 v)
	{
		return 1.0/System.Math.Sqrt(1-v.sqrMagnitude);
	}

	Vector4 CombineTemporalAndSpacial(float t, Vector3 p)
	{
		return new Vector4(t, p.x, p.y, p.z);
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

	Vector3 RelVelAdd(Vector3 u, Vector3 v)
	{
		float γu = 1f/Mathf.Sqrt(1-u.sqrMagnitude);
		return 1f/(1 + Vector3.Dot(u,v)) * ((1 + γu/(1+γu)*Vector3.Dot(u,v))*u + (1f/γu)*v);
	}

	Vector3 ProperVelAdd(Vector3 u, Vector3 v)
	{
		float βu = 1f/Mathf.Sqrt(1+u.sqrMagnitude);
		float βv = 1f/Mathf.Sqrt(1+v.sqrMagnitude);
		return u + v + (βu/(1 + βu)*Vector3.Dot(u, v) + (1 - βv)/βv)*u;
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

	Matrix4x4 InverseRotationMatrix4(Vector3 u, Vector3 v)
	{
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

	float fourSqrMagnitude(Vector4 u)
	{
		return -Mathf.Pow(u.x,2) + Mathf.Pow(u.y,2) + Mathf.Pow(u.z,2) + Mathf.Pow(u.w,2);
	}

	float fourScalarProduct(Vector4 u, Vector4 v)
	{
		return -(u.x*v.x) + (u.y*v.y) + (u.z*v.z) + (u.w*v.w);
	}
}
	