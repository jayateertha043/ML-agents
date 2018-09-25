using System.Collections.Generic;
using UnityEngine;

public class Ball
{
	public FootballPlayer InPossession { get; set; }

	public bool InOpponentGoal(Team team)
	{
		if (team != InPossession.Team)
		{
			var x = InPossession.Position.x;
			var y = InPossession.Position.y;
			return x == 0 && (y == 1 || y == 2 || y == 3);
		}
		else
		{
			var x = InPossession.Position.x;
			var y = InPossession.Position.y;
			return x == 9 && (y == 1 || y == 2 || y == 3);
		}
	}
}
