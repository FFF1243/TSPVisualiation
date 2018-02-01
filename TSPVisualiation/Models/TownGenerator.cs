using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSPVisualiation
{
    public class Point
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        protected bool Equals(Point other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Point) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }
    }

    public class TownGenerator
    {
        public int Size { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public List<Point> PointList { get; set; }
        private Random _randGenerator;
        public TownGenerator()
        {
            Size = 0;
            Width = 0;
            _randGenerator = new Random();
        }

        private HashSet<Point> GeneratePoints()
        {
            var set = new HashSet<Point>();
            do
            { 
                var x = _randGenerator.Next(Width - 1);
                var y = _randGenerator.Next(Height - 1);
                set.Add(new Point(x, y));
            } while (set.Count < Size);

            return set;
        }

        private int getDistance(Point a, Point b)
        {
            var dist = Math.Sqrt(Math.Pow((b.X - a.X), 2) + Math.Pow((b.Y - a.Y), 2));
            return Convert.ToInt32(Math.Round(dist));
        }

        public TSPInstance GenerateInstance()
        {
            var list = GeneratePoints().ToList();
            PointList = list;
            int [,] data = new int[Size,Size];
            var ins = new TSPInstance();
            ins.Dimension = Size;
            ins.Data = data;

            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (i == j)
                        data[i, j] = -1;
                    else
                        data[i, j] = getDistance(list[i], list[j]);
                }
            }

            return ins;
        }
    }
}
