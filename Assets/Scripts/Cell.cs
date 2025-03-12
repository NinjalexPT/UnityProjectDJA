using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Cell
{
    public bool Visited;
    public int WallTop;
    public int WallBottom;
    public int WallLeft;
    public int WallRight;
    public int X;
    public int Y;
    
    public Cell(int x, int y)
    {
        X = x;
        Y = y;
        Visited = false;
        WallTop = 0;
        WallBottom = 0;
        WallLeft = 0;
        WallRight = 0;
    }
}
