using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLX
{
    public unsafe class Heap : MonoBehaviour
    {
        public class RuntimeObject
        {
            private static int BASE_ID = System.Int32.MinValue + 1;
            public RuntimeObject()
            {
                _id = BASE_ID++;
            }
            public RuntimeObject(object obj)
            {
                _id = BASE_ID++;
                refCount = 0;
                ptr = obj;
            }
            public int refCount;
            public object ptr;
            public int _id;
            public Datatype type;

            public override int GetHashCode()
            {
                return _id;
            }
        }

        Dictionary<int, RuntimeObject> _objects;

        public Heap()
        {
            _objects = new Dictionary<int, RuntimeObject>();
        }

        public RuntimeObject Get(int id)
        {
            return _objects[id];
        }
        public void Forget(int id)
        {
            _objects.Remove(id);
        }
        public RuntimeObject NewObject(object data, Datatype type)
        {
            RuntimeObject runtimeObject = new RuntimeObject(data);
            runtimeObject.type = type;
            _objects.Add(runtimeObject._id, runtimeObject);
            return runtimeObject;
        }
    }
}
