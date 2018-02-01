using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSPVisualiation
{
    public class TSPInstance
    {
        public int Dimension { get; set; }
        public int[,] Data { get; set; }

        public TSPInstance()
        {

        }

        public override string ToString()
        {
            StringBuilder value = new StringBuilder();

            value.AppendFormat($"Liczba miast: {Dimension}\n Macierz:\n");
            if (Data != null)
                for (int i = 0; i < Dimension; i++)
                {
                    for (int j = 0; j < Dimension; j++)
                    {
                        value.Append($"{Data[i, j],5} ");
                    }
                    value.Append('\n');
                }

            return value.ToString();
        }
    }
}
