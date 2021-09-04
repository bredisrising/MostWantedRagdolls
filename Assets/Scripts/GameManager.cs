using UnityEngine;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // When enemies will be spawned, immediately add them to this list.
    public Transform[] enemies;

    public CinemachineTargetGroup cinemachineTargetGroup;

    [SerializeField] float slowDownFactor = 0.05f;
    [SerializeField] float slowDownLength = 3f;
    [SerializeField] bool goBackToNormalTime = true;
    [SerializeField] bool slowdownAtStart = false;
    // Start is called before the first frame update

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }
    void Start()
    { 
        foreach (Transform enemy in enemies)
        {
            cinemachineTargetGroup.AddMember(enemy, 1, 1);
        }

        if (slowdownAtStart)
        {
            DoSlowmotion();
        }
    }

    void Update()
    {
        if(Time.timeScale != 1 && Time.fixedDeltaTime != .02f && goBackToNormalTime)
        {
            Time.timeScale += (1f / slowDownLength) * Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 1f);

            Time.fixedDeltaTime = Time.timeScale * .02f;
        }

        //Time.timeScale += (1f / slowDownLength) * Time.unscaledDeltaTime;
        //Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 1f);

    }

    public void DoSlowmotion()
    {
        Time.timeScale *= slowDownFactor;
        //Time.fixedDeltaTime *= slowDownFactor;
    }

}
