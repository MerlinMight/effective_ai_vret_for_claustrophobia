Shader "Custom/SkyboxVideo"
{
    Properties
    {
        _MainTex ("Skybox Texture", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "Queue" = "Background" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            struct appdata_t { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = float2((v.vertex.x + 1) * 0.5, (v.vertex.y + 1) * 0.5); // Proper UV mapping
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the texture based on the UV coordinates
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
