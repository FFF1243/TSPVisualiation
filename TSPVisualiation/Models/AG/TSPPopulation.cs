using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSPVisualiation.Models.AG
{
    class TSPPopulation
    {
        private Random _randGenerator;
        private List<TSPRoute> _routes;
        private TSPInstance _instance;
        private int _populationSize;
        private static int _tournamentSize = 2;
        public static double _crossBreedRatio = 0.8;

        private delegate List<TSPRoute> CrossBreed(TSPRoute p, TSPRoute q);
        private CrossBreed Cross;


        public List<TSPRoute> Routes
        {
            get
            {
                return _routes;
            }

            set
            {
                _routes = value;
            }
        }

        public TSPPopulation(TSPInstance ins, int popSize, CrossBreedType crossType)
        {
            _randGenerator = new Random();
            _instance = ins;
            if (crossType == CrossBreedType.PMX)
            {
                Cross = Pmx;
            }
            else
            {
                Cross = Ox;
            }
            _populationSize = popSize;
            _routes = GenerateRandomPopulation(popSize);

        }

        public TSPPopulation(List<TSPRoute> bestRoutes, TSPInstance ins, int populationSize, CrossBreedType crossType)
        {
            _randGenerator = new Random();
            if (crossType == CrossBreedType.PMX)
            {
                Cross = Pmx;
            }
            else
            {
                Cross = Ox;
            }
            Routes = bestRoutes;
            _instance = ins;
            _populationSize = populationSize;

        }

        public TSPRoute GetBestRoute()
        {
            var result = _routes.OrderByDescending(x => x.FitnessValue).FirstOrDefault();
            return result;
        }

        public List<TSPRoute> TournamentSelection()
        {
            List<TSPRoute> selectedRoutes = new List<TSPRoute>();
            var allRoutes = new List<TSPRoute>(_routes);
            do
            {
                var randomRoutes = allRoutes.OrderBy(x => _randGenerator.Next()).
                    Take(_tournamentSize).ToList();
                var selectedRoute = randomRoutes.OrderByDescending(x => x.FitnessValue).First();

                if (!selectedRoutes.Contains(selectedRoute))
                    selectedRoutes.Add(selectedRoute);

                allRoutes.Remove(selectedRoute);
            } while (selectedRoutes.Count < 2);
            return selectedRoutes;
        }

        public List<TSPRoute> RouletteSelection()
        {
            List<TSPRoute> selectedRoutes = new List<TSPRoute>();
            // var allRoutes = new List<TSPRoute>(_routes);

            for (int j = 0; j < 2; j++)
            {
                var sum = _routes.Sum(x => x.FitnessValue);
                var normalizedDic =
                    _routes.ToDictionary(x => x, x => (x.FitnessValue / sum)).OrderByDescending(t => t.Value).ToDictionary(t => t.Key, t => t.Value);
                var acuumulatedDic = new Dictionary<TSPRoute, double>();
                int i = 1;

                foreach (var keyValuePair in normalizedDic)
                {
                    var fitnessSum = normalizedDic.Take(i).Sum(x => x.Value);
                    acuumulatedDic.Add(keyValuePair.Key, fitnessSum);
                    i++;
                }
                var randNumber = _randGenerator.NextDouble();
                var selectedRoute = acuumulatedDic.LastOrDefault(x => x.Value < randNumber);
                if (selectedRoute.Key == null)
                {
                    selectedRoute = acuumulatedDic.First();
                }
                //allRoutes.Remove(selectedRoute.Key);
                if (selectedRoutes.Contains(selectedRoute.Key))
                {
                    j--;
                }
                else
                {
                    selectedRoutes.Add(selectedRoute.Key);
                }
            }

            return selectedRoutes;
        }

        public List<TSPRoute> StartReproduction()
        {
            var childs = new List<TSPRoute>();
            do
            {
                List<TSPRoute> parents = TournamentSelection();
                if (_randGenerator.NextDouble() < _crossBreedRatio)
                {
                    List<TSPRoute> newChilds = new List<TSPRoute>();
                    do
                    {
                        newChilds = Cross(parents[0], parents[1]);
                    } while (childs.Intersect(newChilds).Any());
                    childs.AddRange(newChilds);
                }
                else
                {
                    int i = 0;
                    //   parents = TournamentSelection();
                    while (childs.Intersect(parents).Any())
                    {
                        if (i > 100)
                            break;
                        parents = TournamentSelection();
                        i++;
                    }
                    childs.AddRange(parents);
                }
            } while (childs.Count < _populationSize);

            return childs;
        }

        private int GetMappedInteger(Dictionary<int, int> dic, int value, List<int> forbbiden, int lastValue)
        {
            int returnValue = value;

            if (dic.ContainsKey(value) && dic[value] != lastValue)
            {
                returnValue = dic[value];
                if (!forbbiden.Contains(returnValue))
                    return returnValue;
            }
            if (dic.ContainsValue(value) && dic.FirstOrDefault(x => x.Value == value).Key != lastValue)
            {
                returnValue = dic.FirstOrDefault(x => x.Value == value).Key;
                if (!forbbiden.Contains(returnValue))
                    return returnValue;
            }

            return GetMappedInteger(dic, returnValue, forbbiden, value);
        }

        public List<TSPRoute> Pmx(TSPRoute p, TSPRoute q)
        {
            int startIndex = _randGenerator.Next(1, _instance.Dimension - 2);
            int endIndex = _randGenerator.Next(startIndex + 1, _instance.Dimension - 1);

            /* startIndex = 3;
             endIndex = 7;
             _instance.Dimension = 10;*/

            var pMiddle = p.Route.GetRange(startIndex, endIndex - startIndex);
            var pRest = p.Route.GetRange(1, startIndex - 1);
            pRest.AddRange(p.Route.GetRange(endIndex, (_instance.Dimension - endIndex)));

            var qMiddle = q.Route.GetRange(startIndex, endIndex - startIndex);
            var qRest = q.Route.GetRange(1, startIndex - 1);
            qRest.AddRange(q.Route.GetRange(endIndex, (_instance.Dimension - endIndex)));




            List<int> r = new List<int>(new int[_instance.Dimension + 1]);
            List<int> s = new List<int>(new int[_instance.Dimension + 1]);

            //INSERT MIDDLE TOWNS
            var mappingDic = new Dictionary<int, int>();
            for (int i = startIndex; i < endIndex; i++)
            {
                r[i] = qMiddle[i - startIndex];
                s[i] = pMiddle[i - startIndex];
                mappingDic.Add(r[i], s[i]);
            }

            for (int i = 1; i < r.Count - 1; i++)
            {
                if (i == startIndex)
                {
                    i += (endIndex - startIndex - 1);                           //-1?
                    continue;
                }

                if (!pMiddle.Contains(q.Route[i]))
                    s[i] = q.Route[i];
                else
                {
                    int value = q.Route[i];
                    if (value == 0)
                    {
                        Console.WriteLine("CO");
                    }
                    var forbbiden = new List<int>(pMiddle);
                    value = GetMappedInteger(mappingDic, value, forbbiden, 0);
                    s[i] = value;
                }

                if (!qMiddle.Contains(p.Route[i]))
                    r[i] = p.Route[i];
                else
                {
                    int value = p.Route[i];
                    if (value == 0)
                    {
                        Console.WriteLine("CO");
                    }
                    var forbbiden = new List<int>(qMiddle);
                    value = GetMappedInteger(mappingDic, value, forbbiden, 0);
                    r[i] = value;
                }
            }

            var rRoute = new TSPRoute()
            {
                Route = r,
                Distance = CalculateDistance(r.ToArray())
            };
            var sRoute = new TSPRoute()
            {
                Route = s,
                Distance = CalculateDistance(s.ToArray())
            };
            var result = new List<TSPRoute>();
            result.Add(rRoute); result.Add(sRoute);
            return result;
        }

        private List<TSPRoute> GenerateRandomPopulation(int populationSize)
        {
            var result = new List<TSPRoute>();
            for (int i = 0; i < populationSize; i++)
            {
                result.Add(getRandomRoute());
            }
            return result;
        }

        private void fillRestOfOXCross(int startIndex, int endIndex, TSPRoute parent, List<int> child, List<int> middle)
        {
            bool looped = false;
            int counter = endIndex;
            int j = endIndex;

            do
            {
                if (!middle.Contains(parent.Route[counter]))
                {
                    child[j] = parent.Route[counter];
                    j++;
                }
                counter++;
                if (counter == parent.Route.Count - 1)
                {
                    counter = 1;
                }
                if (j == parent.Route.Count - 1)
                {
                    j = 1;
                    looped = true;
                }
            } while (j < startIndex || !looped);

            //if(child.Distinct().Count() != 17)
            // Console.WriteLine("SCREAM");

        }

        public List<TSPRoute> Ox(TSPRoute p, TSPRoute q)
        {
            int startIndex = _randGenerator.Next(1, _instance.Dimension - 2);
            int endIndex = _randGenerator.Next(startIndex + 1, _instance.Dimension);

            /*startIndex = 3;
            endIndex = 7;
            _instance.Dimension = 10;*/

            var pMiddle = p.Route.GetRange(startIndex, endIndex - startIndex);
            var pRest = p.Route.GetRange(1, startIndex - 1);
            pRest.AddRange(p.Route.GetRange(endIndex, (_instance.Dimension - endIndex)));

            var qMiddle = q.Route.GetRange(startIndex, endIndex - startIndex);
            var qRest = q.Route.GetRange(1, startIndex - 1);
            qRest.AddRange(q.Route.GetRange(endIndex, (_instance.Dimension - endIndex)));

            List<int> r = new List<int>(new int[_instance.Dimension + 1]);
            List<int> s = new List<int>(new int[_instance.Dimension + 1]);

            //INSERT MIDDLE TOWNS
            for (int i = startIndex; i < endIndex; i++)
            {
                r[i] = qMiddle[i - startIndex];
                s[i] = pMiddle[i - startIndex];
            }
            //INSERT REST OF TOWNS
            fillRestOfOXCross(startIndex, endIndex, p, r, qMiddle);
            fillRestOfOXCross(startIndex, endIndex, q, s, pMiddle);

            var rRoute = new TSPRoute()
            {
                Route = r,
                Distance = CalculateDistance(r.ToArray())
            };
            var sRoute = new TSPRoute()
            {
                Route = s,
                Distance = CalculateDistance(s.ToArray())
            };
            var result = new List<TSPRoute>();
            result.Add(rRoute); result.Add(sRoute);
            return result;
        }

        public int CalculateDistance(int[] route)
        {
            int sum = 0;
            for (int i = 0; i < route.Length - 1; i++)
            {
                sum += _instance.Data[route[i], route[i + 1]];
            }

            return sum;
        }
        private TSPRoute getRandomRoute()
        {
            TSPRoute result = new TSPRoute();


            int[] numbers = new int[_instance.Dimension - 1];
            for (int i = 0; i < numbers.Length; i++)
                numbers[i] = i + 1;

            var randomShuffle = numbers.OrderBy(x => _randGenerator.Next()).ToArray();

            result.InitRouteFromPermutation(randomShuffle);


            var route = new List<int>();
            route.Add(0); route.AddRange(randomShuffle); route.Add(0);
            result.Distance = CalculateDistance(route.ToArray());
            return result;
        }
    }
}
