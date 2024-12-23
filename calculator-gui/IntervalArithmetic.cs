﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Xml;

namespace calculator_gui
{
    public class Interval
    {
        private double _minimum;
        public double Minimum
        {
            get => _minimum; 
            set => _minimum = value;
        }
        public bool IncludesMinimum = true;

        private double _maximum;
        public double Maximum
        {
            get => _maximum;
            set => _maximum = value;
        }
        public bool IncludesMaximum = true;

        public Interval(double minimum, double maximum)
        {
            _minimum = minimum;
            _maximum = maximum;
        }

        public static Interval operator +(Interval a) => a;
        public static Interval operator -(Interval a)
        {
            return new Interval(-a.Maximum, -a.Minimum) { IncludesMinimum = a.IncludesMinimum, IncludesMaximum = a.IncludesMaximum };
        }
        public static Interval operator ++(Interval a)
        {
            return new Interval(a.Minimum + 1, a.Maximum + 1) { IncludesMinimum = a.IncludesMinimum, IncludesMaximum = a.IncludesMaximum };
        }
        public static Interval operator --(Interval a)
        {
            return new Interval(a.Minimum - 1, a.Maximum - 1) { IncludesMinimum = a.IncludesMinimum, IncludesMaximum = a.IncludesMaximum };
        }

        public static Interval operator +(Interval a, Interval b)
        {
            return new Interval(a.Minimum + b.Minimum, a.Maximum + b.Maximum) 
            { 
                IncludesMinimum = a.IncludesMinimum && b.IncludesMinimum,
                IncludesMaximum = a.IncludesMaximum && b.IncludesMaximum
            };
        }
        public static Interval operator -(Interval a, Interval b) => a + -b;
        public static Interval operator *(Interval a, Interval b)
        {
            if (a.Minimum >= 0 && b.Minimum <= 0)
            {
                return new Interval(a.Minimum * b.Minimum, a.Maximum * b.Maximum)
                {
                    IncludesMinimum = a.IncludesMinimum && b.IncludesMinimum,
                    IncludesMaximum = a.IncludesMaximum && b.IncludesMaximum
                };
            }
            else if (a.Maximum <= 0 && b.Maximum <= 0)
            {
                return new Interval(a.Maximum * b.Maximum, a.Minimum * b.Minimum)
                {
                    IncludesMinimum = a.IncludesMinimum && b.IncludesMinimum,
                    IncludesMaximum = a.IncludesMaximum && b.IncludesMaximum
                };
            }
            else
            {
                return new Interval(
                    Math.Min(
                        Math.Min(
                            a.Minimum * b.Minimum,
                            a.Minimum * b.Maximum),
                        Math.Min(
                            a.Maximum * b.Minimum,
                            a.Maximum * b.Maximum)),
                    Math.Max(
                        Math.Max(
                            a.Minimum * b.Minimum,
                            a.Minimum * b.Maximum),
                        Math.Max(
                            a.Maximum * b.Minimum,
                            a.Maximum * b.Maximum)))
                {
                    IncludesMinimum = a.IncludesMinimum && b.IncludesMinimum,
                    IncludesMaximum = a.IncludesMaximum && b.IncludesMaximum
                };
            }
        }

        public static implicit operator Interval(float a) => new Interval(a, a);
        public static implicit operator Interval(double a) => new Interval(a, a);

        /// <summary>
        /// Computes the range of x^y given domains for x and y
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>The range of x^y</returns>
        public static Interval Exponentiation(Interval a, Interval b)
        {
            if (a.Minimum < 0 && b.Minimum == b.Maximum)
            {
                // [x]^a
                // this only handles the cases that arent also handled in the more general [x]^[y] case
                if (a.Maximum <= 0 && ((int)Math.Round(b.Minimum)) % 2 == 0)
                {
                    return new Interval(Math.Pow(a.Maximum, b.Minimum), Math.Pow(a.Minimum, b.Maximum));
                }
                else if (a.Minimum < 0 && a.Maximum > 0 && ((int)Math.Round(b.Minimum)) % 2 == 0)
                {
                    double minimum = Math.Pow(a.Minimum, b.Minimum);
                    if (Double.IsNaN(minimum))
                    {
                        minimum = 0;
                    }
                    return new Interval(0, Math.Max(minimum, Math.Pow(a.Maximum, b.Minimum)));
                }
                else
                {
                    double minimum = Math.Pow(a.Minimum, b.Minimum);
                    if (Double.IsNaN(minimum))
                    {
                        minimum = 0;
                    }
                    return new Interval(minimum, Math.Pow(a.Maximum, b.Minimum));
                }
            }
            else if (a.Minimum >= 0)
            {
                if (b.Minimum > 0)
                {
                    return new Interval(Math.Pow(a.Minimum, b.Minimum), Math.Pow(a.Maximum, b.Maximum));
                }
                else if (b.Minimum <= 0 && 0 <= b.Maximum)
                {
                    return new Interval(
                        Math.Min(
                            1, Math.Min(
                                Math.Pow(a.Minimum, b.Maximum),
                                Math.Pow(a.Maximum, b.Minimum))),
                        Math.Max(
                            Math.Pow(a.Minimum, b.Minimum),
                            Math.Pow(a.Maximum, b.Maximum)));
                }
                else
                {
                    double minimum = Math.Pow(a.Maximum, b.Minimum);
                    if (Double.IsNaN(minimum))
                    {
                        minimum = 0;
                    }
                    return new Interval(minimum, Math.Pow(a.Minimum, b.Maximum));
                }
            }
            else
            {
                return new Interval(Double.NaN, Double.NaN);
            }
        }

        public static Interval Sin(Interval a)
        {
            return Cos(new Interval(a.Minimum - Math.PI / 2, a.Maximum - Math.PI / 2));
        }

        public static Interval Cos(Interval a)
        {
            Interval output;
            if (a.Maximum - a.Minimum >= Math.PI * 2)
            {
                output = new Interval(-1, 1);
            }
            else
            {
                double normalisedMinimum = a.Minimum - 2 * Math.PI * (int)(a.Minimum / (2 * Math.PI));
                double normalisedMaximum = a.Maximum - 2 * Math.PI * (int)(a.Minimum / (2 * Math.PI));
                if (normalisedMinimum < 0)
                {
                    normalisedMinimum += Math.PI * 2;
                    normalisedMaximum += Math.PI * 2;
                }

                double outputMinimum = 0;
                double outputMaximum = 0;

                if (normalisedMinimum <= Math.PI)
                {
                    // minimum is within first half of first period of cos x
                    if (normalisedMaximum <= Math.PI)
                    {
                        // maximum is within first half of first period of cos x
                        outputMinimum = Math.Cos(a.Maximum);
                        outputMaximum = Math.Cos(a.Minimum);
                    }
                    else if (normalisedMaximum <= Math.PI * 2)
                    {
                        // maximum is within second half of first period of cos x
                        outputMinimum = -1;
                        outputMaximum = Math.Max(Math.Cos(a.Minimum), Math.Cos(a.Maximum));
                    }
                    else
                    {
                        // maximum is within first half of second period of cos x
                        outputMinimum = -1;
                        outputMaximum = 1;
                    }
                }
                else
                {
                    // minimum is within second half of first period of cos x
                    if (normalisedMaximum <= Math.PI * 2)
                    {
                        // maximum is within second half of first period of cos x
                        outputMinimum = Math.Cos(a.Minimum);
                        outputMaximum = Math.Cos(a.Maximum);
                    }
                    else if (normalisedMaximum <= Math.PI * 3)
                    {
                        // maximum is within first half of second period of cos x
                        outputMinimum = Math.Min(Math.Cos(a.Minimum), Math.Cos(a.Maximum));
                        outputMaximum = 1;
                    }
                    else
                    {
                        // maximum is within second half of second period of cos x
                        outputMinimum = -1;
                        outputMaximum = 1;
                    }
                }

                output = new Interval(outputMinimum, outputMaximum);
            }
            return output;
        }

        public static Interval Arcsin(Interval a)
        {
            double minimum = Math.Max(a.Minimum, -1);
            double maximum = Math.Min(a.Maximum, 1);
            return new Interval(Math.Asin(minimum), Math.Asin(maximum));
        }

        public static Interval Arccos(Interval a)
        {
            double minimum = Math.Max(a.Minimum, -1);
            double maximum = Math.Min(a.Maximum, 1);
            return new Interval(Math.Acos(maximum), Math.Acos(minimum));
        }

        public static Interval Arctan(Interval a)
        {
            return new Interval(Math.Atan(a.Minimum), Math.Atan(a.Maximum));
        }

        public static Interval Absolute(Interval a)
        {
            if (a.Minimum < 0 && a.Maximum > 0)
            {
                return new Interval(0, Math.Max(
                    Math.Abs(a.Minimum),
                    Math.Abs(a.Maximum)));
            }
            else if (a.Maximum <= 0)
            {
                return new Interval(Math.Abs(a.Maximum), Math.Abs(a.Minimum));
            }
            else
            {
                return new Interval(Math.Abs(a.Minimum), Math.Abs(a.Maximum));
            }
        }

        /// <summary>
        /// Returns true if given number is contained within the interval
        /// </summary>
        /// <param name="number">The number to check</param>
        /// <returns>True if number is inside the interval</returns>
        public bool Contains(double number)
        {
            if (Minimum <= number && Maximum >= number)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            return "[" + Minimum + ", " + Maximum + "]";
        }
    }

    public class MultiInterval
    {
        private List<Interval> Intervals = new List<Interval>();

        public MultiInterval()
        {
            Intervals.Add(new Interval(0, 0));
        }

        public MultiInterval(Interval interval)
        {
            Intervals.Add(interval);
        }

        public List<double> GetBoundaries()
        {
            List<double> output = new List<double>();
            foreach (Interval interval in Intervals)
            {
                output.Add(interval.Minimum);
                output.Add(interval.Maximum);
            }
            return output;
        }

        /// <summary>
        /// Returns true if given number is contained within the multi-interval
        /// </summary>
        /// <param name="number">The number to check</param>
        /// <returns>True if number is inside the multi-interval</returns>
        public bool Contains(double number)
        {
            foreach (Interval interval in Intervals)
            {
                if (interval.Contains(number))
                {
                    return true;
                }
            }
            return false;
        }

        public override string ToString()
        {
            string output = "";
            for (int index = 0; index < Intervals.Count; index++)
            {
                output += Intervals[index].ToString();
                if (index !=  Intervals.Count - 1)
                {
                    output += " U ";
                }
            }
            return output;
        }

        public static List<int> GetOverlaps(MultiInterval a, MultiInterval b)
        {
            List<int> output = new List<int>();
            List<double> aList = a.GetBoundaries();
            List<double> bList = b.GetBoundaries();
            int aIndex = 0;
            int bIndex = 0;
            bool aOverlapping = false;
            bool bOverlapping = false;
            while (aIndex < aList.Count && bIndex < bList.Count)
            {
                if (aList[aIndex] <= bList[bIndex])
                {
                    aIndex++;
                    if (aOverlapping)
                    {
                        aOverlapping = false;
                    }
                    else
                    {
                        aOverlapping = true;
                    }
                }
                else
                {
                    bIndex++;
                    if (bOverlapping)
                    {
                        bOverlapping = false;
                    }
                    else
                    {
                        bOverlapping = true;
                    }
                }
                if (aOverlapping && bOverlapping)
                {
                    output.Add(2);
                }
                else if (aOverlapping || bOverlapping)
                {
                    output.Add(1);
                }
                else
                {
                    output.Add(0);
                }
            }
            if (aIndex == aList.Count)
            {
                while (bIndex < bList.Count)
                {
                    if (bOverlapping)
                    {
                        bOverlapping = false;
                        output.Add(0);
                    }
                    else
                    {
                        bOverlapping = true;
                        output.Add(1);
                    }
                    bIndex++;
                }
            }
            else if (bIndex == bList.Count)
            {
                while (aIndex < aList.Count)
                {
                    if (aOverlapping)
                    {
                        aOverlapping = false;
                        output.Add(0);
                    }
                    else
                    {
                        aOverlapping = true;
                        output.Add(1);
                    }
                    aIndex++;
                }
            }
            return output;
        }

        public static List<double> GetOverlapBoundaries(MultiInterval a,  MultiInterval b)
        {
            List<double> output = new List<double>();
            List<double> aList = a.GetBoundaries();
            List<double> bList = b.GetBoundaries();
            int aIndex = 0;
            int bIndex = 0;
            while (aIndex < aList.Count && bIndex < bList.Count)
            {
                if (aList[aIndex] <= bList[bIndex])
                {
                    output.Add(aList[aIndex]);
                    aIndex++;
                }
                else
                {
                    output.Add(bList[bIndex]);
                    bIndex++;
                }
            }
            if (aIndex == aList.Count)
            {
                while (bIndex < bList.Count)
                {
                    output.Add(bList[bIndex]);
                    bIndex++;
                }
            }
            else if (bIndex == bList.Count)
            {
                while (aIndex < aList.Count)
                {
                    output.Add(aList[aIndex]);
                    aIndex++;
                }
            }
            return output;
        }

        public static MultiInterval Exponentiation(MultiInterval a, MultiInterval b)
        {
            MultiInterval output = new MultiInterval();
            output.Intervals.Clear();
            foreach (Interval aInterval in a.Intervals)
            {
                foreach (Interval bInterval in b.Intervals)
                {
                    output |= Interval.Exponentiation(aInterval, bInterval);
                }
            }
            return output;
        }

        public static MultiInterval Logarithm(MultiInterval value, MultiInterval newBase)
        {
            MultiInterval output = new MultiInterval();
            output.Intervals.Clear();
            foreach (Interval valueInterval in value.Intervals)
            {
                foreach (Interval baseInterval in newBase.Intervals)
                {
                    double minimum = valueInterval.Minimum;
                    if (valueInterval.Minimum <= 0)
                    {
                        minimum = 0;
                    }
                    output |= (MultiInterval)(
                        new Interval(Math.Log(minimum), Math.Log(valueInterval.Maximum))) / 
                        new Interval(Math.Log(baseInterval.Minimum), Math.Log(baseInterval.Maximum));
                }
            }
            return output;
        }

        public static MultiInterval Sin(MultiInterval a)
        {
            MultiInterval output = new MultiInterval();
            output.Intervals.Clear();
            foreach (Interval aInterval in a.Intervals)
            {
                output |= Interval.Sin(aInterval);
            }
            return output;
        }

        public static MultiInterval Cos(MultiInterval a)
        {
            MultiInterval output = new MultiInterval();
            output.Intervals.Clear();
            foreach (Interval aInterval in a.Intervals)
            {
                output |= Interval.Cos(aInterval);
            }
            return output;
        }

        public static MultiInterval Tan(MultiInterval a)
        {
            MultiInterval output = new MultiInterval();
            output.Intervals.Clear();
            foreach (Interval aInterval in a.Intervals)
            {
                if (aInterval.Maximum - aInterval.Minimum >= Math.PI)
                {
                    output |= new Interval(Double.NegativeInfinity, Double.PositiveInfinity);
                }
                else
                {
                    double normalisedMinimum = aInterval.Minimum - Math.PI * (int)(aInterval.Minimum / Math.PI);
                    double normalisedMaximum = aInterval.Maximum - Math.PI * (int)(aInterval.Minimum / Math.PI);
                    if (normalisedMinimum < 0)
                    {
                        normalisedMinimum += Math.PI;
                        normalisedMaximum += Math.PI;
                    }

                    if (normalisedMinimum < Math.PI / 2)
                    {
                        // minimum is within first half of first period of tan x
                        if (normalisedMaximum < Math.PI / 2)
                        {
                            // maximum is also within first half of first period of tan x
                            output |= new Interval(Math.Tan(aInterval.Minimum), Math.Tan(aInterval.Maximum));
                        }
                        else
                        {
                            output |= new Interval(Math.Tan(aInterval.Minimum), Double.PositiveInfinity);
                            output |= new Interval(Double.NegativeInfinity, Math.Tan(aInterval.Maximum));
                        }
                    }
                    else
                    {
                        // minimum is within second half of first period of tan x
                        if (normalisedMaximum < 3 * Math.PI / 2)
                        {
                            // maximum is not within second half of second period of tan x
                            output |= new Interval(Math.Tan(aInterval.Minimum), Math.Tan(aInterval.Maximum));
                        }
                        else
                        {
                            output |= new Interval(Math.Tan(aInterval.Minimum), Double.PositiveInfinity);
                            output |= new Interval(Double.NegativeInfinity, Math.Tan(aInterval.Maximum));
                        }
                    }
                }
            }
            return output;
        }

        public static MultiInterval Arcsin(MultiInterval a)
        {
            MultiInterval output = new MultiInterval();
            output.Intervals.Clear();
            foreach (Interval aInterval in a.Intervals)
            {
                output |= Interval.Arcsin(aInterval);
            }
            return output;
        }

        public static MultiInterval Arccos(MultiInterval a)
        {
            MultiInterval output = new MultiInterval();
            output.Intervals.Clear();
            foreach (Interval aInterval in a.Intervals)
            {
                output |= Interval.Arccos(aInterval);
            }
            return output;
        }

        public static MultiInterval Arctan(MultiInterval a)
        {
            MultiInterval output = new MultiInterval();
            output.Intervals.Clear();
            foreach (Interval aInterval in a.Intervals)
            {
                output |= Interval.Arctan(aInterval);
            }
            return output;
        }

        public static MultiInterval Absolute(MultiInterval a)
        {
            MultiInterval output = new MultiInterval();
            foreach (Interval aInterval in a.Intervals)
            {
                output |= Interval.Absolute(aInterval);
            }
            return output;
        }

        public static MultiInterval operator +(MultiInterval a) => a;
        public static MultiInterval operator -(MultiInterval a)
        {
            MultiInterval output = new MultiInterval();
            foreach (Interval interval in a.Intervals)
            {
                output.Intervals.Add(new Interval(-interval.Maximum, -interval.Minimum));
            }
            return output;
        }
        public static MultiInterval operator ~(MultiInterval a)
        {
            List<double> outputBoundaries = new List<double>();
            List<int> overlaps = MultiInterval.GetOverlaps(a, ExtendedReals);
            List<double> list = MultiInterval.GetOverlapBoundaries(a, MultiInterval.ExtendedReals);
            for (int overlapIndex = 1;  overlapIndex < overlaps.Count; overlapIndex++)
            {
                if (overlaps[overlapIndex] == 1)
                {
                    outputBoundaries.Add(list[overlapIndex - 1]);
                }
                else if (overlaps[overlapIndex - 1] == 1)
                {
                    outputBoundaries.Add(list[overlapIndex - 1]);
                }
            }
            if (outputBoundaries.Count >= 2 && Double.IsNegativeInfinity(outputBoundaries[1]))
            {
                outputBoundaries.RemoveAt(0);
                outputBoundaries.RemoveAt(0);
            }
            if (outputBoundaries.Count >= 2 && Double.IsPositiveInfinity(outputBoundaries[outputBoundaries.Count - 2]))
            {
                outputBoundaries.RemoveAt(outputBoundaries.Count - 1);
                outputBoundaries.RemoveAt(outputBoundaries.Count - 1);
            }
          
            MultiInterval output = new MultiInterval();
            for (int index = 0; index < outputBoundaries.Count; index += 2)
            {
                output.Intervals.Add(new Interval(outputBoundaries[index], outputBoundaries[index + 1]));
            }
            return output;
        }

        public static MultiInterval operator +(MultiInterval a, MultiInterval b)
        {
            MultiInterval result = new MultiInterval();
            result.Intervals.Clear();
            foreach (Interval aInterval in a.Intervals)
            {
                foreach (Interval bInterval in b.Intervals)
                {
                    result |= aInterval + bInterval;
                }
            }
            return result;
        }
        public static MultiInterval operator -(MultiInterval a, MultiInterval b)
        {
            MultiInterval result = new MultiInterval();
            result.Intervals.Clear();
            foreach (Interval aInterval in a.Intervals)
            {
                foreach (Interval bInterval in b.Intervals)
                {
                    result |= aInterval - bInterval;
                }
            }
            return result;
        }
        public static MultiInterval operator *(MultiInterval a, MultiInterval b)
        {
            MultiInterval result = new MultiInterval();
            result.Intervals.Clear();
            foreach (Interval aInterval in a.Intervals)
            {
                foreach (Interval bInterval in b.Intervals)
                {
                    result |= aInterval * bInterval;
                }
            }
            return result;
        }
        public static MultiInterval operator /(MultiInterval a, MultiInterval b)
        {
            // complicated
            if (b.Contains(0))
            {
                MultiInterval reciprocal = new MultiInterval();
                reciprocal.Intervals.Clear();
                foreach (Interval interval in b.Intervals)
                {
                    reciprocal |= new Interval(Double.NegativeInfinity, 1 / interval.Minimum);
                    reciprocal |= new Interval(1 / interval.Maximum, Double.PositiveInfinity);
                }
                return a * reciprocal;
            }
            else
            {
                MultiInterval reciprocal = new MultiInterval();
                reciprocal.Intervals.Clear();
                foreach (Interval interval in b.Intervals)
                {
                    reciprocal |= new Interval(1 / interval.Maximum, 1 / interval.Minimum);
                }
                return a * reciprocal;
            }
        }
        public static MultiInterval operator &(MultiInterval a, MultiInterval b)
        {
            List<double> outputBoundaries = new List<double>();
            List<int> overlaps = MultiInterval.GetOverlaps(a, b);
            List<double> list = MultiInterval.GetOverlapBoundaries(a, b);
            for (int overlapIndex = 1; overlapIndex < overlaps.Count; overlapIndex++)
            {
                if (overlaps[overlapIndex] == 2)
                {
                    outputBoundaries.Add(list[overlapIndex - 1]);
                }
                else if (overlaps[overlapIndex - 1] == 2)
                {
                    outputBoundaries.Add(list[overlapIndex - 1]);
                }
            }
            if (outputBoundaries.Count >= 2 && outputBoundaries[0] == outputBoundaries[1])
            {
                outputBoundaries.RemoveAt(0);
                outputBoundaries.RemoveAt(0);
            }
            if (outputBoundaries.Count >= 2 && outputBoundaries[outputBoundaries.Count - 1] == outputBoundaries[outputBoundaries.Count - 2])
            {
                outputBoundaries.RemoveAt(outputBoundaries.Count - 1);
                outputBoundaries.RemoveAt(outputBoundaries.Count - 1);
            }

            MultiInterval output = new MultiInterval();
            for (int index = 0; index < outputBoundaries.Count; index += 2)
            {
                output.Intervals.Add(new Interval(outputBoundaries[index], outputBoundaries[index + 1]));
            }
            return output;
        }
        public static MultiInterval operator |(MultiInterval a, MultiInterval b)
        {
            List<double> outputBoundaries = new List<double>();
            List<int> overlaps = MultiInterval.GetOverlaps(a, b);
            List<double> list = MultiInterval.GetOverlapBoundaries(a, b);
            if (overlaps.Count > 0 && overlaps[0] >= 1)
            {
                outputBoundaries.Add(list[0]);
            }
            for (int overlapIndex = 1; overlapIndex < overlaps.Count; overlapIndex++)
            {
                if (overlaps[overlapIndex] >= 1 && overlaps[overlapIndex - 1] == 0)
                {
                    outputBoundaries.Add(list[overlapIndex]);
                }
                else if (overlaps[overlapIndex] == 0 && overlaps[overlapIndex - 1] >= 1)
                {
                    outputBoundaries.Add(list[overlapIndex]);
                }
            }
            //if (outputBoundaries.Count >= 2 && outputBoundaries[0] == outputBoundaries[1])
            //{
            //    outputBoundaries.RemoveAt(0);
            //    outputBoundaries.RemoveAt(0);
            //}
            //if (outputBoundaries.Count >= 2 && outputBoundaries[outputBoundaries.Count - 1] == outputBoundaries[outputBoundaries.Count - 2])
            //{
            //    outputBoundaries.RemoveAt(outputBoundaries.Count - 1);
            //    outputBoundaries.RemoveAt(outputBoundaries.Count - 1);
            //}

            MultiInterval output = new MultiInterval();
            output.Intervals.Clear();
            for (int index = 0; index < outputBoundaries.Count - 1; index += 2)
            {
                output.Intervals.Add(new Interval(outputBoundaries[index], outputBoundaries[index + 1]));
            }
            return output;
        }
        public static MultiInterval operator ^(MultiInterval a, MultiInterval b)
        {
            List<double> outputBoundaries = new List<double>();
            List<int> overlaps = MultiInterval.GetOverlaps(a, b);
            List<double> list = MultiInterval.GetOverlapBoundaries(a, b);
            for (int overlapIndex = 1; overlapIndex < overlaps.Count; overlapIndex++)
            {
                if (overlaps[overlapIndex] == 1)
                {
                    outputBoundaries.Add(list[overlapIndex - 1]);
                }
                else if (overlaps[overlapIndex - 1] == 1)
                {
                    outputBoundaries.Add(list[overlapIndex - 1]);
                }
            }
            if (outputBoundaries.Count >= 2 && outputBoundaries[0] == outputBoundaries[1])
            {
                outputBoundaries.RemoveAt(0);
                outputBoundaries.RemoveAt(0);
            }
            if (outputBoundaries.Count >= 2 && outputBoundaries[outputBoundaries.Count - 1] == outputBoundaries[outputBoundaries.Count - 2])
            {
                outputBoundaries.RemoveAt(outputBoundaries.Count - 1);
                outputBoundaries.RemoveAt(outputBoundaries.Count - 1);
            }

            MultiInterval output = new MultiInterval();
            for (int index = 0; index < outputBoundaries.Count; index += 2)
            {
                output.Intervals.Add(new Interval(outputBoundaries[index], outputBoundaries[index + 1]));
            }
            return output;
        }

        public static implicit operator MultiInterval(Interval interval)
        {
            MultiInterval output = new MultiInterval();
            output.Intervals.Clear();
            output.Intervals.Add(interval);
            return output;
        }

        public static MultiInterval ExtendedReals = new Interval(Double.NegativeInfinity, Double.PositiveInfinity);
    }
}
