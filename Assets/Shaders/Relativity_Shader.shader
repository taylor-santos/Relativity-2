// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Relativity/Relativity_Shader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		Tags { "ForceNoShadowCasting" = "True"}
		LOD 100
		
		CGPROGRAM

		//#pragma surface surf Standard fullforwardshadows vertex:vert tessellate:tessFixed nolightmap
		#pragma surface surf Standard fullforwardshadows vertex:vert nolightmap
		// Use shader model 3 target, to get nicer looking lighting
		//#pragma target 3
		//#include "Tessellation.cginc"
		#include "UnityCG.cginc"

		

		struct appdata {
			float4 vertex : POSITION;
			float4 tangent : TANGENT;
			float3 normal : NORMAL;
			float2 texcoord : TEXCOORD0;
			float4 color : COLOR;
		};

		struct Input {
			float2 uv_MainTex;
		};

		sampler2D _MainTex;
		half _EdgeLength;
        half _Tess;
		half _Glossiness;
		half _Metallic;
		float _Observer_Time;
		float _Proper_Time_Offset;
		fixed4 _Color;
		float4 _Velocity;
		float4 _Observer_Velocity;
		float4 _Observer_Position;
		float4 _accelerations[32];
		float _durations[32];
		float4 _accel_positions[32];
        
		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}

		float sqr_magnitude(float3 v);
		float gamma(float3 v);
		float alpha(float3 v);
		/*
		float sinh(float x);
		float cosh(float x);
		float tanh(float x);
		float atanh(float x);
		float acosh(float x);
		float asinh(float x);
		*/
		float get_observer_time(float3 a, float3 cV, float4x4 object_to_coordinate_boost, float4 current_event_coordinate, float MCRFTime);
		float get_MCRF_time(float3 a, float3 coordinate_velocity, float4x4 object_to_coordinate_boost, float4 current_event_coordinate, float observer_time);
		float get_temporal_component(float4 v);
		float3 velocity_to_proper(float3 v);
		float3 proper_to_velocity(float3 v);
		float3 add_velocity(float3 v, float3 u);
		float3 add_proper_velocity(float3 v, float3 u);
		float3 get_spatial_component(float4 v);
		float3 add_to_Z_axis(float3 v, float z);
		float3 boost_to_minkowski(float4 pt, float4x4 boost);
		float3 get_displacement(float3 a, float T);
		float4 combine_temporal_and_spatial(float t, float3 p);
		float4x4 lorentz_boost(float3 v);

		void vert (inout appdata v)
		{
			float3 coordinate_to_object_velocity = _Velocity;
			float3 observer_to_coordinate_velocity = -_Observer_Velocity;
			float4x4 object_to_coordinate_boost = lorentz_boost(-coordinate_to_object_velocity);
			float4x4 coordinate_to_observer_boost = lorentz_boost(-observer_to_coordinate_velocity); //Boost from coordinate frame to observer frame
			float3 vertex_world_position = mul(unity_ObjectToWorld, v.vertex).xyz;
			float3 object_world_position = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
			float4 current_event_object = combine_temporal_and_spatial(-_Proper_Time_Offset, vertex_world_position);
			float4 current_event_coordinate = mul(object_to_coordinate_boost, current_event_object) - combine_temporal_and_spatial(0, _Observer_Position);
			float4 current_event_observer = mul(coordinate_to_observer_boost, current_event_coordinate);
			float3 observer_to_object_velocity = add_velocity(observer_to_coordinate_velocity, coordinate_to_object_velocity);
			
			float3 velocities[32];
			int velocities_index = 0;
			//velocities[velocities_index++] = coordinate_to_object_velocity;
			velocities[velocities_index++] = velocity_to_proper(coordinate_to_object_velocity);
			for (int i=0; i<32; ++i){
				if (get_temporal_component(current_event_observer) < _Observer_Time){
					float3 proper_acceleration = _accelerations[i].xyz;
					float proper_duration = _durations[i];
					float a = length(proper_acceleration);
					float3 offset = (object_world_position + _accel_positions[i].xyz) - vertex_world_position;
					float L = dot(offset, proper_acceleration)/a;
					if (L <= 1/a)
					{
						float b = 1/(1 - a*L);
						proper_acceleration *= b;
						proper_duration /= b;
					}else{
						proper_acceleration = float3(0,0,0);
						proper_duration = 0;
					}
					if (length(proper_acceleration) > 0){
						float MCRF_duration = sinh(length(proper_acceleration) * proper_duration)/length(proper_acceleration);
						float4 next_event_object = combine_temporal_and_spatial(MCRF_duration, get_displacement(proper_acceleration, MCRF_duration));
						float4 next_event_coordinate = current_event_coordinate + mul(object_to_coordinate_boost, next_event_object);
						float4 next_event_observer = mul(coordinate_to_observer_boost, next_event_coordinate);
						if (get_temporal_component(next_event_observer) > _Observer_Time){
							MCRF_duration = get_MCRF_time(proper_acceleration, -observer_to_coordinate_velocity, object_to_coordinate_boost, current_event_coordinate, _Observer_Time);

							next_event_observer = current_event_observer;
							current_event_object = combine_temporal_and_spatial(MCRF_duration, get_displacement(proper_acceleration, MCRF_duration));
							current_event_coordinate = current_event_coordinate + mul(object_to_coordinate_boost, current_event_object);
							current_event_observer = mul(coordinate_to_observer_boost, current_event_coordinate);
							float3 added_velocity = proper_to_velocity(proper_acceleration*MCRF_duration);
							//velocities[velocities_index++] = added_velocity;
							velocities[velocities_index++] = proper_acceleration*MCRF_duration;
							object_to_coordinate_boost = mul(object_to_coordinate_boost, lorentz_boost(-added_velocity));
							break;
						}else{
							current_event_object = next_event_object;
							current_event_coordinate = next_event_coordinate;
							current_event_observer = next_event_observer;
							float3 added_velocity = proper_to_velocity(proper_acceleration*MCRF_duration);
							//velocities[velocities_index++] = added_velocity;
							velocities[velocities_index++] = proper_acceleration*MCRF_duration;
							object_to_coordinate_boost = mul(object_to_coordinate_boost, lorentz_boost(-added_velocity));
						}
					}else{
						
					}
				}else{
					break;
				}
			}
			coordinate_to_object_velocity = velocities[--velocities_index];
			velocities_index--;
			for (int j=velocities_index; j>=0; j--){
				coordinate_to_object_velocity = add_proper_velocity(velocities[j], coordinate_to_object_velocity);
			}
			coordinate_to_object_velocity = proper_to_velocity(coordinate_to_object_velocity);
			observer_to_object_velocity = add_velocity(observer_to_coordinate_velocity, coordinate_to_object_velocity);
			current_event_observer += (_Observer_Time - get_temporal_component(current_event_observer))*combine_temporal_and_spatial(1, observer_to_object_velocity);
			float4 relative_event_observer = current_event_observer + combine_temporal_and_spatial(0, _Observer_Position);
			v.vertex = mul(unity_WorldToObject, float4(get_spatial_component(relative_event_observer), 1));
		}

		float sqr_magnitude(float3 v){
			return dot(v, v);
		}

		float3 velocity_to_proper(float3 v){
			return v / sqrt(1 - sqr_magnitude(v));
		}

		float3 proper_to_velocity(float3 v){
			return v / sqrt(1 + sqr_magnitude(v));
		}

		float3 add_velocity(float3 v, float3 u){
			//Einstein Velocity Addition
			if (sqr_magnitude(v) == 0)
				return u;
			if (sqr_magnitude(u) == 0)
				return v;
			return 1/(1+dot(u, v))*(v + u*alpha(v) + gamma(v)/(1 + gamma(v))*dot(u, v)*v);
		}

		float3 add_proper_velocity(float3 v, float3 u){
			float Bu = 1/sqrt(1 + sqr_magnitude(u));
			float Bv = 1/sqrt(1 + sqr_magnitude(v));
			return v+u+(Bv/(1+Bv)*dot(v,u) + (1-Bu)/Bu)*v;
		}

		float gamma(float3 v){
			//Lorentz Factor
			return 1/sqrt(1 - sqr_magnitude(v));
		}

		float alpha(float3 v){ 
			//Reciprocal Lorentz Factor
			return sqrt(1 - sqr_magnitude(v));
		}
		/*
		float sinh(float x)
		{
			return (pow(exp(1), x) - pow(exp(1), -x))/2;
		}

		float cosh(float x)
		{
			return (pow(exp(1), x) + pow(exp(1), -x))/2;
		}

		float tanh(float x)
		{
			return sinh(x)/cosh(x);
		}

		float atanh(float x)
		{
			return (log(1 + x) - log(1 - x))/2;
		}

		float acosh(float x)
		{
			return log(x + sqrt(pow(x,2) - 1));
		}

		float asinh(float x)
		{
			return log(x + sqrt(1 + pow(x,2)));
		}
		*/
		float4x4 lorentz_boost(float3 v){
			if (dot(v, v) == 0)
			{
				return float4x4(
					1, 0, 0, 0,
					0, 1, 0, 0,
					0, 0, 1, 0,
					0, 0, 0, 1
				);
			}

			float gamma = 1/sqrt(1 - dot(v, v));

			float4x4 boost = float4x4(
				gamma,       -v.x*gamma,                             -v.y*gamma,                             -v.z*gamma,
				-v.x*gamma,  (gamma-1)*(v.x*v.x)/(dot(v, v)) + 1,  (gamma-1)*(v.x*v.y)/(dot(v, v)),      (gamma-1)*(v.x*v.z)/(dot(v, v)),
				-v.y*gamma,  (gamma-1)*(v.y*v.x)/(dot(v, v)),      (gamma-1)*(v.y*v.y)/(dot(v, v)) + 1,  (gamma-1)*(v.y*v.z)/(dot(v, v)),
				-v.z*gamma,  (gamma-1)*(v.z*v.x)/(dot(v, v)),      (gamma-1)*(v.z*v.y)/(dot(v, v)),      (gamma-1)*(v.z*v.z)/(dot(v, v)) + 1
			);

			return boost;
		}

		float4 combine_temporal_and_spatial(float t, float3 p){
			return float4(t, p.x, p.y, p.z);
		}

		float get_temporal_component(float4 v){
			return v.x;
		}

		float3 get_spatial_component(float4 v){
			return float3(v.y, v.z, v.w);
		}

		float3 add_to_Z_axis(float3 v, float z){
			//Add new z value to vector v's z-axis
			v.z += z;
			return v;
		}

		float3 boost_to_minkowski(float4 pt, float4x4 boost){
			//Applies a Lorentz boost to a (t,x,y,z) point, then converts it into (x,y,t) coordinates for Minkowski diagram rendering
			float4 new_pt = mul(boost, pt);
			return float3(new_pt.y, new_pt.z, new_pt.x);
		}

		float3 get_displacement(float3 a, float T){
			return
			(
				a
				*
				(
					sqrt(
						1
						+
						pow(T, 2)
						*
						sqr_magnitude(a)
					)
					-
					1
				)
			)
			/
			(
				sqr_magnitude(a)
			);
		}

		float get_observer_time(float3 a, float3 cV, float4x4 object_to_coordinate_boost, float4 current_event_coordinate, float MCRFTime){
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
			float B11 = object_to_coordinate_boost[0][0];
			float B12 = object_to_coordinate_boost[0][1];
			float B13 = object_to_coordinate_boost[0][2];
			float B14 = object_to_coordinate_boost[0][3];
			float B21 = object_to_coordinate_boost[1][0];
			float B22 = object_to_coordinate_boost[1][1];
			float B23 = object_to_coordinate_boost[1][2];
			float B24 = object_to_coordinate_boost[1][3];
			float B31 = object_to_coordinate_boost[2][0];
			float B32 = object_to_coordinate_boost[2][1];
			float B33 = object_to_coordinate_boost[2][2];
			float B34 = object_to_coordinate_boost[2][3];
			float B41 = object_to_coordinate_boost[3][0];
			float B42 = object_to_coordinate_boost[3][1];
			float B43 = object_to_coordinate_boost[3][2];
			float B44 = object_to_coordinate_boost[3][3];
			
			return
			(1/((pow(ax, 2) + pow(ay, 2) + pow(az, 2))*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))))*((-az)*B14 + pow(ax, 2)*(currCoordT - currCoordX*cVx - currCoordY*cVy - currCoordZ*cVz + B11*MCRFTime - (B21*cVx + B31*cVy + B41*cVz)*MCRFTime) + 
			pow(ay, 2)*(currCoordT - currCoordX*cVx - currCoordY*cVy - currCoordZ*cVz + B11*MCRFTime - (B21*cVx + B31*cVy + B41*cVz)*MCRFTime) + ax*(B12 - B22*cVx - B32*cVy - B42*cVz)*(-1 + sqrt(1 + (pow(ax, 2) + pow(ay, 2) + pow(az, 2))*pow(MCRFTime, 2))) + 
			ay*(B13 - B23*cVx - B33*cVy - B43*cVz)*(-1 + sqrt(1 + (pow(ax, 2) + pow(ay, 2) + pow(az, 2))*pow(MCRFTime, 2))) + 
			az*(B24*cVx + B34*cVy + B44*cVz + az*(currCoordT - currCoordX*cVx - currCoordY*cVy - currCoordZ*cVz + B11*MCRFTime - (B21*cVx + B31*cVy + B41*cVz)*MCRFTime) + B14*sqrt(1 + (pow(ax, 2) + pow(ay, 2) + pow(az, 2))*pow(MCRFTime, 2)) - 
			(B24*cVx + B34*cVy + B44*cVz)*sqrt(1 + (pow(ax, 2) + pow(ay, 2) + pow(az, 2))*pow(MCRFTime, 2))));
		}

		float get_MCRF_time(float3 a, float3 coordinate_velocity, float4x4 object_to_coordinate_boost, float4 current_event_coordinate, float observer_time){
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
			float B11 = object_to_coordinate_boost[0][0];
			float B12 = object_to_coordinate_boost[0][1];
			float B13 = object_to_coordinate_boost[0][2];
			float B14 = object_to_coordinate_boost[0][3];
			float B21 = object_to_coordinate_boost[1][0];
			float B22 = object_to_coordinate_boost[1][1];
			float B23 = object_to_coordinate_boost[1][2];
			float B24 = object_to_coordinate_boost[1][3];
			float B31 = object_to_coordinate_boost[2][0];
			float B32 = object_to_coordinate_boost[2][1];
			float B33 = object_to_coordinate_boost[2][2];
			float B34 = object_to_coordinate_boost[2][3];
			float B41 = object_to_coordinate_boost[3][0];
			float B42 = object_to_coordinate_boost[3][1];
			float B43 = object_to_coordinate_boost[3][2];
			float B44 = object_to_coordinate_boost[3][3];
			float t = observer_time;

			float T1 = (sqrt(pow(ax*(-B12 + B22*cVx + B32*cVy + B42*cVz) + ay*(-B13 + B23*cVx + B33*cVy + B43*cVz) + az*(-B14 + B24*cVx + B34*cVy + B44*cVz), 2)*(pow(B11, 2) - 2*az*B14*currCoordT + pow(az, 2)*pow(currCoordT, 2) + 2*az*B24*currCoordT*cVx + 
			           2*az*B14*currCoordX*cVx - 2*pow(az, 2)*currCoordT*currCoordX*cVx + pow(B21, 2)*pow(cVx, 2) - 2*az*B24*currCoordX*pow(cVx, 2) + pow(az, 2)*pow(currCoordX, 2)*pow(cVx, 2) + 2*az*B34*currCoordT*cVy + 2*az*B14*currCoordY*cVy - 2*pow(az, 2)*currCoordT*currCoordY*cVy + 
			           2*B21*B31*cVx*cVy - 2*az*B34*currCoordX*cVx*cVy - 2*az*B24*currCoordY*cVx*cVy + 2*pow(az, 2)*currCoordX*currCoordY*cVx*cVy + pow(B31, 2)*pow(cVy, 2) - 2*az*B34*currCoordY*pow(cVy, 2) + pow(az, 2)*pow(currCoordY, 2)*pow(cVy, 2) + 2*az*B44*currCoordT*cVz + 
			           2*az*B14*currCoordZ*cVz - 2*pow(az, 2)*currCoordT*currCoordZ*cVz + 2*B21*B41*cVx*cVz - 2*az*B44*currCoordX*cVx*cVz - 2*az*B24*currCoordZ*cVx*cVz + 2*pow(az, 2)*currCoordX*currCoordZ*cVx*cVz + 2*B31*B41*cVy*cVz - 
			           2*az*B44*currCoordY*cVy*cVz - 2*az*B34*currCoordZ*cVy*cVz + 2*pow(az, 2)*currCoordY*currCoordZ*cVy*cVz + pow(B41, 2)*pow(cVz, 2) - 2*az*B44*currCoordZ*pow(cVz, 2) + pow(az, 2)*pow(currCoordZ, 2)*pow(cVz, 2) - 2*B11*(B21*cVx + B31*cVy + B41*cVz) + 
			           2*ay*(B13 - B23*cVx - B33*cVy - B43*cVz)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz) + 2*ay*B13*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t + 2*az*B14*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t - 
			           2*pow(ay, 2)*currCoordT*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t - 2*pow(az, 2)*currCoordT*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t - 2*ay*B23*cVx*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t - 2*az*B24*cVx*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t + 
			           2*pow(ay, 2)*currCoordX*cVx*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t + 2*pow(az, 2)*currCoordX*cVx*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t - 2*ay*B33*cVy*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t - 2*az*B34*cVy*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t + 
			           2*pow(ay, 2)*currCoordY*cVy*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t + 2*pow(az, 2)*currCoordY*cVy*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t - 2*ay*B43*cVz*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t - 2*az*B44*cVz*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t + 
			           2*pow(ay, 2)*currCoordZ*cVz*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t + 2*pow(az, 2)*currCoordZ*cVz*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t + pow(az, 2)*pow(t, 2) - pow(az, 2)*pow(cVx, 2)*pow(t, 2) - pow(az, 2)*pow(cVy, 2)*pow(t, 2) - pow(az, 2)*pow(cVz, 2)*pow(t, 2) + 
			           2*ax*(B12 - B22*cVx - B32*cVy - B42*cVz)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t) + 
			           pow(ay, 2)*(pow(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz, 2) - (-1 + pow(cVx, 2) + pow(cVy, 2) + pow(cVz, 2))*pow(t, 2)) + pow(ax, 2)*(pow(currCoordT, 2) + pow(currCoordX, 2)*pow(cVx, 2) + pow(currCoordY, 2)*pow(cVy, 2) + 2*currCoordY*currCoordZ*cVy*cVz + 
			           pow(currCoordZ, 2)*pow(cVz, 2) + 2*currCoordY*cVy*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t + 2*currCoordZ*cVz*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t + pow(t, 2) - pow(cVx, 2)*pow(t, 2) - pow(cVy, 2)*pow(t, 2) - pow(cVz, 2)*pow(t, 2) + 
			           2*currCoordX*cVx*(currCoordY*cVy + currCoordZ*cVz + sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t) - 2*currCoordT*(currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t)))) + 
			           (B11 - B21*cVx - B31*cVy - B41*cVz)*(ax*(B12 - B22*cVx - B32*cVy - B42*cVz) + ay*(B13 - B23*cVx - B33*cVy - B43*cVz) + pow(ax, 2)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + 
			           sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t) + pow(ay, 2)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t) + 
			           az*(B14 - B24*cVx - B34*cVy - B44*cVz + az*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t))))/
			           (pow(ax, 2)*(B11 - B12 - B21*cVx + B22*cVx - B31*cVy + B32*cVy - B41*cVz + B42*cVz)*(B11 + B12 - (B21 + B22)*cVx - (B31 + B32)*cVy - (B41 + B42)*cVz) + 
			           pow(ay, 2)*(B11 - B13 - B21*cVx + B23*cVx - B31*cVy + B33*cVy - B41*cVz + B43*cVz)*(B11 + B13 - (B21 + B23)*cVx - (B31 + B33)*cVy - (B41 + B43)*cVz) + 
			           2*ay*az*(B13 - B23*cVx - B33*cVy - B43*cVz)*(-B14 + B24*cVx + B34*cVy + B44*cVz) + pow(az, 2)*(B11 - B14 - B21*cVx + B24*cVx - B31*cVy + B34*cVy - B41*cVz + B44*cVz)*
			           (B11 + B14 - (B21 + B24)*cVx - (B31 + B34)*cVy - (B41 + B44)*cVz) + 2*ax*(B12 - B22*cVx - B32*cVy - B42*cVz)*(ay*(-B13 + B23*cVx + B33*cVy + B43*cVz) + az*(-B14 + B24*cVx + B34*cVy + B44*cVz)));

			float T2 = (-sqrt(pow(ax*(-B12 + B22*cVx + B32*cVy + B42*cVz) + ay*(-B13 + B23*cVx + B33*cVy + B43*cVz) + az*(-B14 + B24*cVx + B34*cVy + B44*cVz), 2)*(pow(B11, 2) - 2*az*B14*currCoordT + pow(az, 2)*pow(currCoordT, 2) + 2*az*B24*currCoordT*cVx + 
			           2*az*B14*currCoordX*cVx - 2*pow(az, 2)*currCoordT*currCoordX*cVx + pow(B21, 2)*pow(cVx, 2) - 2*az*B24*currCoordX*pow(cVx, 2) + pow(az, 2)*pow(currCoordX, 2)*pow(cVx, 2) + 2*az*B34*currCoordT*cVy + 2*az*B14*currCoordY*cVy - 2*pow(az, 2)*currCoordT*currCoordY*cVy + 
			           2*B21*B31*cVx*cVy - 2*az*B34*currCoordX*cVx*cVy - 2*az*B24*currCoordY*cVx*cVy + 2*pow(az, 2)*currCoordX*currCoordY*cVx*cVy + pow(B31, 2)*pow(cVy, 2) - 2*az*B34*currCoordY*pow(cVy, 2) + pow(az, 2)*pow(currCoordY, 2)*pow(cVy, 2) + 2*az*B44*currCoordT*cVz + 
			           2*az*B14*currCoordZ*cVz - 2*pow(az, 2)*currCoordT*currCoordZ*cVz + 2*B21*B41*cVx*cVz - 2*az*B44*currCoordX*cVx*cVz - 2*az*B24*currCoordZ*cVx*cVz + 2*pow(az, 2)*currCoordX*currCoordZ*cVx*cVz + 2*B31*B41*cVy*cVz - 
			           2*az*B44*currCoordY*cVy*cVz - 2*az*B34*currCoordZ*cVy*cVz + 2*pow(az, 2)*currCoordY*currCoordZ*cVy*cVz + pow(B41, 2)*pow(cVz, 2) - 2*az*B44*currCoordZ*pow(cVz, 2) + pow(az, 2)*pow(currCoordZ, 2)*pow(cVz, 2) - 2*B11*(B21*cVx + B31*cVy + B41*cVz) + 
			           2*ay*(B13 - B23*cVx - B33*cVy - B43*cVz)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz) + 2*ay*B13*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t + 2*az*B14*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t - 
			           2*pow(ay, 2)*currCoordT*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t - 2*pow(az, 2)*currCoordT*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t - 2*ay*B23*cVx*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t - 2*az*B24*cVx*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t + 
			           2*pow(ay, 2)*currCoordX*cVx*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t + 2*pow(az, 2)*currCoordX*cVx*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t - 2*ay*B33*cVy*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t - 2*az*B34*cVy*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t + 
			           2*pow(ay, 2)*currCoordY*cVy*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t + 2*pow(az, 2)*currCoordY*cVy*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t - 2*ay*B43*cVz*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t - 2*az*B44*cVz*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t + 
			           2*pow(ay, 2)*currCoordZ*cVz*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t + 2*pow(az, 2)*currCoordZ*cVz*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t + pow(az, 2)*pow(t, 2) - pow(az, 2)*pow(cVx, 2)*pow(t, 2) - pow(az, 2)*pow(cVy, 2)*pow(t, 2) - pow(az, 2)*pow(cVz, 2)*pow(t, 2) + 
			           2*ax*(B12 - B22*cVx - B32*cVy - B42*cVz)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t) + 
			           pow(ay, 2)*(pow(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz, 2) - (-1 + pow(cVx, 2) + pow(cVy, 2) + pow(cVz, 2))*pow(t, 2)) + pow(ax, 2)*(pow(currCoordT, 2) + pow(currCoordX, 2)*pow(cVx, 2) + pow(currCoordY, 2)*pow(cVy, 2) + 2*currCoordY*currCoordZ*cVy*cVz + 
			           pow(currCoordZ, 2)*pow(cVz, 2) + 2*currCoordY*cVy*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t + 2*currCoordZ*cVz*sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t + pow(t, 2) - pow(cVx, 2)*pow(t, 2) - pow(cVy, 2)*pow(t, 2) - pow(cVz, 2)*pow(t, 2) + 
			           2*currCoordX*cVx*(currCoordY*cVy + currCoordZ*cVz + sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t) - 2*currCoordT*(currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t)))) + 
			           (B11 - B21*cVx - B31*cVy - B41*cVz)*(ax*(B12 - B22*cVx - B32*cVy - B42*cVz) + ay*(B13 - B23*cVx - B33*cVy - B43*cVz) + pow(ax, 2)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + 
			           sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t) + pow(ay, 2)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t) + 
			           az*(B14 - B24*cVx - B34*cVy - B44*cVz + az*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + sqrt(1 - pow(cVx, 2) - pow(cVy, 2) - pow(cVz, 2))*t))))/
			           (pow(ax, 2)*(B11 - B12 - B21*cVx + B22*cVx - B31*cVy + B32*cVy - B41*cVz + B42*cVz)*(B11 + B12 - (B21 + B22)*cVx - (B31 + B32)*cVy - (B41 + B42)*cVz) + 
			           pow(ay, 2)*(B11 - B13 - B21*cVx + B23*cVx - B31*cVy + B33*cVy - B41*cVz + B43*cVz)*(B11 + B13 - (B21 + B23)*cVx - (B31 + B33)*cVy - (B41 + B43)*cVz) + 
			           2*ay*az*(B13 - B23*cVx - B33*cVy - B43*cVz)*(-B14 + B24*cVx + B34*cVy + B44*cVz) + pow(az, 2)*(B11 - B14 - B21*cVx + B24*cVx - B31*cVy + B34*cVy - B41*cVz + B44*cVz)*
			           (B11 + B14 - (B21 + B24)*cVx - (B31 + B34)*cVy - (B41 + B44)*cVz) + 2*ax*(B12 - B22*cVx - B32*cVy - B42*cVz)*(ay*(-B13 + B23*cVx + B33*cVy + B43*cVz) + az*(-B14 + B24*cVx + B34*cVy + B44*cVz)));
			//As these are inverses of get_observer_time() (each valid under certain circumstances), feed the results back into get_observer_time() to find which is valid under these circumstances.
			float obs_time1 = get_observer_time(a, coordinate_velocity, object_to_coordinate_boost, current_event_coordinate, T1);
			float obs_time2 = get_observer_time(a, coordinate_velocity, object_to_coordinate_boost, current_event_coordinate, T2);
			if (abs(observer_time - obs_time2) > abs(observer_time - obs_time1)){
				return T1;
			}else{
				return T2;
			}
		}
		ENDCG
	}
	FallBack "Diffuse"
}
