
	class MyCustomShadingModel
	{

		static float4 Shade( PixelInput i )
		{
			return float4( 1, 0, 1, 1 );
		}


		static float3 DoLightingGenerated()
		{
			return float3( 1, 0, 1 );
		}
	}
