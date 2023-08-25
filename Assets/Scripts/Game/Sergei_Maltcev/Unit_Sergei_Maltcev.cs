using Game;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Graphs;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Sergei_Maltcev
{
    public class Unit_Sergei_Maltcev : Unit
    {
        private LineRenderer _lr;
        public new Team_Sergei_Maltcev Team => base.Team as Team_Sergei_Maltcev;

        protected override Unit SelectTarget(List<Unit> enemiesInRange)
        {
            return Team.GetTargetUnit(); //enemiesInRange[Random.Range(0, enemiesInRange.Count)];
        }
        
        protected override GraphUtils.Path GetPathToTarget()
        {
            return Team.CustomGetShortestPath(CurrentNode, TargetNode);
        }

        protected override void Start()
        {
            base.Start();
            _lr = GetComponent<LineRenderer>();
            StartCoroutine(TacticalLogic());
        }

        private void DrawLinePath()
        {
            var path = GetPathToTarget();
            if (path != null)
            {
                _lr.positionCount = path.Count;
                for (int i = 0; i < path.Count; i++)
                {
                    var a = path[i].Target as Battlefield.Node;
                    _lr.SetPosition(i, a.WorldPosition);
                }
            }
        }
        

        IEnumerator TacticalLogic()
        {
            while (true)
            {
                if (Team.GetTargetUnit() != null
                    && Battlefield.Instance.InCover(CurrentNode, Team.GetTargetUnit().transform.position)
                    && Team.CanShoot(transform.position,Team.GetTargetUnit().transform.position))
                {
                    Debug.Log("InCover");
                    yield return new WaitForSeconds(1f);
                }
                // wait (or take cover)
                else if (Team.GetTargetUnit() != null && !Team.CanShoot(transform.position,Team.GetTargetUnit().transform.position))
                {
                    Debug.Log("Moving");
                    TargetNode = Team.GetNodeInShootingRange(Team.GetTargetUnit());
                    DrawLinePath();
                    yield return new WaitForSeconds(0.5f);
                }
                else
                {
                    Debug.Log("SearchingForCover");
                    TargetNode = Team.ClosestCoverNode(transform.position, this);
                    DrawLinePath();
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }
    }
}