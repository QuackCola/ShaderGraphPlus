namespace ShaderGraphPlus;

internal class VariantValueConverter : JsonConverter<VariantValueBase>
{
	public override VariantValueBase Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
	{

		if ( reader.TokenType != JsonTokenType.StartObject )
		{
			SGPLog.Error( $"Unable to read from \"{reader.TokenType}\"" );
			return new VariantValueVector3( Vector3.Zero, SubgraphInputType.Vector3 );
		}

		string className = "";
		SubgraphInputType inputType = SubgraphInputType.Invalid;
		object typeInstance = null;

		while ( reader.Read() )
		{
			if ( reader.TokenType == JsonTokenType.EndObject )
				break;

			if ( reader.TokenType == JsonTokenType.PropertyName )
			{
				string propName = reader.GetString();
				reader.Read();

				switch ( propName )
				{
					case "__class":
						className = reader.GetString();
						//SGPLog.Info( $"Got classname \"{className}\"" );
						break;

					case nameof( VariantValueBase.InputType ):
						inputType = JsonSerializer.Deserialize<SubgraphInputType>( ref reader, options );

						//SGPLog.Info( $"Got inputType \"{inputType}\"" );
						break;

					case "Value":
						if ( inputType == SubgraphInputType.Invalid )
							throw new JsonException( "InputType must be read before Value" );

						typeInstance = inputType switch
						{
							SubgraphInputType.Bool => JsonSerializer.Deserialize<bool>( ref reader, options ),
							SubgraphInputType.Float => JsonSerializer.Deserialize<float>( ref reader, options ),
							SubgraphInputType.Vector2 => JsonSerializer.Deserialize<Vector2>( ref reader, options ),
							SubgraphInputType.Vector3 => JsonSerializer.Deserialize<Vector3>( ref reader, options ),
							SubgraphInputType.Color => JsonSerializer.Deserialize<Color>( ref reader, options ),
							SubgraphInputType.Sampler => JsonSerializer.Deserialize<Sampler>( ref reader, options ),
							SubgraphInputType.Texture2DObject => JsonSerializer.Deserialize<TextureInput>( ref reader, options ),
							_ => throw new JsonException( $"Unknown InputType \"{inputType}\"" )
						};
						break;
				}
			}
		}

		if ( typeInstance != null )
		{
			var variant = VariantValueBase.CreateNew( typeInstance, inputType );
			return variant;
		}

		SGPLog.Warning( $"{nameof( VariantValueConverter )} - typeInstance is null. Returning default of \"{nameof( VariantValueVector3 )}\"" );
		return new VariantValueVector3( Vector3.Zero, SubgraphInputType.Vector3 );
	}

	public override void Write( Utf8JsonWriter writer, VariantValueBase value, JsonSerializerOptions options )
	{
		writer.WriteStartObject();
		writer.WriteString( "__class", value.GetType().Name );
		writer.WriteString( nameof( VariantValueBase.InputType ), value.InputType.ToString() );

		writer.WritePropertyName( "Value" );

		switch ( value.InputType )
		{
			case SubgraphInputType.Bool:
				JsonSerializer.Serialize( writer, ((VariantValueBool)value).Value, options );
				break;
			case SubgraphInputType.Float:
				JsonSerializer.Serialize( writer, ((VariantValueFloat)value).Value, options );
				break;
			case SubgraphInputType.Vector2:
				JsonSerializer.Serialize( writer, ((VariantValueVector2)value).Value, options );
				break;
			case SubgraphInputType.Vector3:
				JsonSerializer.Serialize( writer, ((VariantValueVector3)value).Value, options );
				break;
			case SubgraphInputType.Color:
				JsonSerializer.Serialize( writer, ((VariantValueColor)value).Value, options );
				break;
			case SubgraphInputType.Sampler:
				JsonSerializer.Serialize( writer, ((VariantValueSampler)value).Value, options );
				break;
			case SubgraphInputType.Texture2DObject:
				JsonSerializer.Serialize( writer, ((VariantValueTexture2D)value).Value, options );
				break;
			default:
				writer.WriteNullValue();
				break;
		}

		writer.WriteEndObject();
	}
}
