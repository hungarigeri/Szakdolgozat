using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public TileManager tileManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            Instance = this;

        }
        DontDestroyOnLoad(this.gameObject);

        tileManager=GetComponent<TileManager>();
    }
}
