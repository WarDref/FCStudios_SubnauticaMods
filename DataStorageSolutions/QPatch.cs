﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Buildables.Antenna;
using DataStorageSolutions.Buildables.FilterMachine;
using DataStorageSolutions.Buildables.Racks;
using DataStorageSolutions.Buildables.Terminal;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Craftables;
using FCSCommon.Utilities;
using FCSTechFabricator;
using FCSTechFabricator.Components;
using FCSTechFabricator.Craftables;
using QModManager.API.ModLoading;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace DataStorageSolutions
{
    [QModCore]
    public class QPatch
    {
        internal static ConfigFile Configuration { get; private set; }
        internal static AssetBundle GlobalBundle { get; set; }

        [QModPatch]
        public static void Patch()
        {
            try
            {
                QuickLogger.Info($"Started patching. Version: {QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly())}");

#if DEBUG
                QuickLogger.DebugLogsEnabled = true;
                QuickLogger.Debug("Debug logs enabled");
#endif

                GlobalBundle = FcAssetBundlesService.PublicAPI.GetAssetBundleByName(FcAssetBundlesService.PublicAPI.GlobalBundleName);

                Configuration = Mod.LoadConfiguration();

                AuxPatchers.AdditionalPatching();

                DSSModelPrefab.GetPrefabs();

                AddTechFabricatorItems();

                var antenna = new AntennaBuildable();
                antenna.Patch();

                var floorMountedRack = new FloorMountedRackBuildable();
                floorMountedRack.Patch();

                var wallMountedRack = new WallMountedRackBuildable();
                wallMountedRack.Patch();

                var terminal = new DSSTerminalC48Buildable();
                terminal.Patch();

                var serverFormattingStation = new ServerFormattingStationBuildable();
                serverFormattingStation.Patch();

                QuickLogger.Info("Finished patching");
            }
            catch (Exception ex)
            {
                QuickLogger.Error(ex);
            }
        }
        
        private static void AddTechFabricatorItems()
        {
            var icon = ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetFolder(), $"{Mod.ModName}.png"));
            var craftingTab = new CraftingTab(Mod.DSSTabID, Mod.ModFriendlyName, icon);

            var floorMountedRack = new FCSKit(Mod.FloorMountedRackKitClassID, Mod.FloorMountedRackFriendlyName, craftingTab , Mod.FloorMountedRackIngredients);
            floorMountedRack.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);

            var wallMountedRack = new FCSKit(Mod.WallMountedRackKitClassID, Mod.WallMountedRackFriendlyName, craftingTab, Mod.WallMountedRackIngredients);
            wallMountedRack.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);

            var terminal = new FCSKit(Mod.TerminalKitClassID, Mod.TerminalFriendlyName, craftingTab, Mod.TerminalIngredients);
            terminal.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);

            var antenna = new FCSKit(Mod.AntennaKitClassID, Mod.AntennaFriendlyName, craftingTab, Mod.AntennaIngredients);
            antenna.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);

            var serverFormattingStation = new FCSKit(Mod.ServerFormattingStationKitClassID, Mod.ServerFormattingStationFriendlyName, craftingTab, Mod.ServerFormattingStationIngredients);
            serverFormattingStation.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);

            Server = new ServerCraftable(Mod.ServerClassID, Mod.ServerFriendlyName, Mod.ServerDescription, craftingTab);
            Server.Patch(FcTechFabricatorService.PublicAPI, FcAssetBundlesService.PublicAPI);
        }

        internal static ServerCraftable Server { get; set; }
    }
}
