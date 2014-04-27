using UnityEngine;
using System.Collections.Generic;
using System;

public class SLAM : MonoBehaviour{

    public int LIFE = 40;
    public float MAX_ERROR = 0.5f;
    public int MIN_OBSERVATIONS = 15; //min times a landmark must be seen to be counted as a landmark
    public float MAX_RANGE = 1f;
    public int MAX_LANDMARKS = 3000;

    //RANSAC variables
    public int MAX_TRIALS = 100;
    public int SAMPLES = 10;
    public int CONSENSUS = 30;
    public float TOLERANCE = 0.05f;
    public float DEGREES = 10f;

    //Raycasting variables
    public float distance = 5;
    public float fov = 90;
    public float angle = 0;
    public float degree_separation = 1f;
    public bool showRays = false;
    public bool showLines = false;

    //Landmark variables
    private List<Landmark> landmarkDB = new List<Landmark>();
    private int DBSize = 0;
    int[,] IDtoID;
    int EKFLandmarks = 0;

    void Start()
    {
        Landmark.LIFE = LIFE;
        IDtoID = new int[MAX_LANDMARKS, 2];
    }

    void Update()
    {
        //Raycasting
        List<RANSAC.Sample> points = new List<RANSAC.Sample>();
        for (float i = -fov / 2; i < fov / 2; i += degree_separation)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(Convert.ToSingle(Math.Cos((angle + i) * Math.PI / 180)), Convert.ToSingle(Math.Sin((angle + i) * Math.PI / 180))), distance, 1);
            if (hit.point != new Vector2(0, 0))
            {
                points.Add(new RANSAC.Sample(hit.point - (Vector2)transform.position, (angle + i)));
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
        
        //Extract lines
        List<RANSAC.Line> lines = RANSAC.run(points, degree_separation, MAX_TRIALS, SAMPLES, DEGREES, TOLERANCE, CONSENSUS);

        Landmark[] extracted_landmarks = new Landmark[lines.Count];
        for (int i = 0; i < lines.Count; i++)
        {
            extracted_landmarks[i] = GetLineLandmark(lines[i], this.gameObject.transform.position);
        }

        extracted_landmarks = UpdateAndAddLineLandmarks(extracted_landmarks);

        if (showLines)
        {
            foreach (Landmark lm in landmarkDB)
            {
                Debug.DrawRay(lm.Point, lm.D, Color.red);
            }
        }
        
    }

	public Vector2 Position
	{
		get { return this.gameObject.transform.position; }
	}

	public int[][] OccupancyGrid
	{
		get { return null; }
	}

    public class Landmark
    {
        public static int LIFE = 40;

        private Vector2 point;
        private int id;
        private int life;
        private int timesObserved;
        private float range;
        private float bearing;

        private Vector2 d;

        private float range_error;
        private float bearing_error;

        public Landmark()
        {
            timesObserved = 0;
            id = -1;
            point = new Vector2();
            life = LIFE;
            d = new Vector2();
        }

        public Vector2 Point
        {
            get { return point; }
            set { point = value; }
        }

        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        public int Life
        {
            get { return life; }
            set { life = value; }
        }

        public int TimesObserved
        {
            get { return timesObserved; }
            set { timesObserved = value; }
        }

        public float Range
        {
            get { return range; }
            set { range = value; }
        }

        public float Bearing
        {
            get { return bearing; }
            set { bearing = value; }
        }

        public Vector2 D
        {
            get { return d; }
            set { d = value; }
        }

        public float RangeError
        {
            get { return range_error; }
            set { range_error = value; }
        }

        public float BearingError
        {
            get { return bearing_error; }
            set { bearing_error = value; }
        }
    }

    private int GetSlamID(int id)
    {
        for (int i = 0; i < EKFLandmarks; i++)
        {
            if (IDtoID[i, 0] == id)
            {
                return IDtoID[i, 1];
            }
        }
        return -1;
    }

    private int AddSlamID(int lmID, int slamID)
    {
        IDtoID[EKFLandmarks, 0] = lmID;
        IDtoID[EKFLandmarks, 1] = slamID;
        EKFLandmarks++;
        return 0;
    }

    private Landmark[] UpdateAndAddLineLandmarks(Landmark[] extractedLandmarks)
    {
        Landmark[] tempLandmarks = new Landmark[extractedLandmarks.Length];
        for(int i = 0; i < extractedLandmarks.Length; i++)
        {
            tempLandmarks[i] = UpdateLandmark(extractedLandmarks[i]);
        }
        return tempLandmarks;
    }

    private Landmark UpdateLandmark(Landmark lm)
    {
        int id = GetAssociation(lm);
        if (id == -1)
            id = AddToDB(lm);
        lm.ID = id;
        return lm;
    }

    //todo: add landmark removal

    private Landmark GetLineLandmark(RANSAC.Line line, Vector2 AIPosition)
    {
        Vector2 d0 = new Vector2(line.D.y, line.D.x);
        Vector2 p0 = new Vector2(line.Point.y, line.Point.x);

        Vector2 pos = line.Point + AIPosition;

        float range = (float)Math.Sqrt(Math.Pow(line.Point.x, 2) + Math.Pow(line.Point.y, 2));
        float bearing = (float)Math.Atan(line.Point.y / line.Point.x) - angle;

        float range_error = d0.x == 0.0f ? Math.Abs(line.Point.x) : Math.Abs(line.Point.y);
        float bearing_error = (float)Math.Atan(p0.y/p0.x) - angle;

        Landmark lm = new Landmark();

        lm.Point = pos;
        lm.Range = range;
        lm.Bearing = bearing;
        lm.D = line.D;
        lm.RangeError = range_error;
        lm.BearingError = bearing_error;

        lm.ID = -1;
        lm.TimesObserved = 0;


        return lm;
    }

    private int GetAssociation(Landmark lm)
    {
        for (int i = 0; i < landmarkDB.Count; i++)
        {
            if (Distance(lm, landmarkDB[i]) < MAX_ERROR && landmarkDB[i].ID != -1)
            {
                landmarkDB[i].Life = LIFE;
                landmarkDB[i].TimesObserved++;
                landmarkDB[i].Bearing = lm.Bearing;
                landmarkDB[i].Range = lm.Range;
                return landmarkDB[i].ID;
            }
        }
        return -1;
    }

    public float Distance(Landmark l1, Landmark l2)
    {
        return (float)Math.Sqrt(Math.Pow(l2.Point.x - l1.Point.x, 2) + Math.Pow(l2.Point.y - l1.Point.y, 2));
    }

    private int AddToDB(Landmark lm)
    {
        if (DBSize + 1 < MAX_LANDMARKS)
        {
            Landmark toAdd = new Landmark();
            toAdd.Point = lm.Point;
            toAdd.Life = LIFE;
            toAdd.ID = DBSize;
            toAdd.TimesObserved = 1;
            toAdd.Bearing = lm.Bearing;
            toAdd.Range = lm.Range;
            toAdd.D = lm.D;

            DBSize++;
            return DBSize - 1;
        }
        return -1;
    }

}
