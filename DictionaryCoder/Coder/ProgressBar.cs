using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Coder
{
    class ProgressBar
    {
        private ConsoleColor[] colors = new ConsoleColor[] {
            ConsoleColor.DarkRed, ConsoleColor.Red,
            ConsoleColor.DarkBlue, ConsoleColor.DarkBlue,
            ConsoleColor.DarkGreen, ConsoleColor.Green
        };
        public int OffsetY { get; set; }
        public List<string> TitleMessages { get; set; }
        public List<string> FinishMessages { get; set; }
        public float MaxValue { get; set; }

        private float _value;
        public float Value
        {
            get { return _value; }
            set
            {
                _value = value;
                Display();
            }
        }
        public int StepsCount { get; set; }

        public ProgressBar(float maxValue = 100, int stepsCount = 50)
        {
            MaxValue = maxValue;
            _value = 0;
            StepsCount = stepsCount;
        }

        public void Display()
        {
            List<string> messages = new List<string>();
            try
            {
                int offsetY = OffsetY;
                string progressBar = "[";
                for (int step = 1; step <= StepsCount; step++) progressBar += IsCompleted(step, _value) ? '-' : ' ';
                progressBar += "]";

                messages.AddRange(TitleMessages);
                messages.Add(progressBar);
                messages.Add(GetProgressMessage(_value));
                if (_value >= MaxValue) messages.AddRange(FinishMessages);

                Console.CursorVisible = false;

                foreach (string message in messages)
                {
                    if (message == progressBar) Console.ForegroundColor = GetColor(_value);
                    else Console.SetCursorPosition(GetOffsetToCenter(message), offsetY);
                    Console.WriteLine(message);
                    Console.ResetColor();
                    offsetY++;
                }
            }
            catch (Exception e)
            {
                Console.SetCursorPosition(0, OffsetY + messages.Count + 2);
                Console.WriteLine("Progressbar exception" + e.StackTrace);
            }
        }

        private string GetProgressMessage(float value)
        {
            return $"{(int)(value / MaxValue * 100)}% Completed";
        }

        private int GetOffsetToCenter(string text)
        {
            int offset = (StepsCount + 2 - text.Length) / 2;
            return offset > 0 ? offset : 0;
        }

        private bool IsCompleted(float step, float value)
        {
            return value / MaxValue >= step / StepsCount;
        }

        private ConsoleColor GetColor(float value)
        {
            int index = value == 0 ? 0 : (int)Math.Ceiling(value / MaxValue * colors.Length) - 1;
            index = index > 5 ? 5 : index;
            return colors[index];
        }
    }
}
