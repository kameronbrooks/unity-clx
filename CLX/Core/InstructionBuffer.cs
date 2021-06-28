using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLX
{
    public class InstructionBuffer
    {
        public class Node
        {
            public Instruction instruction;
            public Node previous;
            public Node next;
            public int index;
            public object reference;

            /// <summary>
            /// Get last node in chain
            /// </summary>
            /// <returns></returns>
            public Node GetTail()
            {
                if (next == null) return this;
                else return next.GetTail();
            }
            /// <summary>
            /// Get first node in chain
            /// </summary>
            /// <returns></returns>
            public Node GetHead()
            {
                if (previous == null) return this;
                else return previous.GetHead();
            }
            /// <summary>
            /// Insert a node after this node
            /// </summary>
            /// <param name="node"></param>
            /// <returns></returns>
            public Node InsertAfter(Node node)
            {
                if (next != null)
                {
                    node.GetTail().next = next;
                }
                return next = node;
            }
            /// <summary>
            /// Insert a node before this node
            /// </summary>
            /// <param name="node"></param>
            /// <returns></returns>
            public Node InsertBefore(Node node)
            {
                if (previous != null)
                {
                    node.GetHead().previous = previous;
                }
                return previous = node;
            }
        }

        Node _root;
        Node _tail;
        /// <summary>
        /// Count the nodes from head to tail
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            int i = 0;
            Node c = _root;
            while (c != null)
            {
                ++i;
                c = c.next;
            }
            return i;
        }
        /// <summary>
        /// Update the indices of every node
        /// </summary>
        public void UpdateIndices()
        {
            int i = 0;
            Node c = _root;
            while (c != null)
            {
                c.index = i;
                ++i;
                c = c.next;
            }
        }
        /// <summary>
        /// This will bake any instruction indices into the data fields of the referencing instructions. 
        /// This must be done, otherwise the jumps and branches will not point to the correct instructions
        /// Indices must be updated before running this
        /// </summary>
        public void BakeInstructionReferences()
        {
            Node c = _root;
            while (c != null)
            {
                unsafe
                {
                    switch (c.instruction.opCode)
                    {
                        case OpCode.Jump:
                        case OpCode.BrchFalse:
                        case OpCode.BrchTrue:
                            fixed (byte* data = c.instruction.data)
                            {
                                *(int*)data = ((Node)c.reference).index;
                            }    
                            break;
                        default:
                            break;
                    }
                }
                
                c = c.next;
            }
        }
        /// <summary>
        /// Add instruction to buffer
        /// </summary>
        /// <param name="instruction"></param>
        /// <returns></returns>
        public Node Add(Instruction instruction)
        {
            if (_root == null)
            {
                _root = new Node
                {
                    instruction = instruction,
                    previous = null,
                    next = null
                };

                return _tail = _root;
            }
            else
            {
                Node newNode = new Node
                {
                    instruction = instruction,
                    previous = null,
                    next = null
                };
                return _tail = _tail.InsertAfter(newNode);

            }
        }
        public Node Add(OpCode op)
        {
            return Add(new Instruction(op));
        }
        public Node Add(OpCode op, bool val)
        {
            return Add(new Instruction(op, val));
        }
        public Node Add(OpCode op, byte val)
        {
            return Add(new Instruction(op, val));
        }
        public Node Add(OpCode op, short val)
        {
            return Add(new Instruction(op, val));
        }
        public Node Add(OpCode op, int val)
        {
            return Add(new Instruction(op, val));
        }
        /// <summary>
        /// Make sure that head and tail are correct
        /// </summary>
        public void Normalize()
        {
            while (_tail.next != null)
            {
                _tail = _tail.next;
            }
            while (_root.previous != null)
            {
                _root = _root.previous;
            }
        }
        /// <summary>
        /// Export as array
        /// </summary>
        /// <returns></returns>
        public Instruction[] ToArray()
        {
            List<Instruction> output = new List<Instruction>();
            int i = 0;
            Node c = _root;
            while (c != null)
            {
                output.Add(c.instruction);
                c = c.next;
            }
            return output.ToArray();
        }
        /// <summary>
        /// Bake node references and export as array
        /// </summary>
        /// <returns></returns>
        public Instruction[] BakeAndExport()
        {
            UpdateIndices();
            BakeInstructionReferences();
            return ToArray();

        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            Node cNode = _root;
            while (cNode != null)
            {
                unsafe
                {
                    fixed (byte* data = cNode.instruction.data)
                    {
                        sb.Append(cNode.index + " : " + cNode.instruction.opCode.ToString() + "\t" + *(int*)data + "\n");
                    }
                }
                
                cNode = cNode.next;
            }
            return sb.ToString();
        }
    }
}
