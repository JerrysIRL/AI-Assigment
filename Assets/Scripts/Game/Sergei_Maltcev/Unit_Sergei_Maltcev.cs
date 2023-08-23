using Game;
using System.Collections;
using System.Collections.Generic;
using Graphs;
using UnityEngine;

namespace Sergei_Maltcev
{
    public class Unit_Sergei_Maltcev : Unit
    {
        private Team_Sergei_Maltcev _team;
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

            StartCoroutine(TacticalLogic());
        }

        IEnumerator TacticalLogic()
        {
            while (true)
            {
                // wait (or take cover)
                //TargetNode = _team.GetTargetUnit()?.CurrentNode;
                Battlefield.Node rangeTargetNode = _team.FindCoverNodeCloseToTarget(_team.GetTargetUnit());
                if(rangeTargetNode != null)
                {
                    TargetNode = _team.FindCoverNodeCloseToTarget(_team.GetTargetUnit());
                    yield return new WaitForSeconds(Random.Range(1.0f, 2.0f));
                }

                yield return new WaitForSeconds(1f);
                
            }
        }
       
    }
}