using UnityEngine;
using System.Collections;

public class LightFlash : MonoBehaviour {
	public GameObject Light;
	public bool Flash = false;
	public double FlashAtProperTime = 0;
	public bool TimedFlash;
	public float FlashFrequency;
	public bool RepeatFlash = false;
	private Relativity_Rigidbody rb;
	private Relativity_Observer obs;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Relativity_Rigidbody>();
		obs = rb.Observer.GetComponent<Relativity_Observer>();
	}
	
	// Update is called once per frame
	void LateUpdate () {
		if (Flash)
		{
			Flash = false;
			GameObject lightPrefab = Instantiate(Light, transform.position, Quaternion.Euler(0,0,0)) as GameObject;
			Relativity_PropagateLight lp = lightPrefab.GetComponent<Relativity_PropagateLight>();
			lp.Observer = GetComponent<Relativity_Rigidbody>().Observer;
			lp.coordinateTimeStart = obs.CoordinateTime;
		}
		if (TimedFlash && rb.Proper_Time >= FlashAtProperTime)
		{
			if (RepeatFlash)
			{
				FlashAtProperTime += 1f/FlashFrequency;
			}else{
				TimedFlash = false;
			}
			GameObject lightPrefab = Instantiate(Light, transform.position, Quaternion.Euler(0,0,0)) as GameObject;
			Relativity_PropagateLight lp = lightPrefab.GetComponent<Relativity_PropagateLight>();
			lp.Observer = GetComponent<Relativity_Rigidbody>().Observer;
			double offset = rb.Proper_Time*rb.γ - obs.CoordinateTime;
			double CoordinateTimeStart = FlashAtProperTime*rb.γ - offset;
			lp.coordinateTimeStart = CoordinateTimeStart;
			lightPrefab.transform.position = transform.position + rb.Relative_Velocity * (float)(CoordinateTimeStart-obs.CoordinateTime);
		}
	}
}
