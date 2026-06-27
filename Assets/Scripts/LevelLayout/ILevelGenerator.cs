using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstraction over level generation used by <see cref="StageSwitchScript"/>.
/// Implementations can define their own pools of rooms, enemies and CodeBlocks.
/// </summary>
public interface ILevelGenerator
{
    /// <summary>
    /// Invoked after the new level has been fully generated and connected.
    /// </summary>
    event Action<List<RoomScript>> OnLevelGenerated;

    /// <summary>
    /// Clears any existing level and generates a new one.
    /// </summary>
    /// <param name="playFadeIn">If true, the generator may fade the screen in after generation.
    /// Set to false when the caller handles fading (e.g. stage switching).</param>
    void GenerateLevel(bool playFadeIn = true);

    /// <summary>
    /// Destroys or returns to pool all currently spawned rooms.
    /// </summary>
    void ClearLevel();
}
