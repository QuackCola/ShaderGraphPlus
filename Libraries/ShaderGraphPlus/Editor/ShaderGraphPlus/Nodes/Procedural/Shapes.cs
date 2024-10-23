namespace Editor.ShaderGraphPlus.Nodes;

[Title( "Box Shape" ), Category( "Procedural/Shapes" )]
public sealed class BoxShapeNode : ShaderNodePlus
{
[Hide]
public string BoxShape => @"
float BoxShape( float2 UV, float Width, float Height )
{
	float2 d = abs(UV * 2 - 1) - float2(Width, Height);
    d = 1 - d / fwidth(d);
	return saturate(min(d.x, d.y));
}
";

	[Title( "UV" )]
	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput Coords { get; set; }

	[Title( "Width" )]
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Width { get; set; }

	[Title( "Height" )]
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Height { get; set; }


	public float DefaultWidth { get; set; } = 0.5f;
	public float DefaultHeight { get; set; } = 0.5f;

	[Output( typeof( float ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var incoords = compiler.Result( Coords );
		var width = compiler.ResultOrDefault( Width, DefaultWidth );
		var height = compiler.ResultOrDefault( Height, DefaultHeight );

		var coords = "";

		if ( compiler.Graph.MaterialDomain is MaterialDomain.PostProcess )
		{
			coords = incoords.IsValid ? $"{incoords.Cast( 2 )}" : "CalculateViewportUv( i.vPositionSs.xy )";
		}
		else
		{
			coords = incoords.IsValid ? $"{incoords.Cast( 2 )}" : "i.vTextureCoords.xy";
		}

		return new NodeResult( ResultType.Float, compiler.ResultFunction( BoxShape, args: $"{coords}, {width}, {height}" ));
	};

}

[Title( "Elipse Shape" ), Category( "Procedural/Shapes" )]
public sealed class ElipseShapeNode : ShaderNodePlus
{
[Hide]
public string ElipseShape => @"
float ElipseShape( float2 UV, float Width, float Height )
{
    float d = length((UV * 2 - 1) / float2(Width, Height));
    return saturate((1 - d) / fwidth(d));
}
";

	[Title( "UV" )]
	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput Coords { get; set; }

	[Title( "Width" )]
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Width { get; set; }

	[Title( "Height" )]
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Height { get; set; }


	public float DefaultWidth { get; set; } = 0.5f;
	public float DefaultHeight { get; set; } = 0.5f;

	[Output( typeof( float ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var incoords = compiler.Result( Coords );
		var width = compiler.ResultOrDefault( Width, DefaultWidth );
		var height = compiler.ResultOrDefault( Height, DefaultHeight );

		var coords = "";

		if ( compiler.Graph.MaterialDomain is MaterialDomain.PostProcess )
		{
			coords = incoords.IsValid ? $"{incoords.Cast( 2 )}" : "CalculateViewportUv( i.vPositionSs.xy )";
		}
		else
		{
			coords = incoords.IsValid ? $"{incoords.Cast( 2 )}" : "i.vTextureCoords.xy";
		}

		return new NodeResult( ResultType.Float, compiler.ResultFunction( ElipseShape, args: $"{coords}, {width}, {height}" ));
	};

}

[Title( "Polygon Shape" ), Category( "Procedural/Shapes" )]
public sealed class PolygonShapeNode : ShaderNodePlus
{
[Hide]
public string PolygonShape => @"
float PolygonShape( float2 UV, float Sides, float Width, float Height )
{
    float pi = 3.14159265359;
    float aWidth = Width * cos(pi / Sides);
    float aHeight = Height * cos(pi / Sides);
    float2 uv = (UV * 2 - 1) / float2(aWidth, aHeight);
    uv.y *= -1;
    float pCoord = atan2(uv.x, uv.y);
    float r = 2 * pi / Sides;
    float distance = cos(floor(0.5 + pCoord / r) * r - pCoord) * length(uv);
    return saturate((1 - distance) / fwidth(distance));
}
";

	[Title( "UV" )]
	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput Coords { get; set; }

	[Title( "Sides" )]
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Sides { get; set; }

	[Title( "Width" )]
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Width { get; set; }

	[Title( "Height" )]
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Height { get; set; }


	public float DefaultSides { get; set; } = 4.0f;
	public float DefaultWidth { get; set; } = 0.5f;
	public float DefaultHeight { get; set; } = 0.5f;

	[Output( typeof( float ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var incoords = compiler.Result( Coords );
		var sides = compiler.ResultOrDefault( Sides, DefaultSides );
		var width = compiler.ResultOrDefault( Width, DefaultWidth );
		var height = compiler.ResultOrDefault( Height, DefaultHeight );

		var coords = "";

		if ( compiler.Graph.MaterialDomain is MaterialDomain.PostProcess )
		{
			coords = incoords.IsValid ? $"{incoords.Cast( 2 )}" : "CalculateViewportUv( i.vPositionSs.xy )";
		}
		else
		{
			coords = incoords.IsValid ? $"{incoords.Cast( 2 )}" : "i.vTextureCoords.xy";
		}

		return new NodeResult( ResultType.Float, compiler.ResultFunction( PolygonShape, args: $"{coords}, {sides}, {width}, {height}" ));
	};

}

