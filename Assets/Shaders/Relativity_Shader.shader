// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Relativity/Relativity_Shader" {
	Properties {
		_Tess ("Tessellation", Range(1,64)) = 4
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM

		#pragma surface surf Standard fullforwardshadows vertex:vert tessellate:tessFixed nolightmap
		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0
		#include "Tessellation.cginc"

		

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
			float _Coordinate_Time; //TODO: Remove
		float _Observer_Time;
		float _Proper_Time_Offset;
		fixed4 _Color;
			float4 _Proper_Velocity; //TODO: Remove
		float4 _Velocity;
			float4 _Observer_Proper_Velocity; //TODO: Remove
		float4 _Observer_Velocity;
		float4 _Observer_Position;
		float4 _accelerations[512];
		float _durations[512];
		float4 _accel_positions[512];

		float4 tessFixed()
        {
            return _Tess;
        }

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}

		float sqr_magnitude(float3 v)
		{
			return dot(v, v);
		}

		float alpha(float3 v){ 
			//Reciprocal Lorentz Factor
			return sqrt(1 - sqr_magnitude(v));
		}

		float get_temporal_component(float4 V)
		{
			return V.x;
		}

		float get_duration(float3 a,  float3 u_0,  float T)
		{
			return (dot(a, u_0)*cosh(length(a)*T) + length(a)*sqrt(1.0 + sqr_magnitude(u_0))*sinh(length(a)*T) - dot(a, u_0))/sqr_magnitude(a);
		}

		float get_proper_duration(float3 a,  float3 u_0,  float t)
		{
			return (sqrt(1.0/(1.0 + sqr_magnitude(a*t + u_0)))*sqrt(1.0 + sqr_magnitude(a*t + u_0))*log(dot(a, a*t + u_0) + length(a)*sqrt(1.0 + sqr_magnitude(a*t + u_0))) - sqrt(1.0/(1.0 + sqr_magnitude(u_0)))*sqrt(1.0 + sqr_magnitude(u_0))*log(dot(a, u_0) + length(a)*sqrt(1.0 + sqr_magnitude(u_0))))/length(a);
		}

		float get_observer_duration(float3 a, float3 u, float3 v, float t)
		{
			//Given proper acceleration a, initial proper velocity u, observer velocity v, and coordinate duration t, solve for observer duration.
			return (t*pow(sqr_magnitude(a),3.0/2)+sqrt(sqr_magnitude(a))*dot(a,v)*sqrt(1+sqr_magnitude(u))-sqrt(sqr_magnitude(a))*dot(a,v)*sqrt(1+pow(t,2)*sqr_magnitude(a)+2*t*dot(a,u)+sqr_magnitude(u))-(dot(a,u)*dot(a,v)-sqr_magnitude(a)*dot(u,v))*(log(dot(a,u)+sqrt(sqr_magnitude(a))*sqrt(1+sqr_magnitude(u)))-log(t*sqr_magnitude(a)+dot(a,u)+sqrt(sqr_magnitude(a))*sqrt(1+pow(t,2)*sqr_magnitude(a)+2*t*dot(a,u)+sqr_magnitude(u)))))/(pow(sqr_magnitude(a),3.0/2)*sqrt(1-sqr_magnitude(v)));
		}

		float3 add_velocity(float3 v, float3 u){
			//Einstein Velocity Addition
			if (sqr_magnitude(v) == 0)
				return u;
			if (sqr_magnitude(u) == 0)
				return v;
			return 1.0/(1 + dot(v, u))*(u*alpha(v) + v + (1 - alpha(v))*dot(v, u)/sqr_magnitude(v)*v);
		}

		float3 get_displacement(float3 a, float3 u_0, float t)
		{
			return (a*length(a)*(sqrt(1.0 + sqr_magnitude(a*t + u_0)) - sqrt(1.0 + sqr_magnitude(u_0))) - cross(cross(u_0, a), - a)*(log(dot(a, u_0) + length(a)*sqrt(1.0 + sqr_magnitude(u_0))) - log(sqr_magnitude(a)*t + dot(a, u_0) + length(a)*sqrt(1.0 + sqr_magnitude(a*t + u_0)))))/pow(length(a), 3);
		}

		float3 get_spatial_component(float4 V)
		{
			return float3(V.y,V.z,V.w);
		}

		float4 combine_temporal_and_spatial(float t, float3 p)
		{
			return float4(t, p);
		}

		float4x4 lorentz_boost(float3 v)
		{
			if (dot(v, v) == 0.0)
			{
				return float4x4(
					1, 0, 0, 0,
					0, 1, 0, 0,
					0, 0, 1, 0,
					0, 0, 0, 1
				);
			}

			float gamma = 1.0/sqrt(1.0 - dot(v, v));

			float4x4 boost = float4x4(
				gamma,       -v.x*gamma,                             -v.y*gamma,                             -v.z*gamma,
				-v.x*gamma,  (gamma-1.0)*(v.x*v.x)/(dot(v, v)) + 1,  (gamma-1.0)*(v.x*v.y)/(dot(v, v)),      (gamma-1.0)*(v.x*v.z)/(dot(v, v)),
				-v.y*gamma,  (gamma-1.0)*(v.y*v.x)/(dot(v, v)),      (gamma-1.0)*(v.y*v.y)/(dot(v, v)) + 1,  (gamma-1.0)*(v.y*v.z)/(dot(v, v)),
				-v.z*gamma,  (gamma-1.0)*(v.z*v.x)/(dot(v, v)),      (gamma-1.0)*(v.z*v.y)/(dot(v, v)),      (gamma-1.0)*(v.z*v.z)/(dot(v, v)) + 1
			);

			return boost;
		}

		void vert (inout appdata v)
		{
			float4x4 object_coordinate_boost = lorentz_boost(-_Velocity.xyz); //Boost from object frame to coordinate frame
			float4x4 coordinate_observer_boost = lorentz_boost(_Observer_Velocity.xyz); //Boost from coordinate frame to observer frame
			float3 vertex_world_position = mul(unity_ObjectToWorld, v.vertex).xyz;
			float3 object_world_position = mul(unity_ObjectToWorld, float4(0,0,0,1));
			float4 starting_event_object = combine_temporal_and_spatial(-_Proper_Time_Offset, vertex_world_position); //Object will always be at (T,0,0,0) in its own frame.
			float4 current_event_coordinate = mul(object_coordinate_boost, starting_event_object);
			float4 current_event_observer = mul(coordinate_observer_boost, current_event_coordinate);
			float3 current_object_velocity = _Velocity.xyz;
			float3 proper_object_velocity = current_object_velocity / alpha(current_object_velocity);
			float3 observer_object_velocity = add_velocity(-_Observer_Velocity.xyz, current_object_velocity);
			float proper_time = 0;
			for (int i=0; i<512; ++i){
				if (get_temporal_component(current_event_observer) < _Observer_Time){
					float3 proper_acceleration = _accelerations[i].xyz;
					float proper_duration = _durations[i];
					float a = length(proper_acceleration);
					if (a > 0){
						float3 offset = (object_world_position + _accel_positions[i].xyz) - vertex_world_position;
						float L = dot(offset, proper_acceleration)/a;
						if (L <= 1.0/a)
						{
							float b = 1.0/(1.0 - a*L);
							proper_acceleration *= b;
							proper_duration /= b;
						}else{
							proper_acceleration = float3(0,0,0);
							proper_duration = 0;
						}
						float coordinate_duration = get_duration(proper_acceleration, proper_object_velocity, proper_duration);
						float3 coordinate_displacement = get_displacement(proper_acceleration, proper_object_velocity, coordinate_duration);
						float4 next_event_coordinate = current_event_coordinate + combine_temporal_and_spatial(coordinate_duration, coordinate_displacement);
						float4 next_event_observer = mul(coordinate_observer_boost, next_event_coordinate);
						if (get_temporal_component(next_event_observer) > _Observer_Time){
							coordinate_duration = 0;
							float observer_duration = 0;
							for (int j=0; j<200; ++j){
								float prev_duration = coordinate_duration;
								float diff = _Observer_Time - get_temporal_component(current_event_observer) - observer_duration;
								coordinate_duration += diff / 2;
								float t = coordinate_duration;
								observer_duration = get_observer_duration(proper_acceleration, proper_object_velocity, _Observer_Velocity.xyz, coordinate_duration);
							}
							coordinate_displacement = get_displacement(proper_acceleration, proper_object_velocity, coordinate_duration);
							next_event_coordinate = current_event_coordinate + combine_temporal_and_spatial(coordinate_duration, coordinate_displacement);
							next_event_observer = mul(coordinate_observer_boost, next_event_coordinate);
						}
						proper_time += get_proper_duration(proper_acceleration, proper_object_velocity, coordinate_duration);
						proper_object_velocity += proper_acceleration*coordinate_duration;
						current_object_velocity = proper_object_velocity / sqrt(1.0 + sqr_magnitude(proper_object_velocity));
						current_event_coordinate += combine_temporal_and_spatial(coordinate_duration, coordinate_displacement);
						current_event_observer = mul(coordinate_observer_boost, current_event_coordinate);
						observer_object_velocity = add_velocity(-_Observer_Velocity.xyz, current_object_velocity);
					}else{
						float coordinate_duration = proper_duration / alpha(current_object_velocity);
						float3 coordinate_displacement = current_object_velocity * coordinate_duration;
						current_event_coordinate += combine_temporal_and_spatial(coordinate_duration, coordinate_displacement);
						current_event_observer = mul(coordinate_observer_boost, current_event_coordinate);
						observer_object_velocity = add_velocity(-_Observer_Velocity.xyz, current_object_velocity);
						proper_time += proper_duration;
					}
				}else{
					break;
				}
			}
			observer_object_velocity = add_velocity(-_Observer_Velocity.xyz, current_object_velocity);
			proper_time += (_Observer_Time - get_temporal_component(current_event_observer))*alpha(observer_object_velocity);
			current_event_observer += (_Observer_Time - get_temporal_component(current_event_observer))*combine_temporal_and_spatial(1, observer_object_velocity);
			v.vertex = mul(unity_WorldToObject, float4(get_spatial_component(current_event_observer), 1));
		}
		ENDCG
	}
	FallBack "Diffuse"
}
