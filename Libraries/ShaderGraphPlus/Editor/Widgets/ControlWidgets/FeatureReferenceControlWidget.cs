namespace Editor.ShaderGraphPlus.Nodes;

[CustomEditor( typeof( string ) , WithAllAttributes = new[] { typeof( ShaderFeatureInfoReferenceAttribute ) } )]
internal sealed class FeatureReferenceControlWidget : DropdownControlWidget<ShaderFeatureInfo>
{ 
	ShaderGraphPlus Graph;
	StaticSwitchNode Node;

	public FeatureReferenceControlWidget( SerializedProperty property ) : base( property )
	{
		Node = property.Parent.Targets.OfType<StaticSwitchNode>().FirstOrDefault();
		Graph = (ShaderGraphPlus)Node.Graph;

		if ( Graph is null ) return;

		//if ( SerializedProperty.GetValue<ShaderFeatureInfo>().IsValid )
		//{
		//	var name = SerializedProperty.GetValue<ShaderFeatureInfo>().UserDefinedName;
		//	if ( Graph.Features.ContainsKey( name ) )
		//	{
		//		SerializedProperty.SetValue<ShaderFeatureInfo>( Graph.Features[name] );
		//	}
		//}
		if ( string.IsNullOrWhiteSpace( SerializedProperty.GetValue<string>()  ))
		{
			SerializedProperty.SetValue<string>( "None" );
		}
	}

	protected override IEnumerable<object> GetDropdownValues()
	{
		List<object> list = new();
		list.Add( new ShaderFeatureInfo( "", "", "", 0, false ) { PlaceHolder = "None" } );

		foreach ( var feature in Graph.Features )
		{
			var entry = new Entry();
			entry.Value = feature.Value;
			entry.Label = $"{feature.Value}";
			entry.Description = feature.Value.FeatureDescription;
			list.Add( entry );
		}

		return list;
	}

	protected override void OnPaint()
	{
		base.OnPaint();
	}
}
