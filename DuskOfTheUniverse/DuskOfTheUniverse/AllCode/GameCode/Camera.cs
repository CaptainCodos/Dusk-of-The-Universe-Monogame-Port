using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DuskOfTheUniverse
{
    class Camera
    {
        // Camera transforms
        private Matrix m_transform;
        private Matrix m_inversetransform;

        // Camera position
        private Vector2 m_position;

        // Game viewport
        private Viewport m_viewport;

        // Scroll val
        private Int32 m_scroll;

        // Camera zoom
        private float m_zoom;

        // Checks if the current screen is construction screen
        private bool m_inConstruction;

        // Checks if the scroll function is available
        private bool m_scrollAvailable;

        // Camera speed
        private int m_speed;

        public float Zoom { get { return m_zoom; } set { m_zoom = value; } } 
        public Matrix Transform { get { return m_transform; } set { m_transform = value; } } 
        public Matrix InverseTransform { get { return m_inversetransform; } set { m_inversetransform = value; } }

        public Vector2 Position { get { return m_position; } set { m_position = value; } } 
        public float PosX { get { return m_position.X; } set { m_position.X = value; } } 
        public float PosY { get { return m_position.Y; } set { m_position.Y = value; } }

        public bool InConstruction { get { return m_inConstruction; } set { m_inConstruction = value; } }
        public bool ScrollAvailable { get { return m_scrollAvailable; } set { m_scrollAvailable = value; } }

        // Camera Class Constructor
        public Camera(Viewport viewPort)
        {
            m_zoom = 1f;
            m_scroll = 1;
            m_position = Vector2.Zero;
            m_viewport = viewPort;
            m_inConstruction = false;
            m_scrollAvailable = true;

            m_speed = 300;
        }

        // Update Camera
        public void UpdateMe(MouseState mouse, KeyboardState keys, KeyboardState oldkeys, GameTime gt)
        {
            // Run the camera location properties through a matrix.
            m_transform = Matrix.CreateTranslation(new Vector3(-m_position.X, -m_position.Y, 0)) * Matrix.CreateScale(new Vector3(m_zoom, m_zoom, 1)) * Matrix.CreateTranslation(new Vector3(m_viewport.Width * 0.5f, m_viewport.Height * 0.5f, 0));
            m_inversetransform = Matrix.Invert(m_transform);

            // Get keyboard and mouse input.
            Input(mouse, keys, oldkeys, gt);
        }

        // Allows the camera to be zoomed and moved around.
        public void Input(MouseState mouse, KeyboardState keys, KeyboardState oldkeys, GameTime gt)
        {
            Vector2 newPos = m_position;
            float elapsedTime = (float)gt.ElapsedGameTime.TotalSeconds;

            if (keys.IsKeyDown(Keys.A))
            {
                Position += new Vector2(-m_speed * elapsedTime, 0);
            }
            if (keys.IsKeyDown(Keys.D))
            {
                Position += new Vector2(m_speed * elapsedTime, 0);
            }
            if (keys.IsKeyDown(Keys.W))
            {
                Position += new Vector2(0, -m_speed * elapsedTime);
            }
            if (keys.IsKeyDown(Keys.S))
            {
                Position += new Vector2(0, m_speed * elapsedTime);
            }


            if (m_scrollAvailable)
            {
                if (mouse.ScrollWheelValue > m_scroll)
                {
                    m_zoom += 0.1f;
                    m_scroll = mouse.ScrollWheelValue;
                }
                else if (mouse.ScrollWheelValue < m_scroll)
                {
                    m_zoom -= 0.1f;
                    m_scroll = mouse.ScrollWheelValue;
                }
            }

            m_scroll = mouse.ScrollWheelValue;

            // If the current menu is construction, then lock the zoom differently and alter the speed
            if (m_inConstruction)
            {
                if (m_zoom < 3)
                    m_zoom = 3;
                if (m_zoom > 10)
                    m_zoom = 10;

                // Constrains the camera to the construction area. (Bit of a cheating way of doing it)
                Position = Vector2.Clamp(m_position, new Vector2(126 / m_zoom, 126 / m_zoom), new Vector2(252 - (126 / m_zoom), (252 - (126 / m_zoom))));

                m_speed = 100;
            }
            else
            {
                if (m_zoom < 0.8f)
                    m_zoom = 0.8f;
                if (m_zoom > 3)
                    m_zoom = 3;

                // Constrains the camera to the map. (Bit of a cheating way of doing it)
                Position = Vector2.Clamp(m_position, new Vector2(960 / m_zoom, 540 / m_zoom), new Vector2(2875 - (960 / m_zoom), (1615 - (540 / m_zoom)) + (216 / m_zoom)));

                m_speed = 300;
            }
        }
    }
}
