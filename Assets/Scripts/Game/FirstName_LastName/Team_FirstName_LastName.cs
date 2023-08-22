using Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FirstName_LastName
{
    public class Team_FirstName_LastName : Team
    {
        [SerializeField]
        private Color   m_myFancyColor;

        #region Properties

        public override Color Color => m_myFancyColor;

        #endregion
    }
}