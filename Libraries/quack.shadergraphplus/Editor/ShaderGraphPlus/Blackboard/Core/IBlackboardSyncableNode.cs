namespace ShaderGraphPlus;

internal interface IBlackboardSyncableNode
{
	Guid BlackboardParameterIdentifier { get; set; }
	void UpdateFromBlackboard( BaseBlackboardParameter parameter );
}
