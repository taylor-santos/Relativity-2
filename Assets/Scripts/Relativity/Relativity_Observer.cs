using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Relativity_Observer : MonoBehaviour {
	public bool FreezeTime = true;
	public Vector3 velocity;
	public List<Vector3> accelerations;
	public List<float> durations;
	public float CoordinateTime = 0;
	public float TimeScale = 1;
	public KeyCode PauseKey = KeyCode.Space;
	public KeyCode SlowTime = KeyCode.LeftArrow;
	public KeyCode SpeedTime = KeyCode.RightArrow;
	public List<GameObject> charges;
	public float LocalTime;
	// Use this for initialization
	void Start () {
		GameObject[] objects = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
		charges = new List<GameObject>();
		foreach(GameObject obj in objects)
		{
			if (obj.GetComponent<Relativity_ChargedObject>() != null)
			{
				charges.Add(obj);
			}
		}
	}

	void Update () {
		if (velocity.magnitude >= 0.999999f)
			velocity = velocity.normalized * (0.999999f);
		if (Input.GetKeyDown(PauseKey)){
			FreezeTime = !FreezeTime;
		}
		if (!FreezeTime){
			//CoordinateTime += Time.deltaTime * TimeScale;
			LocalTime += Time.deltaTime * TimeScale;

		}
		/*
		float elapsed = 0;
		CoordinateTime = 0;
		velocity = Vector3.zero;
		for (int i=0; i<accelerations.Count; ++i){
			float duration = durations[i];
			if (elapsed + duration < LocalTime){
				float coordinate_duration = Sinh(accelerations[i].magnitude*duration)/accelerations[i].magnitude;
				CoordinateTime += coordinate_duration;
				velocity = add_velocity(velocity, accelerations[i]*coordinate_duration / Mathf.Sqrt(1f + (accelerations[i]*coordinate_duration).sqrMagnitude));
				elapsed += duration;
			}else if (elapsed < LocalTime){
				duration = LocalTime-elapsed;
				float coordinate_duration = Sinh(accelerations[i].magnitude*duration)/accelerations[i].magnitude;
				CoordinateTime += coordinate_duration;
				velocity = add_velocity(velocity, accelerations[i]*coordinate_duration / Mathf.Sqrt(1f + (accelerations[i]*coordinate_duration).sqrMagnitude));
				elapsed += duration;
			}
		}
		CoordinateTime += (LocalTime - elapsed);
		*/
		CoordinateTime = LocalTime;
		
		//CoordinateTime = LocalTime;
		//velocity = (transform.position - start_pos) / Time.deltaTime;
		//start_pos = transform.position;

		/*
		float current_time = 0;
		float duration = LocalTime < durations[0] ? LocalTime : durations[0];
		current_time += duration;
		CoordinateTime = Sinh(accelerations[0].magnitude*duration)/accelerations[0].magnitude;
		velocity = accelerations[0]*CoordinateTime / Mathf.Sqrt(1f + accelerations[0].sqrMagnitude*Mathf.Pow(CoordinateTime, 2));
		CoordinateTime += (LocalTime - current_time);
		*/
		/*
		float current_time = 0;
		velocity = initialVelocity;
		float new_coordinate_time = 0;
		for (int i=0; i<accelerations.Count; ++i){
			if (current_time + durations[i] < LocalTime){
				float temp_coordinate_time = Sinh(accelerations[i].magnitude*durations[i])/accelerations[i].magnitude;
				//float temp_coordinate_time = get_coordinate_time(accelerations[i], velocity, durations[i]);
				velocity = add_velocity(velocity, accelerations[i]*temp_coordinate_time / Mathf.Sqrt(1f + accelerations[i].sqrMagnitude * Mathf.Pow(temp_coordinate_time, 2)));
				new_coordinate_time += temp_coordinate_time;
				current_time += durations[i];
			}else{
				
				float duration = LocalTime - current_time;
				float temp_coordinate_time = Sinh(accelerations[i].magnitude*duration)/accelerations[i].magnitude;
				//float temp_coordinate_time = get_coordinate_time(accelerations[i], velocity, duration);
				velocity = add_velocity(velocity, accelerations[i]*temp_coordinate_time / Mathf.Sqrt(1f + accelerations[i].sqrMagnitude * Mathf.Pow(temp_coordinate_time, 2)));
				new_coordinate_time += temp_coordinate_time;
				current_time += duration;
				
				break;
			}
			
		}
		new_coordinate_time += (LocalTime - current_time)*α(velocity);
		CoordinateTime = new_coordinate_time;
		*/
		if (Input.GetKeyDown(SlowTime))
		{
			TimeScale -= 0.2f;
		}
		if (Input.GetKeyDown(SpeedTime))
		{
			TimeScale += 0.2f;
		}
		
		Shader.SetGlobalVector("_Observer_Velocity", velocity);
		Shader.SetGlobalFloat("_Observer_Time", CoordinateTime);
		Shader.SetGlobalVector("_Observer_Position", transform.position);
		
	}

	void OnGUI() {
        GUI.Label(new Rect(10, 10, 100, 20), "Time Scale: " + (TimeScale).ToString("#.0"));
        TimeScale = GUI.HorizontalSlider(new Rect(25, 25, 100, 30), TimeScale, -10, 10);
    }

    void OnApplicationQuit() {
		Shader.SetGlobalVector("_Observer_Velocity", Vector3.zero);
		Shader.SetGlobalFloat("_Observer_Time", 0);
    }


	Vector3 add_velocity(Vector3 v, Vector3 u){
		//Einstein Velocity Addition
		if (v.sqrMagnitude == 0)
			return u;
		if (u.sqrMagnitude == 0)
			return v;
		return 1f/(1+Vector3.Dot(u, v))*(v + u*α(v) + γ(v)/(1f + γ(v))*Vector3.Dot(u, v)*v);
	}

	float γ(Vector3 v){
		//Lorentz Factor
		return 1f/Mathf.Sqrt(1 - v.sqrMagnitude);
	}

	float α(Vector3 v){ 
		//Reciprocal Lorentz Factor
		return Mathf.Sqrt(1 - v.sqrMagnitude);
	}

	float get_coordinate_time(Vector3 a, Vector3 v, float T){
		return 
		(ASinh(T*a.magnitude) + T*a.magnitude*(Mathf.Sqrt(1 + Vector3.Dot(a*T, a*T)) + Vector3.Dot(a*T, v)))
		/
		(2*a.magnitude*(Mathf.Sqrt(1 + Mathf.Pow(T, 2)*a.sqrMagnitude) + Vector3.Dot(a*T, v))*Mathf.Sqrt(-((2+2*Mathf.Sqrt(1+v.sqrMagnitude)-v.sqrMagnitude)*(v.sqrMagnitude - 1))/(Mathf.Pow(Mathf.Sqrt(1 + Mathf.Pow(T, 2)*a.sqrMagnitude) + Vector3.Dot(a*T, v), 2)*Mathf.Pow(1 + Mathf.Sqrt(1-v.sqrMagnitude), 2))));
	}

	Vector3 getVelocity(Vector3 a, Vector3 u_0, float t){
		return
		(
			a
			*
			t
			+
			u_0
		)
		/
		Mathf.Sqrt(
			1
			+
			(a*t + u_0).sqrMagnitude
		);
	}

	Vector3 getVelocityFromProperTime(Vector3 a, Vector3 u_0, float T){
		return (u_0*Vector3.Dot(a,a)+a*(-1+Cosh(T*Mathf.Sqrt(Vector3.Dot(a,a))))*Vector3.Dot(a,u_0)+a*Mathf.Sqrt(Vector3.Dot(a,a))*Mathf.Sqrt(1+Vector3.Dot(u_0,u_0))*Sinh(T*Mathf.Sqrt(Vector3.Dot(a,a))))/(Vector3.Dot(a,a)*Mathf.Sqrt(Mathf.Pow(Cosh(T*Mathf.Sqrt(Vector3.Dot(a,a))),2)*(1+Vector3.Dot(u_0,u_0))+(Mathf.Pow(Vector3.Dot(a,u_0),2)*Mathf.Pow(Sinh(T*Mathf.Sqrt(Vector3.Dot(a,a))),2))/Vector3.Dot(a,a)+(Vector3.Dot(a,u_0)*Mathf.Sqrt(1+Vector3.Dot(u_0,u_0))*Sinh(2*T*Mathf.Sqrt(Vector3.Dot(a,a))))/Mathf.Sqrt(Vector3.Dot(a,a))));
	}

    float Sinh(float x)
	{
		return (Mathf.Pow(Mathf.Exp(1), x) - Mathf.Pow(Mathf.Exp(1), -x))/2;
	}

   	float ASinh(float x)
	{
		return Mathf.Log(x + Mathf.Sqrt(1 + Mathf.Pow(x,2)));
	}

	float Cosh(float x)
	{
		return (Mathf.Pow(Mathf.Exp(1), x) + Mathf.Pow(Mathf.Exp(1), -x))/2;
	}
	float Tanh(float x)
	{
		return Sinh(x)/Cosh(x);
	}
}
