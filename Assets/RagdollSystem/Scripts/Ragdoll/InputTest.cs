using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GunsArcade.Ragdoll;

public class InputTest : MonoBehaviour
{
    Ray ray;
    public LayerMask limbMask;
    public float force = 5f;

    float timer = 0;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (timer <= 0)
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                HitObject();
            }
            else timer -= Time.deltaTime;
        }
    }

    void HitObject()
    {
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f, limbMask))
        {
            float minX = Random.Range(0, 2) == 0 ? ray.direction.x : -ray.direction.x;
            float minY = Random.Range(0, 2) == 0 ? ray.direction.y : -ray.direction.y;
            float maxX = minX > 0 ? 1 : -1;
            float maxY = minY > 0 ? 1 : -1;
            hit.collider.GetComponent<MuscleScript>().GetDamage(new Vector3(Random.Range(ray.direction.x, maxX), Random.Range(ray.direction.y, maxY), ray.direction.z) * Random.Range(5, force + 5));
        }
        timer = 0.1f;
    }
}
