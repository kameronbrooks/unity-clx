using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLX
{
    public unsafe struct Thread
    {
        const int SIZE_BYTE = sizeof(byte);
        const int SIZE_INT16 = sizeof(short);
        const int SIZE_INT32 = sizeof(int);
        const int SIZE_INT64 = sizeof(long);
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
                        while(continueLoop)
                        {
                            switch(ip->opCode)
                            {
                                case OpCode.Jump:
                                    ip = instructions + *(int*)ip->data;                                   
                                    break;
                                case OpCode.BrchFalse:
                                    ip = (*((bool*)sp++)) ? instructions + *(int*)ip->data : ip + 1;
                                    break;
                                case OpCode.BrchTrue:
                                    ip = (*((bool*)sp++)) ? ip + 1 : instructions + *(int*)ip->data;
                                    break;
                                case OpCode.PushFrame:
                                    // push return instruction
                                    *((Instruction**)(sp -= SIZE_INT64)) = ip + 1;
                                    // push old frame pointer
                                    *((byte**)(sp -= sizeof(byte*))) = sp;
                                    fp = sp;
                                    break;
                                case OpCode.PopFrame:
                                    // pop frame pointer
                                    fp = *((byte**)sp);
                                    // pop instruction pointer
                                    ip = *((Instruction**)(sp += SIZE_INT64));
                                    sp += SIZE_INT64;
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