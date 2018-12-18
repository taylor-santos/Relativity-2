using UnityEngine;
using System.Collections;

public class LightFlash : MonoBehaviour {
	public GameObject Light;
	public bool Flash = false;
	public double FlashAtProperTime = 0;
	public bool TimedFlash;
	public float FlashFrequency;
	public bool RepeatFlash = false;
	private Relativity_Controller RC;
	private Relativity_Observer obs;

	// Use this for initialization
	void Start () {
		RC = GetComponent<Relativity_Controller>();
		obs = RC.Observer.GetComponent<Relativity_Observer>();
	}
	
	// Update is called once per frame
	void LateUpdate () {
		if (Flash)
		{
			Flash = false;
			GameObject lightPrefab = Instantiate(Light, transform.position, Quaternion.Euler(0,0,0)) as GameObject;
			Relativity_PropagateLight lp = lightPrefab.GetComponent<Relativity_PropagateLight>();
			lp.Observer = GetComponent<Relativity_Controller>().Observer;
			lp.coordinateTimeStart = obs.CoordinateTime;
		}
		if (TimedFlash && RC.ProperTime >= FlashAtProperTime)
		{
			if (RepeatFlash)
			{
				FlashAtProperTime += 1f/FlashFrequency;
			}else{
				TimedFlash = false;
			}
			GameObject lightPrefab = Instantiate(Light, transform.position, Quaternion.Euler(0,0,0)) as GameObject;
			Relativity_PropagateLight lp = lightPrefab.GetComponent<Relativity_PropagateLight>();
			lp.Observer = GetComponent<Relativity_Controller>().Observer;
			double offset = RC.ProperTime/Mathf.Sqrt(RC.Velocity.sqrMagnitude) - obs.CoordinateTime;
			double CoordinateTimeStart = FlashAtProperTime/Mathf.Sqrt(RC.Velocity.sqrMagnitude) - offset;
			lp.coordinateTimeStart = CoordinateTimeStart;
			//lightPrefab.transform.position = transform.position + RC.Current_Velocity * (float)(CoordinateTimeStart-obs.CoordinateTime);
		}
	}
}
