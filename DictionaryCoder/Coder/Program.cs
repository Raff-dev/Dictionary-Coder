using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace DictionaryCoder.Coder
{
    class Program
    {

        static void Main(string[] args)
        {
            Coder Coder = new Coder(10, 5);
            Coder.EncodeLZ77("text.txt", "output.txt");
        }
    }
}

