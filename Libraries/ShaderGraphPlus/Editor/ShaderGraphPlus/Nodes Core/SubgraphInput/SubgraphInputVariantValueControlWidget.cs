
using Editor;

namespace ShaderGraphPlus;

[CustomEditor( typeof( VariantValueBase ), NamedEditor = "DefaultValue", WithAllAttributes = [typeof( InlineEditorAttribute )] )]
internal class SubgraphInputVariantValueControlWidget : ControlWidget
{
	public override bool SupportsMultiEdit => false;

	SubgraphInput Node;

	private readonly VariantValueWidget VariantProperty;
	private VariantPropertyCollection InputDefaultCollection;


	public SubgraphInputVariantValueControlWidget( SerializedProperty property ) : base( property )
	{
		Node = property.Parent.Targets.First() as SubgraphInput;

		Layout = Layout.Column();
		Layout.Spacing = 2;

		if ( Node == null  )
		{ 
			return;
		}

		if ( Node.InputData == null )
		{
			SGPLog.Info( $"Node.DefaultValue is  null setting default to Vector3" );
			Node.InputData = new VariantValueVector3( Vector3.Zero, Vector3.Zero, Vector3.One, SubgraphInputType.Vector3 );
		}

		property.TryGetAsObject( out var so );

		so.TryGetProperty( nameof( VariantValueBase.InputType ), out var inputType );
		so.OnPropertyChanged += x =>
		{
			OnInputTypeChanged( x.GetValue<SubgraphInputType>() );
		};

		Layout.Add( Create( inputType ) );
		Layout.AddSeparator();
		VariantProperty = new VariantValueWidget( this );
		Layout.Add( VariantProperty );

		Rebuild();
	}

	private void OnInputTypeChanged( SubgraphInputType inputType )
	{
		switch ( inputType )
		{
			case SubgraphInputType.Bool:
				Node.InputData = new VariantValueBool( false, SubgraphInputType.Bool );
				break;
			case SubgraphInputType.Float:
				Node.InputData = new VariantValueFloat( 0.0f, 0.0f, 1.0f, SubgraphInputType.Float );
				break;
			case SubgraphInputType.Vector2:
				Node.InputData = new VariantValueVector2( Vector2.Zero, Vector2.Zero, Vector2.One, SubgraphInputType.Vector2 );
				break;
			case SubgraphInputType.Vector3:
				Node.InputData = new VariantValueVector3( Vector3.Zero, Vector3.Zero, Vector3.One, SubgraphInputType.Vector3 );
				break;
			case SubgraphInputType.Color:
				Node.InputData = new VariantValueColor( Color.White, SubgraphInputType.Color );
				break;
			case SubgraphInputType.Sampler:
				Node.InputData = new VariantValueSampler( new Sampler(), SubgraphInputType.Sampler ) ;
				break;
			case SubgraphInputType.Texture2DObject:
				Node.InputData = new VariantValueTexture2D( new TextureInput(), SubgraphInputType.Texture2DObject );
				break;
		}

		Rebuild();
	}

	//protected override void OnPaint()
	//{
	//
	//}

	private void Rebuild()
	{
		VariantProperty.SetNode( Node );

		InputDefaultCollection = new VariantPropertyCollection( this, Node );
		VariantProperty.SetAccessor( InputDefaultCollection );
		InputDefaultCollection.OnPropertyUpdate += OnDefaultValuePropertyUpdated;
	}

	private void OnDefaultValuePropertyUpdated()
	{
		Node.Update();
		Node.IsDirty = true;
	}

	class VariantPropertyCollection : VariantValueWidget.IAccessor
	{
		private SubgraphInput Node;
		private SubgraphInputVariantValueControlWidget Parent;
		public SerializedProperty ParentProperty => null;
		public Action OnPropertyUpdate { get; set; }
	
		public  VariantPropertyCollection( SubgraphInputVariantValueControlWidget parent, SubgraphInput node )
		{
			Parent = parent;
			Node = node;
		}
	
		public T GetValue<T>( )
		{
			return Node.GetDefaultValue<T>();
		}

		public T GetValueRangeMin<T>()
		{
			return Node.GetDefaultValueRangeMin<T>();
		}

		public T GetValueRangeMax<T>()
		{
			return Node.GetDefaultValueRangeMax<T>();
		}

		public void SetValue<T>( T value )
		{
			Node?.SetDefaultValue<T>( value );
			OnPropertyUpdate?.Invoke();
		}

		public void SetValueRangeMin<T>( T value )
		{
			Node?.SetDefaultValueRangeMin<T>( value );
		}

		public void SetValueRangeMax<T>( T value )
		{
			Node?.SetDefaultValueRangeMax<T>( value );
		}
	}
}
