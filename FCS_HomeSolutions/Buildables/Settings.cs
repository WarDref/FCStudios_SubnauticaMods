﻿using UnityEngine;

namespace FCS_HomeSolutions.Buildables
{
    internal struct Settings
    {
        internal bool AllowedOutside;
        internal bool AllowedInBase;
        internal bool AllowedOnGround;
        internal bool AllowedOnWall;
        internal bool RotationEnabled;
        internal bool AllowedOnCeiling;
        internal bool AllowedInSub;
        internal bool AllowedOnConstructables;
        internal string KitClassID;
        internal Vector3 Size;
        internal Vector3 Center;
        internal TechGroup GroupForPDA;
        internal TechCategory CategoryForPDA;
    }
}