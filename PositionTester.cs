using System;
using System.Collections.Generic;
using System.Linq;
using CommandTerminal;
using UnityEngine;

namespace FoxyTools
{
    static class PositionTester
    {
        static GameObject PosIndicator = null;
        public static bool Active = false;

        public static void RegisterCommands()
        {
            Terminal.Shell.AddCommand("FT.Pointer", SetActive, 0, 0, "Activate/deactivate the pointer tool");
            Terminal.Autocomplete.Register("FT.Pointer");
        }

        public static void SetActive( CommandArg[] args )
        {
            if( Terminal.IssuedError ) return;


            if( Active )
            {
                Deactivate();
            }
            else
            {
                Activate();
            }
        }

        public static void Activate()
        {
            if( PosIndicator == null )
            {
                PosIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                PosIndicator.transform.localScale = Vector3.one / 8;
                PosIndicator.transform.position = new Vector3(0, -1000, 0);
                PosIndicator.name = "FT_PosIndicator";

                var col = PosIndicator.GetComponent<SphereCollider>();
                if( col != null ) UnityEngine.Object.DestroyImmediate(col);

                PosIndicator.AddComponent<PosIndicatorManager>();
            }

            PosIndicator.SetActive(true);
        }

        public static void Deactivate()
        {
            if( PosIndicator != null ) PosIndicator.SetActive(false);
        }
    }

    public class PosIndicatorManager : MonoBehaviour
    {
        private float x = 0;
        private float y = 0;
        private float z = 0;

        private Vector3 norm = Vector3.zero;
        private string hitObjName = "N/A";

        private void OnDisable()
        {
            gameObject.transform.position = new Vector3(0, -1000, 0);
        }

        private void OnGUI()
        {
            Vector2 camCenter = new Vector2(PlayerManager.PlayerCamera.pixelWidth / 2, PlayerManager.PlayerCamera.pixelHeight / 2);
            Ray lookLine = PlayerManager.PlayerCamera.ScreenPointToRay(camCenter);

            if( Physics.Raycast(lookLine, out RaycastHit hit) )
            {
                x = hit.point.x;
                y = hit.point.y;
                z = hit.point.z;

                gameObject.transform.position = hit.point;

                norm = hit.normal.normalized;

                hitObjName = hit.collider.gameObject?.name ?? "???";
            }
            else
            {
                x = y = z = 0;
                norm = Vector3.zero;
                hitObjName = "N/A";
            }

            int top = Screen.height / 3;
            int width = 130;
            int left = Screen.width - width - 10;
            int height = Screen.height / 3;

            int boxLeft = left + 10;

            GUI.Box(new Rect(left, top, width, height), "Position Indicator");

            GUI.Label(new Rect(boxLeft, top + 40, 110, 20), $"x = {x}");
            GUI.Label(new Rect(boxLeft, top + 70, 110, 20), $"y = {y}");
            GUI.Label(new Rect(boxLeft, top + 100, 110, 20), $"z = {z}");

            GUI.Label(new Rect(boxLeft, top + 130, 110, 20), $"nx = {norm.x}");
            GUI.Label(new Rect(boxLeft, top + 160, 110, 20), $"ny = {norm.y}");
            GUI.Label(new Rect(boxLeft, top + 190, 110, 20), $"nz = {norm.z}");

            GUI.Label(new Rect(boxLeft, top + 220, 110, 20), $"obj = {hitObjName}");
        }
    }
}
