using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ZFrame.Pathfinding
{
    public class JumpPointSearch : Pathfinder
    {
        public JumpPointSearch(IMapGrid map) : base(map)
        {
            jumpNodes = new List<Node>();
            searchLines = new List<Vector2Int>();
            m_Successors = new List<Node>();
        }

        public List<Node> jumpNodes;
        public List<Vector2Int> searchLines;

        private List<Node> m_Successors;

        private bool _forced;

        #region MAP API        
        private List<Node> GetNeighbours(Node currentNode)
        {
            m_Neighbours.Clear();
            var neighbours = m_Neighbours;

            Node parentNode = currentNode.parent;

            if (parentNode == null) {
                AddAllNeighgours(neighbours, currentNode);
            } else {
                int xDirection = Mathf.Clamp(currentNode.x - parentNode.x, -1, 1);
                int yDirection = Mathf.Clamp(currentNode.y - parentNode.y, -1, 1);

                if (xDirection != 0 && yDirection != 0) {
                    bool neighbourUp = m_Map.IsWalkable(currentNode.x, currentNode.y + yDirection);
                    bool neighbourRight = m_Map.IsWalkable(currentNode.x + xDirection, currentNode.y);
                    bool neighbourLeft = m_Map.IsWalkable(currentNode.x - xDirection, currentNode.y);
                    bool neighbourDown = m_Map.IsWalkable(currentNode.x, currentNode.y - yDirection);

                    if (neighbourUp)
                        neighbours.Add(GetNode(currentNode.x, currentNode.y + yDirection));

                    if (neighbourRight)
                        neighbours.Add(GetNode(currentNode.x + xDirection, currentNode.y));

                    if (neighbourUp || neighbourRight)
                        if (m_Map.IsWalkable(currentNode.x + xDirection, currentNode.y + yDirection))
                            neighbours.Add(GetNode(currentNode.x + xDirection, currentNode.y + yDirection));

                    if (!neighbourLeft && neighbourUp)
                        TryAddNeighbour(neighbours, currentNode.x - xDirection, currentNode.y + yDirection);

                    if (!neighbourDown && neighbourRight)
                        TryAddNeighbour(neighbours, currentNode.x + xDirection, currentNode.y - yDirection);
                } else {
                    if (xDirection == 0) {
                        if (m_Map.IsWalkable(currentNode.x, currentNode.y + yDirection)) {
                            neighbours.Add(GetNode(currentNode.x, currentNode.y + yDirection));

                            if (!m_Map.IsWalkable(currentNode.x + 1, currentNode.y))
                                TryAddNeighbour(neighbours, currentNode.x + 1, currentNode.y + yDirection);

                            if (!m_Map.IsWalkable(currentNode.x - 1, currentNode.y))
                                TryAddNeighbour(neighbours, currentNode.x - 1, currentNode.y + yDirection);
                        }
                    } else {
                        if (m_Map.IsWalkable(currentNode.x + xDirection, currentNode.y)) {
                            neighbours.Add(GetNode(currentNode.x + xDirection, currentNode.y));
                            if (!m_Map.IsWalkable(currentNode.x, currentNode.y + 1))
                                neighbours.Add(GetNode(currentNode.x + xDirection, currentNode.y + 1));
                            if (!m_Map.IsWalkable(currentNode.x, currentNode.y - 1))
                                neighbours.Add(GetNode(currentNode.x + xDirection, currentNode.y - 1));
                        }
                    }
                }
            }
            return neighbours;
        }

        #endregion
        protected override bool _CalculateShortestPath()
        {
            jumpNodes.Clear();
            searchLines.Clear();

            Node currentNode;

            openSet.Add(_startNode);
            openSetContainer.Add(_startNode);

            while (openSet.Count > 0) {
                currentNode = openSet.RemoveFirst();
                openSetContainer.Remove(_startNode);

                if (m_Map.IsDestination(currentNode.x, currentNode.y, _targetNode.x, _targetNode.y)) {
                    _targetNode = currentNode;
                    return true;
                } else {
                    closedSet.Add(currentNode);
                    List<Node> Nodes = _GetSuccessors(currentNode);

                    foreach (Node node in Nodes) {
                        jumpNodes.Add(node);

                        if (closedSet.Contains(node))
                            continue;

                        int newGCost = currentNode.gCost + _GetDistance(currentNode, node);
                        if (newGCost < node.gCost || !openSetContainer.Contains(node)) {
                            node.gCost = newGCost;
                            node.hCost = _GetDistance(node, _targetNode);
                            node.parent = currentNode;

                            if (!openSetContainer.Contains(node)) {
                                openSetContainer.Add(node);
                                openSet.Add(node);
                            } else {
                                openSet.UpdateItem(node);
                            }
                        }
                    }
                }
            }
            return false;
        }
        private List<Node> _GetSuccessors(Node currentNode)
        {
            Node jumpNode;

            m_Successors.Clear();
            List<Node> successors = m_Successors;
            List<Node> neighbours = GetNeighbours(currentNode);

            foreach (Node neighbour in neighbours) {
                int xDirection = neighbour.x - currentNode.x;
                int yDirection = neighbour.y - currentNode.y;

                jumpNode = _Jump(neighbour, currentNode, xDirection, yDirection);

                if (jumpNode != null)
                    successors.Add(jumpNode);
            }
            return successors;
        }
        private Node _Jump(Node currentNode, Node parentNode, int xDirection, int yDirection)
        {
            if (currentNode == null || !m_Map.IsWalkable(currentNode.x, currentNode.y))
                return null;
            if (m_Map.IsDestination(currentNode.x, currentNode.y, _targetNode.x, _targetNode.y)) {
                _forced = true;
                return currentNode;
            }

            _forced = false;
            if (xDirection != 0 && yDirection != 0) {
                if ((!m_Map.IsWalkable(currentNode.x - xDirection, currentNode.y) && m_Map.IsWalkable(currentNode.x - xDirection, currentNode.y + yDirection)) ||
                    (!m_Map.IsWalkable(currentNode.x, currentNode.y - yDirection) && m_Map.IsWalkable(currentNode.x + xDirection, currentNode.y - yDirection))) {
                    return currentNode;
                }

                Node nextHorizontalNode = GetNodeFromIndex(currentNode.x + xDirection, currentNode.y);
                Node nextVerticalNode = GetNodeFromIndex(currentNode.x, currentNode.y + yDirection);
                if (nextHorizontalNode == null && nextVerticalNode == null) return null;

                if (_Jump(nextHorizontalNode, currentNode, xDirection, 0) != null || _Jump(nextVerticalNode, currentNode, 0, yDirection) != null) {
                    if (!_forced) {
                        Debug.Log(currentNode);
                        Node temp = GetNodeFromIndex(currentNode.x + xDirection, currentNode.y + yDirection);
                        if (temp != null && showDebug) {
                            //Debug.DrawLine(new Vector3(currentNode.x, 1, currentNode.y), new Vector3(temp.x, 1, temp.y), Color.green, Mathf.Infinity);
                            searchLines.Add(new Vector2Int(currentNode.x, currentNode.y));
                            searchLines.Add(new Vector2Int(temp.x, temp.y));
                        }
                        return _Jump(temp, currentNode, xDirection, yDirection);
                    } else {
                        return currentNode;
                    }
                }
            } else {
                if (xDirection != 0) {
                    if ((m_Map.IsWalkable(currentNode.x + xDirection, currentNode.y + 1) && !m_Map.IsWalkable(currentNode.x, currentNode.y + 1)) ||
                        (m_Map.IsWalkable(currentNode.x + xDirection, currentNode.y - 1) && !m_Map.IsWalkable(currentNode.x, currentNode.y - 1))) {
                        _forced = true;
                        return currentNode;
                    }
                } else {
                    if ((m_Map.IsWalkable(currentNode.x + 1, currentNode.y + yDirection) && !m_Map.IsWalkable(currentNode.x + 1, currentNode.y)) ||
                        (m_Map.IsWalkable(currentNode.x - 1, currentNode.y + yDirection) && !m_Map.IsWalkable(currentNode.x - 1, currentNode.y))) {
                        _forced = true;
                        return currentNode;
                    }
                }
            }
            Node nextNode = GetNodeFromIndex(currentNode.x + xDirection, currentNode.y + yDirection);
            if (nextNode != null && showDebug) {
                //Debug.DrawLine(new Vector3(currentNode.x, 1, currentNode.y), new Vector3(nextNode.x, 1, nextNode.y), Color.green, Mathf.Infinity);
                searchLines.Add(new Vector2Int(currentNode.x, currentNode.y));
                searchLines.Add(new Vector2Int(nextNode.x, nextNode.y));
            }
            return _Jump(nextNode, currentNode, xDirection, yDirection);
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
