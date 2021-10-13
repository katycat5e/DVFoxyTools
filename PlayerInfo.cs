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
        [FTCommand(Help = "Get information about the car the player is in")]
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

        [FTCommand(Help = "Get the full structure of the train car the player is on")]
        public static void DumpTrainCar( CommandArg[] args )
        {
            TrainCar currentCar = PlayerManager.Car;
            if( currentCar == null )
            {
                Debug.Log("Player is not currently on a car");
                return;
            }

            Debug.Log($"Dumping structure of {currentCar.carType.DisplayName()}");
            var structure = GameObjectDumper.DumpObject(currentCar.gameObject);
            GameObjectDumper.SendJsonToFile(currentCar.name, "spawned", structure);
        }

        [FTCommand(Help = "Get the player transform")]
        public static void GetTransform( CommandArg[] args )
        {
            var tform = PlayerManager.PlayerTransform;

            Debug.Log($"Player transform: p: {tform.position}, r: {tform.rotation}");
        }
    }
}
