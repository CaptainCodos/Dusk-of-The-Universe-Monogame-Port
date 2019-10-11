using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{
    // Circle will often be used for either collision or as a distance check.
    class Circle
    {
        // Holds the properties of the circle.
        private Vector2 m_position;
        private float m_radius;

        public Vector2 Centre { get { return m_position; } set { m_position = value; } }
        public float Radius { get { return m_radius; } set { m_radius = value; } }

        public float X { get { return m_position.X; } }
        public float Y { get { return m_position.Y; } }

        // Circle Constructor.
        public Circle(float x, float y, float radius)
        {
            m_position = new Vector2(x, y);
            m_radius = radius;
        }

        // Checks if the circle contains a rectangle.
        public bool Contains(Rectangle rectangle)
        {
            double[] distance = new double[4];

            // Get the distance between the centre of the circle and each point on rectangle.
            distance[0] = Math.Sqrt(Math.Pow(rectangle.Left - m_position.X, 2) + Math.Pow(rectangle.Top - m_position.Y, 2));
            distance[1] = Math.Sqrt(Math.Pow(rectangle.Right - m_position.X, 2) + Math.Pow(rectangle.Top - m_position.Y, 2));
            distance[2] = Math.Sqrt(Math.Pow(rectangle.Left - m_position.X, 2) + Math.Pow(rectangle.Bottom - m_position.Y, 2));
            distance[3] = Math.Sqrt(Math.Pow(rectangle.Right - m_position.X, 2) + Math.Pow(rectangle.Bottom - m_position.Y, 2));

            // Check if each rectangle point is within the circle's radius.
            if (distance[0] <= m_radius && distance[1] <= m_radius && distance[2] <= m_radius && distance[3] <= m_radius)
                return true;
            else
                return false;
        }

        // Checks if the circle contains another circle.
        public bool Contains(Circle circle)
        {
            double distance;

            // Get the distance between the centres of both circles and add the radius of the second.
            distance = Math.Sqrt(Math.Pow(circle.X - m_position.X, 2) + Math.Pow(circle.Y - m_position.Y, 2)) + circle.Radius;

            // If the distance (plus other's radius) is within this circle's radius, then it contains the other circle.
            if (distance <= m_radius)
                return true;
            else
                return false;
        }

        // Checks if the circle contains a position.
        public bool Contains(Vector2 position)
        {
            double distance;

            // Get distance from the centre to the position.
            distance = Math.Sqrt(Math.Pow(position.X - m_position.X, 2) + Math.Pow(position.Y - m_position.Y, 2));

            // If the position is with a radius of the circle, then it is contained.
            if (distance < m_radius)
                return true;
            else
                return false;
        }

        // Checks if the circle intersects another circle
        public bool Intersects(Circle circle)
        {
            double distance;

            // Get the distance between the centres of both circles and add the radius of the second.
            distance = Math.Sqrt(Math.Pow(circle.X - m_position.X, 2) + Math.Pow(circle.Y - m_position.Y, 2));

            // If the distance (plus other's radius) is within this circle's radius, then it contains the other circle.
            if (distance < m_radius + circle.Radius)
                return true;
            else
                return false;
        }
    }
}
