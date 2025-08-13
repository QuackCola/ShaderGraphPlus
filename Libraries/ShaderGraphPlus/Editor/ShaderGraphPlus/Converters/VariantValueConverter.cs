namespace ShaderGraphPlus;

internal class VariantValueConverter : JsonConverter<VariantValueBase>
{
	public override VariantValueBase Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
	{
		if ( reader.TokenType != JsonTokenType.StartObject )
		{
			SGPLog.Error( $"Unable to read from \"{reader.TokenType}\"" );
			return new VariantValueVector3( Vector3.Zero, Vector3.Zero, Vector3.One, SubgraphPortType.Vector3 );
		}

		string className = "";
		SubgraphPortType inputType = SubgraphPortType.Invalid;
		object typeInstance = null;
		object minValueTypeInstance = null;
		object maxValueTypeInstance = null;

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
					break;
					case nameof( VariantValueBase.InputType ):
						inputType = JsonSerializer.Deserialize<SubgraphPortType>( ref reader, options );
					break;
					case "Value":
						if ( inputType == SubgraphPortType.Invalid )
							throw new JsonException( "InputType must be read before Value" );

						typeInstance = inputType switch
						{
							SubgraphPortType.Bool => JsonSerializer.Deserialize<bool>( ref reader, options ),
							//SubgraphInputType.Int => JsonSerializer.Deserialize<int>( ref reader, options ),
							SubgraphPortType.Float => JsonSerializer.Deserialize<float>( ref reader, options ),
							SubgraphPortType.Vector2 => JsonSerializer.Deserialize<Vector2>( ref reader, options ),
							SubgraphPortType.Vector3 => JsonSerializer.Deserialize<Vector3>( ref reader, options ),
							SubgraphPortType.Color => JsonSerializer.Deserialize<Color>( ref reader, options ),
							SubgraphPortType.Sampler => JsonSerializer.Deserialize<Sampler>( ref reader, options ),
							SubgraphPortType.Texture2DObject => JsonSerializer.Deserialize<TextureInput>( ref reader, options ),
							_ => throw new JsonException( $"Unknown InputType \"{inputType}\"" )
						};
					break;
					case "MinValue":
						minValueTypeInstance = inputType switch
						{
							//SubgraphInputType.Int => JsonSerializer.Deserialize<int>( ref reader, options ),
							SubgraphPortType.Float => JsonSerializer.Deserialize<float>( ref reader, options ),
							SubgraphPortType.Vector2 => JsonSerializer.Deserialize<Vector2>( ref reader, options ),
							SubgraphPortType.Vector3 => JsonSerializer.Deserialize<Vector3>( ref reader, options ),
							//SubgraphInputType.Vector4 => JsonSerializer.Deserialize<Vector4>( ref reader, options ),
							SubgraphPortType.Color => JsonSerializer.Deserialize<Color>( ref reader, options ),
							_ => throw new JsonException( $"Unknown InputType \"{inputType}\"" )
						};
					break;
					case "MaxValue":
						maxValueTypeInstance = inputType switch
						{
							//SubgraphInputType.Int => JsonSerializer.Deserialize<int>( ref reader, options ),
							SubgraphPortType.Float => JsonSerializer.Deserialize<float>( ref reader, options ),
							SubgraphPortType.Vector2 => JsonSerializer.Deserialize<Vector2>( ref reader, options ),
							SubgraphPortType.Vector3 => JsonSerializer.Deserialize<Vector3>( ref reader, options ),
							//SubgraphInputType.Vector4 => JsonSerializer.Deserialize<Vector4>( ref reader, options ),
							SubgraphPortType.Color => JsonSerializer.Deserialize<Color>( ref reader, options ),
							_ => throw new JsonException( $"Unknown InputType \"{inputType}\"" )
						};
					break;
				}
			}
		}

		if ( typeInstance != null )
		{
			var variant = VariantValueBase.CreateNew( typeInstance, minValueTypeInstance, maxValueTypeInstance, inputType );
			return variant;
		}

		SGPLog.Warning( $"{nameof( VariantValueConverter )} - typeInstance is null. Returning default of \"{nameof( VariantValueVector3 )}\"" );
		return new VariantValueVector3( Vector3.Zero, Vector3.Zero, Vector3.One, SubgraphPortType.Vector3 );
	}

	public override void Write( Utf8JsonWriter writer, VariantValueBase value, JsonSerializerOptions options )
	{
		writer.WriteStartObject();
		writer.WriteString( "__class", value.GetType().Name );
		writer.WriteString( nameof( VariantValueBase.InputType ), value.InputType.ToString() );

		writer.WritePropertyName( "Value" );

		switch ( value.InputType )
		{
			case SubgraphPortType.Bool:
				JsonSerializer.Serialize( writer, ((VariantValueBool)value).Value, options );
				break;
			//case SubgraphInputType.Int:
			//	JsonSerializer.Serialize( writer, ((VariantValueInt)value).Value, options );
			//
			//	writer.WritePropertyName( "MinValue" );
			//	JsonSerializer.Serialize( writer, ((VariantValueInt)value).MinValue, options );
			//
			//	writer.WritePropertyName( "MaxValue" );
			//	JsonSerializer.Serialize( writer, ((VariantValueInt)value).MaxValue, options );
			//
			//	break;
			case SubgraphPortType.Float:
				JsonSerializer.Serialize( writer, ((VariantValueFloat)value).Value, options );

				writer.WritePropertyName( "MinValue" );
				JsonSerializer.Serialize( writer, ((VariantValueFloat)value).MinValue, options );

				writer.WritePropertyName( "MaxValue" );
				JsonSerializer.Serialize( writer, ((VariantValueFloat)value).MaxValue, options );

				break;
			case SubgraphPortType.Vector2:
				JsonSerializer.Serialize( writer, ((VariantValueVector2)value).Value, options );

				writer.WritePropertyName( "MinValue" );
				JsonSerializer.Serialize( writer, ((VariantValueVector2)value).MinValue, options );

				writer.WritePropertyName( "MaxValue" );
				JsonSerializer.Serialize( writer, ((VariantValueVector2)value).MaxValue, options );

				break;
			case SubgraphPortType.Vector3:
				JsonSerializer.Serialize( writer, ((VariantValueVector3)value).Value, options );

				writer.WritePropertyName( "MinValue" );
				JsonSerializer.Serialize( writer, ((VariantValueVector3)value).MinValue, options );

				writer.WritePropertyName( "MaxValue" );
				JsonSerializer.Serialize( writer, ((VariantValueVector3)value).MaxValue, options );
				break;
			case SubgraphPortType.Color:
				JsonSerializer.Serialize( writer, ((VariantValueColor)value).Value, options );
				break;
			case SubgraphPortType.Sampler:
				JsonSerializer.Serialize( writer, ((VariantValueSampler)value).Value, options );
				break;
			case SubgraphPortType.Texture2DObject:
				JsonSerializer.Serialize( writer, ((VariantValueTexture2D)value).Value, options );
				break;
			default:
				writer.WriteNullValue();
				break;
		}

		writer.WriteEndObject();
	}
}
