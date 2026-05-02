using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

/* TODO
    * DONE | When the rod is not in a stance mode, it should be ruffle and then stunned for a certain amount of time
        - Ruffle animation with animation curve
        - Stun unable to move or receive input
        - Show stunned indicator above rod players
    * If player blocks the ball while in defensive stance mode, ball should snap the corresponding rod player
        - Snap the ball to rod player
        - Play struggle animation with animation curve
    * Player should be able to pass to the rod players on its right and left
        - Play pass animation with animation curve
        - Pass the ball to the player in the direction of the pass 
*/
public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    [Header("Teams")]
    [SerializeField] private TeamController m_HomeTeam;
    [SerializeField] private TeamController m_AwayTeam;

    [Header("Scoreboard")]
    [SerializeField] private TextMeshPro m_HomeScoreText;
    [SerializeField] private TextMeshPro m_AwayScoreText;

#region Unity Lifecycle


    public static GameManager Instance
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
        instance = FindObjectsByType<GameManager>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)[0];
        if (instance == null)
        {
            GameManager gameObj = new GameManager();
            gameObj.name = "GameManager";
            instance = gameObj.AddComponent<GameManager>();
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
        InitializeTeams();

    }

    void Update()
    {
        
    }
#endregion

    private void InitializeTeams()
    {
        ResetGame(); 
        Debug.Log($"[GameManager] Game started: {m_HomeTeam.teamName} {m_AwayTeam.teamName}");
    }

    public void ScoreGoal(bool isHome)
    {
        if(isHome)
        {
            m_HomeTeam.IncrementScore();
            Debug.Log($"[GameManager] Home Team({m_HomeTeam.teamName}) scored!");
        }
        else
        {
            m_AwayTeam.IncrementScore();
            Debug.Log($"[GameManager] Away Team({m_AwayTeam.teamName}) scored!");
        }

        UpdateScoreboard();

        Debug.Log($"[GameManager] {m_HomeTeam.teamName} {m_HomeTeam.score} - {m_AwayTeam.teamName} {m_AwayTeam.score}");
    }

    private void ResetGame()
    {
        m_HomeTeam.ResetScore();
        m_AwayTeam.ResetScore();

        UpdateScoreboard();

        Debug.Log($"[GameManager] Game reset: {m_HomeTeam.teamName} {m_AwayTeam.teamName}");
    }

    private void UpdateScoreboard()
    {
        m_HomeScoreText.text = m_HomeTeam.score.ToString();
        m_AwayScoreText.text = m_AwayTeam.score.ToString();
    }

#region Getters
    public TeamController GetHomeTeam() => m_HomeTeam;
    public TeamController GetAwayTeam() => m_AwayTeam;
#endregion
}
