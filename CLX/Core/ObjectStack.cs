using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLX
{
    /// <summary>
    /// A class for holding runtime C# objects to use for calcuations during the execcution of the program
    /// </summary>
    public class ObjectStack
    {
        public object apiTarget;
        object[] _stack;
        int _count;

        public ObjectStack(object apiTarget, int size = 128)
        {
            _stack = new object[size];
            _count = 0;
        }

        public void Push(object elem)
        {
            _stack[_count++] = elem;
        }
        public object Peek()
        {
            return _stack[_count-1];
        }
        public object Pop()
        {
            return _stack[--_count];
        }
        public int count
        {
            get
            {
                return _count;
            }
        }
        protected void Resize()
        {
            int stackLength = _stack.Length;
            object[] newStack = new object[_stack.Length * 2];
            for(int i = 0; i < stackLength; ++i)
            {
                newStack[i] = _stack[i];
            }
            _stack = newStack;
        }

        public object this[int i]
        {
            get
            {
                return _stack[i];
            }
            set
            {
                _stack[i] = value;
            }
        }
        public void PushAPIOntoStack()
        {
            _stack[_count++] = apiTarget;
        }
    }

}