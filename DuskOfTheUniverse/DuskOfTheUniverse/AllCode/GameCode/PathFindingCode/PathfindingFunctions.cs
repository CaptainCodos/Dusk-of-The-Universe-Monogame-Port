using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{

    // This type of search utilizes Greedy Best-first searching. Similar to A* however, it is a bit simpler and does not necessarily require backtracking.
    // Note it will not always find the absolute best path as A* does, however will be faster a looking for a good path.
    class GBFsearch
    {
        // This represents the graph to be used for finding the path.
        private NodeGraph m_graph;

        // This queue is used to evaluate nodes.
        private Queue<PNode> m_openQueue;

        // Nodes to be used for the current node, start and target nodes.
        private PNode m_currNode, m_startNode, m_targetNode;

        // temporary node to help create the path.
        private PNode m_tempNode;

        // This queue is used to create a queue of tiles for the actor.
        private Queue<PNode> m_pathQueue;
        // This is the queue of tiles to be given to the actor to follow.
        private Queue<MapTile> m_path;

        // This is the map the actor exists on.
        private Map m_map;

        // Class Constructor
        public GBFsearch(NodeGraph graph, Map map)
        {
            m_openQueue = new Queue<PNode>();
            m_pathQueue = new Queue<PNode>();
            m_path = new Queue<MapTile>();

            m_graph = graph;
            m_map = map;
        }

        // This method searches the grid.
        public void Search(int x, int y, int targetX, int targetY)
        {
            m_openQueue = new Queue<PNode>();
            m_pathQueue = new Queue<PNode>();
            m_path = new Queue<MapTile>();

            // Setup the nodes.
            m_targetNode = m_graph.PNodes[targetX, targetY];
            m_startNode = m_graph.PNodes[x, y];
            m_tempNode = m_startNode;

            // Set or Reset the visited values of each node.
            ResetNodes(targetX, targetY);

            // Add the target node to the open queue.
            // Note that the parents must point towards the target, so the parents will be created from the target outward.
            m_openQueue.Enqueue(m_targetNode);

            while (m_openQueue.Count > 0)
            {
                // set the current node to the next node in the open queue and st it to visited.
                m_currNode = m_openQueue.Dequeue();
                m_currNode.Visited = true;

                // If the current node is the starting node, then break. (prevent unnecessary nodes being evaluated)
                if (m_currNode == m_startNode)
                {
                    break;
                }
                else // Otherwise continue evaluating nodes around the current node.
                {
                    EvaluateNeighbours(m_currNode, x, y);
                }
            }
        }

        // Set all nodes to be not visited.
        private void ResetNodes(int targetX, int targetY)
        {
            foreach (PNode n in m_graph.PNodes)
            {
                n.Visited = false;
            }
        }

        // Evaluate nodes around the current node.
        private void EvaluateNeighbours(PNode currentNode, int sX, int sY)
        {
            for (int y = -1; y < 2; y++)
            {
                for (int x = -1; x < 2; x++)
                {
                    bool isOnGrid;

                    // Values for checking the evaluating node's coordinates.
                    int checkX = currentNode.Coords.X + x;
                    int checkY = currentNode.Coords.Y + y;

                    // Check if the node is on the node grid.
                    if ((checkX >= 0 && checkX < m_graph.PNodes.GetLength(0)) && (checkY >= 0 && checkY < m_graph.PNodes.GetLength(1)))
                        isOnGrid = true;
                    else
                        isOnGrid = false;

                    if (isOnGrid // If the node is on grid.
                        && !(y == x || y == -x) // No diagonals allowed.
                        && !(y == 0 && x == 0) // Don't consider the current node.
                        && m_graph.PNodes[checkX, checkY].IsWalkable // The node must be walkable.
                        && !m_openQueue.Contains(m_graph.PNodes[checkX, checkY]) // The node is not in the open queue.
                        && !m_graph.PNodes[checkX, checkY].Visited) // The node is not visited yet.
                    {
                        // Set the node's parent and add it to the open queue.
                        m_graph.PNodes[checkX, checkY].Parent = currentNode;
                        m_openQueue.Enqueue(m_graph.PNodes[checkX, checkY]);
                    }
                }
            }
        }

        // This method follows the parents from the start node to the target, adding the tiles to the path queue and returns this path.
        public Queue<MapTile> GetPath()
        {
            // Follow the parents from the start till the target is reached, then return the path.
            while (m_tempNode != m_targetNode)
            {
                m_tempNode = m_tempNode.Parent;

                // This prevents adding a null node to the path (prevents crashes)
                if (m_tempNode == null)
                {
                    return m_path;
                }

                m_path.Enqueue(m_map.Tiles[m_tempNode.Coords.X, m_tempNode.Coords.Y]);
            }

            return m_path;
        }
    }
}
