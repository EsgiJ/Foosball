using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class RodController : MonoBehaviour
{
#region Properties 

    /* Rod General Properties */
    [Header("Rod Properties")]
    [SerializeField, Min(0f)]
    private float m_MovementSpeed = 5f;

    [SerializeField, Min(0f)]
    private float m_MinZPos = -1.5f;

    [SerializeField, Min(0f)]
    private float m_MaxZPos = 1.5f;

    [SerializeField, Min(0f)]
    private float m_RodUsableExtent = 6f;

    [SerializeField, Range(1, 5)]
    private int m_FootballPlayerCount = 3;

    /* Child Football Players*/
    [Header("Football Player Properties")]
    [SerializeField] 
    private GameObject footballPlayer;

    private List<GameObject> footballPlayers = new List<GameObject>();

    /* Action properties*/
    [Header("Animation Properties")]
    InputAction m_ShootAction;
    InputAction m_MoveAction;
    InputAction m_AimAction ;
    
    private Vector2 m_AimVector = Vector2.zero;

    /* Animation properties */
    [Header("Animation Properties")]
    [SerializeField] 
    private AnimationCurve m_RotationCurve;

    [SerializeField, Min(0f)] 
    private float m_RotationDuration = 0.5f;

    /*Events*/
    public static event Action<Vector2> OnShootEvent;

    /*State*/
    private enum EState
    {
        None,
        DefenceStance,
        AttackStance,
        Shoot
    }
    EState m_State = EState.None;
#endregion

#region Unity Lifecycle
    void Start()
    {
        /* InputAction bindings */
        m_ShootAction = InputSystem.actions.FindAction("Shoot");
        m_ShootAction.started += OnShoot;

        m_MoveAction = InputSystem.actions.FindAction("MoveRod");  
        m_AimAction = InputSystem.actions.FindAction("Aim");

        SpawnFootballPlayers();
    }

    void Update()
    {
        OnMove();
        OnAim();
    }

    void OnDestroy()
    {
        m_ShootAction.started -= OnShoot;
    }
#endregion

#region InputActions
    private void OnShoot(InputAction.CallbackContext context)
    {
        if(m_ShootAction == null)
        {
            Debug.LogWarning("Shoot action not found!");
            return;
        }

        Debug.Log("Shoot action triggered!");

        OnShootEvent?.Invoke(m_AimVector);
        StartCoroutine(ShootCoroutine());
    }

    private void OnMove()
    {        
        if(m_MoveAction == null)
        {
            Debug.LogWarning("MoveRod action not found!");
            return;
        }

        Vector3 position = transform.localPosition;

        if (position.z < m_MinZPos)
            position.z = m_MinZPos;
        else if (position.z > m_MaxZPos)
            position.z = m_MaxZPos;
        else
            position.z += m_MoveAction.ReadValue<float>() * m_MovementSpeed * Time.deltaTime;

        transform.localPosition = position;
    }

    private void OnAim()
    {
        if(m_AimAction == null)
        {
            Debug.LogWarning("Aim action not found!");
            return;
        }
        m_AimVector = m_AimAction.ReadValue<Vector2>();
    }
#endregion

#region Coroutines
    IEnumerator ShootCoroutine()
    {
        float elapsed = 0f;
        Quaternion startRotation = transform.localRotation;
        Quaternion m_shootTargetRotation = startRotation * Quaternion.Euler(0, 180f, 0);

        while(elapsed < m_RotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / m_RotationDuration;

            float curveValue = m_RotationCurve.Evaluate(t);

            transform.localRotation = Quaternion.Lerp(startRotation, m_shootTargetRotation, curveValue);

            yield return null;
        }
    }
#endregion

#region Spawn Football Players
    void SpawnFootballPlayers()
    {
        for(int i = 0; i < m_FootballPlayerCount; i++)
        {
            Vector3 spawnPosition = CalculateFootballPlayerPosition(i);
            GameObject player = Instantiate(footballPlayer, spawnPosition, footballPlayer.transform.rotation);
            player.transform.SetParent(transform, true);
            footballPlayers.Add(player);
        }
    }

    Vector3 CalculateFootballPlayerPosition(int index)
    {
        float spacing = m_RodUsableExtent / (m_FootballPlayerCount + 1);
        float zOffset = (index + 1) * spacing - (m_RodUsableExtent / 2f);
        return transform.position + new Vector3(0f, 0f, zOffset);
    }
#endregion
}
