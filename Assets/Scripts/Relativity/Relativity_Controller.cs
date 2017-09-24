using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relativity_Controller : MonoBehaviour {
	public GameObject Observer;
	public Vector3 Velocity; //Velocity relative to coordinate frame
	public bool DrawWorldLine = true;
	public float ProperTimeOffset;
	public List<Vector3> ProperAccelerations;
	public List<float> AccelerationDurations;
	public List<Vector3> AccelerationOffsets;
	public Vector3 Current_Velocity;
	public float Proper_Time;
	public Vector4 Current_Event;

	private Relativity_Observer observerScript;
	private Vector3 observerVelocity; //Observer's velocity relative to coordinate frame
	private List<Material> mats;
	private MeshFilter[] MFs;

	void Start () {
		observerScript = Observer.GetComponent<Relativity_Observer>();
		Renderer[] renderers = GetComponentsInChildren<Renderer>();
		mats = new List<Material>();
		foreach (Renderer R in renderers){
			Material[] childMaterials = R.materials;
			foreach (Material mat in childMaterials){
				if (mat.shader == Shader.Find("Relativity/Relativity_Shader"))
					mats.Add(mat);
			}
		}
		MFs = GetComponentsInChildren<MeshFilter>();
	}
	
	void Update () {
		observerVelocity = observerScript.velocity;
		float observer_time = observerScript.CoordinateTime;
		
		for (int i=AccelerationOffsets.Count; i<ProperAccelerations.Count; ++i){
			if (AccelerationOffsets.Count == 0)
				AccelerationOffsets.Add(Vector4.zero);
			else
				AccelerationOffsets.Add(AccelerationOffsets[i-1]);
		}
		if (mats.Count != 0){
			if (ProperAccelerations.Count > 0 && AccelerationOffsets.Count > 0){
				Vector4[] shader_ProperAccelerations = new Vector4[ProperAccelerations.Count];
				Vector4[] shader_AccelerationOffsets = new Vector4[ProperAccelerations.Count];
				for (int i=0; i<ProperAccelerations.Count; ++i){
					shader_ProperAccelerations[i] = (Vector4)ProperAccelerations[i];
					shader_AccelerationOffsets[i] = (Vector4)AccelerationOffsets[i];
				}
				foreach(Material mat in mats){
					mat.SetVectorArray("_accelerations", shader_ProperAccelerations);
					mat.SetFloatArray("_durations", AccelerationDurations.ToArray());
					mat.SetVectorArray("_accel_positions", shader_AccelerationOffsets);
				}
			}
			foreach(Material mat in mats){
				mat.SetVector("_Velocity", Velocity);
				mat.SetFloat("_Proper_Time_Offset", ProperTimeOffset);
			}
		}
		
			

		Current_Event = GetState(observer_time); //Object's current event in observer's frame
		foreach (MeshFilter MF in MFs){
			Bounds newBounds = MF.sharedMesh.bounds;
			newBounds.center = transform.InverseTransformPoint(GetSpatialComponent(Current_Event));
			newBounds.extents = new Vector3(500,500,500);
			MF.sharedMesh.bounds = newBounds;
		}
		if (DrawWorldLine)
			DrawPath(observer_time);
		/*
		Matrix4x4 coordinate_observer_boost = LorentzBoost(observerVelocity);
		Color[] colors = new Color[3]{Color.blue, Color.red, Color.green};
		int sep = 15;
		int[] seps = new int[3]{0, 1, -1};
		for (int i=0; i<3; ++i){
			for (int j=0; j<2; ++j){
				for (int k=0; k<3; ++k){
					for (int m=0; m<3; ++m){
						Vector4 dir = new Vector4();
						dir[i] = 1;
						Vector4 pt = new Vector4();
						pt[(i+1)%3] = seps[k]*sep;
						pt[(i+2)%3] = seps[m]*sep;
						Debug.DrawRay(BoostToMinkowski(pt, coordinate_observer_boost), BoostToMinkowski(dir, coordinate_observer_boost)*100 * (j==0?1:-1), colors[i]);
					}
				}
			}
		}
		*/
	}

	Vector4 GetState(float observer_time){
		Matrix4x4 object_coordinate_boost = LorentzBoost(-Velocity); //Boost from object frame to coordinate frame
		Matrix4x4 coordinate_observer_boost = LorentzBoost(observerVelocity); //Boost from coordinate frame to observer frame
		Vector4 starting_event_object = CombineTemporalAndSpatial(-ProperTimeOffset, transform.position); //Object will always be at (T,0,0,0) in its own frame.
		Vector4 current_event_coordinate = object_coordinate_boost * starting_event_object;
		Vector4 current_event_observer = coordinate_observer_boost * current_event_coordinate;
		Vector3 current_object_velocity = Velocity;
		Vector3 proper_object_velocity = current_object_velocity / α(current_object_velocity);
		Vector3 observer_object_velocity = AddVelocity(-observerVelocity, current_object_velocity);
		Proper_Time = 0;
		for (int i=0; i<ProperAccelerations.Count; ++i){
			if (GetTemporalComponent(current_event_observer) < observer_time){
				Vector3 proper_acceleration = ProperAccelerations[i];
				float proper_duration = AccelerationDurations[i];
				float a = proper_acceleration.magnitude;
				if (a > 0){
					Vector3 offset = AccelerationOffsets[i];
					float L = Vector3.Dot(offset, proper_acceleration)/a;
					if (L <= 1f/a){
						float b = 1f/(1f - a*L);
						proper_acceleration *= b;
						proper_duration /= b;
					}else{
						proper_acceleration = new Vector3(0,0,0);
						proper_duration = 0;
					}
					float coordinate_duration = getDuration(proper_acceleration, proper_object_velocity, proper_duration);
					Vector3 coordinate_displacement = getDisplacement(proper_acceleration, proper_object_velocity, coordinate_duration);
					Vector4 next_event_coordinate = current_event_coordinate + CombineTemporalAndSpatial(coordinate_duration, coordinate_displacement);
					Vector4 next_event_observer = coordinate_observer_boost * next_event_coordinate;
					if (GetTemporalComponent(next_event_observer) > observer_time){
						coordinate_duration = 0;
						float observer_duration = 0;
						for (int j=0; j<100; ++j){
							float prev_duration = coordinate_duration;
							float diff = observer_time - GetTemporalComponent(current_event_observer) - observer_duration;
							coordinate_duration += diff / 2;
							float t = coordinate_duration;
							observer_duration = getObserverDuration(proper_acceleration, proper_object_velocity, observerVelocity, coordinate_duration);
						}
						coordinate_displacement = getDisplacement(proper_acceleration, proper_object_velocity, coordinate_duration);
						next_event_coordinate = current_event_coordinate + CombineTemporalAndSpatial(coordinate_duration, coordinate_displacement);
						next_event_observer = coordinate_observer_boost * next_event_coordinate;
					}
					Proper_Time += getProperDuration(proper_acceleration, proper_object_velocity, coordinate_duration);
					proper_object_velocity += proper_acceleration*coordinate_duration;
					current_object_velocity = proper_object_velocity / Mathf.Sqrt(1f + proper_object_velocity.sqrMagnitude);
					current_event_coordinate += CombineTemporalAndSpatial(coordinate_duration, coordinate_displacement);
					current_event_observer = coordinate_observer_boost * current_event_coordinate;
					observer_object_velocity = AddVelocity(-observerVelocity, current_object_velocity);
				}else{
					float coordinate_duration = proper_duration / α(current_object_velocity);
					Vector3 coordinate_displacement = current_object_velocity * coordinate_duration;
					current_event_coordinate += CombineTemporalAndSpatial(coordinate_duration, coordinate_displacement);
					current_event_observer = coordinate_observer_boost * current_event_coordinate;
					observer_object_velocity = AddVelocity(-observerVelocity, current_object_velocity);
					Proper_Time += proper_duration;
				}
			}else{
				break;
			}
		}
		observer_object_velocity = AddVelocity(-observerVelocity, current_object_velocity);
		Current_Velocity = observer_object_velocity;
		Proper_Time += (observer_time - GetTemporalComponent(current_event_observer))*α(observer_object_velocity);
		current_event_observer += (observer_time - GetTemporalComponent(current_event_observer))*CombineTemporalAndSpatial(1, observer_object_velocity);
		return current_event_observer;
	}


	void DrawPath(float observer_time){
		Matrix4x4 object_coordinate_boost = LorentzBoost(-Velocity); //Boost from object frame to coordinate frame
		Matrix4x4 coordinate_observer_boost = LorentzBoost(observerVelocity); //Boost from coordinate frame to observer frame
		Vector4 starting_event_object = CombineTemporalAndSpatial(-ProperTimeOffset, transform.position); //Object will always be at (T,0,0,0) in its own frame.
		Vector4 current_event_coordinate = object_coordinate_boost * starting_event_object;
		Vector4 current_event_observer = coordinate_observer_boost * current_event_coordinate;
		Vector3 current_object_velocity = Velocity;
		Vector3 proper_object_velocity = current_object_velocity / α(current_object_velocity);
		Vector3 observer_object_velocity = AddVelocity(-observerVelocity, current_object_velocity);
		Debug.DrawRay(AddToZAxis(GetSpatialComponent(current_event_observer), GetTemporalComponent(current_event_observer) - observer_time), -AddToZAxis(observer_object_velocity, 1)*10000, Color.cyan);
		float proper_time = 0;
		for (int i=0; i<ProperAccelerations.Count; ++i){
			Vector3 proper_acceleration = ProperAccelerations[i];
			float proper_duration = AccelerationDurations[i];
			float a = proper_acceleration.magnitude;
			if (a > 0){
				Vector3 offset = AccelerationOffsets[i];
				float L = Vector3.Dot(offset, proper_acceleration)/a;
				if (L <= 1f/a){
					float b = 1f/(1f - a*L);
					proper_acceleration *= b;
					proper_duration /= b;
				}else{
					proper_acceleration = new Vector3(0,0,0);
					proper_duration = 0;
				}
				float coordinate_duration = getDuration(proper_acceleration, proper_object_velocity, proper_duration);
				int count = 100;
				float increment = coordinate_duration/count;
				for (int j=0; j<count; ++j){
					coordinate_duration = increment;
					Vector3 coordinate_displacement = getDisplacement(proper_acceleration, proper_object_velocity, coordinate_duration);
					
					proper_time += getProperDuration(proper_acceleration, proper_object_velocity, coordinate_duration);
					proper_object_velocity += proper_acceleration*coordinate_duration;
					current_object_velocity = proper_object_velocity / Mathf.Sqrt(1f + proper_object_velocity.sqrMagnitude);
					current_event_coordinate += CombineTemporalAndSpatial(coordinate_duration, coordinate_displacement);
					Vector4 next_event_observer = coordinate_observer_boost * current_event_coordinate;
					observer_object_velocity = AddVelocity(current_object_velocity, -observerVelocity);
					Debug.DrawLine( AddToZAxis(GetSpatialComponent(current_event_observer), GetTemporalComponent(current_event_observer) - observer_time),
					                AddToZAxis(GetSpatialComponent(next_event_observer), GetTemporalComponent(next_event_observer) - observer_time),
					                new Color(1,0.5f,0));
					current_event_observer = next_event_observer;
				}
			}else{
				float coordinate_duration = proper_duration / α(current_object_velocity);
				Vector3 coordinate_displacement = current_object_velocity * coordinate_duration;
				current_event_coordinate += CombineTemporalAndSpatial(coordinate_duration, coordinate_displacement);
				Vector4 next_event_observer = coordinate_observer_boost * current_event_coordinate;
				observer_object_velocity = AddVelocity(current_object_velocity, -observerVelocity);
				Debug.DrawLine( AddToZAxis(GetSpatialComponent(current_event_observer), GetTemporalComponent(current_event_observer) - observer_time),
				                AddToZAxis(GetSpatialComponent(next_event_observer), GetTemporalComponent(next_event_observer) - observer_time),
				                Color.white);

				current_event_observer = next_event_observer;
				proper_time += proper_duration;
			}
		}
		observer_object_velocity = AddVelocity(-observerVelocity, current_object_velocity);
		proper_time += (observer_time - GetTemporalComponent(current_event_observer))*α(observer_object_velocity);
		Debug.DrawRay(AddToZAxis(GetSpatialComponent(current_event_observer), GetTemporalComponent(current_event_observer) - observer_time), AddToZAxis(observer_object_velocity,1)*10000, Color.yellow);
	}

	Vector3 AddVelocity(Vector3 v, Vector3 u){
		//Einstein Velocity Addition
		if (v.sqrMagnitude == 0)
			return u;
		if (u.sqrMagnitude == 0)
			return v;
		return 1f/(1 + Vector3.Dot(v, u))*(u*α(v) + v + (1 - α(v))*Vector3.Dot(v, u)/v.sqrMagnitude*v);
	}

	float γ(Vector3 v){
		//Lorentz Factor
		return 1f/Mathf.Sqrt(1 - v.sqrMagnitude);
	}

	float α(Vector3 v){ 
		//Reciprocal Lorentz Factor
		return Mathf.Sqrt(1 - v.sqrMagnitude);
	}

	float Sinh(float x)
	{
		return (Mathf.Pow(Mathf.Exp(1), x) - Mathf.Pow(Mathf.Exp(1), -x))/2;
	}

	float Cosh(float x)
	{
		return (Mathf.Pow(Mathf.Exp(1), x) + Mathf.Pow(Mathf.Exp(1), -x))/2;
	}

	float Tanh(float x)
	{
		return Sinh(x)/Cosh(x);
	}

	float ATanh(float x)
	{
		return (Mathf.Log(1 + x) - Mathf.Log(1 - x))/2;
	}

	float ACosh(float x)
	{
		return Mathf.Log(x + Mathf.Sqrt(Mathf.Pow(x,2) - 1));
	}

	float ASinh(float x)
	{
		return Mathf.Log(x + Mathf.Sqrt(1 + Mathf.Pow(x,2)));
	}

	Matrix4x4 LorentzBoost(Vector3 v){
		//Computes the Lorentz Boost matrix for a given 3-dimensional velocity
		float βSqr = v.sqrMagnitude;
		if (βSqr == 0f)
		{
			return Matrix4x4.identity;
		}
		float βx = v.x;
		float βy = v.y;
		float βz = v.z;

		Matrix4x4 boost = new Matrix4x4();
		boost.SetRow(0,new Vector4(     γ(v),  -γ(v)*βx,                    -γ(v)*βy,                    -γ(v)*βz                    ));
		boost.SetRow(1,new Vector4( -βx*γ(v),  (γ(v)-1)*(βx*βx)/(βSqr) + 1, (γ(v)-1)*(βx*βy)/(βSqr),     (γ(v)-1)*(βx*βz)/(βSqr)     ));
		boost.SetRow(2,new Vector4( -βy*γ(v),  (γ(v)-1)*(βy*βx)/(βSqr),     (γ(v)-1)*(βy*βy)/(βSqr) + 1, (γ(v)-1)*(βy*βz)/(βSqr)     ));
		boost.SetRow(3,new Vector4( -βz*γ(v),  (γ(v)-1)*(βz*βx)/(βSqr),     (γ(v)-1)*(βz*βy)/(βSqr),     (γ(v)-1)*(βz*βz)/(βSqr) + 1 ));

		return boost;

	}

	Vector4 CombineTemporalAndSpatial(float t, Vector3 p){
		return new Vector4(t, p.x, p.y, p.z);
	}

	float GetTemporalComponent(Vector4 v){
		return v.x;
	}

	Vector3 GetSpatialComponent(Vector4 v){
		return new Vector3(v.y, v.z, v.w);
	}

	Vector3 AddToZAxis(Vector3 v, float z){
		//Add new z value to vector v's z-axis
		v.z += z;
		return v;
	}

	Vector3 BoostToMinkowski(Vector4 pt, Matrix4x4 boost){
		//Applies a Lorentz boost to a (t,x,y,z) point, then converts it into (x,y,t) coordinates for Minkowski diagram rendering
		Vector4 new_pt = boost*pt;
		return new Vector3(new_pt.y, new_pt.z, new_pt.x);
	}

	Vector3 getDisplacement(Vector3 a, Vector3 u_0, float t){
		//Given proper acceleration a, initial proper velocity u_0, and coordinate duration t, solve for displacement in the coordinate frame.
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
		//Given proper acceleration a, initial proper velocity u_0, and proper duration T, solve for coordinate duration.
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
		//Given proper acceleration a, initial proper velocity u_0, and coordinate duration t, solve for proper duration.
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

	float getObserverDuration(Vector3 a, Vector3 u_0, Vector3 v, float t){
		//Given proper acceleration a, initial proper velocity u_0, observer velocity v, and coordinate duration t, solve for observer duration.
		return
		(
			t
			*
			Mathf.Pow(
				a.sqrMagnitude,
				3f/2
			)
			+
			Mathf.Sqrt(
				a.sqrMagnitude
			)
			*
			Vector3.Dot(
				a,
				v
			)
			*
			Mathf.Sqrt(
				1
				+
				u_0.sqrMagnitude
			)
			-
			Mathf.Sqrt(
				a.sqrMagnitude
			)
			*
			Vector3.Dot(
				a,
				v
			)
			*
			Mathf.Sqrt(
				1
				+
				Mathf.Pow(
					t,
					2
				)
				*
				a.sqrMagnitude
				+
				2
				*
				t
				*
				Vector3.Dot(
					a,
					u_0
				)
				+
				u_0.sqrMagnitude
			)
			-
			(
				Vector3.Dot(
					a,
					u_0
				)
				*
				Vector3.Dot(
					a,
					v
				)
				-
				a.sqrMagnitude
				*
				Vector3.Dot(
					u_0,
					v
				)
			)
			*
			(
				Mathf.Log(
					Vector3.Dot(
						a,
						u_0
					)
					+
					Mathf.Sqrt(
						a.sqrMagnitude
					)
					*
					Mathf.Sqrt(
						1
						+
						u_0.sqrMagnitude
					)
				)
				-
				Mathf.Log(
					t
					*
					a.sqrMagnitude
					+
					Vector3.Dot(
						a,
						u_0
					)
					+
					Mathf.Sqrt(
						a.sqrMagnitude
					)
					*
					Mathf.Sqrt(
						1
						+
						Mathf.Pow(
							t,
							2
						)
						*
						a.sqrMagnitude
						+
						2
						*
						t
						*
						Vector3.Dot(
							a,
							u_0
						)
						+
						u_0.sqrMagnitude
					)
				)
			)
		)
		/
		(
			Mathf.Pow(
				a.sqrMagnitude,
				3f/2
			)
			*
			Mathf.Sqrt(
				1
				-
				v.sqrMagnitude
			)
		);
	}
}
