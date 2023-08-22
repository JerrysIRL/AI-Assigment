using Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FirstName_LastName
{
    public class Unit_FirstName_LastName : Unit
    {
        protected override Unit SelectTarget(List<Unit> enemiesInRange)
        {
            return enemiesInRange[Random.Range(0, enemiesInRange.Count)];
        }
    }
}