using System;
using Game;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Sergei_Maltcev
{
    public class Team_Sergei_Maltcev : Team
    {
        [SerializeField] private Color myFancyColor;

        private Unit _teamTarget;

        #region Properties

        public override Color Color => myFancyColor;

        #endregion


        protected override void Start()
        {
            base.Start();
            StartCoroutine(SetTargetUnit());
            
        }

        private IEnumerator SetTargetUnit()
        {
            Debug.Log("Updated target");
            
                Vector3 center = Vector3.zero;
                foreach (var unit in Units)
                {
                    center += unit.transform.position;
                }

                center /= Units.Count();
                
                float fBestDistance = float.MaxValue;
                Unit closestEnemy = null;
                foreach (Unit enemy in EnemyTeam.Units)
                {
                    float fDistance = Vector3.Distance(enemy.transform.position, center);
                    if (fDistance < fBestDistance)
                    {
                        fBestDistance = fDistance;
                        closestEnemy = enemy; 
                    }
                }

                _teamTarget = closestEnemy;
            

            Debug.Log(_teamTarget);
            yield return new WaitForSeconds(2f);
            yield return SetTargetUnit();
        }


        public Unit GetTargetUnit()
        {
            return _teamTarget;
        }
    }
}