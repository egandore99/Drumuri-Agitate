using UnityEngine;

public class Home : MonoBehaviour
{
    public GameObject frog;
    public GameManager gm;

    private void OnEnable()
    {
        frog.SetActive(true);
    }

    private void OnDisable()
    {
        frog.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            enabled = true;
            gm.HomeOccupied();
        }
    }
}