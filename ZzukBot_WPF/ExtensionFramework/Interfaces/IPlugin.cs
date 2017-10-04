using System;

namespace ZzukBot.ExtensionFramework.Interfaces
{
    /// <summary>
    ///     An interface for creating plugins
    /// </summary>
    public interface IPlugin : IDisposable
    {
        /// <summary>
        ///     Gets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        string Name { get; }

        /// <summary>
        ///     Gets the author.
        /// </summary>
        /// <value>
        ///     The author.
        /// </value>
        string Author { get; }

        /// <summary>
        ///     Gets the version.
        /// </summary>
        /// <value>
        ///     The version.
        /// </value>
        Version Version { get; }

        /// <summary>
        ///     Loads the plugin
        /// </summary>
        /// <returns>Returns true when the plugin was loaded succesfully</returns>
        bool Load();

        /// <summary>
        ///     Unloads the plugin
        /// </summary>
        void Unload();

        /// <summary>
        ///     Method will be called to open the Plugin GUI
        /// </summary>
        void ShowGui();
    }
}