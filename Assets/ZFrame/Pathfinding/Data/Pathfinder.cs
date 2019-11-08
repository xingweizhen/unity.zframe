using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;

namespace ZFrame.Pathfinding
{   public abstract class Pathfinder 
    {
        public bool showDebug;

        protected Node _startNode;
        protected Node _targetNode;

        protected Heap<Node> openSet;
        protected HashSet<Node> openSetContainer;
        protected HashSet<Node> closedSet;
        protected List<Node> m_Path, m_Neighbours;

        protected IMapGrid m_Map;
        protected Node[,] m_Nodes;
        protected int m_NodeVer;

        public Pathfinder(IMapGrid map)
        {
            m_Map = map;
            openSet = new Heap<Node>(m_Map.Width * m_Map.Length);
            openSetContainer = new HashSet<Node>();
            closedSet = new HashSet<Node>();
            m_Nodes = new Node[m_Map.Width, m_Map.Length];

            m_Path = new List<Node>();
            m_Neighbours = new List<Node>();
        }

        protected Node GetNode(int x, int y)
        {
            var node = m_Nodes[x, y];
            if (node == null) {
                node = new Node(x, y) { ver = m_NodeVer };
                m_Nodes[x, y] = node;
            } else if (node.ver != m_NodeVer) {
                node.ver = m_NodeVer;
                node.parent = null;
                node.gCost = 0;
                node.hCost = 0;
                node.HeapIndex = 0;
            }
            return node;
        }

        protected bool IsBlockedNode(int x, int y)
        {
            if (x < 0 || x >= m_Map.Width) return false;
            if (y < 0 || y >= m_Map.Length) return false;

            return !m_Map.IsWalkable(x, y);
        }

        protected Node GetNodeFromIndex(int x, int y)
        {
            if (!m_Map.IsWalkable(x, y))
                return null;
            return GetNode(x, y);
        }

        protected void TryAddNeighbour(List<Node> neighbours, int x, int y)
        {
            if (m_Map.IsWalkable(x, y)) {
                neighbours.Add(GetNode(x, y));
            }
        }

        protected void TryAddNeighbour(List<Node> neighbours, Node currentNode, int x, int y)
        {
            var nx = currentNode.x + x;
            var ny = currentNode.y + y;
            if (x * y == 0) {
                TryAddNeighbour(neighbours, nx, ny);
            } else {
                // 斜对角的判断
                if (m_Map.IsWalkable(nx, ny) && (
                    m_Map.IsWalkable(currentNode.x, ny) || m_Map.IsWalkable(nx, currentNode.y))) {
                    neighbours.Add(GetNode(nx, ny));
                }
            }
        }

        protected void AddAllNeighgours(List<Node> neighbours, Node currentNode)
        {
            TryAddNeighbour(neighbours, currentNode, -1, -1);
            TryAddNeighbour(neighbours, currentNode, -1, 0);
            TryAddNeighbour(neighbours, currentNode, -1, 1);
            TryAddNeighbour(neighbours, currentNode, 0, -1);
            TryAddNeighbour(neighbours, currentNode, 0, 1);
            TryAddNeighbour(neighbours, currentNode, 1, -1);
            TryAddNeighbour(neighbours, currentNode, 1, 0);
            TryAddNeighbour(neighbours, currentNode, 1, 1);
        }

        protected abstract bool _CalculateShortestPath();

        protected List<Node> _RetracePath()
        {
            m_Path.Clear();
            Node currentNode = _targetNode;

            while (!currentNode.Equals(_startNode)) {
                m_Path.Add(currentNode);
                currentNode = currentNode.parent;
            }
            m_Path.Reverse();
            return m_Path;
        }
        public List<Node> GetPath(int sx, int sy, int ex, int ey)
        {
            m_NodeVer += 1;

            openSet.Count = 0;
            openSetContainer.Clear();
            closedSet.Clear();

            _startNode = GetNode(sx, sy);
            _targetNode = GetNode(ex, ey);

            var time = Time.realtimeSinceStartup;
            UnityEngine.Profiling.Profiler.BeginSample("CalculateShortestPath");
            var found = _CalculateShortestPath();
            UnityEngine.Profiling.Profiler.EndSample();

            var passTime = Time.realtimeSinceStartup - time;
            if (found) {
                UnityEngine.Debug.LogFormat("Jump Point Path - Path found in : {0:f2} ms. [{1}]->[{2}]", passTime * 1000, _startNode, _targetNode);
                //EventHandler.Instance.Broadcast(new PathTimerEvent(ts.Milliseconds));
                return _RetracePath();
            } else {
                UnityEngine.Debug.LogFormat("Jump Point Path - No path found in : {0:f2} ms. [{1}]->[{2}]", passTime * 1000, _startNode, _targetNode);
                //EventHandler.Instance.Broadcast(new PathTimerEvent(ts.Milliseconds));
                return null;
            }
        }
    }
}
