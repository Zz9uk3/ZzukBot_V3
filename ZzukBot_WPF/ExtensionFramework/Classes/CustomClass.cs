using System;
using System.Collections.Generic;
using ZzukBot.Constants;
using ZzukBot.Objects;

namespace ZzukBot.ExtensionFramework.Classes
{
    /// <summary>
    /// Template for a CustomClass
    /// </summary>
    public class CustomClass : IDisposable
    {
        #region old custom class code

        /// <summary>
        /// The WoW class the CustomClass is designed for
        /// </summary>
        public virtual Enums.ClassId Class { get; protected set; }
        
        /// <summary>
        /// Should be called when the CC is loaded
        /// </summary>
        /// <returns></returns>
        public virtual bool Load()
        {
            return true;
        }

        /// <summary>
        /// Should be called when the CC is unloaded
        /// </summary>
        public virtual void Unload()
        {
        }

        /// <summary>
        /// Should be called to show the settings form
        /// </summary>
        public virtual void ShowGui()
        {
        }

        /// <summary>
        /// The name of the CC
        /// </summary>
        public virtual string Name { get; protected set; }

        /// <summary>
        /// The author of the CC
        /// </summary>
        public virtual string Author { get; protected set; }

        /// <summary>
        /// The version of the CC
        /// </summary>
        public virtual Version Version { get; protected set; }

        /// <summary>
        /// Should disable the combat movement of the botbase while true
        /// </summary>
        public virtual bool SuppressBotMovement { get; protected set; }

        /// <inheritdoc />
        public virtual void Dispose()
        {
            
        }

        #endregion
        
        /// <summary>
        /// Determine is it possible to win if we rush into battle, 
        /// hard to calculate it at botbase because that strongly depends on equip and class 
        /// </summary>
        /// <returns></returns>
        public virtual bool CanWin(IEnumerable<WoWUnit> possibleTargets)
        {
            return true;
        }

        /// <summary>
        /// Determine if we are ready to fight
        /// </summary>
        /// <returns></returns>
        public virtual bool IsReadyToFight(IEnumerable<WoWUnit> possibleTargets)
        {
            return true;
        }

        /// <summary>
        /// heal/mana restore wait for some CD etc
        /// </summary>
        /// <returns></returns>
        public virtual void PrepareForFight()
        {
            
        }

        /// <summary>
        /// Call every time we can do any action
        /// </summary>
        public virtual void Fight(IEnumerable<WoWUnit> possibleTargets)
        {
            
        }

        /// <summary>
        /// Pulls mob
        /// </summary>
        /// <param name="target"></param>
        public virtual void Pull(WoWUnit target)
        {
        }

        /// <summary>
        /// Should used for reset values like distance
        /// </summary>
        public virtual void OnFightEnded()
        {
            
        }

        /// <summary>
        /// Indicate that some of buffs are missed
        /// </summary>
        /// <returns></returns>
        public virtual bool IsBuffRequired()
        {
            return false;
        }

        /// <summary>
        /// Perform Re-buff
        /// </summary>
        public virtual void Rebuff() { }

        /// <summary>
        /// No description yet
        /// </summary>
        /// <returns></returns>
        public virtual bool CanBuffAnotherPlayer()
        {
            return false;
        }

        /// <summary>
        /// No description yet
        /// </summary>
        /// <param name="player"></param>
        public virtual void TryToBuffAnotherPlayer(WoWUnit player)
        {
        }

        /// <summary>
        /// Distance from which to pull
        /// </summary>
        /// <returns></returns>
        public virtual float GetPullDistance()
        {
            return 25;
        }

        /// <summary>
        /// Distance that need to hold while kiting, 
        /// have effect only when GetCombatPosition returns Kite
        /// </summary>
        /// <returns></returns>
        public virtual float GetKiteDistance()
        {
            return 18;
        }

        /// <summary>
        /// Max mobs that would be pulled, 
        /// less or equal to zero should be treated as one
        /// </summary>
        /// <returns></returns>
        public virtual int GetMaxPullCount()
        {
            return 1;
        }

        /// <summary>
        /// Position which a bot must adhere to while in combat
        /// </summary>
        /// <returns></returns>
        public virtual Enums.CombatPosition GetCombatPosition()
        {
            return Enums.CombatPosition.Before;
        }
    }
}
