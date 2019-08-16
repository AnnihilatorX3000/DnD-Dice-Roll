using System;
using System.Collections.Generic;
using System.Linq;

namespace DnD_Dice_Roll
{
    class Program
    {
        static void Main(string[] args)
        {
            DisplayInstructions();


            ulong rangeHi, tempNo, totalComb;
            int noOfDice, rangeLo, prevDiceNo, currDiceNo, noOfAdditionalPairs;
            bool performSymmetry;
            List<string> diceTerm = new List<string>();
            Tuple<int, int> diceNoAndSide = new Tuple<int, int>(0, 0);
            List<string> tempList = new List<string>();
            List<int> diceSides = new List<int>();
            List<ulong> seqWorking = new List<ulong>();
            List<ulong> seqToCombine = new List<ulong>();
            List<ulong> seqOfOnes = new List<ulong>();

            while (true)
            {
                //Resetting variables
                diceTerm.Clear();
                diceSides.Clear();
                seqWorking.Clear();
                seqToCombine.Clear();
                seqOfOnes.Clear();
                noOfDice = 0;
                rangeHi = 0;
                tempNo = 0;
                totalComb = 1;
                rangeLo = 0;
                performSymmetry = false;

                try
                {
                    //Start of program - User Input
                    Console.Write("Enter your command: ");
                    diceTerm = Console.ReadLine().ToUpper().Split(' ').ToList();

                    //Splitting eg. 10D3 4D5

                    foreach (string term in diceTerm)
                    {
                        tempList = term.Split('D').ToList();
                        diceNoAndSide = new Tuple<int, int>(int.Parse(tempList[0]), int.Parse(tempList[1]));
                        noOfDice += diceNoAndSide.Item1;

                        //Obtaining range of numbers after summing up dice rolls and the total number of combinations
                        tempNo = (ulong)diceNoAndSide.Item2;
                        for (int i = 0; i < diceNoAndSide.Item1; i++)
                        {
                            diceSides.Add((int)tempNo);
                        }
                        rangeHi += (ulong)diceNoAndSide.Item1 * tempNo;
                        totalComb *= (ulong)Math.Pow(tempNo, diceNoAndSide.Item1);
                    }
                    rangeLo = noOfDice;

                    //Reordering list (descending) for more efficient computation
                    diceSides.Sort();
                    diceSides.Reverse();

                    //Generating first sequence
                    if (noOfDice == 1)
                    {
                        for (ulong i = (ulong)rangeLo; i <= rangeHi; i++)
                        {
                            seqWorking.Add(1);
                        }
                    }
                    else
                    {
                        //First two dice - Working sequence
                        prevDiceNo = diceSides[1];
                        currDiceNo = diceSides[0];
                        CalculatePairSeq(seqWorking, prevDiceNo, currDiceNo);

                        //Determine how many pairs
                        noOfAdditionalPairs = (int)Math.Floor((decimal)noOfDice / 2) - 1;

                        //Next pair to combine with working pair
                        for (int i = 0; i < noOfAdditionalPairs; i++)
                        {
                            prevDiceNo = diceSides[3 + (2 * i)];
                            currDiceNo = diceSides[2 + (2 * i)];
                            CalculatePairSeq(seqToCombine, prevDiceNo, currDiceNo);
   
                            //Perform symmetry operation on last pair if no 'remainder' dice
                            if (i == noOfAdditionalPairs - 1 && noOfDice % 2 == 0)
                            {
                                performSymmetry = true;
                            }

                            //Combine sequences
                            CombineSeq(seqWorking, seqToCombine, rangeLo, rangeHi, performSymmetry);

                            seqToCombine.Clear();
                        }

                        //For final (if available) die
                        if (noOfDice % 2 != 0)
                        {
                            for (int i = 1; i <= diceSides[diceSides.Count - 1]; i++)
                            {
                                seqOfOnes.Add(1);
                            }
                            performSymmetry = true;
                            CombineSeq(seqWorking, seqOfOnes, rangeLo, rangeHi, performSymmetry);
                        }
                    }

                    OutputResults(diceSides, seqWorking, rangeLo, rangeHi, totalComb);
                    Console.WriteLine();
                }
                catch
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("<<ERROR: You either screwed up somewhere or you put too many dice / sides>>");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine();
                }
            }          
        }
        




        static void DisplayInstructions()
        {
            Console.WriteLine("INSTRUCTIONS");
            Console.WriteLine("1. The number of dice must be >= 1");
            Console.WriteLine("2. Each dice must have >= 2 sides");
            Console.WriteLine("3. Command entry: '3d4 5d2' means 3 dice with 4 sides and 5 dice with 2 sides ");
            Console.WriteLine("(Note that inputting a large number of dice or sides may either lead to incorrect results due to integer overflow or it may not work at all due to a division by 0 error caused by large number of combinations of dice rolls)");
            Console.WriteLine();
        }

        static void CalculatePairSeq(List<ulong> seq, int prevDiceNo, int currDiceNo)
        {
            //Ascending to prevDiceNo by 1
            for (int i = 1; i < prevDiceNo; i++)
            {
                seq.Add((ulong)i);
            }

            //Repeating prevDiceNo       
            for (int i = 0; i <= currDiceNo - prevDiceNo; i++)
            {
                seq.Add((ulong)prevDiceNo);
            }

            //Descending from prevDiceNo to 1
            for (int i = prevDiceNo - 1; i > 0; i--)
            {
                seq.Add((ulong)i);
            }
        }

        /*
        static void CombineSeq(List<ulong> seqWorking, List<ulong> seqToCombine)
        {
            List<ulong> tempList = new List<ulong>(seqWorking);
            int tempNo = seqWorking.Count;
            for (int j = 1; j < seqToCombine.Count; j++)
            {
                for (int i = 0; i < tempNo; i++)
                {
                    //To prevent accessing undefined index on offset sum
                    if (j + i >= seqWorking.Count)
                    {
                        seqWorking.Add(seqToCombine[j] * tempList[i]);
                    }
                    else
                    {
                        seqWorking[j + i] += seqToCombine[j] * tempList[i];
                    }
                }
            }
        }
        */

        static void CombineSeq(List<ulong> seqWorking, List<ulong> seqToCombine, int rangeLo, ulong rangeHi, bool performSymmetry)
        {
            List<ulong> tempList = new List<ulong>(seqWorking);
            //For symmetry (see below) - Still needed for normal operation
            int symmNo = (int)Math.Ceiling((decimal)((ulong)rangeLo + rangeHi + 1) / 2 - rangeLo);
            for (int j = 1; j < seqToCombine.Count; j++)
            {
                for (int i = 0; i < symmNo; i++)
                {
                    if (i >= tempList.Count)
                    {
                        //Prevents going out of bounds on tempList
                        seqWorking.Add(0);
                    }
                    else if (j + i >= seqWorking.Count)
                    {
                        //Prevents accessing undefined index on offset sum
                        seqWorking.Add(seqToCombine[j] * tempList[i]);
                    }
                    else
                    {
                        seqWorking[j + i] += (ulong)(seqToCombine[j] * tempList[i]);
                    }
                }
            }

            //Perform symmetry operation at the very end
            if (performSymmetry)
            {
                Symmetrize(seqWorking, rangeLo, rangeHi, symmNo);
            }    
        }

        static void Symmetrize(List<ulong> seq, int rangeLo, ulong rangeHi, int symmNo)
        {
            //Using symmetry
            int tempNo = (int)((rangeHi - (ulong)rangeLo) % 2);
            if (tempNo == 0)
            {
                //No repeated middle
                for (int i = symmNo; i < 2 * symmNo - 1; i++)
                {
                    //To prevent accessing undefined index on offset sum
                    if (i >= seq.Count)
                    {
                        seq.Add(seq[2 * symmNo - 2 - i]);
                    }
                    else
                    {
                        seq[i] = seq[2 * symmNo - 2 - i];
                    }
                }
            }
            else
            {
                //Has repeated middle
                for (int i = symmNo; i < 2 * symmNo; i++)
                {
                    //To prevent accessing undefined index on offset sum
                    if (i >= seq.Count)
                    {
                        seq.Add(seq[2 * symmNo - 1 - i]);
                    }
                    else
                    {
                        seq[i] = seq[2 * symmNo - 1 - i];
                    }
                }
            }
        }


        static void OutputResults(List<int> diceSides, List<ulong> seqWorking, int rangeLo, ulong rangeHi, ulong totalComb)
        {
            Console.WriteLine();

            Console.WriteLine("RESULTS:");
            Console.Write("Numbers in list: ");
            string numbersInList = "";
            foreach (ulong side in diceSides)
            {
                numbersInList += side + ", ";
            }
            Console.WriteLine(numbersInList);
            Console.WriteLine("Range of no.s: " + rangeLo + " to " + rangeHi);
            Console.WriteLine();

            Console.WriteLine("[Format: Number obtained after summing | Number of instances of that number | Percentage Chance]");
            ulong tempNo = 0;
            for (ulong i = (ulong)rangeLo; i <= rangeHi; i++)
            {
                tempNo = (ulong)seqWorking[(int)(i - (ulong)rangeLo)];
                Console.WriteLine(i + " | " + tempNo + " | " + ((decimal)tempNo / totalComb) * 100 + " %");
            }
        }
    }
}


