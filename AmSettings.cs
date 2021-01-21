using Modding;
using System.Collections.Generic;

namespace AdditionalMaps
{
    public class AmSaveSettings : ModSettings
    {
        // Start Mod Quest
        public bool SFGrenadeTestOfTeamwork_StartQuest { get => GetBool(false); set => SetBool(value); }
        // Mechanics
        public bool SFGrenadeTestOfTeamwork_HornetCompanion { get => GetBool(false); set => SetBool(value); }
        // Bosses
        public bool SFGrenadeTestOfTeamwork_EncounterBeforeBoss { get => GetBool(false); set => SetBool(value); }
        public bool SFGrenadeTestOfTeamwork_DefeatedWeaverPrincess { get => GetBool(false); set => SetBool(value); }
        // Areas
        public bool SFGrenadeTestOfTeamwork_TotOpened { get => GetBool(false); set => SetBool(value); }
        public bool SFGrenadeTestOfTeamwork_VisitedTestOfTeamwork { get => GetBool(false); set => SetBool(value); }
        public bool SFGrenadeTestOfTeamwork_TotOpenedShortcut { get => GetBool(false); set => SetBool(value); }
        public bool SFGrenadeTestOfTeamwork_TotOpenedTotem { get => GetBool(false); set => SetBool(value); }

        // Better charms
        public List<bool> gotCharms = new List<bool>() { true, true, true, true };
        public List<bool> newCharms = new List<bool>() { false, false, false, false };
        public List<bool> equippedCharms = new List<bool>() { false, false, false, false };
        public List<int> charmCosts = new List<int>() { 1, 1, 1, 1 };
    }

    public class AmGlobalSettings : ModSettings
    {
    }
}
