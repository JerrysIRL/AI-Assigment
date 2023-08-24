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
        private Team_Sergei_Maltcev _team;
        private LineRenderer _lr;

        protected override Unit SelectTarget(List<Unit> enemiesInRange)
        {
            return _team.GetTargetUnit(); //enemiesInRange[Random.Range(0, enemiesInRange.Count)];
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _team = Team as Team_Sergei_Maltcev;
        }

        protected override GraphUtils.Path GetPathToTarget()
        {
            return _team.CustomGetShortestPath(CurrentNode, TargetNode);
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

        private bool CanShoot(Vector3 enemyPos)
        {
            return Vector3.Distance(transform.position, enemyPos) <= (Unit.FIRE_RANGE * 0.95f);
        }

        IEnumerator TacticalLogic()
        {
            while (true)
            {
                if (_team.GetTargetUnit() != null
                    && Battlefield.Instance.InCover(CurrentNode, _team.GetTargetUnit().transform.position)
                    && CanShoot(_team.GetTargetUnit().transform.position))
                {
                    Debug.Log("InCover");
                    yield return new WaitForSeconds(2f);
                }
                // wait (or take cover)
                else if (_team.GetTargetUnit() != null && !CanShoot(_team.GetTargetUnit().transform.position))
                {
                    TargetNode = _team.GetNodeInShootingRange(_team.GetTargetUnit());
                    DrawLinePath();
                    yield return new WaitForSeconds(0.5f);
                }
                else
                {
                    Debug.Log("SearchingForCover");
                    TargetNode = _team.ClosestCoverNode(transform.position);
                    DrawLinePath();
                    yield return new WaitForSeconds(1f);
                }
            }
        }
    }
}