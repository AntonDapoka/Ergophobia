public enum EventTypes
{
    OnPlay, OnStop,
    // v2.7 - OnPointerUpEnd event name changed to OnPrimaryKeyUpEnd
    OnDrag, OnPrimaryKeyUpEnd,
    OnAnyVariableValueChanged, OnAnyVariableAddedOrRemoved,
    OnBlocksStackArrayUpdate,
    // v2.7 - added new events
    OnPrimaryKeyDown, OnPrimaryKey, OnPrimaryKeyUp, OnSecondaryKeyDown, OnPrimaryKeyHold,
    OnDeleteKeyDown,
    // v2.10 - new events added to enable the use of an auxiliary key to change the programmingEnv zoom using key+scroll: OnSecondaryKeyUp, OnAuxKeyDown, OnAuxKeyUp
    OnSecondaryKeyUp, OnAuxKeyDown, OnAuxKeyUp,
    // v2.12 - new general OnBlockDrop event added
    OnBlockDrop,
    // v2.13 - OnDragStart event added
    OnDragStart
}