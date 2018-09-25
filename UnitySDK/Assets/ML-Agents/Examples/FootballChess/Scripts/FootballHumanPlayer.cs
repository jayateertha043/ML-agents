using UnityEngine;

public class FootballHumanPlayer: MonoBehaviour
{
	private FootballMatch FootballMatch;

	private void Update()
	{
#if UNITY_ANDROID
	if (Input.GetTouch(0).phase == TouchPhase.Began)	
#endif
		if (Input.GetKeyDown(KeyCode.Mouse0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			//if (Physics.Raycast(ray))
				//Instantiate(particle, transform.position, transform.rotation);
		}
		
	}

	public void ScheduleTransaction(FootballMatch match, int player, AllMoves move)
	{
		
	}

	public void FinishButtonHandler()
	{
		
	}
}
