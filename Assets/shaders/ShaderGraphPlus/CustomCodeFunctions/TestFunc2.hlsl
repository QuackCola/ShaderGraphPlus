#ifndef TESTFUNC_H
#define TESTFUNC_H	
	void TestFunc( float2 uv, float speed,  out float mask, out float thing )
	{
		float3 col1 = float3(1,0,1);
		mask = 0.0f;
		thing = 0.0f;	
		
		for( float i=0.; i<.5; i+=.01 )
		{
			uv.x+= clamp(sin(2.*g_flTime*speed)*.1,-.5,.5)*.15;
			uv.y+= clamp(cos(g_flTime+i*5.)*.1,-.5,.5)*.15;
			float d = length(uv);
			float s = step(d,i)*.01;
			col1+=s;	
			mask = s;
			thing = col1.y;
		}	
	}
#endif // TESTFUNC_H