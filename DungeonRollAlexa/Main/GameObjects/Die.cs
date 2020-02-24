using System;
using System.Collections.Generic;
using System.Text;

namespace DungeonRollAlexa.Main.GameObjects
{
    /// <summary>
    /// Parent class for the dice. Dungeon and party dice should inherit from  this.
    /// </summary>
    public abstract class Die
    {
        /// <summary>
        /// Name of the current die value.
        /// </summary>
        public virtual string Name { get;  }

        /// <summary>
        /// If true, die is selected for being rerolled or for an action to be applied to it.
        /// </summary>
        public bool IsSelected { get; set; }

        public abstract void Roll();


    }
}
