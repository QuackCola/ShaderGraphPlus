using Sandbox.Internal;
using System.Diagnostics.CodeAnalysis;

namespace Editor.ShaderGraphPlus;

public enum SamplerFilter
{
	Aniso,
	Bilinear,
	Trilinear,
	Point,
}

public enum SamplerAddress
{
	Wrap,
	Mirror,
	Clamp,
	Border,
	Mirror_Once,
}

[JsonConverter( typeof( SamplerConverter ) )]
public struct Sampler
{
	/// <summary>
	/// Name of this Sampler
	/// </summary>
	public string Name { get; set; } = "";

	/// <summary>
	/// Smooth or Pixelated filtering
	/// </summary>
	public SamplerFilter Filter { get; set; } = SamplerFilter.Aniso;

	/// <summary>
	/// Horizontal wrapping, repeating or stretched
	/// </summary>
	public SamplerAddress AddressU { get; set; } = SamplerAddress.Wrap;

	/// <summary>
	/// Vertical wrapping, repeating or stretched
	/// </summary>
	public SamplerAddress AddressV { get; set; } = SamplerAddress.Wrap;

	public readonly string CreateSampler( string name )
	{
		return $"SamplerState g_s{name}";
	}

	public Sampler()
	{
	}
}

//[CustomEditor( typeof( Sampler ) )]
public class SamplerControlWidget : ControlObjectWidget
{
	// Whether or not this control supports multi-editing (if you have multiple GameObjects selected)
	public override bool SupportsMultiEdit => false;

	public SamplerControlWidget( SerializedProperty property ) : base( property, true )
	{
		Layout = Layout.Column();
		Layout.Spacing = 4;

		if ( SerializedObject == null )
		{
			SGPLog.Error( $"SerialisedObject is null!!! {property.Name}" );

		}



		// Get the Color and Name properties from the serialized object
		SerializedObject.TryGetProperty( nameof( Sampler.Name ), out var name );
		SerializedObject.TryGetProperty( nameof( Sampler.Filter ), out var filter );
		SerializedObject.TryGetProperty( nameof( Sampler.AddressU ), out var addressU );
		SerializedObject.TryGetProperty( nameof( Sampler.AddressV ), out var addressV );

		// Add some Controls to the Layout, both referencing their serialized properties
		Layout.Add( Create( name ) );
		Layout.Add( Create( filter ) );
		Layout.Add( Create( addressU ) );
		Layout.Add( Create( addressV ) );
	}

	//protected override void OnPaint()
	//{
	//	// Overriding and doing nothing here will prevent the default background from being painted
	//}
}

internal class SamplerConverter : JsonConverter<Sampler>
{
	public override Sampler Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
	{
		var sampler = new Sampler();

		while ( reader.Read() )
		{
			if ( reader.TokenType == JsonTokenType.PropertyName )
			{
				string propertyName = reader.GetString();
				
				reader.Read();

				switch ( propertyName )
				{
					case nameof( Sampler.Name ) :
						sampler.Name = reader.GetString();
						break;
					case nameof( Sampler.Filter ) :
						sampler.Filter = Enum.Parse<SamplerFilter>( reader.GetString(), true );
						break;
					case nameof( Sampler.AddressU ) :
						sampler.AddressU = Enum.Parse<SamplerAddress>( reader.GetString(), true );
						break;
					case nameof( Sampler.AddressV ) :
						sampler.AddressV = Enum.Parse<SamplerAddress>( reader.GetString(), true );
						break;
				}
			}

			if ( reader.TokenType == JsonTokenType.EndObject )
				break;
		}

		return sampler;
	}

	public override void Write( Utf8JsonWriter writer, Sampler value, JsonSerializerOptions options )
	{
		//var defaultSampler = default( Sampler );

		writer.WriteStartObject();
		writer.WriteString( nameof( Sampler.Name ), string.IsNullOrWhiteSpace( value.Name ) ? "" : value.Name );
		writer.WriteString( nameof( Sampler.Filter ), value.Filter.ToString() );
		writer.WriteString( nameof( Sampler.AddressU ), value.AddressU.ToString() );
		writer.WriteString( nameof( Sampler.AddressV ), value.AddressU.ToString() );
		writer.WriteEndObject();
	}
}
