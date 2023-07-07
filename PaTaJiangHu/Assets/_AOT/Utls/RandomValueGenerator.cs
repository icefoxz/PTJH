using System;

namespace AOT._AOT.Utls
{
    public class RandomValueGenerator
    {
        private Random _random = new Random(DateTime.Now.Millisecond);

        public int[] Generate(int size, int minValue, int maxValue, int total)
        {
            if (size * maxValue < total)
            {
                throw new ArgumentException(
                    $"The total({total}) is outside of the possible MAX({size * maxValue}) range given the constraints.");
            }
            if (size * minValue > total)
            {
                throw new ArgumentException(
                    $"The total({total}) is outside of the possible MIN({size * minValue}) range given the constraints.");
            }

            double[] proportions = new double[size];
            double totalProportions = 0;
            for (int i = 0; i < size; i++)
            {
                proportions[i] = _random.NextDouble();
                totalProportions += proportions[i];
            }

            int[] values = new int[size];
            int currentTotal = 0;
            for (int i = 0; i < size; i++)
            {
                // calculate each value based on its proportion of the total
                values[i] = (int)(total * (proportions[i] / totalProportions));

                // ensure values fall within the min and max constraints
                values[i] = Math.Max(minValue, Math.Min(maxValue, values[i]));

                currentTotal += values[i];
            }

            // correct any rounding errors to ensure the values sum to the total
            int diff = total - currentTotal;
            for (int i = 0; i < size && diff != 0; i++)
            {
                int adjustment = Math.Max(minValue - values[i], Math.Min(maxValue - values[i], diff));
                values[i] += adjustment;
                diff -= adjustment;
            }

            return values;
        }
    }
}