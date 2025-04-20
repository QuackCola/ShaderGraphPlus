#ifndef LIGHTINGTEST_H
#define LIGHTINGTEST_H	

	void LightingTest( float3 Albedo, float3 Normal, float2 ScreenPos, float3 WorldPos, out float4 Result )
	{
		Result = float4( 1.0f, 0.0f, 1.0f, 1.0f );

		float3 finalColor = float3(0, 0, 0);

		for ( int index = 0; index < Light::Count( ScreenPos ); index++ )
    	{
			Light light = Light::From( ScreenPos, WorldPos, index );			
			float NdotL = dot( Normal, light.Direction);			
			
			finalColor += float3(-NdotL,-NdotL,-NdotL);
    	}

		Result = float4(finalColor, 1);
	}

#endif // LIGHTINGTEST_H
 