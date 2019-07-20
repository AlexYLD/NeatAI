using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NeatNet.NEAT.ComplexEntities;
using NeatNet.Snake;

namespace NeatNet.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        List<Dot> field = new List<Dot>();
        private const int fieldSide = 30;

        public MainWindow()
        {
            InitializeComponent();
            for (int i = 0; i < fieldSide; i++)
            {
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition());
                MainGrid.RowDefinitions.Add(new RowDefinition());
            }

            for (int i = 0; i < fieldSide; i++)
            {
                for (int j = 0; j < fieldSide; j++)
                {
                    Label dotLabel = new Label()
                        {Background = new SolidColorBrush((j + i) % 2 == 0 ? Colors.LightGray : Colors.White)};
                    Dot dot = new Dot(j, i, dotLabel);
                    field.Add(dot);
                    Grid.SetColumn(dotLabel, j);
                    Grid.SetRow(dotLabel, i);
                    MainGrid.Children.Add(dotLabel);
                }
            }
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            new Thread(() =>
            {
                Population population = new Population(4, 4);
                Genom best = population.AllNets[0];
                int i = 0;
              
                while (true)
                {
                    foreach (Genom brain in population.AllNets)
                    {
                        //Console.WriteLine(brain.LocalLinks[0].Input + ";" + brain.LocalLinks[0].Output);
                        //Genom brain = best;
                        List<Dot> tempfield;
                        if (brain.Equals(best))
                        {
                            tempfield = field;
                        }
                        else
                        {
                            tempfield = new List<Dot>();
                            field.ForEach(d => tempfield.Add(new Dot(d.X, d.Y)));
                        }

                        Game game = new Game(tempfield, brain, this);
                        while (game.move())
                        {
                            if (game.Brain.Equals(best))
                            {
                                Thread.Sleep(100);
                            }
                        }

                        foreach (Dot dot in game._snake)
                        {
                            PaintDot(dot, DotStatus.Field);
                        }


                        PaintDot(game._apple, DotStatus.Field);
                        game.Brain.Fitness = game.Score;
                    }

                    population.NextGen();
                    best = population.GetBest(population.AllNets);
                    Console.WriteLine("gen:" + i++ + "Count: " + population.AllNets.Count);
                }
            }).Start();
        }

        public void PaintDot(Dot dot, DotStatus dotStatus)
        {
            dot.status = dotStatus;
            if (dot.Label == null)
            {
                return;
            }

            Dispatcher.Invoke(() =>
            {
                switch (dotStatus)
                {
                    case DotStatus.Apple:
                        dot.Label.Background = new SolidColorBrush(Colors.Red);
                        break;
                    case DotStatus.Snake:
                        dot.Label.Background = new SolidColorBrush(Colors.Black);
                        break;
                    case DotStatus.Field:
                        dot.Label.Background =
                            new SolidColorBrush((dot.X + dot.Y) % 2 == 0 ? Colors.LightGray : Colors.White);
                        break;
                }
            });
        }
    }
}