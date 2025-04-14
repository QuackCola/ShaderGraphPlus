
using System.Text.Json.Nodes;

namespace Editor.ShaderGraphPlus;

public partial class ShaderGraphPlus
{
    [JsonUpgrader( typeof( ShaderGraphPlus ), 1 )]
    static void Upgrader_v1( JsonObject obj )
    {
       if ( obj.ContainsKey( "MaterialDomain" ) )
       {
           var value = obj["MaterialDomain"].GetValue<MaterialDomain>();

           obj["Domain"] = (int)value;
       }
    }
}