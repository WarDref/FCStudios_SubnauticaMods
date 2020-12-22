﻿using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_StorageSolutions.Configuration;
using FCS_StorageSolutions.Mods.AlterraStorage.Buildable;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Rack
{
    internal class DSSFloorServerRackController : FcsDevice,IFCSSave<SaveData>,IHandTarget
    {
        private bool _runStartUpOnEnable;
        private bool _isFromSave;
        private bool _isBeingDestroyed;
        private DSSFloorServerRackDataEntry _savedData;
        private float _targetPos;
        private float _speed = 3f;
        private GameObject _tray;
        private Dictionary<string,DSSSlotController> _slots;
        private FCSStorage _storage;

        internal bool IsOpen => _targetPos > 0;
        
        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.DSSTabID, Mod.ModName);
        }

        private void Update()
        {
            MoveTray();
        }

        private void OnEnable()
        {
            if (_runStartUpOnEnable)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }

                if (_isFromSave)
                {
                    if (_savedData == null)
                    {
                        ReadySaveData();
                    }

                    if (_savedData != null)
                    {
                        _colorManager.ChangeColor(_savedData.BodyColor.Vector4ToColor());
                        _colorManager.ChangeColor(_savedData.SecondaryColor.Vector4ToColor(),ColorTargetMode.Secondary);
                        if (_savedData.IsTrayOpen)
                        {
                            Open();
                        }
                    }
                }

                _runStartUpOnEnable = false;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _isBeingDestroyed = true;
        }

        public override Vector3 GetPosition()
        {
            return transform.position;
        }

        public override void Initialize()
        {
            //_storageAmount = GameObjectHelpers.FindGameObject(gameObject, "StorageAmount").GetComponent<Text>();
            
            _slots = new Dictionary<string,DSSSlotController>();

            //var slotsLocation = GameObjectHelpers.FindGameObject(gameObject, "rack_door_mesh").transform;
            //int i = 1;
            //foreach (Transform slot in slotsLocation)
            //{
            //    var slotName = $"Slot {i++}";
            //    var slotController = slot.gameObject.AddComponent<DSSSlotController>();
            //    //slotController.Initialize(slotName, this);
            //    _slots.Add(slotName,slotController);
            //}

            //_tray = GameObjectHelpers.FindGameObject(gameObject, "anim_rack_door");

            if (_colorManager == null)
            {
                _colorManager = gameObject.EnsureComponent<ColorManager>();
                _colorManager.Initialize(gameObject,ModelPrefab.BodyMaterial,ModelPrefab.SecondaryMaterial);
            }

            //var canvas = gameObject.GetComponentInChildren<Canvas>();

            IsInitialized = true;
        }

        public override FCSStorage GetStorage()
        {
            return _storage;
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoSerialize");

            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {GetPrefabID()}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {GetPrefabID()}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            
            if (_savedData == null)
            {
                ReadySaveData();
            }

            if (!IsInitialized)
            {
                Initialize();
            }

            //_slots.ElementAt(0).Value.RestoreItems(serializer,_savedData.Slot1);
            //_slots.ElementAt(1).Value.RestoreItems(serializer,_savedData.Slot2);
            //_slots.ElementAt(2).Value.RestoreItems(serializer,_savedData.Slot3);
            //_slots.ElementAt(3).Value.RestoreItems(serializer,_savedData.Slot4);
            //_slots.ElementAt(4).Value.RestoreItems(serializer,_savedData.Slot5);
            //_slots.ElementAt(5).Value.RestoreItems(serializer,_savedData.Slot6);
            //_slots.ElementAt(6).Value.RestoreItems(serializer,_savedData.Slot7);
            //_slots.ElementAt(7).Value.RestoreItems(serializer,_savedData.Slot8);
            //_slots.ElementAt(8).Value.RestoreItems(serializer,_savedData.Slot9);
            //_slots.ElementAt(9).Value.RestoreItems(serializer,_savedData.Slot10);
            //_slots.ElementAt(10).Value.RestoreItems(serializer,_savedData.Slot11);
            //_slots.ElementAt(11).Value.RestoreItems(serializer,_savedData.Slot12);
            //_slots.ElementAt(12).Value.RestoreItems(serializer,_savedData.Slot13);
            //_slots.ElementAt(13).Value.RestoreItems(serializer,_savedData.Slot14);
            //_slots.ElementAt(14).Value.RestoreItems(serializer,_savedData.Slot15);

            _isFromSave = true;
        }

        public override bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;
            return true;
        }

        public override void OnConstructedChanged(bool constructed)
        {
            IsConstructed = constructed;
            if (constructed)
            {
                if (isActiveAndEnabled)
                {
                    if (!IsInitialized)
                    {
                        Initialize();
                    }

                    IsInitialized = true;
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            try
            {
                if (!IsInitialized || !IsConstructed) return;

                if (_savedData == null)
                {
                    _savedData = new DSSFloorServerRackDataEntry();
                }

                _savedData.ID = GetPrefabID();
                _savedData.BodyColor = _colorManager.GetColor().ColorToVector4();
                _savedData.SecondaryColor = _colorManager.GetSecondaryColor().ColorToVector4();
                _savedData.IsTrayOpen = IsOpen;
                //_savedData.Slot1 = _slots.ElementAt(0).Value.Save(serializer);
                //_savedData.Slot2 = _slots.ElementAt(1).Value.Save(serializer);
                //_savedData.Slot3 = _slots.ElementAt(2).Value.Save(serializer);
                //_savedData.Slot4 = _slots.ElementAt(3).Value.Save(serializer);
                //_savedData.Slot5 = _slots.ElementAt(4).Value.Save(serializer);
                //_savedData.Slot6 = _slots.ElementAt(5).Value.Save(serializer);
                //_savedData.Slot7 = _slots.ElementAt(5).Value.Save(serializer);
                //_savedData.Slot8 = _slots.ElementAt(5).Value.Save(serializer);
                //_savedData.Slot9 = _slots.ElementAt(5).Value.Save(serializer);
                //_savedData.Slot10 = _slots.ElementAt(5).Value.Save(serializer);
                //_savedData.Slot11 = _slots.ElementAt(5).Value.Save(serializer);
                //_savedData.Slot12 = _slots.ElementAt(5).Value.Save(serializer);
                //_savedData.Slot13 = _slots.ElementAt(5).Value.Save(serializer);
                //_savedData.Slot14 = _slots.ElementAt(5).Value.Save(serializer);
                //_savedData.Slot15 = _slots.ElementAt(5).Value.Save(serializer);
                newSaveData.DSSFloorServerRackDataEntries.Add(_savedData);
                QuickLogger.Debug($"Saving ID {_savedData.ID}", true);
            }
            catch (Exception e)
            {
                QuickLogger.Error($"Failed to save {UnitID}:");
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
                QuickLogger.Error(e.InnerException);
            }
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetDSSFloorServerRackSaveData(GetPrefabID());
        }
        
        private void MoveTray()
        {
            if (_tray == null) return;

            // remember, 10 - 5 is 5, so target - position is always your direction.
            Vector3 dir = new Vector3(_tray.transform.localPosition.x, _tray.transform.localPosition.y, _targetPos) - _tray.transform.localPosition;

            // magnitude is the total length of a vector.
            // getting the magnitude of the direction gives us the amount left to move
            float dist = dir.magnitude;

            // this makes the length of dir 1 so that you can multiply by it.
            dir = dir.normalized;

            // the amount we can move this frame
            float move = _speed * DayNightCycle.main.deltaTime;

            // limit our move to what we can travel.
            if (move > dist) move = dist;

            // apply the movement to the object.
            _tray.transform.Translate(dir * move);
        }

        private void Open()
        {
            _targetPos = 0.382f;
        }

        private void Close()
        {
            _targetPos = 0f;
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }

        public void OnHandHover(GUIHand hand)
        {
            HandReticle main = HandReticle.main;
            main.SetIcon(HandReticle.IconType.Hand);
            main.SetInteractText(IsOpen ? AuxPatchers.CloseWallServerRack() : AuxPatchers.OpenWallServerRack());
        }

        public void OnHandClick(GUIHand hand)
        {
            if (IsOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        public override void TurnOnDevice()
        {
            IsVisible = true;
            foreach (KeyValuePair<string, DSSSlotController> slot in _slots)
            {
                slot.Value.SetIsVisible(IsVisible);
            }
            //TODO Turn OnScreen
        }

        public override void TurnOffDevice()
        {
            IsVisible = false;
            foreach (KeyValuePair<string, DSSSlotController> slot in _slots)
            {
                slot.Value.SetIsVisible(IsVisible);
            }
            //TODO Turn OffScreen
        }
    }
}
