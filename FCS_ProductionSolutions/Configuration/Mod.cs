﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FCS_AlterraHub.Mono;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCS_ProductionSolutions.Configuration
{
    internal static class Mod
    {
        #region Private Members

        private static ModSaver _saveObject;
        private static SaveData _saveData;
        private static List<TechType> _hydroponicKnownTech;

        #endregion

        internal static string ModName => "FCSProductionSolutions";
        internal static string SaveDataFilename => $"FCSProductionSolutionsSaveData.json";
        internal const string ModBundleName = "fcsproductionsolutionsbundle";

        internal const string HydroponicHarvesterModTabID = "HH";
        internal const string HydroponicHarvesterModFriendlyName = "Hydroponic Harvester";
        internal const string HydroponicHarvesterModName = "HydroponicHarvester";
        public const string HydroponicHarvesterModDescription = "A hydroponic harvester that allows you to store 3 DNA samples to clone.";
        internal static string HydroponicHarvesterKitClassID => $"{HydroponicHarvesterModName}_Kit";
        internal static string HydroponicHarvesterModClassName => HydroponicHarvesterModName;
        internal static string HydroponicHarvesterModPrefabName => HydroponicHarvesterModName;

        internal const string MatterAnalyzerTabID = "MA";
        internal const string MatterAnalyzerFriendlyName = "Matter Analyzer";
        internal const string MatterAnalyzerModName = "MatterAnalyzer";
        public const string MatterAnalyzerDescription = "A device that scans items and learns its matter makeup.";
        internal static string MatterAnalyzerKitClassID => $"{MatterAnalyzerModName}_Kit";
        internal static string MatterAnalyzerClassName => MatterAnalyzerModName;
        internal static string MatterAnalyzerPrefabName => MatterAnalyzerModName;

#if SUBNAUTICA
        internal static TechData HydroponicHarvesterIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData HydroponicHarvesterIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(HydroponicHarvesterKitClassID.ToTechType(), 1),
            }
        };

        internal const string ModDescription = "";

        internal static event Action<SaveData> OnDataLoaded;

        #region Internal Methods

        internal static void Save()
        {
            if (!IsSaving())
            {
                _saveObject = new GameObject().AddComponent<ModSaver>();

                SaveData newSaveData = new SaveData();

                var controllers = GameObject.FindObjectsOfType<MonoBehaviour>().OfType<IFCSSave<SaveData>>();

                foreach (var controller in controllers)
                {
                    controller.Save(newSaveData);
                }

                newSaveData.HydroponicHarvesterKnownTech = _hydroponicKnownTech;

                _saveData = newSaveData;

                ModUtils.Save<SaveData>(_saveData, SaveDataFilename, GetSaveFileDirectory(), OnSaveComplete);
            }
        }

        internal static string GetAssetPath()
        {
            return Path.Combine(GetModDirectory(), "Assets");
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

        internal static HydroponicHarvesterDataEntry GetHydroponicHarvesterSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.HydroponicHarvesterEntries)
            {
                if (string.IsNullOrEmpty(entry.ID)) continue;

                if (entry.ID == id)
                {
                    return entry;
                }
            }

            return new HydroponicHarvesterDataEntry() { ID = id };
        }

        internal static MatterAnalyzerDataEntry GetMatterAnalyzerSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.MatterAnalyzerEntries)
            {
                if (string.IsNullOrEmpty(entry.ID)) continue;

                if (entry.ID == id)
                {
                    return entry;
                }
            }

            return new MatterAnalyzerDataEntry() { ID = id };
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

        public static void AddHydroponicKnownTech(TechType techType)
        {
            if (_hydroponicKnownTech == null)
            {
                _hydroponicKnownTech = new List<TechType>();
            }
            _hydroponicKnownTech.Add(techType);
        }
        public static bool IsHydroponicKnownTech(TechType techType)
        {
            if (_hydroponicKnownTech == null)
            {
                _hydroponicKnownTech = new List<TechType>();
            }
            return _hydroponicKnownTech.Contains(techType);
        }
    }
}