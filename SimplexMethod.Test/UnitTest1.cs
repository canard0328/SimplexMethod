using NUnit.Framework;
using System;
using System.Linq;
using System.Collections;
using System.Reflection;

namespace SimplexMethod.Test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestMakeSimplexTableau()
        {
            decimal[,] A = new decimal[3, 2] { { 1, 1 }, { 1, 3 }, { 2, 1 } };
            decimal[] b = new decimal[3] { 6, 12, 10 };
            decimal[] c = new decimal[2] { 1, 2 };
            decimal[,] tableau = new decimal[4, 6] { { 6, 1, 1, 1, 0, 0 }, { 12, 1, 3, 0, 1, 0 }, { 10, 2, 1, 0, 0, 1 }, { 0, -1, -2, 0, 0, 0 } };

            int m = A.GetLength(0); // §–ñðŒ‚Ì”
            int n = A.GetLength(1); // •Ï”‚Ì”

            var type = typeof(SimplexMethod);
            var methodInfo = type.GetMethod("MakeSimplexTableau", BindingFlags.NonPublic | BindingFlags.Static);

            var result = (decimal[,])methodInfo.Invoke(null, new object[] { A, b, c, m, n });

            CollectionAssert.AreEqual(tableau, result, new DecimalComparer());
        }

        [Test]
        public void TestSelectNonbasicVariableIndexSuccess()
        {
            decimal[,] tableau = new decimal[4, 6] { { 6, 1, 1, 1, 0, 0 }, { 12, 1, 3, 0, 1, 0 }, { 10, 2, 1, 0, 0, 1 }, { 0, -1, -2, 0, 0, 0 } };

            int m = 3;
            int n = 2;

            int[] nonbasicVariableSuffixes = Enumerable.Range(0, n).ToArray();

            var type = typeof(SimplexMethod);
            var methodInfo = type.GetMethod("SelectNonbasicVariableIndex", BindingFlags.NonPublic | BindingFlags.Static);

            var result = (int)methodInfo.Invoke(null, new object[] { tableau, m, n, nonbasicVariableSuffixes, "large" });

            Assert.AreEqual(1, result);
        }

        [Test]
        public void TestSelectNonbasicVariableIndexFail()
        {
            decimal[,] tableau = new decimal[4, 6] { { 3, 1, 0, 3.0m / 2.0m, -1.0m / 2.0m, 0 },
                                                     { 3, 0, 1, -1.0m / 2.0m, 1.0m / 2.0m, 0 },
                                                     { 1, 0, 0, -5.0m / 2.0m, 1.0m / 2.0m, 1 },
                                                     { 9, 0, 0, 1.0m / 2.0m, 1.0m / 2.0m, 0 } };

            int m = 3;
            int n = 2;

            int[] nonbasicVariableSuffixes = new int[] { 2, 3 };

            var type = typeof(SimplexMethod);
            var methodInfo = type.GetMethod("SelectNonbasicVariableIndex", BindingFlags.NonPublic | BindingFlags.Static);

            var result = (int)methodInfo.Invoke(null, new object[] { tableau, m, n, nonbasicVariableSuffixes, "large" });

            Assert.AreEqual(-1, result);
        }

        [Test]
        public void TestSelectNonbasicVariableIndexSmall()
        {
            decimal[,] tableau = new decimal[4, 6] { { 6, 1, 1, 1, 0, 0 }, { 12, 1, 3, 0, 1, 0 }, { 10, 2, 1, 0, 0, 1 }, { 0, -1, -2, 0, 0, 0 } };

            int m = 3;
            int n = 2;

            int[] nonbasicVariableSuffixes = Enumerable.Range(0, n).ToArray();

            var type = typeof(SimplexMethod);
            var methodInfo = type.GetMethod("SelectNonbasicVariableIndex", BindingFlags.NonPublic | BindingFlags.Static);

            var result = (int)methodInfo.Invoke(null, new object[] { tableau, m, n, nonbasicVariableSuffixes, "small" });

            Assert.AreEqual(0, result);
        }

        [Test]
        public void TestSelectBasicVariableIndex()
        {
            decimal[,] tableau = new decimal[4, 6] { { 6, 1, 1, 1, 0, 0 }, { 12, 1, 3, 0, 1, 0 }, { 10, 2, 1, 0, 0, 1 }, { 0, -1, -2, 0, 0, 0 } };

            int m = 3;
            int n = 2;

            int[] basicVariableSuffixes = Enumerable.Range(n, m).ToArray();

            var type = typeof(SimplexMethod);
            var methodInfo = type.GetMethod("SelectBasicVariableIndex", BindingFlags.NonPublic | BindingFlags.Static);

            var result = (int)methodInfo.Invoke(null, new object[] { tableau, m, basicVariableSuffixes, 1 });

            Assert.AreEqual(1, result);
        }

        [Test]
        public void TestSelectBasicVariableIndexUnbounded()
        {
            decimal[,] tableau = new decimal[3, 5] { { 4, 1, -2, 1, 0 }, { 6, 0, -1, 1, 1 }, { 8, 0, -5, 2, 0 } };

            int m = 2;

            int[] basicVariableSuffixes = new int[] { 0, 3 };

            var type = typeof(SimplexMethod);
            var methodInfo = type.GetMethod("SelectBasicVariableIndex", BindingFlags.NonPublic | BindingFlags.Static);

            var result = (int)methodInfo.Invoke(null, new object[] { tableau, m, basicVariableSuffixes, 1 });

            Assert.AreEqual(-1, result);
        }

        [Test]
        public void TestSwapVariables()
        {
            decimal[,] tableau = new decimal[4, 6] { { 6, 1, 1, 1, 0, 0 },
                                                     { 4, 1.0m/3.0m, 1, 0, 1.0m/3.0m, 0 },
                                                     { 10, 2, 1, 0, 0, 1 },
                                                     { 0, -1, -2, 0, 0, 0 } };
            decimal[,] result = new decimal[4, 6] { { 6, 1, 1, 1, 0, 0 },
                                                    { 12, 1, 3, 0, 1, 0 },
                                                    { 10, 2, 1, 0, 0, 1 },
                                                    { 0, -1, -2, 0, 0, 0 } };

            int m = 3;
            int n = 2;

            int[] basicVariableSuffixes = Enumerable.Range(n, m).ToArray();
            int[] nonbasicVariableSuffixes = Enumerable.Range(0, n).ToArray();

            int basicVariableIndex = 1;
            int nonbasicVariableSuffix = 1;

            var type = typeof(SimplexMethod);
            var methodInfo = type.GetMethod("SwapVariables", BindingFlags.NonPublic | BindingFlags.Static);

            methodInfo.Invoke(null, new object[] { result, m, n, basicVariableSuffixes, nonbasicVariableSuffixes, basicVariableIndex, nonbasicVariableSuffix });

            CollectionAssert.AreEqual(tableau, result, new DecimalComparer());
            CollectionAssert.AreEqual(new int[] { 2, 1, 4 }, basicVariableSuffixes);
            CollectionAssert.AreEqual(new int[] { 0, 3 }, nonbasicVariableSuffixes);
        }

        [Test]
        public void TestUpdateVariablesAfterSwap()
        {
            decimal[,] tableau = new decimal[4, 6] { { 2, 2.0m / 3.0m, 0, 1, -1.0m / 3.0m, 0 },
                                                     { 4, 1.0m/3.0m, 1, 0, 1.0m/3.0m, 0 },
                                                     { 6, 5.0m / 3.0m, 0, 0, -1.0m / 3.0m, 1 },
                                                     { 8, -1.0m / 3.0m, 0, 0, 2.0m / 3.0m, 0 } };
            decimal[,] result = new decimal[4, 6] { { 6, 1, 1, 1, 0, 0 },
                                                    { 4, 1.0m/3.0m, 1, 0, 1.0m/3.0m, 0 },
                                                    { 10, 2, 1, 0, 0, 1 },
                                                    { 0, -1, -2, 0, 0, 0 } };

            int m = 3;
            int n = 2;

            int basicVariableIndex = 1;
            int nonbasicVariableSuffix = 1;

            var type = typeof(SimplexMethod);
            var methodInfo = type.GetMethod("UpdateVariablesAfterSwap", BindingFlags.NonPublic | BindingFlags.Static);

            methodInfo.Invoke(null, new object[] { result, basicVariableIndex, nonbasicVariableSuffix, m, n });

            CollectionAssert.AreEqual(tableau, result, new DecimalComparer());
        }

        [Test]
        public void TestUpdateTableau()
        {
            decimal[,] tableau = new decimal[4, 6] { { 2, 2.0m / 3.0m, 0, 1, -1.0m / 3.0m, 0 },
                                                     { 4, 1.0m/3.0m, 1, 0, 1.0m/3.0m, 0 },
                                                     { 6, 5.0m / 3.0m, 0, 0, -1.0m / 3.0m, 1 },
                                                     { 8, -1.0m / 3.0m, 0, 0, 2.0m / 3.0m, 0 } };
            decimal[,] result = new decimal[4, 6] { { 6, 1, 1, 1, 0, 0 }, { 12, 1, 3, 0, 1, 0 }, { 10, 2, 1, 0, 0, 1 }, { 0, -1, -2, 0, 0, 0 } };

            int m = 3;
            int n = 2;

            int[] basicVariableIndexes = Enumerable.Range(n, m).ToArray();
            int[] nonbasicVariableSuffixes = Enumerable.Range(0, n).ToArray();

            var type = typeof(SimplexMethod);
            var methodInfo = type.GetMethod("UpdateTableau", BindingFlags.NonPublic | BindingFlags.Static);

            int ret = (int)methodInfo.Invoke(null, new object[] { result, m, n, basicVariableIndexes, nonbasicVariableSuffixes, "large" });

            CollectionAssert.AreEqual(tableau, result, new DecimalComparer());
            Assert.AreEqual(0, ret);
        }

        [Test]
        public void TestGetResult()
        {
            decimal[,] tableau = new decimal[4, 6] { { 2, 2.0m / 3.0m, 0, 1, -1.0m / 3.0m, 0 },
                                                     { 4, 1.0m/3.0m, 1, 0, 1.0m/3.0m, 0 },
                                                     { 6, 5.0m / 3.0m, 0, 0, -1.0m / 3.0m, 1 },
                                                     { 8, -1.0m / 3.0m, 0, 0, 2.0m / 3.0m, 0 } };

            int m = 3;
            int n = 2;

            int[] basicVariableIndexes = new int[] { 2, 1, 4 };

            var type = typeof(SimplexMethod);
            var methodInfo = type.GetMethod("GetResult", BindingFlags.NonPublic | BindingFlags.Static);

            decimal[] result = (decimal[])methodInfo.Invoke(null, new object[] { tableau, m, n, basicVariableIndexes });

            CollectionAssert.AreEqual(new decimal[] { 0, 4, 8 }, result, new DecimalComparer());
        }

        [Test]
        public void TestSolve()
        {
            decimal[,] A = new decimal[3, 2] { { 1, 1 }, { 1, 3 }, { 2, 1 } };
            decimal[] b = new decimal[3] { 6, 12, 10 };
            decimal[] c = new decimal[2] { 1, 2 };

            var type = typeof(SimplexMethod);
            var methodInfo = type.GetMethod("Solve", BindingFlags.Public | BindingFlags.Static);

            decimal[] result = (decimal[])methodInfo.Invoke(null, new object[] { A, b, c, 100000 });

            CollectionAssert.AreEqual(new decimal[] { 3, 3, 9 }, result, new DecimalComparer());
        }

        [Test]
        public void TestSolveCycle()
        {
            decimal[,] A = new decimal[3, 4] { { 1.0m / 2.0m, -11.0m / 2.0m, -5.0m / 2.0m, 9 },
                                               { 1.0m / 2.0m, -3.0m / 2.0m, -1.0m / 2.0m, 1 },
                                               { 1, 0, 0, 0 } };
            decimal[] b = new decimal[3] { 0, 0, 1 };
            decimal[] c = new decimal[4] { 10, -57, -9, -24 };

            var type = typeof(SimplexMethod);
            var methodInfo = type.GetMethod("Solve", BindingFlags.Public | BindingFlags.Static);

            decimal[] result = (decimal[])methodInfo.Invoke(null, new object[] { A, b, c, 100000 });

            CollectionAssert.AreEqual(new decimal[] { 1, 0, 1, 0, 1 }, result, new DecimalComparer());
        }

        [Test]
        public void TestSolveUnbounded()
        {
            decimal[,] A = new decimal[2, 2] { { 1, -2 }, { -1, 1 } };
            decimal[] b = new decimal[2] { 4, 2 };
            decimal[] c = new decimal[2] { 2, 1 };

            var type = typeof(SimplexMethod);
            var methodInfo = type.GetMethod("Solve", BindingFlags.Public | BindingFlags.Static);

            decimal[] result = (decimal[])methodInfo.Invoke(null, new object[] { A, b, c, 100000 });

            CollectionAssert.AreEqual(null, result);
        }

        [Test]
        public void TestSolveTwoPhase()
        {
            decimal[,] A = new decimal[3, 2] { { 1, 1 },
                                               { 1, 3 },
                                               { -3, -2 } };
            decimal[] b = new decimal[3] { 6, 12, -6 };
            decimal[] c = new decimal[2] { 1, 2 };

            var type = typeof(SimplexMethod);
            var methodInfo = type.GetMethod("Solve", BindingFlags.Public | BindingFlags.Static);

            decimal[] result = (decimal[])methodInfo.Invoke(null, new object[] { A, b, c, 100000 });

            CollectionAssert.AreEqual(new decimal[] { 3, 3, 9 }, result, new DecimalComparer());
        }

        [Test]
        public void TestAddArtificialVariable()
        {
            decimal[,] tableauOrg = new decimal[4, 6] { { 6, 1, 1, 1, 0, 0 },
                                                     { 12, 1, 3, 0, 1, 0 },
                                                     { -6, -3, -2, 0, 0, 1 },
                                                     { 0, -1, -2, 0, 0, 0 } };
            decimal[,] tableau = new decimal[4, 7] { { 6, 1, 1, 1, 0, 0, -1 },
                                                     { 12, 1, 3, 0, 1, 0, -1 },
                                                     { -6, -3, -2, 0, 0, 1, -1 },
                                                     { 0, 0, 0, 0, 0, 0, 1 } };

            int m = 3;
            int n = 2;

            var type = typeof(SimplexMethod);
            var methodInfo = type.GetMethod("AddArtificialVariable", BindingFlags.NonPublic | BindingFlags.Static);

            decimal[,] result = (decimal[,])methodInfo.Invoke(null, new object[] { tableauOrg, m, n });

            CollectionAssert.AreEqual(tableau, result, new DecimalComparer());
        }

        [Test]
        public void TestFirstPhase()
        {
            decimal[,] tableauOrg = new decimal[4, 6] { { 6, 1, 1, 1, 0, 0 },
                                                     { 12, 1, 3, 0, 1, 0 },
                                                     { -6, -3, -2, 0, 0, 1 },
                                                     { 0, -1, -2, 0, 0, 0 } };
            decimal[,] tableau = new decimal[4, 6] { { 4, 0, 1.0m / 3.0m, 1, 0, 1.0m / 3.0m },
                                                     { 10, 0, 7.0m / 3.0m, 0, 1, 1.0m / 3.0m },
                                                     { 2, 1, 2.0m / 3.0m, 0, 0, -1.0m / 3.0m },
                                                     { 2, 0, -4.0m / 3.0m, 0, 0, -1.0m / 3.0m } };

            int m = 3;
            int n = 2;

            var type = typeof(SimplexMethod);
            var methodInfo = type.GetMethod("FirstPhaseOfTwoPhaseSimplex", BindingFlags.NonPublic | BindingFlags.Static);

            decimal[,] result = (decimal[,])methodInfo.Invoke(null, new object[] { tableauOrg, m, n, 100000 });

            CollectionAssert.AreEqual(tableau, result, new DecimalComparer());
        }

        class DecimalComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                return (x, y) switch
                {
                    (decimal dx, decimal dy) => Math.Abs(dx - dy) < 1E-12m ? 0 : dx.CompareTo(dy),
                    _ => -1
                };
            }
        }
    }
}