using FairMultiplayerCutsceneExperience.Config;
using FairMultiplayerCutsceneExperience.Menus;
using FairMultiplayerCutsceneExperience.Utils;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace FairMultiplayerCutsceneExperience
{
    internal sealed class ModEntry : Mod
    {
        private const string MessageTypeSendChatMessage = "sendChatMessage";
        private const string MessageTypeOpenPauseMenu = "openPauseMenu";
        private const string MessageTypeClosePauseMenu = "closePauseMenu";
        private const string MessageTypeStartPause = "startPause";
        private const string MessageTypeSpecificStartPause = "specificStartPause";
        private const string MessageTypeEndPause = "endPause";
        private const string MessageTypeResetPauseState = "resetPauseState";

        public static ButtonState PreviousLeftButtonState = ButtonState.Released;
        public static readonly HashSet<long> CutsceneInitiators = new();
        public static IModHelper StaticHelper = null!;

        private static IMonitor _monitor = null!;
        private static ModConfig _config = null!;
        private static string? _hostModVersion;
        private static string? _currTip;

        public override void Entry(IModHelper helper)
        {
            StaticHelper = helper;
            _monitor = Monitor;
            _config = helper.ReadConfig<ModConfig>();

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

            helper.Events.Display.RenderingHud += OnRenderingHud;
            helper.Events.Multiplayer.ModMessageReceived += OnModMessageReceived;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.Multiplayer.PeerConnected += OnPeerConnected;
            helper.Events.Multiplayer.PeerDisconnected += OnPeerDisconnected;
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;

            helper.ConsoleCommands.Add("reset", helper.Translation.Get("command.resetCommandDescription"),
                ResetConsoleCommand);

            ChatCommands.Register(
                "reset",
                ResetChatBoxCommand,
                name => helper.Translation.Get("command.resetCommandDescription"),
                cheatsOnly: false
            );
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            if (StaticHelper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu"))
                SetupGenericModConfigMenu();
        }

        private void ResetChatBoxCommand(string[] args, ChatBox chat)
        {
            // Only the host can use the command
            if (Game1.player.UniqueMultiplayerID != Game1.MasterPlayer.UniqueMultiplayerID)
            {
                Game1.chatBox.addMessage($"[{ModManifest.Name}] " + GetString("command.resetCommandError"), Color.Red);
                return;
            }

            ResetPauseState();
            SendResetPauseStateToAll();
        }

        private void ResetConsoleCommand(string command, string[] args)
        {
            // Only the host can use the command
            if (Game1.player.UniqueMultiplayerID != Game1.MasterPlayer.UniqueMultiplayerID)
            {
                Monitor.Log(GetString("command.resetCommandError"), LogLevel.Error);
                return;
            }

            ResetPauseState();
            SendResetPauseStateToAll();
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            // Store the host mod version
            if (Game1.player.UniqueMultiplayerID == Game1.MasterPlayer.UniqueMultiplayerID)
            {
                _hostModVersion = ModManifest.Version.ToString();
            }
        }

        private void OnPeerConnected(object? sender, PeerConnectedEventArgs e)
        {
            var peer = e.Peer;
            var peerName = Game1.getOnlineFarmers().ToList()
                .Find(farmer => farmer.UniqueMultiplayerID == peer.PlayerID)?.Name;
            var peerMod = peer.Mods.FirstOrDefault(mod => mod.ID == ModManifest.UniqueID);

            if (peer.IsHost)
            {
                _hostModVersion = peerMod?.Version.ToString();
                return;
            }

            // Only the host performs necessary actions when a peer connects
            if (Game1.player.UniqueMultiplayerID != Game1.MasterPlayer.UniqueMultiplayerID)
                return;

            Thread.Sleep(3000);

            // Check if cutscene is active and open menu for the peer
            if (
                IsCutsceneActive() &&
                !IsPlayerInCutscene(e.Peer.PlayerID))
            {
                SendSpecificStartPauseMessage(e.Peer.PlayerID, CutsceneInitiators.ToArray()[0]);
            }

            // Handle mod version mismatch
            if (peerMod == null)
            {
                var message = $"[{ModManifest.Name}] " + GetString("message.noMod", new { name = peerName });
                BroadcastMessage(message, true);
                return;
            }

            if (peerMod?.Version.ToString() != _hostModVersion)
            {
                var message = $"[{ModManifest.Name}] " + GetString(
                    "message.modVersionMismatch",
                    new { name = peerName, modVersion = peerMod?.Version.ToString(), hostModVersion = _hostModVersion }
                );
                BroadcastMessage(message, true);
            }
        }

        private void OnPeerDisconnected(object? sender, PeerDisconnectedEventArgs e)
        {
            // When a peer disconnects, remove them from the cutscene initiators
            if (!CutsceneInitiators.Contains(e.Peer.PlayerID))
                return;

            CutsceneInitiators.Remove(e.Peer.PlayerID);

            EndPause();
            SendEndPauseMessageToAll(e.Peer.PlayerID);
        }

        private void OnModMessageReceived(object? sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID != ModManifest.UniqueID)
                return;

            switch (e.Type)
            {
                case MessageTypeResetPauseState:
                    ResetPauseState();
                    break;
                case MessageTypeSendChatMessage:
                    // Log and display chat messages received from other players
                    // Read message text and warning flag
                    var messageTuple = e.ReadAs<(string, bool)>();
                    Monitor.Log(messageTuple.Item1, messageTuple.Item2 ? LogLevel.Warn : LogLevel.Info);
                    Game1.chatBox.addMessage(messageTuple.Item1, messageTuple.Item2 ? Color.Orange : Color.Gold);
                    break;
                case MessageTypeOpenPauseMenu:
                    // Open pause menu with random tip on /pause command
                    if (StaticHelper.ReadConfig<ModConfig>().EnablePauseMenu)
                    {
                        MenuStack.PushMenu(new PauseMenu());
                    }

                    break;
                case MessageTypeClosePauseMenu:
                    // Close pause menu with random tip on /pause or /resume command
                    if (StaticHelper.ReadConfig<ModConfig>().EnablePauseMenu)
                    {
                        MenuStack.PopMenu();
                    }

                    break;
                case MessageTypeStartPause:
                    // Handle starting the pause when a cutscene begins
                    // Read initiator id
                    CutsceneInitiators.Add(e.ReadAs<long>());
                    if (!IsPlayerInCutscene(Game1.player.UniqueMultiplayerID))
                        StartPause();
                    break;
                case MessageTypeSpecificStartPause:
                    // Handle specific start of a pause for a player and initiator
                    // Read player id and initiator id
                    var playerIds = e.ReadAs<(long, long)>();
                    if (Game1.player.UniqueMultiplayerID == playerIds.Item1)
                    {
                        var initiatorName = Game1.otherFarmers.ToList()
                            .Find(farmer => farmer.Key == playerIds.Item2).Value.Name;

                        Game1.chatBox.addMessage(
                            GetString("message.startCutscene", new { name = initiatorName }), Color.Gold);

                        CutsceneInitiators.Add(playerIds.Item2);
                        StartPause();
                    }

                    break;
                case MessageTypeEndPause:
                    // Handle the end of the pause
                    // Read initiator id
                    CutsceneInitiators.Remove(e.ReadAs<long>());
                    if (!IsCutsceneActive())
                        EndPause();
                    break;
            }
        }

        private void OnRenderingHud(object? sender, RenderingHudEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.activeClickableMenu != null)
                return;

            // Open pause menu if a cutscene is active and the player is not in the cutscene
            if (
                IsCutsceneActive() &&
                !IsPlayerInCutscene(Game1.player.UniqueMultiplayerID))
            {
                StartPause();
            }
        }

        private void ResetPauseState()
        {
            PreviousLeftButtonState = ButtonState.Released;

            if (IsPlayerInCutscene(Game1.player.UniqueMultiplayerID))
            {
                Game1.CurrentEvent?.skipEvent();
                Game1.CurrentEvent?.exitEvent();
            }

            CutsceneInitiators.Clear();

            Game1.currentMinigame?.unload();
            Game1.currentMinigame = null;
            Game1.activeClickableMenu = null;
            _currTip = null;

            Game1.chatBox.addMessage(
                $"[{ModManifest.Name}] " + GetString("command.resetCommandResult"), Color.Gold);
            Monitor.Log(GetString("command.resetCommandResult"), LogLevel.Info);
        }

        public static void BroadcastMessage(string message, bool isWarning = false)
        {
            _monitor.Log(message, isWarning ? LogLevel.Warn : LogLevel.Info);
            Game1.chatBox.addMessage(message, isWarning ? Color.Orange : Color.Gold);
            SendChatMessageToAll(message, isWarning);
        }

        public static void SendChatMessageToAll(string message, bool isError = false)
        {
            StaticHelper.Multiplayer.SendMessage(
                message: (message, isError),
                messageType: MessageTypeSendChatMessage,
                modIDs: new[] { StaticHelper.ModRegistry.ModID }
            );
        }

        public static void SendOpenPauseMenuMessageToAll()
        {
            StaticHelper.Multiplayer.SendMessage(
                message: MessageTypeOpenPauseMenu,
                messageType: MessageTypeOpenPauseMenu,
                modIDs: new[] { StaticHelper.ModRegistry.ModID }
            );
        }

        public static void SendClosePauseMenuMessageToAll()
        {
            StaticHelper.Multiplayer.SendMessage(
                message: MessageTypeClosePauseMenu,
                messageType: MessageTypeClosePauseMenu,
                modIDs: new[] { StaticHelper.ModRegistry.ModID }
            );
        }

        public static void SendStartPauseMessageToAll(long initiatorId)
        {
            StaticHelper.Multiplayer.SendMessage(
                message: initiatorId,
                messageType: MessageTypeStartPause,
                modIDs: new[] { StaticHelper.ModRegistry.ModID }
            );
        }

        public static void SendSpecificStartPauseMessage(long playerId, long initiatorId)
        {
            StaticHelper.Multiplayer.SendMessage(
                message: (playerId, initiatorId),
                messageType: MessageTypeSpecificStartPause,
                modIDs: new[] { StaticHelper.ModRegistry.ModID }
            );
        }

        public static void SendEndPauseMessageToAll(long initiatorId)
        {
            StaticHelper.Multiplayer.SendMessage(
                message: initiatorId,
                messageType: MessageTypeEndPause,
                modIDs: new[] { StaticHelper.ModRegistry.ModID }
            );
        }

        public static void SendResetPauseStateToAll()
        {
            StaticHelper.Multiplayer.SendMessage(
                message: MessageTypeResetPauseState,
                messageType: MessageTypeResetPauseState,
                modIDs: new[] { StaticHelper.ModRegistry.ModID }
            );
        }

        public static void StartPause()
        {
            var initiatorName = Game1.getOnlineFarmers()
                .First(farmer => farmer.UniqueMultiplayerID == CutsceneInitiators.ToArray()[0]).Name;

            if (String.IsNullOrEmpty(_currTip))
                _currTip = GetRandomTip();

            MenuStack.PushMenu(new CutscenePauseMenu(initiatorName, _currTip));
        }

        public static void EndPause()
        {
            Game1.currentMinigame?.unload();
            Game1.currentMinigame = null;
            MenuStack.PopMenu();
            _currTip = null;
        }

        public static bool IsPlayerInCutscene(long playerId)
        {
            return CutsceneInitiators.Contains(playerId);
        }

        public static bool IsCutsceneActive()
        {
            return CutsceneInitiators.Count > 0;
        }

        public static string GetString(string key, object? tokens = null)
        {
            return StaticHelper.Translation.Get(key, tokens);
        }

        public static string GetRandomTip()
        {
            return GetString($"tips.tip{new Random().Next(1, 37)}");
        }

        private void SetupGenericModConfigMenu()
        {
            var gmcmApi = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (gmcmApi == null)
                return;

            gmcmApi.Register(
                mod: ModManifest,
                reset: () => _config = new ModConfig(),
                save: () => Helper.WriteConfig(_config)
            );

            gmcmApi.AddBoolOption(
                mod: ModManifest,
                name: () => GetString("config.option.EnablePauseMenu"),
                tooltip: () => GetString("config.option.EnablePauseMenu.tooltip"),
                getValue: () => _config.EnablePauseMenu,
                setValue: value => _config.EnablePauseMenu = value
            );
        }
    }
}