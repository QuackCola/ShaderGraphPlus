namespace Editor;

[CanEdit(typeof(Float3x3), "float3x3")]
public class Float3x3Property : Widget
{
	FloatProperty M11 { get; set; }
	FloatProperty M12 { get; set; }
	FloatProperty M13 { get; set; }

	FloatProperty M21 { get; set; }
	FloatProperty M22 { get; set; }
	FloatProperty M23 { get; set; }

	FloatProperty M31 { get; set; }
	FloatProperty M32 { get; set; }
	FloatProperty M33 { get; set; }


	public Float3x3 Value
	{

		// 9 floats
		get => new Float3x3(
			M11.Value,
			M12.Value,
			M13.Value,

			M21.Value,
			M22.Value,
			M23.Value,

			M31.Value,
			M32.Value,
			M33.Value
		);
		set
		{
			M11.Value = value.M11;
			M12.Value = value.M12;
			M13.Value = value.M13;

			M21.Value = value.M21;
			M22.Value = value.M22;
			M23.Value = value.M23;

			M31.Value = value.M31;
			M32.Value = value.M32;
			M33.Value = value.M33;
		}


	}



	public Float3x3Property( Widget parent ) : base( parent )
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
			Row1Layout.AddStretchCell( 0 );

			M13 = Row1Layout.Add( new FloatProperty( "M13", this ), 1 );
			SetSize( M13, 196.0f );
		}

		var Row2Layout = Layout.AddRow();
		Row2Layout.Spacing = 4;
		{
			M21 = Row2Layout.Add( new FloatProperty( "M21", this ), 1 );
			SetSize( M21, 196.0f );
			Row2Layout.AddStretchCell( 0 );

			M22 = Row2Layout.Add( new FloatProperty( "M22", this ), 1 );
			SetSize( M22, 196.0f );
			Row2Layout.AddStretchCell( 0 );

			M23 = Row2Layout.Add( new FloatProperty( "M23", this ), 1 );
			SetSize( M23, 196.0f );
		}

		var Row3Layout = Layout.AddRow();
		Row3Layout.Spacing = 4;
		{
			M31 = Row3Layout.Add( new FloatProperty("M31", this), 1 );
			SetSize( M31, 196.0f );
			Row3Layout.AddStretchCell( 0 );

			M32 = Row3Layout.Add( new FloatProperty( "M32", this ), 1 );
			SetSize( M32, 196.0f );
			Row3Layout.AddStretchCell( 0 );

			M33 = Row3Layout.Add( new FloatProperty( "M33", this ), 1 );
			SetSize( M33, 196.0f );
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
