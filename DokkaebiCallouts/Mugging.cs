using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FivePD.API;
using FivePD.API.Utils;
using CitizenFX.Core;
using static DokkaebiCallouts.GlobalVariables;

/* DokkaebiCallouts for FivePD | Developed by xSklzx Dokkaebi */
/* Available via the MIT license */

namespace DokkaebiCallouts
{
    [CalloutProperties("Mugging", "xSklzx Dokkaebi", "1.0.0.0")]
    public class Mugging : Callout
    {
        private readonly Random rnd = new Random();
        Ped suspect, victim;

        public Mugging()
        {
            float offsetX = rnd.Next(100, 450);
            float offsetY = rnd.Next(100, 450);

            InitInfo(World.GetNextPositionOnStreet(Game.PlayerPed.GetOffsetPosition(new Vector3(offsetX, offsetY, 0))));

            ShortName = "Mugging";
            CalloutDescription = "A mugging has been reported by a member of the public.";
            ResponseCode = 3;
            StartDistance = 100f;
        }

        public override async Task OnAccept()
        {
            InitBlip();
            suspect = await SpawnPed(RandomUtils.GetRandomPed(), Location);
            victim = await SpawnPed(RandomUtils.GetRandomPed(), suspect.GetOffsetPosition(new Vector3(2f, 4f, 0f)));

            Debug.WriteLine("Mugging callout initialized, test successful.");

            keepTask(suspect);
            keepTask(victim);

            suspect.Weapons.Give(getRandomWeapon(), 1, true, true);
        }

        public override void OnStart(Ped player)
        {
            base.OnStart(player);

            suspect.Task.FightAgainst(victim);

            int chance = rnd.Next(0, 10);
            // 40% chance that the victim attacks the suspect
            if (chance >= 0 && chance <= 3)
            {
                //50% chance the victim has a weapon too
                chance = rnd.Next(0, 10);
                if (chance >= 0 && chance <= 4) victim.Weapons.Give(getRandomWeapon(), 1, true, true);

                victim.Task.FightAgainst(suspect);
            }
            else victim.Task.ReactAndFlee(suspect);
        }

        private static void keepTask(Ped p)
        {
            p.BlockPermanentEvents = true;
            p.AlwaysKeepTask = true;
        }

        private WeaponHash getRandomWeapon()
        {
            return randomMeleeWeaponList[rnd.Next(randomMeleeWeaponList.Count)];
        }
    }
}