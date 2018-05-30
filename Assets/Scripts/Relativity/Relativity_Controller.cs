using System.Collections.Generic;
using UnityEngine;

public class Matrix {
	public Matrix4x4 Transformation;
	public Vector4 Translation;
	Matrix() {
		Transformation = Matrix4x4.identity;
		Translation = Vector3.zero;
	}
	Matrix(Matrix4x4 _Transformation, Vector4 _Translation) {
		Transformation = _Transformation;
		Translation = _Translation;
	}
	Matrix(Matrix4x4 _Transformation) {
		Transformation = _Transformation;
		Translation = Vector4.zero;
	}
	public static Matrix operator *(Matrix lhs, Matrix rhs){
		Matrix ret = new Matrix();
		ret.Transformation = lhs.Transformation * rhs.Transformation;
		ret.Translation.w  = lhs.Translation.x + rhs.Translation.x * lhs.Transformation[0, 0] + rhs.Translation.y * lhs.Transformation[0, 1] + rhs.Translation.z * lhs.Transformation[0, 2] + rhs.Translation.w * lhs.Transformation[0, 3];
		ret.Translation.y  = lhs.Translation.y + rhs.Translation.x * lhs.Transformation[1, 0] + rhs.Translation.y * lhs.Transformation[1, 1] + rhs.Translation.z * lhs.Transformation[1, 2] + rhs.Translation.w * lhs.Transformation[1, 3];
		ret.Translation.z  = lhs.Translation.z + rhs.Translation.x * lhs.Transformation[2, 0] + rhs.Translation.y * lhs.Transformation[2, 1] + rhs.Translation.z * lhs.Transformation[2, 2] + rhs.Translation.w * lhs.Transformation[2, 3];
		ret.Translation.w  = lhs.Translation.w + rhs.Translation.x * lhs.Transformation[3, 0] + rhs.Translation.y * lhs.Transformation[3, 1] + rhs.Translation.z * lhs.Transformation[3, 2] + rhs.Translation.w * lhs.Transformation[3, 3];
		return ret;
	}
}

public class Relativity_Controller : MonoBehaviour {

	public GameObject Observer;
	public Vector3 Velocity; //Velocity relative to coordinate frame
	public float ProperTimeOffset;
	public List<Vector3> ProperAccelerations;
	public List<float> AccelerationDurations;
	public List<Vector3> AccelerationOffsets;
	public uint AccelerationCurveDetail = 10;
	public Vector3 Current_Velocity;
	public float Proper_Time;
	public Vector4 Current_Event;
	public int system;
	public bool method;

	private Relativity_Observer observerScript;
	private List<Material> mats;
	private MeshFilter[] MFs;

	void Start() {
		observerScript = Observer.GetComponent<Relativity_Observer>();
		Renderer[] renderers = GetComponentsInChildren<Renderer>();
		mats = new List<Material>();
		foreach (Renderer R in renderers) {
			Material[] childMaterials = R.materials;
			foreach (Material mat in childMaterials) {
				if (mat.shader == Shader.Find("Relativity/Relativity_Shader"))
					mats.Add(mat);
			}
		}
		MFs = GetComponentsInChildren<MeshFilter>();
		if (mats.Count != 0) {
			if (ProperAccelerations.Count > 0 && AccelerationOffsets.Count > 0) {
				Vector4[] shader_ProperAccelerations = new Vector4[ProperAccelerations.Count];
				Vector4[] shader_AccelerationOffsets = new Vector4[ProperAccelerations.Count];
				for (int i = 0; i < ProperAccelerations.Count; ++i) {
					shader_ProperAccelerations[i] = (Vector4)ProperAccelerations[i];
					shader_AccelerationOffsets[i] = (Vector4)AccelerationOffsets[i];
				}
				foreach (Material mat in mats) {
					mat.SetVectorArray("_accelerations", shader_ProperAccelerations);
					mat.SetFloatArray("_durations", AccelerationDurations.ToArray());
					mat.SetVectorArray("_accel_positions", shader_AccelerationOffsets);
				}
			}
			foreach (Material mat in mats) {
				mat.SetVector("_Velocity", Velocity);
				mat.SetFloat("_Proper_Time_Offset", ProperTimeOffset);
			}
		}
	}

	void FixedUpdate() {
		float observer_time = observerScript.CoordinateTime;
		Current_Event = get_state(observer_time);
	}
	
	Vector4 get_state(float observer_time) {
		return Vector4.zero;
	}

	void draw_event_ray(Vector4 start, Vector3 direction, Vector4 offset, Color color) {
		Debug.DrawRay(add_time_to_Z_axis(start, get_temporal_component(offset)) + get_spatial_component(offset), direction, color);
	}

	void draw_event_line(Vector4 start, Vector4 end, Vector4 offset, Color color) {
		Debug.DrawLine(add_time_to_Z_axis(start, get_temporal_component(offset)) + get_spatial_component(offset), add_time_to_Z_axis(end, get_temporal_component(offset)) + get_spatial_component(offset), color);
	}

	Vector3 velocity_to_proper(Vector3 v) {
		return v / Mathf.Sqrt(1f - v.sqrMagnitude);
	}

	Vector3 proper_to_velocity(Vector3 v) {
		return v / Mathf.Sqrt(1f + v.sqrMagnitude);
	}

	Vector3 add_velocity(Vector3 v, Vector3 u) {
		//Einstein Velocity Addition
		if (v.sqrMagnitude == 0)
			return u;
		if (u.sqrMagnitude == 0)
			return v;
		float gamma = γ(v);
		return 1f / (1 + Vector3.Dot(u, v)) * (v + u * α(v) + gamma / (1f + gamma) * Vector3.Dot(u, v) * v);
	}

	Vector3 add_proper_velocity(Vector3 v, Vector3 u) {
		float Bu = 1 / Mathf.Sqrt(1f + u.sqrMagnitude);
		float Bv = 1 / Mathf.Sqrt(1f + v.sqrMagnitude);
		return v + u + (Bv / (1 + Bv) * Vector3.Dot(v, u) + (1 - Bu) / Bu) * v;
	}

	float gamma(Vector3 v) {
		//Lorentz Factor
		return 1f / Mathf.Sqrt(1 - v.sqrMagnitude);
	}

	float γ(Vector3 v) {
		//Lorentz Factor
		return 1f / Mathf.Sqrt(1 - v.sqrMagnitude);
	}

	float α(Vector3 v) {
		//Reciprocal Lorentz Factor
		return Mathf.Sqrt(1 - v.sqrMagnitude);
	}

	float sinh(float x) {
		return (Mathf.Pow(Mathf.Exp(1), x) - Mathf.Pow(Mathf.Exp(1), -x)) / 2;
	}

	float cosh(float x) {
		return (Mathf.Pow(Mathf.Exp(1), x) + Mathf.Pow(Mathf.Exp(1), -x)) / 2;
	}

	float tanh(float x) {
		return sinh(x) / cosh(x);
	}

	float atanh(float x) {
		return (Mathf.Log(1 + x) - Mathf.Log(1 - x)) / 2;
	}

	float acosh(float x) {
		return Mathf.Log(x + Mathf.Sqrt(Mathf.Pow(x, 2) - 1));
	}

	float asinh(float x) {
		return Mathf.Log(x + Mathf.Sqrt(1 + Mathf.Pow(x, 2)));
	}

	Matrix4x4 lorentz_boost(Vector3 v) {
		//Computes the Lorentz Boost matrix for a given 3-dimensional velocity
		float βSqr = v.sqrMagnitude;
		if (βSqr == 0f) {
			return Matrix4x4.identity;
		}
		float βx = v.x;
		float βy = v.y;
		float βz = v.z;
		float gamma = γ(v);

		Matrix4x4 boost = new Matrix4x4(
			new Vector4(gamma, -gamma * βx, -gamma * βy, -gamma * βz),
			new Vector4(-βx * gamma, (gamma - 1) * (βx * βx) / (βSqr) + 1, (gamma - 1) * (βx * βy) / (βSqr), (gamma - 1) * (βx * βz) / (βSqr)),
			new Vector4(-βy * gamma, (gamma - 1) * (βy * βx) / (βSqr), (gamma - 1) * (βy * βy) / (βSqr) + 1, (gamma - 1) * (βy * βz) / (βSqr)),
			new Vector4(-βz * gamma, (gamma - 1) * (βz * βx) / (βSqr), (gamma - 1) * (βz * βy) / (βSqr), (gamma - 1) * (βz * βz) / (βSqr) + 1)
		);

		return boost;

	}

	Vector4 combine_temporal_and_spatial(float t, Vector3 p) {
		return new Vector4(t, p.x, p.y, p.z);
	}

	float get_temporal_component(Vector4 v) {
		return v.x;
	}

	Vector3 get_spatial_component(Vector4 v) {
		return new Vector3(v.y, v.z, v.w);
	}

	Vector3 add_time_to_Z_axis(Vector4 e, float t) {
		//Add new z value to vector v's z-axis
		return get_spatial_component(e) + new Vector3(0, 0, get_temporal_component(e) + t);
	}

	Vector3 boost_to_minkowski(Vector4 pt, Matrix4x4 boost) {
		//Applies a Lorentz boost to a (t,x,y,z) point, then converts it into (x,y,t) coordinates for Minkowski diagram rendering
		Vector4 new_pt = boost * pt;
		return new Vector3(new_pt.y, new_pt.z, new_pt.x);
	}

	float get_MCRF_time(Vector3 a, Vector3 B, float B0, float t) {
		float aa = a.sqrMagnitude;
		float ab = Vector3.Dot(a, B);
		float t2 = t * t;
		float ab2 = ab * ab;
		float B02 = B0 * B0;
		float ab3 = ab2 * ab;
		if (aa == -(ab / t) && ab < 0 && B0 < -Mathf.Sqrt(-2 * ab * t - aa * t2) && t > 0) { return (-ab * B0 - aa * B0 * t) / (ab2 - aa * B02) - Mathf.Sqrt((ab2 * B02 + 2 * ab3 * t + aa * ab2 * t2) / Mathf.Pow(ab2 - aa * B02, 2)); }
		else if (aa == -(ab / t) && t < 0 && ab > 0 && B0 > Mathf.Sqrt(-2 * ab * t - aa * t2)) { return (-ab * B0 - aa * B0 * t) / (ab2 - aa * B02) - Mathf.Sqrt((ab2 * B02 + 2 * ab3 * t + aa * ab2 * t2) / Mathf.Pow(ab2 - aa * B02, 2)); }
		else if (aa == -(ab / t) && ab < 0 && B0 > Mathf.Sqrt(-2 * ab * t - aa * t2) && t > 0) { return (-ab * B0 - aa * B0 * t) / (ab2 - aa * B02) + Mathf.Sqrt((ab2 * B02 + 2 * ab3 * t + aa * ab2 * t2) / Mathf.Pow(ab2 - aa * B02, 2)); }
		else if (aa == -(ab / t) && B0 < -Mathf.Sqrt(-2 * ab * t - aa * t2) && t < 0 && ab > 0) { return (-ab * B0 - aa * B0 * t) / (ab2 - aa * B02) + Mathf.Sqrt((ab2 * B02 + 2 * ab3 * t + aa * ab2 * t2) / Mathf.Pow(ab2 - aa * B02, 2)); }
		else if (ab == 0 && B0 == 0 && t == 0 && aa > 0) { Debug.Log("Error, no MCRFTime possible."); return 0; }
		else if (ab == 0 && t == 0 && B0 < 0 && aa > 0) { return 0; }
		else if (ab == 0 && t == 0 && aa > 0 && B0 > 0) { return 0; }
		else if (ab == 0 && B0 < 0 && t < 0 && aa > 0) { return t / B0; }
		else if (ab == 0 && B0 < 0 && aa > 0 && t > 0) { return t / B0; }
		else if (ab == 0 && t < 0 && aa > 0 && B0 > 0) { return t / B0; }
		else if (ab == 0 && aa > 0 && B0 > 0 && t > 0) { return t / B0; }
		else if (B0 == 0 && t == 0 && ab < 0 && aa > 0) { return 0; }
		else if (B0 == 0 && t == 0 && aa > 0 && ab > 0) { return 0; }
		else if (B0 == -Mathf.Sqrt((ab2 / aa)) && 0 < aa && aa < -(ab / t) && ab < 0 && t > 0) { return (2 * ab * t + aa * t2) / (2 * ab * B0 + 2 * aa * B0 * t); }
		else if (B0 == -Mathf.Sqrt((ab2 / aa)) && 0 < aa && aa < -(ab / t) && t < 0 && ab > 0) { return (2 * ab * t + aa * t2) / (2 * ab * B0 + 2 * aa * B0 * t); }
		else if (B0 == -Mathf.Sqrt((ab2 / aa)) && ab < 0 && t < 0 && aa > 0) { return (2 * ab * t + aa * t2) / (2 * ab * B0 + 2 * aa * B0 * t); }
		else if (B0 == -Mathf.Sqrt((ab2 / aa)) && aa > 0 && ab > 0 && t > 0) { return (2 * ab * t + aa * t2) / (2 * ab * B0 + 2 * aa * B0 * t); }
		else if (B0 == Mathf.Sqrt(ab2 / aa) && 0 < aa && aa < -(ab / t) && ab < 0 && t > 0) { return (2 * ab * t + aa * t2) / (2 * ab * B0 + 2 * aa * B0 * t); }
		else if (B0 == Mathf.Sqrt(ab2 / aa) && 0 < aa && aa < -(ab / t) && t < 0 && ab > 0) { return (2 * ab * t + aa * t2) / (2 * ab * B0 + 2 * aa * B0 * t); }
		else if (B0 == Mathf.Sqrt(ab2 / aa) && ab < 0 && t < 0 && aa > 0) { return (2 * ab * t + aa * t2) / (2 * ab * B0 + 2 * aa * B0 * t); }
		else if (B0 == Mathf.Sqrt(ab2 / aa) && aa > 0 && ab > 0 && t > 0) { return (2 * ab * t + aa * t2) / (2 * ab * B0 + 2 * aa * B0 * t); }
		else if (B0 == -Mathf.Sqrt(-2 * ab * t - aa * t2) && 0 < aa && aa < -(ab / t) && ab < 0 && t > 0) { return (-ab * B0 - aa * B0 * t) / (ab2 - aa * B02) - Mathf.Sqrt((ab2 * B02 + 2 * ab3 * t + aa * ab2 * t2) / Mathf.Pow(ab2 - aa * B02, 2)); }
		else if (B0 == -Mathf.Sqrt(-2 * ab * t - aa * t2) && 0 < aa && aa < -(ab / t) && t < 0 && ab > 0) { return (-ab * B0 - aa * B0 * t) / (ab2 - aa * B02) - Mathf.Sqrt((ab2 * B02 + 2 * ab3 * t + aa * ab2 * t2) / Mathf.Pow(ab2 - aa * B02, 2)); }
		else if (B0 == Mathf.Sqrt(-2 * ab * t - aa * t2) && 0 < aa && aa < -(ab / t) && ab < 0 && t > 0) { return (-ab * B0 - aa * B0 * t) / (ab2 - aa * B02) - Mathf.Sqrt((ab2 * B02 + 2 * ab3 * t + aa * ab2 * t2) / Mathf.Pow(ab2 - aa * B02, 2)); }
		else if (B0 == Mathf.Sqrt(-2 * ab * t - aa * t2) && 0 < aa && aa < -(ab / t) && t < 0 && ab > 0) { return (-ab * B0 - aa * B0 * t) / (ab2 - aa * B02) - Mathf.Sqrt((ab2 * B02 + 2 * ab3 * t + aa * ab2 * t2) / Mathf.Pow(ab2 - aa * B02, 2)); }
		else if (t == 0 && 0 < B0 && B0 < Mathf.Sqrt(ab2 / aa) && ab < 0 && aa > 0) { return 0; }
		else if (t == 0 && 0 < B0 && B0 < Mathf.Sqrt(ab2 / aa) && aa > 0 && ab > 0) { return 0; }
		else if (t == 0 && -Mathf.Sqrt((ab2 / aa)) < B0 && B0 < 0 && ab < 0 && aa > 0) { return 0; }
		else if (t == 0 && -Mathf.Sqrt((ab2 / aa)) < B0 && B0 < 0 && aa > 0 && ab > 0) { return 0; }
		else if (t == 0 && ab < 0 && B0 >= Mathf.Sqrt(ab2 / aa) && aa > 0) { return 0; }
		else if (t == 0 && ab < 0 && aa > 0 && B0 <= -Mathf.Sqrt((ab2 / aa))) { return 0; }
		else if (t == 0 && B0 >= Mathf.Sqrt(ab2 / aa) && aa > 0 && ab > 0) { return 0; }
		else if (t == 0 && aa > 0 && ab > 0 && B0 <= -Mathf.Sqrt((ab2 / aa))) { return 0; }
		else if (t == 0 && 0 < B0 && B0 < Mathf.Sqrt(ab2 / aa) && aa > 0 && ab > 0) { return -Mathf.Sqrt(((ab2 * B02) / Mathf.Pow(ab2 - aa * B02, 2))) - (ab * B0) / (ab2 - aa * B02); }
		else if (t == 0 && -Mathf.Sqrt((ab2 / aa)) < B0 && B0 < 0 && ab < 0 && aa > 0) { return -Mathf.Sqrt(((ab2 * B02) / Mathf.Pow(ab2 - aa * B02, 2))) - (ab * B0) / (ab2 - aa * B02); }
		else if (t == 0 && 0 < B0 && B0 < Mathf.Sqrt(ab2 / aa) && ab < 0 && aa > 0) { return Mathf.Sqrt((ab2 * B02) / Mathf.Pow(ab2 - aa * B02, 2)) - (ab * B0) / (ab2 - aa * B02); }
		else if (t == 0 && -Mathf.Sqrt((ab2 / aa)) < B0 && B0 < 0 && aa > 0 && ab > 0) { return Mathf.Sqrt((ab2 * B02) / Mathf.Pow(ab2 - aa * B02, 2)) - (ab * B0) / (ab2 - aa * B02); }
		else if (0 < aa && aa < -(ab / t) && -Mathf.Sqrt((ab2 / aa)) < B0 && B0 < -Mathf.Sqrt(-2 * ab * t - aa * t2) && ab < 0 && t > 0) { return (-ab * B0 - aa * B0 * t) / (ab2 - aa * B02) - Mathf.Sqrt((ab2 * B02 + 2 * ab3 * t + aa * ab2 * t2) / Mathf.Pow(ab2 - aa * B02, 2)); }
		else if (0 < aa && aa < -(ab / t) && -Mathf.Sqrt((ab2 / aa)) < B0 && B0 < -Mathf.Sqrt(-2 * ab * t - aa * t2) && t < 0 && ab > 0) { return (-ab * B0 - aa * B0 * t) / (ab2 - aa * B02) - Mathf.Sqrt((ab2 * B02 + 2 * ab3 * t + aa * ab2 * t2) / Mathf.Pow(ab2 - aa * B02, 2)); }
		else if (0 < aa && aa < -(ab / t) && Mathf.Sqrt(-2 * ab * t - aa * t2) < B0 && B0 < Mathf.Sqrt(ab2 / aa) && ab < 0 && t > 0) { return (-ab * B0 - aa * B0 * t) / (ab2 - aa * B02) - Mathf.Sqrt((ab2 * B02 + 2 * ab3 * t + aa * ab2 * t2) / Mathf.Pow(ab2 - aa * B02, 2)); }
		else if (0 < aa && aa < -(ab / t) && Mathf.Sqrt(-2 * ab * t - aa * t2) < B0 && B0 < Mathf.Sqrt(ab2 / aa) && t < 0 && ab > 0) { return (-ab * B0 - aa * B0 * t) / (ab2 - aa * B02) - Mathf.Sqrt((ab2 * B02 + 2 * ab3 * t + aa * ab2 * t2) / Mathf.Pow(ab2 - aa * B02, 2)); }
		else if (0 < aa && aa < -(ab / t) && ab < 0 && B0 < -Mathf.Sqrt((ab2 / aa)) && t > 0) { return (-ab * B0 - aa * B0 * t) / (ab2 - aa * B02) - Mathf.Sqrt((ab2 * B02 + 2 * ab3 * t + aa * ab2 * t2) / Mathf.Pow(ab2 - aa * B02, 2)); }
		else if (0 < aa && aa < -(ab / t) && t < 0 && ab > 0 && B0 > Mathf.Sqrt(ab2 / aa)) { return (-ab * B0 - aa * B0 * t) / (ab2 - aa * B02) - Mathf.Sqrt((ab2 * B02 + 2 * ab3 * t + aa * ab2 * t2) / Mathf.Pow(ab2 - aa * B02, 2)); }
		else if (-Mathf.Sqrt((ab2 / aa)) < B0 && B0 < Mathf.Sqrt(ab2 / aa) && ab < 0 && t < 0 && aa > 0) { return (-ab * B0 - aa * B0 * t) / (ab2 - aa * B02) - Mathf.Sqrt((ab2 * B02 + 2 * ab3 * t + aa * ab2 * t2) / Mathf.Pow(ab2 - aa * B02, 2)); }
		else if (-Mathf.Sqrt((ab2 / aa)) < B0 && B0 < Mathf.Sqrt(ab2 / aa) && aa > 0 && ab > 0 && t > 0) { return (-ab * B0 - aa * B0 * t) / (ab2 - aa * B02) - Mathf.Sqrt((ab2 * B02 + 2 * ab3 * t + aa * ab2 * t2) / Mathf.Pow(ab2 - aa * B02, 2)); }
		else if (ab < 0 && B0 < -Mathf.Sqrt((ab2 / aa)) && t < 0 && aa > 0) { return (-ab * B0 - aa * B0 * t) / (ab2 - aa * B02) - Mathf.Sqrt((ab2 * B02 + 2 * ab3 * t + aa * ab2 * t2) / Mathf.Pow(ab2 - aa * B02, 2)); }
		else if (ab < 0 && B0 < -Mathf.Sqrt((ab2 / aa)) && aa > -(ab / t) && t > 0) { return (-ab * B0 - aa * B0 * t) / (ab2 - aa * B02) - Mathf.Sqrt((ab2 * B02 + 2 * ab3 * t + aa * ab2 * t2) / Mathf.Pow(ab2 - aa * B02, 2)); }
		else if (t < 0 && aa > -(ab / t) && ab > 0 && B0 > Mathf.Sqrt(ab2 / aa)) { return (-ab * B0 - aa * B0 * t) / (ab2 - aa * B02) - Mathf.Sqrt((ab2 * B02 + 2 * ab3 * t + aa * ab2 * t2) / Mathf.Pow(ab2 - aa * B02, 2)); }
		else if (aa > 0 && ab > 0 && B0 > Mathf.Sqrt(ab2 / aa) && t > 0) { return (-ab * B0 - aa * B0 * t) / (ab2 - aa * B02) - Mathf.Sqrt((ab2 * B02 + 2 * ab3 * t + aa * ab2 * t2) / Mathf.Pow(ab2 - aa * B02, 2)); }
		else if (0 < aa && aa < -(ab / t) && -Mathf.Sqrt((ab2 / aa)) < B0 && B0 < -Mathf.Sqrt(-2 * ab * t - aa * t2) && ab < 0 && t > 0) { return (-ab * B0 - aa * B0 * t) / (ab2 - aa * B02) + Mathf.Sqrt((ab2 * B02 + 2 * ab3 * t + aa * ab2 * t2) / Mathf.Pow(ab2 - aa * B02, 2)); }
		else if (0 < aa && aa < -(ab / t) && -Mathf.Sqrt((ab2 / aa)) < B0 && B0 < -Mathf.Sqrt(-2 * ab * t - aa * t2) && t < 0 && ab > 0) { return (-ab * B0 - aa * B0 * t) / (ab2 - aa * B02) + Mathf.Sqrt((ab2 * B02 + 2 * ab3 * t + aa * ab2 * t2) / Mathf.Pow(ab2 - aa * B02, 2)); }
		else if (0 < aa && aa < -(ab / t) && Mathf.Sqrt(-2 * ab * t - aa * t2) < B0 && B0 < Mathf.Sqrt(ab2 / aa) && ab < 0 && t > 0) { return (-ab * B0 - aa * B0 * t) / (ab2 - aa * B02) + Mathf.Sqrt((ab2 * B02 + 2 * ab3 * t + aa * ab2 * t2) / Mathf.Pow(ab2 - aa * B02, 2)); }
		else if (0 < aa && aa < -(ab / t) && Mathf.Sqrt(-2 * ab * t - aa * t2) < B0 && B0 < Mathf.Sqrt(ab2 / aa) && t < 0 && ab > 0) { return (-ab * B0 - aa * B0 * t) / (ab2 - aa * B02) + Mathf.Sqrt((ab2 * B02 + 2 * ab3 * t + aa * ab2 * t2) / Mathf.Pow(ab2 - aa * B02, 2)); }
		else if (0 < aa && aa < -(ab / t) && ab < 0 && B0 > Mathf.Sqrt(ab2 / aa) && t > 0) { return (-ab * B0 - aa * B0 * t) / (ab2 - aa * B02) + Mathf.Sqrt((ab2 * B02 + 2 * ab3 * t + aa * ab2 * t2) / Mathf.Pow(ab2 - aa * B02, 2)); }
		else if (0 < aa && aa < -(ab / t) && B0 < -Mathf.Sqrt((ab2 / aa)) && t < 0 && ab > 0) { return (-ab * B0 - aa * B0 * t) / (ab2 - aa * B02) + Mathf.Sqrt((ab2 * B02 + 2 * ab3 * t + aa * ab2 * t2) / Mathf.Pow(ab2 - aa * B02, 2)); }
		else if (-Mathf.Sqrt((ab2 / aa)) < B0 && B0 < Mathf.Sqrt(ab2 / aa) && ab < 0 && t < 0 && aa > 0) { return (-ab * B0 - aa * B0 * t) / (ab2 - aa * B02) + Mathf.Sqrt((ab2 * B02 + 2 * ab3 * t + aa * ab2 * t2) / Mathf.Pow(ab2 - aa * B02, 2)); }
		else if (-Mathf.Sqrt((ab2 / aa)) < B0 && B0 < Mathf.Sqrt(ab2 / aa) && aa > 0 && ab > 0 && t > 0) { return (-ab * B0 - aa * B0 * t) / (ab2 - aa * B02) + Mathf.Sqrt((ab2 * B02 + 2 * ab3 * t + aa * ab2 * t2) / Mathf.Pow(ab2 - aa * B02, 2)); }
		else if (ab < 0 && t < 0 && aa > 0 && B0 > Mathf.Sqrt(ab2 / aa)) { return (-ab * B0 - aa * B0 * t) / (ab2 - aa * B02) + Mathf.Sqrt((ab2 * B02 + 2 * ab3 * t + aa * ab2 * t2) / Mathf.Pow(ab2 - aa * B02, 2)); }
		else if (ab < 0 && aa > -(ab / t) && B0 > Mathf.Sqrt(ab2 / aa) && t > 0) { return (-ab * B0 - aa * B0 * t) / (ab2 - aa * B02) + Mathf.Sqrt((ab2 * B02 + 2 * ab3 * t + aa * ab2 * t2) / Mathf.Pow(ab2 - aa * B02, 2)); }
		else if (B0 < -Mathf.Sqrt((ab2 / aa)) && t < 0 && aa > -(ab / t) && ab > 0) { return (-ab * B0 - aa * B0 * t) / (ab2 - aa * B02) + Mathf.Sqrt((ab2 * B02 + 2 * ab3 * t + aa * ab2 * t2) / Mathf.Pow(ab2 - aa * B02, 2)); }
		else if (B0 < -Mathf.Sqrt((ab2 / aa)) && aa > 0 && ab > 0 && t > 0) { return (-ab * B0 - aa * B0 * t) / (ab2 - aa * B02) + Mathf.Sqrt((ab2 * B02 + 2 * ab3 * t + aa * ab2 * t2) / Mathf.Pow(ab2 - aa * B02, 2)); }
		Debug.Log("Error: No valid MCRFTime found!");
		return 0;
	}

}