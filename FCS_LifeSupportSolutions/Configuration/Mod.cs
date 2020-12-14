﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using HarmonyLib;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCS_LifeSupportSolutions.Configuration
{
    internal static class Mod
    {
        #region Private Members

        private static ModSaver _saveObject;
        private static SaveData _saveData;
        #endregion

        internal static string ModName => "FCSLifeSupportSolutions";
        internal static string SaveDataFilename => $"FCSLifeSupportSolutionsSaveData.json";
        internal const string ModBundleName = "fcslifesupportsolutionsbundle";

        internal const string EnergyPillVendingMachineTabID = "EPV";
        internal const string EnergyPillVendingMachineFriendlyName = "Energy Pill Vending Machine";
        internal const string EnergyPillVendingMachineName = "EnergyPillVendingMachine";
        internal const string EnergyPillVendingMachineDescription = "The Energy Pill Vending Machine allows you to get energy pills that gives you a boost of adrenaline in those times hunger and water lack.";
        internal static string EnergyPillVendingMachineKitClassID => $"{EnergyPillVendingMachineName}_Kit";
        internal static string EnergyPillVendingMachineClassName => EnergyPillVendingMachineName;
        internal static string EnergyPillVendingMachinePrefabName => "EnergyPillVendingMachine";

        internal const string MiniMedBayTabID = "MMB";
        internal const string MiniMedBayFriendlyName = "Mini Med Bay";
        internal const string MiniMedBayName = "MiniMedBay";
        internal const string MiniMedBayDescription = "The MiniMedBay is a medical bay that heals you with the little cost of power. Get your life recharged.";
        internal static string MiniMedBayKitClassID => $"{MiniMedBayClassName}_Kit";
        internal static string MiniMedBayClassName => MiniMedBayName;
        internal static string MiniMedBayPrefabName => MiniMedBayName;

        internal const string BaseUtilityUnitTabID = "BUU";
        internal const string BaseUtilityUnitFriendlyName = "Base Utility Unit";
        internal const string BaseUtilityUnitName = "BaseUtilityUnit";
        internal const string BaseUtilityUnitDescription = "The Base Utility Unity provides oxygen and water to your base. Mini fountain filter benefits from this by using the water for use above the water line";
        internal static string BaseUtilityUnityKitClassID => $"{BaseUtilityUnitName}_Kit";
        internal static string BaseUtilityUnityClassName => BaseUtilityUnitName;
        internal static string BaseUtilityUnityPrefabName => BaseUtilityUnitName;


#if SUBNAUTICA
        internal static TechData EnergyPillVendingMachineIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData EnergyPillVendingMachineIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(EnergyPillVendingMachineKitClassID.ToTechType(), 1),
            }
        };

#if SUBNAUTICA
        internal static TechData MiniMedBayIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData MiniMedBayIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(MiniMedBayKitClassID.ToTechType(), 1),
            }
        };

#if SUBNAUTICA
        internal static TechData BaseUtilityUnitIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData BaseUtilityUnitIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(BaseUtilityUnityKitClassID.ToTechType(), 1),
            }
        };

        public static TechType RedEnergyPillTechType { get; set; }
        public static TechType GreenEnergyPillTechType { get; set; }
        public static TechType BlueEnergyPillTechType { get; set; }

        internal const string ModDescription ="";

        internal static event Action<SaveData> OnDataLoaded;

        #region Internal Methods

        internal static void Save(ProtobufSerializer serializer)
        {
            if (!IsSaving())
            {
                _saveObject = new GameObject().AddComponent<ModSaver>();

                SaveData newSaveData = new SaveData();

                foreach (var controller in FCSAlterraHubService.PublicAPI.GetRegisteredDevices())
                {
                    if (controller.Value.PackageId == ModName)
                    {
                        QuickLogger.Debug($"Saving device: {controller.Value.UnitID}");
                        ((IFCSSave<SaveData>)controller.Value).Save(newSaveData,serializer);
                    }
                }

                _saveData = newSaveData;

                ModUtils.Save<SaveData>(_saveData, SaveDataFilename, GetSaveFileDirectory(), OnSaveComplete);
            }
        }

        internal static void LoadData()
        {
            QuickLogger.Info("Loading Save Data...");
            ModUtils.LoadSaveData<SaveData>(SaveDataFilename, GetSaveFileDirectory(), (data) =>
            {
                _saveData = data;
                QuickLogger.Info("Save Data Loaded");
                OnDataLoaded?.Invoke(_saveData);
            });
        }

        internal static bool IsSaving()
        {
            return _saveObject != null;
        }

        internal static void OnSaveComplete()
        {
            _saveObject.StartCoroutine(SaveCoroutine());
        }

        internal static string GetSaveFileDirectory()
        {
            return Path.Combine(SaveUtils.GetCurrentSaveDataDir(), ModName);
        }

        internal static MiniMedBayEntry GetMiniMedBaySaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.MiniMedBayEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new MiniMedBayEntry() { Id = id };
        }

        internal static EnergyPillVendingMachineEntry GetEnergyPillVendingMachineSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.EnergyPillVendingMachineEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new EnergyPillVendingMachineEntry() { Id = id };
        }

        public static BaseUtilityEntry GetBaseUtilityUnitSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.BaseUtilityUnitEntries)
            {
                if (string.IsNullOrEmpty(entry.Id)) continue;

                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return new BaseUtilityEntry() { Id = id };
        }

        internal static SaveData GetSaveData()
        {
            return _saveData ?? new SaveData();
        }

        internal static string GetAssetFolder()
        {
            return Path.Combine(GetModDirectory(), "Assets");
        }

        internal static string GetModDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
        #endregion

        #region Private Methods
        private static IEnumerator SaveCoroutine()
        {
            while (SaveLoadManager.main != null && SaveLoadManager.main.isSaving)
            {
                yield return null;
            }
            GameObject.DestroyImmediate(_saveObject.gameObject);
            _saveObject = null;
        }

        #endregion
    }
}