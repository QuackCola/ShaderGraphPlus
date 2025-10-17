
using Editor;

namespace ShaderGraphPlus;

internal sealed class VariantValueWidget : Widget
{
	private readonly ControlSheet Sheet;
	private SubgraphInput Node;
	private IAccessor Accessor;

	internal interface IAccessor
	{
		SerializedProperty ParentProperty { get; }
		public Action OnPropertyUpdate { get; set; }

		public T GetValue<T>();
		public void SetValue<T>( T value );
	}

	public VariantValueWidget( Widget parent ) : base( parent )
	{
		Layout = Layout.Column();

		Sheet = new ControlSheet();
		Layout.Add( Sheet, 1 );
	}

	public void SetAccessor( IAccessor accessor )
	{
		if ( Node is null )
			return;

		Accessor = accessor;

		Sheet.Clear( true );
		Sheet.AddObject( new VariantValueSerializedObject( Node, accessor ) );
	}

	public void SetNode( SubgraphInput node )
	{
		Node = node;
	}
}

internal class VariantValueSerializedObject : SerializedObject
{
	public SubgraphInput Node { get; }
	public VariantValueWidget.IAccessor Accessor { get; }

	public VariantValueSerializedObject( SubgraphInput node, VariantValueWidget.IAccessor accessor )
	{
		Node = node;
		Accessor = accessor;
		ParentProperty = accessor.ParentProperty;

		UpdateProperty();
	}

	private void UpdateProperty()
	{
		PropertyList = new List<SerializedProperty>();
		
		var paramProperty = ParamProperty.Create( this );
		PropertyList.Add( paramProperty );
	}
}

public struct VariantParam<T>
{
	public string Name;
	public string Description;
	public string Group;
	public int Order;

	public T Value;
	public T DefaultValue;
}
