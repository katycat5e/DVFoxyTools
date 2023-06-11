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
        public static bool IgnoreCurves = false;

        public static void Reset()
        {
            IgnoreCurves = false;
        }

        private static JToken GetSpecialConversion(object obj)
        {
            var objType = obj.GetType();

            if (obj is Collider collider)
            {
                return Collider(collider);
            }
            else if (obj is AnimationCurve curve)
            {
                return AnimationCurve(curve);
            }
            else if (obj is CarDamageProperties damage)
            {
                return CarDamageProperties(damage);
            }
            else if (obj is DrivingForce force)
            {
                return DrivingForce(force);
            }
            else if (obj is AudioPoolReferences pool)
            {
                return AudioPoolReferences(pool);
            }
            else if (obj is AudioClip clip)
            {
                return AudioClip(clip);
            }
            else if (obj is AudioSource source)
            {
                return AudioSource(source);
            }
            else if (obj is ParticleSystem particles)
            {
                return ParticleSystem(particles);
            }

            return null;
        }

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

            props.Add("isTrigger", collider.isTrigger);

            return props;
        }

        public static JObject AnimationCurve( AnimationCurve curve )
        {
            var props = new JObject()
            {
                new JProperty("preWrapMode", curve.preWrapMode),
                new JProperty("postWrapMode", curve.postWrapMode)
            };

            //var keyArray = new JArray();
            //foreach( Keyframe key in curve.keys )
            //{
            //    keyArray.Add(Keyframe(key));
            //}

            //props.Add("keys", keyArray);

            if (IgnoreCurves || (curve.keys == null) || (curve.keys.Length == 0))
            {
                props.Add("points", !IgnoreCurves ? "[]" : "[...]");
                return props;
            }

            var keysSorted = curve.keys.OrderBy(k => k.time);
            var firstKey = curve.keys.First();
            var lastKey = curve.keys.Last();

            float step = (lastKey.time - firstKey.time) / 1000;
            var pts = Enumerable.Range(0, 1001).Select(t => curve.Evaluate(firstKey.time + (t * step)));
            string ptString = string.Join(",", pts);

            props.Add("minT", firstKey.time);
            props.Add("maxT", lastKey.time);
            props.Add("points", $"[{ptString}]");

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

            JToken audioComp = JValue.CreateNull();
            if (poolData.audioPrefab)
            {
                audioComp = GenericObject(poolData.audioPrefab.GetComponent<TrainAudio>(), 6);
            }

            return new JObject()
            {
                { "trainCarType", poolData.trainCarType.DisplayName() },
                { "poolSize", poolData.poolSize },
                { "audioPrefab", prefabInfo },
                { "trainAudio", audioComp }
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

        public static JToken AudioClip(AudioClip audioClip)
        {
            if (!audioClip) return JValue.CreateNull();

            return new JObject()
            {
                { "_componentType", "AudioClip" },
                { "_name", audioClip.name },
                { "length", audioClip.length },
                { "channels", audioClip.channels },
                { "frequency", audioClip.frequency },
                { "samples", audioClip.samples },
            };
        }

        public static JObject AudioSource(AudioSource source)
        {
            return new JObject()
            {
                { "_componentType", "AudioSource" },
                { "_name", source.name },
                { "clip", AudioClip(source.clip) },
                { "playOnAwake", source.playOnAwake },
                { "rolloffMode", source.rolloffMode.ToString() },
                { "minDistance", source.minDistance },
                { "maxDistance", source.maxDistance },
                { "spread", source.spread },
                { "pitch", source.pitch },
                { "volume", source.volume },
                { "spatialBlend", source.spatialBlend },
                { "dopplerLevel", source.dopplerLevel },
                { "ignoreListenerPause", source.ignoreListenerPause },
                { "outputAudioMixerGroup", GenericObject(source.outputAudioMixerGroup, 1) }
            };
        }

        public static JObject ParticleSystem(ParticleSystem particles)
        {
            return new JObject()
            {
                { "_name", particles.gameObject.name },
                { "_componentType", "ParticleSystem" },
                { "main", MainModule(particles.main) },
                { "emission", EmissionModule(particles.emission) },
            };
        }

        private static JObject MainModule(ParticleSystem.MainModule main)
        {
            return new JObject()
            {
                { "maxParticles", main.maxParticles },
                { "startLifetime", main.startLifetime.constant },
                { "startLifetimeMult", main.startLifetimeMultiplier },

                { "startSize", main.startSize.constant },
                { "startSizeMult", main.startSizeMultiplier },
                { "startSpeed", main.startSpeed.constant },
                { "startSpeedMult", main.startSpeedMultiplier },
            };
        }

        private static JObject EmissionModule(ParticleSystem.EmissionModule emission)
        {
            return new JObject()
            {
                { "rateOverDistance", emission.rateOverDistance.constant },
                { "rateOverDistanceMult", emission.rateOverDistanceMultiplier },
                { "rateOverTime", emission.rateOverTime.constant },
                { "rateOverTimeMult", emission.rateOverTimeMultiplier },
            };
        }

        public static JToken GenericObject( object obj, int depthLimit = 20 )
        {
            if( obj == null ) return JValue.CreateNull();
            if( depthLimit == 0 ) return "Depth limit reached";

            Type objType = obj.GetType();
            if (objType.IsPrimitive || obj is string)
            {
                return new JValue(obj);
            }
            else if (objType.IsEnum)
            {
                return $"{Enum.GetName(objType, obj)} ({objType.Name})";
            }
            else if ( typeof(IEnumerable).IsAssignableFrom(objType) )
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
            else if (GetSpecialConversion(obj) is JToken specialJson)
            {
                return specialJson;
            }
            else if (obj is UnityEngine.Object script)
            {
                if( script )
                {
                    var props = new JObject()
                    {
                        { "_componentType", objType.Name },
                        { "_name", script.name }
                    };

                    var fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
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
            //else if( obj is UnityEngine.Object unityVal )
            //{
            //    if( unityVal ) return unityVal.name;
            //    return "";
            //}
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
