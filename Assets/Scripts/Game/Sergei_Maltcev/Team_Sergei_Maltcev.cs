using Game;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Graphs;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Sergei_Maltcev
{
    public class Team_Sergei_Maltcev : Team
    {
        [SerializeField] private Color myFancyColor;
        private List<Battlefield.Node> allCoverNodes = new List<Battlefield.Node>();

        #region Properties

        public override Color Color => myFancyColor;
        public List<Battlefield.Node> CoverNodes => allCoverNodes;

        #endregion


        protected override void Start()
        {
            base.Start();
            allCoverNodes = GetAllCoverNodes();
        }


        public Vector3 GetTeamCenter(Team team)
        {
            Vector3 center = Vector3.zero;
            foreach (var unit in team.Units)
            {
                center += unit.transform.position;
            }

            center /= Units.Count();
            return center;
        }


        public Battlefield.Node GetNodeInShootingRange(Unit targetUnit)
        {
            if (targetUnit != null)
            {
                return GraphUtils.GetClosestNode<Node_Grass>(Battlefield.Instance, targetUnit.transform.position + GetTeamCenter(this).normalized * (Unit.FIRE_RANGE));
            }

            return null;
        }

        public Battlefield.Node ClosestCoverNode(Vector3 startPos, Unit myUnit, Unit targetUnit)
        {
            Battlefield.Node bestCover = null;
            float bestDistance = float.MaxValue;
            bool occupied = false;
            foreach (Battlefield.Node gNode in CoverNodes)
            {
                foreach (Unit teammate in Units)
                {
                    if (teammate == myUnit && teammate.TargetNode == gNode || teammate.CurrentNode == gNode)
                    {
                        occupied = true;
                        break;
                    }
                }

                if (occupied)
                {
                    continue;
                }

                if (targetUnit != null
                    && myUnit != null
                    && Battlefield.Instance.InCover(gNode, targetUnit.transform.position))
                {
                    float distance = Vector3.Distance(startPos, gNode.WorldPosition);
                    if (distance < bestDistance)
                    {
                        bestCover = gNode;
                        bestDistance = distance;
                    }
                }
            }

            return bestCover;
        }
        

        public bool WithinRange(Vector3 startPos, Vector3 enemyPos)
        {
            return Vector3.Distance(startPos, enemyPos) <= (Unit.FIRE_RANGE * 1.1f);
        }


        private List<Battlefield.Node> GetAllCoverNodes()
        {
            var nodes = Battlefield.Instance.Nodes;
            foreach (Battlefield.Node node in nodes)
            {
                if (Battlefield.Instance.HasAnyCoverAt(node))
                {
                    allCoverNodes.Add(node);
                }
            }

            return allCoverNodes;
        }


        public GraphUtils.Path CustomGetShortestPath(Battlefield.Node start, Battlefield.Node goal)
        {
            if (start == null ||
                goal == null ||
                start == goal ||
                Battlefield.Instance == null)
            {
                return null;
            }

            // initialize pathfinding
            foreach (Battlefield.Node node in Battlefield.Instance.Nodes)
            {
                node?.ResetPathfinding();
            }

            // add start node
            start.m_fDistance = 0.0f;
            start.m_fRemainingDistance = Battlefield.Instance.Heuristic(goal, start);
            List<Battlefield.Node> open = new List<Battlefield.Node>();
            HashSet<Battlefield.Node> closed = new HashSet<Battlefield.Node>();
            open.Add(start);

            // search
            while (open.Count > 0)
            {
                // get next node (the one with the least remaining distance)
                Battlefield.Node current = open[0];
                for (int i = 1; i < open.Count; ++i)
                {
                    if (open[i].m_fRemainingDistance < current.m_fRemainingDistance)
                    {
                        current = open[i];
                    }
                }

                open.Remove(current);
                closed.Add(current);

                // found goal?
                if (current == goal)
                {
                    // construct path
                    GraphUtils.Path path = new GraphUtils.Path();
                    while (current != null)
                    {
                        path.Add(current.m_parentLink);
                        current = current != null && current.m_parentLink != null ? current.m_parentLink.Source : null;
                    }

                    path.RemoveAll(l => l == null); // HACK: check if path contains null links
                    path.Reverse();
                    return path;
                }
                else
                {
                    foreach (Battlefield.Link link in current.Links)
                    {
                        if (link.Target is Battlefield.Node target)
                        {
                            if (!closed.Contains(target) &&
                                target.Unit == null)
                            {
                                //added additional cost for mud tiles so the units always can shoot :)
                                float newDistance = current.m_fDistance +
                                                    Vector3.Distance(current.WorldPosition, target.WorldPosition) +
                                                    (target.AdditionalCost);
                                float newRemainingDistance =
                                    newDistance + Battlefield.Instance.Heuristic(target, start);

                                if (open.Contains(target))
                                {
                                    if (newRemainingDistance < target.m_fRemainingDistance)
                                    {
                                        // re-parent neighbor node
                                        target.m_fRemainingDistance = newRemainingDistance;
                                        target.m_fDistance = newDistance;
                                        target.m_parentLink = link;
                                    }
                                }
                                else
                                {
                                    // add target to openlist
                                    target.m_fRemainingDistance = newRemainingDistance;
                                    target.m_fDistance = newDistance;
                                    target.m_parentLink = link;
                                    open.Add(target);
                                }
                            }
                        }
                    }
                }
            }

            // no path found :(
            return null;
        }
    }
}