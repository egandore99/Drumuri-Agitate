using UnityEngine;

public class CameraLogic : MonoBehaviour
{
    public void MoveCamera(int step)
    {
        switch (step)
        {
            case 0:
                transform.position = new Vector3(0f, 0.75f, -10f);
                break;
            case 1:
                transform.position = new Vector3(0f, 12.5f, -10f);
                break;
        }
    }
}
