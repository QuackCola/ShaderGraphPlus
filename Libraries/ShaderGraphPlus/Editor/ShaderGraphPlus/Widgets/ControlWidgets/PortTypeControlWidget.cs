using Editor;

namespace ShaderGraphPlus;

/// <summary>
/// 
/// </summary>
[CustomEditor(typeof(string), NamedEditor = "portType")]
sealed class PortTypeControlWidget : DropdownControlWidget<string>
{
	public PortTypeControlWidget( SerializedProperty property ) : base( property )
	{
	}

	protected override IEnumerable<object> GetDropdownValues()
	{
		List<object> list = new();
		foreach ( var type in GraphCompiler.ValueTypes )
		{
			if ( type.Key == typeof( float ) ) list.Add("float");
			else if ( type.Key == typeof( int ) ) list.Add("int");
			else if ( type.Key == typeof( bool ) ) list.Add("bool");
			else if ( type.Key == typeof( Texture2DObject ) ) list.Add( "Texture2D" );
			else if ( type.Key == typeof( Sampler ) ) list.Add( "Sampler" );
			else list.Add( type.Key );
		}
		return list;
	}
}

