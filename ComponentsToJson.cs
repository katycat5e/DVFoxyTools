using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DV.CabControls.Spec;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace FoxyTools
{
    public static class ComponentsToJson
    {
        public static JArray Colliders( IEnumerable<Collider> colliders )
        {
            var colliderList = new JArray();

            foreach( Collider collider in colliders )
            {
                colliderList.Add(Collider(collider));
            }

            return colliderList;
        }

        public static JObject Collider( Collider collider )
        {
            var props = new JObject();

            if( collider is BoxCollider bc )
            {
                props.Add(new JProperty("type", "box"));
                props.Add(new JProperty("center", $"{bc.center.x},{bc.center.y},{bc.center.z}"));
                props.Add(new JProperty("size", $"{bc.size.x},{bc.size.y},{bc.size.z}"));
            }
            else if( collider is CapsuleCollider cc )
            {
                props.Add(new JProperty("type", "capsule"));
                props.Add(new JProperty("center", $"{cc.center.x},{cc.center.y},{cc.center.z}"));
                props.Add(new JProperty("direction", $"{cc.direction}"));
                props.Add(new JProperty("height", $"{cc.height}"));
                props.Add(new JProperty("radius", $"{cc.radius}"));
            }
            else if( collider is SphereCollider sc )
            {
                props.Add(new JProperty("type", "sphere"));
                props.Add(new JProperty("center", $"{sc.center.x},{sc.center.y},{sc.center.z}"));
                props.Add(new JProperty("radius", $"{sc.radius}"));
            }
            else if( collider is MeshCollider mc )
            {
                props.Add(new JProperty("type", "mesh"));
                props.Add(new JProperty("mesh", mc.sharedMesh.name));
            }

            return props;
        }

        public static JObject AnimationCurve( AnimationCurve curve )
        {
            var props = new JObject()
            {
                new JProperty("preWrapMode", curve.preWrapMode),
                new JProperty("postWrapMode", curve.postWrapMode)
            };

            var keyArray = new JArray();
            foreach( Keyframe key in curve.keys )
            {
                keyArray.Add(Keyframe(key));
            }

            props.Add("keys", keyArray);
            return props;
        }

        public static JObject Keyframe( Keyframe key )
        {
            return new JObject()
            {
                new JProperty("time", key.time),
                new JProperty("value", key.value),
                new JProperty("weightedMode", key.weightedMode),
                new JProperty("inTangent", key.inTangent),
                new JProperty("inWeight", key.inWeight),
                new JProperty("outTangent", key.outTangent),
                new JProperty("outWeight", key.outWeight)
            };
        }

        public static JObject CarDamageProperties( CarDamageProperties props )
        {
            return new JObject()
            {
                { "maxHealth", props.maxHealth },
                { "dmgResistance", props.damageResistance },
                { "dmgMultiplier", props.damageMultiplier },
                { "fireResistance", props.fireResistance },
                { "fireMultiplier", props.fireDamageMultiplier },
                { "dmgTolerance", props.damageTolerance }
            };
        }

        public static JObject DrivingForce( DrivingForce driver )
        {
            return new JObject()
            {
                { "frictionCoefficient", driver.frictionCoeficient },
                { "preventWheelslip", driver.preventWheelslip },
                { "sandCoefMax", driver.sandCoefMax },
                { "slopeCoefMultiplier", driver.slopeCoeficientMultiplier },
                { "wheelslipFrictionCurve", AnimationCurve(driver.wheelslipToFrictionModifierCurve) }
            };
        }
        
        private static JToken AudioPoolData( AudioPoolReferences.AudioPoolData poolData )
        {
            var prefabInfo = GameObjectDumper.DumpObject(poolData.audioPrefab);

            return new JObject()
            {
                { "trainCarType", poolData.trainCarType.DisplayName() },
                { "poolSize", poolData.poolSize },
                { "audioPrefab", prefabInfo }
            };
        }

        public static JToken AudioPoolReferences( AudioPoolReferences audioPool )
        {
            var defaultData = AudioPoolData(audioPool.defaultData);

            var poolArray = new JArray();
            foreach( AudioPoolReferences.AudioPoolData subPool in audioPool.poolData )
            {
                poolArray.Add(AudioPoolData(subPool));
            }

            var result = new JObject()
            {
                { "defaultData", defaultData },
                { "poolData", poolArray }
            };

            return result;
        }

        public static JToken GenericObject( object obj, int depthLimit = 20 )
        {
            if( obj == null ) return JValue.CreateNull();
            if( depthLimit == 0 ) return "Depth limit reached";

            Type objType = obj.GetType();
            if( typeof(IEnumerable).IsAssignableFrom(objType) )
            {
                if( obj is IEnumerable val )
                {
                    var arr = new JArray();
                    foreach( object member in val )
                    {
                        arr.Add(GenericObject(member, depthLimit - 1));
                    }
                    return arr;
                }
                else
                {
                    FoxyToolsMain.ModEntry.Logger.Warning("Failed to get array field value");
                    return JValue.CreateNull();
                }
            }
            else if( (obj is MonoBehaviour) || (obj is ScriptableObject) )
            {
                var script = obj as UnityEngine.Object;

                if( script )
                {
                    var props = new JObject()
                    {
                        { "name", script.name }
                    };

                    var fields = obj.GetType().GetFields();
                    foreach( FieldInfo field in fields )
                    {
                        var token = GenericObject(field.GetValue(obj), depthLimit - 1);
                        props.Add(field.Name, token);
                    }
                    return props;
                }
                else
                {
                    return $"Null {objType.Name}";
                }
            }
            else if( obj is UnityEngine.Object unityVal )
            {
                if( unityVal ) return unityVal.name;
                return "";
            }
            else if( objType.IsPrimitive )
            {
                return new JValue(obj);
            }
            else
            {
                var props = new JObject();
                var fields = obj.GetType().GetFields();
                foreach( FieldInfo field in fields )
                {
                    var token = GenericObject(field.GetValue(obj), depthLimit - 1);
                    props.Add(field.Name, token);
                }
                return props;
            }
        }
    }
}
