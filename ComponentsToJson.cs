using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
