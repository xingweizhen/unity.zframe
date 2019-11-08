using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Diagnostics;

namespace ZFrame.Pathfinding
{
    public class AStar : Pathfinder
    {
        public AStar(IMapGrid map) : base(map)
        {
            closedList = new List<Node>();
        }

        public List<Node> closedList;
        private List<Node> GetNeighbours(Node currentNode)
        {
            m_Neighbours.Clear();
            var neighbours = m_Neighbours;
            AddAllNeighgours(neighbours, currentNode);
            return neighbours;
        }
        protected override bool _CalculateShortestPath()
        {
            closedList.Clear();

            Node currentNode;

            openSet.Add(_startNode);
            openSetContainer.Add(_startNode);

            while (openSet.Count > 0) {
                currentNode = openSet.RemoveFirst();
                openSetContainer.Remove(currentNode);
                closedSet.Add(currentNode);
                closedList.Add(currentNode);

                if (currentNode.Equals(_targetNode))
                    return true;

                foreach (Node neighbour in GetNeighbours(currentNode)) {
                    if (!m_Map.IsWalkable(neighbour.x, neighbour.y) || closedSet.Contains(neighbour))
                        continue;

                    int newMovementCostToNeighbour = currentNode.gCost + _GetDistance(currentNode, neighbour);
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSetContainer.Contains(neighbour)) {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = _GetDistance(neighbour, _targetNode);
                        neighbour.parent = currentNode;

                        if (!openSetContainer.Contains(neighbour)) {
                            openSet.Add(neighbour);
                            openSetContainer.Add(neighbour);
                        } else
                            openSet.UpdateItem(neighbour);
                    }
                }
            }
            return false;
        }
        private int _GetDistance(Node a, Node b)
        {
            int distX = Mathf.Abs(a.x - b.x);
            int distY = Mathf.Abs(a.y - b.y);

            if (distX > distY)
                return 14 * distY + 10 * (distX - distY);

            return 14 * distX + 10 * (distY - distX);
        }
    }
}
