using FairMultiplayerCutsceneExperience.Menus;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;

namespace FairMultiplayerCutsceneExperience.Patches
{
    [HarmonyPatch(typeof(Event), "InitializeEvent")]
    public static class StartCutscenePatch
    {
        public static void Postfix(GameLocation location, GameTime time)
        {
            if (Game1.player == null)
                return;

            var playerId = Game1.player.UniqueMultiplayerID;

            if (ModEntry.IsPlayerInCutscene(playerId))
                return;

            if (Game1.activeClickableMenu is CutscenePauseMenu)
                ModEntry.EndPause();

            var message = ModEntry.GetString("message.startCutscene", new { Game1.player.Name });
            ModEntry.BroadcastMessage(message);

            ModEntry.CutsceneInitiators.Add(playerId);
            ModEntry.SendStartPauseMessageToAll(playerId);
        }
    }
}