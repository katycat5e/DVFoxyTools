using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommandTerminal;
using DV.Logic.Job;
using Harmony12;
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

        private static readonly FieldInfo adhesionField = AccessTools.Field(typeof(DrivingForce), "factorOfAdhesion");
        private static readonly FieldInfo forceLimitField = AccessTools.Field(typeof(DrivingForce), "tractionForceWheelslipLimit");

        [FTCommand(Help = "Print driving force debug")]
        public static void GetForces(CommandArg[] args)
        {
            TrainCar currentCar = PlayerManager.Car;
            if (currentCar == null)
            {
                Debug.Log("Player is not currently on a car");
                return;
            }

            var force = currentCar.GetComponent<DrivingForce>();
            if (force)
            {
                Debug.Log($"Friction Coef:   {force.frictionCoeficient:#.00}");
                Debug.Log($"Adhesion Factor: {(float)adhesionField.GetValue(force):#} kg");
                Debug.Log($"Slip Limit:      {(float)forceLimitField.GetValue(force):#} N");
                Debug.Log($"Slope Mult:      {force.slopeCoeficientMultiplier:#.00}");
                Debug.Log($"Wheelslip:       {force.wheelslip:#.00}");
            }
        }
    }
}
