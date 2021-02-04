using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimplexMethod
{
    public class SimplexMethod
    {
        public static decimal[] Solve(decimal[,] A, decimal[] b, decimal[] c, int maxIter = 100000)
        {
            int m = A.GetLength(0); // 制約条件の数
            int n = A.GetLength(1); // 変数の数

            // 基底変数の添え字（0 <= suffix < n + m）と非基底変数の添え字（0 <= suffix < n + m）
            int[] basicVariableSuffixes = Enumerable.Range(n, m).ToArray();
            int[] nonbasicVariableSuffixes = Enumerable.Range(0, n).ToArray();

            // 単体表の作成
            decimal[,] tableau = MakeSimplexTableau(A, b, c, m, n);

            // 初期解をもつこと（b >= 0）の確認
            for (int i = 0; i < m; i++)
            {
                if (b[i] < 0)
                {
                    // 初期解が求まらない場合は補助問題を作成し単体表を更新する
                    tableau = FirstPhaseOfTwoPhaseSimplex(tableau, m, n, maxIter);
                    break;
                }
            }

            return Calc(tableau, m, n, basicVariableSuffixes, nonbasicVariableSuffixes, maxIter);
        }

        static decimal[] Calc(decimal[,] tableau, int m, int n, int[] basicVariableSuffixes, int[] nonbasicVariableSuffixes, int maxIter)
        {
            string rule = "large";

            // 初期解
            decimal[] result = GetResult(tableau, m, n, basicVariableSuffixes);
            decimal lastValue = result[n];

            int ret = 0;
            for (int iter = 0; iter < maxIter; iter++)
            {
                ret = UpdateTableau(tableau, m, n, basicVariableSuffixes, nonbasicVariableSuffixes, rule);

                if (ret == 0)
                    result = GetResult(tableau, m, n, basicVariableSuffixes);
                else if (ret == -1)
                    break;
                else
                    return null;

                if (result[n] == lastValue) // 退化
                    rule = "small";

                lastValue = result[n];
            }

            return result;
        }

        /// <summary>
        /// 単体表を作成する
        /// </summary>
        /// <param name="A"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="m"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        static decimal[,] MakeSimplexTableau(decimal[,] A, decimal[] b, decimal[] c, int m, int n)
        {
            var tableau = new decimal[m + 1, 1 + n + m];

            for (int i = 0; i < m; i ++)
            {
                tableau[i, 0] = b[i];

                tableau[i, n + i + 1] = 1;

                for (int j = 0; j < n; j++)
                {
                    tableau[i, j + 1] = A[i, j];

                    if (i == 0) tableau[m, j + 1] = -c[j];
                }
            }

            return tableau;
        }

        /// <summary>
        /// 入れ替える非基底変数の添え字を返す（0 <= index < n）
        /// 単体表でのインデックスではないので注意
        /// </summary>
        /// <param name="tableau"></param>
        /// <param name="m"></param>
        /// <param name="nonbasicVariableSuffixes"></param>
        /// <returns>入れ替える非基底変数のインデックス（0 <= index < n or -1 (最適解））</returns>
        static int SelectNonbasicVariableIndex(decimal[,] tableau, int m, int n, int[] nonbasicVariableSuffixes, string rule)
        {
            decimal minCoef = decimal.MaxValue;
            int minIndex = int.MaxValue;
            for (int i = 0; i < n; i++)
            {
                int suffix = nonbasicVariableSuffixes[i];
                decimal coef = tableau[m, 1 + suffix];

                if (coef < -1E-12m)
                {
                    // 最大係数規則では係数が負で最も小さいもの（複数あれば添え字の小さいもの）を，
                    // 最小添字規則では係数が負で添え字が最も小さいものを選ぶ
                    if ((rule == "large" && coef < minCoef) ||
                        (rule == "large" && Math.Abs(coef - minCoef) < 1E-12m && minIndex < int.MaxValue && suffix < nonbasicVariableSuffixes[minIndex]) ||
                        (rule == "small" && (minIndex == int.MaxValue || (minIndex < int.MaxValue && suffix < nonbasicVariableSuffixes[minIndex]))))
                    {
                        minCoef = coef;
                        minIndex = i;
                    }
                }
            }

            return (minIndex == int.MaxValue) ? -1 : minIndex;
        }

        /// <summary>
        /// 入れ替える基底変数の単体表でのインデックスを返す（0 <= index < m）
        /// 係数の添え字ではないので注意
        /// </summary>
        /// <param name="tableau"></param>
        /// <param name="m"></param>
        /// <param name="basicVariableSuffixes"></param>
        /// <param name="nonbasicVariableSuffix"></param>
        /// <returns>入れ替える基底変数の単体表でのインデックス（0 <= index < m or -1（非有界））</returns>
        static int SelectBasicVariableIndex(decimal[,] tableau, int m, int[] basicVariableSuffixes, int nonbasicVariableSuffix)
        {
            decimal minBdivA = decimal.MaxValue;
            int minIndex = int.MaxValue;
            for (int i = 0; i < m; i++)
            {
                decimal a = tableau[i, 1 + nonbasicVariableSuffix];
                decimal b = tableau[i, 0];

                if (a > 0 && b >= 0) // この条件でないとx (>=0)を上から押さえられない
                {
                    decimal BdivA = b / a;

                    if ((BdivA < minBdivA) ||
                        (Math.Abs(BdivA - minBdivA) < 1E-12m && basicVariableSuffixes[i] < basicVariableSuffixes[minIndex]))
                    {
                        minBdivA = BdivA;
                        minIndex = i;
                    }
                }
            }

            return (minIndex == int.MaxValue) ? -1 : minIndex;
        }

        /// <summary>
        /// 基底変数と非基底変数を入れ替える
        /// </summary>
        /// <param name="tableau"></param>
        /// <param name="basicVariableIndex"></param>
        /// <param name="nonbasicVariableSuffix"></param>
        /// <param name="m"></param>
        /// <param name="n"></param>
        static void SwapVariables(decimal[,] tableau, int m, int n, int[] basicVariableSuffixes, int[] nonbasicVariableSuffixes, int basicVariableIndex, int nonbasicVariableIndex)
        {
            int nonbasicVariableSuffix = nonbasicVariableSuffixes[nonbasicVariableIndex];

            var temp = basicVariableSuffixes[basicVariableIndex];
            basicVariableSuffixes[basicVariableIndex] = nonbasicVariableSuffixes[nonbasicVariableIndex];
            nonbasicVariableSuffixes[nonbasicVariableIndex] = temp;

            decimal pivot = tableau[basicVariableIndex, 1 + nonbasicVariableSuffix];

            for (int i = 0; i < 1 + n + m; i++)
            {
                if (i == 1 + nonbasicVariableSuffix)
                    tableau[basicVariableIndex, i] = 1;
                else
                    tableau[basicVariableIndex, i] /= pivot;
            }
        }

        /// <summary>
        /// 基底変数と非基底変数の入れ替え後に単体表の他の値を更新する
        /// </summary>
        /// <param name="tableau"></param>
        /// <param name="basicVariableIndex"></param>
        /// <param name="nonbasicVariableSuffix"></param>
        /// <param name="m"></param>
        /// <param name="n"></param>
        static void UpdateVariablesAfterSwap(decimal[,] tableau, int basicVariableIndex, int nonbasicVariableSuffix, int m, int n)
        {
            for (int i = 0; i < m + 1; i++)
            {
                if (i == basicVariableIndex) continue;

                decimal val = tableau[i, 1 + nonbasicVariableSuffix] / tableau[basicVariableIndex, 1 + nonbasicVariableSuffix];

                for (int j = 0; j < 1 + n + m; j++)
                {
                    if (j == 1 + nonbasicVariableSuffix)
                        tableau[i, j] = 0;
                    else
                        tableau[i, j] -= val * tableau[basicVariableIndex, j];
                }
            }
        }

        /// <summary>
        /// 単体表の1回の更新を行う
        /// </summary>
        /// <param name="tableau"></param>
        /// <param name="m"></param>
        /// <param name="n"></param>
        /// <param name="basicVariableSuffixes"></param>
        /// <param name="nonbasicVariableSuffixes"></param>
        /// <param name="rule"></param>
        /// <returns>-1:最適解，0:正常終了，int.MinValue:非有界</returns>
        static int UpdateTableau(decimal[,] tableau, int m, int n, int[] basicVariableSuffixes, int[] nonbasicVariableSuffixes, string rule)
        {
            int nonbasicVariableIndex = SelectNonbasicVariableIndex(tableau, m, n, nonbasicVariableSuffixes, rule);
            if (nonbasicVariableIndex == -1) return -1;

            int nonbasicVariableSuffix = nonbasicVariableSuffixes[nonbasicVariableIndex];

            int basicVariableIndex = SelectBasicVariableIndex(tableau, m, basicVariableSuffixes, nonbasicVariableSuffix);
            if (basicVariableIndex == -1) return int.MinValue;

            SwapVariables(tableau, m, n, basicVariableSuffixes, nonbasicVariableSuffixes,  basicVariableIndex, nonbasicVariableIndex);

            UpdateVariablesAfterSwap(tableau, basicVariableIndex, nonbasicVariableSuffix, m, n);

            return 0;
        }

        /// <summary>
        /// 最適解と目的関数の値を得る
        /// </summary>
        /// <param name="tableau"></param>
        /// <param name="m"></param>
        /// <param name="n"></param>
        /// <param name="basicVariableSuffixes"></param>
        /// <returns></returns>
        static decimal[] GetResult(decimal[,] tableau, int m, int n, int[] basicVariableSuffixes)
        {
            decimal[] result = new decimal[n + 1];

            for (int i = 0; i < m; i++)
            {
                if (basicVariableSuffixes[i] < n)
                {
                    result[basicVariableSuffixes[i]] = tableau[i, 0];
                }
            }
            result[n] = tableau[m, 0];

            return result;
        }

        /// <summary>
        /// 初期解が求まらない場合に補助問題を作成して単体表を更新する
        /// </summary>
        /// <param name="tableauOrg"></param>
        /// <param name="m"></param>
        /// <param name="n"></param>
        /// <param name="maxIter"></param>
        /// <returns></returns>
        static decimal[,] FirstPhaseOfTwoPhaseSimplex(decimal[,] tableauOrg, int m, int n, int maxIter)
        {
            decimal[,] tableau = AddArtificialVariable(tableauOrg, m, n);

            // 制約条件の違反量が最大の基底変数を見つける
            decimal minVal = 0;
            int basicVariableIndex = -1;
            for (int i = 0; i < m; i++)
            {
                if (tableau[i, 0] < minVal)
                {
                    minVal = tableau[i, 0];
                    basicVariableIndex = i;
                }
            }
            if (minVal == 0m) throw new Exception();

            // 上記の基底変数と非基底変数x_0をスワップ
            int[] basicVariableSuffixes = Enumerable.Range(n, m).ToArray();
            int[] nonbasicVariableSuffixes = Enumerable.Range(0, n).ToList().Concat(new List<int>() { n + m }).ToArray();
            int nonbasicVariableIndex = n;
            SwapVariables(tableau, m, n + 1, basicVariableSuffixes, nonbasicVariableSuffixes,  basicVariableIndex, nonbasicVariableIndex);

            int nonbasicVariableSuffix = n + m;
            UpdateVariablesAfterSwap(tableau, basicVariableIndex, nonbasicVariableSuffix, m, n + 1);

            decimal[] result = Calc(tableau, m, n + 1, basicVariableSuffixes, nonbasicVariableSuffixes, maxIter);

            if (result == null)
                return null;
            else
                return RemakeTableauWithoutArtificialVariable(tableauOrg, tableau, m, n, basicVariableSuffixes);
        }

        /// <summary>
        /// 単体表に人工変数を追加
        /// </summary>
        /// <param name="tableauOrg"></param>
        /// <param name="m"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        static decimal[,] AddArtificialVariable(decimal[,] tableauOrg, int m, int n)
        {
            decimal[,] tableau = new decimal[m + 1, 1 + n + m + 1];

            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < 1 + n + m; j++)
                {
                    tableau[i, j] = tableauOrg[i, j];
                }
                tableau[i, 1 + n + m] = -1;
            }
            tableau[m, 1 + n + m] = 1;

            return tableau;
        }

        static decimal[,] RemakeTableauWithoutArtificialVariable(decimal[,] tableauOrg, decimal[,] tableauArt, int m, int n, int[] basicVariableSuffixes)
        {
            decimal[,] tableau = new decimal[m + 1, 1 + n + m];

            // x_0の列を除き，wの行をzに
            for (int i = 0; i < m + 1; i++)
            {
                for (int j = 0; j < 1 + n + m; j++)
                {
                    if (i == m)
                        tableau[i, j] = tableauOrg[i, j];
                    else
                        tableau[i, j] = tableauArt[i, j];
                }
            }

            // 基底変数のzの係数が0になるようにする
            for (int i = 0; i < m; i++)
            {
                int suffix = basicVariableSuffixes[i];

                if (Math.Abs(tableau[m, 1 + suffix]) > 1E-12m)
                {
                    decimal val = tableau[m, 1 + suffix] / tableau[i, 1 + suffix];

                    for (int j = 0; j < 1 + n + m; j++)
                    {
                        if (j == 1 + suffix)
                            tableau[m, j] = 0m;
                        else
                            tableau[m, j] -= val * tableau[i, j];
                    }
                }
                else
                {
                    tableau[m, 1 + suffix] = 0m;
                }
            }

            return tableau;
        }
    }
}
