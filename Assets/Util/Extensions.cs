using UnityEngine;
using System;

using SciFi.Players;

namespace SciFi.Util.Extensions {
    public static class VectorExtensions {
        /// Returns a new vector with x as the negative of the original,
        /// and y the same.
        public static Vector2 FlipX(this Vector2 vec) {
            return new Vector2(-vec.x, vec.y);
        }

        /// Returns a new vector with x as the negative of the original,
        /// and y and z the same.
        public static Vector3 FlipX(this Vector3 vec) {
            return new Vector3(-vec.x, vec.y, vec.z);
        }

        /// Returns a vector where the x value is negative when the direction is left,
        /// and positive when the direction is right.
        public static Vector2 FlipDirection(this Vector2 vec, Direction direction) {
            if ((direction == Direction.Left) == (vec.x < 0f)) {
                return vec;
            } else {
                return vec.FlipX();
            }
        }

        /// Returns a vector where the x value is negative when the direction is left,
        /// and positive when the direction is right.
        public static Vector3 FlipDirection(this Vector3 vec, Direction direction) {
            if ((direction == Direction.Left) == (vec.x < 0f)) {
                return vec;
            } else {
                return vec.FlipX();
            }
        }
    }

    public static class NumberExtensions {
        /// Returns a negative number when the direction is left, and
        /// a positive number when the direction is right.
        public static int FlipDirection(this int i, Direction direction) {
            if ((direction == Direction.Left) == (i < 0)) {
                return i;
            } else {
                return -i;
            }
        }

        public static float FlipDirection(this float f, Direction direction) {
            if ((direction == Direction.Left) == (f < 0f)) {
                return f;
            } else {
                return -f;
            }
        }

        /// Returns a number that represents the same point in the "to" scale as
        /// value is in the "from" scale - for example, 5.Scale(0, 10, 0, 100) => 50.
        /// It is expected that <c>value</c> is between <c>fromMin</c> and <c>fromMax</c>.
        public static int Scale(this int value, int fromMin, int fromMax, int toMin, int toMax) {
            float percent = ((float)(value - fromMin)) / (fromMax - fromMin);
            return toMin + (int)((toMax - toMin) * percent);
        }

        /// Returns a number that represents the same point in the "to" scale as
        /// value is in the "from" scale - for example, 5.Scale(0, 10, 0, 100) => 50.
        /// It is expected that <c>value</c> is between <c>fromMin</c> and <c>fromMax</c>.
        public static float Scale(this float value, float fromMin, float fromMax, float toMin, float toMax) {
            float percent = (value - fromMin) / (fromMax - fromMin);
            return toMin + (toMax - toMin) * percent;
        }
    }

    public static class DirectionExtensions {
        /// Selects between two values based on the direction.
        public static T LeftRight<T>(this Direction direction, T left, T right) {
            if (direction == Direction.Left) {
                return left;
            } else if (direction == Direction.Right) {
                return right;
            } else {
                throw new ArgumentException("Direction should be left or right only", "direction");
            }
        }

        public static Direction Opposite(this Direction direction) {
            switch (direction) {
            case Direction.Left:
                return Direction.Right;
            case Direction.Right:
                return Direction.Left;
            case Direction.Down:
                return Direction.Up;
            case Direction.Up:
                return Direction.Down;
            default:
                return Direction.Invalid;
            }
        }
    }

    public static class ColorExtensions {
        /// Changes the alpha value for a color, leaving the color values the same.
        public static Color WithAlpha(this Color c, float alpha) {
            return new Color(c.r, c.g, c.b, alpha);
        }

        /// Changes the color but leaves the alpha the same.
        public static Color WithColor(this Color c, float red, float green, float blue) {
            return new Color(red, green, blue, c.a);
        }
    }
}