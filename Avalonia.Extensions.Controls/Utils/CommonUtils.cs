using System;

namespace Avalonia.Extensions.Controls
{
    public static class CommonUtils
    {
        public static double ToRadians(this double angle)
        {
            return Math.PI * angle / 180;
        }
        public static double Round(this double d, int decimals)
        {
            return Math.Round(d, decimals);
        }
        public static bool SmallerThan(this PixelPoint pixelPoint, PixelPoint point, bool inCludeEquals = false)
        {
            if (inCludeEquals && pixelPoint.X <= point.X && pixelPoint.Y <= point.Y)
                return true;
            else
                return pixelPoint.X < point.X && pixelPoint.Y < point.Y;
        }
        public static bool BiggerThan(this PixelPoint pixelPoint, PixelPoint point, bool inCludeEquals = false)
        {
            if (inCludeEquals && pixelPoint.X >= point.X && pixelPoint.Y >= point.Y)
                return true;
            else
                return pixelPoint.X > point.X && pixelPoint.Y > point.Y;
        }
        public static int ToInt32(this object obj)
        {
            try
            {
                if (obj is int result)
                    return result;
                else if (obj is double d && double.IsNaN(d))
                    return 0;
                else
                {
                    if (int.TryParse(obj.ToString(), out result))
                        return result;
                    else
                        return Convert.ToInt32(obj.ToString());
                }
            }
            catch
            {
                return 0;
            }
        }
        /// <summary>
        /// only target is Initialized can get the value
        /// </summary>
        /// <param name="visual">target</param>
        /// <returns>size</returns>
        public static double ActualWidth(this Visual visual)
        {
            return visual.Bounds.Width;
        }
        /// <summary>
        /// only target is Initialized can get the value
        /// </summary>
        /// <param name="visual">target</param>
        /// <returns>size</returns>
        public static double ActualHeight(this Visual visual)
        {
            return visual.Bounds.Height;
        }
    }
}