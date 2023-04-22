using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    [HideInInspector]
    public List<Boid> boids;

    public Transform boidParent;


    void Awake()
    {
        boids = new List<Boid>(FindObjectsOfType<Boid>());

        foreach(Boid b in boids) {
            b.UpdateBoids(boids);
        }

    }

    private void Update()
    {
        foreach(Boid b in boids) {
            b.MovementStep(Time.deltaTime);
        }      

    }



}
