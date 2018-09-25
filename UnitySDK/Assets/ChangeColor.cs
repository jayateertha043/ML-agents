using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColor : MonoBehaviour
{
	[SerializeField] private MeshRenderer Renderer;

	public void ChangeMeshColor(Color color)
	{
		Renderer.material.color = color;
	}
}
