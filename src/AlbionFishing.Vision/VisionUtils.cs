using System;

namespace AlbionFishing.Vision
{
    /// <summary>
    /// Utility class for vision-related helper methods
    /// </summary>
    public static class VisionUtils
    {
        /// <summary>
        /// Determines which side (left or right) the bobber is detected on within the detection region
        /// </summary>
        /// <param name="posX">X-coordinate of the detected bobber's center</param>
        /// <param name="width">Width of the detection region</param>
        /// <returns>"esquerdo" for left side, "direito" for right side</returns>
        public static string GetBobberLado(float posX, int width)
        {
            var middle = width / 2.0f;
            return posX < middle ? "esquerdo" : "direito";
        }

        /// <summary>
        /// Determines which side (left or right) the bobber is detected on within the detection region
        /// </summary>
        /// <param name="posX">X-coordinate of the detected bobber's center</param>
        /// <param name="width">Width of the detection region</param>
        /// <returns>"esquerdo" for left side, "direito" for right side</returns>
        public static string GetBobberLado(double posX, int width)
        {
            return GetBobberLado((float)posX, width);
        }

        /// <summary>
        /// Gets a visual indicator for the bobber's side
        /// </summary>
        /// <param name="posX">X-coordinate of the detected bobber's center</param>
        /// <param name="width">Width of the detection region</param>
        /// <returns>Arrow emoji indicating the side</returns>
        public static string GetBobberSideIndicator(float posX, int width)
        {
            var middle = width / 2.0f;
            return posX < middle ? "↪️" : "↩️";
        }

        /// <summary>
        /// Gets a visual indicator for the bobber's side
        /// </summary>
        /// <param name="posX">X-coordinate of the detected bobber's center</param>
        /// <param name="width">Width of the detection region</param>
        /// <returns>Arrow emoji indicating the side</returns>
        public static string GetBobberSideIndicator(double posX, int width)
        {
            return GetBobberSideIndicator((float)posX, width);
        }

        /// <summary>
        /// Calculates the distance from the center of the detection region
        /// </summary>
        /// <param name="posX">X-coordinate of the detected bobber's center</param>
        /// <param name="width">Width of the detection region</param>
        /// <returns>Distance from center (0 = center, positive = right side, negative = left side)</returns>
        public static float GetDistanceFromCenter(float posX, int width)
        {
            var center = width / 2.0f;
            return posX - center;
        }

        /// <summary>
        /// Calculates the distance from the center of the detection region
        /// </summary>
        /// <param name="posX">X-coordinate of the detected bobber's center</param>
        /// <param name="width">Width of the detection region</param>
        /// <returns>Distance from center (0 = center, positive = right side, negative = left side)</returns>
        public static float GetDistanceFromCenter(double posX, int width)
        {
            return GetDistanceFromCenter((float)posX, width);
        }
    }
} 