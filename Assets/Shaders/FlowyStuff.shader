Shader "Custom/FlowyStuff" {
    Properties {
        _MainTex ("Main Texture", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _WindMap ("Wind Noise Map", 2D) = "white" {}
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
        _Shininess ("Smoothness", Range(0,1)) = 0.5
        _Anisotropy ("Anisotropy", Range(-1, 1)) = 0.0
        _SwaySpeed ("Sway Speed", Range(0, 10)) = 0.0
        _SwayStrength ("Sway Strength", Range(0, 1)) = 0.0
    }
    SubShader {
        Tags {
            "Queue"="Transparent" // Ensure correct render order
            "RenderType"="Transparent" // For proper alpha blending
        }
        LOD 200

        ZWrite On // Ensures the shader writes to the depth buffer
        Blend SrcAlpha OneMinusSrcAlpha // Standard alpha blending
        Cull Off // Disable backface culling

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _NormalMap;
        sampler2D _WindMap;
        half _Cutoff;
        half _Shininess;
        half _Anisotropy;
        half _SwaySpeed;
        half _SwayStrength;

        struct Input {
            float2 uv_MainTex;
            float2 uv_NormalMap;
            float3 worldNormal;
            float3 worldPos;
            INTERNAL_DATA
        };

        float ContinuousNoise(float x) {
            float frequency = 4.0;
            float amplitude = 1.0;

            // Calculate noise using sine and cosine functions
            float noise = sin(x * frequency) + cos(x * frequency * 0.5) + sin(x * frequency * 0.25);

            // Normalize the noise to the range [-1, 1]
            noise = noise * (0.5/3.0);

            // Scale the noise by the amplitude
            return noise * amplitude;
        }

        float3 NoiseBasedPosition(float t, half3 xyz) {
            // Use time or any other float parameter to generate noise
            float noiseX = ContinuousNoise(t * 0.5 + xyz.z) * 1.0; // Scale the noise for x-axis
            float noiseY = ContinuousNoise(t * 0.3 + xyz.x) * 2.0; // Scale the noise for y-axis
            float noiseZ = ContinuousNoise(t * 0.7 + xyz.y) * 3.0; // Scale the noise for z-axis

            return float3(noiseX, noiseY, noiseZ);
        }

        void vert(inout appdata_full v) {
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz * 27.65;
            v.vertex.xyz += NoiseBasedPosition(_Time.w * _SwaySpeed, v.vertex.xyz + worldPos) * (1.0 - v.color.r) * _SwayStrength;
        }

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;

            // Alpha cutoff
            clip(c.a - _Cutoff);

            // Normal mapping
            o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap));

            // Smoothness
            o.Smoothness = _Shininess;

            // Modulate lighting by isotropy
            half isotropicFactor = _Anisotropy * 2 - 1; // Convert from range [0,1] to [-1,1]
            o.Normal = lerp(o.Normal, normalize(isotropicFactor * o.Normal + (1 - isotropicFactor) * float3(0, 0, 1)), _Anisotropy);
        }
        ENDCG
    }

    FallBack "Diffuse"
}