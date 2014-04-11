using UnityEngine;
using System.Collections.Generic;
using System;

public class RANSAC {
    private static int N = 100;
    private static int S = 3;
    private static float D = 10;
    private static float X = 10;
    private static int C = 10;

    private static System.Random random = new System.Random();

    public static List<Line> run(List<Vector2> points, float sep_degree)
    {
        List<Line> lines = new List<Line>();
        int n_tires = 0;

        while (!(points.Count == 0) && n_tires < N)
        {
            int i = random.Next(points.Count);
            List<Vector2> samples = new List<Vector2>();
            samples.Add(points[i]);
            Vector2 sample;
            int range = (int)(D / sep_degree);
            int lowerBound = (i - range) < 0 ? 0 : i - range;
            int upperBound = (i + range) > points.Count - 1 ? points.Count - 1 : i + range;
            List<Vector2> neighbors = samples.GetRange(lowerBound, upperBound - lowerBound);
            //int c = 0;
            //while (c < S && samples.Count != points.Count)
            //{
            //    Vector2 sample;
            //    int range = (int)(D / sep_degree);
            //    int lowerBound = (i - range) < 0 ? 0 : i - range;
            //    int upperBound = (i + range) > points.Count - 1 ? points.Count - 1 : i + range;
            //    int ind = i + random.Next(lowerBound, upperBound);
            //    sample = points[ind];
            //    if (!(samples.Contains(sample)))
            //    {
            //        samples.Add(sample);
            //    }
            //    else
            //    {
            //        c--;
            //    }
            //}
            float m, b;
            LLS(samples, out m, out b);
            foreach (Vector2 point in points)
            {
                if(samples.Contains(point))
                    continue;
                if (distance(m, b, point) <= X)
                    samples.Add(point);
            }
            if (samples.Count > C)
            {
                LLS(samples, out m, out b);
                lines.Add(new Line(new Vector2(0, b), new Vector2(1, m)));
                foreach (Vector2 sample in samples)
                {
                    points.Remove(sample);
                }
            }

        }

        return lines;
    }

    static void LLS(List<Vector2> points, out float m, out float b)
    {
        float x, y, xy, x2, J;

        x = y = xy = x2 = 0.0f;

        for (int i = 0; i < points.Count; i++)
        {
            x += points[i].x;
            y += points[i].y;
            xy += points[i].x * points[i].y;
            x2 += points[i].x * points[i].x;
        }

        J = ((float)points.Count * x2) - (x * x);
        if (J != 0.0)
        {
            m = (((float)points.Count * xy) - (x * y)) / J;
            m = Convert.ToSingle(Math.Floor(1.0E3 * m + 0.5) / 1.0E3);
            b = ((y * x2) - (x * xy)) / J;
            b = Convert.ToSingle(Math.Floor(1.0E3 * b + 0.5) / 1.0E3);
        }
        else
        {
            m = 0.0f;
            b = 0.0f;
        }
    }

    static double distance(double m, double b, Vector2 point)
    {
        return Math.Abs(m * point.x + 1 * point.y + b) / Math.Sqrt(m * m + 1 * 1);
    }

    public static void seed(int seed)
    {
        random = new System.Random(seed);
    }

    public class Line
    {
        private Vector2 point;
        private Vector2 d;

        public Line(Vector2 point, Vector2 d)
        {
            this.point = point;
            this.d = d;
        }

        public Vector2 Point
        {
            get { return point; }
        }
        
        public Vector2 D 
        {
            get { return d; }
        }
    }

}
