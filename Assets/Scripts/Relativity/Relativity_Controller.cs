using System.Collections.Generic;
using System;
using UnityEngine;

public class Plane {
	public Vector4d Normal;
	public double Distance;
	public Plane() {
		Normal = new Vector4d {
			t = 1
		};
		Distance = 0;
	}
	public Plane(Vector4d _Normal, double _Distance) {
		Normal = _Normal;
		Distance = _Distance;
	}
}
[Serializable]
public class Vector3d {
	public double x, y, z;

	public double Magnitude() { return Math.Sqrt(x* x + y* y + z* z); }
	public double SqrMagnitude() { return x * x + y * y + z * z; }
	public double this[int index] {
		get {
			switch (index) {
				case 0: return x;
				case 1: return y;
				case 2: return z;
				default: throw new ArgumentOutOfRangeException();
			}
		}
		set {
			switch (index) {
				case 0: x = value; break;
				case 1: y = value; break;
				case 2: z = value; break;
				default: throw new ArgumentOutOfRangeException();
			}
		}
	}

	public Vector3d() {
		 x = y = z = 0;
	}
	public Vector3d(double x_, double y_, double z_) {
		x = x_;
		y = y_;
		z = z_;
	}
	public Vector3d(Vector3d other) {
		x = other.x;
		y = other.y;
		z = other.z;
	}
	public Vector3d(Vector3 other) {
		x = other.x;
		y = other.y;
		z = other.z;
	}
	public Vector3d Normalized() {
		double magnitude = Magnitude();
		return new Vector3d(
			x / magnitude,
			y / magnitude,
			z / magnitude
		);
	}
	public Vector3 Vector3() {
		return new Vector3((float)x, (float)y, (float)z);
	}
	public static double operator *(Vector3d lhs, Vector3d rhs) {
		return
			lhs.x * rhs.x +
			lhs.y * rhs.y +
			lhs.z * lhs.z;
	}
	public static Vector3d operator +(Vector3d lhs, Vector3d rhs) {
		return new Vector3d {
			x = lhs.x + rhs.x,
			y = lhs.y + rhs.y,
			z = lhs.z + rhs.z
		};
	}
	public static Vector3d operator *(double d, Vector3d v) {
		return new Vector3d {
			x = d * v.x,
			y = d * v.y,
			z = d * v.z
		};
	}
	public static Vector3d operator *(Vector3d v, double d) {
		return d * v;
	}
	public static Vector3d operator /(Vector3d v, double d) {
		return new Vector3d {
			x = v.x / d,
			y = v.y / d,
			z = v.z / d
		};
	}
	public static Vector3d operator -(Vector3d v) {
		return new Vector3d {
			x = -v.x,
			y = -v.y,
			z = -v.z
		};
	}
}
[Serializable]
public class Vector4d {
	public double t, x, y, z;

	public double this[int index] {
		get {
			switch(index) {
				case 0: return t;
				case 1: return x;
				case 2: return y;
				case 3: return z;
				default: throw new ArgumentOutOfRangeException();
			}
		}
		set {
			switch (index) {
				case 0: t = value; break;
				case 1: x = value; break;
				case 2: y = value; break;
				case 3: z = value; break;
				default: throw new ArgumentOutOfRangeException();
			}
		}
	}

	public double Magnitude() { return Math.Sqrt(t * t + x * x + y * y + z * z); }
	public double SqrMagnitude() { return t * t + x * x + y * y + z * z; }

	public Vector4d() {
		t = x = y = z = 0;
	}
	public Vector4d(double t, double x, double y, double z) {
		this.t = t;
		this.x = x;
		this.y = y;
		this.z = z;
	}
	public Vector4d(Vector4d other) {
		t = other.t;
		x = other.x;
		y = other.y;
		z = other.z;
	}
	public Vector4d(Vector4 other) {
		//Place the V4d's t coordinate in the x-position of Vector4 so matrix multiplication works correctly.
		//The spacial components then fill out the (y,z,w) positions respectively
		t = other.x;
		x = other.y;
		y = other.z;
		z = other.w;
	}
	public Vector4d(double t, Vector3d s) {
		this.t = t;
		x = s.x;
		y = s.y;
		z = s.z;
	}
	public Vector4d Normalized() {
		double magnitude = Magnitude();
		return new Vector4d(
			t / magnitude,
			x / magnitude,
			y / magnitude,
			z / magnitude
		);
	}
	public Vector4 Vector4() {
		return new Vector4((float)t, (float)x, (float)y, (float)z);
	}
	public Vector3d Space() {
		return new Vector3d(x, y, z);
	}
	public static double operator* (Vector4d lhs, Vector4d rhs) {
		return
			lhs.t * rhs.t +
			lhs.x * rhs.x +
			lhs.y * rhs.y +
			lhs.z * lhs.z;
	}
	public static Vector4d operator+ (Vector4d lhs, Vector4d rhs) {
		return new Vector4d {
			t = lhs.t + rhs.t,
			x = lhs.x + rhs.x,
			y = lhs.y + rhs.y,
			z = lhs.z + rhs.z
		};
	}
	public static Vector4d operator* (double d, Vector4d v) {
		return new Vector4d {
			t = d * v.t,
			x = d * v.x,
			y = d * v.y,
			z = d * v.z
		};
	}
	public static Vector4d operator *(Vector4d v, double d) {
		return d * v;
	}
	public static Vector4d operator- (Vector4d v) {
		return new Vector4d {
			t = -v.t,
			x = -v.x,
			y = -v.y,
			z = -v.z
		};
	}
}

public class Matrix4d {
	public double
		m00, m10, m20, m30,
		m01, m11, m21, m31,
		m02, m12, m22, m32,
		m03, m13, m23, m33;

	public Matrix4d() { }
	public Matrix4d(Vector4d column0, Vector4d column1, Vector4d column2, Vector4d column3) {
		m00 = column0.t; m01 = column1.t; m02 = column2.t; m03 = column3.t;
		m10 = column0.x; m11 = column1.x; m12 = column2.x; m13 = column3.x;
		m20 = column0.y; m21 = column1.y; m22 = column2.y; m23 = column3.y;
		m30 = column0.z; m31 = column1.z; m32 = column2.z; m33 = column3.z;
	}
	public double this[int row, int column] {
		get {
			return this[row + column * 4];
		}
		set {
			this[row + column * 4] = value;
		}
	}
	public double this[int index] {
		get {
			switch (index) {
				case 0: return m00;
				case 1: return m10;
				case 2: return m20;
				case 3: return m30;
				case 4: return m01;
				case 5: return m11;
				case 6: return m21;
				case 7: return m31;
				case 8: return m02;
				case 9: return m12;
				case 10: return m22;
				case 11: return m32;
				case 12: return m03;
				case 13: return m13;
				case 14: return m23;
				case 15: return m33;
				default:
					throw new IndexOutOfRangeException("Invalid matrix index!");
			}
		}
		set {
			switch (index) {
				case 0: m00 = value; break;
				case 1: m10 = value; break;
				case 2: m20 = value; break;
				case 3: m30 = value; break;
				case 4: m01 = value; break;
				case 5: m11 = value; break;
				case 6: m21 = value; break;
				case 7: m31 = value; break;
				case 8: m02 = value; break;
				case 9: m12 = value; break;
				case 10: m22 = value; break;
				case 11: m32 = value; break;
				case 12: m03 = value; break;
				case 13: m13 = value; break;
				case 14: m23 = value; break;
				case 15: m33 = value; break;

				default:
					throw new IndexOutOfRangeException("Invalid matrix index!");
			}
		}
	}
	public static Matrix4d operator *(Matrix4d lhs, Matrix4d rhs) {
		Matrix4d res = new Matrix4d {
			m00 = lhs.m00 * rhs.m00 + lhs.m01 * rhs.m10 + lhs.m02 * rhs.m20 + lhs.m03 * rhs.m30,
			m01 = lhs.m00 * rhs.m01 + lhs.m01 * rhs.m11 + lhs.m02 * rhs.m21 + lhs.m03 * rhs.m31,
			m02 = lhs.m00 * rhs.m02 + lhs.m01 * rhs.m12 + lhs.m02 * rhs.m22 + lhs.m03 * rhs.m32,
			m03 = lhs.m00 * rhs.m03 + lhs.m01 * rhs.m13 + lhs.m02 * rhs.m23 + lhs.m03 * rhs.m33,

			m10 = lhs.m10 * rhs.m00 + lhs.m11 * rhs.m10 + lhs.m12 * rhs.m20 + lhs.m13 * rhs.m30,
			m11 = lhs.m10 * rhs.m01 + lhs.m11 * rhs.m11 + lhs.m12 * rhs.m21 + lhs.m13 * rhs.m31,
			m12 = lhs.m10 * rhs.m02 + lhs.m11 * rhs.m12 + lhs.m12 * rhs.m22 + lhs.m13 * rhs.m32,
			m13 = lhs.m10 * rhs.m03 + lhs.m11 * rhs.m13 + lhs.m12 * rhs.m23 + lhs.m13 * rhs.m33,

			m20 = lhs.m20 * rhs.m00 + lhs.m21 * rhs.m10 + lhs.m22 * rhs.m20 + lhs.m23 * rhs.m30,
			m21 = lhs.m20 * rhs.m01 + lhs.m21 * rhs.m11 + lhs.m22 * rhs.m21 + lhs.m23 * rhs.m31,
			m22 = lhs.m20 * rhs.m02 + lhs.m21 * rhs.m12 + lhs.m22 * rhs.m22 + lhs.m23 * rhs.m32,
			m23 = lhs.m20 * rhs.m03 + lhs.m21 * rhs.m13 + lhs.m22 * rhs.m23 + lhs.m23 * rhs.m33,

			m30 = lhs.m30 * rhs.m00 + lhs.m31 * rhs.m10 + lhs.m32 * rhs.m20 + lhs.m33 * rhs.m30,
			m31 = lhs.m30 * rhs.m01 + lhs.m31 * rhs.m11 + lhs.m32 * rhs.m21 + lhs.m33 * rhs.m31,
			m32 = lhs.m30 * rhs.m02 + lhs.m31 * rhs.m12 + lhs.m32 * rhs.m22 + lhs.m33 * rhs.m32,
			m33 = lhs.m30 * rhs.m03 + lhs.m31 * rhs.m13 + lhs.m32 * rhs.m23 + lhs.m33 * rhs.m33
		};

		return res;
	}
	public static Vector4d operator *(Matrix4d lhs, Vector4d vector) {
		Vector4d res = new Vector4d {
			t = lhs.m00 * vector.t + lhs.m01 * vector.x + lhs.m02 * vector.y + lhs.m03 * vector.z,
			x = lhs.m10 * vector.t + lhs.m11 * vector.x + lhs.m12 * vector.y + lhs.m13 * vector.z,
			y = lhs.m20 * vector.t + lhs.m21 * vector.x + lhs.m22 * vector.y + lhs.m23 * vector.z,
			z = lhs.m30 * vector.t + lhs.m31 * vector.x + lhs.m32 * vector.y + lhs.m33 * vector.z
		};
		return res;
	}

	public Matrix4d Inverse() {
		Matrix4d result = new Matrix4d();
		result[0] = this[5] * this[10] * this[15] -
			this[5] * this[11] * this[14] -
			this[9] * this[6] * this[15] +
			this[9] * this[7] * this[14] +
			this[13] * this[6] * this[11] -
			this[13] * this[7] * this[10];

		result[4] = -this[4] * this[10] * this[15] +
			this[4] * this[11] * this[14] +
			this[8] * this[6] * this[15] -
			this[8] * this[7] * this[14] -
			this[12] * this[6] * this[11] +
			this[12] * this[7] * this[10];

		result[8] = this[4] * this[9] * this[15] -
			this[4] * this[11] * this[13] -
			this[8] * this[5] * this[15] +
			this[8] * this[7] * this[13] +
			this[12] * this[5] * this[11] -
			this[12] * this[7] * this[9];

		result[12] = -this[4] * this[9] * this[14] +
			this[4] * this[10] * this[13] +
			this[8] * this[5] * this[14] -
			this[8] * this[6] * this[13] -
			this[12] * this[5] * this[10] +
			this[12] * this[6] * this[9];

		result[1] = -this[1] * this[10] * this[15] +
			this[1] * this[11] * this[14] +
			this[9] * this[2] * this[15] -
			this[9] * this[3] * this[14] -
			this[13] * this[2] * this[11] +
			this[13] * this[3] * this[10];

		result[5] = this[0] * this[10] * this[15] -
			this[0] * this[11] * this[14] -
			this[8] * this[2] * this[15] +
			this[8] * this[3] * this[14] +
			this[12] * this[2] * this[11] -
			this[12] * this[3] * this[10];

		result[9] = -this[0] * this[9] * this[15] +
			this[0] * this[11] * this[13] +
			this[8] * this[1] * this[15] -
			this[8] * this[3] * this[13] -
			this[12] * this[1] * this[11] +
			this[12] * this[3] * this[9];

		result[13] = this[0] * this[9] * this[14] -
			this[0] * this[10] * this[13] -
			this[8] * this[1] * this[14] +
			this[8] * this[2] * this[13] +
			this[12] * this[1] * this[10] -
			this[12] * this[2] * this[9];

		result[2] = this[1] * this[6] * this[15] -
			this[1] * this[7] * this[14] -
			this[5] * this[2] * this[15] +
			this[5] * this[3] * this[14] +
			this[13] * this[2] * this[7] -
			this[13] * this[3] * this[6];

		result[6] = -this[0] * this[6] * this[15] +
			this[0] * this[7] * this[14] +
			this[4] * this[2] * this[15] -
			this[4] * this[3] * this[14] -
			this[12] * this[2] * this[7] +
			this[12] * this[3] * this[6];

		result[10] = this[0] * this[5] * this[15] -
			this[0] * this[7] * this[13] -
			this[4] * this[1] * this[15] +
			this[4] * this[3] * this[13] +
			this[12] * this[1] * this[7] -
			this[12] * this[3] * this[5];

		result[14] = -this[0] * this[5] * this[14] +
			this[0] * this[6] * this[13] +
			this[4] * this[1] * this[14] -
			this[4] * this[2] * this[13] -
			this[12] * this[1] * this[6] +
			this[12] * this[2] * this[5];

		result[3] = -this[1] * this[6] * this[11] +
			this[1] * this[7] * this[10] +
			this[5] * this[2] * this[11] -
			this[5] * this[3] * this[10] -
			this[9] * this[2] * this[7] +
			this[9] * this[3] * this[6];

		result[7] = this[0] * this[6] * this[11] -
			this[0] * this[7] * this[10] -
			this[4] * this[2] * this[11] +
			this[4] * this[3] * this[10] +
			this[8] * this[2] * this[7] -
			this[8] * this[3] * this[6];

		result[11] = -this[0] * this[5] * this[11] +
			this[0] * this[7] * this[9] +
			this[4] * this[1] * this[11] -
			this[4] * this[3] * this[9] -
			this[8] * this[1] * this[7] +
			this[8] * this[3] * this[5];

		result[15] = this[0] * this[5] * this[10] -
			this[0] * this[6] * this[9] -
			this[4] * this[1] * this[10] +
			this[4] * this[2] * this[9] +
			this[8] * this[1] * this[6] -
			this[8] * this[2] * this[5];

		double det = this[0] * result[0] + this[1] * result[4] + this[2] * result[8] + this[3] * result[12];

		if (det == 0)
			return new Matrix4d();

		det = 1.0 / det;

		for (int i = 0; i < 16; i++)
			result[i] *= det;

		return result;
	}
	public Matrix4d Transpose() {
		Matrix4d result = new Matrix4d {
			m00 = m00,
			m01 = m10,
			m02 = m20,
			m03 = m30,
			m10 = m01,
			m11 = m11,
			m12 = m21,
			m13 = m31,
			m20 = m02,
			m21 = m12,
			m22 = m22,
			m23 = m32,
			m30 = m03,
			m31 = m13,
			m32 = m23,
			m33 = m33
		};
		return result;
	}

	public static Matrix4d Identity() {
		return new Matrix4d() {
			m00 = 1,
			m11 = 1,
			m22 = 1,
			m33 = 1
		};
	}
}

public class Matrix5d {
	public Matrix4d Transformation;
	public Vector4d Translation;
	public Matrix5d() {
		Transformation = Matrix4d.Identity();
		Translation = new Vector4d();
	}
	public Matrix5d(Matrix4d _Transformation, Vector4d _Translation) {
		Transformation = _Transformation;
		Translation = _Translation;
	}
	public Matrix5d(Matrix4d _Transformation) {
		Transformation = _Transformation;
		Translation = new Vector4d();
	}
	public Matrix5d(Matrix5d other) {
		Transformation = other.Transformation;
		Translation = other.Translation;
	}

	public Matrix5d Inverse() {
		Matrix4d inv = Transformation.Inverse();
		return new Matrix5d(inv, -(inv * Translation));
	}
	public Plane InvTransposePlaneMultiplication(Plane p) {
		Vector4d O = this * (p.Normal.Normalized() * p.Distance);
		Vector4d N = (Transformation.Inverse().Transpose() * p.Normal).Normalized();
		double d = O * N;
		return new Plane(N, d);
	}
	public Vector4d MultiplyDirection(Vector4d d) {
		return Transformation.Inverse().Transpose() * d;
	}
	public static Matrix5d operator *(Matrix5d lhs, Matrix5d rhs) {
		Matrix5d ret = new Matrix5d();
		ret.Transformation = lhs.Transformation * rhs.Transformation;
		ret.Translation = lhs.Translation + lhs.Transformation * rhs.Translation;
		return ret;
	}
	public static Vector4d operator *(Matrix5d lhs, Vector4d rhs) {
		return lhs.Translation + lhs.Transformation * rhs;
	}
}

public class Relativity_Controller : MonoBehaviour {
	public double deltaT = 0.5;
	public GameObject Observer;
	public Vector3d InitialVelocity; //Velocity relative to coordinate frame
	public double ProperTimeOffset;
	public List<Vector3d> ProperAccelerations;
	public List<double> AccelerationDurations;
	public List<Vector3d> AccelerationOffsets;
	public Vector4d CurrentEvent;
	public double ProperTime;

	private Relativity_Observer observerScript;
	private GameObject plane;
	private GameObject sphere;

	void Start() {
		observerScript = Observer.GetComponent<Relativity_Observer>();
		//plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
		//plane.name = name + " Simultaneity Plane";
		sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		sphere.name = name + " Observer Representation";
	}

	void FixedUpdate() {
		if (deltaT <= 0.01) {
			deltaT = 0.01;
		}
		double observerTime = observerScript.CoordinateTime;
		DrawWorldline();
		//CurrentEvent = GetState(observerTime);
		/*
		float di = 1f;
		Vector3 A = observerScript.accelerations[0];
		for (float i = 0; i + di < Math.Sinh(observerScript.durations[0] * A.magnitude) / A.magnitude; i += di) {
			Vector3 d1 = A * (-1 + Mathf.Sqrt(1 + i * i * A.sqrMagnitude)) / A.sqrMagnitude;
			Vector3 d2 = A * (-1 + Mathf.Sqrt(1 + (i + di) * (i + di) * A.sqrMagnitude)) / A.sqrMagnitude;
			d1.z += i;
			d2.z += i + di;
			Debug.DrawLine(d1, d2);
		}
		*/
	}

	Vector4d GetState(double observerTime) {
		Vector4d localObserverEvent = new Vector4d();
		Vector3d observerVelocity = new Vector3d(observerScript.velocity);
		Matrix5d observerToCoordinateBoost = new Matrix5d(LorentzBoost(observerVelocity), new Vector4d(0, -new Vector3d(Observer.transform.position))).Inverse();

		double calculatedTime = 0;
		if (observerTime > 0) {
			for (int i = 0; i < observerScript.accelerations.Count; i++) {
				Vector3d A = new Vector3d(observerScript.accelerations[i]);
				double T = observerScript.durations[i];
				bool lastAccel = false;
				if (observerTime < localObserverEvent.t + T) {
					T = observerTime - localObserverEvent.t;
					lastAccel = true;
				}
				localObserverEvent = new Vector4d(T, new Vector3d());
				double aSqr = A.SqrMagnitude();
				double a = A.Magnitude();
				double t = Math.Sinh(T * a) / a;
				Vector3d velocity = A * t / Math.Sqrt(1 + t * t * aSqr);
				Vector3d displacement = A * (Math.Sqrt(1 + t * t * aSqr) - 1) / aSqr;
				Matrix5d M = new Matrix5d(LorentzBoost(velocity), new Vector4d(-t, displacement));
				Matrix5d MInv = M.Inverse();
				observerToCoordinateBoost *= MInv;
				calculatedTime += T;
				if (lastAccel) break;
			}
		}
		localObserverEvent = new Vector4d(observerTime - calculatedTime, new Vector3d());

		Matrix5d coordinateToObjectStartBoost = new Matrix5d(LorentzBoost(InitialVelocity), new Vector4d(0, -new Vector3d(transform.position)));
		Matrix5d observerToObjectStartBoost = coordinateToObjectStartBoost * observerToCoordinateBoost;


		Plane simulPlane = new Plane {
			Distance = observerTime - calculatedTime
		};
		Vector4d coordinateObserverEvent = observerToObjectStartBoost * localObserverEvent;
		Vector3d coe = coordinateObserverEvent.Space();
		coe.z += coordinateObserverEvent.t;
		Debug.DrawRay(coe.Vector3(), Vector3.up, Color.white);
		simulPlane = observerToObjectStartBoost.InvTransposePlaneMultiplication(simulPlane);
		Vector4d normal4 = simulPlane.Normal.Normalized();
		Vector3d normal3 = normal4.Space();
		normal3.z += normal4.t;
		/*
		plane.transform.position = coe.Vector3();

		plane.transform.LookAt(coe.Vector3() - normal3.Vector3(), Vector3.up);
		plane.transform.Rotate(90f, 0, 0, Space.Self);
		*/
		double d = simulPlane.Distance / normal4.t;
		Debug.DrawRay(new Vector3(0, 0, (float)d), Vector3.up, Color.red);
		Vector4d eventInObjectFrame = new Vector4d(d, new Vector3d());
		Vector4d eventInObserverFrame = observerToObjectStartBoost.Inverse() * eventInObjectFrame;

		Vector3d drawInObserverFrame = eventInObserverFrame.Space();
		drawInObserverFrame.z += eventInObserverFrame.t + calculatedTime;

		sphere.transform.position = drawInObserverFrame.Vector3();
		Debug.DrawRay(drawInObserverFrame.Vector3(), Vector3.up, Color.white, 10);

		/*
		double di = 10;
		for (double i = -10; i < 10; i += di) {
			Vector4d e1 = observerToObjectStartBoost * new Vector4d(i + observerTime - calculatedTime, new Vector3d());
			Vector4d e2 = observerToObjectStartBoost * new Vector4d(i + di + observerTime - calculatedTime, new Vector3d());
			Vector3d v1 = e1.Space();
			v1.z += e1.t;
			Vector3d v2 = e2.Space();
			v2.z += e2.t;
			Debug.DrawLine(v1.Vector3(), v2.Vector3());
		}
		*/
		return new Vector4d();
	}

	void DrawWorldline() {
		double maxT = 10;
		/*
		for (double properTime = 0; properTime < 20.0; properTime += dT) {
			Matrix5d objectToCoordinateBoost = new Matrix5d(LorentzBoost(InitialVelocity), new Vector4d(0.0, -new Vector3d(transform.position))).Inverse();
			double calculatedTime = 0.0;
			for (int i=0; i<ProperAccelerations.Count; i++) {
				Vector3d A = ProperAccelerations[i];
				double T = AccelerationDurations[i];
				bool lastAccel = false;
				if (properTime - calculatedTime <= AccelerationDurations[i]) {
					T = properTime - calculatedTime;
					lastAccel = true;
				}
				double aSqr = A.SqrMagnitude();
				double a = A.Magnitude();
				double t = Math.Sinh(T * a) / a;
				Vector3d velocity = A * t / Math.Sqrt(1 + t * t * aSqr);
				Vector3d displacement = A * (Math.Sqrt(1 + t * t * aSqr) - 1) / aSqr;
				Matrix5d M = new Matrix5d(LorentzBoost(velocity), new Vector4d(-t, displacement));
				Matrix5d MInv = M.Inverse();
				objectToCoordinateBoost *= MInv;
				calculatedTime += T;
				if (lastAccel) break;
			}
			Vector4d localObjectEvent = new Vector4d(properTime - calculatedTime, new Vector3d());

			Matrix5d coordinateToObserverStartBoost = new Matrix5d(LorentzBoost(-new Vector3d(observerScript.velocity)), new Vector4d(0, new Vector3d(Observer.transform.position))).Inverse();
			Matrix5d objectToObserverBoost = coordinateToObserverStartBoost * objectToCoordinateBoost;
			Vector4d currentObjectEvent = objectToObserverBoost * localObjectEvent;
			if (currentObjectEvent.t > 0) {
				for (int i = 0; i < observerScript.accelerations.Count; i++) {
					Vector3d A = new Vector3d(observerScript.accelerations[i]);
					Vector3d X = currentObjectEvent.Space();
					double t = currentObjectEvent.t;
					double aSqr = A.SqrMagnitude();
					double a = Math.Sqrt(aSqr);
					double t2 = 0;
					if (A*A < Math.Pow(1 + A*X, 2)/(t*t)) {
						if (t != 0) {
							t2 = Math.Abs(t) / Math.Sqrt(-t * t * aSqr + Math.Pow(1 + A * X, 2));
							if ((A * X > -1 && t < 0) || (A * X < -1 && t > 0)) {
								t2 = -t2;
							}
						}
						Vector3d velocity = A * t2 / Math.Sqrt(1 + t2 * t2 * aSqr);
						Vector3d displacement = A * (Math.Sqrt(1 + t2 * t2 * aSqr) - 1) / aSqr;
						double T = ASinh(t2 * a) / a;
						Matrix5d M = new Matrix5d(LorentzBoost(velocity), new Vector4d(-t2+T, displacement));
						//Matrix5d MInv = M.Inverse();
						objectToObserverBoost = M * objectToObserverBoost;
					} else {
						// Rindler Horizon
						// Debug.Log("Horizon " + A.x + " " + A.y + " " + A.z + " | " + X.x + " " + X.y + " " + X.z + " | " + t);
					}
				}
			}
			currentObjectEvent = objectToObserverBoost * localObjectEvent;
			Vector3d drawInObserverFrame = currentObjectEvent.Space();
			drawInObserverFrame.z += currentObjectEvent.t;
			Debug.DrawRay(drawInObserverFrame.Vector3(), Vector3.up, Color.green);
		}
		*/
		bool first = true;
		for (double observerTime = 0; observerTime < maxT; observerTime += deltaT) {
			Vector3d observerVelocity = new Vector3d(observerScript.velocity);
			Matrix5d observerToCoordinateBoost = new Matrix5d(LorentzBoost(observerVelocity), new Vector4d(0, -new Vector3d(Observer.transform.position))).Inverse();
			Vector4d localObserver = new Vector4d {
				t = observerTime
			};
			Vector4d coordObserver = observerToCoordinateBoost * localObserver;
			Vector3d coordObserverDraw = coordObserver.Space();
			coordObserverDraw.z = coordObserver.t;
			//Debug.DrawRay(coordObserverDraw.Vector3(), Vector3.up, Color.cyan);
			double calculatedTime = 0;
			Vector4d prevTr = new Vector4d();
			for (int i = 0; i < observerScript.accelerations.Count; i++) {
				Vector3d A = new Vector3d(observerScript.accelerations[i]);
				double T = observerScript.durations[i];
				bool lastAccel = false;
				if (observerTime < calculatedTime + T) {
					T = observerTime - calculatedTime;
					lastAccel = true;
				}
				
				double aSqr = A.SqrMagnitude();
				double a = A.Magnitude();
				double t = Math.Sinh(T * a) / a;
				Vector3d velocity = A * t / Math.Sqrt(1 + t * t * aSqr);
				Vector3d displacement = A * (Math.Sqrt(1 + t * t * aSqr) - 1) / aSqr;

				//Vector4d newTr = new Vector4d(, new Vector3d());
				//Matrix5d Tr = new Matrix5d(Matrix4d.Identity(), prevTr);
				Matrix5d M = new Matrix5d(LorentzBoost(velocity), new Vector4d(-t + T, displacement));
				observerToCoordinateBoost *= new Matrix5d(Matrix4d.Identity(), prevTr) * M.Inverse();
				prevTr = new Vector4d(T, new Vector3d());
				//observerToCoordinateBoost *= MInv * Tr;// * observerToCoordinateBoost;
												  //Debug.Log(simulPlane2.Normal.Space() * displacement + simulPlane2.Normal.t * t - simulPlane2.Distance);

				//observerToCoordinateBoost *= new Matrix5d(Matrix4d.Identity(), new Vector4d(-prevObserverTime, new Vector3d { x = observerTime }));
				localObserver = new Vector4d {
					t = observerTime - calculatedTime
				};
				calculatedTime += T;
				
				if (lastAccel) break;
			}
			//localObserverEvent = new Vector4d(observerTime - calculatedTime, new Vector3d());
			Matrix5d coordinateToObjectBoost = new Matrix5d(LorentzBoost(InitialVelocity), new Vector4d(0, -new Vector3d(transform.position)));
			Matrix5d observerToObjectBoost = coordinateToObjectBoost * observerToCoordinateBoost;

			/*
			Vector4d right = observerToObjectBoost.MultiplyDirection(new Vector4d { x = 1 });
			Vector4d up = observerToObjectBoost.MultiplyDirection(new Vector4d { y = 1 });
			Vector4d forward = observerToObjectBoost.MultiplyDirection(new Vector4d { t = 1 });
			*/
			Vector4d right = observerToObjectBoost.Inverse().MultiplyDirection(new Vector4d { x = 1 });
			Vector4d up = observerToObjectBoost.Inverse().MultiplyDirection(new Vector4d { y = 1 });
			Vector4d forward = observerToObjectBoost.Inverse().MultiplyDirection(new Vector4d { t = 1 });
			Vector4d origin = observerToObjectBoost * localObserver;

			Vector3d rightDraw = right.Space();
			rightDraw.z = right.t;
			Vector3d upDraw = up.Space();
			upDraw.z = up.t;
			Vector3d forwardDraw = forward.Space();
			forwardDraw.z = forward.t;
			Vector3d originDraw = origin.Space();
			originDraw.z = origin.t;

			Debug.DrawRay(originDraw.Vector3(), rightDraw.Vector3() * (float)deltaT, Color.red);
			Debug.DrawRay(originDraw.Vector3(), upDraw.Vector3() * (float)deltaT, Color.green);
			Debug.DrawRay(originDraw.Vector3(), forwardDraw.Vector3() * (float)deltaT, Color.blue);
			Debug.DrawRay(originDraw.Vector3(), -rightDraw.Vector3() * (float)deltaT, Color.red);
			Debug.DrawRay(originDraw.Vector3(), -upDraw.Vector3() * (float)deltaT, Color.green);
			Debug.DrawRay(originDraw.Vector3(), -forwardDraw.Vector3() * (float)deltaT, Color.blue);



			/*
			double properTime = 0;
			calculatedTime = 0;
			int accelIndex = 0;
			for (accelIndex = 0; accelIndex < ProperAccelerations.Count; accelIndex++) {
				Vector3d A = ProperAccelerations[accelIndex];
				Vector4d N = simulPlane.Normal.Normalized();
				Vector3d nX = N.Space();
				double nXSqr = nX.SqrMagnitude();
				double nT = N.t;
				double nT2 = nT * nT;
				double d = simulPlane.Distance;
				double d2 = d * d;
				double aSqr = A.SqrMagnitude();
				double a = Math.Sqrt(aSqr);
				double AnX = A * nX;
				double AnX2 = AnX * AnX;
				double t2;
				//Find intersection time t2 between simulPlane and worldline of particle under proper acceleration A:
				if (d == 0 && nT == 0 && AnX != 0 && nXSqr >= 0 && aSqr > 0) {
					t2 = 0;
				} else if (AnX == 0 && d >= 0 && nT != 0 && nXSqr >= 0 && aSqr > 0) {
					t2 = d / nT;
				} else if (aSqr == AnX2 / nT2 && nXSqr >= 0 && nT != 0 && ((d >= 0 && ((nT2 + 2 * d * AnX >= 0 && AnX < 0) || AnX > 0)) || (nT2 + d * AnX > 0 && AnX < 0 && nT2 + 2 * d * AnX < 0))) {
					t2 = 1 / (nT * (1 / d + 1 / (d + (2 * AnX) / aSqr)));
				} else if (nXSqr >= 0 && AnX > 0 && nT == 0 && d > 0 && aSqr > 0) {
					t2 = -(Math.Sqrt(d * (d * aSqr + 2 * AnX)) / AnX);
				} else if (nXSqr >= 0 && AnX > 0 && nT == 0 && d > 0 && aSqr > 0) {
					t2 = Math.Sqrt((d * (d * aSqr + 2 * AnX)) / AnX2);
				} else if (nXSqr >= 0 && ((AnX < 0 && ((nT2 + 2 * d * AnX < 0 && ((nT2 + d * AnX > 0 && nT != 0 && (nT2 + d2 * aSqr + 2 * d * AnX == 0 || (nT2 + d2 * aSqr + 2 * d * AnX > 0 && nT2 * aSqr < AnX2))) || (nT2 * aSqr > AnX2 && nT < 0))) || (d >= 0 && ((nT < 0 && ((aSqr > 0 && nT2 + 2 * d * AnX >= 0 && nT2 * aSqr < AnX2) || nT2 * aSqr > AnX2)) || (nT > 0 && aSqr > 0 && nT2 + 2 * d * AnX >= 0 && nT2 * aSqr < AnX2))) || (nT < 0 && nT2 + d * AnX <= 0 && nT2 * aSqr > AnX2))) || (d >= 0 && AnX > 0 && ((nT != 0 && aSqr > 0 && nT2 * aSqr < AnX2) || (nT > 0 && nT2 * aSqr > AnX2))))) {
					t2 = -((Math.Abs(AnX) * Math.Sqrt(nT2 + d2 * aSqr + 2 * d * AnX)) / Math.Abs((-nT2) * aSqr + AnX2)) + (nT * (d * aSqr + AnX)) / (nT2 * aSqr - AnX2);
				} else if (nXSqr >= 0 && ((nT != 0 && nT2 * aSqr < AnX2 && ((aSqr > 0 && d >= 0 && ((nT2 + 2 * d * AnX >= 0 && AnX < 0) || AnX > 0)) || (AnX < 0 && nT2 + 2 * d * AnX < 0 && nT2 + d * AnX > 0 && nT2 + d2 * aSqr + 2 * d * AnX > 0))) || (nT2 * aSqr > AnX2 && ((d >= 0 && ((nT < 0 && AnX > 0) || (nT > 0 && AnX < 0))) || (nT > 0 && AnX < 0 && (nT2 + 2 * d * AnX < 0 || nT2 + d * AnX <= 0)))))) {
					t2 = (Math.Abs(AnX) * Math.Sqrt(nT2 + d2 * aSqr + 2 * d * AnX)) / Math.Abs((-nT2) * aSqr + AnX2) + (nT * (d * aSqr + AnX)) / (nT2 * aSqr - AnX2);
				} else {
					break;
				}
				properTime = ASinh(t2 * a) / a;
				if (0 < properTime && properTime <= AccelerationDurations[accelIndex]) {
					Vector3d velocity = A * t2 / Math.Sqrt(1 + t2 * t2 * aSqr);
					Vector3d displacement = A * (Math.Sqrt(1 + t2 * t2 * aSqr) - 1) / aSqr;
					Matrix5d M = new Matrix5d(LorentzBoost(velocity), new Vector4d(-t2 + properTime, displacement));
					observerToObjectBoost = M * observerToObjectBoost;
					break;
				} else if (0 < properTime) {
					properTime = AccelerationDurations[accelIndex];
					t2 = Math.Sinh(properTime * a) / a;
					calculatedTime += t2;
					Vector3d velocity = A * t2 / Math.Sqrt(1 + t2 * t2 * aSqr);
					Vector3d displacement = A * (Math.Sqrt(1 + t2 * t2 * aSqr) - 1) / aSqr;

					Matrix5d M = new Matrix5d(LorentzBoost(velocity), new Vector4d(-t2, displacement));

					observerToObjectBoost = M * observerToObjectBoost;
					//simulPlane = M.InvTransposePlaneMultiplication(simulPlane);
					
					simulPlane = new Plane {
						Distance = observerTime - calculatedTime
					};
					simulPlane = observerToObjectBoost.InvTransposePlaneMultiplication(simulPlane);
				} else {
					break;
				}
			}
			*/




			Vector4d objObserver = observerToObjectBoost * localObserver;
			Vector3d objObserverDraw = objObserver.Space();
			objObserverDraw.z = objObserver.t;
			Debug.DrawRay(objObserverDraw.Vector3(), Vector3.up, Color.HSVToRGB((float)(observerTime / maxT), 1, 1));
			/*
			Plane simulPlane = new Plane {
				Distance = observerTime
			};
			simulPlane = observerToObjectBoost.InvTransposePlaneMultiplication(simulPlane);
			Vector4d planeRay = simulPlane.Normal * simulPlane.Distance;
			Vector3d planeDraw = planeRay.Space();
			planeDraw.z += planeRay.t;

			Debug.DrawRay(planeDraw.Vector3(), 100 * Vector3.Cross(planeDraw.Vector3(), Vector3.up), Color.HSVToRGB((float)(observerTime / maxT), 1, 1));
			Debug.DrawRay(planeDraw.Vector3(), -100 * Vector3.Cross(planeDraw.Vector3(), Vector3.up), Color.HSVToRGB((float)(observerTime / maxT), 1, 1));

			double simulPlaneT = simulPlane.Normal.Normalized().t;
			Vector4d localObjectEvent = new Vector4d {
				t = simulPlane.Distance / simulPlaneT
			};
			Vector3d localObjectDraw = localObjectEvent.Space();
			localObjectDraw.z += localObjectEvent.t;
			Debug.DrawRay(localObjectDraw.Vector3(), -Vector3.up, Color.HSVToRGB((float)(observerTime / maxT), 1, 1));
			//Debug.DrawLine(localObjectDraw.Vector3(), objObserverDraw.Vector3(), Color.HSVToRGB((float)(observerTime / maxT), 1, 1));

			Vector4d obsObject = observerToObjectBoost.Inverse() * localObjectEvent;
			Vector3d obsObjectDraw = obsObject.Space();
			obsObjectDraw.z += obsObject.t;
			Debug.DrawRay(obsObjectDraw.Vector3(), Vector3.up * 2);
			*/
		}
	}

	Vector3d VelocityToProper(Vector3d v) {
		return v / Math.Sqrt(1.0 - v.SqrMagnitude());
	}

	Vector3d ProperToVelocity(Vector3d v) {
		return v / Math.Sqrt(1.0 + v.SqrMagnitude());
	}

	Vector3d AddVelocity(Vector3d v, Vector3d u) {
		//Einstein Velocity Addition
		if (v.SqrMagnitude() == 0)
			return u;
		if (u.SqrMagnitude() == 0)
			return v;
		double gamma = Lorentz(v);
		return 1.0 / (1 + u * v) * (v + u * InvLorentz(v) + gamma / (1f + gamma) * (u * v) * v);
	}

	Vector3d AddProperVelocity(Vector3d v, Vector3d u) {
		double Bu = 1 / Math.Sqrt(1.0 + u.SqrMagnitude());
		double Bv = 1 / Math.Sqrt(1.0 + v.SqrMagnitude());
		return v + u + (Bv / (1 + Bv) * (v * u) + (1 - Bu) / Bu) * v;
	}

	double Lorentz(Vector3d v) {
		//Lorentz Factor
		return 1.0 / Math.Sqrt(1.0 - v.SqrMagnitude());
	}

	double InvLorentz(Vector3d v) {
		//Reciprocal Lorentz Factor
		return Math.Sqrt(1 - v.SqrMagnitude());
	}

	double ASinh(double z) {
		return Math.Log(z + Math.Sqrt(1 + z * z));
	}

	Matrix4d LorentzBoost(Vector3d v) {
		//Computes the Lorentz Boost for a given 3-velocity
		double βSqr = v.SqrMagnitude();
		if (βSqr == 0f) {
			return Matrix4d.Identity();
		}
		double βx = v.x;
		double βy = v.y;
		double βz = v.z;
		double gamma = Lorentz(v);
		Matrix4d boost = new Matrix4d(
			new Vector4d(gamma, -gamma * βx, -gamma * βy, -gamma * βz),
			new Vector4d(-βx * gamma, (gamma - 1) * (βx * βx) / (βSqr) + 1, (gamma - 1) * (βx * βy) / (βSqr), (gamma - 1) * (βx * βz) / (βSqr)),
			new Vector4d(-βy * gamma, (gamma - 1) * (βy * βx) / (βSqr), (gamma - 1) * (βy * βy) / (βSqr) + 1, (gamma - 1) * (βy * βz) / (βSqr)),
			new Vector4d(-βz * gamma, (gamma - 1) * (βz * βx) / (βSqr), (gamma - 1) * (βz * βy) / (βSqr), (gamma - 1) * (βz * βz) / (βSqr) + 1)
		);
		return boost;
	}
}