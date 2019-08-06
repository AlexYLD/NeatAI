using System;
using System.Collections.Generic;
using System.Windows;
using NeatNet.NEAT.BasicEntities;
using NeatNet.NEAT.ComplexEntities;
using NeatNet.View;

namespace NeatNet.Snake
{
    public class Game
    {
        private static Random rnd = new Random();
        private Dot[,] _field;
        public LinkedList<Dot> _snake = new LinkedList<Dot>();
        private Dot _apple;
        public Genom Brain;
        public int Distance;
        public int AppleScore;
        public MainWindow Window;

        public Game(Genom brain, MainWindow window)
        {
            int fieldSide = 30;
            Window = window;
            if (Window != null)
            {
                _field = Window.field;
            }
            else
            {
                _field = new Dot[fieldSide, fieldSide];
                for (int i = 0; i < fieldSide; i++)
                {
                    for (int j = 0; j < fieldSide; j++)
                    {
                        Dot dot = new Dot(j, i, null);
                        _field[j, i] = dot;
                    }
                }
            }

            Brain = brain;


            for (int i = 0; i < 4; i++)
            {
                _snake.AddLast(_field[fieldSide / 2 + i, fieldSide / 2]);

                ChangeDotStatus(_snake.Last.Value, DotStatus.Snake);
            }

            while (_apple == null || _snake.Contains(_apple))
            {
                int rndX = rnd.Next(fieldSide);
                int rndY = rnd.Next(fieldSide);
                _apple = _field[rndX, rndY];
            }


            ChangeDotStatus(_apple, DotStatus.Apple);
        }

        private void ChangeDotStatus(Dot dot, DotStatus status)
        {
            dot.status = status;
            Window?.PaintDot(dot, status);
        }


        public bool move()
        {
            int fieldSide = _field.GetLength(0);
            Dot head = _snake.First.Value;
            Dot neck = _snake.First.Next.Value;
            int delX = head.X - neck.X;
            int delY = head.Y - neck.Y;

            SetInputs(fieldSide, head, delY, delX);


            double leftOut = Brain.Outputs[0].GetValue();
            double rightOut = Brain.Outputs[1].GetValue();
            double straightOut = Brain.Outputs[2].GetValue();
            //Console.WriteLine($@"Up:{upOut} Down {downOut} Left {leftOut} Right {rightOut}");
            foreach (Node output in Brain.Outputs)
            {
                output.reset();
            }

            double max = Math.Max(leftOut, Math.Max(rightOut, straightOut));
            Dot nextDot;

            if (max == leftOut)
            {
                nextDot = new Dot(head.X + delY, head.Y + delX);
            }
            else if (max == rightOut)
            {
                nextDot = new Dot(head.X - delY, head.Y - delX);
            }
            else
            {
                nextDot = new Dot(head.X + delX, head.Y + delY);
            }


            if (nextDot.X >= fieldSide || nextDot.Y >= fieldSide ||
                nextDot.X < 0 || nextDot.Y < 0)
            {
                SetScore();
                return false;
            }

            nextDot = _field[nextDot.X, nextDot.Y];

            if (_snake.Contains(nextDot) && !nextDot.Equals(_snake.Last.Value))
            {
                SetScore();
                return false;
            }


            if (!nextDot.Equals(_apple))
            {
                ChangeDotStatus(_snake.Last.Value, DotStatus.Field);
                _snake.RemoveLast();
            }
            else
            {
                _apple = null;
                while (_apple == null || _snake.Contains(_apple))
                {
                    int rndX = rnd.Next(fieldSide);
                    int rndY = rnd.Next(fieldSide);
                    _apple = _field[rndX, rndY];
                }

                ChangeDotStatus(_apple, DotStatus.Apple);
                AppleScore++;
            }

            _snake.AddFirst(nextDot);
            ChangeDotStatus(nextDot, DotStatus.Snake);


            Distance++;
            if (Distance / 120 > AppleScore)
            {
                SetScore();
                return false;
            }

            return true;
        }

        private void SetScore()
        {
            Brain.Fitness = AppleScore * 1000;// + (AppleScore < 15 ? Distance : -Distance);
        }

        private void SetInputs(double fieldSide, Dot head, int delY, int delX)
        {
            /*for (int i = 1; i <= fieldSide; i++)
            {
                if (head.Y + delY * i >= fieldSide || head.X + delX * i >= fieldSide ||
                    head.Y + delY * i < 0 || head.X + delX * i < 0 ||
                    _field[head.X + delX * i, head.Y + delY * i].status.Equals(DotStatus.Snake))
                {
                    Brain.Inputs[0].OldValue = i - 1;
                    break;
                }

                if (_field[head.X + delX * i, head.Y + delY * i].status.Equals(DotStatus.Apple))
                {
                    Brain.Inputs[0].OldValue = -(fieldSide - 1) + i;
                    break;
                }
            }

            for (int j = 1; j <= 2; j++)
            {
                for (int i = 1; i <= fieldSide; i++)
                {
                    if (head.Y + delX * i >= fieldSide || head.X + delY * i >= fieldSide ||
                        head.Y + delX * i < 0 || head.X + delY * i < 0 ||
                        _field[head.X + delY * i, head.Y + delX * i].status.Equals(DotStatus.Snake))
                    {
                        Brain.Inputs[j].OldValue = i - 1;
                        break;
                    }

                    if (_field[head.X + delY * i, head.Y + delX * i].status.Equals(DotStatus.Apple))
                    {
                        Brain.Inputs[j].OldValue = -(fieldSide - 1) + i;
                        break;
                    }
                }

                delX = -delX;
                delY = -delY;
            }

            int sumY = -(delX + delY);
            int sumX = sumY;
            for (int j = 3; j <= 6; j++)
            {
                for (int i = 1; i <= fieldSide * 2; i++)
                {
                    if (head.Y + sumY * i >= fieldSide || head.X + sumX * i >= fieldSide ||
                        head.Y + sumY * i < 0 || head.X + sumX * i < 0 ||
                        _field[head.X + sumX * i, head.Y + sumY * i].status.Equals(DotStatus.Snake))
                    {
                        Brain.Inputs[j].OldValue = i - 1;
                        break;
                    }

                    if (_field[head.X + sumX * i, head.Y + sumY * i].status.Equals(DotStatus.Apple))
                    {
                        Brain.Inputs[j].OldValue = -(fieldSide - 1) + i;
                        break;
                    }
                }

                if (j == 3 || j == 5)
                {
                    sumX = -sumX;
                }

                if (j == 4)
                {
                    sumX = -sumX;
                    sumY = -sumY;
                }
            }*/
            Brain.Inputs[0].OldValue = head.Y + delY >= fieldSide || head.X + delX >= fieldSide ||
                                       head.Y + delY < 0 || head.X + delX < 0 ||
                                       _field[head.X + delX, head.Y + delY].status.Equals(DotStatus.Snake)
                ? 1
                : 0;
            Brain.Inputs[1].OldValue = head.Y + delX >= fieldSide || head.X + delY >= fieldSide ||
                                       head.Y + delX < 0 || head.X + delY < 0 ||
                                       _field[head.X + delY, head.Y + delX].status.Equals(DotStatus.Snake)
                ? 1
                : 0;
            Brain.Inputs[2].OldValue = head.Y - delX >= fieldSide || head.X - delY >= fieldSide ||
                                       head.Y - delX < 0 || head.X - delY < 0 ||
                                       _field[head.X - delY, head.Y - delX].status.Equals(DotStatus.Snake)
                ? 1
                : 0;
            Brain.Inputs[3].OldValue = head.X == _apple.X ? 0 : head.X > _apple.X ? 1 : -1;
            Brain.Inputs[4].OldValue = head.Y == _apple.Y ? 0 : head.Y > _apple.Y ? 1 : -1;
            Brain.Inputs[5].OldValue = delX;
            Brain.Inputs[6].OldValue = delY;
        }

        public void clearField()
        {
            foreach (Dot dot in _snake)
            {
                ChangeDotStatus(dot, DotStatus.Field);
            }

            ChangeDotStatus(_apple, DotStatus.Field);
        }
    }
}