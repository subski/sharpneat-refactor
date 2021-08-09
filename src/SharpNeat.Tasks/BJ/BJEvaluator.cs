/* ***************************************************************************
 * This file is part of SharpNEAT - Evolution of Neural Networks.
 *
 * Copyright 2004-2020 Colin Green (sharpneat@gmail.com)
 *
 * SharpNEAT is free software; you can redistribute it and/or modify
 * it under the terms of The MIT License (MIT).
 *
 * You should have received a copy of the MIT License
 * along with SharpNEAT; if not, see https://opensource.org/licenses/MIT.
 */
using System;
using System.Diagnostics;
using System.Linq;
using SharpNeat.BlackBox;
using SharpNeat.Evaluation;

namespace SharpNeat.Tasks.BJ
{
    /// <summary>
    /// Evaluator for the logical XOR task.
    ///
    /// Two inputs supply the two XOR input values.
    ///
    /// The correct response for the single output is input1 XOR input2.
    ///
    /// Evaluation consists of querying the provided black box for all possible input combinations (2^2 = 4).
    /// </summary>
    public sealed class BJEvaluator : IPhenomeEvaluator<IBlackBox<double>>
    {
        #region Public Methods

        private readonly int[][] tableLabelInput;
        private readonly int[][] tableLabelOutput;
        private readonly Random rand = new Random();

        /// <summary>
        /// Initialize the blackjack reference table.
        /// </summary>
        public BJEvaluator()
        {
            tableLabelInput = new int[1000][];
            tableLabelOutput = new int[1000][];

            int i = 0;
            for (int row = 1; row <= 10; row++)
            {
                for (int c1 = 1; c1 <= 10; c1++)
                {
                    for (int c2 = 1; c2 <= 10; c2++)
                    {
                        int[] input = new int[] { 1, c1, c2, c1 + c2, row };
                        int[] output;

                        int[] pl_hand = new int[] { c1, c2 };
                        Array.Sort(pl_hand);

                        if (Enumerable.SequenceEqual(pl_hand, new int[] { 1, 10 }))
                        {
                            output = new int[] { 1, 0, 0, 0 };
                        }
                        else if (c1 == 1 && c2 == 1)
                        {
                            output = new int[] { 0, 0, 1, 0 };
                        }
                        else if (Enumerable.SequenceEqual(pl_hand, new int[] { 1, 2 }) || Enumerable.SequenceEqual(pl_hand, new int[] { 1, 3 }))
                        {
                            if (row >= 5 && row <= 6)
                                output = new int[] { 0, 0, 0, 1 };
                            else
                                output = new int[] { 0, 1, 0, 0 };
                        }
                        else if (Enumerable.SequenceEqual(pl_hand, new int[] { 1, 4 }) || Enumerable.SequenceEqual(pl_hand, new int[] { 1, 5 }))
                        {
                            if (row >= 4 && row <= 6)
                                output = new int[] { 0, 0, 0, 1 };
                            else
                                output = new int[] { 0, 1, 0, 0 };
                        }
                        else if (Enumerable.SequenceEqual(pl_hand, new int[] { 1, 6 }))
                        {
                            if (row >= 3 && row <= 6)
                                output = new int[] { 0, 0, 0, 1 };
                            else
                                output = new int[] { 0, 1, 0, 0 };
                        }
                        else if (Enumerable.SequenceEqual(pl_hand, new int[] { 1, 7 }))
                        {
                            if (row >= 2 && row <= 8)
                                output = new int[] { 1, 0, 0, 0 };
                            else
                                output = new int[] { 0, 1, 0, 0 };
                        }
                        else if (Enumerable.SequenceEqual(pl_hand, new int[] { 1, 8 }) || Enumerable.SequenceEqual(pl_hand, new int[] { 1, 9 }))
                        {
                            output = new int[] { 1, 0, 0, 0 };
                        }
                        else if (Enumerable.SequenceEqual(pl_hand, new int[] { 2, 2 }) || Enumerable.SequenceEqual(pl_hand, new int[] { 3, 3 }))
                        {
                            if (row >= 2 && row <= 7)
                                output = new int[] { 0, 0, 1, 0 };
                            else
                                output = new int[] { 0, 1, 0, 0 };
                        }
                        else if (Enumerable.SequenceEqual(pl_hand, new int[] { 4, 4 }))
                        {
                            if (row >= 5 && row <= 6)
                                output = new int[] { 0, 0, 1, 0 };
                            else
                                output = new int[] { 0, 1, 0, 0 };
                        }
                        else if (Enumerable.SequenceEqual(pl_hand, new int[] { 5, 5 }))
                        {
                            if (row >= 2 && row <= 9)
                                output = new int[] { 0, 0, 0, 1 };
                            else
                                output = new int[] { 0, 1, 0, 0 };
                        }
                        else if (Enumerable.SequenceEqual(pl_hand, new int[] { 6, 6 }))
                        {
                            if (row >= 2 && row <= 6)
                                output = new int[] { 0, 0, 1, 0 };
                            else
                                output = new int[] { 0, 1, 0, 0 };
                        }
                        else if (Enumerable.SequenceEqual(pl_hand, new int[] { 7, 7 }))
                        {
                            if (row >= 2 && row <= 7)
                                output = new int[] { 0, 0, 1, 0 };
                            else
                                output = new int[] { 0, 1, 0, 0 };
                        }
                        else if (Enumerable.SequenceEqual(pl_hand, new int[] { 8, 8 }))
                        {
                            output = new int[] { 0, 0, 1, 0 };
                        }
                        else if (Enumerable.SequenceEqual(pl_hand, new int[] { 9, 9 }))
                        {
                            if (row >= 2 && row <= 9)
                                output = new int[] { 0, 0, 1, 0 };
                            else
                                output = new int[] { 1, 0, 0, 0 };
                        }
                        else if (Enumerable.SequenceEqual(pl_hand, new int[] { 10, 10 }))
                        {
                            output = new int[] { 1, 0, 0, 0 };
                        }
                        else if (c1 + c2 <= 8)
                        {
                            output = new int[] { 0, 1, 0, 0 };
                        }
                        else if (c1 + c2 == 9)
                        {
                            if (row >= 3 && row <= 6)
                                output = new int[] { 0, 0, 0, 1 };
                            else
                                output = new int[] { 0, 1, 0, 0 };
                        }
                        else if (c1 + c2 == 10)
                        {
                            if (row >= 2 && row <= 9)
                                output = new int[] { 0, 0, 0, 1 };
                            else
                                output = new int[] { 0, 1, 0, 0 };
                        }
                        else if (c1 + c2 == 11)
                        {
                            if (row >= 2 && row <= 10)
                                output = new int[] { 0, 0, 0, 1 };
                            else
                                output = new int[] { 0, 1, 0, 0 };
                        }
                        else if (c1 + c2 == 12)
                        {
                            if (row >= 4 && row <= 6)
                                output = new int[] { 1, 0, 0, 0 };
                            else
                                output = new int[] { 0, 1, 0, 0 };
                        }
                        else if (c1 + c2 == 13)
                        {
                            if (row >= 2 && row <= 6)
                                output = new int[] { 1, 0, 0, 0 };
                            else
                                output = new int[] { 0, 1, 0, 0 };
                        }
                        else if (c1 + c2 == 14)
                        {
                            if (row >= 2 && row <= 6)
                                output = new int[] { 1, 0, 0, 0 };
                            else
                                output = new int[] { 0, 1, 0, 0 };
                        }
                        else if (c1 + c2 == 15)
                        {
                            if (row >= 2 && row <= 6)
                                output = new int[] { 1, 0, 0, 0 };
                            else
                                output = new int[] { 0, 1, 0, 0 };
                        }
                        else if (c1 + c2 == 16)
                        {
                            if (row >= 2 && row <= 6)
                                output = new int[] { 1, 0, 0, 0 };
                            else
                                output = new int[] { 0, 1, 0, 0 };
                        }
                        else if (c1 + c2 >= 17)
                        {
                            output = new int[] { 1, 0, 0, 0 };
                        }
                        else
                        {
                            Debug.Assert(true);
                            output = new int[] { 0, 0, 0, 0 };
                        }

                        tableLabelInput[i] = input;
                        tableLabelOutput[i] = output;
                        i++;
                    }
                }
            }
            //PrintTable(tableLabelInput, tableLabelOutput);
        }

        private static void PrintTable(int[][] table_in, int[][] table_out)
        {
            for (int i = 0; i < table_in.Length; i++)
            {
                if (table_in[i] == null)
                    continue;
                Debug.Write("[");
                foreach (int value in table_in[i])
                {
                    Debug.Write(value);
                    Debug.Write(", ");
                }
                Debug.Write("]");

                Debug.Write("   |    ");

                Debug.Write("[");
                foreach (int value in table_out[i])
                {
                    Debug.Write(value);
                    Debug.Write(", ");
                }
                Debug.Write("]");
                Debug.Write("\n");
            }
        }

        private static void PrintTable(int[] table_in, int[] table_out)
        {
            Debug.Write("[");
            foreach (int value in table_in)
            {
                Debug.Write(value);
                Debug.Write(", ");
            }
            Debug.Write("]");

            Debug.Write("   |    ");

            Debug.Write("[");
            foreach (int value in table_out)
            {
                Debug.Write(value);
                Debug.Write(", ");
            }
            Debug.Write("]");
            Debug.Write("\n");
        }


        /// <summary>
        /// Evaluate the provided black box against the logical XOR task,
        /// and return its fitness score.
        /// </summary>
        /// <param name="box">The black box to evaluate.</param>
        /// <returns>A new instance of <see cref="FitnessInfo"/>.</returns>
        public FitnessInfo Evaluate(IBlackBox<double> box)
        {
            double fitness = 0.0;
            bool success = true;

            for (int i = 0; i < tableLabelInput.Length; i++)
            {
                box.ResetState();

                //for (int j = 0; j < tableLabelInput[i].Length; j++)
                //{
                //    box.InputVector[j] = tableLabelInput[i][j] == 0 ? -1 : (double)tableLabelInput[i][j];
                //    box.InputVector[j] = tableLabelInput[i][j];
                //}
                box.InputVector[0] = 1.0;
                box.InputVector[1] = tableLabelInput[i][0];
                box.InputVector[2] = tableLabelInput[i][1];
                box.InputVector[3] = tableLabelInput[i][3];
                box.Activate();

                for (int j = 0; j < tableLabelOutput[i].Length; j++)
                {
                    double output = Clip(box.OutputVector[j]);
                    if (tableLabelOutput[i][j] != 1)
                    {
                        //fitness -= 1.0 - ((1 - output) * (1 - output));
                        //fitness -= (-0.25 * (output * output)) + (0.5 * output) + 0.75;
                    }
                    else if (tableLabelOutput[i][j] == 1)
                    {
                        //fitness += 1.0 - ((1 - output) * (1 - output));
                        //fitness += (-0.25 * (output * output)) + (0.5 * output) + 0.75;
                        double correct = 0.0;
                        for (int k=0; k< tableLabelOutput[i].Length; k++)
                        {
                            if (k!=j)
                            {
                                correct += 1 - Clip(box.OutputVector[k]);
                            }
                        }
                        fitness += (correct + output)/4;
                    }
                }
            }

            if (success)
            {
                //fitness += 1000;
            }

            return new FitnessInfo(fitness);
        }

        #endregion

        #region Private Static Methods

        private static double Clip(double x)
        {
            //return x < -1.0 ? -1.0 : x > 1.0 ? 1.0 : x;
            return x < 0 ? 0 : x > 1.0 ? 1.0 : x;
        }

        private static double CrossEntropy(double yHat, double y)
        {
            if (y != 1)
                return -Math.Log(yHat);
            else
                return -Math.Log(1-yHat);
        }

        #endregion
    }
}
