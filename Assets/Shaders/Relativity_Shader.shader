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
		float _Coordinate_Time;
		float _Proper_Time_Offset;
		fixed4 _Color;
		float4 _Proper_Velocity;
		float4 _Observer_Proper_Velocity;
		float4 _Observer_Position;
		float4 _accelerations[100];
		float4 _accel_positions[100];

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

		float3 proper_vel_add(float3 u, float3 v)
		{
			float Bu = 1.0/sqrt(1.0 + sqr_magnitude(u));
			float Bv = 1.0/sqrt(1.0 + sqr_magnitude(v));
			return u + v + u*(Bu/(1.0 + Bu)*dot(u, v) + (1.0 - Bv)/Bv);
		}

		float3 get_displacement(float3 a, float3 u_0, float t)
		{
			return (a*length(a)*(sqrt(1.0 + sqr_magnitude(a*t + u_0)) - sqrt(1.0 + sqr_magnitude(u_0))) - cross(cross(u_0, a), - a)*(log(dot(a, u_0) + length(a)*sqrt(1.0 + sqr_magnitude(u_0))) - log(sqr_magnitude(a)*t + dot(a, u_0) + length(a)*sqrt(1.0 + sqr_magnitude(a*t + u_0)))))/pow(length(a), 3);
		}

		float3 get_spacial_component(float4 V)
		{
			return float3(V.y,V.z,V.w);
		}

		float4 combine_temporal_and_spacial(float t, float3 p)
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
			float3 coordinate_proper_velocity = _Proper_Velocity.xyz;
			float3 coordinate_velocity = coordinate_proper_velocity / sqrt(1.0 + sqr_magnitude(coordinate_proper_velocity));

			float3 observer_proper_velocity = -_Observer_Proper_Velocity.xyz;
			float3 observer_velocity = observer_proper_velocity / sqrt(1.0 + sqr_magnitude(observer_proper_velocity));

			float3 current_proper_velocity = proper_vel_add(observer_proper_velocity, coordinate_proper_velocity);
			float3 current_velocity = current_proper_velocity / sqrt(1.0 + sqr_magnitude(current_proper_velocity));

			//coordinate_proper_velocity = current_proper_velocity;
			//coordinate_velocity = current_velocity;

			float3 vertex_world_position = mul(unity_ObjectToWorld, v.vertex).xyz;
			float3 object_world_position = mul(unity_ObjectToWorld, float4(0,0,0,1));

			float4x4 boost = lorentz_boost(-coordinate_velocity); //Object -> coordinate
			//float4x4 obs_boost = lorentz_boost(-observer_velocity); //Coordinate -> observer
			float4 proper_starting_event = combine_temporal_and_spacial(-_Proper_Time_Offset, vertex_world_position);
			float4 coordinate_current_event = mul(boost, proper_starting_event);
			//float4 observer_current_event = mul(obs_boost, coordinate_current_event);

			float proper_time = 0;
			float coordinate_time = _Coordinate_Time;

			for (int i=0; i<100; ++i)
			{
				//if (get_temporal_component(observer_current_event) < coordinate_time)
				if (get_temporal_component(coordinate_current_event) < coordinate_time)
				{
					float3 a = get_spacial_component(_accelerations[i]);
					float proper_duration = get_temporal_component(_accelerations[i]);
					if (length(a) > 0.0)
					{
						float3 pos = (object_world_position + _accel_positions[i].xyz) - vertex_world_position;
						float L = dot(pos, a)/length(a);
						if (L <= 1.0/length(a))
						{
							float b = 1.0/(1.0 - length(a)*L);
							a *= b;
							proper_duration /= b;
						}else{
							a = float3(0,0,0);
							proper_duration = 0;
						}
						float3 u_0 = coordinate_proper_velocity;
					
						float coordinate_duration = get_duration(a, u_0, proper_duration);

						float3 displacement = get_displacement(a, u_0, coordinate_duration);
						float4 new_coordinate_event = coordinate_current_event + combine_temporal_and_spacial(coordinate_duration, displacement);
						//float4 new_observer_event = mul(obs_boost, new_coordinate_event);
						float3 new_proper_velocity = coordinate_proper_velocity + a*coordinate_duration;
						//float3 new_current_proper_velocity = proper_vel_add(observer_proper_velocity, new_proper_velocity);
						
						//if (get_temporal_component(new_observer_event) > coordinate_time)
						if (get_temporal_component(new_coordinate_event) > coordinate_time)
						{
							coordinate_duration = coordinate_time - get_temporal_component(coordinate_current_event);
							proper_duration = get_proper_duration(a, u_0, coordinate_duration);
							displacement = get_displacement(a, u_0, coordinate_duration);
							new_coordinate_event = coordinate_current_event + combine_temporal_and_spacial(coordinate_duration, displacement);
							//new_observer_event = mul(obs_boost, new_coordinate_event);
							new_proper_velocity = coordinate_proper_velocity + a*coordinate_duration;
							//new_current_proper_velocity = proper_vel_add(observer_proper_velocity, new_proper_velocity);
						}
						coordinate_proper_velocity = new_proper_velocity;
						coordinate_velocity = coordinate_proper_velocity / sqrt(1.0 + sqr_magnitude(coordinate_proper_velocity));
						proper_time += proper_duration;
						coordinate_current_event = new_coordinate_event;
						//observer_current_event = new_observer_event;
						/*
						if (get_temporal_component(new_observer_event) < coordinate_time)
						{
							coordinate_proper_velocity = new_proper_velocity;
							coordinate_velocity = coordinate_proper_velocity / sqrt(1.0 + sqr_magnitude(coordinate_proper_velocity));
							proper_time += proper_duration;
							coordinate_current_event = new_coordinate_event;
							observer_current_event = new_observer_event;
						}else{
							while (get_temporal_component(observer_current_event) < coordinate_time)
							{
								proper_duration = 0.1;
								u_0 = coordinate_proper_velocity;
								coordinate_duration = get_duration(a, u_0, proper_duration);
								displacement = get_displacement(a, u_0, coordinate_duration);
								coordinate_current_event += combine_temporal_and_spacial(coordinate_duration, displacement);
								observer_current_event = mul(obs_boost, coordinate_current_event);
								coordinate_proper_velocity += a*coordinate_duration;
								proper_time += proper_duration;
							}
							break;
						}
						*/				
					}else{
						float coordinate_duration = proper_duration / sqrt(1.0 + sqr_magnitude(coordinate_proper_velocity));
						float3 displacement = coordinate_velocity * coordinate_duration;
						coordinate_current_event += combine_temporal_and_spacial(coordinate_duration, displacement);
						proper_time += proper_duration;
						//float4 observer_prev_event = observer_current_event;
						//observer_current_event = mul(obs_boost, coordinate_current_event);
					}
				}else{
					break;
				}
			}
			//current_proper_velocity = proper_vel_add(observer_proper_velocity, coordinate_proper_velocity);
			//current_velocity = current_proper_velocity / sqrt(1.0 + sqr_magnitude(current_proper_velocity));
			//proper_time += (coordinate_time - get_temporal_component(observer_current_event)) * sqrt(1.0 - sqr_magnitude(current_velocity));
			//observer_current_event += (coordinate_time - get_temporal_component(observer_current_event)) * combine_temporal_and_spacial(1.0, current_velocity);
			//v.vertex = mul(unity_WorldToObject, float4(get_spacial_component(observer_current_event), 1));
			proper_time += (coordinate_time - get_temporal_component(coordinate_current_event)) * sqrt(1.0 - sqr_magnitude(coordinate_velocity));
			coordinate_current_event += (coordinate_time - get_temporal_component(coordinate_current_event)) * combine_temporal_and_spacial(1.0, coordinate_velocity);
			v.vertex = mul(unity_WorldToObject, float4(get_spacial_component(coordinate_current_event), 1));
		}
		ENDCG
	}
	FallBack "Diffuse"
}
