#define SAFE_LOOP
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLX
{
    public unsafe struct Thread
    {
        const int SIZE_BYTE =       sizeof(byte);
        const int SIZE_INT16 =      sizeof(short);
        const int SIZE_INT32 =      sizeof(int);
        const int SIZE_INT64 =      sizeof(long);
        const int STACK_SHIFT_INT32_COMPARISON = sizeof(int) * 2 - 1;


        public const int REGISTER_COUNT = 8;
        public fixed int register[REGISTER_COUNT];
        public byte[] _stack;
        int _stackSize;

        public Thread(int stackSize)
        {
            _stackSize = stackSize;
            _stack = new byte[_stackSize];
        }

        /// <summary>
        /// Execute the specified program on the apiTarget
        /// </summary>
        /// <param name="program"></param>
        /// <param name="apiTarget"></param>
        /// <returns></returns>
        public int Execute(Program program, object apiTarget)
        {
            // Allocate stack if the current stack is not large enough for the current program
            if (_stack.Length < program.minStackSize)
            {
                _stackSize = program.minStackSize;
                _stack = new byte[_stackSize];
            }
            // Main execution loop
            unsafe
            {
                fixed (Instruction* instructions = &program.instructions[0])
                {
                    fixed(byte* s_buffer = &_stack[0])
                    {
                        // initialize stack pointer to top of stack
                        byte* sp = s_buffer + _stackSize;
                        // initialize frame pointer
                        byte* fp = sp;
                        // initialize instruction pointer
                        Instruction* ip = instructions;

                        bool continueLoop = true;
#if SAFE_LOOP
                        int __safety__accum = 1000000;
#endif
                        while(continueLoop)
                        {
#if SAFE_LOOP
                            // Incase we are debugging the engine and want a failsafe to end the thread without freezing the program in the even of an infinite loop
                            --__safety__accum; 
                            if (__safety__accum < 0)
                            {
                                continueLoop = false;
                                Debug.LogError("Exceded the saftey count. Potential infinite loop.");
                            }
#endif
                            // Main op code logic
                            switch(ip->opCode)
                            {
                                case OpCode.Jump:
                                    ip = instructions + *(int*)ip->data;                                   
                                    break;
                                /* =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-= */
                                /* =-=-=-=-=-=-=-=-=-= Branches =-=-=-=-=-=-=-=-=-=-=    */
                                /* =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-= */
                                case OpCode.BrchTrue:
                                    ip = (*((bool*)(sp++))) ? (instructions + (*(int*)ip->data)) : ip + 1;
                                    break;
                                case OpCode.BrchFalse:
                                    ip = (*((bool*)(sp++))) ? ip + 1 : instructions + *(int*)ip->data;
                                    break;

                                /* =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-= */
                                /* =-=-=-=-=-=-=-=-=-=    Logic    =-=-=-=-=-=-=-=-=-=-= */
                                /* =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-= */
                                case OpCode.And_i8:
                                    (*((bool*)sp++)) = (*((bool*)sp + 1)) && (*((bool*)sp));
                                    ++ip;
                                    break;
                                case OpCode.Or_i8:
                                    (*((bool*)sp++)) = (*((bool*)sp + 1)) || (*((bool*)sp));
                                    ++ip;
                                    break;
                                case OpCode.Not_i8:
                                    (*((bool*)sp)) = !(*((bool*)sp));
                                    ++ip;
                                    break;

                                /* =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-= */
                                /* =-=-=-=-=-=-=-=-=-= Comparison  =-=-=-=-=-=-=-=-=-=-= */
                                /* =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-= */
                                case OpCode.Eq_i32:
                                    *((bool*)(sp + STACK_SHIFT_INT32_COMPARISON)) = *((int*)(sp + SIZE_INT32)) == *((int*)(sp));
                                    sp += STACK_SHIFT_INT32_COMPARISON;
                                    ++ip;
                                    break;
                                case OpCode.NEq_i32:
                                    *((bool*)(sp + STACK_SHIFT_INT32_COMPARISON)) = *((int*)(sp + SIZE_INT32)) != *((int*)(sp));
                                    sp += STACK_SHIFT_INT32_COMPARISON;
                                    ++ip;
                                    break;
                                case OpCode.LT_i32:
                                    *((bool*)(sp + STACK_SHIFT_INT32_COMPARISON)) = ((*((int*)(sp + SIZE_INT32))) < (*((int*)(sp))));
                                    sp += STACK_SHIFT_INT32_COMPARISON;
                                    ++ip;
                                    break;
                                case OpCode.GT_i32:
                                    *((bool*)(sp + STACK_SHIFT_INT32_COMPARISON)) = *((int*)(sp + SIZE_INT32)) > *((int*)(sp));
                                    sp += STACK_SHIFT_INT32_COMPARISON;
                                    ++ip;
                                    break;
                                case OpCode.LTEq_i32:
                                    *((bool*)(sp + STACK_SHIFT_INT32_COMPARISON)) = *((int*)(sp + SIZE_INT32)) <= *((int*)(sp));
                                    sp += STACK_SHIFT_INT32_COMPARISON;
                                    ++ip;
                                    break;
                                case OpCode.GTEq_i32:
                                    *((bool*)(sp + STACK_SHIFT_INT32_COMPARISON)) = *((int*)(sp + SIZE_INT32)) >= *((int*)(sp));
                                    sp += STACK_SHIFT_INT32_COMPARISON;
                                    ++ip;
                                    break;
                                /* =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-= */
                                /* =-=-=-=-=-=-=-=-=-= Stack Manip =-=-=-=-=-=-=-=-=-=-= */
                                /* =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-= */
                                case OpCode.LdC_i8:
                                    *((byte*)(sp--)) = *(byte*)ip->data;
                                    ++ip;
                                    break;
                                case OpCode.LdC_i32:
                                    *((int*)(sp -= SIZE_INT32)) = *(int*)ip->data;
                                    ++ip;
                                    break;
                                case OpCode.AllocLoc:
                                    sp -= *(int*)ip->data;
                                    ++ip;
                                    break;
                                case OpCode.LdArg_i32:
                                    *((int*)(sp -= SIZE_INT32)) = *(int*)(fp + (*(int*)ip->data));
                                    ++ip;
                                    break;
                                case OpCode.LdArg_i8:
                                    *((byte*)(sp--)) = *(byte*)(fp + (*(int*)ip->data));
                                    ++ip;
                                    break;
                                case OpCode.StArg_i32:
                                    *(int*)(fp + (*(int*)ip->data)) = *((int*)(sp));
                                    sp += SIZE_INT32;
                                    ++ip;
                                    break;
                                case OpCode.StArg_i8:
                                    *(byte*)(fp + (*(int*)ip->data)) = *((byte*)(sp));
                                    ++sp;
                                    ++ip;
                                    break;
                                case OpCode.LdLoc_i32:
                                    *((int*)(sp -= SIZE_INT32)) = *(int*)(fp - (*(int*)ip->data));
                                    ++ip;
                                    break;
                                case OpCode.StLoc_i32:
                                    *(int*)(fp - (*(int*)ip->data)) = *((int*)(sp));
                                    sp += SIZE_INT32;
                                    ++ip;
                                    break;
                                case OpCode.PushFrame:
                                    // push return instruction
                                    *((Instruction**)(sp -= SIZE_INT64)) = ip + 1;
                                    // push old frame pointer
                                    *((byte**)(sp - sizeof(byte*))) = sp;
                                    sp -= sizeof(byte*);
                                    fp = sp;
                                    break;
                                case OpCode.PopFrame:
                                    // pop frame pointer
                                    fp = *((byte**)sp);
                                    // pop instruction pointer
                                    ip = *((Instruction**)(sp += SIZE_INT64));
                                    sp += SIZE_INT64;
                                    break;
                                /* =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-= */
                                /* =-=-=-=-=-=-=-=-=-= Arithmetic  =-=-=-=-=-=-=-=-=-=-= */
                                /* =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-= */
                                case OpCode.Neg_i32:
                                    *((int*)(sp)) *= -1;
                                    ++ip;
                                    break;
                                case OpCode.Neg_f32:
                                    *((float*)(sp)) *= -1;
                                    ++ip;
                                    break;
                                case OpCode.Add_i32:
                                    *((int*)(sp + SIZE_INT32)) += *((int*)(sp));
                                    sp += SIZE_INT32;
                                    ++ip;
                                    break;
                                case OpCode.Add_f32:
                                    *((float*)(sp + SIZE_INT32)) += *((float*)(sp));
                                    sp += SIZE_INT32;
                                    ++ip;
                                    break;
                                case OpCode.Sub_i32:
                                    *((int*)(sp + SIZE_INT32)) -= *((int*)(sp));
                                    sp += SIZE_INT32;
                                    ++ip;
                                    break;
                                case OpCode.Sub_f32:
                                    *((float*)(sp + SIZE_INT32)) -= *((float*)(sp));
                                    sp += SIZE_INT32;
                                    ++ip;
                                    break;
                                case OpCode.Mul_i32:
                                    *((int*)(sp + SIZE_INT32)) *= *((int*)(sp));
                                    sp += SIZE_INT32;
                                    ++ip;
                                    break;
                                case OpCode.Mul_f32:
                                    *((float*)(sp + SIZE_INT32)) *= *((float*)(sp));
                                    sp += SIZE_INT32;
                                    ++ip;
                                    break;
                                case OpCode.Div_i32:
                                    *((int*)(sp + SIZE_INT32)) /= *((int*)(sp));
                                    sp += SIZE_INT32;
                                    ++ip;
                                    break;
                                case OpCode.Div_f32:
                                    *((float*)(sp + SIZE_INT32)) /= *((float*)(sp));
                                    sp += SIZE_INT32;
                                    ++ip;
                                    break;
                                case OpCode.Mod_i32:
                                    *((int*)(sp + SIZE_INT32)) %= *((int*)(sp));
                                    sp += SIZE_INT32;
                                    ++ip;
                                    break;
                                case OpCode.Mod_f32:
                                    *((float*)(sp + SIZE_INT32)) %= *((float*)(sp));
                                    sp += SIZE_INT32;
                                    ++ip;
                                    break;
                                case OpCode.Pow_f32:
                                    *((float*)(sp + SIZE_INT32)) = Mathf.Pow(*((float*)(sp + SIZE_INT32)) ,* ((float*)(sp)));
                                    sp += SIZE_INT32;
                                    ++ip;
                                    break;
                                case OpCode.Inc_i32:
                                    ++*((int*)(sp));
                                    ++ip;
                                    break;
                                case OpCode.Dec_i32:
                                    --*((int*)(sp));
                                    ++ip;
                                    break;
                                /* =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-= */
                                /* =-=-=-=-=-=-=-=-=-= Arithmetic  =-=-=-=-=-=-=-=-=-=-= */
                                /* =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-= */
                                case OpCode.Prnt_bool:
                                    Debug.Log(*((bool*)(sp)) ? "true" : "false");
                                    sp++;
                                    ++ip;
                                    break;
                                case OpCode.Prnt_i32:
                                    Debug.Log(*((int*)(sp)));
                                    sp += SIZE_INT32;
                                    ++ip;
                                    break;
                                case OpCode.Term:
                                    continueLoop = false;
                                    break;
                                default:
                                    throw new System.Exception("Invalid Operation: " + ip->opCode.ToString());
                            }
                        }
                    }
                }
            }
            return 0;
        }

    }
    public class Runtime
    {
        
    }

}