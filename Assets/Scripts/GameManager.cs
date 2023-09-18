using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using Scene = UnityEngine.SceneManagement.Scene;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Camera cam;
    [SerializeField]
    private Disc discBlackUp;
    [SerializeField]
    private Disc discWhiteUp;
    [SerializeField]
    private GameObject highLightPrefab;
    [SerializeField]
    private UIManager uIManager;

    private Dictionary<Player, Disc> discPrefabs = new Dictionary<Player, Disc>();
    private GameState gameState = new GameState();
    private Disc[,] discs = new Disc[8, 8];
    private List<GameObject> highLights = new List<GameObject>();

    private const float PLACE_DISC_TIME = 0.33f;
    private const float FLIP_TIME = 0.83f;
    private const float TWITCH_TIME = 0.05f;

    // Start is called before the first frame update
    private void Start()
    {
        discPrefabs[Player.Black] = discBlackUp;
        discPrefabs[Player.White] = discWhiteUp;

        AddStartDiscs();
        ShowLegalMoves();
        uIManager.SetPlayerText(gameState.CurrentPlayer);
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log("Mus tryckt");
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                Vector3 impact = hitInfo.point;
                Position boardPos = SceneToBoardPos(impact);
                OnBoardClicked(boardPos);
            }
        }
    }

    private void ShowLegalMoves()
    {
        foreach (Position boardPos in gameState.LegalMoves.Keys)
        {
            Vector3 scenePos = BoardToScenePos(boardPos) + Vector3.up * 0.01f;
            GameObject highLight = Instantiate(highLightPrefab, scenePos, Quaternion.identity);
            highLights.Add(highLight);
        }
    }
    private void HideLegalMoves()
    {
        highLights.ForEach(Destroy);
        highLights.Clear();
    }
    private void OnBoardClicked(Position boardPos)
    {
        if (gameState.MakeMove(boardPos, out MoveInfo moveInfo))
        {
            StartCoroutine(OnMoveMade(moveInfo));
        }
    }

    private IEnumerator OnMoveMade(MoveInfo moveInfo)
    {
        HideLegalMoves();
        yield return ShowMove(moveInfo);
        yield return ShowTurnOutcome(moveInfo);
        ShowLegalMoves();
    }

    private Position SceneToBoardPos(Vector3 scenePos)
    {
        int col = (int)(scenePos.x - 0.25f);
        int row = 7 - (int)(scenePos.z - 0.25f);
        return new Position(row, col);
    }
    private Vector3 BoardToScenePos(Position boardPos)
    {
        return new Vector3(boardPos.Col + 0.75f, 0, 7 - boardPos.Row + 0.75f);
    }

    private void SpawnDisc(Disc prefab, Position boardPos)
    {
        Vector3 scenePos = BoardToScenePos(boardPos) + Vector3.up * 0.1f;
        discs[boardPos.Row, boardPos.Col] = Instantiate(prefab, scenePos, Quaternion.identity);
    }
    private void AddStartDiscs()
    {
        foreach (Position boardPos in gameState.OccupiedPositions())
        {
            Player player = gameState.Board[boardPos.Row, boardPos.Col];
            SpawnDisc(discPrefabs[player], boardPos);
        }
    }
    private void FlipDiscs(List<Position> positions)
    {
        foreach (Position boardPos in positions)
        {
            discs[boardPos.Row, boardPos.Col].Flip();
        }
    }
    private IEnumerator ShowMove(MoveInfo moveInfo)
    {
        SpawnDisc(discPrefabs[moveInfo.Player], moveInfo.Position);
        yield return new WaitForSeconds(PLACE_DISC_TIME);
        FlipDiscs(moveInfo.Outflanked);
        yield return new WaitForSeconds(FLIP_TIME);
    }
    private IEnumerator ShowSkippedTurn(Player skippedPlayer)
    {
        uIManager.SetSkippedPlayerText(skippedPlayer);
        yield return uIManager.AnimateTopText();
    }
    private IEnumerator ShowTurnOutcome(MoveInfo moveInfo)
    {
        if (gameState.GameOver)
        {
            //Show game over
            yield return ShowGameOver(gameState.Winner);
            yield break;
        }
        Player currentPlayer = gameState.CurrentPlayer;
        if (currentPlayer == moveInfo.Player)
        {
            yield return ShowSkippedTurn(currentPlayer.Opponent());
        }
        uIManager.SetPlayerText(currentPlayer);
    }

    private IEnumerator ShowGameOver(Player winner)
    {
        uIManager.setTopText("Neither player can move!");
        yield return uIManager.AnimateTopText();
        yield return uIManager.ShowScoreText();
        yield return new WaitForSeconds(0.5f); //Paus halv sekund
        yield return ShowDiscCounting();
        uIManager.SetWinnerText(winner);
        yield return uIManager.ShowEndScreen();

    }

    private IEnumerator ShowDiscCounting()
    {
        int black = 0, white = 0;
        foreach (Position position in gameState.OccupiedPositions())
        {
            Player player = gameState.Board[position.Row, position.Col];
            if (player == Player.Black) {
                black++;
                uIManager.SetBlackScoreText(black);
            }
            else /* if (player == Player.White) */ {
                white++;
                uIManager.SetWhiteScoreText(white);
            }
            discs[position.Row,position.Col].Twitch();
            yield return new WaitForSeconds(TWITCH_TIME);
        }
    }

    private IEnumerator RestartGame(){
        yield return uIManager.HideEndScreen();
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }

    public void OnPlayAgainClicked(){
        StartCoroutine(RestartGame());
    }
}