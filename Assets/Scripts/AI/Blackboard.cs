using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public class Blackboard
    {
        private Dictionary<string, object> m_data = new Dictionary<string, object>();

        #region Properties

        public IEnumerable<KeyValuePair<string, object>> Items => m_data;

        public int Count => m_data.Count;

        #endregion

        public T GetValue<T>(string key, T defaultValue)
        {
            object value;
            if (m_data.TryGetValue(key, out value))
            {
                if(value is T returnValue)
                {
                    return returnValue;
                }
            }

            return defaultValue;
        }

        public void SetValue<T>(string key, T value)
        {
            m_data[key] = value;
        }
    }
}
