using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrabalhoSocketsEngine;

namespace TrabalhoSockets
{
    class Program
    {
        static void Main(string[] args)
        {
            var g = new GameBoard();
            g.Print();
            Console.ReadLine();
        }
    }
}
