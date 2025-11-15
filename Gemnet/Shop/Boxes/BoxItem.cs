using System;

namespace Gemnet.Shop.Boxes
{
    public class BoxItem
    {
        public int ItemID { get; set; }
        public string ItemName { get; set; }
        public int ItemType { get; set; } // 0 = Perm, 1 = Temp
        public int Quantity { get; set; }
        public int Probability { get; set; } // If 0, probability will be auto-calculated
    }
}
