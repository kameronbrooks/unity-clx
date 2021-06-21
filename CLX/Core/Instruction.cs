using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLX
{
    public enum OpCode : short
    {
        Term = 0,
        Jump,

        /* =-=-= Branches =-=-= */
        BrchTrue,
        BrchFalse,

        BrchGT_i32,
        BrchLT_i32,
        BrchGTEq_i32,
        BrcgLTEq_i32,

        BrchEq_i32,
        BrchNEq_i32,

        /* =-=-= Stack Manip =-=-= */
        LdC_i8, LdC_i32,
        LdArg_i8, LdArg_i32,
        StArg_i8, StArg_i32,

        AllocLoc,
        PushFrame,
        PopFrame,

        /* =-=-= Logic =-=-= */
        And_i8,
        Or_i8,
        Not_i8,

        /* =-=-= Arithmetic =-=-= */
        Neg_i32, Neg_f32,
        Add_i32, Add_f32,
        Sub_i32, Sub_f32,
        Mul_i32, Mul_f32,
        Div_i32, Div_f32,
        Pow_i32, Pow_f32,
        Mod_i32, Mod_f32


    }
    public unsafe struct Instruction
    {
        public OpCode opCode;
        public fixed byte data[32];

        public Instruction(OpCode op)
        {
            unsafe
            {
                fixed(byte* bdata = this.data)
                {
                    opCode = op;
                    *(int*)(bdata) = 0;
                }
                
            }
            
        }
        public Instruction(OpCode op, long val)
        {
            unsafe
            {
                fixed (byte* bdata = this.data)
                {
                    opCode = op;
                    *(long*)(bdata) = val;
                }

            }
        }
        public Instruction(OpCode op, int val)
        {
            unsafe
            {
                fixed (byte* bdata = this.data)
                {
                    opCode = op;
                    *(int*)(bdata) = val;
                }

            }
        }
        public Instruction(OpCode op, short val)
        {
            unsafe
            {
                fixed (byte* bdata = this.data)
                {
                    opCode = op;
                    *(short*)(bdata) = val;
                }

            }
        }
        public Instruction(OpCode op, byte val)
        {
            unsafe
            {
                fixed (byte* bdata = this.data)
                {
                    opCode = op;
                    *(byte*)(bdata) = val;
                }

            }
        }
        public Instruction(OpCode op, bool val)
        {
            unsafe
            {
                fixed (byte* bdata = this.data)
                {
                    opCode = op;
                    *(bool*)(bdata) = val;
                }

            }
        }

    }

}