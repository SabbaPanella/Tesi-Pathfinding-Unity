using UnityEngine;

public class RtsSpawner : MonoBehaviour
{
    public GridManager grid;
    public GameObject unitPrefab;
    public int count = 100;

    void Start()
    {
        for (int i = 0; i < count; ++i)
        {
            Vector2Int gp = new Vector2Int(Random.Range(0, grid.Width),
                Random.Range(0, grid.Height));
            Node n = grid.GetNode(gp.x, gp.y);
            if (!n.Walkable) { i--; continue; }

            Vector3 world = grid.CellCenter(gp);
            Instantiate(unitPrefab, world, Quaternion.identity, transform);
        }
    }
}