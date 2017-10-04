using ZzukBot.Constants;

namespace ZzukBot.Game.Frames.FrameObjects
{
    /// <summary>
    ///     Representing Gossip Options related to a quest
    /// </summary>
    public abstract class QuestOption
    {
        internal QuestOption(int parId, string parText, Enums.QuestGossipState parState)
        {
            Id = parId;
            Text = parText;
            State = parState;
        }

        /// <summary>
        ///     The state of the Gossip Quest Option (Accept, Complete etc.)
        /// </summary>
        /// <value>
        ///     The state.
        /// </value>
        public Enums.QuestGossipState State { get; }

        /// <summary>
        ///     The text of the Gossip Quest Option
        /// </summary>
        /// <value>
        ///     The text.
        /// </value>
        public string Text { get; }

        /// <summary>
        ///     The ID of the Quest related to the Gossip Dialog
        /// </summary>
        /// <value>
        ///     The identifier.
        /// </value>
        public int Id { get; }
    }
}