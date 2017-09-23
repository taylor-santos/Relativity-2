using UnityEngine;
using System.Collections;

public class ClockCountIncrementer : MonoBehaviour {
	public GameObject Clock;
	private ClockCounter counter;
	// Use this for initialization
	void Start () {
		counter = Clock.GetComponent<ClockCounter>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider collision)
	{
		if (collision.gameObject.GetComponent<ClockCountIncrementer>() != null)
		{
			counter.Count++;
		}
	}
}
