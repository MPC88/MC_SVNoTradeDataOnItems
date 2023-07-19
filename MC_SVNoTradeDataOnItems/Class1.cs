
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;

namespace MC_SVNoTradeDataOnItems
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string pluginGuid = "mc.starvalor.notradedataonitems";
        public const string pluginName = "SV No Trade Data On Items";
        public const string pluginVersion = "2.0.1";

        private readonly int[] idAmmo = { 20, 21, 22, 53 };        
        private readonly int idEnergyCells = 18;
        private readonly int idDroneParts = 23;
        private readonly int[] idCrafting = { 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52 };

        private static ConfigEntry<bool> cfgEnabled;
        private static ConfigEntry<bool> cfgReducedMode;
        private static ConfigEntry<bool> cfgAmmo;
        private static ConfigEntry<bool> cfgEnergyCells;
        private static ConfigEntry<bool> cfgDroneParts;
        private static ConfigEntry<bool> cfgCrafting;

        private bool lastReducedMode;
        private bool lastAmmo;
        private bool lastEnergyCells;
        private bool lastDroneParts;
        private bool lastCrafting;

        private static List<int> reducedModeActiveItems = new List<int>();

        public void Awake()
        {
            cfgEnabled = Config.Bind<bool>("1. Enable/Disable Mod",
                "1. Enable mod?",
                true,
                "Enables/disables trade data.  Disabling will not restore trade data for items where it has been removed.");
            cfgReducedMode = Config.Bind<bool>("2. Reduced Mode",
                "1. Reduced mode?",
                true,
                "Only the checked item sets below have data removed.  If this setting is not checked, all items have data removed.");            
            cfgAmmo = Config.Bind<bool>("2. Reduced Mode",
                "2. Remove from ammo?",
                true,
                "");            
            cfgEnergyCells = Config.Bind<bool>("2. Reduced Mode",
                "3. Remove from energy cells?",
                true,
                "");
            cfgDroneParts = Config.Bind<bool>("2. Reduced Mode",
                "4. Remove from drone parts?",
                true,
                "");
            cfgCrafting = Config.Bind<bool>("2. Reduced Mode",
                "5. Remove from crafting items?",
                true,
                "Stashable crafting items only e.g. upgrade kits, scrap metal, transmitters etc.");
            lastAmmo = !cfgAmmo.Value; // Forces update on first pass
            Harmony.CreateAndPatchAll(typeof(Main));
        }

        public void Update()
        {
            if (cfgEnabled.Value && cfgReducedMode.Value &&
                (lastReducedMode != cfgReducedMode.Value ||
                lastAmmo != cfgAmmo.Value ||
                lastEnergyCells != cfgEnergyCells.Value ||
                lastDroneParts != cfgDroneParts.Value ||
                lastCrafting != cfgCrafting.Value)
                )
            {
                reducedModeActiveItems.Clear();
                if (cfgAmmo.Value)
                    reducedModeActiveItems.AddRange(idAmmo);
                if (cfgEnergyCells.Value)
                    reducedModeActiveItems.Add(idEnergyCells);
                if (cfgDroneParts.Value)
                    reducedModeActiveItems.Add(idDroneParts);
                if (cfgCrafting.Value)
                    reducedModeActiveItems.AddRange(idCrafting);
                                
                lastAmmo = cfgAmmo.Value;
                lastEnergyCells = cfgEnergyCells.Value;
                lastDroneParts = cfgDroneParts.Value;
                lastCrafting = cfgCrafting.Value;
            }

            if(!cfgReducedMode.Value && lastReducedMode)
                lastReducedMode = cfgReducedMode.Value;
        }

        [HarmonyPatch(typeof(CargoSystem), "StoreItem", new System.Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(float), typeof(int), typeof(int), typeof(CI_Data)})]
        [HarmonyPrefix]
        private static void CargoSystem_StoreItem_Pre(int itemID, ref float pricePaid)
        {        
            if (cfgEnabled.Value && pricePaid != 0f && 
                (!cfgReducedMode.Value || (cfgReducedMode.Value && reducedModeActiveItems.Contains(itemID))))
            {
                    pricePaid = 0f;
            }
        }
    }
}
