using UnityEngine;
using System.Collections.Generic;
using System;

public class Unit : MonoBehaviour
{
    
    public Vector2 movement;
    public float distance;
    public event Action<float> OnMove;

    // Update is called once per frame
    void FixedUpdate()
    {
        float movedDistance = movement.magnitude * Time.fixedDeltaTime;
        Move(new Vector2(movement.x, 0) * Time.fixedDeltaTime);
        Move(new Vector2(0, movement.y) * Time.fixedDeltaTime);
        distance += movedDistance;

        if (distance > 0.5f)
        {
            OnMove?.Invoke(distance);
            if(gameObject.CompareTag("Player")) // Only track distance for the player
            {
                RelicEventBus.PlayerMovedDistance(distance);
            }
            distance = 0;
        }
    }

    public void Move(Vector2 ds)
    {
        List<RaycastHit2D> hits = new List<RaycastHit2D>();
        int n = GetComponent<Rigidbody2D>().Cast(ds, hits, ds.magnitude * 2);
        if (n == 0)
        {
            transform.Translate(ds);
        }
    }


}