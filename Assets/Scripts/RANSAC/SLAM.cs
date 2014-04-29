using UnityEngine;
using System.Collections.Generic;
using System;

public class SLAM : MonoBehaviour
{

    #region Instance Variables
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
    private float prev_angle;
    public float degree_separation = 1f;
    public bool showRays = false;
    public bool showLines = false;

    //Landmark variables
    private List<Landmark> landmarkDB = new List<Landmark>();
    private int DBSize = 0;
    int[,] IDtoID;
    int EKFLandmarks = 0;

    //localization and occupancy grid variables
    private Vector2 position, estimate_position;
    private Cell[,] occupancyGrid;
    private Matrix X; //state matrix
    private Matrix P; //covariance matrix
    private float c = 0.01f; //error for position
    private float range_error = 0.01f;
    private float bearing_error = 0.1f;
    #endregion

    #region Unity Calls
    void Start()
    {
        Landmark.LIFE = LIFE;
        IDtoID = new int[MAX_LANDMARKS, 2];
        int width = (int)(48*(1/MAX_ERROR)*2);
        int height = (int)(24*(1/MAX_ERROR)*2);
        occupancyGrid = new Cell[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                occupancyGrid[i, j] = new Cell(Occupant.UNEXPLORED, 1.0f);
            }
        }
        prev_angle = angle;
        P = new Matrix(3, 3);
        X = new Matrix(3, 1);
    }

    void Update()
    {
        #region Step 1 - Update the estimate of the state based on odometry

        Vector2 delta_p = rigidbody2D.velocity * Time.deltaTime;
        float delta_t = angle - prev_angle;
        estimate_position = delta_p + position;
        X[0, 0] = estimate_position.x;
        X[1, 0] = estimate_position.y;
        X[2, 0] = delta_t;
        Matrix A = Matrix.Parse("1  0 " + (-1 * delta_p.y) + "\r\n0 1 "+ (delta_p.x) + "\r\n0 0 1");
        Matrix Q = Matrix.Parse(c*delta_p.x*delta_p.x +" "+ c*delta_p.x*delta_p.y +" "+ c*delta_p.x*delta_t+"\r\n"+
                                c*delta_p.y*delta_p.x +" "+ c*delta_p.y*delta_p.y +" "+ c*delta_p.y*delta_t+"\r\n"+
                                c*delta_t*delta_p.x   +" "+ c*delta_t*delta_p.y   +" "+ c*delta_t*delta_t);
        updatePrr(A, Q);
        updatePri(A);
        #endregion

        #region Landmark extraction
        List<RANSAC.Sample> points = new List<RANSAC.Sample>();
        for (float i = -fov / 2; i < fov / 2; i += degree_separation)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(Convert.ToSingle(Math.Cos((angle + i) * Math.PI / 180)), Convert.ToSingle(Math.Sin((angle + i) * Math.PI / 180))), distance, 1);
            if (hit.point != new Vector2(0, 0))
            {
                if (hit.collider.name == "Wall")
                {
                    points.Add(new RANSAC.Sample(hit.point - (Vector2)transform.position, (angle + i)));
                    if (showRays)
                    {
                        Debug.DrawLine(transform.position, hit.point, Color.cyan);
                    }
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
            extracted_landmarks[i] = GetLineLandmark(lines[i], position);
        }

        extracted_landmarks = UpdateAndAddLineLandmarks(extracted_landmarks);

        if (showLines)
        {
            foreach (Landmark lm in landmarkDB)
            {
                Debug.DrawRay(lm.Point, lm.D, Color.red);
            }
        }
        #endregion

        #region Step 2 - Update state from reobserved landmarks
        for (int i = 0; i < EKFLandmarks; i++)
        {
            Landmark lm = landmarkDB[i];
            Matrix H = new Matrix(2, DBSize * 2 + 3);
            H[0, 0] = (estimate_position.x - lm.Point.x) / lm.Range;
            H[0, 1] = (estimate_position.y - lm.Point.y) / (lm.Range * lm.Range);
            H[1, 0] = (estimate_position.y - lm.Point.y) / lm.Range;
            H[1, 1] = (estimate_position.x - lm.Point.x) / (lm.Range * lm.Range);
            H[2, 0] = 0;
            H[2, 1] = -1;
            H[(2 * i) + 3, 0] = -1 * (estimate_position.x - lm.Point.x) / lm.Range; ;
            H[(2 * i) + 3, 1] = -1 * (estimate_position.y - lm.Point.y) / (lm.Range * lm.Range);
            H[(2 * i) + 4, 0] = -1 * (estimate_position.y - lm.Point.y) / lm.Range;
            H[(2 * i) + 4, 1] = -1 * (estimate_position.x - lm.Point.x) / (lm.Range * lm.Range);

            Matrix R = new Matrix(2, 2);
            R[0, 0] = lm.Range * range_error;
            R[1, 1] = bearing_error;

            Matrix V = Matrix.IdentityMatrix(2, 2);

            Matrix S = H * P * Matrix.Transpose(H) + V * R * Matrix.Transpose(V);

            Matrix K = P * Matrix.Transpose(H) * S.Invert();

            Matrix z = new Matrix(2, 1);
            z[0, 0] = lm.Range;
            z[1, 0] = lm.Bearing;

            Matrix h = new Matrix(2, 1);
            h[0, 0] = lm.RangeError;
            h[1, 0] = lm.BearingError;

            X = X + K * (z - h);
        }
        #endregion

        #region Step 3
        for (int i = EKFLandmarks; i < DBSize; i++)
        {
            //add landmark to state
            Landmark lm = landmarkDB[i];
            Matrix newState = new Matrix(X.rows + 2, 1);
            copyTo(ref newState, X, 0, X.rows, 0, 0, 1, 0);
            newState[X.rows, 0] = lm.Point.x;
            newState[X.rows + 1, 0] = lm.Point.y;
            X = newState;

            //add landmark to covariance matrix
            Matrix Jxr = new Matrix(2, 3);
            Jxr[0, 0] = 1;
            Jxr[0, 1] = 0;
            Jxr[0, 2] = -delta_p.y;
            Jxr[1, 0] = 0;
            Jxr[1, 1] = 1;
            Jxr[1, 2] = delta_p.x;

            float delta_thrust = delta_p.y / (float)Math.Sin(angle * Math.PI / 180);

            Matrix Jz = new Matrix(2, 2);
            Jz[0, 0] = Math.Cos((angle + delta_t)*Math.PI/180);
            Jz[0, 1] = -delta_thrust * Math.Sin((angle + delta_t) * Math.PI / 180);
            Jz[1, 0] = Math.Sin((angle + delta_t)*Math.PI/180);
            Jz[1, 1] = delta_thrust * Math.Cos((angle + delta_t) * Math.PI / 180);
            
            Matrix R = new Matrix(2, 2);
            R[0, 0] = lm.Range * range_error;
            R[1, 1] = bearing_error;

            addPn1n1(Jxr, Jz, R);

            //add robot - landmark covariance and landmark - robot covariance
            addPrn1(Jxr);

            //add landmark - landmark covariance
            addPn1i(Jxr);
        }
        #endregion
        EKFLandmarks = DBSize;
        position = new Vector2((float)X[0, 0], (float)X[1, 0]);
        prev_angle = angle;
    }
    #endregion

    #region Properties
    public Vector2 Position
	{
		get { return position; }
	}

	public Cell[,] OccupancyGrid
	{
		get { return occupancyGrid; }
	}
    #endregion

    #region Landmark
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
    #endregion

    #region SLAM stuff
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

			landmarkDB.Add (toAdd);

            DBSize++;
            return DBSize - 1;
        }
        return -1;
    }
    #endregion

    #region EKF functions
    private void updatePrr(Matrix A, Matrix Q)
    {
        Matrix Prr = new Matrix(3, 3);
        copyTo(ref Prr, P, 0, 3, 0, 0, 3, 0);
        Prr = A * Prr * A + Q;
        copyTo(ref P, Prr, 0, 3, 0, 0, 3, 0);
        //float[,] tmp = new float[3, 3];
        //for (int i = 0; i < 3; i++)
        //{
        //    for (int j = 0; j < 3; j++)
        //    {
        //        tmp[i,j] = A[i,0]*P[0,j] + A[i,1]*P[1,j] + A[i,2]*P[2,j];
        //    }
        //}
        //for (int i = 0; i < 3; i++)
        //{
        //    for (int j = 0; j < 3; j++)
        //    {
        //        P[i, j] = tmp[i,j];
        //    }
        //}

        //for (int i = 0; i < 3; i++)
        //{
        //    for (int j = 0; j < 3; j++)
        //    {
        //        tmp[i, j] = P[i, 0]*A[0, j] + P[i, 1]*A[1, j] + P[i, 2]*A[2, j];
        //    }
        //}
        //for (int i = 0; i < 3; i++)
        //{
        //    for (int j = 0; j < 3; j++)
        //    {
        //        P[i, j] = tmp[i, j];
        //    }
        //}

        //for (int i = 0; i < 3; i++)
        //{
        //    for (int j = 0; j < 3; j++)
        //    {
        //        P[i, j] += Q[i, j];
        //    }
        //}
    }

    private void updatePri(Matrix A)
    {
        if (DBSize <= 0)
            return;
        Matrix Pri = new Matrix(3, DBSize * 2);
        copyTo(ref Pri, P, 0, 3, 0, 0, DBSize * 2, 3);
        Pri = A * Pri;
        copyTo(ref P, Pri, 0, 3, 0, 3, DBSize * 2 + 3, -3);

        //float[,] tmp = new float[3, P.GetUpperBound(1) + 1];
        //for (int i = 0; i < 3; i++)
        //{
        //    for (int j = 3; j <= P.GetUpperBound(1); j++)
        //    {
        //        tmp[i,j] = A[i,0]*P[0,j] + A[i,1]*P[1,j] + A[i,2]*P[2,j];
        //    }
        //}
        //for (int i = 0; i < 3; i++)
        //{
        //    for (int j = 3; j <= P.GetUpperBound(1); j++)
        //    {
        //        P[i,j] = tmp[i, j];
        //    }
        //}
    }

    private void addPn1n1(Matrix Jxr, Matrix Jz, Matrix R)
    {
        Matrix Prr = new Matrix(3, 3);
        copyTo(ref Prr, P, 0, 3, 0, 0, 3, 0);

        Matrix Pn1n1 = Jxr * Prr * Matrix.Transpose(Jxr) + Jz * R * Matrix.Transpose(Jz);

        Matrix newP = new Matrix(P.rows + 2, P.cols + 2);
        copyTo(ref newP, P, 0, P.rows, 0, 0, P.cols, 0);
        newP[P.rows, P.cols] = Pn1n1[0, 0];
        newP[P.rows, P.cols + 1] = Pn1n1[0, 1];
        newP[P.rows + 1, P.cols] = Pn1n1[1, 0];
        newP[P.rows + 1, P.cols + 1] = Pn1n1[1, 1];

        P = newP;
    }

    private void addPrn1(Matrix Jxr)
    {
        Matrix Prr = new Matrix(3, 3);
        copyTo(ref Prr, P, 0, 3, 0, 0, 3, 0);
        Matrix Prn1 = Prr * Matrix.Transpose(Jxr);
        Matrix Pn1r = Matrix.Transpose(Prn1);
        copyTo(ref P, Prn1, 0, 3, 0, 0, 2, P.cols - 2);
        copyTo(ref P, Pn1r, 0, 2, P.rows - 2, 0, 3, 0);
    }

    private void addPn1i(Matrix Jxr)
    {
        Matrix Pri = new Matrix(3, P.rows - 2 - 3);
        copyTo(ref Pri, P, 0, 3, 0, 0, P.rows - 2 - 3, 3);
        Matrix Pn1i = Jxr * Pri;
        Matrix Pin1 = Matrix.Transpose(Pn1i);
        copyTo(ref P, Pn1i, P.rows - 2, P.rows, -1 * (P.rows - 2), 3, P.cols - 2, -3);
        copyTo(ref P, Pin1, 3, P.rows - 2, -3, P.cols - 2, P.cols, -1 * (P.cols - 2));
    }
    #endregion

    private void copyTo(ref Matrix to, Matrix from, int i_start, int i_end, int from_i_offset, int j_start, int j_end, int from_j_offset)
    {
        for (int i = i_start; i < i_end; i++)
        {
            for (int j = j_start; j < j_end; j++)
            {
                to[i, j] = from[i + from_i_offset, j + from_j_offset];
            }
        }
    }

    public bool exploredEnoughToSlam()
    {
		return false;
	}

    #region Occupancy Grid stuff
    public enum Occupant
    {
        UNEXPLORED = -1, OPEN = 0, OUTTER_WALL = 1, WALL = 2, DOOR = 3, DANGER = 4, AURA = 5 
    }

    public class Cell
    {
        Occupant occupant;
        float probability;

        public Cell(Occupant occupant, float probability)
        {
            this.occupant = occupant;
            this.probability = probability;
        }

        public Occupant Occupant
        {
            get { return occupant; }
            set { occupant = value; }
        }

        public float Probability
        {
            get { return probability; }
            set { probability = value; }
        }
    }

    private void updateOccupancyGrid(Landmark[] extracted_landmarks)
    {
        for (int x = 0; x < extracted_landmarks.Length; x++)
        {
            Landmark lm = extracted_landmarks[x];
            int r = (int)(lm.Point.x * (1 / MAX_ERROR));
            int c = (int)(lm.Point.y * (1 / MAX_ERROR));

            occupancyGrid[r, c].Occupant = Occupant.WALL;

            int i_start = (int)(position.x * (1 / MAX_ERROR));
            int j_start = (int)(position.y * (1 / MAX_ERROR));
            int i_d = i_start > r ? -1 : 1;
            int j_d = j_start > c ? -1 : 1;

            for (int i = i_start; i != r; i += i_d)
            {
                for (int j = j_start; j != r; j += j_d)
                {
                    if (occupancyGrid[i, j].Occupant == Occupant.UNEXPLORED)
                    {
                        occupancyGrid[i, j].Occupant = Occupant.OPEN;
                    }
                }
            }
        }
    }

    private void removeOccupant(Landmark lm)
    {
        int r = (int)(lm.Point.x * (1 / MAX_ERROR));
        int c = (int)(lm.Point.y * (1 / MAX_ERROR));

        occupancyGrid[r, c].Occupant = Occupant.OPEN;
    }
    #endregion
}
