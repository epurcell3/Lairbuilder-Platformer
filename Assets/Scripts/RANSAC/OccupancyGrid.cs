using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class OccupancyGrid
{
    private SLAM.Cell[,] grid;

    private int xoffset = 0, yoffset = 0;
    private int rows, cols;

    public OccupancyGrid(int rows, int cols)
    {
        this.rows = rows;
        this.cols = cols;
        grid = new SLAM.Cell[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                grid[i, j].Occupant = SLAM.Occupant.UNEXPLORED;
            }
        }
    }

    public int GetLength(int dimension)
    {
        if (dimension < 0 || dimension > 1)
            return -1;
        return grid.GetLength(dimension);
    }

    public SLAM.Cell this[int x, int y]
    {
        get {
            int i = x + xoffset;
            int j = y + yoffset;
            if (i >= 0 && i < rows && j >= 0 && j < cols)
                return grid[i, j];
            else
                return new SLAM.Cell(SLAM.Occupant.UNEXPLORED, 1f);
        }
        set
        {
            int i = x + xoffset;
            int j = y + yoffset;
            if (i < 0 || i >= rows)
            {
                xoffset += rows / 2;
                SLAM.Cell[,] newgrid = new SLAM.Cell[rows * 2, cols];
                for (int i2 = 0; i2 < rows * 2; i2++)
                {
                    for (int j2 = 0; j2 < cols; j2++)
                    {
                        if (i2 < rows / 2 || i2 >= 3 * rows / 2)
                            newgrid[i2, j2].Occupant = SLAM.Occupant.UNEXPLORED;
                        else
                            newgrid[i2, j2] = grid[i2 + xoffset - rows / 2, j2 + yoffset];
                    }
                }
                rows *= 2;
                grid = newgrid;
            }
            if (j < 0 || j >= cols)
            {
                yoffset += cols / 2;
                SLAM.Cell[,] newgrid = new SLAM.Cell[rows, cols * 2];
                for (int i2 = 0; i2 < rows; i2++)
                {
                    for (int j2 = 0; j2 < cols * 2; j2++)
                    {
                        if (j2 < cols / 2 || j2 >= 3 * cols / 2)
                            newgrid[i2, j2].Occupant = SLAM.Occupant.UNEXPLORED;
                        else
                            newgrid[i2, j2] = grid[i2 + xoffset, j2 + yoffset - cols/2];
                    }
                }
                cols *= 2;
                grid = newgrid;
            }
            grid[i, j] = value;
        }
    }
}
