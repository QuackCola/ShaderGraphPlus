using static Sandbox.TagSet;

namespace Editor.ShaderGraphPlus;

internal class DefaultSubgraphValueDataConverter : JsonConverter<DefaultSubgraphValueData>
{
	public override DefaultSubgraphValueData Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
	{
		if ( reader.TokenType == JsonTokenType.StartObject )
		{
			reader.Read(); // Fetch the _class property

			if ( reader.GetString() != "_class" )
			{
				throw new Exception( $"_class Property was not found!" );
			}

			reader.Read(); // Fetch the _class string

			var className = reader.GetString();

			SGPLog.Info( $"_class string is `{className}`" );

			reader.Read(); // Fetch the DefaultValue property
			
			if ( reader.TokenType == JsonTokenType.EndObject ) // Bail if there is no DefaultValue property.
			{
				return default( DefaultSubgraphValueData );
			}
			
			reader.Read(); // Fetch the DefaultValue data

			object defaultValue = null;
			
			if ( className == typeof( bool ).Name )
			{
				defaultValue = reader.GetBoolean();
			}
			else if ( className == typeof( int ).Name )
			{
				defaultValue = reader.GetInt32();
			}
			else if ( className == typeof( float ).Name )
			{
				defaultValue = reader.GetSingle();
			}
			else if ( className == typeof( Vector2 ).Name )
			{
				defaultValue = Vector2.Parse( reader.GetString() );
			}
			else if ( className == typeof( Vector3 ).Name )
			{
				defaultValue = Vector3.Parse( reader.GetString() );
			}
			else if ( className == typeof( Vector4 ).Name )
			{
				defaultValue = Vector4.Parse( reader.GetString() );
			}
			else if ( className == typeof( Color ).Name )
			{
				defaultValue = Vector4.Parse( reader.GetString() );
			}
			else if ( className == typeof( Sampler ).Name )
			{
				defaultValue = JsonSerializer.Deserialize<Sampler>( ref reader, options )!;
			}
			else
			{
				throw new NotImplementedException( $"Invalid className `{className}`" );
			}
			
			reader.Read();
			
			while ( reader.TokenType != JsonTokenType.EndObject )
			{
				reader.Read();
			}
			
			DefaultSubgraphValueData data = new DefaultSubgraphValueData( defaultValue );
	
			return data;
		}

		SGPLog.Warning( $"{nameof( DefaultSubgraphValueDataConverter )} - unable to read from `{reader.TokenType}`." );
		return default ( DefaultSubgraphValueData );
	}

	public override void Write( Utf8JsonWriter writer, DefaultSubgraphValueData value, JsonSerializerOptions options )
	{
		var type = value.DefaultValue.GetType();

		writer.WriteStartObject();
		writer.WriteString( "_class", type.Name );

		var propertyName = nameof ( DefaultSubgraphValueData.DefaultValue );

		if ( value.DefaultValue is bool @bool )
		{
			writer.WriteBoolean( propertyName, @bool );
		}
		else if ( value.DefaultValue is int @int )
		{
			writer.WriteNumber( propertyName, @int );
		}
		else if ( value.DefaultValue is float @float )
		{
			writer.WriteNumber( propertyName, @float );
		}
		else if ( value.DefaultValue is Vector2 vec2 )
		{
			writer.WriteString( propertyName,
				$"{vec2.x:0.#################################}," +
				$"{vec2.y:0.#################################}"
			);
		}
		else if ( value.DefaultValue is Vector3 vec3 )
		{
			writer.WriteString( propertyName,
				$"{vec3.x:0.#################################}," +
				$"{vec3.y:0.#################################}," +
				$"{vec3.z:0.#################################}"
			);
		}
		else if ( value.DefaultValue is Vector4 vec4 )
		{
			writer.WriteString( propertyName,
				$"{vec4.x:0.#################################}," +
				$"{vec4.y:0.#################################}," +
				$"{vec4.z:0.#################################}," +
				$"{vec4.w:0.#################################}"
			);
		}
		else if ( value.DefaultValue is Color color )
		{
			writer.WriteString( propertyName,
				$"{color.r:0.#####}," +
				$"{color.g:0.#####}," +
				$"{color.b:0.#####}," +
				$"{color.a:0.#####}"
			);
		}
		else if ( value.DefaultValue is Sampler sampler )
		{
			writer.WritePropertyName( propertyName );
			JsonSerializer.Serialize( writer, sampler, options ); // Uses SamplerConverter if registered
		}

		writer.WriteEndObject();
	}
}
