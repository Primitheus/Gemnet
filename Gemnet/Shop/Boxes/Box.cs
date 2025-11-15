using System;
using System.Collections.Generic;
using System.Linq;

namespace Gemnet.Shop.Boxes
{
    public class Box
    {
        public int BoxID { get; set; }
        public string BoxName { get; set; }
        public List<BoxItem> PossibleItems { get; set; }

        private static readonly Random rand = new Random();

        public BoxItem GetRandomItem()
        {
            int fixedSum = PossibleItems.Where(i => i.Probability > 0).Sum(i => i.Probability);
            var flexibleItems = PossibleItems.Where(i => i.Probability <= 0).ToList();

            int remaining = 100 - fixedSum;
            if (remaining < 0)
                throw new InvalidOperationException("Sum of fixed probabilities exceeds 100!");

            if (flexibleItems.Any())
            {
                int share = remaining / flexibleItems.Count;
                int remainder = remaining % flexibleItems.Count;

                foreach (var item in flexibleItems)
                    item.Probability = share;

                for (int i = 0; i < remainder; i++)
                    flexibleItems[i].Probability++;
            }

            int roll = rand.Next(1, 101);
            int cumulative = 0;

            foreach (var item in PossibleItems)
            {
                cumulative += item.Probability;
                if (roll <= cumulative)
                    return item;
            }

            return null; // should never happen
        }
    }
}
