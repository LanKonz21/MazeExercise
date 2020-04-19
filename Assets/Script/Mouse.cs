using UnityEngine;

public class Mouse : MonoBehaviour
{
    private void OnTriggerEnter(Collider  collider)
    {
        MazeSceneManager.instance.SetFind(false);
    }
}
