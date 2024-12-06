using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Minigames;

namespace MultiplayerCutsceneNotification
{
    internal sealed class ModEntry : Mod
    {
        private const string MessageTypeSendChatMessage = "sendChatMessage";
        private const string MessageTypeOpenMinigame = "openMinigame";
        private const string MessageTypeCloseMinigame = "closeMinigame";
        private const string MessageTypeAddPlayerToInitiators = "addPlayerToInitiators";
        private const string MessageTypeRemovePlayerFromInitiators = "removePlayerFromInitiators";

        private readonly HashSet<long> _cutsceneInitiators = new();

        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Display.RenderingHud += OnRenderingHud;
            helper.Events.Multiplayer.ModMessageReceived += OnModMessageReceived;
        }

        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (Game1.CurrentEvent != null)
            {
                Farmer? initiator = Game1.CurrentEvent.farmer;
                if (initiator == null)
                    return;

                long playerId = initiator.UniqueMultiplayerID;

                if (!IsPlayerInCutscene(playerId) &&
                    Game1.CurrentEvent.eventCommands.Contains("skippable"))
                {
                    string message = $"{initiator.Name} has started a cutscene!";
                    BroadcastMessage(message);

                    _cutsceneInitiators.Add(playerId);
                    SendAddInitiatorMessageToAll();

                    SendOpenMinigameMessageToAll();
                }
            }
            else if (IsCutsceneActive() &&
                     IsPlayerInCutscene(Game1.player.UniqueMultiplayerID) &&
                     Game1.CurrentEvent == null)
            {
                SendCloseMinigameMessageToAll();

                _cutsceneInitiators.Remove(Game1.player.UniqueMultiplayerID);
                SendRemoveInitiatorMessageToAll();

                string message = $"{Game1.player.Name} has finished a cutscene!";
                BroadcastMessage(message);
            }
        }

        private void OnModMessageReceived(object? sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID == ModManifest.UniqueID)
            {
                switch (e.Type)
                {
                    case MessageTypeSendChatMessage:
                        Game1.chatBox.addMessage(e.ReadAs<string>(), Color.Gold);
                        break;
                    case MessageTypeOpenMinigame:
                        if (!IsPlayerInCutscene(Game1.player.UniqueMultiplayerID))
                            OpenMinigame();
                        break;
                    case MessageTypeCloseMinigame:
                        if (!IsPlayerInCutscene(Game1.player.UniqueMultiplayerID))
                            CloseMinigame();
                        break;
                    case MessageTypeAddPlayerToInitiators:
                        _cutsceneInitiators.Add(e.FromPlayerID);
                        break;
                    case MessageTypeRemovePlayerFromInitiators:
                        _cutsceneInitiators.Remove(e.FromPlayerID);
                        break;
                }
            }
        }

        private void OnRenderingHud(object? sender, RenderingHudEventArgs e)
        {
            if (
                IsCutsceneActive() &&
                !IsPlayerInCutscene(Game1.player.UniqueMultiplayerID))
            {
                OpenMinigame();
            }
        }

        private void OpenMinigame()
        {
            Game1.currentMinigame = new MineCart(new Random().Next(0, 9), 3);
        }

        private void CloseMinigame()
        {
            Game1.currentMinigame = null;
        }

        private void BroadcastMessage(string message)
        {
            Monitor.Log(message, LogLevel.Info);
            Game1.chatBox.addMessage(message, Color.Gold);
            SendChatMessageToAll(message);
        }

        private void SendChatMessageToAll(string message)
        {
            Helper.Multiplayer.SendMessage(
                message: message,
                messageType: MessageTypeSendChatMessage,
                modIDs: new[] { ModManifest.UniqueID }
            );
        }

        private void SendOpenMinigameMessageToAll()
        {
            Helper.Multiplayer.SendMessage(
                message: MessageTypeOpenMinigame,
                messageType: MessageTypeOpenMinigame,
                modIDs: new[] { ModManifest.UniqueID }
            );
        }

        private void SendCloseMinigameMessageToAll()
        {
            Helper.Multiplayer.SendMessage(
                message: MessageTypeCloseMinigame,
                messageType: MessageTypeCloseMinigame,
                modIDs: new[] { ModManifest.UniqueID }
            );
        }

        private void SendAddInitiatorMessageToAll()
        {
            Helper.Multiplayer.SendMessage(
                message: MessageTypeAddPlayerToInitiators,
                messageType: MessageTypeAddPlayerToInitiators,
                modIDs: new[] { ModManifest.UniqueID }
            );
        }

        private void SendRemoveInitiatorMessageToAll()
        {
            Helper.Multiplayer.SendMessage(
                message: MessageTypeRemovePlayerFromInitiators,
                messageType: MessageTypeRemovePlayerFromInitiators,
                modIDs: new[] { ModManifest.UniqueID }
            );
        }

        private bool IsPlayerInCutscene(long playerId)
        {
            return _cutsceneInitiators.Contains(playerId);
        }

        private bool IsCutsceneActive()
        {
            return _cutsceneInitiators.Count > 0;
        }
    }
}