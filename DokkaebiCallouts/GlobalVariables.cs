using System.Collections.Generic;
using CitizenFX.Core;

namespace DokkaebiCallouts
{
    public class GlobalVariables
    {
        public static List<VehicleHash> randomVehicleList = new List<VehicleHash>
        {
            VehicleHash.Futo,
            VehicleHash.Gauntlet,
            VehicleHash.Gauntlet2,
            VehicleHash.Intruder,
            VehicleHash.Khamelion,
            VehicleHash.Kuruma,
            VehicleHash.Kuruma2,
            VehicleHash.Sentinel,
            VehicleHash.Sentinel2,
            VehicleHash.Schafter2,
            VehicleHash.Schafter3,
            VehicleHash.Schafter4,
            VehicleHash.Schafter5,
            VehicleHash.Schafter6,
            VehicleHash.ZType
        };

        public static List<WeaponHash> randomMeleeWeaponList = new List<WeaponHash>
            {
                WeaponHash.Bottle,
                WeaponHash.Crowbar,
                WeaponHash.Dagger,
                WeaponHash.Hammer,
                WeaponHash.Wrench
            };
    }
}
