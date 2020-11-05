using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FivePD.API;
using FivePD.API.Utils;
using CitizenFX.Core;
using CitizenFX.Core.UI;

namespace DokkaebiCallouts
{
    [CalloutProperties("ALPR: Owner Wanted", "xSklzx Dokkaebi", "1.0.0.0")]
    public class ALPR_OwnerWanted : Callout
    {
        // Variables required.
        private readonly Random rnd = new Random();
        private Ped suspect;
        private Vehicle vehicle;
        public ALPR_OwnerWanted()
        {
            // Picks a random location on the map within a radius given below.
            float offsetX = rnd.Next(100, 450);
            float offsetY = rnd.Next(100, 450);

            InitInfo(World.GetNextPositionOnStreet(new Vector3(offsetX, offsetY, 0)));

            // Information that FivePD uses for both the in-game notification and police computer.
            ShortName = "ALPR: Registered Owner Wanted";
            CalloutDescription = "An ALPR camera has picked up a flag on a vehicle, the registered owner is wanted by law enforcement.";
            ResponseCode = 3;
            StartDistance = 100f;

            // FivePDAudio compatibility - https://gtapolicemods.com/index.php?/files/file/895-fivepd-audio-dispatch-audio/
            BaseScript.TriggerEvent("FivePDAudio::RegisterCallout", new object[]
            {
                this.ShortName,
                @"CRIMES/CRIME_TRAFFIC_ALERT_01.ogg"
            });
        }

        // Runs when the player presses Y to accept the callout.
        public override async Task OnAccept()
        {
            InitBlip();
            // Spawns both the suspect and vehicle.
            suspect = await SpawnPed(RandomUtils.GetRandomPed(), Location);
            vehicle = await SpawnVehicle(spawnRandomCar(), Location);

            keepTask(suspect);

            // Puts the suspect into the driver's seat and tells him to cruise, with normal flags.
            suspect.SetIntoVehicle(vehicle, VehicleSeat.Driver);
            suspect.Task.CruiseWithVehicle(vehicle, 20f, 786603);
        }

        // Runs when the player enters the blipped area.
        public override void OnStart(Ped player)
        {
            base.OnStart(player);
            // Attaches a blip to the vehicle.
            vehicle.AttachBlip();

            // Variables for the driver data and the vehicle data.
            PedData pedData = new PedData();
            VehicleData vehicleData = new VehicleData();
            // Shows a simple subtitle at the bottom of the screen.
            Screen.ShowSubtitle("It looks like the suspect has fled the area.");

            // String variables.
            string firstname = pedData.FirstName;
            string lastname = pedData.LastName;
            // Gives the driver a random warrant from the list.
            pedData.Warrant = randomWarrant();
            // Essentially sets the vehicle to be owned by the driver, as FivePD doesn't do that automatically.
            vehicleData.OwnerFirstName = firstname;
            vehicleData.OwnerLastName = lastname;
            // Sets the vehicle to be flagged up on the system - doesn't seem to actually do it though.
            vehicleData.Flag = "Registered Owner Wanted";

            // Push the data we just set to both the driver and the vehicle.
            Utilities.SetPedData(suspect.NetworkId, pedData);
            Utilities.SetVehicleData(vehicle.NetworkId, vehicleData);

            // 40% chance for the suspect to flee using the IPursuit interface.
            int chance = rnd.Next(0, 10);
            // If the number is between, or equal to, 0 through 3, initiate a pursuit.
            if (chance >= 0 && chance <= 3)
            {
                var pursuit = Pursuit.RegisterPursuit(suspect);
                pursuit.Init(true, 30f, 125f, true);
                pursuit.ActivatePursuit();
                Utilities.ExcludeVehicleFromTrafficStop(vehicle.NetworkId, true);
            }
        }

        // Runs when the user initiates a "code 4".
        public override void OnCancelBefore()
        {
            base.OnCancelBefore();

            // If the vehicle exists, call the CleanUp function.
            if (vehicle.Exists())
                CleanUp();
        }

        // Stops the ped from listening to native GTA algorithms; i.e fleeing when shot at.
        public static void keepTask(Ped p)
        {
            p.BlockPermanentEvents = true;
            p.AlwaysKeepTask = true;
        }

        // A list to pick a random warrant.
        public string randomWarrant()
        {
            List<string> warrantList = new List<string>
            {
                "Search Warrant",
                "Arrest Warrant",
                "Bench Warrant",
                "Failure to Appear",
                "Failure to Pay",
                "Child Support Arrest Warrant"
            };
            return warrantList[rnd.Next(warrantList.Count)];
        }

        // A list to pick a random vehicle.
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

        // Cleanup function as a failsafe if FivePD fails to destroy spawned in assets.
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