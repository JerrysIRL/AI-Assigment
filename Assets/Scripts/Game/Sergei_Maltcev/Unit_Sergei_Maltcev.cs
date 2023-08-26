using Game;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Graphs;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Sergei_Maltcev
{
    public class Unit_Sergei_Maltcev : Unit
    {
        private LineRenderer _lr;
        public new Team_Sergei_Maltcev Team => base.Team as Team_Sergei_Maltcev;
        private Unit _target;

        protected override Unit SelectTarget(List<Unit> enemiesInRange)
        {
            Unit targetUnit = null;
            int lowestHealth = 1000;
            
            foreach (Unit enemy in enemiesInRange)
            {
                if (enemy.Health < lowestHealth)
                {
                    if (!Battlefield.Instance.InCover(enemy.CurrentNode, transform.position))
                    {
                        targetUnit = enemy;
                        lowestHealth = enemy.Health;
                    }
                    else
                    {
                        targetUnit = enemy;
                        lowestHealth = enemy.Health;
                    }
                }
            }

            return _target = targetUnit;
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
                if (_target != null
                    && Battlefield.Instance.InCover(CurrentNode, _target.transform.position))
                {
                    Debug.Log("InCover");
                    yield return new WaitForSeconds(1.5f);
                }
                // wait (or take cover)
                else if (_target != null)
                {
                    Debug.Log("SearchingForCover");
                    if (!Battlefield.Instance.InCover(TargetNode, _target.transform.position))
                    {
                       TargetNode = Team.ClosestCoverNode(transform.position, this, _target);
                        DrawLinePath(); 
                    }
                    yield return new WaitForSeconds(1f);
                }
                else
                {
                    Debug.Log("Moving");
                    TargetNode = Team.GetNodeInShootingRange(ClosestEnemy); //Team.GetNodeInShootingRange(ClosestEnemy);
                    DrawLinePath();
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }
    }
}