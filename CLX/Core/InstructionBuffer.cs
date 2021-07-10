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
            public Node InsertAfter(InstructionBuffer buffer)
            {
                if(next != null)
                {
                    buffer.root.GetTail().next = next;
                }
                return next = buffer.root;
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
            public Node InsertBefore(InstructionBuffer buffer)
            {
                if (previous != null)
                {
                    buffer.root.previous = previous;
                }
                return previous = buffer.tail;
            }
            /// <summary>
            /// Cuts the chain here making this the root of its own chain
            /// </summary>
            /// <returns></returns>
            public Node Cut()
            {
                if(previous != null)
                {
                    previous.next = null;
                }
                previous = null;
                return this;
            }
        }

        Node _root;
        Node _tail;

        Stack<Node> _forwardBranchTargets;

        public InstructionBuffer()
        {
            _forwardBranchTargets = new Stack<Node>();
        }
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
                        case OpCode.Call_API_Get:
                        case OpCode.Call_Extern_Get:
                            /*
                            fixed (byte* data = c.instruction.data)
                            {
                                *(int*)data = ((Program.Resource)c.reference).id;
                            }
                            */
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
                UpdateForwardBranchTargets(_root);
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
                UpdateForwardBranchTargets(newNode);
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
        public Node Add(OpCode op, float val)
        {
            return Add(new Instruction(op, val));
        }

        public Node Append(InstructionBuffer other)
        {
            if(this.tail == null)
            {
                this._root = other._root;
                Normalize();
                return _tail;
            }
            else
            {
                _tail.InsertAfter(other._root);
                Normalize();
                return _tail;
            }
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

        public Node tail
        {
            get
            {
                return _tail;
            }
        }
        public Node root
        {
            get
            {
                return _root;
            }
        }

        /// <summary>
        /// Push a node onto the forward branch target stack
        /// This is for branches that need to branch to a node that has not been added to the stack yet
        /// Any node in this stack will reference the next node that is added to the buffer
        /// </summary>
        /// <param name="target"></param>
        public void PushForwardBranchTarget(Node target)
        {
            _forwardBranchTargets.Push(target);
        }
        private void UpdateForwardBranchTargets(Node targetNode)
        {
            while(_forwardBranchTargets.Count > 0)
            {
                _forwardBranchTargets.Pop().reference = targetNode;
            }
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
