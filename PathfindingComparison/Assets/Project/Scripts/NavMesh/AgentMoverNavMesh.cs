using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AgentMoverNavMesh : MonoBehaviour
{
    NavMeshAgent nav;

    void Awake() => nav = GetComponent<NavMeshAgent>();

    // movimento
    public void FollowPath(List<Vector3> wp)
    {
        if (wp == null || wp.Count < 2) return;

        nav.isStopped = false;              // riattiva il movimento
        nav.SetDestination(wp[wp.Count - 1]);
    }

    // stop / reset
    public void Stop()
    {
        nav.ResetPath();          // svuota il path
        nav.velocity = Vector3.zero;
        nav.isStopped = true;     // blocca subito il movimento
    }

    public void WarpTo(Vector3 pos)
    {
        nav.Warp(pos);            // teleport: aggiorna tutto
        nav.ResetPath();
        nav.velocity = Vector3.zero;
        nav.isStopped = true;
    }
}