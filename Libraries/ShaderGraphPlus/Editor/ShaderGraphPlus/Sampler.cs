using Editor;
using Sandbox.Internal;
using System.Diagnostics.CodeAnalysis;

namespace ShaderGraphPlus;

//public enum SamplerFilter
//{
//	Aniso,
//	Bilinear,
//	Trilinear,
//	Point,
//}
//
//public enum SamplerAddress
//{
//	Wrap,
//	Mirror,
//	Clamp,
//	Border,
//	Mirror_Once,
//}
//
//public struct Sampler
//{
//	/// <summary>
//	/// Name of this Sampler
//	/// </summary>
//	public string Name { get; set; } = "";
//
//	/// <summary>
//	/// Smooth or Pixelated filtering
//	/// </summary>
//	public SamplerFilter Filter { get; set; } = SamplerFilter.Aniso;
//
//	/// <summary>
//	/// Horizontal wrapping, repeating or stretched
//	/// </summary>
//	public SamplerAddress AddressU { get; set; } = SamplerAddress.Wrap;
//
//	/// <summary>
//	/// Vertical wrapping, repeating or stretched
//	/// </summary>
//	public SamplerAddress AddressV { get; set; } = SamplerAddress.Wrap;
//
//	public readonly string CreateSampler( string name )
//	{
//		return $"SamplerState g_s{name}";
//	}
//
//	public Sampler()
//	{
//	}
//
//	public Sampler( string name )
//	{
//		Name = name;
//	}
//}
//
////[CustomEditor( typeof( Sampler ) )]
//public class SamplerControlWidget : ControlObjectWidget
//{
//	public override bool SupportsMultiEdit => false;
//
//	public SamplerControlWidget( SerializedProperty property ) : base( property, true )
//	{
//		Layout = Layout.Column();
//		Layout.Spacing = 4;
//
//		if ( SerializedObject == null )
//			return;
//
//		SerializedObject.TryGetProperty( nameof( Sampler.Name ), out var name );
//		SerializedObject.TryGetProperty( nameof( Sampler.Filter ), out var filter );
//		SerializedObject.TryGetProperty( nameof( Sampler.AddressU ), out var addressU );
//		SerializedObject.TryGetProperty( nameof( Sampler.AddressV ), out var addressV );
//
//		Layout.Add( Create( name ) );
//		Layout.Add( Create( filter ) );
//		Layout.Add( Create( addressU ) );
//		Layout.Add( Create( addressV ) );
//	}
//
//	//protected override void OnPaint()
//	//{
//	//	// Overriding and doing nothing here will prevent the default background from being painted
//	//}
//}
