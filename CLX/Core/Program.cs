using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace CLX
{
    public class Program
    {
        public class Resource
        {
            public string name;
            public int id;
            public object[] args;
            public CSharpAPI.API_CALL func;
        }

        public List<Resource> resources;
        public object[] constObjects;
        public Instruction[] instructions;
        public int minStackSize;

        public Program()
        {
            resources = new List<Resource>();
            minStackSize = 512;
        }
    }

}