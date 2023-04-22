using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    [Header("Boid Stats")]
    public float Speed;
    public float SteeringSpeed;
    public float NoClumpingRadius;
    public float LocalAreaRadius;

    [Header("Avoidance")]
    public float avoidanceStrength;

    [Header("Aligning")]
    public float aligningStrength;

    [Header("Centering")]
    public float centeringStrength;

    private List<Boid> activeBoids;

    void Start()
    {
        
    }

    public void UpdateBoids(List<Boid> _boids)
    {
        //TODO:check memory usage could get quite high
        activeBoids = new List<Boid>(_boids);
        activeBoids.Remove(this);
    }

    public void MovementStep(float deltaTime)
    {
        Vector3 steering = Vector3.zero;

        steering += (Vector3)CalculateAvoidance().normalized * 0.56f;
        steering += (Vector3)CalculateAlignment().normalized * 0.34f;
        steering += (Vector3)CalcuateCentering().normalized * 0.16f;
        

        //Vector3 rotatedVectorToTarget = Quaternion.Euler(0, 0, 90) * steering;
        //Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, steering);

        //if (steering != Vector3.zero)
        //    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, SteeringSpeed * deltaTime);

        Rotate(steering);

        transform.Translate(new Vector2(Speed * deltaTime, 0));

        //Debug.DrawRay(transform.position, transform.position + steering, Color.white);
    }

    private void Rotate(Vector3 moveInput)
    {
        float trig = Mathf.Atan2(-moveInput.x, moveInput.y) * Mathf.Rad2Deg;
        Quaternion q = Quaternion.AngleAxis(trig, Vector3.forward);
        //transform.rotation = Quaternion.Slerp(transform.rotation, q, SteeringSpeed);

        if (moveInput != Vector3.zero)
            transform.rotation = Quaternion.RotateTowards(transform.rotation, q, SteeringSpeed * Time.deltaTime);
    }

    private Vector2 CalculateAvoidance()
    {
        Vector2 avoidanceDirection = Vector2.zero;
        int boidCount = 0;

        foreach (Boid b in activeBoids) {

            float distance = Vector2.Distance(b.transform.position, transform.position);

            if(distance < NoClumpingRadius) {
                avoidanceDirection += (Vector2)(b.transform.position - transform.position);
                boidCount++;
            }

        }

        if(boidCount > 0) {
            avoidanceDirection /= boidCount;
        }

        //flip and normalise
        avoidanceDirection = - avoidanceDirection;

        Debug.DrawRay(transform.position, transform.position + (Vector3)avoidanceDirection,Color.red);

        return avoidanceDirection;
    }

    private Vector2 CalculateAlignment()
    {
        Vector2 alignmentDirection = Vector2.zero;
        int boidCount = 0;

        foreach (Boid b in activeBoids) {

            float distance = Vector2.Distance(b.transform.position, transform.position);

            if (distance < LocalAreaRadius) {
                alignmentDirection += (Vector2)b.transform.forward;
                boidCount++;
            }

        }

        if (boidCount > 0) {
            alignmentDirection /= boidCount;
        }

        Debug.DrawRay(transform.position, transform.position + (Vector3)alignmentDirection,Color.green);

        return alignmentDirection;
    }

    private Vector2 CalcuateCentering()
    {
        Vector2 centeringDirection = Vector2.zero;
        int boidCount = 0;

        foreach (Boid b in activeBoids) {

            float distance = Vector2.Distance(b.transform.position, transform.position);

            if (distance < LocalAreaRadius) {
                centeringDirection += (Vector2)(b.transform.position - transform.position);
                boidCount++;
            }

        }

        if (boidCount > 0) {
            centeringDirection /= boidCount;
        }

        centeringDirection -= (Vector2)transform.position;

        Debug.DrawRay(transform.position, transform.position + (Vector3)centeringDirection,Color.blue);

        return centeringDirection;
    }
}
