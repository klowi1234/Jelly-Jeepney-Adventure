using UnityEngine;

public class ParallaxBG : MonoBehaviour
{
    private float length, startposX, offsetY;
    public GameObject cam;
    public float parallaxEffect;

    void Start()
    {
        startposX = transform.position.x;
        offsetY = transform.position.y - cam.transform.position.y; // preserve vertical depth
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        float temp = cam.transform.position.x * (1 - parallaxEffect);
        float dist = cam.transform.position.x * parallaxEffect;

        // Keep original vertical offset to preserve depth
        transform.position = new Vector3(
            startposX + dist,
            cam.transform.position.y + offsetY,
            transform.position.z
        );

        // Loop background infinitely
        if (temp > startposX + length) startposX += length;
        else if (temp < startposX - length) startposX -= length;
    }
}
