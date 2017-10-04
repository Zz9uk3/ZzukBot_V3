using System;

namespace ZzukBot.ExtensionFramework.Interfaces
{
    /// <summary>
    ///     An interface for creating BotBases
    /// </summary>
    public interface IBotBase : IDisposable
    {
        /// <summary>
        ///     Gets the name.
        /// </summary>
        /// <value>
        ///     The name.
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
        ///     Starts the Botbase
        /// </summary>
        /// <param name="onStopCallback">The callback which will be called after stopping the botbase</param>
        /// <returns></returns>
        bool Start(Action onStopCallback);

        /// <summary>
        /// Can be used to signalise the botbase that it needs to take a break from its activities. Call ResumeBotbase to continue the botbase
        /// </summary>
        /// <param name="onPauseCallback">Will be called after the botbase completele paused all activities</param>
        void PauseBotbase(Action onPauseCallback);

        /// <summary>
        /// Continue the activities after previously pausing the botbase using <see cref="PauseBotbase"/>
        /// </summary>
        /// <returns><c>true</c> means the botbase got resumed successfully. Will return <c>false</c> if the botbase is already running</returns>
        bool ResumeBotbase();

        /// <summary>
        ///     Stops the botbase
        /// </summary>
        void Stop();

        /// <summary>
        ///     Show the GUI of the botbase
        /// </summary>
        void ShowGui();
    }
}