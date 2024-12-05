using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace MultiplayerCutsceneNotification
{
    internal sealed class ModEntry : Mod
    {
        private readonly HashSet<long> _eventLoggedForPlayers = new();
        private bool _isCutsceneActive = false;

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

                if (!_eventLoggedForPlayers.Contains(playerId) &&
                    Game1.CurrentEvent.eventCommands.Contains("skippable"))
                {
                    string playerName = initiator.Name;
                    string message = $"{playerName} has started a cutscene!";

                    Monitor.Log(message, LogLevel.Info);
                    Game1.chatBox.addMessage(message, Color.Gold);

                    _eventLoggedForPlayers.Add(playerId);

                    _isCutsceneActive = true;
                }
            }
            else if (_isCutsceneActive &&
                     _eventLoggedForPlayers.Contains(Game1.player.UniqueMultiplayerID) &&
                     Game1.CurrentEvent == null)
            {
                _eventLoggedForPlayers.Remove(Game1.player.UniqueMultiplayerID);
                _isCutsceneActive = false;
            }
        }
    }
}