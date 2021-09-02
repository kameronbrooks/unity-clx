using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLX
{
    public enum OpCode : short
    {
        NoOp = 0,
        Term,
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

        LdLoc_i8, LdLoc_i32,
        StLoc_i8, StLoc_i32,

        AllocLoc,
        PushFrame,
        PopFrame,

        /* =-=-= Comparison =-=-= */
        Eq_i8, Eq_i32, Eq_f32,
        NEq_i8, NEq_i32, NEq_f32,
        LT_i32, LT_f32,
        GT_i32, GT_f32,
        LTEq_i32, LTEq_f32,
        GTEq_i32, GTEq_f32,


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
        Pow_f32,
        Mod_i32, Mod_f32,
        Inc_i32,
        Dec_i32,

        /* =-=-= IO =-=-= */
        Push_API,
 
        Call_API_Get,
        Call_Extern_Get,
        Call_API_Set,
        Call_Extern_Set,
        Call_API_FUNC_0,
        Call_API_FUNC_1,

        /* =-=-= Conv =-=-= */
        Conv_i32_f32,

        /* =-=-= Print =-=-= */
        Prnt_bool,
        Prnt_i32,
        Prnt_f32

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
        public Instruction(OpCode op, float val)
        {
            unsafe
            {
                fixed (byte* bdata = this.data)
                {
                    opCode = op;
                    *(float*)(bdata) = val;
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