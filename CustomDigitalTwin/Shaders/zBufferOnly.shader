Shader "CDT/zBufferOnly"
{
    
    SubShader
    {
        Tags { "Queue" = "Geometry-1" //jks- this is to render before the opaque geometry (register z-buffer before opaque geometry is rendered)
               "RenderType" = "Opaque" 
               "RenderPipeline" = "UniversalPipeline" }
       
        LOD 100
        
        ColorMask 0    //jks- to disable color writing
        
        Offset -1, -1  //jks- to avoid z-fighting

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct fragOut
            {
                float depth : DEPTH;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fragOut frag (v2f i) : SV_Target
            {
                fragOut o;
                
                #if defined(UNITY_REVERSED_Z)
                o.depth = 0;
                #else
                o.depth = 1;
                #endif
                
                return o;
            }
            
            ENDCG
        }
    }
}
