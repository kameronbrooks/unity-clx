using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLX
{
    public partial class Compiler
    {
        public bool Compile_If()
        {
            return false;
        }
        public bool Compile_For()
        {
            return false;
        }
        public bool Compile_While()
        {
            return false;
        }
        public bool Compile_Foreach()
        {
            return false;
        }
        public bool Compile_Block()
        {
            return false;
        }
        public void Compile_Expression()
        {
            Debug.Log("**Expression:" + Peek());
            Compile_AddSub();
        }

    }
}