using FairMultiplayerCutsceneExperience.Menus;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;

namespace FairMultiplayerCutsceneExperience.Patches
{
    public static class StartCutscenePatch
    {
        public static void ApplyPatch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Event), "InitializeEvent"),
                postfix: new HarmonyMethod(typeof(StartCutscenePatch), nameof(Postfix))
            );
        }

        public static void Postfix(GameLocation location, GameTime time, Event __instance)
        {
            if (Game1.player == null || __instance.isFestival)
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