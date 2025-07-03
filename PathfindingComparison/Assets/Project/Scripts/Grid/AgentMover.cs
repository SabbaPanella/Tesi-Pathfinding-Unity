using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
public class AgentMover : MonoBehaviour
{
    [SerializeField] private float stepTime = .1f;

    public void FollowPath(List<Node> path, float cellSize)
    {
        StopAllCoroutines();
        StartCoroutine(Follow(path, cellSize));
    }

    IEnumerator Follow(List<Node> path, float cellSize)
    {
        foreach (var n in path)
        {
            transform.position = (Vector2)n.GridPos * cellSize;
            yield return new WaitForSeconds(stepTime);
        }
    }
}