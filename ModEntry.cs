using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace MultiplayerCutsceneNotification
{
    internal sealed class ModEntry : Mod
    {
        private readonly HashSet<long> _initiatorIds = new();
        private bool _isCutsceneActive;

        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }

        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (Game1.CurrentEvent != null)
            {
                Farmer? initiator = Game1.CurrentEvent.farmer;
                if (initiator == null)
                    return;

                long playerId = initiator.UniqueMultiplayerID;

                if (!_initiatorIds.Contains(playerId) &&
                    Game1.CurrentEvent.eventCommands.Contains("skippable"))
                {
                    string playerName = initiator.Name;
                    string message = $"{playerName} has started a cutscene!";

                    Monitor.Log(message, LogLevel.Info);
                    Game1.chatBox.addMessage(message, Color.Gold);

                    _initiatorIds.Add(playerId);

                    _isCutsceneActive = true;
                }
            }
            else if (_isCutsceneActive &&
                     _initiatorIds.Contains(Game1.player.UniqueMultiplayerID) &&
                     Game1.CurrentEvent == null)
            {
                _initiatorIds.Remove(Game1.player.UniqueMultiplayerID);

                if (_initiatorIds.Count == 0)
                    _isCutsceneActive = false;

                string message = $"{Game1.player.Name} has finished a cutscene!";

                Monitor.Log(message, LogLevel.Info);
                Game1.chatBox.addMessage(message, Color.Gold);
            }
        }
    }
}