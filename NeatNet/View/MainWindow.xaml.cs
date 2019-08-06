using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using Microsoft.Win32;
using NeatNet.NEAT.ComplexEntities;
using NeatNet.NEAT.Utils;
using NeatNet.Snake;
using Button = System.Windows.Controls.Button;
using Label = System.Windows.Controls.Label;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using TextBox = System.Windows.Controls.TextBox;

namespace NeatNet.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const int fieldSide = 30;
        public Dot[,] field = new Dot[fieldSide, fieldSide];
        private int _timeout = 20;
        private bool _toShow = true;
        private bool _newPop = false;
        private Genom _best;
        private Thread _mainRun;

        public MainWindow()
        {
            InitializeComponent();
            for (int i = 0; i < fieldSide; i++)
            {
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition());
                MainGrid.RowDefinitions.Add(new RowDefinition());
            }

            MainGrid.RowDefinitions.Add(new RowDefinition() {Height = GridLength.Auto});
            TextBox speed = new TextBox {Text = "10"};
            Grid.SetRow(speed, fieldSide + 1);
            Grid.SetColumn(speed, 0);
            Grid.SetColumnSpan(speed, 2);
            MainGrid.Children.Add(speed);
            Button update = new Button {Content = "Change T"};
            Grid.SetRow(update, fieldSide + 1);
            Grid.SetColumn(update, 3);
            Grid.SetColumnSpan(update, 3);
            MainGrid.Children.Add(update);
            update.Click += (e, s) => _timeout = int.Parse(speed.Text);

            Button showButton = new Button {Content = "Show"};
            Grid.SetRow(showButton, fieldSide + 1);
            Grid.SetColumn(showButton, 7);
            Grid.SetColumnSpan(showButton, 3);
            MainGrid.Children.Add(showButton);
            showButton.Click += (e, s) => _toShow = !_toShow;

            Button runButton = new Button {Content = "Run New"};
            Grid.SetRow(runButton, fieldSide + 1);
            Grid.SetColumn(runButton, 11);
            Grid.SetColumnSpan(runButton, 4);
            MainGrid.Children.Add(runButton);
            runButton.Click += (s, e) => RunNewPopulation(s);

            Button saveButton = new Button {Content = "Save"};
            Grid.SetRow(saveButton, fieldSide + 1);
            Grid.SetColumn(saveButton, 16);
            Grid.SetColumnSpan(saveButton, 3);
            MainGrid.Children.Add(saveButton);
            saveButton.Click += (s, e) => { SaveCurrentGenome(); };

            Button loadButton = new Button {Content = "Load"};
            Grid.SetRow(loadButton, fieldSide + 1);
            Grid.SetColumn(loadButton, 20);
            Grid.SetColumnSpan(loadButton, 3);
            MainGrid.Children.Add(loadButton);
            loadButton.Click += (s, e) => RunSavedGenom(s);

            for (int i = 0; i < fieldSide; i++)
            {
                for (int j = 0; j < fieldSide; j++)
                {
                    Label dotLabel = new Label
                    {
                        Background = new SolidColorBrush((j + i) % 2 == 0 ? Colors.LightGray : Colors.White),
                    };
                    Dot dot = new Dot(j, i, dotLabel);
                    field[j, i] = dot;
                    Grid.SetColumn(dotLabel, j);
                    Grid.SetRow(dotLabel, i);
                    MainGrid.Children.Add(dotLabel);
                }
            }
        }

        private void SaveCurrentGenome(string path = null)
        {
            if (path == null)
            {
                OpenFileDialog choofdlog = new OpenFileDialog();


                if (choofdlog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    path = choofdlog.FileName;
                }
            }

            ObjectCopier.Save(_best, path);
        }

        private void RunSavedGenom(object sender)
        {
            _newPop = true;
            ((Button) sender).IsEnabled = false;
            new Thread(() =>
            {
                _mainRun?.Join();
                _newPop = false;
                _mainRun = new Thread(() =>
                {
                    string path = null;
                    Genom brain;
                    Dispatcher.Invoke(() =>
                    {
                        OpenFileDialog choofdlog = new OpenFileDialog();


                        if (choofdlog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            path = choofdlog.FileName;
                        }
                    });
                    if (!string.IsNullOrEmpty(path))
                    {
                        brain = ObjectCopier.Load<Genom>(path);
                    }
                    else
                    {
                        return;
                    }

                    while (!_newPop)
                    {
                        brain.Fitness = 0;
                        Game game = new Game(brain, _toShow ? this : null);

                        while (game.move())
                        {
                            if (_newPop)
                            {
                                game.clearField();
                                return;
                            }

                            if (_toShow)
                            {
                                Thread.Sleep(_timeout);
                            }
                        }

                        game.clearField();
                        Console.WriteLine("Score: " + brain.Fitness);
                    }
                });
                _mainRun.Start();
                Dispatcher.Invoke(() => ((Button) sender).IsEnabled = true);
            }).Start();
        }

        private void RunNewPopulation(object sender)
        {
            _newPop = true;
            ((Button) sender).IsEnabled = false;
            new Thread(() =>
            {
                _mainRun?.Join();
                _newPop = false;
                _mainRun = new Thread(() =>
                {
                    int gen = 0;
                    double bestScore = 0;
                    Population population = new Population(7, 3, true);
                    _best = population.AllNets[0];
                    while (!_newPop)
                    {
                        foreach (Genom brain in population.AllNets)
                        {
                            brain.Fitness = 0;
                            Game game = new Game(brain, brain.Equals(_best) && _toShow ? this : null);

                            while (game.move())
                            {
                                if (_newPop)
                                {
                                    game.clearField();
                                    return;
                                }

                                if (brain.Equals(_best) && _toShow)
                                {
                                    Thread.Sleep(_timeout);
                                }
                            }

                            if (brain.Equals(_best))
                            {
                                game.clearField();
                            }
                        }

                        _best = population.GetBest(population.AllNets);
                        if (_best.Fitness > 100000 && _best.Fitness > bestScore)
                        {
                            SaveCurrentGenome(@"C:\Users\ayarygi\RiderProjects\NeatAI\NeatNet\bin\Debug\best\" + _best.Fitness + "_" + gen);
                        }

                        Console.WriteLine("gen " + gen + " Count " + population.AllNets.Count + " spe: " + population._species.Count + " curBest: " + _best.Fitness + " Best " + bestScore);
                        if (bestScore < _best.Fitness)
                        {
                            bestScore = _best.Fitness;
                            if (bestScore > 10000)
                            {
                                //toShow = true;
                            }
                        }

                        gen++;

                        if (gen >= 1000)
                        {
                            Dispatcher.Invoke(() => RunNewPopulation(sender));
                        }

                        population.NextGen();
                    }
                });
                _mainRun.Start();
                Dispatcher.Invoke(() => ((Button) sender).IsEnabled = true);
            }).Start();
        }

        public void PaintDot(Dot dot, DotStatus dotStatus, string noBorder = null)
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