using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FivePD.API;
using FivePD.API.Utils;
using CitizenFX.Core;
using CitizenFX.Core.UI;

namespace DokkaebiCallouts
{
    [CalloutProperties("ALPR: Driving License Revoked", "xSklzx Dokkaebi", "1.0.0.0")]
    public class ALPR_RevokedLicense : Callout
    {
        public readonly Random rnd = new Random();
        private Ped suspect;
        private Vehicle vehicle;
        public ALPR_RevokedLicense()
        {
            float offsetX = rnd.Next(100, 450);
            float offsetY = rnd.Next(100, 450);

            InitInfo(World.GetNextPositionOnStreet(new Vector3(offsetX, offsetY, 0)));

            ShortName = "ALPR: Driving License Revoked";
            CalloutDescription = "";
            ResponseCode = 2;
            StartDistance = 100f;

            // FivePDAudio compatibility - https://gtapolicemods.com/index.php?/files/file/895-fivepd-audio-dispatch-audio/
            BaseScript.TriggerEvent("FivePDAudio::RegisterCallout", new object[]
            {
                this.ShortName,
                @"CRIMES/CRIME_TRAFFIC_ALERT_01.ogg"
            });
        }

        public override async Task OnAccept()
        {
            InitBlip();

            suspect = await SpawnPed(RandomUtils.GetRandomPed(), Location);
            vehicle = await SpawnVehicle(spawnRandomCar(), Location);

            keepTask(suspect);

            Screen.ShowNotification("Respond to the reported location as quickly as possible - the driver is most likely on the move at quite some speed.");

            suspect.SetIntoVehicle(vehicle, VehicleSeat.Driver);
            suspect.Task.CruiseWithVehicle(vehicle, 20f, 786603);
        }

        public override void OnStart(Ped player)
        {
            base.OnStart(player);

            vehicle.AttachBlip();

            PedData pedData = new PedData();
            VehicleData vehicleData = new VehicleData();

            Screen.ShowSubtitle("");

            string firstname = pedData.FirstName;
            string lastname = pedData.LastName;
            pedData.DriverLicense.LicenseStatus = PedData.License.Status.Revoked;

            vehicleData.OwnerFirstName = firstname;
            vehicleData.OwnerLastName = lastname;

            Utilities.SetPedData(suspect.NetworkId, pedData);
            Utilities.SetVehicleData(vehicle.NetworkId, vehicleData);
        }

        public override void OnCancelBefore()
        {
            base.OnCancelBefore();

            if (vehicle.Exists())
                CleanUp();
        }

        public static void keepTask(Ped p)
        {
            p.BlockPermanentEvents = true;
            p.AlwaysKeepTask = true;
        }

        private VehicleHash spawnRandomCar()
        {
            List<VehicleHash> vehicles = new List<VehicleHash>
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
            return vehicles[rnd.Next(vehicles.Count)];
        }
        public void CleanUp()
        {
            if (vehicle.AttachedBlip.Exists())
                vehicle.AttachedBlip.Delete();
            if (suspect.Exists())
                suspect.Delete();
            if (vehicle.Exists())
                vehicle.Delete();
        }
    }
}
