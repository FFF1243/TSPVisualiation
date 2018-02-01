using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSPVisualiation.Models
{
    public class TSPRoute : IEquatable<TSPRoute>
    {
        public TSPRoute()
        {
            Route = new List<int>();
            Distance = 0;
            FitnessValue = 0;
        }

        public TSPRoute(TSPRoute tsp)
        {
            this.Route = tsp.Route;
            this.Distance = tsp.Distance;
            this.FitnessValue = tsp.FitnessValue;
        }

        public List<int> Route { get; set; }
        private int _distance;

        public int Distance
        {
            get { return _distance; }
            set
            {
                _distance = value;
                FitnessValue = (double)1 / value;
            }
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _distance;
                hashCode = (hashCode * 397) ^ (Route != null ? Route.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ FitnessValue.GetHashCode();
                return hashCode;
            }
        }

        /* public override bool Equals(object obj)
         {
             var tspRoute = obj as TSPRoute;
             return tspRoute.Route.SequenceEqual(this.Route);
         }*/

        public double FitnessValue { get; set; }


        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var city in Route)
            {
                stringBuilder.Append($"{city} - ");
            }
            stringBuilder.Append($"\nDystans: {Distance}");

            return stringBuilder.ToString();
        }

        public void InitRouteFromPermutation(int[] perm)
        {

            Route.Clear();
            Route.Add(0);
            for (int i = 0; i < perm.Length; i++)
            {
                Route.Add(perm[i]);
            }
            Route.Add(0);
        }

        public bool Equals(TSPRoute other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _distance == other._distance && Route.SequenceEqual(other.Route) && FitnessValue.Equals(other.FitnessValue);
        }
    }

    public class TspRouteComparer : IEqualityComparer<TSPRoute>
    {
        // Products are equal if their names and product numbers are equal.
        public bool Equals(TSPRoute x, TSPRoute y)
        {

            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the products' properties are equal.
            return x.Route.SequenceEqual(y.Route);
        }


        public int GetHashCode(TSPRoute route)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(route, null)) return 0;

            return route.Route.GetHashCode();
        }
    }
}
