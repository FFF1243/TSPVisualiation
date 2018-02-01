using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSPVisualiation.Models.AG;

namespace TSPVisualiation.Models
{
    enum MutationType
    {
        Invert,
        Transposition
    }

    enum CrossBreedType
    {
        PMX,
        OX
    }


    class AGSolver
    {
        private Random _randomGenerator;
        private TSPInstance _instance;
        private TSPPopulation _population;
        private List<TSPRoute> _results;

        private int _populationSize;
        public static double _crossBreedRatio = 0.8;
        public static double _mutationRatio = 0.1;
        private Stopwatch _watch;
        private MutationType _mutationType;
        private CrossBreedType _crossType;
        private int _secondsToStop;

        private delegate void MutationDelegate(List<int> route, int i, int j);
        private MutationDelegate mutation;

        public AGSolver(TSPInstance ins, int populationSize, MutationType mutation, CrossBreedType crossType,List<TSPRoute> res, int seconds = 10)
        {
            _randomGenerator = new Random();
            _instance = ins;
            _populationSize = populationSize;
            _results = res;
            _mutationType = mutation;
            _crossType = crossType;
            _population = new TSPPopulation(ins, populationSize, crossType);
            _secondsToStop = seconds;
            if (mutation == MutationType.Invert)
            {
                this.mutation = invert;
            }
            else
            {
                this.mutation = transposition;
            }
        }

        private void transposition(List<int> route, int i, int j)
        {
            int temp = route[i];
            route[i] = route[j];
            route[j] = temp;
        }

        private void invert(List<int> route, int i, int j)
        {
            route.Reverse(i, j - i + 1);
        }

        private void MutationLoop()
        {
            foreach (var route in _population.Routes)
            {
                double chance = _randomGenerator.NextDouble();
                if (chance < _mutationRatio)
                {
                    int start = _randomGenerator.Next(1, route.Route.Count - 2);
                    int end = _randomGenerator.Next(start + 1, route.Route.Count - 2);
                    mutation(route.Route, start, end);
                    route.Distance = _population.CalculateDistance(route.Route.ToArray());
                }
            }
        }

        public TSPPopulation CreateNewPopulation(TSPPopulation oldPopulation, List<TSPRoute> childs)
        {

            var allRoutes = new List<TSPRoute>(childs);

            allRoutes.AddRange(oldPopulation.Routes.OrderBy(x => x.Distance).Take(1));
            //     var allRoutes = new List<TSPRoute>();
            //  allRoutes = allRoutes.Union(childs).ToList();

            var bestRoutes = allRoutes.OrderByDescending(x => x.FitnessValue).Take(_populationSize).ToList();
            var result = new TSPPopulation(bestRoutes, _instance, _populationSize, _crossType);

            return result;
        }

        public void GetAGSolution()
        {
            _watch = new Stopwatch();
            _watch.Start();
            var currentBest = new TSPRoute(_population.GetBestRoute());
            do
            {
                if (_watch.ElapsedMilliseconds >= 1000 * _secondsToStop)
                {
                    break;
                }
                var childs = _population.StartReproduction();
                _population = CreateNewPopulation(_population, childs);
                MutationLoop();
                var best = _population.GetBestRoute();
                if (best.Distance < currentBest.Distance)
                {
                    currentBest = new TSPRoute(best);
                   // yield return currentBest;
                    _results.Add(currentBest);
                   // Console.WriteLine(currentBest);

                }
            } while (true);

            //return currentBest;
        }
    }
}
