using UnityEngine;
using System.Collections;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static bool isApplicationQuitting = false;

    protected static T instance;

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void OnApplicationQuit()
    {
        isApplicationQuitting = true;
    }

    /**
       Returns the instance of this singleton.
    */
    public static T Instance
    {
        get
        {
            if (instance == null && isApplicationQuitting == false)
            {
                instance = FindFirstObjectByType<T>();
                if (instance == null)
                {
					GameObject go = new GameObject(typeof(T).Name, typeof(T));
					instance = go.GetComponent<T>();
                }
            }

            return instance;
        }
    }
}
