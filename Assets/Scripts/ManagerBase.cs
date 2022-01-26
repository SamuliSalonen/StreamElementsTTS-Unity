using UnityEngine;

namespace Settings
{
    public class ManagerBase<T> : MonoBehaviour where T : Component
    {
        static T _instance = null;

        public static T Instance
        {
            get
            {
                if (_instance == null) _instance = FindObjectOfType<T>();

                return _instance;
            }
        }
    }
}