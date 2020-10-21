using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandTerminal;
using DV.Logic.Job;
using UnityEngine;

namespace FoxyTools
{
    static class PlayerInfo
    {
        public static void RegisterCommands()
        {
            Terminal.Shell.AddCommand("FT.GetCarData", GetCarData, 0, 0, "Get information about the car the player is in");
            Terminal.Autocomplete.Register("FT.GetCarData");
        }

        public static void GetCarData( CommandArg[] args )
        {
            TrainCar currentCar = PlayerManager.Car;
            Track currentTrack = currentCar.logicCar.CurrentTrack;

            Debug.Log($"Current car: {currentCar.carType.DisplayName()} {currentCar.ID} on track {currentTrack.ID.FullDisplayID}");
        }
    }
}
