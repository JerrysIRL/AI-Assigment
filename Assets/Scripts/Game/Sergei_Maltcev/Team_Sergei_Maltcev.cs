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

        private Unit _targetUnit;

        #region Properties

        public override Color Color => myFancyColor;

        #endregion


        protected override void Start()
        {
            base.Start();
            StartCoroutine(SetTargetUnit());
            ClosestCoverNode(GetTargetUnit().transform.position);
        }

        private IEnumerator SetTargetUnit()
        {
            Vector3 center = GetSquadCenter();

            float fBestDistance = float.MaxValue;

            foreach (Unit enemy in EnemyTeam.Units)
            {
                float fDistance = Vector3.Distance(enemy.transform.position, center);
                if (fDistance < fBestDistance)
                {
                    fBestDistance = fDistance;
                    _targetUnit = enemy;
                }
            }


            yield return new WaitForSeconds(1f);
            yield return SetTargetUnit();
        }

        private Vector3 GetSquadCenter()
        {
            Vector3 center = Vector3.zero;
            foreach (var unit in Units)
            {
                center += unit.transform.position;
            }

            center /= Units.Count();
            return center;
        }


        public Unit GetTargetUnit()
        {
            return _targetUnit;
        }


        public Battlefield.Node GetNodeInShootingRange(Unit targetUnit)
        {
            if (targetUnit != null)
            {
                Vector3 dir = (targetUnit.transform.position - transform.position).normalized;
                return GraphUtils.GetClosestNode<Node_Grass>(Battlefield.Instance, targetUnit.transform.position + transform.position * (Unit.FIRE_RANGE * 0.9f));
                //targetUnit.transform.position + transform.position.normalized * (Unit.FIRE_RANGE * 0.8f));
            }

            return null;
        }

        public Battlefield.Node ClosestCoverNode(Vector3 startPos)
        {
            var nodes = Battlefield.Instance.Nodes;
            Node_Grass bestCover = null;
            float bestDistance = float.MaxValue;
            bool occupied = false;
            foreach (Node_Grass gNode in nodes)
            {
                // foreach (Unit teammate in Units)
                // {
                //     if (teammate.CurrentNode == gNode)
                //     {
                //         occupied = true;
                //         break;
                //     }
                // }

                if (gNode is Node_Mud || occupied)
                {
                    continue;
                }

                if (Battlefield.Instance.InCover(gNode, GetTargetUnit().CurrentNode))
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
                                float newDistance = current.m_fDistance + Vector3.Distance(current.WorldPosition, target.WorldPosition) + (target.AdditionalCost * 5);
                                float newRemainingDistance = newDistance + Battlefield.Instance.Heuristic(target, start);

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