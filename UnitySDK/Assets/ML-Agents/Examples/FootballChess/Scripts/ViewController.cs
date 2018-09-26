using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewController : MonoBehaviour
{
	private const string GridCellPrefabName = "Prefabs/GridCell";
	private const string PlayerPrefabName = "Prefabs/Player";
	private const string RedPlayerPrefabName = "Prefabs/RedPlayer";
	private const string BluePlayerPrefabName = "Prefabs/BluePlayer";
	private const string FootballPrefabName = "Prefabs/Football";
	
	[SerializeField] private float AnimationDuration = 1f;

	[SerializeField] private Transform GridTransform;
	[SerializeField] private Transform PlayersTransform;
	[SerializeField] private bool UseAnimatedPlayers;
	
	private GameObject gridCellPrefab;
	private GameObject bluePlayerPrefab;
	private GameObject redPlayerPrefab;
	private GameObject footballPrefab;

	private GameSimulator GameSimulator;
	
	private GameObject[,] grid;
	
	private List<GameObject> redTeam;
	private List<GameObject> blueTeam;

	private GameObject ball;

	private int width;
	private int height;

	private void LoadPrefabs()
	{
		gridCellPrefab = Resources.Load<GameObject>(GridCellPrefabName);
		if (UseAnimatedPlayers)
		{
			bluePlayerPrefab = Resources.Load<GameObject>(PlayerPrefabName);
			//bluePlayerPrefab.GetComponent<ChangeColor>().ChangeMeshColor(Color.blue);
			redPlayerPrefab = Resources.Load<GameObject>(PlayerPrefabName);
			//redPlayerPrefab.GetComponent<ChangeColor>().ChangeMeshColor(Color.red);
		}
		else
		{
			bluePlayerPrefab = Resources.Load<GameObject>(BluePlayerPrefabName);
			redPlayerPrefab = Resources.Load<GameObject>(RedPlayerPrefabName);
		}

		footballPrefab = Resources.Load<GameObject>(FootballPrefabName);
	}

	public void Init(int height, int width, Formation formation, GameSimulator simulator)
	{
		GameSimulator = simulator; 
		if (grid != null)
		{
			return;
		}

		this.height = height;
		this.width = width;
		
		SetupGameBoard();
		SetupPlayers(formation);
		
	}

	private void SetupPlayers(Formation formation)
	{
		if (blueTeam != null) return;

		blueTeam = new List<GameObject>();
		redTeam = new List<GameObject>();
		
		for (var i = 0; i < formation.BallFormation.Count; i++)
		{
			var blue = Instantiate(bluePlayerPrefab);
			var red = Instantiate(redPlayerPrefab);
			
			blue.transform.parent = PlayersTransform;
			red.transform.parent = PlayersTransform;

			if (UseAnimatedPlayers)
			{
				blue.GetComponent<ChangeColor>().ChangeMeshColor(Color.blue);
				red.GetComponent<ChangeColor>().ChangeMeshColor(Color.red);
			}
			
			blueTeam.Add(blue);
			redTeam.Add(red);
		}

		ball = Instantiate(footballPrefab);
	}


	private void SetupGameBoard()
	{
		if (grid != null)
		{
			return;
		}

		grid = new GameObject[height, width];
		
		LoadPrefabs();
		
		for (var i = 0; i < height; i++)
		{
			for (var j = 0; j < width; j++)
			{
				var position = transform.position + new Vector3(i, 0, j) * 1.2f;
				grid[i, j] = Instantiate(gridCellPrefab, position, Quaternion.identity);
				grid[i, j].transform.parent = GridTransform;
				if (IsBlueGoal(i, j))
				{
					grid[i,j].GetComponent<Renderer>().material.color = Color.blue;
				}
				if (IsRedGoal(i, j))
				{
					grid[i,j].GetComponent<Renderer>().material.color = Color.red;
				}
			}
		}
	}
	
	public void UpdatePlayerPositions(List<FootballPlayer> bTeam, List<FootballPlayer> rTeam, Ball b, bool force = false)
	{
		for (var i = 0; i < bTeam.Count; i++)
		{
			blueTeam[i].transform.position = grid[bTeam[i].Position.x, bTeam[i].Position.y].transform.position;
			var x = height - 1 - rTeam[i].Position.x;
			var y = width - 1 - rTeam[i].Position.y;
			redTeam[i].transform.position = grid[x,y].transform.position;
		}

		var pos = b.InPossession.Position;

		if (b.InPossession.Team == Team.Red)
		{
			pos.x = height - 1 - pos.x;
			pos.y = width - 1 - pos.y;
		}

		UpdateBallOwner(b.InPossession, force);	
	}

	public void UpdateMovedPlayers(GameSimulator.Move bluePlayer, GameSimulator.Move redPlayer, Ball b)
	{
		var oldBluePosition = grid[bluePlayer.oldPosition.x, bluePlayer.oldPosition.y].transform.position;
		var newBluePosition = grid[bluePlayer.newPosition.x, bluePlayer.newPosition.y].transform.position;

		var oldRedPosition = grid[height - 1 - redPlayer.oldPosition.x, width - 1 - redPlayer.oldPosition.y].transform.position;
		var newRedPosition = grid[height - 1 - redPlayer.newPosition.x, width - 1 - redPlayer.newPosition.y].transform.position;


		if (AnimationDuration == 0)
		{
			UpdatePlayerPositions(GameSimulator.BlueTeam, GameSimulator.RedTeam, GameSimulator.Ball, true);
			GameSimulator.ReadyForNewRequest = true;
		}
		else
		{

			AnimatingPlayers = true;
			data = new AnimateData()
			{
				bluePlayer = blueTeam[bluePlayer.player],
				oldBluePosition = oldBluePosition,
				newBluePosition = newBluePosition,
				oldRedPosition = oldRedPosition,
				newRedPosition = newRedPosition,
				redPlayer = redTeam[redPlayer.player],
				oldBallPosition = ball.transform.localPosition

			};
			timer = 0f;
		}
	}

	public void UpdateBallOwner(FootballPlayer player, bool force = false)
	{
		var team = player.Team == Team.Blue ? blueTeam : redTeam;
		ball.transform.parent = team[player.JerseyNumber].transform;
		if (force)
		{
			ball.transform.localPosition = Vector3.zero;
		}
	}

	struct AnimateData
	{
		public GameObject bluePlayer;
		public Vector3 oldBluePosition;
		public Vector3 newBluePosition;
		public GameObject redPlayer;
		public Vector3 oldRedPosition;
		public Vector3 newRedPosition;
		public Vector3 oldBallPosition;

	}
	
	[HideInInspector]
	public bool AnimatingPlayers;
	private AnimateData data;
	private float timer = 0f;

	private void Update()
	{
		if (AnimatingPlayers)
		{
			timer += Time.deltaTime;
			if (timer <= AnimationDuration)
			{
				var progress = timer / AnimationDuration;
				data.bluePlayer.transform.position = Vector3.Lerp(data.oldBluePosition, data.newBluePosition, progress);
				data.redPlayer.transform.position = Vector3.Lerp(data.oldRedPosition, data.newRedPosition, progress);
				ball.transform.localPosition = Vector3.Lerp(data.oldBallPosition, Vector3.zero, progress);
			}
			else
			{
				UpdatePlayerPositions(GameSimulator.BlueTeam, GameSimulator.RedTeam, GameSimulator.Ball);
				AnimatingPlayers = false;
				GameSimulator.ReadyForNewRequest = true;
			}
		}
	}

/*
	GUIStyle guiStyle = new GUIStyle();
	private void OnGUI()
	{
		guiStyle.fontSize = 50;
		guiStyle.normal.textColor = Color.white;
		GUI.BeginGroup(new Rect(10, 10, 500, 200));
		GUI.Label(new Rect(10, 25, 500, 100), "Score: " + GameSimulator.Score.x + " : " + GameSimulator.Score.y, guiStyle);
		GUI.EndGroup();
	}
*/
	
	private bool IsBlueGoal(int x, int y)
	{
		return  x == 0 && (y == 1 || y == 2 || y == 3);
	}
	
	private bool IsRedGoal(int x, int y)
	{
		return  x == 9 && (y == 1 || y == 2 || y == 3);
	}
}
