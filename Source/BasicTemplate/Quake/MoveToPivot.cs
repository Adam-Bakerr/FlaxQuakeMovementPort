using System;
using System.Collections.Generic;
using FlaxEngine;

namespace Game
{
    /// <summary>
    /// MoveToPivot Script.
    /// </summary>
    public class MoveToPivot : Script
    {
        public Actor Pivot;
        public override void OnStart()
        {
        }
        
        /// <inheritdoc/>
        public override void OnEnable()
        {
            // Here you can add code that needs to be called when script is enabled (eg. register for events)
        }

        /// <inheritdoc/>
        public override void OnDisable()
        {
            // Here you can add code that needs to be called when script is disabled (eg. unregister from events)
        }

        /// <inheritdoc/>
        public override void OnUpdate()
        {
            Actor.Position = Pivot.Position;
            Actor.Rotation = Pivot.Rotation;
        }
    }
}
