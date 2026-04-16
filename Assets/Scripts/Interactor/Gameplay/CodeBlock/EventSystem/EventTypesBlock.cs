public enum EventTypesBlock
{
    OnStackExecutionStart, OnStackExecutionEnd,
    // v2.11.1 - added new block events for drop actions: OnDropAtStack, OnDropAtInputSpot, OnDropAtProgrammingEnv, OnDropDestroy
    OnDrop, OnDropAtStack, OnDropAtInputSpot, OnDropAtProgrammingEnv, OnDropDestroy,
    // v2.12 - new drag block and function blocks events added
    OnDragOut, OnDragFromStack, OnDragFromInputSpot, OnDragFromProgrammingEnv, OnDragFromOutside,
    OnFunctionDefinitionAdded, OnFunctionDefinitionRemoved
}