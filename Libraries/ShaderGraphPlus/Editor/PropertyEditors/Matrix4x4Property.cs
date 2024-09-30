namespace Editor;

[CanEdit( typeof( Float4x4 ), "float4x4" )]
public class Float4x4Property : Widget
{
	FloatProperty M11 { get; set; }
	FloatProperty M12 { get; set; }
	FloatProperty M13 { get; set; }
	FloatProperty M14 { get; set; }

	FloatProperty M21 { get; set; }
	FloatProperty M22 { get; set; }
	FloatProperty M23 { get; set; }
	FloatProperty M24 { get; set; }

	FloatProperty M31 { get; set; }
	FloatProperty M32 { get; set; }
	FloatProperty M33 { get; set; }
	FloatProperty M34 { get; set; }

	FloatProperty M41 { get; set; }
	FloatProperty M42 { get; set; }
	FloatProperty M43 { get; set; }
	FloatProperty M44 { get; set; }

	//Float4x4 _float4X4;


	public Float4x4 Value
	{

		// 16 floats
		get => new Float4x4( 
			M11.Value, 
			M12.Value, 
			M13.Value, 
			M14.Value,

			M21.Value,
			M22.Value,
			M23.Value,
			M24.Value,

			M31.Value,
			M32.Value,
			M33.Value,
			M34.Value,

			M41.Value,
			M42.Value,
			M43.Value,
			M44.Value
		);
		set
		{
			M11.Value = value.M11;
			M12.Value = value.M12;
			M13.Value = value.M13;
			M14.Value = value.M14;
						
			M21.Value = value.M21;
			M22.Value = value.M22;
			M23.Value = value.M23;
			M24.Value = value.M24;
						
			M31.Value = value.M31;
			M32.Value = value.M32;
			M33.Value = value.M33;
			M34.Value = value.M34;
						
			M41.Value = value.M41;
			M42.Value = value.M42;
			M43.Value = value.M43;
			M44.Value = value.M44;
		}
	}

	//Layout Inner;

	public Float4x4Property( Widget parent ) : base( parent )
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
			Row1Layout.AddStretchCell( 0 );

			M14 = Row1Layout.Add( new FloatProperty( "M14",this ), 1 );
			SetSize( M14, 196.0f );
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
			Row2Layout.AddStretchCell( 0 );

			M24 = Row2Layout.Add( new FloatProperty( "M24", this ), 1 );
			SetSize( M24, 196.0f );
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
			Row3Layout.AddStretchCell( 0 );

			M34 = Row3Layout.Add( new FloatProperty( "M34", this ), 1 );
			SetSize( M34, 196.0f );
		}

		var Row4Layout = Layout.AddRow();
		Row4Layout.Spacing = 4;
		{
			M41 = Row4Layout.Add( new FloatProperty( "M41", this ), 1 );
			SetSize( M41, 196.0f );
			Row4Layout.AddStretchCell( 0 );

			M42 = Row4Layout.Add( new FloatProperty( "M42", this ), 1 );
			SetSize( M42, 196.0f );
			Row4Layout.AddStretchCell( 0 );

			M43 = Row4Layout.Add( new FloatProperty( "M43", this ), 1 );
			SetSize( M43, 196.0f );
			Row4Layout.AddStretchCell( 0 );

			M44 = Row4Layout.Add( new FloatProperty( "M44", this ), 1 );
			SetSize( M44, 196.0f );
		}
	}

	private void SetSize( FloatProperty property ,float sizeX )
	{
		//property.MinimumSize = new Vector2( 128, Theme.RowHeight );
		//property.MaximumSize = property.MinimumSize;
	}

	public override void ChildValuesChanged( Widget source )
	{
		base.ChildValuesChanged( source );
	}
}
