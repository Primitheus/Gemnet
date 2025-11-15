using System;
using System.Collections.Generic;

namespace Gemnet.Shop.Boxes
{
    public static class BoxRegistry
    {
        private static readonly Dictionary<int, Box> _boxes = new();

        public static void RegisterBox(Box box)
        {
            if (_boxes.ContainsKey(box.BoxID))
                throw new InvalidOperationException($"Box with ID {box.BoxID} already registered.");

            _boxes[box.BoxID] = box;
        }

        public static Box? GetBox(int boxId)
        {
            if (_boxes.TryGetValue(boxId, out var box))
                return box;

            return null;
        }

        public static IEnumerable<Box> GetAllBoxes() => _boxes.Values;
    }
}
