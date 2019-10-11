using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{
    class StaticGraphic
    {
        // Texture and position and rectangle to be used.
        public Texture2D m_txr;
        protected Vector2 m_position;
        protected Rectangle m_rect;

        protected Color[] m_txrData;

        // Tint of the sprite.
        protected Color m_tint;

        protected List<Color> m_altTints;

        public Vector2 Position
        {
            get { return m_position; }
            set { m_position = value; }
        }

        public Texture2D Txr
        {
            get { return m_txr; }
        }

        public Color Tint
        {
            get { return m_tint; }
            set { m_tint = value; }
        }
        public byte RedCompontent
        {
            get { return m_tint.R; }
            set { m_tint.R = value; }
        }
        public byte GreenCompontent
        {
            get { return m_tint.G; }
            set { m_tint.G = value; }
        }
        public byte BlueCompontent
        {
            get { return m_tint.B; }
            set { m_tint.B = value; }
        }

        public List<Color> AltTints
        {
            get { return m_altTints; }
            set { m_altTints = value; }
        }

        public Rectangle Rect
        {
            get { return m_rect; }
            set { m_rect = value; }
        }

        public byte TintAlpha
        {
            get { return m_tint.A; }
            set { m_tint.A = value; }
        }

        public StaticGraphic(Texture2D txr, Vector2 position, Color tint)
        {
            m_txr = txr;
            Position = position;
            m_rect = new Rectangle((int)Position.X, (int)Position.Y, m_txr.Width, m_txr.Height);
            m_tint = tint;
            m_altTints = new List<Color>();
        }

        // Draw sprite
        public virtual void DrawMe(SpriteBatch sb)
        {
            sb.Draw(m_txr, Position, m_tint);
        }
    }

    class CenteredStaticGraphic : StaticGraphic
    {
        // Centre of the sprite in relation to it's texture.
        protected Vector2 m_origin;

        // Scale of sprite / actor.
        protected float m_scale;

        // Sprite Layer
        protected float m_layer;

        public float Layer
        {
            get { return m_layer; }
            set { m_layer = value; }
        }

        public float Scale
        {
            get { return m_scale; }
            set { m_scale = value; }
        }

        public Vector2 Origin { get { return m_origin; } set { m_origin = value; } }

        public CenteredStaticGraphic(Texture2D txr, Vector2 position, Color tint, float scale) : base(txr, position, tint)
        {
            // Set origin to the centre of the texture.
            m_scale = scale;
            m_origin = new Vector2(m_txr.Width / 2, m_txr.Height / 2);
            m_rect = new Rectangle((int)Position.X - (int)(m_origin.X * m_scale), (int)Position.Y - (int)(m_origin.Y * m_scale), (int)(m_txr.Width * m_scale), (int)(m_txr.Height * m_scale));
            m_layer = 1;
        }
        
        // Draw Centred Sprite
        public override void DrawMe(SpriteBatch sb)
        {
            sb.Draw(m_txr, Position, null, m_tint, 0, m_origin, m_scale, SpriteEffects.None, m_layer);
        }
    }

    class MotionGraphic : CenteredStaticGraphic
    {
        // Velocity of sprite / actor.
        protected Vector2 m_velocity;

        // Rotation and rotating speed of sprite / actor.
        protected float m_rot;
        protected float m_rotSpeed;

        public float Rotation { get { return m_rot; } set { m_rot = value; } }

        public Vector2 Velocity { get { return m_velocity; } set { m_velocity = value; } }

        public MotionGraphic(Texture2D txr, Vector2 position, Color tint, Vector2 startSpeed, float rotationSpeed, float scale) : base(txr, position, tint, scale)
        {
            m_rot = 0;
            m_rotSpeed = rotationSpeed;
            m_velocity = startSpeed;
        }

        // Move and rotate sprite / actor.
        public virtual void UpdateMe(GameTime gt)
        {
            m_rot += m_rotSpeed;
            m_position += 60 * m_velocity * (float)gt.ElapsedGameTime.TotalSeconds;
            m_rect.X = (int)Position.X - (int)(m_origin.X * m_scale);
            m_rect.Y = (int)Position.Y - (int)(m_origin.Y * m_scale);
        }

        // Draw Motion Sprite / Actor.
        public override void DrawMe(SpriteBatch sb)
        {
            sb.Draw(m_txr, Position, null, m_tint, m_rot, m_origin, m_scale, SpriteEffects.None, m_layer);
        }
    }

    class AnimGraphic : MotionGraphic
    {
        // Rectangle to be used to get the visible section of sprite / actor's spritesheet.
        protected Rectangle m_srcRect;

        // Time counter and frame rate of sprite / actor's animation.
        protected float m_animTime;
        protected int m_fps;

        public Rectangle SourceRectangle { get { return m_srcRect; } set { m_srcRect = value; } }
        public int SourceRectX { get { return m_srcRect.X; } set { m_srcRect.X = value; } }
        public int SourceRectY { get { return m_srcRect.Y; } set { m_srcRect.Y = value; } }
        public int SourceRectWidth { get { return m_srcRect.Width; } set { m_srcRect.Width = value; } }
        public int SourceRectHeight { get { return m_srcRect.Height; } set { m_srcRect.Height = value; } }

        public AnimGraphic(Texture2D txr, Vector2 position, Color tint, Vector2 startSpeed, float rotationSpeed, float scale, int fps, int framesX, int framesY) : base(txr, position, tint, startSpeed, rotationSpeed, scale)
        {
            // Set up the source rectangle to the size of a single animation cell.
            m_srcRect = new Rectangle(2, 2, m_txr.Width / framesX - 4, m_txr.Height / framesY - 4);

            // Set a new origin to take the sprite sheet size into account.
            m_origin = new Vector2((m_txr.Width / framesX) / 2 - 2, (m_txr.Height / framesY) / 2 - 2);

            // Set up the normal rectangle so that it isn't too large.
            m_rect = new Rectangle((int)m_position.X - (int)m_origin.X, (int)m_position.Y - (int)m_origin.Y, m_txr.Width / framesX, m_txr.Height / framesY);

            m_animTime = 1;
            m_fps = fps;
        }

        // Draw and animate sprite / actor.
        public virtual void DrawMe(SpriteBatch sb, GameTime gt)
        {
            // If the animation time is less than or equal to 0, increment animation cell to the next cell.
            if (m_animTime <= 0)
            {
                m_srcRect.X += m_srcRect.Width + 4;

                // If the animation source rectangle exceeds the texture size, reset it to the beginning.
                if (m_srcRect.X >= m_txr.Width)
                    m_srcRect.X = 2;

                // Reset timer.
                m_animTime = 1;
            }
            else // Take away time from the counter in relation to the fps given.
                m_animTime -= (float)gt.ElapsedGameTime.TotalSeconds * m_fps;

            sb.Draw(m_txr, Position, m_srcRect, m_tint, m_rot, m_origin, m_scale, SpriteEffects.None, m_layer);
        }
    }
}
