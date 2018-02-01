using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSPVisualiation.Models.TabuSearch
{
    enum Neighbours
    {
        SWAP,
        INVERT,
        EDGE_SWAP
    }

    class TSSolver
    {
        private static int NUM_ITERATION = Int32.MaxValue;
        private TSPInstance instance;
        private bool[] visited;
        private int[,] tabu_list;
        private int tenure;
        private int QUIT = 1000;
        private delegate void NeighbourFunc(int[] route, int i, int j);
        private NeighbourFunc neighbour;
        private Stopwatch watch;
        private int seconds;
        private bool diversification;

        private List<TSPRoute> _results;
        public TSSolver(TSPInstance ins, Neighbours neig, bool div, List<TSPRoute> res,  int sec = 1200)
        {
            instance = ins;
            tenure = Convert.ToInt32(Math.Sqrt(instance.Dimension));
            seconds = sec;
            diversification = div;
            _results = res;

            if (neig == Neighbours.SWAP)
                neighbour = swap;
            else if (neig == Neighbours.INVERT)
                neighbour = invert;
            else if (neig == Neighbours.EDGE_SWAP)
                neighbour = edgeSwap;

            initArrays();
        }

        private void clearVisitedArray()
        {
            visited = new bool[instance.Dimension];
            visited[0] = true;
            for (int i = 1; i < visited.Length; i++)
            {
                visited[i] = false;
            }
        }

        private void initArrays()
        {
            clearVisitedArray();
            tabu_list = new int[instance.Dimension, instance.Dimension];
            for (int i = 0; i < instance.Dimension; i++)
            {
                for (int j = 0; j < instance.Dimension; j++)
                {
                    tabu_list[i, j] = 0;
                }
            }
        }

        private int getVertexOfMinimumDistance(int x)
        {
            int min = Int32.MaxValue;
            int result = 0;
            for (int i = 1; i < visited.Length; i++)
            {
                if (visited[i]) continue;
                if (instance.Data[x, i] < min)
                {
                    min = instance.Data[x, i];
                    result = i;
                }
            }
            visited[result] = true;
            return result;
        }

        public TSPRoute GetGreedySolution()
        {
            TSPRoute result = new TSPRoute();
            List<int> route = new List<int>();
            route.Add(0);
            int x = 0;
            int y;
            do
            {
                y = getVertexOfMinimumDistance(x);
                route.Add(y);
                x = y;
            } while (y != 0);

            result.Route = route;
            result.Distance = calculateDistance(route.ToArray());


            clearVisitedArray();
            return result;
        }

        private int calculateDistance(int[] route)
        {
            int sum = 0;
            for (int i = 0; i < route.Length - 1; i++)
            {
                sum += instance.Data[route[i], route[i + 1]];
            }

            return sum;
        }

        private void decrementTabuList()
        {
            for (int i = 0; i < tabu_list.GetLength(0); i++)
            {
                for (int j = 0; j < tabu_list.GetLength(1); j++)
                {
                    if (tabu_list[i, j] > 0)
                        tabu_list[i, j]--;
                }
            }
        }

        private void swap(int[] route, int i, int j)
        {
            int temp = route[i];
            route[i] = route[j];
            route[j] = temp;
        }

        private TSPRoute getRandomRoute()
        {
            TSPRoute result = new TSPRoute();
            Random rnd = new Random();

            int[] numbers = new int[instance.Dimension - 1];
            for (int i = 0; i < numbers.Length; i++)
                numbers[i] = i + 1;

            var randomShuffle = numbers.OrderBy(x => rnd.Next()).ToArray();

            result.InitRouteFromPermutation(randomShuffle);


            var route = new List<int>();
            route.Add(0); route.AddRange(randomShuffle); route.Add(0);
            result.Distance = calculateDistance(route.ToArray());
            return result;
        }

        private void invert(int[] route, int i, int j)
        {
            Array.Reverse(route, i, j - i + 1);
        }

        private void edgeSwap(int[] route, int i, int j)
        {
            if (j == instance.Dimension - 1 || i == instance.Dimension - 1)
                return;

            swap(route, i, j);
            swap(route, i + 1, j + 1);
        }

        private bool AspirationCriterium(int value, int globalBest)
        {
            return value < globalBest;
        }


        private TSPRoute GetBestNearbySolution(TSPRoute initial, int globalBest)
        {
            int besti = 1, bestj = 2;
            int[] currentRoute = initial.Route.ToArray();
            int[] bestRoute = new int[currentRoute.Length];
            Array.Copy(currentRoute, bestRoute, currentRoute.Length);
            int currentBest = calculateDistance(currentRoute);
            int temp;
            for (int i = 1; i < instance.Dimension; i++)
            {
                for (int j = i + 1; j < instance.Dimension; j++)
                {
                    neighbour(currentRoute, i, j);
                    temp = calculateDistance(currentRoute);
                    if (temp < currentBest && (tabu_list[currentRoute[i], currentRoute[j]] == 0 || AspirationCriterium(temp, globalBest)))                        //Kryterium aspiracji
                    {
                        currentBest = temp;
                        Array.Copy(currentRoute, bestRoute, currentRoute.Length);
                        besti = currentRoute[i];
                        bestj = currentRoute[j];
                    }
                    neighbour(currentRoute, i, j);
                }
            }

            if (tabu_list[besti, bestj] == 0)
            {
                tabu_list[besti, bestj] += tenure;
                tabu_list[bestj, besti] += tenure;
            }

            TSPRoute result = new TSPRoute();
            result.Route = bestRoute.ToList();
            result.Distance = currentBest;

            return result;
        }

        public void GetTabuSearchSolution()
        {
            TSPRoute bestSolution = getRandomRoute();
            TSPRoute solution = new TSPRoute();
            solution.Route = new List<int>(bestSolution.Route);
            solution.Distance = bestSolution.Distance;
            int restartCounter = 0;


            watch = Stopwatch.StartNew();
            for (int i = 0; i < NUM_ITERATION; i++)
            {
                solution = GetBestNearbySolution(solution, bestSolution.Distance);
                if (solution.Distance < bestSolution.Distance)
                {
                   // Console.WriteLine($"Stara watosc: {bestSolution.Distance}, nowa wartosc:{solution.Distance}");
                    bestSolution = solution;
                    _results.Add(bestSolution);
                    restartCounter = 0;
                }
                decrementTabuList();
                restartCounter++;
                if (restartCounter > QUIT && diversification)
                {
                    // Console.WriteLine($"i = {i}, Local Best po: {solution.Distance} Current best: {bestSolution.Distance}");
                    solution = getRandomRoute();
                    restartTabu();
                    //  Console.WriteLine($"i = {i}, Local Best przed: {solution.Distance} Current best: {bestSolution.Distance}");
                    restartCounter = 0;
                }
                if (CriticalEvent())
                {
                    watch.Stop();
                    break;
                }
            }

            //    Console.WriteLine($"i = {NUM_ITERATION}, Local Best: {solution.Distance} Current best: {bestSolution.Distance}");
           // return bestSolution;
        }

        private bool CriticalEvent()
        {
            if (watch.ElapsedMilliseconds > seconds * 1000)
                return true;

            return false;
        }



        void restartTabu()
        {
            tabu_list = new int[instance.Dimension, instance.Dimension];
        }
    }
}
