namespace Editor;

[CanEdit( typeof( Float2x2 ), "float2x2" )]
public class Float2x2Property : Widget
{
	FloatProperty M11 { get; set; }
	FloatProperty M12 { get; set; }

	FloatProperty M21 { get; set; }
	FloatProperty M22 { get; set; }

	public Float2x2 Value
	{

		// 4 floats
		get => new Float2x2(
			M11.Value,
			M12.Value,

			M21.Value,
			M22.Value
		);
		set
		{
			M11.Value = value.M11;
			M12.Value = value.M12;

			M21.Value = value.M21;
			M22.Value = value.M22;
		}
	}

	public Float2x2Property( Widget parent ) : base( parent )
	{
		Layout = Layout.Column();
		Layout.Spacing = 2;

		var Row1Layout = Layout.AddRow();
		Row1Layout.Spacing = 4;
		{
			M11 = Row1Layout.Add( new FloatProperty( "M11", this ), 1 );
			SetSize( M11, 196.0f );
			Row1Layout.AddStretchCell( 0 );

			M12 = Row1Layout.Add( new FloatProperty( "M12", this ), 1 );
			SetSize( M12, 196.0f );
		}

		var Row2Layout = Layout.AddRow();
		Row2Layout.Spacing = 4;
		{
			M21 = Row2Layout.Add( new FloatProperty( "M21", this ), 1 );
			SetSize( M21, 196.0f );
			Row2Layout.AddStretchCell( 0 );

			M22 = Row2Layout.Add( new FloatProperty( "M22", this ), 1 );
			SetSize( M22, 196.0f );
		}
	}

	private void SetSize( FloatProperty property, float sizeX )
	{
		//property.MinimumSize = new Vector2( 128, Theme.RowHeight );
		//property.MaximumSize = property.MinimumSize;
	}

	public override void ChildValuesChanged( Widget source )
	{
		base.ChildValuesChanged( source );
	}


}
