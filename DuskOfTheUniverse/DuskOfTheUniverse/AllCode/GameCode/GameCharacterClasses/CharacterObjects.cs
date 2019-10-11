using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{
    // General Character State
    enum CharacterState
    {
        Idle, Travelling, Fighting, Dead
    }

    // Basic Game Character
    class GameChar : AnimGraphic
    {
        // Character health stats (ally health can exceed base max health)
        protected float m_health;
        protected float m_baseMaxHealth;
        protected float m_maxHealth;
        // Character health bar
        protected CharHealthBar m_healthBar;

        // Character coordinates on the map
        protected int m_xCoords;
        protected int m_yCoords;

        // Fighting target
        protected GameChar m_target;
        // Combat Variables (allies can exceed base stats through motivation)
        protected float m_baseDamage;
        protected float m_damage;
        protected float m_baseDamageTick;
        protected float m_damageTick;
        protected float m_currTick;

        // Detection circles for finding nearby tiles
        protected Circle m_innerDetectionCircle;
        protected Circle m_outerDetectionCircle;
        // Circle for view
        protected Circle m_viewCircle;
        // Circle for initiating a fight
        protected Circle m_fightingCircle;
        // Circle for snaping to tiles
        protected Circle m_snapCircle;
        // Circle for selection the character
        protected Circle m_selectionCircle;

        // Queue for pathfinding
        protected Queue<MapTile> m_path;

        // Determines how fast character moves
        // Speed can be modified through towers
        protected float m_baseSpeed;
        protected float m_speed;
        // Target rotation the character is trying to achieve
        protected float m_targetRot;

        // Has the character been selected?
        protected bool m_isSelected;

        // Check if character is moving
        protected bool m_isMoving;

        // Current character state
        protected CharacterState m_state;

        // Check if character is shaded
        protected bool m_isShaded;

        #region Circles
        public Circle InnerDetectionCircle { get { return m_innerDetectionCircle; } set { m_innerDetectionCircle = value; } }
        public Circle OuterDetectionCircle { get { return m_outerDetectionCircle; } set { m_outerDetectionCircle = value; } }

        public Circle ViewCircle { get { return m_viewCircle; } set { m_viewCircle = value; } }
        public Circle FightingCircle { get { return m_fightingCircle; } set { m_fightingCircle = value; } }

        public Circle SelectionCircle { get { return m_selectionCircle; } }
        #endregion

        #region Health Vars
        public CharHealthBar HealthBar { get { return m_healthBar; } }

        public float Health { get { return m_health; } set { m_health = value; } }
        public float BaseMaxHealth { get { return m_baseMaxHealth; } }
        public float MaxHealth { get { return m_maxHealth; } set { m_maxHealth = value; } }
        #endregion

        #region Combat Vars
        public GameChar Target { get { return m_target; } set { m_target = value; } }

        public float BaseDamage { get { return m_baseDamage; } }
        public float Damage { get { return m_damage; } set { m_damage = value; } }
        public float BaseTick { get { return m_baseDamageTick; } }
        public float DamageTick { get { return m_damageTick; } set { m_damageTick = value; } }
        #endregion

        // Coordinates
        public int CoordsX{ get { return m_xCoords; } set { m_xCoords = value; } }
        public int CoordsY { get { return m_yCoords; } set { m_yCoords = value; } }

        // Selected bools
        public bool IsSelected { get { return m_isSelected; } set { m_isSelected = value; } }
        public bool IsShaded { get { return m_isShaded; } set { m_isShaded = value; } }

        // Character path
        public Queue<MapTile> Path { get { return m_path; } set { m_path = value; } }

        // Character speed
        public float BaseSpeed { get { return m_baseSpeed; } }
        public float Speed { get { return m_speed; } set { m_speed = value; } }

        public GameChar(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, int maxHealth, float speed, float detectionRadius, float damage, float tickTime) 
            : base(txr, position, tint, Vector2.Zero, 0, scale, fps, framesX, framesY)
        {
            m_baseMaxHealth = maxHealth;
            m_maxHealth = maxHealth;
            m_health = maxHealth;

            m_baseDamage = damage;
            m_damage = damage;
            m_baseDamageTick = tickTime;
            m_damageTick = tickTime;
            m_currTick = m_damageTick;

            m_baseSpeed = speed;
            m_speed = speed;

            m_targetRot = 0;
            m_rotSpeed = MathHelper.Pi;

            m_innerDetectionCircle = new Circle(m_position.X, m_position.Y, (detectionRadius * 36) - 18);
            m_outerDetectionCircle = new Circle(m_position.X, m_position.Y, (detectionRadius * 36) + 18);
            m_viewCircle = new Circle(m_position.X, m_position.Y, (detectionRadius * 36) * 2);
            m_fightingCircle = new Circle(m_position.X, m_position.Y, 36);
            m_snapCircle = new Circle(m_position.X, m_position.Y, m_speed / 2);
            m_selectionCircle = new Circle(m_position.X, m_position.Y, 18);

            m_state = CharacterState.Idle;

            m_isMoving = false;
            m_isSelected = false;
            m_isShaded = false;
        }

        // Lock character HP
        protected void LockHP()
        {
            m_maxHealth = (int)m_maxHealth;
            m_baseMaxHealth = (int)m_baseMaxHealth;
            m_health = (int)m_health;

            if (m_health < 0)
                m_health = 0;
            else if (m_health > m_maxHealth)
                m_health = m_maxHealth;
        }

        // Charcter finds fastest route to target rotation
        // Then character gradually turns to face the target rotation
        protected void UpdateRotation(GameTime gt)
        {
            // Face current moving direction
            if (m_velocity.Length() > 0)
            {
                // Get rotation of moving direction
                m_targetRot = (float)Math.Atan2(m_velocity.Y, m_velocity.X) + 1.5707f;

                // Target rotation can range from -Pi to 0 to +Pi (-180 to 0 to +180)
                if (m_targetRot > MathHelper.Pi)
                    m_targetRot -= MathHelper.Pi * 2;
                if (m_targetRot < -MathHelper.Pi)
                    m_targetRot += MathHelper.Pi * 2;

                // Get rotation distances
                float rot1 = m_targetRot;
                float rot2 = m_targetRot - (MathHelper.Pi * 2);

                float check1 = rot1 - m_rot;
                float check2 = rot2 - m_rot;

                // Figure out which is shorter
                if (Math.Abs(check1) < Math.Abs(check2))
                    m_targetRot = rot1;
                else
                    m_targetRot = rot2;

                // Turn to face the target rotation
                if (m_rot < m_targetRot - 0.1f)
                    m_rot += 2 * m_rotSpeed * (float)gt.ElapsedGameTime.TotalSeconds;
                else if (m_rot > m_targetRot + 0.1f)
                    m_rot -= 2 * m_rotSpeed * (float)gt.ElapsedGameTime.TotalSeconds;
                else
                    m_rot = m_targetRot;
            }
        }

        // Shade the character
        protected void Selection()
        {
            if (m_isSelected)
            {
                m_tint = Color.LightGreen;
                m_healthBar.CurrShader = m_healthBar.Shaders[1];
            }
            else
            {
                m_tint = Color.White;
                m_healthBar.CurrShader = m_healthBar.Shaders[0];
            }
        }

        // If the character's health is equal to the base health or greater, then leave the health bar alone
        // Otherwise bring the maxhealth back to normal levels again
        protected void ClampOverHeal()
        {
            if (m_health < m_baseMaxHealth)
                m_maxHealth = m_baseMaxHealth;
        }
    }

    // VIP Character
    class ImportantChar : GameChar
    {
        // The target coordinates of the mouse on the map.
        private Vector2 m_targetCoords;
        private int m_targetIndexX;
        private int m_targetIndexY;

        // The node graph for navigation.
        private Map m_map;
        private NodeGraph m_graph;
        private GBFsearch m_search;

        // Vector to store the mouse screen coordinates.
        private Vector2 m_mousePos;

        // Class Constructor.
        public ImportantChar(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, int maxHealth, float speed, float detectionRadius, UniMapData data, ContentManager content, Map map) 
            : base(txr, position, tint, 1, fps, framesX, framesY, maxHealth, speed, detectionRadius, 0, 0)
        {
            m_isSelected = false;
            m_isMoving = false;
            m_isShaded = true;
            m_targetCoords = Vector2.Zero;
            m_velocity = Vector2.Zero;
            m_map = map;
            m_graph = new NodeGraph(data);
            m_search = new GBFsearch(m_graph, m_map);
            m_xCoords = (int)(position.X / 36);
            m_yCoords = (int)(position.Y / 36);

            m_path = new Queue<MapTile>();

            m_healthBar = new CharHealthBar("Art\\GameArt\\GameplayGUI")
                .SetPosition(new Vector2(1300, 960))
                .LoadShader(content, "Shaders\\SmallHighlightEffect")
                .LoadTextures(content, "\\AllyHealthBarBorder", "\\AllyHealthBar", "\\AllyHealthBar", 1)
                .SetHpValues((int)m_health)
                .SetColours(Color.White, Color.Black, Color.Red)
                .SetParentOfBar(this);
        }

        public void UpdateMe(InputManager input, Camera cam, GameTime gt)
        {
            LockHP();
            ClampOverHeal();
            m_healthBar.UpdateBar();

            // Update the rectangle position, mouse position and mouse target coordinates.
            #region Update Coordinates
            // Move the rectangle with the character.
            m_rect.X = (int)(m_position.X - m_origin.X);
            m_rect.Y = (int)(m_position.Y - m_origin.Y);
            m_snapCircle.Centre = m_position;
            m_viewCircle.Centre = m_position;
            m_outerDetectionCircle.Centre = m_position;
            m_innerDetectionCircle.Centre = m_position;
            m_fightingCircle.Centre = m_position;
            m_selectionCircle.Centre = m_position;

            // Get the mouse position
            m_mousePos.X = input.Mouse.X;
            m_mousePos.Y = input.Mouse.Y;

            // Get the mouse position with respect to the world.
            m_targetCoords = Vector2.Transform(m_mousePos, Matrix.Invert(cam.Transform));
            m_targetCoords = Vector2.Clamp(m_targetCoords, Vector2.Zero, new Vector2(4031, 2265));
            m_targetIndexX = (int)(m_targetCoords.X / 36);
            m_targetIndexY = (int)(m_targetCoords.Y / 36);
            #endregion

            m_map.Tiles[m_xCoords, m_yCoords].IsOccupied = true;
            m_map.Tiles[m_xCoords, m_yCoords].OccupiedChar = this;

            // Update selection and navigation logic.
            GetTarget(input);
            Selection();

            UpdateStates(gt);
        }

        private void UpdateStates(GameTime gt)
        {
            switch (m_state)
            {
                case CharacterState.Idle:
                    m_velocity = Vector2.Zero;

                    // Get stuck in a fight if target is not null
                    if (m_target != null)
                    {
                        m_state = CharacterState.Fighting;
                        m_srcRect.Y = 82;
                    }

                    // If health is less than or equal to 0, kill it.
                    if (m_health <= 0)
                    {
                        m_state = CharacterState.Dead;
                        m_srcRect.Y = 122;
                    }
                    
                    // If there is no more path, go idle
                    if (m_isMoving)
                    {
                        m_state = CharacterState.Travelling;
                        m_srcRect.Y = 42;
                    }
                    break;
                case CharacterState.Travelling:
                    // Update character movement.
                    Movement();

                    // Move the character if applicable.
                    m_position += 50 * m_velocity * (float)gt.ElapsedGameTime.TotalSeconds;

                    UpdateRotation(gt);

                    // Get stuck in a fight if target is not null
                    if (m_target != null)
                    {
                        m_state = CharacterState.Fighting;
                        m_srcRect.Y = 82;
                    }

                    // If health is less than or equal to 0, kill it.
                    if (m_health <= 0)
                    {
                        m_state = CharacterState.Dead;
                        m_srcRect.Y = 122;
                    }

                    // If there is no more path, go idle
                    if (!m_isMoving)
                    {
                        m_state = CharacterState.Idle;
                        m_srcRect.Y = 2;
                    }
                    break;
                case CharacterState.Fighting:
                    // If health is less than or equal to 0, kill it.
                    if (m_health <= 0)
                    {
                        m_state = CharacterState.Dead;
                        m_srcRect.Y = 122;
                    }

                    if ((m_target != null && m_target.Health <= 0) || m_target == null)
                    {
                        m_target = null;
                        m_srcRect.Y = 42;
                        m_state = CharacterState.Travelling;
                    }
                    break;
                case CharacterState.Dead:
                    // If dead do nothing.
                    break;
            }
        }

        public override void DrawMe(SpriteBatch sb, GameTime gt)
        {
            //m_map.DrawMe(sb, gt);
            base.DrawMe(sb, gt);
        }

        public void DrawInfo(SpriteBatch sb, MouseState mouse, MouseState oldmouse)
        {
#if DEBUG
            sb.Begin();
            sb.DrawString(Game1.debugFont, "CharPos: " + m_position + "\nVelocity: " + m_velocity + "\nCharPos Grid: " + m_xCoords + " " + m_yCoords + "\nIs Selected: " + m_isSelected + "\nMouse: " + m_targetIndexX + " " + m_targetIndexY + "\nIs Moving: " + m_isMoving, Vector2.One, Color.Red);
            
            sb.End();
#endif
        }

        private void Movement()
        {
            // As long as path exists.
            if (m_path.Count > 0)
            {
                // If the circle contains the next tile's position then snap to the tile.
                if (m_snapCircle.Contains(m_path.Peek().Position))
                {
                    m_position = m_path.Peek().Position;
                    m_velocity = Vector2.Zero;
                    m_xCoords = m_path.Peek().GridX;
                    m_yCoords = m_path.Peek().GridY;

                    m_path.Dequeue();
                }
                else // Otherwise move towards the tile.
                {
                    m_velocity = m_path.Peek().Position - m_position;
                    m_velocity.Normalize();
                    m_velocity *= (m_speed / 2);
                }
            }
            else // If no path exists, then moving is false.
            {
                m_isMoving = false;
            }
        }

        private void GetTarget(InputManager input)
        {
            // If the character is selected and a walkable tile is clicked, then update the character's path and move character to location.
            if (input.LeftPressed && m_isSelected && (m_targetIndexX > 0 && m_targetIndexX < m_map.Tiles.GetLength(0) && m_targetIndexY > 0 && m_targetIndexY < m_map.Tiles.GetLength(1)) && m_map.Tiles[m_targetIndexX, m_targetIndexY].IsWalkable)
            {
                //graphics.ToggleFullScreen();
                m_search.Search(m_xCoords, m_yCoords, m_targetIndexX, m_targetIndexY);
                m_path = m_search.GetPath();
                m_isMoving = true;
            }
        }
    }

    // VIP guards
    class GuardChar : GameChar
    {
        // Is the guard fighting?
        private bool m_isFighting;

        // Is the guard shifting to new, unoccupied position.
        private bool m_isShifting;

        // The node graph for navigation.
        private Map m_map;
        private NodeGraph m_graph;
        private GBFsearch m_search;

        private ImportantChar m_vip;
        private Point m_vipCoords, m_oldVipCoords;

        // Class Constructor.
        public GuardChar(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, int maxHealth, float speed, float detectionRadius, float damage, float tickTime, UniMapData data, ContentManager content, Map map, Vector2 healthBarPosition) 
            : base(txr, position, tint, 1, fps, framesX, framesY, maxHealth, speed, detectionRadius, damage, tickTime)
        {
            m_isSelected = false;
            m_isMoving = false;
            m_isFighting = false;
            m_isShifting = false;
            m_isShaded = true;
            m_velocity = Vector2.Zero;
            m_map = map;
            m_graph = new NodeGraph(data);
            m_search = new GBFsearch(m_graph, m_map);
            m_xCoords = (int)(position.X / 36);
            m_yCoords = (int)(position.Y / 36);

            m_path = new Queue<MapTile>();

            m_healthBar = new CharHealthBar("Art\\GameArt\\GameplayGUI")
                .SetPosition(healthBarPosition)
                .LoadShader(content, "Shaders\\SmallHighlightEffect")
                .LoadTextures(content, "\\AllyHealthBarBorder", "\\AllyHealthBar", "\\AllyHealthBar", 1)
                .SetHpValues((int)m_health)
                .SetColours(Color.White, Color.Yellow, Color.Red)
                .SetParentOfBar(this);
        }

        public void UpdateMe(ImportantChar vip, GameTime gt, List<EnemyChar> enemies)
        {
            m_vip = vip;
            m_vipCoords = new Point(m_vip.CoordsX, m_vip.CoordsY);

            LockHP();
            ClampOverHeal();
            m_healthBar.UpdateBar();

            // Update the rectangle position, mouse position and mouse target coordinates.
            #region Update Coordinates
            // Move the rectangle with the character.
            m_rect.X = (int)(m_position.X - m_origin.X);
            m_rect.Y = (int)(m_position.Y - m_origin.Y);
            m_snapCircle.Centre = m_position;
            m_viewCircle.Centre = m_position;
            m_outerDetectionCircle.Centre = m_position;
            m_innerDetectionCircle.Centre = m_position;
            m_fightingCircle.Centre = m_position;
            m_selectionCircle.Centre = m_position;
            #endregion

            if (!m_map.Tiles[m_xCoords, m_yCoords].IsOccupied)
            {
                m_map.Tiles[m_xCoords, m_yCoords].IsOccupied = true;
                m_map.Tiles[m_xCoords, m_yCoords].OccupiedChar = this;
            }

            Selection();

            UpdateStates(gt, enemies);

            m_oldVipCoords = m_vipCoords;
        }

        private void UpdateStates(GameTime gt, List<EnemyChar> enemies)
        {
            switch (m_state)
            {
                case CharacterState.Idle:

                    FightInitiation(enemies);

                    if (m_target != null)
                    {
                        m_state = CharacterState.Fighting;
                        m_srcRect.Y = 82;
                    }

                    m_velocity = Vector2.Zero;

                    if (m_map.Tiles[m_xCoords, m_yCoords].IsOccupied && m_map.Tiles[m_xCoords, m_yCoords].OccupiedChar != this)
                    {
                        GetEmptyTile(m_map.Tiles[m_xCoords, m_yCoords].OccupiedChar.OuterDetectionCircle);
                    }

                    // If health is less than or equal to 0, kill it.
                    if (m_health <= 0)
                    {
                        m_state = CharacterState.Dead;
                        m_srcRect.Y = 122;
                    }

                    if (!m_innerDetectionCircle.Contains(m_vip.Position))
                    {
                        // Update selection and navigation logic.
                        GetTarget();
                        m_isMoving = true;
                    }

                    if (m_vip.Target != null)
                    {
                        m_target = m_vip.Target;
                        m_state = CharacterState.Fighting;
                        m_srcRect.Y = 82;
                    }

                    if (m_isMoving)
                    {
                        m_state = CharacterState.Travelling;
                        m_srcRect.Y = 42;
                    }
                    break;
                case CharacterState.Travelling:

                    if (m_path.Count > 0)
                    {
                        if (m_path.Peek().IsOccupied)
                        {
                            m_srcRect.Y = 2;
                        }
                    }

                    FightInitiation(enemies);

                    // Update selection and navigation logic.
                    GetTarget();

                    // Update character movement.
                    Movement();

                    if (!m_map.Tiles[m_xCoords, m_yCoords].IsOccupied)
                    {
                        m_isMoving = false;
                        m_isShifting = false;
                    }

                    // Move the character if applicable.
                    m_position += 50 * m_velocity * (float)gt.ElapsedGameTime.TotalSeconds;

                    UpdateRotation(gt);

                    // If health is less than or equal to 0, kill it.
                    if (m_health <= 0)
                    {
                        m_state = CharacterState.Dead;
                        m_srcRect.Y = 122;
                    }

                    if (m_vip.Target != null)
                    {
                        m_target = m_vip.Target;
                        m_state = CharacterState.Fighting;
                        m_srcRect.Y = 82;
                    }

                    if (!m_isMoving)
                    {
                        m_state = CharacterState.Idle;
                        m_srcRect.Y = 2;
                    }
                    break;
                case CharacterState.Fighting:

                    MeleeUpdate(gt);
                    break;
                case CharacterState.Dead:
                    // If dead do nothing.
                    break;
            }
        }

        #region Fighting Mechanics
        // Update fighting mechanics
        private void MeleeUpdate(GameTime gt)
        {
            // If target is not null and it's time to do damage
            // Do damage to target
            if (m_currTick <= 0 && m_target != null)
            {
                m_target.Health -= m_damage;
                m_currTick = m_damageTick;
            }
            else
            {
                m_currTick -= (float)gt.ElapsedGameTime.TotalSeconds;
            }

            // If target is dead, make target null
            if (m_target != null && m_target.Health <= 0)
            {
                m_target = null;
                m_state = CharacterState.Idle;
            }

            // If target is null or target is out of range then make target null and follow VIP
            if (m_target == null || (m_target != null && !m_target.FightingCircle.Contains(m_target.Position)))
            {
                m_target = null;
                GetTarget();
                m_state = CharacterState.Idle;
            }

            // If this is dead, then switch state
            if (m_health <= 0)
            {
                m_state = CharacterState.Dead;
            }
        }
        // Update initiation of fighting
        private void FightInitiation(List<EnemyChar> enemies)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                // If enemy is in range then fight enemy
                if (m_fightingCircle.Contains(enemies[i].Position) && enemies[i].Health > 0)
                {
                    m_target = enemies[i];
                    m_target.Target = this;
                    m_velocity = Vector2.Zero;
                    m_state = CharacterState.Fighting;
                    break;
                }
            }

            if (m_target != null)
            {
                m_state = CharacterState.Fighting;
            }
        }
        #endregion

        public override void DrawMe(SpriteBatch sb, GameTime gt)
        {
            if (m_path.Count > 0)
            {
                if (m_path.Peek().IsOccupied)
                {
                    m_srcRect.Y = 2;
                }
            }

            base.DrawMe(sb, gt);
        }

        public void DrawInfo(SpriteBatch sb, MouseState mouse, MouseState oldmouse)
        {
#if DEBUG
            sb.Begin();
            sb.DrawString(Game1.debugFont, "CharPos: " + m_position + "\nVelocity: " + m_velocity + "\nCharPos Grid: " + m_xCoords + " " + m_yCoords + "\nIs Selected: " + m_isSelected + "\nIs Moving: " + m_isMoving, Vector2.One, Color.Red);
            
            sb.End();
#endif
        }

        private void Movement()
        {
            // As long as path exists.
            if (m_path.Count > 0)
            {
                // If the circle contains the next tile's position then snap to the tile.
                if (m_snapCircle.Contains(m_path.Peek().Position))
                {
                    m_position = m_path.Peek().Position;
                    m_velocity = Vector2.Zero;
                    m_xCoords = m_path.Peek().GridX;
                    m_yCoords = m_path.Peek().GridY;

                    m_path.Dequeue();
                }
                else if (!m_snapCircle.Contains(m_path.Peek().Position) && !m_path.Peek().IsOccupied)// Otherwise move towards the tile.
                {
                    m_path.Peek().OccupiedChar = this;
                    m_velocity = m_path.Peek().Position - m_position;
                    m_velocity.Normalize();
                    m_velocity *= (m_speed / 2);
                }
                else
                {
                    m_isMoving = false;
                    m_isShifting = false;
                    m_srcRect.Y = 2;
                    //m_search.Search(m_xCoords, m_yCoords, m_vipCoords.X, m_vipCoords.Y);
                    //m_path = m_search.GetPath();
                }
            }
            else // If no path exists, then moving is false.
            {
                m_isMoving = false;
                m_isShifting = false;
                //m_search.Search(m_xCoords, m_yCoords, m_vipCoords.X, m_vipCoords.Y);
                //m_path = m_search.GetPath();
            }
        }

        private void GetTarget()
        {
            // If the character is selected and a walkable tile is clicked, then update the character's path and move character to location.
            if (m_vipCoords != m_oldVipCoords && !m_isShifting)
            {
                m_search.Search(m_xCoords, m_yCoords, m_vipCoords.X, m_vipCoords.Y);
                m_path = m_search.GetPath();
                m_isMoving = true;
            }
        }

        // Find nearby empty tile
        private void GetEmptyTile(Circle circle)
        {
            m_isShifting = true;

            List<MapTile> possibleTiles = new List<MapTile>();
            MapTile randomTile;

            // Check tiles around the VIP
            for (int y = -1; y < 2; y++)
            {
                for (int x = -1; x < 2; x++)
                {
                    int checkX = m_vip.CoordsX + x;
                    int checkY = m_vip.CoordsY + y;

                    if (checkX >= 0 && checkX < m_map.Tiles.GetLength(0) && checkY >= 0 && checkY < m_map.Tiles.GetLength(1))
                    {
                        if (m_map.Tiles[checkX, checkY].IsWalkable && !m_map.Tiles[checkX, checkY].IsOccupied)
                        {
                            if (circle.Contains(m_map.Tiles[checkX, checkY].Position))
                            {
                                possibleTiles.Add(m_map.Tiles[checkX, checkY]);
                            }
                        }
                    }
                }
            }

            // If tiles are available, select a random tile and move to it
            if (possibleTiles.Count > 0)
            {
                randomTile = possibleTiles[Game1.RNG.Next(0, possibleTiles.Count)];

                m_search.Search(m_xCoords, m_yCoords, randomTile.GridX, randomTile.GridY);
                m_path = m_search.GetPath();
                m_isMoving = true;
            }
        }
    }

    // Any Enemy Character
    class EnemyChar : GameChar
    {
        // Circle for use in projectile collisions.
        protected Circle m_collisionCircle;

        // What type is the enemy
        public enum EnemyType
        {
            Normal, Scout, Tank
        }
        protected EnemyType m_enemyType;

        // What type of combat is the enemy ine (only 1 type of enemy can utilize ranged)
        protected enum CombatType
        {
            Melee, Ranged
        }
        protected CombatType m_combatType;

        // Map and a breadth first section to go with it.
        protected Map m_map;
        protected NodeGraph m_graph;
        protected GBFsearch m_search;

        // Values used for the source rectangle position
        protected int m_srcBaseY;
        protected int m_srcLocalY;
        protected int m_baseFPS;

        // Vip and variables for storing it's coordinates.
        protected ImportantChar m_vip;
        protected Point m_vipCoords, m_oldVipCoords;

        // Timer to make the character disappear.
        protected float m_vanishTimer;

        protected int m_xpReward;
        protected int m_cashReward;

        protected Tower m_towerTarget;

        public Circle CollisionCircle { get { return m_collisionCircle; } set { m_collisionCircle = value; } }

        public EnemyType Etype { get { return m_enemyType; } set { m_enemyType = value; } }

        public int ExperienceReward { get { return m_xpReward; } }
        public int CashReward { get { return m_cashReward; } }

        public EnemyChar(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, int maxHealth, float speed, float detectionRadius, float damage, float tickTime, Map map, UniMapData data, ContentManager content, int currentLevel, int currentWave) 
            : base(txr, position, tint, scale, fps, framesX, framesY, maxHealth, speed, detectionRadius, damage, tickTime)
        {
            m_collisionCircle = new Circle(m_position.X, m_position.Y, 18);

            m_map = map;
            m_graph = new NodeGraph(data);
            m_search = new GBFsearch(m_graph, m_map);

            m_xCoords = (int)(position.X / 36);
            m_yCoords = (int)(position.Y / 36);

            m_vanishTimer = 2;

            m_baseFPS = m_fps;

            m_isShaded = false;

            // Truthfully, this was testing the idea of method chaining
            m_healthBar = new CharHealthBar("Art\\GameArt\\GameplayGUI")
                .SetPosition(m_position)
                .LoadShader(content, "Shaders\\HighlightEffect")
                .LoadTextures(content, "\\EnemyHealthBarBorder", "\\EnemyHealthBar", "\\EnemyHealthBar", 0.5f)
                .SetHpValues((int)m_health)
                .SetColours(Color.White, Color.Yellow, Color.Red)
                .SetParentOfBar(this)
                .SetScaleBorder(0.5f)
                .SetScaleBar(1)
                .SetScaleBacking(1);
        }

        public virtual void UpdateMe(ImportantChar vip, GameTime gt, ContentManager content, List<EnemyChar> enemyList, List<MessagePopup> popups, PlayerStats userData, List<Tower> towers, List<GameChar> friendlies, List<BaseProjectile> projectiles)
        {
            m_vip = vip;
            m_vipCoords = new Point(m_vip.CoordsX, m_vip.CoordsY);

            LockHP();
            m_healthBar.UpdateBar(m_position);

            // Update the rectangle position, mouse position and mouse target coordinates.
            #region Update Coordinates
            // Move the rectangle with the character.
            m_rect.X = (int)(m_position.X - m_origin.X);
            m_rect.Y = (int)(m_position.Y - m_origin.Y);
            m_snapCircle.Centre = m_position;
            m_viewCircle.Centre = m_position;
            m_outerDetectionCircle.Centre = m_position;
            m_innerDetectionCircle.Centre = m_position;
            m_fightingCircle.Centre = m_position;
            m_collisionCircle.Centre = m_position;
            #endregion

            UpdateStates(gt, content, enemyList, popups, userData, towers, friendlies, projectiles);

            m_oldVipCoords = m_vipCoords;
        }

        private void UpdateStates(GameTime gt, ContentManager content, List<EnemyChar> enemyList, List<MessagePopup> popups, PlayerStats userData, List<Tower> towers, List<GameChar> friendlies, List<BaseProjectile> projectiles)
        {
            switch (m_state)
            {
                case CharacterState.Idle:

                    m_srcLocalY = 2;
                    m_fps = m_baseFPS;

                    Selection();
                    GetTarget();

                    // If any friendly is in range initiate combat
                    MeleeInitiation(friendlies);

                    // If any tower is in range initiate ranged combat
                    if (m_enemyType == EnemyType.Scout)
                        RangeInitiation(towers);

                    m_velocity = Vector2.Zero;

                    // If health is less than or equal to 0, kill it.
                    if (m_health <= 0)
                    {
                        // If enemy is dead, show cash and xp rewards
                        AddRewardPopup(content, popups, userData);

                        UpdateAchievementsOnDeath(userData);

                        // MAKE IT DEAD! DEAD!
                        m_state = CharacterState.Dead;
                        break;
                    }

                    // checks if there is a path to follow
                    if (m_isMoving)
                    {
                        m_state = CharacterState.Travelling;
                        break;
                    }
                    break;
                case CharacterState.Travelling:

                    m_srcLocalY = 42;
                    m_fps = 3 * m_baseFPS;

                    Selection();
                    GetTarget();

                    // If any friendly is in range, initiate combat
                    MeleeInitiation(friendlies);

                    // If any tower is in range, initiate ranged combat
                    if (m_enemyType == EnemyType.Scout)
                        RangeInitiation(towers);

                    Movement();

                    // Move the character if applicable.
                    m_position += 50 * m_velocity * (float)gt.ElapsedGameTime.TotalSeconds;

                    UpdateRotation(gt);

                    // If health is less than or equal to 0, kill it.
                    if (m_health <= 0)
                    {
                        // If enemt is dead, show cash and xp rewards
                        AddRewardPopup(content, popups, userData);

                        UpdateAchievementsOnDeath(userData);

                        // MAKE IT DEAD! DEAD!
                        m_state = CharacterState.Dead;
                        break;
                    }

                    // When finished moving, be idle
                    if (!m_isMoving)
                    {
                        m_state = CharacterState.Idle;
                        break;
                    }

                    break;
                case CharacterState.Fighting:
                    MeleeInitiation(friendlies);

                    // Update appropriate combat mechanics
                    switch (m_enemyType)
                    {
                        case EnemyType.Normal:
                            m_srcLocalY = 82;
                            m_fps = (int)(1.5f * m_baseFPS);

                            MeleeUpdate(gt, content, popups, userData);
                            break;
                        case EnemyType.Scout:
                            switch (m_combatType)
                            {
                                case CombatType.Melee:
                                    m_srcLocalY = 82;

                                    MeleeUpdate(gt, content, popups, userData);
                                    break;
                                case CombatType.Ranged:
                                    m_srcLocalY = 2;

                                    RangeUpdate(gt, content, projectiles, popups, userData);
                                    break;
                            }
                            break;
                        case EnemyType.Tank:
                            m_srcLocalY = 82;
                            m_fps = m_baseFPS;

                            MeleeUpdate(gt, content, popups, userData);
                            break;
                    }

                    // If dead, add reward
                    if (m_health <= 0)
                    {
                        // Show cash and xp reward if dead
                        AddRewardPopup(content, popups, userData);

                        UpdateAchievementsOnDeath(userData);

                        // MAKE IT DEAD! DEAD!
                        m_state = CharacterState.Dead;
                    }
                    break;
                case CharacterState.Dead:

                    m_srcLocalY = 122;
                    m_fps = m_baseFPS;

                    // Make enemy vanish
                    Vanish(gt, content, enemyList, popups, userData);
                    break;
            }
        }

        private void UpdateAchievementsOnDeath(PlayerStats userData)
        {
            // Add to achievements
            switch (m_enemyType)
            {
                case EnemyType.Normal:
                    userData.AchievementCounts[1]++;
                    break;
                case EnemyType.Scout:
                    userData.AchievementCounts[2]++;
                    break;
                case EnemyType.Tank:
                    userData.AchievementCounts[3]++;
                    break;
            }
        }

        // Add cash and xp reward
        private void AddRewardPopup(ContentManager content, List<MessagePopup> popups, PlayerStats userData)
        {
            popups.Add(new MessagePopup(content.Load<Texture2D>("Art\\PlaceholderArt\\MenuGUI\\MessageBacking"), m_position, Color.White, new Vector2(0, -0.5f), "$" + m_cashReward, 1, Game1.gameFontNorm));
            popups.Add(new MessagePopup(content.Load<Texture2D>("Art\\PlaceholderArt\\MenuGUI\\MessageBacking"), m_position + new Vector2(0, 50), Color.White, new Vector2(0, -0.5f), m_xpReward + " XP", 1, Game1.gameFontNorm));
            userData.Cash += m_cashReward;
            userData.PlayerXP += m_xpReward;
        }

        // Update firing ranged projectiles
        private void RangeUpdate(GameTime gt, ContentManager content, List<BaseProjectile> projectiles, List<MessagePopup> popups, PlayerStats userData)
        {
            if (m_currTick <= 0)
            {
                Vector2 vel = m_towerTarget.TowerPos - m_position;
                vel.Normalize();
                vel *= 5;
                float rotation = (float)Math.Atan2(vel.Y, vel.X) + 1.5707f;
                EnemyProjectile proj = new EnemyProjectile(content.Load<Texture2D>("Art\\GameArt\\Projectiles\\SpitballSheet"), m_position, Color.White, vel, 1, 8, 4, 1, 100, 5, m_viewCircle.Radius, 0, null);
                proj.Rotation = rotation;
                projectiles.Add(proj);
                m_currTick = m_damageTick * 2;
            }
            else
            {
                m_currTick -= (float)gt.ElapsedGameTime.TotalSeconds;
            }

            // If target tower is dead, make target null
            if (m_towerTarget != null && m_towerTarget.Health <= 0)
            {
                m_towerTarget = null;
                m_state = CharacterState.Idle;
            }
        }

        // Initiate ranged combat
        private void RangeInitiation(List<Tower> towers)
        {
            for (int i = 0; i < towers.Count; i++)
            {
                if (m_viewCircle.Contains(towers[i].TowerPos))
                {
                    m_towerTarget = towers[i];
                    m_velocity = Vector2.Zero;
                    m_state = CharacterState.Fighting;
                    m_combatType = CombatType.Ranged;
                    break;
                }
            }
        }

        // Initiate melee combate
        private void MeleeInitiation(List<GameChar> friendlies)
        {
            for (int i = 0; i < friendlies.Count; i++)
            {
                if (m_fightingCircle.Contains(friendlies[i].Position) && friendlies[i].Health > 0)
                {
                    m_target = friendlies[i];
                    friendlies[i].Target = this;
                    m_velocity = Vector2.Zero;
                    m_state = CharacterState.Fighting;
                    m_combatType = CombatType.Melee;
                    break;
                }
            }

            if (m_target != null)
            {
                m_state = CharacterState.Fighting;
            }
        }

        // Update fighting mechanics
        private void MeleeUpdate(GameTime gt, ContentManager content, List<MessagePopup> popups, PlayerStats userData)
        {
            // Attack target if target is not null
            if (m_currTick <= 0 && m_target != null)
            {
                m_target.Health -= m_damage;
                m_currTick = m_damageTick;
            }
            else
            {
                m_currTick -= (float)gt.ElapsedGameTime.TotalSeconds;
            }

            // If target is dead, make target null
            if (m_target != null && m_target.Health <= 0)
            {
                m_target = null;
                GetTarget();
                m_state = CharacterState.Idle;
            }

            // If target is out of range or null, then go idle
            if (m_target == null || (m_target != null && !m_fightingCircle.Contains(m_target.Position)))
            {
                m_target = null;
                GetTarget();
                m_state = CharacterState.Idle;
            }
        }

        public override void DrawMe(SpriteBatch sb, GameTime gt)
        {
            m_srcRect.Y = m_srcBaseY + m_srcLocalY;

            switch (m_state)
            {
                case CharacterState.Idle:
                case CharacterState.Travelling:
                case CharacterState.Fighting:
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
                    break;
                case CharacterState.Dead:
                    // If the animation time is less than or equal to 0, increment animation cell to the next cell.
                    if (m_animTime <= 0 && m_srcRect.X < m_txr.Width - m_srcRect.Width - 4)
                    {
                        m_srcRect.X += m_srcRect.Width + 4;

                        // Reset timer.
                        m_animTime = 1;
                    }
                    else // Take away time from the counter in relation to the fps given.
                        m_animTime -= (float)gt.ElapsedGameTime.TotalSeconds * m_fps;
                    break;
            }
            
            sb.Draw(m_txr, Position, m_srcRect, m_tint, m_rot, m_origin, m_scale, SpriteEffects.None, m_layer);

#if DEBUG
            sb.DrawString(Game1.debugFont, "Position: " + m_position + "\nVelocity: " + m_velocity + "\nCollision Circle: " + m_collisionCircle.Centre + " " + m_collisionCircle.Radius + "\nHealth: " + m_health + "\nCurrRot: " + m_rot + "\nTarget Rot: " + m_targetRot, m_position, Color.Red);
#endif
        }

        public void DrawInfo(SpriteBatch sb, Vector2 infoPos)
        {
            sb.Begin();
            sb.DrawString(Game1.debugFont, "Is Moving: " + m_isMoving + " Path Length: " + m_path.Count + " Velocity: " + m_velocity + "Health: " + m_health, infoPos, Color.Red);
            sb.End();
        }

        // Make enemy disappear on death
        protected virtual void Vanish(GameTime gt, ContentManager content, List<EnemyChar> enemyList, List<MessagePopup> popups, PlayerStats userData)
        {
            m_vanishTimer -= (float)gt.ElapsedGameTime.TotalSeconds;

            if (m_vanishTimer <= 0)
            {
                enemyList.Remove(this);
            }
        }

        protected virtual void GetTarget()
        {
            // If the character is selected and a walkable tile is clicked, then update the character's path and move character to location.
            if (m_vipCoords != m_oldVipCoords)
            {
                //graphics.ToggleFullScreen();
                m_search.Search(m_xCoords, m_yCoords, m_vipCoords.X, m_vipCoords.Y);
                m_path = m_search.GetPath();
                m_isMoving = true;

                while (m_path == null || m_path.Count <= 0)
                {
                    List<MapTile> walkables = new List<MapTile>();

                    for (int y = 0; y < m_map.Tiles.GetLength(1); y++)
                    {
                        if (m_map.Tiles[m_xCoords, y].IsWalkable)
                        {
                            walkables.Add(m_map.Tiles[m_xCoords, y]);
                        }
                    }

                    MapTile newTile = walkables[Game1.RNG.Next(0, walkables.Count)];

                    m_position = newTile.Position;
                    m_yCoords = newTile.GridY;

                    m_search.Search(m_xCoords, m_yCoords, m_vipCoords.X, m_vipCoords.Y);
                    m_path = m_search.GetPath();
                    m_isMoving = true;
                }
            }
        }

        protected virtual void Movement()
        {
            // As long as path exists.
            if (m_path.Count > 0)
            {
                // If the circle contains the next tile's position then snap to the tile.
                if (m_snapCircle.Contains(m_path.Peek().Position))
                {
                    m_position = m_path.Peek().Position;
                    m_velocity = Vector2.Zero;
                    m_xCoords = m_path.Peek().GridX;
                    m_yCoords = m_path.Peek().GridY;

                    m_path.Dequeue();

                    //if (m_path.Count > 0 && m_path.Peek().IsOccupied && m_path.Peek().OccupiedChar.Path.Count <= 0)
                    //{
                    //    GetEmptyTile(m_vip.OuterDetectionCircle);
                    //}
                }
                else // Otherwise move towards the tile.
                {
                    m_velocity = m_path.Peek().Position - m_position;
                    m_velocity.Normalize();
                    m_velocity *= (m_speed / 2);
                }
            }
            else // If no path exists, then moving is false.
            {
                m_isMoving = false;
            }
        }

        // Set rewards with respect to LOTS of variables (current level, current wave, base stats etc...)
        protected void SetRewards(int currentLevel, int currentWave, int baseXp, int baseCash, int levelMultXP, int levelMultCash, int waveMultXP, int waveMultCash)
        {
            m_xpReward = baseXp + (levelMultXP * currentLevel) + (waveMultXP * currentWave);
            m_cashReward = baseCash + (levelMultCash * currentLevel) + (waveMultCash * currentWave);
        }
    }
}
