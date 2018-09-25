using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FootballMatch : MonoBehaviour
{
	public const string FormationPath = "Assets/ML-Agents/Examples/FootballChess/Resources/Formation.json";

	private GameSimulator GameSimulator;
	
	[SerializeField] private ViewController ViewController;
	
	[SerializeField] private FootballAgent BluePlayer;
	[SerializeField] private FootballAgent RedPlayer;
	
	private MoveTransaction redPlayerMove;
	private MoveTransaction bluePlayerMove;

	[HideInInspector] 
	public Vector2Int Dimensions => new Vector2Int(10, 5);

	public Ball Ball => GameSimulator.Ball;

	private bool done = false;

	private void Awake()
	{
		var formation = GenerateFormation();
		GameSimulator = new GameSimulator(5, 10, ViewController);
		ViewController.Init(Dimensions.x, Dimensions.y, formation, GameSimulator);
	}

	private void Start()
	{
		MatchReset();
	}
	
	private void Update()
	{
		if (GameSimulator.ReadyForNewRequest)
		{
			if (BluePlayer != null)
			{
				BluePlayer.RequestDecision();
			}

			RedPlayer.RequestDecision();
		}
	}


	private Formation GenerateFormation()
	{
		var dimensions = Dimensions;

		var ballFormation = new List<Vector2Int>();
		GenerateSinglePlayerPosition(ballFormation);
		GenerateSinglePlayerPosition(ballFormation);
		GenerateSinglePlayerPosition(ballFormation);
		
		
		var normalFormation = new List<Vector2Int>();
		GenerateSinglePlayerPosition(normalFormation);
		GenerateSinglePlayerPosition(normalFormation);
		GenerateSinglePlayerPosition(normalFormation);
		

		var startingTeam = (Team) Random.Range(0, 2);

		return new Formation()
			{BallFormation = ballFormation, NormalFormation = normalFormation, StartingTeam = startingTeam};
	}

	private void GenerateSinglePlayerPosition(List<Vector2Int> formation)
	{
		var dimensions = Dimensions;
		
		var player = new Vector2Int(Random.Range(1, dimensions.x / 2 - 1), Random.Range(0, dimensions.y - 1));
		while (formation.Contains(player))
		{
			player = new Vector2Int(Random.Range(1, dimensions.x / 2 - 1), Random.Range(0, dimensions.y - 1));
		}
		formation.Add(player);
	}


	public void MatchReset()
	{
		GameSimulator.Reset(GenerateFormation());
		RedPlayer.AgentReset();
		if (BluePlayer != null)
		{
			BluePlayer.AgentReset();
		}
	}

	public void ScheduleTransaction(MoveTransaction moveTransaction)
	{
		if (moveTransaction.team == Team.Blue)
		{
			bluePlayerMove = moveTransaction;
		}
		else
		{
			redPlayerMove = moveTransaction;
		}

		if (bluePlayerMove != null && redPlayerMove != null)
		{
			ExecuteMoves();
			bluePlayerMove = null;
			redPlayerMove = null;
		}
	}

	private void ExecuteMoves()
	{
		var moves = new List<MoveTransaction>() {bluePlayerMove, redPlayerMove};
		//ViewController.StartAnimation(moves, GameSimulator.BlueTeam, GameSimulator.RedTeam);
		
		var rewards = GameSimulator.ExecuteMoves(moves);

		//DebugPrint();

		BluePlayer.AddReward(rewards[0]);
		RedPlayer.AddReward(rewards[1]);

		if (rewards[0] > 0.5f || rewards[0] < -0.5f)
		{
			GameSimulator.Reset(GenerateFormation());
			BluePlayer.Done();
			RedPlayer.Done();
		}
	}

	private void DebugPrint()
	{
		string bTeam = "";
		foreach (var player in GetTeam(Team.Blue))
		{
			bTeam += player.WorldPosition(Dimensions) + " ";
		}

		string rTeam = "";
		foreach (var player in GetTeam(Team.Red))
		{
			rTeam += player.WorldPosition(Dimensions) + " ";
		}

		Debug.Log("Blue: " + bTeam + "\nRed: " + rTeam + "\nBall " + GameSimulator.Ball.InPossession.WorldPosition(Dimensions));
	}

	public List<FootballPlayer> GetTeam(Team team)
	{
		return team == Team.Blue ? GameSimulator.BlueTeam : GameSimulator.RedTeam;
	}
}