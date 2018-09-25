using System;
using System.Collections.Generic;
using UnityEngine;

public class FootballPlayer
{
	public Vector2Int Position;
	public Team Team;
	public int JerseyNumber;
	public GameSimulator GameSimulator;

	public FootballPlayer(Vector2Int Position, Team Team, int JerseyNumber, GameSimulator GameSimulator)
	{
		this.Position = Position;
		this.Team = Team;
		this.JerseyNumber = JerseyNumber;
		this.GameSimulator = GameSimulator;
	}
	
	public Vector2Int WorldPosition(Vector2Int dimensions)
	{
		if (Team == Team.Blue)
		{
			return Position;
		}

		return new Vector2Int(dimensions.x - 1 - Position.x, dimensions.y - 1 - Position.y);
	}
	
	public void HandleMove( AllMoves action)
	{
		switch (action)
		{
			case AllMoves.DoNothing:
				break;
			case AllMoves.Up:
				Position.x +=1;
				break;
			case AllMoves.Down:
				Position.x -=1;
				break;
			case AllMoves.Left:
				Position.y -=1;
				break;
			case AllMoves.Right:
				Position.y +=1;
				break;
			case AllMoves.UpLeft:
				Position.x +=1;
				Position.y -=1;
				break;
			case AllMoves.UpRight:
				Position.x +=1;
				Position.y +=1;
				break;
			case AllMoves.DownLeft:
				Position.x -=1;
				Position.y -=1;
				break;
			case AllMoves.DownRight:
				Position.x -=1;
				Position.y +=1;
				break;
			case AllMoves.PassTo1:
				GameSimulator.Ball.InPossession = GameSimulator.GetTeam(Team)[0];
				break;
			case AllMoves.PassTo2:
				GameSimulator.Ball.InPossession = GameSimulator.GetTeam(Team)[1];
				break;
			case AllMoves.PassTo3:
				GameSimulator.Ball.InPossession = GameSimulator.GetTeam(Team)[2];
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(action), action, null);
		}
	}
}
