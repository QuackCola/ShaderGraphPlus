
#ifndef CUSTOMSHADINGTEST_H
#define CUSTOMSHADINGTEST_H	

	void CustomShadingTest(  float3 Albedo,  float3 Ambient,  float2 ScreenPosition,  float3 WorldNormal,  float3 WorldPosition,  float3 ViewDirection, out float4 ResultColor)
	{
		float3 lightingResult = Ambient;

		for( int i=0; i < Light::Count( ScreenPosition ); i++ )
		{
			Light l = Light::From( ScreenPosition, WorldPosition, i );

			float diffuse = dot( l.Direction, WorldNormal );
			lightingResult += smoothstep( .1 , .2, diffuse ) * l.Attenuation * l.Color;
		}

		ResultColor = float4( lightingResult, 1);
	}

#endif // CUSTOMSHADINGTEST_H
