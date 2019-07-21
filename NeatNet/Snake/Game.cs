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
        private List<Dot> _field;
        public LinkedList<Dot> _snake = new LinkedList<Dot>();
        public Dot _apple;
        public Genom Brain;
        public int Score;
        public MainWindow Window;

        public Game(List<Dot> field, Genom brain, MainWindow window)
        {
            Window = window;
            _field = field;
            Brain = brain;
            int fieldSide = (int) Math.Sqrt(_field.Count);
            for (int i = 0; i < 4; i++)
            {
                _snake.AddLast(field.Find(d => d.X == fieldSide / 2 + i && d.Y == fieldSide / 2));
                if (_snake.Last.Value.Label != null)
                {
                    window.PaintDot(_snake.Last.Value, DotStatus.Snake);
                }
            }

            int rndX = rnd.Next(fieldSide);
            int rndY = rnd.Next(fieldSide / 2);
            _apple = _field.Find(d => d.X == rndX && d.Y == rndY);

            if (_apple.Label != null)
            {
                window.PaintDot(_apple, DotStatus.Apple);
            }
        }

        public bool move()
        {
            Dot head = _snake.First.Value;
            Brain.Inputs[0].OldValue = head.X;
            Brain.Inputs[1].OldValue = head.Y;
            Brain.Inputs[2].OldValue = _apple.X;
            Brain.Inputs[3].OldValue = _apple.Y;
            
            double leftOut = Brain.Outputs[0].GetValue();
            double rightOut = Brain.Outputs[1].GetValue();
            double upOut = Brain.Outputs[2].GetValue();
            double downOut = Brain.Outputs[3].GetValue();
            //Console.WriteLine($@"Up:{upOut} Down {downOut} Left {leftOut} Right {rightOut}");
            foreach (Node output in Brain.Outputs)
            {
                output.reset();
            }

            double max = Math.Max(leftOut, Math.Max(rightOut, Math.Max(upOut, downOut)));
            Dot nextDot;

            if (max == leftOut)
            {
                nextDot = new Dot(head.X - 1, head.Y);
                if (nextDot.Equals(_snake.First.Next.Value))
                {
                    nextDot = new Dot(head.X + 1, head.Y);
                }
            }
            else if (max == rightOut)
            {
                nextDot = new Dot(head.X + 1, head.Y);
                if (nextDot.Equals(_snake.First.Next.Value))
                {
                    nextDot = new Dot(head.X - 1, head.Y);
                }
            }
            else if (max == upOut)
            {
                nextDot = new Dot(head.X, head.Y - 1);
                if (nextDot.Equals(_snake.First.Next.Value))
                {
                    nextDot = new Dot(head.X, head.Y + 1);
                }
            }
            else
            {
                nextDot = new Dot(head.X, head.Y + 1);
                if (nextDot.Equals(_snake.First.Next.Value))
                {
                    nextDot = new Dot(head.X, head.Y - 1);
                }
            }

            if (!_field.Contains(nextDot))
            {
                return false;
            }

            nextDot = _field.Find(d => d.Equals(nextDot));

            /*if (!nextDot.Equals(_snake.Last.Value) && _snake.Contains(nextDot))
            {
                return false;
            }*/

            _snake.AddFirst(nextDot);
            if (nextDot.Label != null)
            {
                Window.PaintDot(nextDot, DotStatus.Snake);
            }

            if (!nextDot.Equals(_apple))
            {
                if (_snake.Last.Value.Label != null)
                {
                    Window.PaintDot(_snake.Last.Value, DotStatus.Field);
                }

                _snake.RemoveLast();
            }
            else
            {
                int rndX = rnd.Next(15);
                int rndY = rnd.Next(15);
                _apple = _field.Find(d => d.X == rndX && d.Y == rndY);

                if (_apple.Label != null)
                {
                    Window.PaintDot(_apple, DotStatus.Apple);
                }
                Score += 10;
            }

            Score++;
            return true;
        }
    }
}