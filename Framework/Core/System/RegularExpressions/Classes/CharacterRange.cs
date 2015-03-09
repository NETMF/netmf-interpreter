/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * Ported to C# for the .Net Micro Framework by <a href="mailto:juliusfriedman@gmail.com">Julius Friedman</a>
 * http://netmf.codeplex.com/
 *
 * Local, nested class for maintaining character ranges for character classes.
*/
namespace System.Text.RegularExpressions
{
    /// <summary>
    /// Class for maintaining char ranges for char classes.
    /// </summary>
    internal sealed class CharacterRange
    {
        int size = 16;                      // Capacity of current range arrays
        int[] minimums = new int[16];     // Range minima
        int[] maximums = new int[16];     // Range maxima
        int elements = 0;                        // Number of range array elements in use

        public int Elements { get { return elements; } }
        public int[] Minimums { get { return minimums; } }
        public int[] Maximums { get { return maximums; } }

        /// <summary>
        /// Deletes the range at a given index from the range lists
        /// </summary>
        /// <param name="index">Index of range to delete from minRange and maxRange arrays.</param>
        internal void Delete(int index)
        {
            // Return if no elements left or index is out of range
            if (elements == 0 || index >= elements)
            {
                return;
            }

            // Move elements down
            while (++index < elements)
            {
                if (index - 1 >= 0)
                {
                    minimums[index - 1] = minimums[index];
                    maximums[index - 1] = maximums[index];
                }
            }

            // One less element now
            --elements;
        }

        /// <summary>
        /// Merges a range into the range list, coalescing ranges if possible.
        /// </summary>
        /// <param name="min">min Minimum end of range</param>
        /// <param name="max">max Maximum end of range</param>
        internal void Merge(int min, int max)
        {
            // Loop through ranges
            for (int i = 0; i < elements; ++i)
            {
                // Min-max is subsumed by minRange[i]-maxRange[i]
                if (min >= minimums[i] && max <= maximums[i])
                {
                    return;
                }

                // Min-max subsumes minRange[i]-maxRange[i]
                else if (min <= minimums[i] && max >= maximums[i])
                {
                    Delete(i);
                    Merge(min, max);
                    return;
                }

                // Min is in the range, but max is outside
                else if (min >= minimums[i] && min <= maximums[i])
                {
                    min = minimums[i];
                    Delete(i);
                    Merge(min, max);
                    return;
                }

                // Max is in the range, but min is outside
                else if (max >= minimums[i] && max <= maximums[i])
                {
                    max = maximums[i];
                    Delete(i);
                    Merge(min, max);
                    return;
                }
            }

            // Must not overlap any other ranges
            if (elements >= size)
            {
                size *= 2;
                int[] newMin = new int[size];
                int[] newMax = new int[size];
                System.Array.Copy(minimums, 0, newMin, 0, elements);
                System.Array.Copy(maximums, 0, newMax, 0, elements);
                minimums = newMin;
                maximums = newMax;
            }
            minimums[elements] = min;
            maximums[elements] = max;
            ++elements;
        }

        /// <summary>
        /// Removes a range by deleting or shrinking all other ranges
        /// </summary>
        /// <param name="min">Minimum end of range</param>
        /// <param name="max">Maximum end of range</param>
        internal void Remove(int min, int max)
        {
            // Loop through ranges
            for (int i = 0; i < elements; ++i)
            {
                // minRange[i]-maxRange[i] is subsumed by min-max
                if (minimums[i] >= min && maximums[i] <= max)
                {
                    Delete(i);
                    return;
                }

                // min-max is subsumed by minRange[i]-maxRange[i]
                else if (min >= minimums[i] && max <= maximums[i])
                {
                    int minr = minimums[i];
                    int maxr = maximums[i];
                    Delete(i);
                    if (minr < min)
                    {
                        Merge(minr, min - 1);
                    }
                    if (max < maxr)
                    {
                        Merge(max + 1, maxr);
                    }
                    return;
                }

                // minRange is in the range, but maxRange is outside
                else if (minimums[i] >= min && minimums[i] <= max)
                {
                    minimums[i] = max + 1;
                    return;
                }

                // maxRange is in the range, but minRange is outside
                else if (maximums[i] >= min && maximums[i] <= max)
                {
                    maximums[i] = min - 1;
                    return;
                }
            }
        }

        /// <summary>
        /// Includes (or excludes) the range from min to max, inclusive.
        /// </summary>
        /// <param name="min">Minimum end of range</param>
        /// <param name="max">Maximum end of range</param>
        /// <param name="include">True if range should be included.  False otherwise.</param>
        internal void Include(int min, int max, bool include)
        {
            if (include)
            {
                Merge(min, max);
            }
            else
            {
                Remove(min, max);
            }
        }

        /// <summary>
        /// Includes a range with the same min and max
        /// </summary>
        /// <param name="minmax">Minimum and maximum end of range (inclusive)</param>
        /// <param name="shouldInclude">True if range should be included.  False otherwise.</param>
        internal void Include(char minmax, bool shouldInclude)
        {
            Include(minmax, minmax, shouldInclude);
        }
    }
}
