using UnityEngine;
using System.Collections;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    //public const string GameObjectName = "MANAGER";
    
    private static T _instance = null;
    private static bool bLoadedBefore = false;
    public static T a
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType(typeof(T)) as T;
                //Object not found, we create a temporary one.
                if (_instance == null)
                {
                    string name = typeof(T).Name;
                    var obj = GameObject.Find(name);

                    if (obj == null)
                    {
                        obj = new GameObject(name);
                    }
                    
                    _instance = obj.AddComponent<T>();
                }
            }
            return _instance;
        }
    }

	private void Awake() {
        if (bLoadedBefore)
        {
            Destroy(this);
            return;
        }

        bLoadedBefore = true;
		_instance = this as T;
        DontDestroyOnLoad(this.gameObject);
        Initialize();
    }

    protected virtual void Initialize()
    {}

    private void OnDestroy()
    {
        if (_instance != null)
        {
            _instance = null;
            bLoadedBefore = false;
        }
    }

}

