using UnityEngine;

public class FootballPlayerController : MonoBehaviour
{
#region Unity Lifecycle 
    void Start()
    {
        RodController.OnShootEvent += HandleShoot;
    }

    void Update()
    {
        
    }

    void OnDestroy()
    {
        RodController.OnShootEvent -= HandleShoot;
    }
#endregion

#region Collider
    private void OnCollisionEnter(Collision collision)
    {
    }
#endregion

#region Shoot
    void HandleShoot(Vector2 shootDirection)
    {

    }
#endregion
}
