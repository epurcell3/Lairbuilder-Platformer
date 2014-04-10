using UnityEngine;
using System.Collections.Generic;
using System;

public class RANSACTest : MonoBehaviour {

    List<RANSAC.Line> lines;

    public bool showRays = false;
    public bool showLines = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        List<Vector2> points = new List<Vector2>();
        double fov = 90;
        float distance = 1;
        for (double i = -fov/2; i < fov/2; i += 1)
        {
            Ray ray = new Ray(transform.position, new Vector2(Convert.ToSingle(Math.Cos(i * Math.PI / 180)), Convert.ToSingle(Math.Sin(i * Math.PI / 180))));
            //Debug.DrawRay(transform.position, new Vector2(Convert.ToSingle(Math.Cos(i * 2 * Math.PI / 180)), Convert.ToSingle(Math.Sin(i * 2 * Math.PI / 180))));
            RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(Convert.ToSingle(Math.Cos(i * Math.PI / 180)), Convert.ToSingle(Math.Sin(i * Math.PI / 180))), distance, 1);
            if (hit != null && hit.point != new Vector2(0,0))
            {
                points.Add(hit.point);
                Debug.Log(hit.point);
                if (showRays)
                    Debug.DrawLine(transform.position, hit.point, Color.cyan);
            }
            else
            {
                if (showRays)
                {
                    Vector2 end = new Vector2(transform.position.x + Convert.ToSingle(Math.Cos(i * Math.PI / 180)) * distance, transform.position.y + Convert.ToSingle(Math.Sin(i * Math.PI / 180)) * distance);
                    Debug.DrawLine(transform.position, end, Color.green);
                }
            }
        }

        lines = RANSAC.run(points, 1);

        foreach (RANSAC.Line line in lines)
        {
            if (showLines)
            {
                Debug.DrawRay(line.Point, line.D);
            }
        }

	}
}
