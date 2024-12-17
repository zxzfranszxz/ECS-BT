using UnityEngine;

namespace Utils.Vector
{
    public static class VectorHelper
    {
        public static bool TryParseVector2(string value, out Vector2 result)
        {
            result = Vector2.zero;
            if (value == null) return false;
            value = value.Trim('(', ')');
            var parts = value.Split(',');
            if (parts.Length == 2 &&
                float.TryParse(parts[0], out var x) &&
                float.TryParse(parts[1], out var y))
            {
                result = new Vector2(x, y);
                return true;
            }
            result = Vector2.zero;
            return false;
        }

        public static bool TryParseVector3(string value, out Vector3 result)
        {
            value = value.Trim('(', ')');
            var parts = value.Split(',');
            if (parts.Length == 3 &&
                float.TryParse(parts[0], out var x) &&
                float.TryParse(parts[1], out var y) &&
                float.TryParse(parts[2], out var z))
            {
                result = new Vector3(x, y, z);
                return true;
            }
            result = Vector3.zero;
            return false;
        }
    }
}