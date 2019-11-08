using UnityEngine;
using System.Collections;
using System;

namespace ZFrame.Pathfinding
{
    public class Node : IHeapItem<Node>
    {
        public int ver;

        public Node parent;
        public int x;
        public int y;
        public int HeapIndex { get; set; }

        public int gCost;
        public int hCost;
        public int fCost {
            get {
                return gCost + hCost;
            }
        }

        public Node(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        public int CompareTo(Node nodeToCompare)
        {
            int compare = fCost.CompareTo(nodeToCompare.fCost);
            if (compare == 0) {
                compare = hCost.CompareTo(nodeToCompare.hCost);
            }
            return -compare;
        }
        public override string ToString()
        {
            return x.ToString() + " , " + y.ToString();
        }

        public override bool Equals(object obj)
        {
            var node = obj as Node;
            return node != null && node.x == x && node.y == y;
        }

        public override int GetHashCode()
        {
            var hashCode = 1502939027;
            hashCode = hashCode * -1521134295 + x.GetHashCode();
            hashCode = hashCode * -1521134295 + y.GetHashCode();
            return hashCode;
        }
    }
}