using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using DunGen;
using HarmonyLib;
using LethalLevelLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;


namespace MentalHospital
{

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class MentalHospitalDunGen : BaseUnityPlugin
    {
        private const string modGUID = "DantorMentalHospital";

        private const string modName = "Dantor's Mental Hospital";

        private const string modVersion = "1.2.0";

        private static MentalHospitalDunGen Instance;

        private readonly Harmony harmony = new Harmony("Dantor's Mental Hospital");

        public static ConfigEntry<int> configRarity;
        public static ConfigEntry<string> configMoons;
        public static ConfigEntry<bool> configGuaranteed;
        public static ConfigEntry<float> configMinSize;
        public static ConfigEntry<float> configMaxSize;
        public static ConfigEntry<float> configDynamicValue;
        public static ConfigEntry<bool> configDynamicToggle;
        public static ConfigEntry<bool> configGreenFog;
        public static ConfigEntry<bool> configFogDamage;

        internal ManualLogSource mls;
        public static AssetBundle Assets;


        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }

            mls = BepInEx.Logging.Logger.CreateLogSource(PluginInfo.PLUGIN_GUID);

            string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            Assets = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "hospitalbundle"));
            if (Assets == null)
            {
                mls.LogError("Failed to load DantorsMentalHospital assets.");
                return;
            }

            DunGen.Graph.DungeonFlow Hospitalflow = Assets.LoadAsset<DunGen.Graph.DungeonFlow>("Assets/Abandoned_Psychiatric_Hospitals/DunGen Data/HospitalFlow.asset");
            if (Hospitalflow == null)
            {
                mls.LogError("Failed to load DantorsMentalHospital Dungeon Flow.");
                return;
            }

            // Config setup
            configRarity = Config.Bind("General", "HospitalDefaultRarity", 100, new ConfigDescription("How rare it is for the mental hospital to be chosen. Higher values increases the chance of spawning the mental hospital. Vanillas' main dungeons use a value of 300. Google Weighted Random if you don't know how it works, as that's how Lethal Company rarities function."));
            configMoons = Config.Bind("General", "HospitalMoonsList", "Assurance@100,Vow@100,Offense@100,March@100,Rend@100,Dine@100,Titan@100,PsychSanctum@9999", new ConfigDescription("The moon(s) that the hospital can spawn on, in the form of a comma separated list of selectable level names and optionally a weight value by using an '@' and weight value after it (e.g. \"Titan@300,Dine,Rend@10,PsychSanctum@9999\")\nThe name matching is lenient and should pick it up if you use the terminal name or internal mod name. If no rarity is specified, the HospitalRarity parameter is used.\nThe following strings: \"all\", \"vanilla\", \"modded\", \"paid\", \"free\" are dynamic presets which add the dungeon to that specified group (string must only contain one of these, or a manual moon name list).\n"));
            configGuaranteed = Config.Bind("General", "HospitalGuaranteed", false, new ConfigDescription("\nSet it to true to guarantee the interior to spawn on any moon you visit. It works in any case except when if you use list mode"));
            configDynamicToggle = Config.Bind("Size", "DynamicScaleToggle", true, new ConfigDescription("Enables the next 3 options. \nATTENTION: READ CAREFULLY HOW IT WORKS. This adjust the dungeon size accordingly to which moon you visit, it is recommended to let it on true\nDefault: true"));
            configDynamicValue = Config.Bind("Size", "DynamicScaleValue", 0.8f, new ConfigDescription("If the DungeonMinSize/DungeonMaxSize is above or below the next two settings, the dungeon size multiplier will aproximate to the value between the moon's specific dungeon size and this value.\nExample 1: If set to 0, the dungeon size will not be higher than DungeonMaxSize.\nExample 2: If set to 0.5, the dungeon size will be between the DungeonMaxSize and the moon's dungeon size multiplier.\nExample 3: If Set To 1, the dungeon size will be the moon's dungeon size multiplier with no restrictions.\nATTENTION: It is recommended to let it at default value or lower, the closer to 1 the bigger the dungeon.\nDefault: 0.8"));
            configMinSize = Config.Bind("Size", "DungeonMinSize", 0.5f, new ConfigDescription("Input the minimum's dungeon size multiplier.\nDefault: 0.5"));
            configMaxSize = Config.Bind("Size", "DungeonMaxSize", 0.65f, new ConfigDescription("Input the maximum's dungeon size multiplier.\nDefault: 0.65"));
            configGreenFog = Config.Bind("Interior Settings", "GreenFogToggle", true, new ConfigDescription("Sets the Hospital fog from steams valves to green, this is just an aesthetic change"));
            configFogDamage = Config.Bind("Interior Settings", "FogDamageToggle", true, new ConfigDescription("Enables the damage over time if you are inside the fog for too long (It only works in the Mental Hospital)"));

            ExtendedDungeonFlow extendedDungeon = ScriptableObject.CreateInstance<ExtendedDungeonFlow>();
            extendedDungeon.contentSourceName = "Dantors Mental Hospital";
            extendedDungeon.dungeonFlow = Hospitalflow;
            //extendedDungeon.dungeonFirstTimeAudio = Assets.LoadAsset<AudioClip>("assets/Mods/SCP/snd/Horror8.ogg");
            extendedDungeon.dungeonDefaultRarity = configRarity.Value;

            int iDefaultRarity = configGuaranteed.Value ? 99999 : configRarity.Value; // If configGuaranteed is true, set rarity absurdly high
            string sMoonConfig = configMoons.Value.ToLower();
            if (sMoonConfig == "all")
            {
                extendedDungeon.dynamicLevelTagsList.Add(new StringWithRarity("Vanilla", iDefaultRarity));
                extendedDungeon.dynamicLevelTagsList.Add(new StringWithRarity("Custom", iDefaultRarity));
                mls.LogInfo("Registered DantorsMentalHospital dungeon for all moons.");
            }
            else if (sMoonConfig == "vanilla")
            {
                extendedDungeon.manualContentSourceNameReferenceList.Add(new StringWithRarity("Lethal Company", iDefaultRarity));
                mls.LogInfo("Registered DantorsMentalHospital dungeon for all vanilla moons.");
            }
            else if (sMoonConfig == "modded")
            {
                extendedDungeon.manualContentSourceNameReferenceList.Add(new StringWithRarity("Custom", iDefaultRarity));
                mls.LogInfo("Registered DantorsMentalHospital dungeon for all modded moons.");
            }
            else if (sMoonConfig == "paid")
            {
                extendedDungeon.dynamicRoutePricesList.Add(new Vector2WithRarity(new Vector2(1, 9999), iDefaultRarity));
                mls.LogInfo("Registered DantorsMentalHospital dungeon for all paid moons.");
            }
            else if (sMoonConfig == "free")
            {
                extendedDungeon.dynamicRoutePricesList.Add(new Vector2WithRarity(new Vector2(0, 0), iDefaultRarity));
                mls.LogInfo("Registered DantorsMentalHospital dungeon for all free moons.");
            }
            else
            {
                mls.LogInfo("Registering DantorsMentalHospital dungeon for predefined moon list.");
                // First split by comma, then split by @ symbol if present
                string[] arMoonNames = configMoons.Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                List<StringWithRarity> lMoonsWithRarity = new List<StringWithRarity>();
                for (int i = 0; i < arMoonNames.Length; i++)
                {
                    string[] arMoonNameRarity = arMoonNames[i].Split('@', StringSplitOptions.RemoveEmptyEntries);
                    int iLength = arMoonNameRarity.Length;
                    if (iLength > 2)
                    {
                        mls.LogError($"Invalid setup for moon rarity config: {arMoonNames[i]}. Skipping.");
                        continue;
                    }
                    if (iLength == 1)
                    {
                        mls.LogInfo($"Registering DantorsMentalHospital dungeon for moon {arMoonNames[i]} at default rarity {iDefaultRarity}");
                        lMoonsWithRarity.Add(new StringWithRarity(arMoonNames[i], iDefaultRarity));
                    }
                    else
                    { // Length == 2
                        int iRarity;
                        if (!int.TryParse(arMoonNameRarity[1], out iRarity))
                        {
                            mls.LogError($"Failed to parse rarity value for moon {arMoonNameRarity[0]}: {arMoonNameRarity[1]}. Skipping.");
                            continue;
                        }
                        mls.LogInfo($"Registering DantorsMentalHospital dungeon for moon {arMoonNames[i]} at rarity {iRarity}");
                        lMoonsWithRarity.Add(new StringWithRarity(arMoonNameRarity[0], iRarity));
                    }
                }
                extendedDungeon.manualPlanetNameReferenceList = lMoonsWithRarity;
            }
            extendedDungeon.dungeonDisplayName = "Mental Hospital";
            extendedDungeon.dungeonSizeMin = configMinSize.Value;
            extendedDungeon.dungeonSizeMax = configMaxSize.Value;
            extendedDungeon.dungeonSizeLerpPercentage = configDynamicValue.Value;
            extendedDungeon.enableDynamicDungeonSizeRestriction = configDynamicToggle.Value;
            PatchedContent.RegisterExtendedDungeonFlow(extendedDungeon);


            harmony.PatchAll(typeof(MentalHospitalDunGen));
            harmony.PatchAll(typeof(RoundManagerPatch));
            harmony.PatchAll(typeof(BurstValveColorPatch));
            harmony.PatchAll(typeof(SteamValveHazard_Update_Patch));
            mls.LogInfo($"DantorsMentalHospital DunGen for Lethal Company [Version {PluginInfo.PLUGIN_VERSION}] successfully loaded.");

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
        
        [HarmonyPatch(typeof(RoundManager))]
        internal class RoundManagerPatch
        {
            // Find and replace the dummy scrap items with the ones already in LE (for some reason it compares the hash of the item directly, not any actual data in it)
            [HarmonyPatch("SpawnScrapInLevel")]
            [HarmonyPrefix]
            private static bool SetItemSpawnPoints(ref RuntimeDungeon ___dungeonGenerator)
            {
                String name = DungeonManager.CurrentExtendedDungeonFlow.name;
                if (name != "HospitalExtendedDungeonFlow")
                {
                    return true; // Do nothing to non-Hospital dungeons
                }
                // Grab the general and tabletop item groups from the bottle bin item
                StartOfRound startOfRound = StartOfRound.Instance;
                if (startOfRound == null)
                {
                    Instance.mls.LogError("Failed to get start of round instance. Scrap spawns may not work correctly.");
                    return true;
                }

                Item bottleItem = startOfRound.allItemsList.itemsList.Find(x => x.itemName == "Bottles");
                if (bottleItem == null)
                {
                    Instance.mls.LogError("Failed to find bottle bin item for reference snatching; scrap spawn may be significantly lower than expected.");
                    return true;
                }
                // Grab the small item group from the fancy glass (only appears on paid moons, so this one is optional and replaced with tabletop items if invalid)
                Item fancyGlassItem = startOfRound.allItemsList.itemsList.Find(x => x.itemName == "Golden cup");

                int iGeneralScrapCount = 0;
                int iTabletopScrapCount = 0;
                int iSmallScrapCount = 0;

                // Grab the item groups
                ItemGroup itemGroupGeneral = bottleItem.spawnPositionTypes.Find(x => x.name == "GeneralItemClass");
                ItemGroup itemGroupTabletop = bottleItem.spawnPositionTypes.Find(x => x.name == "TabletopItems");
                ItemGroup itemGroupSmall = (fancyGlassItem == null) ? itemGroupTabletop : fancyGlassItem.spawnPositionTypes.Find(x => x.name == "SmallItems"); // Use tabletop items in place of small items if not on a paid moon
                RandomScrapSpawn[] scrapSpawns = FindObjectsOfType<RandomScrapSpawn>();
                foreach (RandomScrapSpawn scrapSpawn in scrapSpawns)
                {
                    switch (scrapSpawn.spawnableItems.name)
                    {
                        case "GeneralItemClassDUMMY":
                            scrapSpawn.spawnableItems = itemGroupGeneral;
                            iGeneralScrapCount++;
                            break;
                        case "TabletopItemsDUMMY":
                            scrapSpawn.spawnableItems = itemGroupTabletop;
                            iTabletopScrapCount++;
                            break;
                        case "SmallItemsDUMMY":
                            scrapSpawn.spawnableItems = itemGroupSmall;
                            iSmallScrapCount++;
                            break;
                    }
                }

                Instance.mls.LogInfo($"Totals for scrap replacement: General: {iGeneralScrapCount}, Tabletop: {iTabletopScrapCount}, Small: {iSmallScrapCount}");
                if ((iGeneralScrapCount + iTabletopScrapCount + iSmallScrapCount) < 10) Instance.mls.LogWarning("Unusually low scrap spawn count; scrap may be sparse.");

                return true;
            }
        }
        
        [HarmonyPatch(typeof(SteamValveHazard))]
        [HarmonyPatch("BurstValve")]
        public class BurstValveColorPatch
        {
            static void Postfix(SteamValveHazard __instance)
            {

                String name = DungeonManager.CurrentExtendedDungeonFlow.name;
                if (name == "HospitalExtendedDungeonFlow" && configGreenFog.Value)
                {
                    // Replace "FF0000" with your desired hexadecimal color value
                    string hexColor = "749B3B";

                    Color newColor;
                    if (ColorUtility.TryParseHtmlString("#" + hexColor, out newColor))
                    {

                        // Replace "fogAnimator" with the actual field name in your SteamValveHazard script
                        var fogVolume = __instance.fogAnimator.GetComponent<LocalVolumetricFog>();
                        if (fogVolume != null)
                        {
                            __instance.fogAnimator.GetComponent<LocalVolumetricFog>().parameters.albedo = newColor;
                        }
                        else
                        {
                            Debug.LogError("LocalVolumetricFog component not found on fogAnimator.");
                        }
                    }
                    else
                    {
                        Debug.LogError("Invalid hexadecimal color value: " + hexColor);
                    }
                }
                else
                {


                }

            }
        }

        [HarmonyPatch(typeof(SteamValveHazard))]
        [HarmonyPatch("Update")]
        public class SteamValveHazard_Update_Patch
        {
            private static readonly FieldInfo valveHasBurstField = AccessTools.Field(typeof(SteamValveHazard), "valveHasBurst");
            private static readonly FieldInfo valveHasBeenRepairedField = AccessTools.Field(typeof(SteamValveHazard), "valveHasBeenRepaired");
            private static Dictionary<int, SteamValveState> valveStates = new Dictionary<int, SteamValveState>();

            // Prefix method is executed before the original Update method
            [HarmonyPrefix]
            public static bool Prefix(SteamValveHazard __instance)
            {
                // Access the private valveHasBurst field using reflection
                bool valveHasBurst = (bool)valveHasBurstField.GetValue(__instance);
                int valveId = __instance.GetInstanceID(); // Assuming there is a method to get a unique identifier for each valve
                bool valveHasBeenRepaired = (bool)valveHasBeenRepairedField.GetValue(__instance);



                String name = DungeonManager.CurrentExtendedDungeonFlow.name;
                if (valveHasBurst && name == "HospitalExtendedDungeonFlow" && !valveHasBeenRepaired && configFogDamage.Value)
                {
                    if (!valveStates.ContainsKey(valveId))
                    {
                        valveStates[valveId] = new SteamValveState();
                    }

                    SteamValveState state = valveStates[valveId];
                    // Check if the local player is within the fog range
                    if (Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, __instance.valveAudio.transform.position) < 10f)
                    {
                        // Increase the time spent inside the fog
                        state.timeInsideFog += Time.deltaTime;
                        
                        // Check if the player has been inside the fog for 5 seconds
                        if (state.timeInsideFog >= 5f && !state.isInsideFog)
                        {
                            state.isInsideFog = true;
                            state.damageTimer = 0f; // Reset the damage timer
                        }

                        // Check if the player is inside the fog and it's time to apply damage
                        if (state.isInsideFog)
                        {
                            state.damageTimer += Time.deltaTime;

                            // Check if the damage interval (1 second) has passed
                            if (state.damageTimer >= 1f)
                            {
                                state.damageTimer = 0f; // Reset the damage timer
                                                        // Apply damage to the player (You can replace this with your actual damage logic)
                                ApplyDamageToPlayer();
                            }
                        }
                    }
                    else
                    {
                        // Reset the timers when the player is outside the fog
                        state.timeInsideFog = 0f;
                        state.damageTimer = 0f;
                        state.isInsideFog = false;
                    }
                    return true;
                }
                else
                {
                    return true;
                }
            }
            private static void ApplyDamageToPlayer()
            {
                
                GameNetworkManager.Instance.localPlayerController.DamagePlayer(5, true, true, CauseOfDeath.Unknown, 0, false, default);
                
            }
            private class SteamValveState
            {
                public float timeInsideFog = 0f;
                public float damageTimer = 0f;
                public bool isInsideFog = false;
            }
        }  
    }
}
