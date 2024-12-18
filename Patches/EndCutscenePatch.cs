using HarmonyLib;
using StardewValley;

namespace FairMultiplayerCutsceneExperience.Patches
{
    [HarmonyPatch(typeof(Event), "exitEvent")]
    public static class EndCutscenePatch
    {
        public static void Postfix()
        {
            var playerId = Game1.player.UniqueMultiplayerID;

            if (!ModEntry.IsCutsceneActive() && !ModEntry.IsPlayerInCutscene(playerId))
                return;

            ModEntry.SendEndPauseMessageToAll(playerId);
            ModEntry.CutsceneInitiators.Remove(playerId);

            // If there is someone else still in the cutscene, start the pause
            if (ModEntry.IsCutsceneActive())
                ModEntry.StartPause();

            var message = ModEntry.GetString("message.finishCutscene", new { Game1.player.Name });
            ModEntry.BroadcastMessage(message);
        }
    }
}