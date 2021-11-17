// Shader 시작. 셰이더의 폴더와 이름을 여기서 결정합니다.

Shader "SecondDay/URPBasic"
{


   Properties
   {
    _TintColor("Color",color) = (1,1,1,1)
    _MainTex("RGB",2D) = "white"{}
    _Intensity("Intensity",Range(0,10)) = 1

    //[Toggle] _AlphaOn("AlphaTest", float) = 1 토글방식
    _AlphaCut("AlphaCut", Range(0,1)) = 0.5

    [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1 // texture 
    [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 0 // back 
    [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull Mode", Float) = 1  // 화면에 그려지는 삼각형(폴리곤)의 앞,뒷면 그릴지 정하기

    [Enum(Off, 0, On, 1)] _ZWrite("ZWrite", Float) = 0

    [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 0
	
	_Factor("Factor", int) = 0
	_Units("Units", int) = 0

[Enum(Off, 0, On, 1)] _Mask ("Alpha to Coverage", Float) = 0

    }  

	SubShader
	{  

	Tags
            {
//Render type과 Render Queue를 여기서 결정합니다.
	   "RenderPipeline"="UniversalPipeline"
                "RenderType"="Transparent"          
                "Queue"="Transparent"
            }
    	Pass
    	{
        Blend [_SrcBlend][_DstBlend]
        Cull [_Cull]
        Zwrite [_ZWrite]
        ZTest[_ZTest]

        Offset[_Factor],[_Units]
	
     	Name "Universal Forward"
              Tags { "LightMode" = "UniversalForward" }

       	HLSLPROGRAM

        	#pragma prefer_hlslcc gles
        	#pragma exclude_renderers d3d11_9x
        	#pragma vertex vert
        	#pragma fragment frag

//cg shader는 .cginc를 hlsl shader는 .hlsl을 include하게 됩니다.
       	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"        	
  
//vertex buffer에서 읽어올 정보를 선언합니다. 	
         	struct VertexInput
         	{
            	float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
          	};

            half4 _TintColor;
            //sampler2D _MainTex;


            float4 _MainTex_ST;
            Texture2D _MainTex;

            SamplerState sampler_MainTex;

            float _AlphaCut;
            float _Intensity;


        	struct VertexOutput
          	{
           	float4 vertex  	: SV_POSITION;
            float2 uv : TEXCOORD0;
      	};

//버텍스 셰이더
      	VertexOutput vert(VertexInput v)
        	{

          	VertexOutput o;      
          	o.vertex = TransformObjectToHClip(v.vertex.xyz);
            o.uv = v.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw; // 크기


            return o;
        	}

//픽셀 셰이더
        	half4 frag(VertexOutput i) : SV_Target
        	{
            half4 color = _MainTex.Sample(sampler_MainTex, i.uv);
            color.rgb *= _TintColor * _Intensity;
            color.a *= _AlphaCut;


          	return color;  
       	
        	}

        	ENDHLSL  
    	}
     }
}
