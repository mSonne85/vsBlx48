namespace vsBlx48.Drawing
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Drawing;

    /// <summary>
    /// Provides an <see cref="IComparer{T}"/> implementation for comparing two color objects based on their HSL values.
    /// </summary>
    public sealed class WebColorComparer : IComparer, IComparer<int>, IComparer<Color>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebColorComparer"/> class.
        /// </summary>
        public WebColorComparer()
        {

        }

        /// <summary>
        /// Compares two signed integer or color objects and returns an indication of their relative sort order.
        /// </summary>
        /// <param name="left">A object to compare to right.</param>
        /// <param name="right">A object to compare to left.</param>
        /// <returns>A signed integer that indicates the relative values of left and right. [ -1: left &lt; right, 0: left = right, 1: left &gt; right ]</returns>
        public int Compare(object left, object right)
        {
            if (left is int argb1 && right is int argb2)
                return Compare(argb1, argb2);

            else if (left is Color color1 && right is Color color2)
                return Compare(color1.ToArgb(), color2.ToArgb());

            return 0;
        }

        /// <summary>
        /// Compares two color objects and returns an indication of their relative sort order.
        /// </summary>
        /// <param name="left">A color to compare to right.</param>
        /// <param name="right">A color to compare to left.</param>
        /// <returns>A signed integer that indicates the relative values of left and right. [ -1: left &lt; right, 0: left = right, 1: left &gt; right ]</returns>
        public int Compare(Color left, Color right)
        {
            return Compare(left.ToArgb(), right.ToArgb());
        }

        /// <summary>
        /// Compares two 32-bit ARGB values and returns an indication of their relative sort order.
        /// </summary>
        /// <param name="left">A 32-bit ARGB values to compare to right.</param>
        /// <param name="right">A 32-bit ARGB values to compare to left.</param>
        /// <returns>A signed integer that indicates the relative values of left and right. [ -1: left &lt; right, 0: left = right, 1: left &gt; right ]</returns>
        public int Compare(int left, int right)
        {
            byte xA = (byte)(left >> 24), yA = (byte)(right >> 24);
            if (xA < yA) return -1; else if (xA > yA) return 1;

            byte R = (byte)(left >> 16), G = (byte)(left >> 8), B = (byte)left;
            byte max = R > G ? R > B ? R : B : G > B ? G : B;
            byte min = R < G ? R < B ? R : B : G < B ? G : B;

            double xH = 0.0, xS = 0.0, xL = (max + min) / 510D; if (max != min)
            {
                if      (R == max) xH = 0 + (double)(G - B) / (max - min);
                else if (G == max) xH = 2 + (double)(B - R) / (max - min);
                else if (B == max) xH = 4 + (double)(R - G) / (max - min);

                if (xH < 0.0) xH += 6.0; xS = xL <= 0.5 ?
                    (double)(max - min) / (max + min) : (max - min) / (510D - max - min);
            }

            R = (byte)(right >> 16); G = (byte)(right >> 8); B = (byte)right;
            max = R > G ? R > B ? R : B : G > B ? G : B;
            min = R < G ? R < B ? R : B : G < B ? G : B;

            double yH = 0.0, yS = 0.0, yL = (max + min) / 510D; if (max != min)
            {
                if      (R == max) yH = 0 + (double)(G - B) / (max - min);
                else if (G == max) yH = 2 + (double)(B - R) / (max - min);
                else if (B == max) yH = 4 + (double)(R - G) / (max - min);

                if (yH < 0.0) yH += 6.0; yS = yL <= 0.5 ? 
                    (double)(max - min) / (max + min) : (max - min) / (510D - max - min);
            }

            if (xH < yH) return -1; else if (xH > yH) return 1;
            if (xS < yS) return -1; else if (xS > yS) return 1;
            if (xL < yL) return -1; else if (xL > yL) return 1;

            return 0;
        }
    }

    /// <summary>
    /// Provides an <see cref="IComparer{T}"/> implementation for comparing two color objects based on their names.
    /// </summary>
    public sealed class SystemColorComparer : IComparer, IComparer<string>, IComparer<Color>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SystemColorComparer"/> class.
        /// </summary>
        public SystemColorComparer()
        {

        }

        /// <summary>
        /// Compares two string or color objects and returns an indication of their relative sort order.
        /// </summary>
        /// <param name="left">A object to compare to right.</param>
        /// <param name="right">A object to compare to left.</param>
        /// <returns>A signed integer that indicates the relative values of left and right. [ -1: left &lt; right, 0: left = right, 1: left &gt; right ]</returns>
        public int Compare(object left, object right)
        {
            if (left is string name1 && right is string name2)
                return string.CompareOrdinal(name1, name2);

            else if (left is Color color1 && right is Color color2)
                return string.CompareOrdinal(color1.Name, color2.Name);

            return 0;
        }

        /// <summary>
        /// Compares two color objects and returns an indication of their relative sort order.
        /// </summary>
        /// <param name="left">A color to compare to right.</param>
        /// <param name="right">A color to compare to left.</param>
        /// <returns>A signed integer that indicates the relative values of left and right. [ -1: left &lt; right, 0: left = right, 1: left &gt; right ]</returns>
        public int Compare(Color left, Color right)
        {
            return string.CompareOrdinal(left.Name, right.Name);
        }

        /// <summary>
        /// Compares two strings and returns an indication of their relative sort order.
        /// </summary>
        /// <param name="left">A string to compare to right.</param>
        /// <param name="right">A string to compare to left.</param>
        /// <returns>A signed integer that indicates the relative values of left and right. [ -1: left &lt; right, 0: left = right, 1: left &gt; right ]</returns>
        public int Compare(string left, string right)
        {
            return string.CompareOrdinal(left, right);
        }
    }
}
