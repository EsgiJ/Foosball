using Unity.VisualScripting;
using UnityEngine;

public class GameJuiceManager : MonoBehaviour
{
    private static GameJuiceManager instance;

#region Unity Lifecycle

    public static GameJuiceManager Instance
    {
        get
        {
            if(instance == null)
            {
                SetupInstance();
            }
            return instance;
        }
    }

    private static void SetupInstance()
    {
        instance = FindObjectsByType<GameJuiceManager>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)[0];
        if (instance == null)
        {
            GameObject gameObj = new GameObject();
            gameObj.name = "GameJuiceManager";
            instance = gameObj.AddComponent<GameJuiceManager>();
            DontDestroyOnLoad(gameObj);
        }
    }
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {

    }

    void Update()
    {
        
    }
#endregion
}
