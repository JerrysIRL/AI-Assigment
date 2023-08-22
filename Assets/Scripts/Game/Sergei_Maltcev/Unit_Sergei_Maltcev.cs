using Game;
using System.Collections;
using System.Collections.Generic;
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

        protected override void Start()
        {
            base.Start();

            StartCoroutine(StupidLogic());
        }

        IEnumerator StupidLogic()
        {
            while (true)
            {
                // wait (or take cover)
                TargetNode = _team.GetTargetUnit()?.Node;
                yield return new WaitForSeconds(Random.Range(1.0f, 2.0f));

                // // move randomly
                // TargetNode = Battlefield.Instance.GetRandomNode();
                // yield return new WaitForSeconds(Random.Range(4.0f, 6.0f));
            }
        }
        /*protected override Unit SelectTarget(List<Unit> enemiesInRange)
        {
            var t = Team as Team_Sergei_Maltcev;
            Debug.Log(t);
            Debug.Log(t.GetTargetUnit());
            return t.GetTargetUnit(); //enemiesInRange[Random.Range(0, enemiesInRange.Count)];
        }



        protected override void OnEnable()
        {
            base.OnEnable();
            var t = Team as Team_Sergei_Maltcev;
            Debug.Log(t);
        }

        protected override void Start()
        {
            base.Start();



        }
    }*/
    }
}