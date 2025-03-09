using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : NetworkBehaviour where T : NetworkBehaviour
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<T>();
                if (instance == null)
                {
                    GameObject gameObject = new GameObject();
                    instance = gameObject.AddComponent<T>();
                }
            }
            return instance;
        }
    }
    public void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            DontDestroyOnLoad(this);
        }
        else
        {
            if (instance != this)
                Destroy(this);
        }
    }

    public static bool HasInstance() => instance != null;

}
