using System;
using System.Collections.Generic;
using System.Linq;
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

            Terminal.Shell.AddCommand("FT.DumpTrainCar", DumpTrainCar, 0, 0, "Get the full structure of the train car the player is on");
            Terminal.Autocomplete.Register("FT.DumpTrainCar");

            Terminal.Shell.AddCommand("FT.GetTransform", GetTransform, 0, 0, "Get the player transform");
            Terminal.Autocomplete.Register("FT.GetTransform");
        }

        public static void GetCarData( CommandArg[] args )
        {
            TrainCar currentCar = PlayerManager.Car;
            if( currentCar == null )
            {
                Debug.Log("Player is not currently on a car");
                return;
            }

            Track currentTrack = currentCar.logicCar.CurrentTrack;

            Debug.Log($"Current car: {currentCar.carType.DisplayName()} {currentCar.ID} on track {currentTrack.ID.FullDisplayID}");
        }

        public static void DumpTrainCar( CommandArg[] args )
        {
            TrainCar currentCar = PlayerManager.Car;
            if( currentCar == null )
            {
                Debug.Log("Player is not currently on a car");
                return;
            }

            Debug.Log($"Dumping structure of {currentCar.carType.DisplayName()}");
            string structure = GameObjectDumper.DumpObject(currentCar.gameObject);
            Debug.Log(structure);
        }

        public static void GetTransform( CommandArg[] args )
        {
            var tform = PlayerManager.PlayerTransform;

            Debug.Log($"Player transform: p: {tform.position}, r: {tform.rotation}");
        }
    }
}
