using System;
using System.Collections;
using System.Collections.Generic;
using MLAgents;
using UnityEngine;
using UnityScript.Scripting.Pipeline;

public class FootballAgent : Agent
{
	public Team Team;
	public List<FootballPlayer> Players;
	public List<FootballPlayer> Enemies;
	public Ball Ball;
	public Vector2Int Dimensions;

	public const int numOfActions = 12;

	[SerializeField] private FootballMatch FootballMatch;

	public override void InitializeAgent()
	{
	}

	public override void AgentReset()
	{
		Players = FootballMatch.GetTeam(Team);
		Enemies = FootballMatch.GetTeam(Team == Team.Blue ? Team.Red : Team.Blue);
		Ball = FootballMatch.Ball;
		Dimensions = FootballMatch.Dimensions;
	}

	public override void CollectObservations()
	{
		//player position
		foreach (var player in Players)
		{
			AddVectorObs(player.Position.x);
			AddVectorObs(player.Position.y);
		}
		//enemy position
		
		foreach (var player in Enemies)
		{
			AddVectorObs(Dimensions.x - 1 - player.Position.x);
			AddVectorObs(Dimensions.y - 1 - player.Position.y);
		}
		
		//ball
		if (Ball.InPossession.Team == Team)
		{
			AddVectorObs(Ball.InPossession.Position.x);
			AddVectorObs(Ball.InPossession.Position.y);
		}
		else
		{
			AddVectorObs(Dimensions.x - 1 - Ball.InPossession.Position.x);
			AddVectorObs(Dimensions.y - 1 - Ball.InPossession.Position.y);
		}

		SetMask();
	}

	private void SetMask()
	{
		for (int i = 0; i < Players.Count; i++)
		{
			var player = Players[i];

			if (player.Position.x == 0)
			{
				SetActionMask(0, new List<int>()
				{
					(int) AllMoves.Down + i * numOfActions,
					(int) AllMoves.DownLeft + i * numOfActions,
					(int) AllMoves.DownRight + i * numOfActions
				});
			}

			if (player.Position.x == Dimensions.x - 1)
			{
				SetActionMask(0, new List<int>()
				{
					(int) AllMoves.Up + i * numOfActions,
					(int) AllMoves.UpLeft + i * numOfActions,
					(int) AllMoves.UpRight + i * numOfActions
				});
			}

			if (player.Position.y == 0)
			{
				SetActionMask(0, new List<int>()
				{
					(int) AllMoves.Left + i * numOfActions,
					(int) AllMoves.DownLeft + i * numOfActions,
					(int) AllMoves.UpLeft + i * numOfActions
				});
			}

			if (player.Position.y == Dimensions.y - 1)
			{
				SetActionMask(0, new List<int>()
				{
					(int) AllMoves.Right + i * numOfActions,
					(int) AllMoves.DownRight + i * numOfActions,
					(int) AllMoves.UpRight + i * numOfActions
				});
			}

			if (Ball.InPossession.Team != Team || Ball.InPossession.Position != player.Position)
			{
				SetActionMask(0, new List<int>()
				{
					(int) AllMoves.PassTo1 + i * numOfActions,
					(int) AllMoves.PassTo2 + i * numOfActions,
					(int) AllMoves.PassTo3 + i * numOfActions
				});
			}
			else if (Ball.InPossession == player)
			{
				SetActionMask(0, (int) AllMoves.PassTo1 + i + i * numOfActions);
			}

			if (Ball.InPossession.Team == Team)
			{
				var ownerPos = Ball.InPossession.Position;
				for (int j = 0; j < Players.Count; j++)
				{
					var pos = Players[j].Position;
					pos -= ownerPos;
					var distance = Math.Abs(pos.x) + Math.Abs(pos.y);
					if (distance > 2)
					{
						SetActionMask(0, (int) AllMoves.PassTo1 + j + i * numOfActions);
					}
				}
			}
		}
	}

	public override void AgentAction(float[] vectorAction, string textAction)
	{
		var num = Mathf.FloorToInt(vectorAction[0]);
		var player = num / numOfActions;
		var action = (AllMoves) (num % numOfActions);

		var move = new MoveTransaction() {player = player, action = action, team = Team};
		FootballMatch.ScheduleTransaction(move);
	}
}
