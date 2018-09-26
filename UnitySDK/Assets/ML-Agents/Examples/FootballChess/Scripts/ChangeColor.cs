using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColor : MonoBehaviour
{
	[SerializeField] private SkinnedMeshRenderer HeadRenderer;
	[SerializeField] private SkinnedMeshRenderer BodyRenderer;

	public void ChangeMeshColor(Color color)
	{
		var newMat = new Material(HeadRenderer.material) {color = color};

		HeadRenderer.material = newMat;
		BodyRenderer.material = newMat;
	}
}
