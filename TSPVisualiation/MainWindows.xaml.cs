using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TSPVisualiation.Models;
using TSPVisualiation.Models.TabuSearch;

namespace TSPVisualiation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TownGenerator _generator;
        private TSPInstance _instance = null;
        private AGSolver _agSolver = null;
        private TSSolver _tabuSolver = null;
        private List<Ellipse> _dotsAG = null;
        private List<Ellipse> _dotsTABU = null;

        private Neighbours _neighbourhood = Neighbours.INVERT;
        private MutationType _mutation = MutationType.Invert;
        private CrossBreedType _breedType = CrossBreedType.OX;

        private int _populationSize = 50;
        private int _time = 30;

        public MainWindow()
        {
            InitializeComponent();

            _generator = new TownGenerator()
            {
                Size = 10,
                Height = 100,
                 Width = 100
            };

        }


        private void GenerateDots()
        {
            _dotsAG = new List<Ellipse>();
            _dotsTABU = new List<Ellipse>();
            int dotSize = 5;
            foreach (var point in _generator.PointList)
            {
                Ellipse currentDot = new Ellipse();
                currentDot.Stroke = new SolidColorBrush(Colors.Black);
                currentDot.StrokeThickness = 3;
                Canvas.SetZIndex(currentDot, 3);
                currentDot.Height = dotSize;
                currentDot.Width = dotSize;
                currentDot.Fill = new SolidColorBrush(Colors.Black);
                currentDot.Margin = new Thickness(point.X, point.Y, 0, 0); // Sets the position.
                _dotsAG.Add(currentDot);
            }

            foreach (var point in _generator.PointList)
            {
                Ellipse currentDot = new Ellipse();
                currentDot.Stroke = new SolidColorBrush(Colors.Black);
                currentDot.StrokeThickness = 3;
                Canvas.SetZIndex(currentDot, 3);
                currentDot.Height = dotSize;
                currentDot.Width = dotSize;
                currentDot.Fill = new SolidColorBrush(Colors.Black);
                currentDot.Margin = new Thickness(point.X, point.Y, 0, 0); // Sets the position.
                _dotsTABU.Add(currentDot);
            }


        }

        private void drawDots(string algorithm)
        {
            int dotSize = 5;
            if (algorithm == "AG")
            {
                foreach (var dot in _dotsAG)
                    mainCanvas.Children.Add(dot);
            }
            else if (algorithm == "TABU")
            {
                foreach (var dot in _dotsTABU)
                    tabuCanvas.Children.Add(dot);
            }

        }


        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            mainCanvas.Children.Clear();
            tabuCanvas.Children.Clear();

            _generator.Size = Convert.ToInt32(SizeTextBox.Text) > 0 ? Convert.ToInt32(SizeTextBox.Text) : 10;
            var canvasSize = mainCanvas.ActualHeight < mainCanvas.ActualWidth ? mainCanvas.ActualHeight : mainCanvas.ActualWidth;
            _generator.Height = _generator.Width = Convert.ToInt32(canvasSize);
            _instance = _generator.GenerateInstance();

            GenerateDots();
            drawDots("AG");
            drawDots("TABU");
        }

        private void StartAG()
        {
            var solutions = new List<TSPRoute>();
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                _agSolver = new AGSolver(_instance, _populationSize, _mutation, _breedType, solutions, _time);
                _agSolver.GetAGSolution();
            }).Start();

            new Thread(() =>
            {
                while (true)
                {
                    if (solutions.Count > 0)
                    {
                        DrawLines(solutions[0], "AG");
                        solutions.RemoveAt(0);
                    }
                }
            }).Start();
        }

        private void StartTabu()
        {
            var solutions = new List<TSPRoute>();
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                _tabuSolver = new TSSolver(_instance, _neighbourhood, true,solutions, _time);
                _tabuSolver.GetTabuSearchSolution();
            }).Start();

            new Thread(() =>
            {
                while (true)
                {
                    if (solutions.Count > 0)
                    {
                        if(solutions[0] != null)
                            DrawLines(solutions[0], "TABU");
                        solutions.RemoveAt(0);
                    }
                }
            }).Start();
        }

        private void StartAlgoButton_Click(object sender, RoutedEventArgs e)
        {
            
            if(_instance != null)
            {
                StartAG();
                StartTabu();
            }

        }

        private void DrawLines(TSPRoute route, string algorithm)
        {
            Application.Current.Dispatcher.BeginInvoke(
              DispatcherPriority.Background, new Action(() =>
              {
                  if(algorithm == "AG")
                     mainCanvas.Children.Clear();
                  else if(algorithm == "TABU")
                      tabuCanvas.Children.Clear();

                  drawDots(algorithm);

                  if (algorithm == "AG")
                  {
                      for (int i = 1; i < (route.Route.Count); i++)
                      {
                          var line = new Line();
                          line.Stroke = Brushes.LightSteelBlue;
                          line.X1 = _generator.PointList[route.Route[i]].X;
                          line.X2 = _generator.PointList[route.Route[i - 1]].X;
                          line.Y1 = _generator.PointList[route.Route[i]].Y;
                          line.Y2 = _generator.PointList[route.Route[i - 1]].Y;
                          line.StrokeThickness = 2;
                          mainCanvas.Children.Add(line);
                      }
                      GeneticTextBlock.Text = $"GENETIC ALGORITHM RESULT: {route.Distance}";
                  }
                  else if (algorithm == "TABU")
                  {
                      for (int i = 1; i < (route.Route.Count); i++)
                      {
                          var line = new Line();
                          line.Stroke = Brushes.LightSteelBlue;
                          line.X1 = _generator.PointList[route.Route[i]].X;
                          line.X2 = _generator.PointList[route.Route[i - 1]].X;
                          line.Y1 = _generator.PointList[route.Route[i]].Y;
                          line.Y2 = _generator.PointList[route.Route[i - 1]].Y;
                          line.StrokeThickness = 2;
                          tabuCanvas.Children.Add(line);
                      }
                      TabuTextBlock.Text = $"TABU SEARCH RESULT: {route.Distance}";
                  }
                 
              }));
            Thread.Sleep(100);         
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb != null)
            {
                string neigh = rb.Tag.ToString();
                switch (neigh)
                {
                    case "SWAP":
                    {
                        _neighbourhood = Neighbours.SWAP;
                        break;
                    }
                    case "INVERT":
                    {
                            _neighbourhood = Neighbours.INVERT;
                            break;
                    }
                    case "SWAP_EDGE":
                        {
                            _neighbourhood = Neighbours.EDGE_SWAP;
                            break;
                        }
                }
            }
        }

        private void ToggleMutationButton_OnChecked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb != null)
            {
                string neigh = rb.Tag.ToString();
                switch (neigh)
                {
                    case "SWAP":
                    {
                        _mutation = MutationType.Transposition;
                            break;
                        }
                    case "INVERT":
                        {
                            _mutation = MutationType.Invert;
                            break;
                        }
                }
            }
        }

        private void ToggleCrossButton_OnChecked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb != null)
            {
                string neigh = rb.Tag.ToString();
                switch (neigh)
                {
                    case "OX":
                        {
                            _breedType = CrossBreedType.OX;
                            break;
                        }
                    case "PMX":
                        {
                            _breedType = CrossBreedType.PMX;
                            break;
                        }
                }
            }
        }

        private void CrossBreedRatioTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
           TextBox tb = sender as TextBox;
            if (tb != null)
            {
                double ratio = Convert.ToDouble(tb.Text);
                if (ratio > 0 && ratio < 1)
                {
                    AGSolver._crossBreedRatio = ratio;
                }
            }
        }

        private void MutationRatioTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb != null)
            {
                double ratio = Convert.ToDouble(tb.Text);
                if (ratio > 0 && ratio < 1)
                {
                    AGSolver._mutationRatio = ratio;
                }
            }
        }

        private void PopulationSizeTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb != null)
            {
                int popSize = Convert.ToInt32(tb.Text);
                _populationSize = popSize;
            }
        }

        private void TimeTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb != null)
            {
                int time = Convert.ToInt32(tb.Text);
                _time = time;
            }
        }
    }
}
