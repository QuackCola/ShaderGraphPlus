using System.Text.Json.Nodes;


//[JsonUpgrader( typeof( ShrimpleCharacterController ), 3 )]
//private static void UpdateTraceShape( JsonObject json )
//{
//	if ( json.Remove( "CylinderTrace", out var newNode ) && newNode is JsonValue boolNode && boolNode.TryGetValue<bool>( out var isCylinder ) )
//		json["TraceShape"] = isCylinder ? "Cylinder" : "Box";
//	else
//		json["TraceShape"] = "Box";
//}
