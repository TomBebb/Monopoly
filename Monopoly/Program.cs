using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Monopoly
{
    class Program
    {
        static void Main(string[] args)
        {
            var board = new ConsoleBoard();
            board.SetupPlayers();
            while(true)
            {
                board.DoTurn();
                Console.ReadLine();
            }
        }
    }
}
