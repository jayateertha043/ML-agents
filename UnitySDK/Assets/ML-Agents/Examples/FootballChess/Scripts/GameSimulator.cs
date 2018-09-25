using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public enum Team
{
	Blue,
	Red
}

public enum AllMoves
{
	DoNothing = 0,
	Up,
	Down,
	Left,
	Right,
	UpLeft,
	UpRight,
	DownLeft,
	DownRight,
	PassTo1,
	PassTo2,
	PassTo3,
}

public class GameSimulator
{
	private int width;
	private int height;
	private bool ready;

	public Vector2Int Score;
	
	private List<FootballPlayer> blueTeam;
	private List<FootballPlayer> redTeam;
	private Ball ball;
	
	private Formation startingFormation;
	
	private ViewController ViewController;

	public Vector2Int GetDimensions => new Vector2Int(height, width);
	
	public Ball Ball
	{
		get { return ball; }
	}
	
	public List<FootballPlayer> BlueTeam => GetTeam(Team.Blue);

	public List<FootballPlayer> RedTeam => GetTeam(Team.Red);

	
	public bool ReadyForNewRequest {
		get {
			if (ready)
			{
				ready = false;
				return true;
			}
			return ready;
		}

		set { ready = value; }
	}

	public GameSimulator(int width, int height, ViewController ViewController)
	{
		this.width = width;
		this.height = height;
		this.ViewController = ViewController;
		ready = true;
	}

	public void Reset(Formation formation)
	{
		startingFormation = formation;
		
		blueTeam = GetPlayersFromFormation(
			startingFormation.StartingTeam == Team.Blue
				? startingFormation.BallFormation
				: startingFormation.NormalFormation, Team.Blue);

		redTeam = GetPlayersFromFormation(
			startingFormation.StartingTeam == Team.Red
				? startingFormation.BallFormation
				: startingFormation.NormalFormation, Team.Red);

		ball = startingFormation.StartingTeam == Team.Blue
			? new Ball() {InPossession = blueTeam[Random.Range(0, blueTeam.Count - 1)]}
			: new Ball() {InPossession = redTeam[Random.Range(0, redTeam.Count - 1)]};
		
		ViewController.UpdatePlayerPositions(BlueTeam, RedTeam, Ball, true);

	}

	private List<FootballPlayer> GetPlayersFromFormation(List<Vector2Int> formation, Team team)
	{
		var lst = new List<FootballPlayer>();
		for (int i = 0; i < formation.Count; i++) 
		{
			var player = formation[i];

			var footballPlayer = new FootballPlayer(player, team, i, this);
			lst.Add(footballPlayer);
		}

		return lst;
	}
	
	public List<FootballPlayer> GetTeam(Team teamColor)
	{
		return teamColor == Team.Blue ? blueTeam : redTeam;
	}

	public struct Move
	{
		public Vector2Int oldPosition;
		public Vector2Int newPosition;
		public int player;
	}
	
	public List<float> ExecuteMoves(List<MoveTransaction> moveTransactions)
	{
		var bluePlayer = new Move();
		var redPlayer = new Move();

		foreach (var move in moveTransactions)
		{
			switch (move.team)
			{
				case Team.Blue:
					bluePlayer.oldPosition = blueTeam[move.player].Position;
					bluePlayer.player = move.player;
					blueTeam[move.player].HandleMove(move.action);
					bluePlayer.newPosition = blueTeam[move.player].Position;
					break;
				case Team.Red:
					redPlayer.oldPosition = redTeam[move.player].Position;
					redPlayer.player = move.player;
					redTeam[move.player].HandleMove(move.action);
					redPlayer.newPosition = redTeam[move.player].Position;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		
		var opponentTeam = ball.InPossession.Team == Team.Blue ? redTeam : blueTeam;
		
		foreach (var player in opponentTeam)
		{
			if (player.WorldPosition(GetDimensions) == ball.InPossession.WorldPosition(GetDimensions))
			{
				ball.InPossession = player;
				ViewController.UpdateBallOwner(player);
				break;
			}
		}
		
		ViewController.UpdateMovedPlayers(bluePlayer, redPlayer, ball);
		CheckIfBallInGoal();

		return GenerateRewards();
	}

	private void CheckIfBallInGoal()
	{
		Score.x += ball.InOpponentGoal(Team.Blue) ? 1 : 0;
		Score.y += ball.InOpponentGoal(Team.Red) ? 1 : 0;
	}

	private List<float> GenerateRewards()
	{
		return new List<float>() { GenerateRewardsForTeam(Team.Blue), GenerateRewardsForTeam(Team.Red) };
	}

	private float GenerateRewardsForTeam(Team team)
	{
		var reward = 0f;
		reward += ball.InPossession.Team == team ? 0.01f : -0.01f;
		reward += ball.InOpponentGoal(team == Team.Blue ? Team.Red : Team.Blue) ? -1f : 0f;
		reward += ball.InOpponentGoal(team) ? 1f : 0f;
		return reward;
	}

	
}