using Game;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Graphs;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Sergei_Maltcev
{
    public class Unit_Sergei_Maltcev : Unit
    {
        [SerializeField] private bool ShowLines;
        public new Team_Sergei_Maltcev Team => base.Team as Team_Sergei_Maltcev;
        private LineRenderer _lr;
        private Unit _target;
        
        //Deciding what enemy to attack based on health and Cover
        protected override Unit SelectTarget(List<Unit> enemiesInRange)
        {
            int lowestHealth = 1000;
            
            foreach (Unit enemy in enemiesInRange)
            {
                if (enemy.Health < lowestHealth)
                {
                    if (!Battlefield.Instance.InCover(enemy.CurrentNode, transform.position))
                    {
                        _target = enemy;
                        lowestHealth = enemy.Health;
                    }
                    else
                    {
                        _target = enemy;
                    }
                }
            }

            return _target;
        }

        protected override GraphUtils.Path GetPathToTarget() // Overrides base A star
        {
            return Team.CustomGetShortestPath(this, CurrentNode, TargetNode);
        }

        protected override void Start()
        {
            base.Start();
            _lr = GetComponent<LineRenderer>();
            StartCoroutine(TacticalLogic());
        }

        private void DrawLinePath() // debug tool to see where myUnits are going
        {
            _lr.positionCount = 0;
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

        //main AI loop
        IEnumerator TacticalLogic()
        {
            while (true)
            {
                // If target is withing range and Unit has cover. HOLD!
                if (EnemiesInRange.Any() && Team.CheckCover(CurrentNode, EnemiesInRange))
                {
                    yield return new WaitForSeconds(Random.Range(0.2f, 1f));
                }
                //If You have target but not Cover, look for one. But if the targetNode is in cover, continue moving
                else if (EnemiesInRange.Any())
                {
                    if (!Team.CheckCover(TargetNode, EnemiesInRange))
                    {
                        TargetNode = Team.ClosestCoverNode(CurrentNode, this);
                    }
                    yield return new WaitForSeconds(Random.Range(0.2f, 1f));
                }
                //if you are far away move forward to closest enemy
                else if (!EnemiesInRange.Any())
                {
                    TargetNode = Team.GetNodeInShootingRange(this, ClosestEnemy);
                    yield return new WaitForSeconds(Random.Range(0.2f, 1f));
                }
                else
                {
                    yield return new WaitForSeconds(0.1f);
                }
                if (ShowLines)
                {
                    DrawLinePath();
                }
            }
        }
    }
}