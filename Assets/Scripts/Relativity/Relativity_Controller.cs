using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relativity_Controller : MonoBehaviour {
	public GameObject Observer;
	public Vector3 Velocity; //Velocity relative to coordinate frame
	public float ProperTimeOffset;
	public List<Vector3> ProperAccelerations;
	public List<float> AccelerationDurations;
	public List<Vector3> AccelerationOffsets;
	public bool DrawMinkowski;
	public bool DrawWorldLine = true;
	public uint AccelerationCurveDetail = 10;
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
	
	void LateUpdate () {
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
		
			

		Current_Event = get_state(observer_time); //Object's current event in observer's frame
		foreach (MeshFilter MF in MFs){
			Bounds newBounds = MF.sharedMesh.bounds;
			newBounds.center = transform.InverseTransformPoint(get_spatial_component(Current_Event));
			newBounds.extents = new Vector3(20,20,20);
			MF.sharedMesh.bounds = newBounds;
		}
		if (DrawWorldLine)
			draw_path(observer_time);
		if (DrawMinkowski)
		{
			Matrix4x4 coordinate_observer_boost = lorentz_boost(observerVelocity);
			Color[] colors = new Color[3]{Color.blue, Color.red, Color.green};
			float[] alpha = new float[3]{0.15f, 1f, 0.1f};
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
							Debug.DrawRay(boost_to_minkowski(pt, coordinate_observer_boost), boost_to_minkowski(dir, coordinate_observer_boost)*100 * (j==0?1:-1), new Color(colors[i].r, colors[i].g, colors[i].b, k==0&&m==0?alpha[1]:alpha[0]));
						}
					}
				}
			}
		}
	}

	Vector4 get_state(float observer_time){
		Vector3 coordinate_to_object_velocity = Velocity;
		Vector3 observer_to_coordinate_velocity = -observerVelocity;
		Matrix4x4 object_to_coordinate_boost = lorentz_boost(-coordinate_to_object_velocity);
		Matrix4x4 coordinate_to_observer_boost = lorentz_boost(-observer_to_coordinate_velocity); //Boost from coordinate frame to observer frame
		Vector4 current_event_object = combine_temporal_and_spatial(-ProperTimeOffset, transform.position);
		Vector4 current_event_coordinate = object_to_coordinate_boost * current_event_object - combine_temporal_and_spatial(0, Observer.transform.position);
		Vector4 current_event_observer = coordinate_to_observer_boost * current_event_coordinate;
		Vector3 observer_to_object_velocity = add_velocity(observer_to_coordinate_velocity, coordinate_to_object_velocity);
		
		List<Vector3> velocities = new List<Vector3>();
		int velocities_index = 0;
		velocities.Add(velocity_to_proper(coordinate_to_object_velocity));
		velocities_index++;
		Proper_Time = 0;
		for (int i=0; i<ProperAccelerations.Count; ++i){
			if (get_temporal_component(current_event_observer) < observer_time){
				Vector3 proper_acceleration = ProperAccelerations[i];
				float proper_duration = AccelerationDurations[i];
				Vector3 offset = AccelerationOffsets[i];
				float a = proper_acceleration.magnitude;
				float L = Vector3.Dot(offset, proper_acceleration)/a;
				if (L <= 1f/a){
					float b = 1f/(1f - a*L);
					proper_acceleration *= b;
					proper_duration /= b;
				}else{
					proper_acceleration = new Vector3(0,0,0);
					proper_duration = 0;
				}
				if (proper_acceleration.magnitude > 0){
					float MCRF_duration = sinh(proper_acceleration.magnitude * proper_duration)/proper_acceleration.magnitude;
					Vector4 next_event_object = combine_temporal_and_spatial(MCRF_duration, get_displacement(proper_acceleration, MCRF_duration));
					Vector4 next_event_coordinate = current_event_coordinate + object_to_coordinate_boost * next_event_object;
					Vector4 next_event_observer = coordinate_to_observer_boost * next_event_coordinate;
					if (get_temporal_component(next_event_observer) > observer_time){
						MCRF_duration = get_MCRF_time(proper_acceleration, -observer_to_coordinate_velocity, object_to_coordinate_boost, current_event_coordinate, observer_time);

						next_event_observer = current_event_observer;
						current_event_object = combine_temporal_and_spatial(MCRF_duration, get_displacement(proper_acceleration, MCRF_duration));
						current_event_coordinate = current_event_coordinate + object_to_coordinate_boost * current_event_object;
						current_event_observer = coordinate_to_observer_boost * current_event_coordinate;
						Vector3 added_velocity = proper_to_velocity(proper_acceleration*MCRF_duration);
						Proper_Time += asinh(proper_acceleration.magnitude * MCRF_duration)/proper_acceleration.magnitude;
						velocities.Add(proper_acceleration*MCRF_duration);
						velocities_index++;
						object_to_coordinate_boost = object_to_coordinate_boost*lorentz_boost(-added_velocity);
						break;
					}else{
						current_event_object = next_event_object;
						current_event_coordinate = next_event_coordinate;
						current_event_observer = next_event_observer;
						Vector3 added_velocity = proper_to_velocity(proper_acceleration*MCRF_duration);
						Proper_Time += asinh(proper_acceleration.magnitude * MCRF_duration)/proper_acceleration.magnitude;
						velocities.Add(proper_acceleration*MCRF_duration);
						velocities_index++;
						object_to_coordinate_boost = object_to_coordinate_boost*lorentz_boost(-added_velocity);
					}
				}else{
					
				}
			}else{
				break;
			}
		}
		Vector3 coordinate_to_object_proper_velocity = velocities[--velocities_index];
		velocities_index--;
		for (int i=velocities_index; i>=0; i--){
			coordinate_to_object_proper_velocity = add_proper_velocity(velocities[i], coordinate_to_object_proper_velocity);
		}
		coordinate_to_object_velocity = proper_to_velocity(coordinate_to_object_proper_velocity);
		observer_to_object_velocity = add_velocity(observer_to_coordinate_velocity, coordinate_to_object_velocity);
		Proper_Time += (observer_time - get_temporal_component(current_event_observer))*α(observer_to_object_velocity);
		current_event_observer += (observer_time - get_temporal_component(current_event_observer))*combine_temporal_and_spatial(1, observer_to_object_velocity);
		Vector4 relative_event_observer = current_event_observer + combine_temporal_and_spatial(0, Observer.transform.position);
		Debug.DrawRay(add_to_Z_axis(get_spatial_component(relative_event_observer), get_temporal_component(relative_event_observer)-observer_time), observer_to_object_velocity, Color.white);
		Current_Velocity = observer_to_object_velocity;
		return current_event_observer + combine_temporal_and_spatial(0, Observer.transform.position);
	}

	void draw_path(float observer_time){
		Vector3 coordinate_to_object_velocity = Velocity;
		Vector3 observer_to_coordinate_velocity = -observerVelocity;
		Matrix4x4 object_to_coordinate_boost = lorentz_boost(-coordinate_to_object_velocity);
		Matrix4x4 coordinate_to_observer_boost = lorentz_boost(-observer_to_coordinate_velocity); //Boost from coordinate frame to observer frame
		Vector4 current_event_object = combine_temporal_and_spatial(-ProperTimeOffset, transform.position);
		Vector4 current_event_coordinate = object_to_coordinate_boost * current_event_object - combine_temporal_and_spatial(0, Observer.transform.position);
		Vector4 current_event_observer = coordinate_to_observer_boost * current_event_coordinate;
		Vector3 observer_to_object_velocity = add_velocity(observer_to_coordinate_velocity, coordinate_to_object_velocity);
		Vector4 relative_event_observer = current_event_observer + combine_temporal_and_spatial(0, Observer.transform.position);
		Debug.DrawRay(add_to_Z_axis(get_spatial_component(relative_event_observer), get_temporal_component(relative_event_observer) - observer_time), -add_to_Z_axis(observer_to_object_velocity, 1)*10000, Color.cyan);
		List<Vector3> velocities = new List<Vector3>();
		int velocities_index = 0;
		velocities.Add(velocity_to_proper(coordinate_to_object_velocity));
		velocities_index++;
		for (int i=0; i<ProperAccelerations.Count; ++i){
			Vector3 proper_acceleration = ProperAccelerations[i];
			float proper_duration = AccelerationDurations[i];
			Vector3 offset = AccelerationOffsets[i];
			float a = proper_acceleration.magnitude;
			float L = Vector3.Dot(offset, proper_acceleration)/a;
			if (L <= 1f/a){
				float b = 1f/(1f - a*L);
				proper_acceleration *= b;
				proper_duration /= b;
			}else{
				proper_acceleration = new Vector3(0,0,0);
				proper_duration = 0;
			}
			if (proper_acceleration.magnitude > 0){
				int count = 1 + (int)AccelerationCurveDetail;
				float increment = proper_duration/count;
				for (int j=0; j<count; ++j){
					float MCRF_time_increment = sinh(proper_acceleration.magnitude * increment)/proper_acceleration.magnitude;
					Vector4 next_event_object = combine_temporal_and_spatial(MCRF_time_increment, get_displacement(proper_acceleration, MCRF_time_increment));
					Vector4 next_event_coordinate = current_event_coordinate + object_to_coordinate_boost * next_event_object;
					Vector4 next_event_observer = coordinate_to_observer_boost * next_event_coordinate;
					relative_event_observer = current_event_observer + combine_temporal_and_spatial(0, Observer.transform.position);
					Vector4 relative_next_event_observer = next_event_observer + combine_temporal_and_spatial(0, Observer.transform.position);
					Debug.DrawLine( add_to_Z_axis(get_spatial_component(relative_event_observer), get_temporal_component(relative_event_observer) - observer_time),
					                add_to_Z_axis(get_spatial_component(relative_next_event_observer), get_temporal_component(relative_next_event_observer) - observer_time),
					                i%2==0?new Color(1,0.5f,0):Color.green);
					current_event_object = next_event_object;
					current_event_coordinate = next_event_coordinate;
					current_event_observer = next_event_observer;
					Vector3 added_velocity = proper_to_velocity(proper_acceleration*MCRF_time_increment);
					Proper_Time += asinh(proper_acceleration.magnitude * MCRF_time_increment)/proper_acceleration.magnitude;
					velocities.Add(proper_acceleration*MCRF_time_increment);
					velocities_index++;
					object_to_coordinate_boost = object_to_coordinate_boost*lorentz_boost(-added_velocity);
				}
			}
		}
		Vector3 coordinate_to_object_proper_velocity = velocities[--velocities_index];
		velocities_index--;
		for (int i=velocities_index; i>=0; i--){
			coordinate_to_object_proper_velocity = add_proper_velocity(velocities[i], coordinate_to_object_proper_velocity);
		}
		coordinate_to_object_velocity = proper_to_velocity(coordinate_to_object_proper_velocity);
		observer_to_object_velocity = add_velocity(observer_to_coordinate_velocity, coordinate_to_object_velocity);
		relative_event_observer = current_event_observer + combine_temporal_and_spatial(0, Observer.transform.position);
		Debug.DrawRay(add_to_Z_axis(get_spatial_component(relative_event_observer), get_temporal_component(relative_event_observer) - observer_time), add_to_Z_axis(observer_to_object_velocity, 1)*10000, Color.cyan);
	}

	Vector3 velocity_to_proper(Vector3 v){
		return v / Mathf.Sqrt(1f - v.sqrMagnitude);
	}

	Vector3 proper_to_velocity(Vector3 v){
		return v / Mathf.Sqrt(1f + v.sqrMagnitude);
	}

	Vector3 add_velocity(Vector3 v, Vector3 u){
		//Einstein Velocity Addition
		if (v.sqrMagnitude == 0)
			return u;
		if (u.sqrMagnitude == 0)
			return v;
		return 1f/(1+Vector3.Dot(u, v))*(v + u*α(v) + γ(v)/(1f + γ(v))*Vector3.Dot(u, v)*v);
	}

	Vector3 add_proper_velocity(Vector3 v, Vector3 u){
		float Bu = 1/Mathf.Sqrt(1f + u.sqrMagnitude);
		float Bv = 1/Mathf.Sqrt(1f + v.sqrMagnitude);
		return v+u+(Bv/(1+Bv)*Vector3.Dot(v,u) + (1-Bu)/Bu)*v;
	}

	float γ(Vector3 v){
		//Lorentz Factor
		return 1f/Mathf.Sqrt(1 - v.sqrMagnitude);
	}

	float α(Vector3 v){ 
		//Reciprocal Lorentz Factor
		return Mathf.Sqrt(1 - v.sqrMagnitude);
	}

	float sinh(float x)
	{
		return (Mathf.Pow(Mathf.Exp(1), x) - Mathf.Pow(Mathf.Exp(1), -x))/2;
	}

	float cosh(float x)
	{
		return (Mathf.Pow(Mathf.Exp(1), x) + Mathf.Pow(Mathf.Exp(1), -x))/2;
	}

	float tanh(float x)
	{
		return sinh(x)/cosh(x);
	}

	float atanh(float x)
	{
		return (Mathf.Log(1 + x) - Mathf.Log(1 - x))/2;
	}

	float acosh(float x)
	{
		return Mathf.Log(x + Mathf.Sqrt(Mathf.Pow(x,2) - 1));
	}

	float asinh(float x)
	{
		return Mathf.Log(x + Mathf.Sqrt(1 + Mathf.Pow(x,2)));
	}

	Matrix4x4 lorentz_boost(Vector3 v){
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

	Vector4 combine_temporal_and_spatial(float t, Vector3 p){
		return new Vector4(t, p.x, p.y, p.z);
	}

	float get_temporal_component(Vector4 v){
		return v.x;
	}

	Vector3 get_spatial_component(Vector4 v){
		return new Vector3(v.y, v.z, v.w);
	}

	Vector3 add_to_Z_axis(Vector3 v, float z){
		//Add new z value to vector v's z-axis
		v.z += z;
		return v;
	}

	Vector3 boost_to_minkowski(Vector4 pt, Matrix4x4 boost){
		//Applies a Lorentz boost to a (t,x,y,z) point, then converts it into (x,y,t) coordinates for Minkowski diagram rendering
		Vector4 new_pt = boost*pt;
		return new Vector3(new_pt.y, new_pt.z, new_pt.x);
	}

	Vector3 get_displacement(Vector3 a, float T){
		return
		(
			a
			*
			(
				Mathf.Sqrt(
					1
					+
					Mathf.Pow(T, 2)
					*
					a.sqrMagnitude
				)
				-
				1
			)
		)
		/
		(
			a.sqrMagnitude
		);
	}

	float get_observer_time(Vector3 a, Vector3 cV, Matrix4x4 object_to_coordinate_boost, Vector4 current_event_coordinate, float MCRFTime){
		float ax = a.x;
		float ay = a.y;
		float az = a.z;
		float currCoordX = get_spatial_component(current_event_coordinate).x;
		float currCoordY = get_spatial_component(current_event_coordinate).y;
		float currCoordZ = get_spatial_component(current_event_coordinate).z;
		float currCoordT = get_temporal_component(current_event_coordinate);
		float cVx = cV.x;
		float cVy = cV.y;
		float cVz = cV.z;
		float B11 = object_to_coordinate_boost[0,0];
		float B12 = object_to_coordinate_boost[0,1];
		float B13 = object_to_coordinate_boost[0,2];
		float B14 = object_to_coordinate_boost[0,3];
		float B21 = object_to_coordinate_boost[1,0];
		float B22 = object_to_coordinate_boost[1,1];
		float B23 = object_to_coordinate_boost[1,2];
		float B24 = object_to_coordinate_boost[1,3];
		float B31 = object_to_coordinate_boost[2,0];
		float B32 = object_to_coordinate_boost[2,1];
		float B33 = object_to_coordinate_boost[2,2];
		float B34 = object_to_coordinate_boost[2,3];
		float B41 = object_to_coordinate_boost[3,0];
		float B42 = object_to_coordinate_boost[3,1];
		float B43 = object_to_coordinate_boost[3,2];
		float B44 = object_to_coordinate_boost[3,3];
		
		return
		(1/((Mathf.Pow(ax, 2) + Mathf.Pow(ay, 2) + Mathf.Pow(az, 2))*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))))*((-az)*B14 + Mathf.Pow(ax, 2)*(currCoordT - currCoordX*cVx - currCoordY*cVy - currCoordZ*cVz + B11*MCRFTime - (B21*cVx + B31*cVy + B41*cVz)*MCRFTime) + 
		Mathf.Pow(ay, 2)*(currCoordT - currCoordX*cVx - currCoordY*cVy - currCoordZ*cVz + B11*MCRFTime - (B21*cVx + B31*cVy + B41*cVz)*MCRFTime) + ax*(B12 - B22*cVx - B32*cVy - B42*cVz)*(-1 + Mathf.Sqrt(1 + (Mathf.Pow(ax, 2) + Mathf.Pow(ay, 2) + Mathf.Pow(az, 2))*Mathf.Pow(MCRFTime, 2))) + 
		ay*(B13 - B23*cVx - B33*cVy - B43*cVz)*(-1 + Mathf.Sqrt(1 + (Mathf.Pow(ax, 2) + Mathf.Pow(ay, 2) + Mathf.Pow(az, 2))*Mathf.Pow(MCRFTime, 2))) + 
		az*(B24*cVx + B34*cVy + B44*cVz + az*(currCoordT - currCoordX*cVx - currCoordY*cVy - currCoordZ*cVz + B11*MCRFTime - (B21*cVx + B31*cVy + B41*cVz)*MCRFTime) + B14*Mathf.Sqrt(1 + (Mathf.Pow(ax, 2) + Mathf.Pow(ay, 2) + Mathf.Pow(az, 2))*Mathf.Pow(MCRFTime, 2)) - 
		(B24*cVx + B34*cVy + B44*cVz)*Mathf.Sqrt(1 + (Mathf.Pow(ax, 2) + Mathf.Pow(ay, 2) + Mathf.Pow(az, 2))*Mathf.Pow(MCRFTime, 2))));
	}

	float get_MCRF_time(Vector3 a, Vector3 coordinate_velocity, Matrix4x4 object_to_coordinate_boost, Vector4 current_event_coordinate, float observer_time){
		//Inverse of get_observer_time(). Outputted using Mathematica (Sorry!). Inverse has two solutions, check which is valid then return. Again, sorry for this. I feel really bad.
		//This is actually the ugliest thing I have ever made. So sorry.
		float currCoordT = get_temporal_component(current_event_coordinate);
		float currCoordX = get_spatial_component(current_event_coordinate).x;
		float currCoordY = get_spatial_component(current_event_coordinate).y;
		float currCoordZ = get_spatial_component(current_event_coordinate).z;
		float ax = a.x;
		float ay = a.y;
		float az = a.z;
		float cVx = coordinate_velocity.x;
		float cVy = coordinate_velocity.y;
		float cVz = coordinate_velocity.z;
		float B11 = object_to_coordinate_boost[0,0];
		float B12 = object_to_coordinate_boost[0,1];
		float B13 = object_to_coordinate_boost[0,2];
		float B14 = object_to_coordinate_boost[0,3];
		float B21 = object_to_coordinate_boost[1,0];
		float B22 = object_to_coordinate_boost[1,1];
		float B23 = object_to_coordinate_boost[1,2];
		float B24 = object_to_coordinate_boost[1,3];
		float B31 = object_to_coordinate_boost[2,0];
		float B32 = object_to_coordinate_boost[2,1];
		float B33 = object_to_coordinate_boost[2,2];
		float B34 = object_to_coordinate_boost[2,3];
		float B41 = object_to_coordinate_boost[3,0];
		float B42 = object_to_coordinate_boost[3,1];
		float B43 = object_to_coordinate_boost[3,2];
		float B44 = object_to_coordinate_boost[3,3];
		float t = observer_time;
		float T1 = (Mathf.Sqrt(Mathf.Pow(ax*(-B12 + B22*cVx + B32*cVy + B42*cVz) + ay*(-B13 + B23*cVx + B33*cVy + B43*cVz) + az*(-B14 + B24*cVx + B34*cVy + B44*cVz), 2)*(Mathf.Pow(B11, 2) - 2*az*B14*currCoordT + Mathf.Pow(az, 2)*Mathf.Pow(currCoordT, 2) + 2*az*B24*currCoordT*cVx + 
		           2*az*B14*currCoordX*cVx - 2*Mathf.Pow(az, 2)*currCoordT*currCoordX*cVx + Mathf.Pow(B21, 2)*Mathf.Pow(cVx, 2) - 2*az*B24*currCoordX*Mathf.Pow(cVx, 2) + Mathf.Pow(az, 2)*Mathf.Pow(currCoordX, 2)*Mathf.Pow(cVx, 2) + 2*az*B34*currCoordT*cVy + 2*az*B14*currCoordY*cVy - 2*Mathf.Pow(az, 2)*currCoordT*currCoordY*cVy + 
		           2*B21*B31*cVx*cVy - 2*az*B34*currCoordX*cVx*cVy - 2*az*B24*currCoordY*cVx*cVy + 2*Mathf.Pow(az, 2)*currCoordX*currCoordY*cVx*cVy + Mathf.Pow(B31, 2)*Mathf.Pow(cVy, 2) - 2*az*B34*currCoordY*Mathf.Pow(cVy, 2) + Mathf.Pow(az, 2)*Mathf.Pow(currCoordY, 2)*Mathf.Pow(cVy, 2) + 2*az*B44*currCoordT*cVz + 
		           2*az*B14*currCoordZ*cVz - 2*Mathf.Pow(az, 2)*currCoordT*currCoordZ*cVz + 2*B21*B41*cVx*cVz - 2*az*B44*currCoordX*cVx*cVz - 2*az*B24*currCoordZ*cVx*cVz + 2*Mathf.Pow(az, 2)*currCoordX*currCoordZ*cVx*cVz + 2*B31*B41*cVy*cVz - 
		           2*az*B44*currCoordY*cVy*cVz - 2*az*B34*currCoordZ*cVy*cVz + 2*Mathf.Pow(az, 2)*currCoordY*currCoordZ*cVy*cVz + Mathf.Pow(B41, 2)*Mathf.Pow(cVz, 2) - 2*az*B44*currCoordZ*Mathf.Pow(cVz, 2) + Mathf.Pow(az, 2)*Mathf.Pow(currCoordZ, 2)*Mathf.Pow(cVz, 2) - 2*B11*(B21*cVx + B31*cVy + B41*cVz) + 
		           2*ay*(B13 - B23*cVx - B33*cVy - B43*cVz)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz) + 2*ay*B13*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*az*B14*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 
		           2*Mathf.Pow(ay, 2)*currCoordT*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*Mathf.Pow(az, 2)*currCoordT*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*ay*B23*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*az*B24*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 
		           2*Mathf.Pow(ay, 2)*currCoordX*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(az, 2)*currCoordX*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*ay*B33*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*az*B34*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 
		           2*Mathf.Pow(ay, 2)*currCoordY*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(az, 2)*currCoordY*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*ay*B43*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*az*B44*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 
		           2*Mathf.Pow(ay, 2)*currCoordZ*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(az, 2)*currCoordZ*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + Mathf.Pow(az, 2)*Mathf.Pow(t, 2) - Mathf.Pow(az, 2)*Mathf.Pow(cVx, 2)*Mathf.Pow(t, 2) - Mathf.Pow(az, 2)*Mathf.Pow(cVy, 2)*Mathf.Pow(t, 2) - Mathf.Pow(az, 2)*Mathf.Pow(cVz, 2)*Mathf.Pow(t, 2) + 
		           2*ax*(B12 - B22*cVx - B32*cVy - B42*cVz)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t) + 
		           Mathf.Pow(ay, 2)*(Mathf.Pow(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz, 2) - (-1 + Mathf.Pow(cVx, 2) + Mathf.Pow(cVy, 2) + Mathf.Pow(cVz, 2))*Mathf.Pow(t, 2)) + Mathf.Pow(ax, 2)*(Mathf.Pow(currCoordT, 2) + Mathf.Pow(currCoordX, 2)*Mathf.Pow(cVx, 2) + Mathf.Pow(currCoordY, 2)*Mathf.Pow(cVy, 2) + 2*currCoordY*currCoordZ*cVy*cVz + 
		           Mathf.Pow(currCoordZ, 2)*Mathf.Pow(cVz, 2) + 2*currCoordY*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*currCoordZ*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + Mathf.Pow(t, 2) - Mathf.Pow(cVx, 2)*Mathf.Pow(t, 2) - Mathf.Pow(cVy, 2)*Mathf.Pow(t, 2) - Mathf.Pow(cVz, 2)*Mathf.Pow(t, 2) + 
		           2*currCoordX*cVx*(currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t) - 2*currCoordT*(currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t)))) + 
		           (B11 - B21*cVx - B31*cVy - B41*cVz)*(ax*(B12 - B22*cVx - B32*cVy - B42*cVz) + ay*(B13 - B23*cVx - B33*cVy - B43*cVz) + Mathf.Pow(ax, 2)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + 
		           Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t) + Mathf.Pow(ay, 2)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t) + 
		           az*(B14 - B24*cVx - B34*cVy - B44*cVz + az*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t))))/
		           (Mathf.Pow(ax, 2)*(B11 - B12 - B21*cVx + B22*cVx - B31*cVy + B32*cVy - B41*cVz + B42*cVz)*(B11 + B12 - (B21 + B22)*cVx - (B31 + B32)*cVy - (B41 + B42)*cVz) + 
		           Mathf.Pow(ay, 2)*(B11 - B13 - B21*cVx + B23*cVx - B31*cVy + B33*cVy - B41*cVz + B43*cVz)*(B11 + B13 - (B21 + B23)*cVx - (B31 + B33)*cVy - (B41 + B43)*cVz) + 
		           2*ay*az*(B13 - B23*cVx - B33*cVy - B43*cVz)*(-B14 + B24*cVx + B34*cVy + B44*cVz) + Mathf.Pow(az, 2)*(B11 - B14 - B21*cVx + B24*cVx - B31*cVy + B34*cVy - B41*cVz + B44*cVz)*
		           (B11 + B14 - (B21 + B24)*cVx - (B31 + B34)*cVy - (B41 + B44)*cVz) + 2*ax*(B12 - B22*cVx - B32*cVy - B42*cVz)*(ay*(-B13 + B23*cVx + B33*cVy + B43*cVz) + az*(-B14 + B24*cVx + B34*cVy + B44*cVz)));
		float T2 = (-Mathf.Sqrt(Mathf.Pow(ax*(-B12 + B22*cVx + B32*cVy + B42*cVz) + ay*(-B13 + B23*cVx + B33*cVy + B43*cVz) + az*(-B14 + B24*cVx + B34*cVy + B44*cVz), 2)*(Mathf.Pow(B11, 2) - 2*az*B14*currCoordT + Mathf.Pow(az, 2)*Mathf.Pow(currCoordT, 2) + 2*az*B24*currCoordT*cVx + 
		           2*az*B14*currCoordX*cVx - 2*Mathf.Pow(az, 2)*currCoordT*currCoordX*cVx + Mathf.Pow(B21, 2)*Mathf.Pow(cVx, 2) - 2*az*B24*currCoordX*Mathf.Pow(cVx, 2) + Mathf.Pow(az, 2)*Mathf.Pow(currCoordX, 2)*Mathf.Pow(cVx, 2) + 2*az*B34*currCoordT*cVy + 2*az*B14*currCoordY*cVy - 2*Mathf.Pow(az, 2)*currCoordT*currCoordY*cVy + 
		           2*B21*B31*cVx*cVy - 2*az*B34*currCoordX*cVx*cVy - 2*az*B24*currCoordY*cVx*cVy + 2*Mathf.Pow(az, 2)*currCoordX*currCoordY*cVx*cVy + Mathf.Pow(B31, 2)*Mathf.Pow(cVy, 2) - 2*az*B34*currCoordY*Mathf.Pow(cVy, 2) + Mathf.Pow(az, 2)*Mathf.Pow(currCoordY, 2)*Mathf.Pow(cVy, 2) + 2*az*B44*currCoordT*cVz + 
		           2*az*B14*currCoordZ*cVz - 2*Mathf.Pow(az, 2)*currCoordT*currCoordZ*cVz + 2*B21*B41*cVx*cVz - 2*az*B44*currCoordX*cVx*cVz - 2*az*B24*currCoordZ*cVx*cVz + 2*Mathf.Pow(az, 2)*currCoordX*currCoordZ*cVx*cVz + 2*B31*B41*cVy*cVz - 
		           2*az*B44*currCoordY*cVy*cVz - 2*az*B34*currCoordZ*cVy*cVz + 2*Mathf.Pow(az, 2)*currCoordY*currCoordZ*cVy*cVz + Mathf.Pow(B41, 2)*Mathf.Pow(cVz, 2) - 2*az*B44*currCoordZ*Mathf.Pow(cVz, 2) + Mathf.Pow(az, 2)*Mathf.Pow(currCoordZ, 2)*Mathf.Pow(cVz, 2) - 2*B11*(B21*cVx + B31*cVy + B41*cVz) + 
		           2*ay*(B13 - B23*cVx - B33*cVy - B43*cVz)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz) + 2*ay*B13*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*az*B14*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 
		           2*Mathf.Pow(ay, 2)*currCoordT*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*Mathf.Pow(az, 2)*currCoordT*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*ay*B23*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*az*B24*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 
		           2*Mathf.Pow(ay, 2)*currCoordX*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(az, 2)*currCoordX*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*ay*B33*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*az*B34*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 
		           2*Mathf.Pow(ay, 2)*currCoordY*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(az, 2)*currCoordY*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*ay*B43*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*az*B44*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 
		           2*Mathf.Pow(ay, 2)*currCoordZ*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(az, 2)*currCoordZ*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + Mathf.Pow(az, 2)*Mathf.Pow(t, 2) - Mathf.Pow(az, 2)*Mathf.Pow(cVx, 2)*Mathf.Pow(t, 2) - Mathf.Pow(az, 2)*Mathf.Pow(cVy, 2)*Mathf.Pow(t, 2) - Mathf.Pow(az, 2)*Mathf.Pow(cVz, 2)*Mathf.Pow(t, 2) + 
		           2*ax*(B12 - B22*cVx - B32*cVy - B42*cVz)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t) + 
		           Mathf.Pow(ay, 2)*(Mathf.Pow(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz, 2) - (-1 + Mathf.Pow(cVx, 2) + Mathf.Pow(cVy, 2) + Mathf.Pow(cVz, 2))*Mathf.Pow(t, 2)) + Mathf.Pow(ax, 2)*(Mathf.Pow(currCoordT, 2) + Mathf.Pow(currCoordX, 2)*Mathf.Pow(cVx, 2) + Mathf.Pow(currCoordY, 2)*Mathf.Pow(cVy, 2) + 2*currCoordY*currCoordZ*cVy*cVz + 
		           Mathf.Pow(currCoordZ, 2)*Mathf.Pow(cVz, 2) + 2*currCoordY*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*currCoordZ*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + Mathf.Pow(t, 2) - Mathf.Pow(cVx, 2)*Mathf.Pow(t, 2) - Mathf.Pow(cVy, 2)*Mathf.Pow(t, 2) - Mathf.Pow(cVz, 2)*Mathf.Pow(t, 2) + 
		           2*currCoordX*cVx*(currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t) - 2*currCoordT*(currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t)))) + 
		           (B11 - B21*cVx - B31*cVy - B41*cVz)*(ax*(B12 - B22*cVx - B32*cVy - B42*cVz) + ay*(B13 - B23*cVx - B33*cVy - B43*cVz) + Mathf.Pow(ax, 2)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + 
		           Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t) + Mathf.Pow(ay, 2)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t) + 
		           az*(B14 - B24*cVx - B34*cVy - B44*cVz + az*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t))))/
		           (Mathf.Pow(ax, 2)*(B11 - B12 - B21*cVx + B22*cVx - B31*cVy + B32*cVy - B41*cVz + B42*cVz)*(B11 + B12 - (B21 + B22)*cVx - (B31 + B32)*cVy - (B41 + B42)*cVz) + 
		           Mathf.Pow(ay, 2)*(B11 - B13 - B21*cVx + B23*cVx - B31*cVy + B33*cVy - B41*cVz + B43*cVz)*(B11 + B13 - (B21 + B23)*cVx - (B31 + B33)*cVy - (B41 + B43)*cVz) + 
		           2*ay*az*(B13 - B23*cVx - B33*cVy - B43*cVz)*(-B14 + B24*cVx + B34*cVy + B44*cVz) + Mathf.Pow(az, 2)*(B11 - B14 - B21*cVx + B24*cVx - B31*cVy + B34*cVy - B41*cVz + B44*cVz)*
		           (B11 + B14 - (B21 + B24)*cVx - (B31 + B34)*cVy - (B41 + B44)*cVz) + 2*ax*(B12 - B22*cVx - B32*cVy - B42*cVz)*(ay*(-B13 + B23*cVx + B33*cVy + B43*cVz) + az*(-B14 + B24*cVx + B34*cVy + B44*cVz)));
		/*
		float T1 = (az*B11*B14 - Mathf.Pow(az, 2)*B11*currCoordT - az*B14*B21*cVx - az*B11*B24*cVx + Mathf.Pow(az, 2)*B21*currCoordT*cVx + Mathf.Pow(az, 2)*B11*currCoordX*cVx + az*B21*B24*Mathf.Pow(cVx, 2) - Mathf.Pow(az, 2)*B21*currCoordX*Mathf.Pow(cVx, 2) - az*B14*B31*cVy - az*B11*B34*cVy + 
		           Mathf.Pow(az, 2)*B31*currCoordT*cVy + Mathf.Pow(az, 2)*B11*currCoordY*cVy + az*B24*B31*cVx*cVy + az*B21*B34*cVx*cVy - Mathf.Pow(az, 2)*B31*currCoordX*cVx*cVy - Mathf.Pow(az, 2)*B21*currCoordY*cVx*cVy + az*B31*B34*Mathf.Pow(cVy, 2) - Mathf.Pow(az, 2)*B31*currCoordY*Mathf.Pow(cVy, 2) - az*B14*B41*cVz - 
		           az*B11*B44*cVz + Mathf.Pow(az, 2)*B41*currCoordT*cVz + Mathf.Pow(az, 2)*B11*currCoordZ*cVz + az*B24*B41*cVx*cVz + az*B21*B44*cVx*cVz - Mathf.Pow(az, 2)*B41*currCoordX*cVx*cVz - Mathf.Pow(az, 2)*B21*currCoordZ*cVx*cVz + az*B34*B41*cVy*cVz + az*B31*B44*cVy*cVz - 
		           Mathf.Pow(az, 2)*B41*currCoordY*cVy*cVz - Mathf.Pow(az, 2)*B31*currCoordZ*cVy*cVz + az*B41*B44*Mathf.Pow(cVz, 2) - Mathf.Pow(az, 2)*B41*currCoordZ*Mathf.Pow(cVz, 2) - ax*(B11 - B21*cVx - B31*cVy - B41*cVz)*(-B12 + B22*cVx + B32*cVy + B42*cVz) - 
		           ay*(B11 - B21*cVx - B31*cVy - B41*cVz)*(-B13 + B23*cVx + B33*cVy + B43*cVz) + Mathf.Pow(az, 2)*B11*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - Mathf.Pow(az, 2)*B21*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - Mathf.Pow(az, 2)*B31*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 
		           Mathf.Pow(az, 2)*B41*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + Mathf.Pow(ax, 2)*(B11 - B21*cVx - B31*cVy - B41*cVz)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t) + 
		           Mathf.Pow(ay, 2)*(B11 - B21*cVx - B31*cVy - B41*cVz)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t) + 
		           Mathf.Sqrt(Mathf.Pow(ax*(-B12 + B22*cVx + B32*cVy + B42*cVz) + ay*(-B13 + B23*cVx + B33*cVy + B43*cVz) + az*(-B14 + B24*cVx + B34*cVy + B44*cVz), 2)*(Mathf.Pow(B11, 2) - 2*ay*B13*currCoordT - 2*az*B14*currCoordT + Mathf.Pow(ay, 2)*Mathf.Pow(currCoordT, 2) + 
		           Mathf.Pow(az, 2)*Mathf.Pow(currCoordT, 2) + 2*ay*B23*currCoordT*cVx + 2*az*B24*currCoordT*cVx + 2*ay*B13*currCoordX*cVx + 2*az*B14*currCoordX*cVx - 2*Mathf.Pow(ay, 2)*currCoordT*currCoordX*cVx - 2*Mathf.Pow(az, 2)*currCoordT*currCoordX*cVx + Mathf.Pow(B21, 2)*Mathf.Pow(cVx, 2) - 
		           2*ay*B23*currCoordX*Mathf.Pow(cVx, 2) - 2*az*B24*currCoordX*Mathf.Pow(cVx, 2) + Mathf.Pow(ay, 2)*Mathf.Pow(currCoordX, 2)*Mathf.Pow(cVx, 2) + Mathf.Pow(az, 2)*Mathf.Pow(currCoordX, 2)*Mathf.Pow(cVx, 2) + 2*ay*B33*currCoordT*cVy + 2*az*B34*currCoordT*cVy + 2*ay*B13*currCoordY*cVy + 2*az*B14*currCoordY*cVy - 
		           2*Mathf.Pow(ay, 2)*currCoordT*currCoordY*cVy - 2*Mathf.Pow(az, 2)*currCoordT*currCoordY*cVy + 2*B21*B31*cVx*cVy - 2*ay*B33*currCoordX*cVx*cVy - 2*az*B34*currCoordX*cVx*cVy - 2*ay*B23*currCoordY*cVx*cVy - 2*az*B24*currCoordY*cVx*cVy + 
		           2*Mathf.Pow(ay, 2)*currCoordX*currCoordY*cVx*cVy + 2*Mathf.Pow(az, 2)*currCoordX*currCoordY*cVx*cVy + Mathf.Pow(B31, 2)*Mathf.Pow(cVy, 2) - 2*ay*B33*currCoordY*Mathf.Pow(cVy, 2) - 2*az*B34*currCoordY*Mathf.Pow(cVy, 2) + Mathf.Pow(ay, 2)*Mathf.Pow(currCoordY, 2)*Mathf.Pow(cVy, 2) + Mathf.Pow(az, 2)*Mathf.Pow(currCoordY, 2)*Mathf.Pow(cVy, 2) + 
		           2*ay*B43*currCoordT*cVz + 2*az*B44*currCoordT*cVz + 2*ay*B13*currCoordZ*cVz + 2*az*B14*currCoordZ*cVz - 2*Mathf.Pow(ay, 2)*currCoordT*currCoordZ*cVz - 2*Mathf.Pow(az, 2)*currCoordT*currCoordZ*cVz + 2*B21*B41*cVx*cVz - 
		           2*ay*B43*currCoordX*cVx*cVz - 2*az*B44*currCoordX*cVx*cVz - 2*ay*B23*currCoordZ*cVx*cVz - 2*az*B24*currCoordZ*cVx*cVz + 2*Mathf.Pow(ay, 2)*currCoordX*currCoordZ*cVx*cVz + 2*Mathf.Pow(az, 2)*currCoordX*currCoordZ*cVx*cVz + 2*B31*B41*cVy*cVz - 
		           2*ay*B43*currCoordY*cVy*cVz - 2*az*B44*currCoordY*cVy*cVz - 2*ay*B33*currCoordZ*cVy*cVz - 2*az*B34*currCoordZ*cVy*cVz + 2*Mathf.Pow(ay, 2)*currCoordY*currCoordZ*cVy*cVz + 2*Mathf.Pow(az, 2)*currCoordY*currCoordZ*cVy*cVz + Mathf.Pow(B41, 2)*Mathf.Pow(cVz, 2) - 
		           2*ay*B43*currCoordZ*Mathf.Pow(cVz, 2) - 2*az*B44*currCoordZ*Mathf.Pow(cVz, 2) + Mathf.Pow(ay, 2)*Mathf.Pow(currCoordZ, 2)*Mathf.Pow(cVz, 2) + Mathf.Pow(az, 2)*Mathf.Pow(currCoordZ, 2)*Mathf.Pow(cVz, 2) - 2*B11*(B21*cVx + B31*cVy + B41*cVz) + 2*ay*B13*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 
		           2*az*B14*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*Mathf.Pow(ay, 2)*currCoordT*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*Mathf.Pow(az, 2)*currCoordT*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*ay*B23*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 
		           2*az*B24*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(ay, 2)*currCoordX*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(az, 2)*currCoordX*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*ay*B33*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 
		           2*az*B34*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(ay, 2)*currCoordY*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(az, 2)*currCoordY*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*ay*B43*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 
		           2*az*B44*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(ay, 2)*currCoordZ*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(az, 2)*currCoordZ*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + Mathf.Pow(ay, 2)*Mathf.Pow(t, 2) + Mathf.Pow(az, 2)*Mathf.Pow(t, 2) - Mathf.Pow(ay, 2)*Mathf.Pow(cVx, 2)*Mathf.Pow(t, 2) - 
		           Mathf.Pow(az, 2)*Mathf.Pow(cVx, 2)*Mathf.Pow(t, 2) - Mathf.Pow(ay, 2)*Mathf.Pow(cVy, 2)*Mathf.Pow(t, 2) - Mathf.Pow(az, 2)*Mathf.Pow(cVy, 2)*Mathf.Pow(t, 2) - Mathf.Pow(ay, 2)*Mathf.Pow(cVz, 2)*Mathf.Pow(t, 2) - Mathf.Pow(az, 2)*Mathf.Pow(cVz, 2)*Mathf.Pow(t, 2) + 2*ax*(B12 - B22*cVx - B32*cVy - B42*cVz)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + 
		           Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t) + Mathf.Pow(ax, 2)*(Mathf.Pow(currCoordT, 2) + Mathf.Pow(currCoordX, 2)*Mathf.Pow(cVx, 2) + Mathf.Pow(currCoordY, 2)*Mathf.Pow(cVy, 2) + 2*currCoordY*currCoordZ*cVy*cVz + Mathf.Pow(currCoordZ, 2)*Mathf.Pow(cVz, 2) + 2*currCoordY*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 
		           2*currCoordZ*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + Mathf.Pow(t, 2) - Mathf.Pow(cVx, 2)*Mathf.Pow(t, 2) - Mathf.Pow(cVy, 2)*Mathf.Pow(t, 2) - Mathf.Pow(cVz, 2)*Mathf.Pow(t, 2) + 2*currCoordX*cVx*(currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t) - 
		           2*currCoordT*(currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t)))))/(2*ay*az*(B13 - B23*cVx - B33*cVy - B43*cVz)*(-B14 + B24*cVx + B34*cVy + B44*cVz) + 
		           Mathf.Pow(ax, 2)*(Mathf.Pow(B11, 2) - Mathf.Pow(B12, 2) + Mathf.Pow(B21, 2)*Mathf.Pow(cVx, 2) - Mathf.Pow(B22, 2)*Mathf.Pow(cVx, 2) + 2*B21*B31*cVx*cVy - 2*B22*B32*cVx*cVy + Mathf.Pow(B31, 2)*Mathf.Pow(cVy, 2) - Mathf.Pow(B32, 2)*Mathf.Pow(cVy, 2) + 2*B21*B41*cVx*cVz - 2*B22*B42*cVx*cVz + 2*B31*B41*cVy*cVz - 2*B32*B42*cVy*cVz + Mathf.Pow(B41, 2)*Mathf.Pow(cVz, 2) - 
		           Mathf.Pow(B42, 2)*Mathf.Pow(cVz, 2) - 2*B11*(B21*cVx + B31*cVy + B41*cVz) + 2*B12*(B22*cVx + B32*cVy + B42*cVz)) + Mathf.Pow(ay, 2)*(Mathf.Pow(B11, 2) - Mathf.Pow(B13, 2) + Mathf.Pow(B21, 2)*Mathf.Pow(cVx, 2) - Mathf.Pow(B23, 2)*Mathf.Pow(cVx, 2) + 2*B21*B31*cVx*cVy - 2*B23*B33*cVx*cVy + Mathf.Pow(B31, 2)*Mathf.Pow(cVy, 2) - Mathf.Pow(B33, 2)*Mathf.Pow(cVy, 2) + 
		           2*B21*B41*cVx*cVz - 2*B23*B43*cVx*cVz + 2*B31*B41*cVy*cVz - 2*B33*B43*cVy*cVz + Mathf.Pow(B41, 2)*Mathf.Pow(cVz, 2) - Mathf.Pow(B43, 2)*Mathf.Pow(cVz, 2) - 2*B11*(B21*cVx + B31*cVy + B41*cVz) + 2*B13*(B23*cVx + B33*cVy + B43*cVz)) + 
		           Mathf.Pow(az, 2)*(Mathf.Pow(B11, 2) - Mathf.Pow(B14, 2) + Mathf.Pow(B21, 2)*Mathf.Pow(cVx, 2) - Mathf.Pow(B24, 2)*Mathf.Pow(cVx, 2) + 2*B21*B31*cVx*cVy - 2*B24*B34*cVx*cVy + Mathf.Pow(B31, 2)*Mathf.Pow(cVy, 2) - Mathf.Pow(B34, 2)*Mathf.Pow(cVy, 2) + 2*B21*B41*cVx*cVz - 2*B24*B44*cVx*cVz + 2*B31*B41*cVy*cVz - 2*B34*B44*cVy*cVz + Mathf.Pow(B41, 2)*Mathf.Pow(cVz, 2) - 
		           Mathf.Pow(B44, 2)*Mathf.Pow(cVz, 2) - 2*B11*(B21*cVx + B31*cVy + B41*cVz) + 2*B14*(B24*cVx + B34*cVy + B44*cVz)) + 2*ax*(B12 - B22*cVx - B32*cVy - B42*cVz)*(ay*(-B13 + B23*cVx + B33*cVy + B43*cVz) + az*(-B14 + B24*cVx + B34*cVy + B44*cVz)));

		float T2 = 	((-az)*B11*B14 + Mathf.Pow(az, 2)*B11*currCoordT + az*B14*B21*cVx + az*B11*B24*cVx - Mathf.Pow(az, 2)*B21*currCoordT*cVx - Mathf.Pow(az, 2)*B11*currCoordX*cVx - az*B21*B24*Mathf.Pow(cVx, 2) + Mathf.Pow(az, 2)*B21*currCoordX*Mathf.Pow(cVx, 2) + az*B14*B31*cVy + az*B11*B34*cVy - 
		           Mathf.Pow(az, 2)*B31*currCoordT*cVy - Mathf.Pow(az, 2)*B11*currCoordY*cVy - az*B24*B31*cVx*cVy - az*B21*B34*cVx*cVy + Mathf.Pow(az, 2)*B31*currCoordX*cVx*cVy + Mathf.Pow(az, 2)*B21*currCoordY*cVx*cVy - az*B31*B34*Mathf.Pow(cVy, 2) + Mathf.Pow(az, 2)*B31*currCoordY*Mathf.Pow(cVy, 2) + az*B14*B41*cVz + 
		           az*B11*B44*cVz - Mathf.Pow(az, 2)*B41*currCoordT*cVz - Mathf.Pow(az, 2)*B11*currCoordZ*cVz - az*B24*B41*cVx*cVz - az*B21*B44*cVx*cVz + Mathf.Pow(az, 2)*B41*currCoordX*cVx*cVz + Mathf.Pow(az, 2)*B21*currCoordZ*cVx*cVz - az*B34*B41*cVy*cVz - az*B31*B44*cVy*cVz + 
		           Mathf.Pow(az, 2)*B41*currCoordY*cVy*cVz + Mathf.Pow(az, 2)*B31*currCoordZ*cVy*cVz - az*B41*B44*Mathf.Pow(cVz, 2) + Mathf.Pow(az, 2)*B41*currCoordZ*Mathf.Pow(cVz, 2) + ax*(B11 - B21*cVx - B31*cVy - B41*cVz)*(-B12 + B22*cVx + B32*cVy + B42*cVz) + 
		           ay*(B11 - B21*cVx - B31*cVy - B41*cVz)*(-B13 + B23*cVx + B33*cVy + B43*cVz) - Mathf.Pow(az, 2)*B11*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + Mathf.Pow(az, 2)*B21*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 
		           Mathf.Pow(az, 2)*B31*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + Mathf.Pow(az, 2)*B41*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - Mathf.Pow(ax, 2)*(B11 - B21*cVx - B31*cVy - B41*cVz)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + 
		           Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t) - Mathf.Pow(ay, 2)*(B11 - B21*cVx - B31*cVy - B41*cVz)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t) + 
		           Mathf.Sqrt(Mathf.Pow(ax*(-B12 + B22*cVx + B32*cVy + B42*cVz) + ay*(-B13 + B23*cVx + B33*cVy + B43*cVz) + az*(-B14 + B24*cVx + B34*cVy + B44*cVz), 2)*(Mathf.Pow(B11, 2) - 2*ay*B13*currCoordT - 2*az*B14*currCoordT + Mathf.Pow(ay, 2)*Mathf.Pow(currCoordT, 2) + 
		           Mathf.Pow(az, 2)*Mathf.Pow(currCoordT, 2) + 2*ay*B23*currCoordT*cVx + 2*az*B24*currCoordT*cVx + 2*ay*B13*currCoordX*cVx + 2*az*B14*currCoordX*cVx - 2*Mathf.Pow(ay, 2)*currCoordT*currCoordX*cVx - 2*Mathf.Pow(az, 2)*currCoordT*currCoordX*cVx + Mathf.Pow(B21, 2)*Mathf.Pow(cVx, 2) - 
		           2*ay*B23*currCoordX*Mathf.Pow(cVx, 2) - 2*az*B24*currCoordX*Mathf.Pow(cVx, 2) + Mathf.Pow(ay, 2)*Mathf.Pow(currCoordX, 2)*Mathf.Pow(cVx, 2) + Mathf.Pow(az, 2)*Mathf.Pow(currCoordX, 2)*Mathf.Pow(cVx, 2) + 2*ay*B33*currCoordT*cVy + 2*az*B34*currCoordT*cVy + 2*ay*B13*currCoordY*cVy + 2*az*B14*currCoordY*cVy - 
		           2*Mathf.Pow(ay, 2)*currCoordT*currCoordY*cVy - 2*Mathf.Pow(az, 2)*currCoordT*currCoordY*cVy + 2*B21*B31*cVx*cVy - 2*ay*B33*currCoordX*cVx*cVy - 2*az*B34*currCoordX*cVx*cVy - 2*ay*B23*currCoordY*cVx*cVy - 2*az*B24*currCoordY*cVx*cVy + 
		           2*Mathf.Pow(ay, 2)*currCoordX*currCoordY*cVx*cVy + 2*Mathf.Pow(az, 2)*currCoordX*currCoordY*cVx*cVy + Mathf.Pow(B31, 2)*Mathf.Pow(cVy, 2) - 2*ay*B33*currCoordY*Mathf.Pow(cVy, 2) - 2*az*B34*currCoordY*Mathf.Pow(cVy, 2) + Mathf.Pow(ay, 2)*Mathf.Pow(currCoordY, 2)*Mathf.Pow(cVy, 2) + Mathf.Pow(az, 2)*Mathf.Pow(currCoordY, 2)*Mathf.Pow(cVy, 2) + 
		           2*ay*B43*currCoordT*cVz + 2*az*B44*currCoordT*cVz + 2*ay*B13*currCoordZ*cVz + 2*az*B14*currCoordZ*cVz - 2*Mathf.Pow(ay, 2)*currCoordT*currCoordZ*cVz - 2*Mathf.Pow(az, 2)*currCoordT*currCoordZ*cVz + 2*B21*B41*cVx*cVz - 
		           2*ay*B43*currCoordX*cVx*cVz - 2*az*B44*currCoordX*cVx*cVz - 2*ay*B23*currCoordZ*cVx*cVz - 2*az*B24*currCoordZ*cVx*cVz + 2*Mathf.Pow(ay, 2)*currCoordX*currCoordZ*cVx*cVz + 2*Mathf.Pow(az, 2)*currCoordX*currCoordZ*cVx*cVz + 
		           2*B31*B41*cVy*cVz - 2*ay*B43*currCoordY*cVy*cVz - 2*az*B44*currCoordY*cVy*cVz - 2*ay*B33*currCoordZ*cVy*cVz - 2*az*B34*currCoordZ*cVy*cVz + 2*Mathf.Pow(ay, 2)*currCoordY*currCoordZ*cVy*cVz + 
		           2*Mathf.Pow(az, 2)*currCoordY*currCoordZ*cVy*cVz + Mathf.Pow(B41, 2)*Mathf.Pow(cVz, 2) - 2*ay*B43*currCoordZ*Mathf.Pow(cVz, 2) - 2*az*B44*currCoordZ*Mathf.Pow(cVz, 2) + Mathf.Pow(ay, 2)*Mathf.Pow(currCoordZ, 2)*Mathf.Pow(cVz, 2) + Mathf.Pow(az, 2)*Mathf.Pow(currCoordZ, 2)*Mathf.Pow(cVz, 2) - 2*B11*(B21*cVx + B31*cVy + B41*cVz) + 
		           2*ay*B13*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*az*B14*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*Mathf.Pow(ay, 2)*currCoordT*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*Mathf.Pow(az, 2)*currCoordT*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 
		           2*ay*B23*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*az*B24*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(ay, 2)*currCoordX*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(az, 2)*currCoordX*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 
		           2*ay*B33*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*az*B34*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(ay, 2)*currCoordY*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(az, 2)*currCoordY*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 
		           2*ay*B43*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*az*B44*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(ay, 2)*currCoordZ*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(az, 2)*currCoordZ*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 
		           Mathf.Pow(ay, 2)*Mathf.Pow(t, 2) + Mathf.Pow(az, 2)*Mathf.Pow(t, 2) - Mathf.Pow(ay, 2)*Mathf.Pow(cVx, 2)*Mathf.Pow(t, 2) - Mathf.Pow(az, 2)*Mathf.Pow(cVx, 2)*Mathf.Pow(t, 2) - Mathf.Pow(ay, 2)*Mathf.Pow(cVy, 2)*Mathf.Pow(t, 2) - Mathf.Pow(az, 2)*Mathf.Pow(cVy, 2)*Mathf.Pow(t, 2) - Mathf.Pow(ay, 2)*Mathf.Pow(cVz, 2)*Mathf.Pow(t, 2) - Mathf.Pow(az, 2)*Mathf.Pow(cVz, 2)*Mathf.Pow(t, 2) + 2*ax*(B12 - B22*cVx - B32*cVy - B42*cVz)*
		           (-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t) + Mathf.Pow(ax, 2)*(Mathf.Pow(currCoordT, 2) + Mathf.Pow(currCoordX, 2)*Mathf.Pow(cVx, 2) + Mathf.Pow(currCoordY, 2)*Mathf.Pow(cVy, 2) + 2*currCoordY*currCoordZ*cVy*cVz + 
		           Mathf.Pow(currCoordZ, 2)*Mathf.Pow(cVz, 2) + 2*currCoordY*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*currCoordZ*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + Mathf.Pow(t, 2) - Mathf.Pow(cVx, 2)*Mathf.Pow(t, 2) - Mathf.Pow(cVy, 2)*Mathf.Pow(t, 2) - Mathf.Pow(cVz, 2)*Mathf.Pow(t, 2) + 
		           2*currCoordX*cVx*(currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t) - 2*currCoordT*(currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t)))))/
		           (-2*ay*az*(B13 - B23*cVx - B33*cVy - B43*cVz)*(-B14 + B24*cVx + B34*cVy + B44*cVz) + Mathf.Pow(ax, 2)*(-Mathf.Pow(B11, 2) + Mathf.Pow(B12, 2) - Mathf.Pow(B21, 2)*Mathf.Pow(cVx, 2) + Mathf.Pow(B22, 2)*Mathf.Pow(cVx, 2) - 2*B21*B31*cVx*cVy + 2*B22*B32*cVx*cVy - Mathf.Pow(B31, 2)*Mathf.Pow(cVy, 2) + Mathf.Pow(B32, 2)*Mathf.Pow(cVy, 2) - 
		           2*B21*B41*cVx*cVz + 2*B22*B42*cVx*cVz - 2*B31*B41*cVy*cVz + 2*B32*B42*cVy*cVz - Mathf.Pow(B41, 2)*Mathf.Pow(cVz, 2) + Mathf.Pow(B42, 2)*Mathf.Pow(cVz, 2) + 2*B11*(B21*cVx + B31*cVy + B41*cVz) - 2*B12*(B22*cVx + B32*cVy + B42*cVz)) + 
		           Mathf.Pow(ay, 2)*(-Mathf.Pow(B11, 2) + Mathf.Pow(B13, 2) - Mathf.Pow(B21, 2)*Mathf.Pow(cVx, 2) + Mathf.Pow(B23, 2)*Mathf.Pow(cVx, 2) - 2*B21*B31*cVx*cVy + 2*B23*B33*cVx*cVy - Mathf.Pow(B31, 2)*Mathf.Pow(cVy, 2) + Mathf.Pow(B33, 2)*Mathf.Pow(cVy, 2) - 2*B21*B41*cVx*cVz + 2*B23*B43*cVx*cVz - 2*B31*B41*cVy*cVz + 2*B33*B43*cVy*cVz - Mathf.Pow(B41, 2)*Mathf.Pow(cVz, 2) + 
		           Mathf.Pow(B43, 2)*Mathf.Pow(cVz, 2) + 2*B11*(B21*cVx + B31*cVy + B41*cVz) - 2*B13*(B23*cVx + B33*cVy + B43*cVz)) + Mathf.Pow(az, 2)*(-Mathf.Pow(B11, 2) + Mathf.Pow(B14, 2) - Mathf.Pow(B21, 2)*Mathf.Pow(cVx, 2) + Mathf.Pow(B24, 2)*Mathf.Pow(cVx, 2) - 2*B21*B31*cVx*cVy + 2*B24*B34*cVx*cVy - Mathf.Pow(B31, 2)*Mathf.Pow(cVy, 2) + Mathf.Pow(B34, 2)*Mathf.Pow(cVy, 2) - 
		           2*B21*B41*cVx*cVz + 2*B24*B44*cVx*cVz - 2*B31*B41*cVy*cVz + 2*B34*B44*cVy*cVz - Mathf.Pow(B41, 2)*Mathf.Pow(cVz, 2) + Mathf.Pow(B44, 2)*Mathf.Pow(cVz, 2) + 2*B11*(B21*cVx + B31*cVy + B41*cVz) - 2*B14*(B24*cVx + B34*cVy + B44*cVz)) - 
		           2*ax*(B12 - B22*cVx - B32*cVy - B42*cVz)*(ay*(-B13 + B23*cVx + B33*cVy + B43*cVz) + az*(-B14 + B24*cVx + B34*cVy + B44*cVz)));
		*/
		//As these are inverses of get_observer_time() (each valid under certain circumstances), feed the results back into get_observer_time() to find which is valid under these circumstances.
		float obs_time1 = get_observer_time(a, coordinate_velocity, object_to_coordinate_boost, current_event_coordinate, T1);
		float obs_time2 = get_observer_time(a, coordinate_velocity, object_to_coordinate_boost, current_event_coordinate, T2);
		if (Mathf.Abs(observer_time - obs_time2) > Mathf.Abs(observer_time - obs_time1)){
			return T1;
		}else{
			return T2;
		}
	}
}