using UnityEngine;
using System.Collections.Generic;
using System;

public class RANSAC {
    private static int N = 100;
    private static int S = 3;
    private static float D = 10;
    private static float X = 0.3f;
    private static int C = 10;

    private static System.Random random = new System.Random();

    public static List<Line> run(List<Sample> points, float sep_degree)
    {
        List<Line> lines = new List<Line>();
        int n_tires = 0;

        while (!(points.Count == 0) && n_tires < N)
        {
            n_tires++;
            int i = random.Next(points.Count);
            List<Sample> samples = new List<Sample>();
            List<int> indices = new List<int>();
            samples.Add(points[i]);
            indices.Add(i);
            int range = (int)Math.Ceiling(D / sep_degree);
            int lowerBound = (i - range) < 0 ? 0 : i - range;
            int upperBound = (i + range) > points.Count - 1 ? points.Count - 1 : i + range;
            List<Sample> neighbors = new List<Sample>();
            for (int c = lowerBound; c < upperBound; c++)
            {
                if (Math.Abs(points[c].Angle - points[i].Angle) < D)
                {
                    neighbors.Add(points[c]);
                }
            }
            Shuffle<Sample>(neighbors);
            for (int c = 0; c < S && c < neighbors.Count; c++)
            {
                samples.Add(neighbors[c]);
                indices.Add(lowerBound + c);
            }
            if (samples.Count < S)
                continue;
            float m, b;
            LLS(samples, out m, out b);
            foreach (Sample point in points)
            {
                if(samples.Contains(point))
                    continue;
                if (distance(m, b, point.Point) <= X)
                    samples.Add(point);
            }
            if (samples.Count > C)
            {
                LLS(samples, out m, out b);
                lines.Add(new Line(new Vector2(0, b), new Vector2(1, m)));
                foreach (Sample sample in samples)
                {
                    points.Remove(sample);
                    //Debug.Log(points.Count);
                }
            }

        }
        return lines;
    }

    public static List<Line> run(List<Sample> points, float sep_degree, int N, int S, float D, float X, int C)
    {
        List<Line> lines = new List<Line>();
        int n_attempts = 0;

        while (!(points.Count == 0) && n_attempts < N)
        {
            n_attempts++;
            int i = random.Next(points.Count);
            List<Sample> samples = new List<Sample>();
            samples.Add(points[i]);
            int range = (int)Math.Ceiling(D / sep_degree);
            int lowerBound = (i - range) < 0 ? 0 : i - range;
            int upperBound = (i + range) > points.Count - 1 ? points.Count - 1 : i + range;
            List<Sample> neighbors = new List<Sample>();
            for (int c = lowerBound; c < upperBound; c++)
            {
                if (Math.Abs(points[c].Angle - points[i].Angle) < D)
                {
                    neighbors.Add(points[c]);
                }
            }
            Shuffle<Sample>(neighbors);
            for (int c = 0; c < S && c < neighbors.Count; c++)
            {
                samples.Add(neighbors[c]);
            }
            if (samples.Count < S)
                continue;
            float m, b;
            LLS(samples, out m, out b);
			if (m == float.NaN)
				continue;
            foreach (Sample point in points)
            {
                if (samples.Contains(point))
                    continue;
                if (distance(m, b, point.Point) <= X)
                    samples.Add(point);
            }
            if (samples.Count > C)
            {
                //Debug.Log("Made it");
                LLS(samples, out m, out b);
				if (m != float.PositiveInfinity)
					lines.Add(new Line(new Vector2(0 + samples[0].Point.x, b), new Vector2(1, m)));
				else
					lines.Add(new Line(new Vector2(b, 0 + samples[0].Point.y), new Vector2(0, 1)));
                //Debug.DrawLine(lines[lines.Count - 1].Point, lines[lines.Count - 1].Point + 100 * lines[lines.Count - 1].D, Color.red);
                //if (n_attempts < 1)
                //    Debug.Log(points.Count);
                foreach (Sample sample in samples)
                {
                    points.Remove(sample);
                }
            }

        }
        //Debug.Log(lines.Count);
        return lines;
    }


    static void LLS(List<Sample> points, out float m, out float b)
    {
        float x, y, xy, x2, J;
		bool same_xs = true;

        x = y = xy = x2 = 0.0f;

        for (int i = 0; i < points.Count; i++)
        {
            x += points[i].Point.x;
            y += points[i].Point.y;
            xy += points[i].Point.x * points[i].Point.y;
            x2 += points[i].Point.x * points[i].Point.x;
            //y2 += points[i].Point.y * points[i].Point.y;
			if (i > 0 && points[i].Point.x != points[i-1].Point.x)
				same_xs = false;
        }

        J = ((float)points.Count * x2) - (x * x);
        if (J != 0.0 && !same_xs) {
			m = (((float)points.Count * xy) - (x * y)) / J;
			m = Convert.ToSingle (Math.Floor (1.0E3 * m + 0.5) / 1.0E3);
			b = ((y * x2) - (x * xy)) / J;
			b = Convert.ToSingle (Math.Floor (1.0E3 * b + 0.5) / 1.0E3);
			if (Math.Abs(m) < 0.01f)
			{
				m = 0.0f;
			}
			else
			{
				m = float.NaN;
			}
		} else if (same_xs) {
			//case for vertical lines, b will store what x we're at
			m = float.PositiveInfinity;
			b = points [0].Point.x;
		} else {
			m = float.NaN;
			b = 0.0f;
		}
    }

    static double distance(float m, float b, Vector2 point)
    {
		if (m != 0.0f && m != float.PositiveInfinity) {
			float mo = -1.0f / m;
			float bo = point.y - mo * point.x;
		
			float px = (b - bo) / (mo - m);
			float py = ((mo * (b - bo)) / (mo - m)) + bo;
		
			return Math.Sqrt (Math.Pow ((px - point.x), 2) + Math.Pow ((py - point.y), 2));
		} else if (m == 0.0f) {
			return Math.Abs (point.y - b);
		} else {
			return Math.Abs (point.x - b);
		}
    }

    public static void seed(int seed)
    {
        random = new System.Random(seed);
    }

    public static void Shuffle<T>(IList<T> l)
    {
        int n = l.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            T value = l[k];
            l[k] = l[n];
            l[n] = value;
        }
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

    public class Sample
    {
        private Vector2 point;
        private float angle;

        public Sample(Vector2 point, float angle)
        {
            this.point = point;
            this.angle = angle;
        }

        public Vector2 Point
        {
            get { return point; }
        }

        public float Angle
        {
            get { return angle; }
        }
    }
}
