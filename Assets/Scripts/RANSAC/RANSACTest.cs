using UnityEngine;
using System.Collections.Generic;
using System;

public class RANSACTest : MonoBehaviour {

    List<RANSAC.Line> lines;

    public bool showRays = true;
    public bool showLines = true;

    public float distance = 5;
    public float fov = 90;
    public float angle = 0;
    public float degree_separation = 1f;


    public int N = 100;
    public int S = 3;
    public float D = 10;
    public float X = 0.3f;
    public int C = 10;

    private int t = 0;

	private List<RANSAC.Line> landmarks = new List<RANSAC.Line>();
	private List<int>		  seen_count = new List<int>();

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        //System.IO.StreamWriter sw = new System.IO.StreamWriter(@"C:\Users\Edd\Documents\GAI\data.txt", true);
        //sw.WriteLine("Time " + t++);
        List<RANSAC.Sample> points = new List<RANSAC.Sample>();
        for (float i = -fov/2; i < fov/2; i += degree_separation)
        {
            //Ray ray = new Ray(transform.position, new Vector2(Convert.ToSingle(Math.Cos(i * Math.PI / 180)), Convert.ToSingle(Math.Sin(i * Math.PI / 180))));
            //Debug.DrawRay(transform.position, new Vector2(Convert.ToSingle(Math.Cos(i * 2 * Math.PI / 180)), Convert.ToSingle(Math.Sin(i * 2 * Math.PI / 180))));
            RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(Convert.ToSingle(Math.Cos((angle + i) * Math.PI / 180)), Convert.ToSingle(Math.Sin((angle + i) * Math.PI / 180))), distance, 1);
            if (hit.point != new Vector2(0,0))
            {
                //sw.WriteLine(hit.point - (Vector2)transform.position + "," + (angle + i));
                points.Add(new RANSAC.Sample(hit.point - (Vector2)transform.position, (angle + i)));
                //Debug.Log(hit.point);
                if (showRays)
                {
                    Debug.DrawLine(transform.position, hit.point, Color.cyan);
                }
            }
            else
            {
                if (showRays)
                {
                    Vector2 end = new Vector2(transform.position.x + Convert.ToSingle(Math.Cos((angle + i) * Math.PI / 180)) * distance, transform.position.y + Convert.ToSingle(Math.Sin((angle + i) * Math.PI / 180)) * distance);
                    Debug.DrawLine(transform.position, end, Color.green);
                }
            }
        }

        //sw.Flush();
        //sw.Close();

        lines = RANSAC.run(points, degree_separation, N, S, D, X, C);
        //Debug.Log(lines.Count);
        foreach (RANSAC.Line line in lines)
        {
			RANSAC.Line adjusted = new RANSAC.Line(line.Point + (Vector2)transform.position, line.D);
			if (landmarks.Contains(adjusted))
			{
				seen_count[landmarks.IndexOf(adjusted)]++;
			}
			else
			{
				landmarks.Add(adjusted);
				seen_count.Add(1);
			}
            //if (showLines)
            //{
                //Debug.DrawLine(line.Point + (Vector2)transform.position, (Vector2)transform.position + line.Point + 100 * line.D, Color.red);
            //    Debug.DrawRay(line.Point + (Vector2)transform.position, line.D, Color.red);
            //}
        }
		foreach (RANSAC.Line landmark in landmarks)
		{
			Debug.DrawRay(landmark.Point, landmark.D, Color.red);
		}

	}
}
