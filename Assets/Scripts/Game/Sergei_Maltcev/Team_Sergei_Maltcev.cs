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
        private HashSet<Battlefield.Node> _allCoverNodes = new HashSet<Battlefield.Node>();

        #region Properties

        public override Color Color => myFancyColor;
        public HashSet<Battlefield.Node> CoverNodes => _allCoverNodes;

        #endregion


        protected override void Start()
        {
            base.Start();
            _allCoverNodes = GetAllCoverNodes();
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

        // returns a node within fire range in direction of targetUnit
        public Battlefield.Node GetNodeInShootingRange(Unit myUnit, Unit targetUnit)
        {
            if (targetUnit != null)
            {
                Vector3 dir = (targetUnit.transform.position - myUnit.transform.position);
                float distance = dir.magnitude - Unit.FIRE_RANGE * 0.9f;
                dir.Normalize();
                return GraphUtils.GetClosestNode<Node_Grass>(Battlefield.Instance,
                    myUnit.transform.position + (dir * distance));
            }

            return null;
        }

        // Function to get closest cover to Units position
        public Battlefield.Node ClosestCoverNode(Unit unit)
        {
            Battlefield.Node bestCover = null;
            float bestDistance = float.MaxValue;
            bool occupied = false;
            foreach (Battlefield.Node cover in CoverNodes)
            {
                // Jump over tiles where teammates are and yourself
                foreach (Unit teammate in Units)
                {
                    occupied = false;
                    if (teammate == unit)
                    {
                        continue;
                    }
                    if (cover == teammate.TargetNode)
                    {
                        occupied = true;
                        break;
                    }
                }

                if (occupied)
                {
                    continue;
                }

                //find closest one
                if (CheckCover(cover, unit.EnemiesInRange))
                {
                    float distance = Vector3.Distance(unit.transform.position, cover.WorldPosition);
                    if (distance < bestDistance)
                    {
                        bestCover = cover;
                        bestDistance = distance;
                    }
                }
            }

            return bestCover;
        }

        // checks for cover from all the enemy positions
        public bool CheckCover(Battlefield.Node gNode, IEnumerable<Unit> units)
        {
            foreach (var enemyUnit in units)
            {
                if (gNode == enemyUnit.TargetNode || !Battlefield.Instance.InCover(gNode, enemyUnit.transform.position))
                {
                    return false;
                }
            }

            return true;
        }


        private HashSet<Battlefield.Node> GetAllCoverNodes() // Caching all covernodes at start
        {
            var nodes = Battlefield.Instance.Nodes;
            foreach (Battlefield.Node node in nodes)
            {
                if (Battlefield.Instance.HasAnyCoverAt(node))
                {
                    _allCoverNodes.Add(node);
                }
            }

            return _allCoverNodes;
        }


        public GraphUtils.Path CustomGetShortestPath(Unit unit, Battlefield.Node start, Battlefield.Node goal)
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
                                                    (target.AdditionalCost * 2);
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