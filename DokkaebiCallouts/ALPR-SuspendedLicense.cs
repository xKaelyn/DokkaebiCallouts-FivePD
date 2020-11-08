using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FivePD.API;
using FivePD.API.Utils;
using CitizenFX.Core;
using CitizenFX.Core.UI;
using static DokkaebiCallouts.GlobalVariables;

/* DokkaebiCallouts for FivePD | Developed by xSklzx Dokkaebi */
/* Available via the MIT license */

namespace DokkaebiCallouts
{
    [CalloutProperties("ALPR: Driving License Suspended", "xSklzx Dokkaebi", "1.0.0.0")]
    public class ALPR_SuspendedLicense : Callout
    {
        //  Variables required.
        public readonly Random rnd = new Random();
        private Ped suspect;
        private Vehicle vehicle;
        public ALPR_SuspendedLicense()
        {
            //  Picks a random location on the map within a radius given below.
            float offsetX = rnd.Next(100, 450);
            float offsetY = rnd.Next(100, 450);

            InitInfo(World.GetNextPositionOnStreet(new Vector3(offsetX, offsetY, 0)));

            //  Information that FivePD uses for both the in-game notification and police computer.
            ShortName = "ALPR: Driving License Suspended";
            CalloutDescription = "An ALPR camera has picked up a flag on a vehicle, the registered owner is driving with a suspended driving license.";
            ResponseCode = 2;
            StartDistance = 100f;

            //  FivePDAudio compatibility - https://gtapolicemods.com/index.php?/files/file/895-fivepd-audio-dispatch-audio/
            BaseScript.TriggerEvent("FivePDAudio::RegisterCallout", new object[]
            {
                this.ShortName,
                @"CRIMES/CRIME_TRAFFIC_ALERT_01.ogg"
            });
        }

        public override async Task OnAccept()
        {
            InitBlip();

            //  Spawns both the suspect and the vehicle.
            suspect = await SpawnPed(RandomUtils.GetRandomPed(), Location);
            vehicle = await SpawnVehicle(spawnRandomCar(), Location);

            keepTask(suspect);

            //  Shows notification once the player accepts the call.
            Screen.ShowNotification("Respond to the reported location as quickly as possible - the driver is on the move.");

            //  Puts the suspect into the driver's seat and tells him to cruise with normal flags.
            suspect.SetIntoVehicle(vehicle, VehicleSeat.Driver);
            suspect.Task.CruiseWithVehicle(vehicle, 20f, 786603);
        }

        //  Runs when the player enters the blipped area.
        public override void OnStart(Ped player)
        {
            base.OnStart(player);

            //  Attaches a blip to the vehicle.
            vehicle.AttachBlip();

            //  Variables for the driver data and the vehicle data.
            PedData pedData = new PedData();
            VehicleData vehicleData = new VehicleData();

            //  Shows a simple subtitle at the bottom of the screen.
            Screen.ShowSubtitle("It looks like the driver is not here, they shouldn't be too far.");

            //  String variables.
            string firstname = pedData.FirstName;
            string lastname = pedData.LastName;

            //  Sets the driver's driving license to revoked.
            pedData.DriverLicense.LicenseStatus = PedData.License.Status.Expired;

            //  Essentially sets the vehicle to be owned by the driver, as FivePD doesn't do that automatically.
            vehicleData.OwnerFirstName = firstname;
            vehicleData.OwnerLastName = lastname;

            //  Sets the vehicle to be flagged up on the system - doesn't seem to actually do it though.
            vehicleData.Flag = "Owner has suspended driving license";

            //  Push the data we just set to both the driver and the vehicle.
            Utilities.SetPedData(suspect.NetworkId, pedData);
            Utilities.SetVehicleData(vehicle.NetworkId, vehicleData);

            //  40% chance for the suspect to flee using the IPursuit interface.
            int chance = rnd.Next(0, 10);

            //  If the number is between, or equal to, 0 through 3, initiate a pursuit.
            if (chance >= 0 && chance <= 3)
            {
                var pursuit = Pursuit.RegisterPursuit(suspect);
                pursuit.Init(true, 30f, 125f, true);
                pursuit.ActivatePursuit();
                Utilities.ExcludeVehicleFromTrafficStop(vehicle.NetworkId, true);
            }
        }

        public override void OnCancelBefore()
        {
            base.OnCancelBefore();

            //  If the vehicle exists, call the CleanUp function.
            if (vehicle.Exists())
                CleanUp();
        }

        //  Stops the ped from listening to native GTA algorithms, i.e fleeing when shot at.
        public static void keepTask(Ped p)
        {
            p.BlockPermanentEvents = true;
            p.AlwaysKeepTask = true;
        }

        // A list to spawn a random car.
        private VehicleHash spawnRandomCar()
        {
            return randomVehicleList[rnd.Next(randomVehicleList.Count)];
        }

        //  Cleanup function as a failsafe if FivePD fails to destroy spawned in assets, which it does quite often.
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
