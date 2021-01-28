using System;
using System.Collections.Generic;
using System.IO;
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

            Terminal.Shell.AddCommand("FT.DumpObjTexture", DumpTextures, 0, 1, "Export the texture set of an object");
            Terminal.Autocomplete.Register("FT.DumpObjTexture");
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

            Active = !Active;
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

        public static void DumpTextures( CommandArg[] args )
        {
            GameObject targetObject;

            if( args.Length == 1 )
            {
                string objName = args[0].String;
                targetObject = GameObject.Find(objName);

                if( !(targetObject is GameObject) )
                {
                    Debug.LogWarning("Specified object not found");
                    return;
                }
            }
            else if( Active )
            {
                var pointer = PosIndicator.GetComponent<PosIndicatorManager>();
                targetObject = pointer.TargetObject;

                if( targetObject == null )
                {
                    Debug.LogWarning("Pointer does not hit any objects");
                    return;
                }
            }
            else
            {
                Debug.LogWarning("Pointer must be active to export textures");
                return;
            }

            string outputDir = Path.Combine(FoxyToolsMain.ModEntry.Path, "Export", targetObject.name);
            Directory.CreateDirectory(outputDir);

            foreach( var renderer in targetObject.GetComponentsInChildren<MeshRenderer>() )
            {
                foreach( var material in renderer.materials )
                {
                    Debug.Log($"{targetObject.name}.{renderer.name}.{material.name} uses shader {material.shader.name}");

                    if( material.GetTexture("_MainTex") is Texture2D diffuse ) ExportTexture(diffuse, outputDir);
                    if( material.GetTexture("_BumpMap") is Texture2D normal ) ExportTexture(normal, outputDir, true);
                    if( material.GetTexture("_MetallicGlossMap") is Texture2D specular ) ExportTexture(specular, outputDir);
                    if( material.GetTexture("_EmissionMap") is Texture2D emission ) ExportTexture(emission, outputDir);
                }
            }

            Debug.Log("Exported textures to " + outputDir);
        }

        static void ExportTexture( Texture2D texture, string dir )
        {
            string origName = texture.name;
            bool isCompressed = (texture.format == TextureFormat.DXT5);

            texture = CreateReadableTexture(texture, isCompressed);

            if( isCompressed )
            {
                texture = DecompressTexture(texture);
            }

            string fullPath = Path.Combine(dir, $"{origName}.png");

            byte[] exportData = texture.EncodeToPNG();
            File.WriteAllBytes(fullPath, exportData);
        }

        private static Color DecompressPixel( Color dxt )
        {
            // red<-alpha (x<-w)
            // green is always the same (y)

            Vector2 xy = new Vector2(dxt.a * 2 - 1, dxt.g * 2 - 1);
            float blue = Mathf.Sqrt(1 - Mathf.Clamp01(Vector2.Dot(xy, xy)));    // recalculate the blue channel (z)

            return new Color(dxt.a, dxt.g, blue);
        }

        static Texture2D DecompressTexture( Texture2D source )
        {
            Color[] data = source.GetPixels();
            
            for( int i = 0; i < data.Length; i++ )
            {
                data[i] = DecompressPixel(data[i]);
            }

            Texture2D newTexture = new Texture2D(source.width, source.height, TextureFormat.ARGB32, true, true);
            newTexture.SetPixels(data);
            newTexture.Apply();

            return newTexture;
        }

        static Texture2D CreateReadableTexture( Texture2D source, bool isDTX )
        {
            RenderTexture renderTexture;
            Texture2D readableTexture;

            if( isDTX )
            {
                readableTexture = new Texture2D(source.width, source.height, TextureFormat.ARGB32, true, true);
                renderTexture = RenderTexture.GetTemporary(
                            source.width,
                            source.height,
                            0,
                            RenderTextureFormat.ARGB32,
                            RenderTextureReadWrite.Linear);
            }
            else
            {
                readableTexture = new Texture2D(source.width, source.height);
                renderTexture = RenderTexture.GetTemporary(
                            source.width,
                            source.height,
                            0,
                            RenderTextureFormat.Default);
            }

            Graphics.Blit(source, renderTexture);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTexture;
            readableTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            readableTexture.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTexture);
            return readableTexture;
        }
    }

    public class PosIndicatorManager : MonoBehaviour
    {
        public GameObject TargetObject = null;

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
                TargetObject = hit.collider.gameObject;

                Vector3 shifted = hit.point - WorldMover.currentMove;

                x = shifted.x;
                y = shifted.y;
                z = shifted.z;

                gameObject.transform.position = hit.point;

                norm = hit.normal.normalized;

                hitObjName = hit.collider.gameObject?.name ?? "???";
            }
            else
            {
                TargetObject = null;
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
