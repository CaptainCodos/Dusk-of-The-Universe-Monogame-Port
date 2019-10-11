using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System.IO;

namespace DuskOfTheUniverse
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // RNG and Fonts
        public static readonly Random RNG = new Random();
        public static SpriteFont debugFont;
        public static SpriteFont gameFontNorm;
        public static SpriteFont gameFontMassive;
        public static SpriteFont cutsceneFont;

        // Screen size values to be stored for use in code.
        private int screenSizeX, screenSizeY;

        // Screenshot bool
        private bool takeShot;

        #region Player Data

        // Used for saving all the player data
        private StoredDataContainer dataContainer;

        // Lists of all player data
        private List<PlayerStats> playerStats;

        // Strings used for login
        private string tmpUser;
        private string tmpPass;

        // Holds the index of the currently logged in player
        private int currentUserIndex;

        // Used in login logic
        private int typingIndex;
        #endregion

        // Check if the intro is playing
        private bool introPlaying;

        // Utilities
        private MouseState mouse, oldmouse;
        private KeyboardState keys, oldkeys;
        private Camera cam;

        // List of Ingame Sound Effects
        public static List<SoundEffect> sfxList;

        // Backgrounds and titles
        private StaticGraphic levelSelectBackground;
        private StaticGraphic backgroundTest;
        private CenteredStaticGraphic title;

        // Character lists
        private List<GameChar> friendlies;
        private ImportantChar vip;
        private List<EnemyChar> enemies;

        #region Tower Related Lists

        // List of projectiles
        private List<BaseProjectile> projectiles;

        // List of towers on the map
        private List<Tower> towers;

        // List of tower menus
        private List<RadialMenu> towerMenus;

        // List of saved towers (Not saved to file)
        private List<Tower> savedTowers;

        // List of tower constructs
        private List<TowerConstruct> towerSchematics;
        #endregion

        // List of popup messages
        private List<MessagePopup> popUpList;

        #region Game Managers

        // Selection manager handles friendly character selection
        private SelectionManager selectionManager;

        // Enemy spawn manager handles where the enemies spawn and when they spawn
        private EnemySpawnManager enemySpawnManager;

        // Placement manager handles the moving and initial placement of towers
        private PlacementManager placementManager;

        // Tower manager handles the use of tower menus and their effects on their respective towers
        private TowerManager towerManager;

        // Handles actor updates
        private ActorUpdateHandler actorUpdater;
        #endregion

        #region Utility Managers

        // Handles general input, such as clicking and stores keyboards states
        private InputManager inputManager;

        // Music manager handles the playing of music in game and the shuffling of tracks
        private MusicManager musicManager;

        // Typing manager handes general typing input
        private TypingManager typingManager;
        #endregion

        #region Map Classes

        // This class is a path generator that is fundamental to creating maps
        private PathGen mapGen;
        private UniMapData mapData;

        // This is the map used during gameplay
        private Map map;

        // This is the mini map used during gameplay
        private MiniMap miniMap;

        // This class generates the cut corners of tiles to give a rounded appearance
        private TileMerger merger;

        // This is a completion zone for the vip to pass into to complete the level
        private CompletionZone completeZone;

        // This variable stores the current level
        private int currentLevel;
        #endregion

        // Setup gamestates and menus
        private enum GameState
        {
            Login, MainMenu, Settings, Tutorial, Achievements, LevelSelect, MapGenerator, GamePlay, Pause, ConstructionMenu, CutScene, Credits
        }
        private GameState currGameState;

        // Most of this is self explanatory
        #region Menus
        private NormalMenu mainMenu;
        private TutorialMenu tutorialMenu;
        private AchievementsMenu achievementsMenu;
        private SettingsMenu setMenu;
        private FreeFormMenu levelsMenu;

        private NormalMenu mapGenMenu;
        private NormalMenu lineAlterButtons;
        // Length used to generate a line of walkable tiles
        private int lineGenLength;

        private NormalMenu pauseMenu;

        private GameplayMenu gameMenu;

        private ConstructionMenu constructMenu;
        #endregion

        private Credits credits;
        private Cutscene cutScene;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.IsFullScreen = true;
            IsMouseVisible = true;

            IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = false;

            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        }

        protected override void Initialize()
        {
            currGameState = GameState.Login;

            enemies = new List<EnemyChar>();
            friendlies = new List<GameChar>();
            towers = new List<Tower>();
            towerMenus = new List<RadialMenu>();
            savedTowers = new List<Tower>();
            towerSchematics = new List<TowerConstruct>();

            popUpList = new List<MessagePopup>();

            // Load up the file containing all the player data and turn the data into a list of different players
            dataContainer = FileManager.LoadUsers("Users.txt");

            if (dataContainer == null)
                playerStats = new List<PlayerStats>();
            else
                playerStats = dataContainer.ToList();

            // Set the temp user and password strings to a figurative null
            typingIndex = 0;
            tmpUser = "";
            tmpPass = "";

            // Set screen size
            screenSizeX = graphics.PreferredBackBufferWidth;
            screenSizeY = graphics.PreferredBackBufferHeight;

            lineGenLength = 8;

            introPlaying = false;
            takeShot = false;

            sfxList = new List<SoundEffect>();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Fonts
            debugFont = Content.Load<SpriteFont>("Fonts\\DebugFontArial8");
            gameFontNorm = Content.Load<SpriteFont>("Fonts\\GameFontQuartzMS19");
            gameFontMassive = Content.Load<SpriteFont>("Fonts\\GameFontQuartzMS96");
            cutsceneFont = Content.Load<SpriteFont>("Fonts\\CutsceneFont19");

            // Sound Effects
            sfxList.Add(Content.Load<SoundEffect>("Sound\\SFX\\NormalShoot"));
            sfxList.Add(Content.Load<SoundEffect>("Sound\\SFX\\LaserShoot"));
            sfxList.Add(Content.Load<SoundEffect>("Sound\\SFX\\PowerUp"));
            sfxList.Add(Content.Load<SoundEffect>("Sound\\SFX\\Explosion"));

            // Create the camera
            cam = new Camera(GraphicsDevice.Viewport);

            #region Managers
            inputManager = new InputManager();
            musicManager = new MusicManager(Content);
            typingManager = new TypingManager(Content);
            actorUpdater = new ActorUpdateHandler(ref friendlies, ref enemies, ref towers);
            #endregion

            backgroundTest = new StaticGraphic(Content.Load<Texture2D>("Art\\BlueUniverse"), Vector2.Zero, Color.White);
            levelSelectBackground = new StaticGraphic(Content.Load<Texture2D>("Art\\GameArt\\MenuGUI\\LevelMenu\\LevelSelectBackground"), Vector2.Zero, Color.White);
            title = new CenteredStaticGraphic(Content.Load<Texture2D>("Art\\GameArt\\MenuGUI\\MainMenu\\Title1"), new Vector2(960, 600), Color.White, 3.5f);

            #region Menus
            mainMenu = new NormalMenu(Content, "Art\\GameArt\\MenuGUI\\MainMenu\\MenuButton", false, false, 5, 20, new Vector2(960, 650));
            tutorialMenu = new TutorialMenu(Content);
            setMenu = new SettingsMenu(Content);
            achievementsMenu = new AchievementsMenu(Content);

            levelsMenu = new FreeFormMenu(Content, "Art\\GameArt\\MenuGUI\\LevelMenu\\LevelButton", false, new Vector2[] { new Vector2(1100, 860), new Vector2(1460, 660), new Vector2(360, 360), new Vector2(820, 160), new Vector2(180, 1000) });

            mapGenMenu = new NormalMenu(Content, "Art\\GameArt\\MenuGUI\\MapGenMenu\\GenButton", true, false, 3, 400, new Vector2(180, 500));
            lineAlterButtons = new NormalMenu(Content, "Art\\GameArt\\MenuGUI\\MapGenMenu\\LineButton", true, false, 2, 460, new Vector2(480, 400));

            pauseMenu = new NormalMenu(Content, "Art\\GameArt\\MenuGUI\\PauseMenu\\PauseButton", true, false, 3, 20, new Vector2(800, 540));

            gameMenu = new GameplayMenu(Content);
            constructMenu = new ConstructionMenu(Content);
            #endregion

            completeZone = new CompletionZone();

            towerManager = new TowerManager();

            #region Load Credits
            List<Vector2> offsetList = new List<Vector2>();
            List<string> messages = new List<string>();

            #region Credit Positions
            offsetList.Add(new Vector2(-935, -540));

            offsetList.Add(new Vector2(-935, -400));
            offsetList.Add(new Vector2(-935, -350));

            offsetList.Add(new Vector2(-935, -250));
            offsetList.Add(new Vector2(-935, -200));

            offsetList.Add(new Vector2(-935, -100));
            offsetList.Add(new Vector2(-935, -50));

            offsetList.Add(new Vector2(-935, 50));
            offsetList.Add(new Vector2(-935, 100));
            offsetList.Add(new Vector2(-935, 250));
            offsetList.Add(new Vector2(-935, 400));
            offsetList.Add(new Vector2(-935, 550));
            offsetList.Add(new Vector2(-935, 700));
            offsetList.Add(new Vector2(-935, 850));
            offsetList.Add(new Vector2(-935, 1000));
            #endregion

            #region Credit Messages
            messages.Add("Credits");

            messages.Add("Programmer");
            messages.Add("Stephan Aldhous");

            messages.Add("Artist");
            messages.Add("Stephan Aldhous");

            messages.Add("Music Web Source");
            messages.Add("www.incompetech.com/music/royalty-free/");

            messages.Add("Music Credits");
            messages.Add("Heart of Nowhere - Kevin MacLeod (incompetech.com)\nLicensed under Creative Commons: By Attribution 3.0 License\nhttp://creativecommons.org/licenses/by/3.0/");
            messages.Add("Crypto - Kevin MacLeod (incompetech.com)\nLicensed under Creative Commons: By Attribution 3.0 License\nhttp://creativecommons.org/licenses/by/3.0/");
            messages.Add("Immersed - Kevin MacLeod (incompetech.com)\nLicensed under Creative Commons: By Attribution 3.0 License\nhttp://creativecommons.org/licenses/by/3.0/");
            messages.Add("Interloper - Kevin MacLeod (incompetech.com)\nLicensed under Creative Commons: By Attribution 3.0 License\nhttp://creativecommons.org/licenses/by/3.0/");
            messages.Add("Stormfront - Kevin MacLeod (incompetech.com)\nLicensed under Creative Commons: By Attribution 3.0 License\nhttp://creativecommons.org/licenses/by/3.0/");
            messages.Add("The Descent - Kevin MacLeod (incompetech.com)\nLicensed under Creative Commons: By Attribution 3.0 License\nhttp://creativecommons.org/licenses/by/3.0/");
            messages.Add("Urban Gaunlet - Kevin MacLeod (incompetech.com)\nLicensed under Creative Commons: By Attribution 3.0 License\nhttp://creativecommons.org/licenses/by/3.0/");
            #endregion

            credits = new Credits(gameFontNorm, backgroundTest, offsetList, messages);
            #endregion
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            keys = Keyboard.GetState();
            mouse = Mouse.GetState();

            inputManager.HandleInput(mouse, keys);

            if (keys.IsKeyDown(Keys.LeftShift) && keys.IsKeyDown(Keys.Escape))
                this.Exit();

            for (int i = 0; i < popUpList.Count; i++)
            {
                popUpList[i].UpdateMe(gameTime, popUpList);
            }

            //Switch statement for menu logic.
            switch (currGameState)
            {
                case GameState.Login:
                    if (keys.IsKeyDown(Keys.Escape) && oldkeys.IsKeyUp(Keys.Escape))
                        this.Exit();

                    LoginMenuMechanics();

                    break;
                case GameState.MainMenu:
                    mainMenu.UpdateMe(inputManager);

                    MainMenuNav();
                    break;
                case GameState.Tutorial:
                    tutorialMenu.UpdateMenu(inputManager);

                    if (tutorialMenu.TutorialEnded)
                        currGameState = GameState.MainMenu;
                    break;
                case GameState.Settings:
                    setMenu.UpdateMenu(inputManager, playerStats[currentUserIndex], musicManager);

                    SettingsNav();
                    break;
                case GameState.Achievements:
                    achievementsMenu.UpdateMenu(playerStats[currentUserIndex], inputManager);

                    AchievementsNav();
                    break;
                case GameState.LevelSelect:
                    levelsMenu.UpdateMe(inputManager, playerStats[currentUserIndex]);

                    LevelSelectNav();
                    break;
                case GameState.MapGenerator:
                    lineAlterButtons.UpdateMe(inputManager);
                    mapGenMenu.UpdateMe(inputManager);
                    cam.UpdateMe(mouse, keys, oldkeys, gameTime);

                    AlterLineLength();
                    MapGenNav();

                    if (miniMap != null && vip != null)
                    {
                        miniMap.UpdateMe(cam, towers, enemies, friendlies);
                    }
                    break;
                case GameState.GamePlay:
                    if (keys.IsKeyDown(Keys.Escape) && oldkeys.IsKeyUp(Keys.Escape))
                        currGameState = GameState.Pause;

                    playerStats[currentUserIndex].TotalGamePlayTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // Update Current Player's stats
                    playerStats[currentUserIndex].UpdateUserStats();

                    #region Update Level Completion

                    // Updates the completion zone with the vip's position
                    completeZone.UpdateZone(vip);

                    // If the level is complete
                    if (completeZone.Complete)
                    {
                        CompleteLevel();

                        break;
                    }
                    #endregion

                    #region UpdateMap and GUI
                    gameMenu.UpdateMe(inputManager, savedTowers, Content, gameTime, enemies, projectiles, playerStats[currentUserIndex], enemySpawnManager);
                    GameMenuNav();

                    cam.UpdateMe(mouse, keys, oldkeys, gameTime);

                    if (miniMap != null && vip != null)
                    {
                        miniMap.UpdateMe(cam, towers, enemies, friendlies);
                    }

                    map.UpdateMap(towers);
                    #endregion

                    #region Update Managers
                    enemySpawnManager.UpdateManager(enemies, gameTime, Content, "Art\\GameArt\\CharacterArt\\EnemyArt", playerStats[currentUserIndex]);
                    selectionManager.UpdateManager(friendlies, inputManager, cam, towerMenus);
                    placementManager.UpdateManager(inputManager, cam, towers, savedTowers, Content, gameMenu, gameTime, enemies, projectiles, towerMenus, playerStats[currentUserIndex]);
                    towerManager.UpdateManager(placementManager, towerMenus, towers, playerStats[currentUserIndex]);
                    #endregion

                    #region Update Map Towers and Tower Menus
                    for (int i = 0; i < towerMenus.Count; i++)
                    {
                        towerMenus[i].UpdateMenu(gameTime, inputManager, cam);

                        // Keeps the menu locked to it's tower and prevents crashes if the tower no longer exists
                        if (i >= towers.Count || towers[i] == null)
                        {
                            towerMenus.RemoveAt(i);
                        }
                        else
                        {
                            towerMenus[i].MenuCentre = towers[i].TowerPos;
                        }
                    }
                    #endregion

                    // Actor Updater is a manager that updates all friendlies, enemies and towers
                    actorUpdater.UpdateManager(gameTime, inputManager, Content, cam, playerStats[currentUserIndex], popUpList, vip, projectiles, ref friendlies, ref enemies, ref towers);

                    // Darken the enemies
                    DarkenActors();

                    #region Update Projectiles
                    for (int i = 0; i < projectiles.Count; i++)
                    {
                        // Update all projectiles according to their index
                        switch (projectiles[i].ProjIndex)
                        {
                            case 0:
                                NormalProjectile proj1 = (NormalProjectile)projectiles[i];
                                proj1.UpdateMe(gameTime, projectiles, enemies);
                                break;
                            case 1:
                                HeavyProjectile proj2 = (HeavyProjectile)projectiles[i];
                                proj2.UpdateMe(gameTime, projectiles, enemies);
                                break;
                            case 2:
                                Laser proj3 = (Laser)projectiles[i];
                                proj3.UpdateMe(gameTime, projectiles, enemies);
                                break;
                            case 3:
                                Missile proj4 = (Missile)projectiles[i];
                                proj4.UpdateMe(gameTime, projectiles, enemies, Content);
                                break;
                            case 4:
                                FlameProjectile proj5 = (FlameProjectile)projectiles[i];
                                proj5.UpdateMe(gameTime, projectiles, enemies);
                                break;
                            case 5:
                                SplashWave proj6 = (SplashWave)projectiles[i];
                                proj6.UpdateMe(gameTime, projectiles, enemies);
                                break;
                            case 6:
                                EnemyProjectile proj7 = (EnemyProjectile)projectiles[i];
                                proj7.UpdateMe(gameTime, towers, projectiles);
                                break;
                        }
                    }
                    #endregion

                    // Light up enemies or tiles in these zones
                    CreateVisibleZones();

                    // This method will check if the VIP is dead
                    // If so, then reset the player's level progress (both player level and game levels)
                    UpdateGameOver();

                    break;
                case GameState.Pause:
                    if (keys.IsKeyDown(Keys.Escape) && oldkeys.IsKeyUp(Keys.Escape))
                        currGameState = GameState.GamePlay;

                    // Updata pause menu
                    pauseMenu.UpdateMe(inputManager);

                    PauseMenuNav();
                    break;
                case GameState.ConstructionMenu:
                    // Update construction logic
                    constructMenu.UpdateMenu(inputManager, typingManager, popUpList, Content, cam, gameTime, towerSchematics, savedTowers, playerStats[currentUserIndex]);
                    if (!constructMenu.Controls.IsTyping)
                    {
                        cam.UpdateMe(inputManager.Mouse, inputManager.Keys, inputManager.Oldkeys, gameTime);
                    }
                    ConstructMenuNav(gameTime);
                    break;
                case GameState.CutScene:
                    // Update cutscene
                    cutScene.UpdateCutScene(gameTime);

                    if ((cutScene.CutsceneEnded && introPlaying) || (inputManager.LeftPressed && introPlaying))
                    {
                        currGameState = GameState.LevelSelect;
                    }
                    else if ((cutScene.CutsceneEnded && !introPlaying) || (inputManager.LeftPressed && !introPlaying))
                    {
                        currGameState = GameState.Credits;
                    }
                    break;
                case GameState.Credits:
                    credits.UpdateCredits(gameTime);

                    if (credits.CreditsEnded)
                    {
                        credits.CreditsEnded = false;
                        currGameState = GameState.MainMenu;
                    }
                    break;
            }

            // Update music manager
            musicManager.UpdateManager(gameTime);

            if (inputManager.Keys.IsKeyDown(Keys.P) && inputManager.Oldkeys.IsKeyUp(Keys.P))
            {
                takeShot = true;
            }

            // Finish handling input this update
            inputManager.FinishHandling();

            oldmouse = mouse;
            oldkeys = keys;

            base.Update(gameTime);
        }

        private void UpdateGameOver()
        {
            // Reset progress on current difficulty and reset the player's level.
            if (vip.Health <= 0)
            {
                popUpList.Add(new MessagePopup(Content.Load<Texture2D>("Art\\GameArt\\MenuGUI\\MessageBacking"), new Vector2(970, 600), Color.White, -Vector2.UnitY / 4, "You died!\n\nYour level has been reset\nAll level progress lost!", 3, cutsceneFont));

                mapGen = null;
                map = null;
                miniMap = null;
                vip = null;
                friendlies = new List<GameChar>();
                enemies = new List<EnemyChar>();
                projectiles = new List<BaseProjectile>();
                selectionManager = null;
                enemySpawnManager = null;
                placementManager = null;
                towers = new List<Tower>();
                towerMenus = new List<RadialMenu>();
                projectiles = new List<BaseProjectile>();
                savedTowers = new List<Tower>();
                towerSchematics = new List<TowerConstruct>();

                playerStats[currentUserIndex].Cash = 0;
                playerStats[currentUserIndex].OffensiveModulesUsed = 0;
                playerStats[currentUserIndex].UtilityModulesUsed = 0;
                playerStats[currentUserIndex].LevelsComplete = 0;
                playerStats[currentUserIndex].NumberOfLevelsCompleted = 0;
                playerStats[currentUserIndex].TotalGamePlayTime = 0;
                playerStats[currentUserIndex].PlayerXP = 0;
                playerStats[currentUserIndex].PlayerLVL = 1;
                playerStats[currentUserIndex].IntroPlayed = false;

                dataContainer = new StoredDataContainer(playerStats);
                FileManager.SaveUsers(dataContainer, "Users.txt");

                currGameState = GameState.MainMenu;
            }
        }

        private void CompleteLevel()
        {
            // If the level is the latest level then increment the number of levels completed
            if (playerStats[currentUserIndex].LevelsComplete < currentLevel)
            {
                playerStats[currentUserIndex].LevelsComplete++;
            }

            // This holds the total number of levels complete where as the similar varibale 
            // holds the player's progress through the game
            playerStats[currentUserIndex].NumberOfLevelsCompleted++;

            // Update the player's stats
            playerStats[currentUserIndex].UpdateUserStats();

            // Save player stats and backout to level select
            currGameState = GameState.LevelSelect;
            dataContainer = new StoredDataContainer(playerStats);
            FileManager.SaveUsers(dataContainer, "Users.txt");

            mapGen = null;
            map = null;
            miniMap = null;
            vip = null;
            friendlies = new List<GameChar>();
            enemies = new List<EnemyChar>();
            projectiles = new List<BaseProjectile>();
            selectionManager = null;
            enemySpawnManager = null;
            placementManager = null;
            towers = null;
            towerMenus = null;
            projectiles = null;

            // If the game is complete, move to the appropriate ending
            if (playerStats[currentUserIndex].LevelsComplete > 3 && currentLevel >= 4)
            {
                playerStats[currentUserIndex].LevelsComplete = 0;

                dataContainer = new StoredDataContainer(playerStats);
                FileManager.SaveUsers(dataContainer, "Users.txt");

                #region Get Ending
                // Adds cutscene narration and cutscene scenes (images)
                switch (playerStats[currentUserIndex].CurrentEnding)
                {
                    case 0:

                        List<string> messages1 = new List<string>();
                        messages1.Add("As you escape, you vow to unleash fury on all those that were unaffected by the crisis . . .");
                        messages1.Add("Believing them to be part of some wide plan to destroy humanity.");
                        messages1.Add("As time went on species upon species would be destroyed by humanitys crusade.");
                        messages1.Add("As more time passed, humans evolve tremendously, becoming god-like beings. \nHowever they were severed from the fabric of conventional time.");
                        messages1.Add("Upon the realization of the magnitude of their actions . . .");
                        messages1.Add("They sent out the remaining beings back in time to wipe themselves out. \nIn an effort to correct their grave mistake!");
                        messages1.Add("And with that the loop is complete!");

                        List<MotionGraphic> images1 = new List<MotionGraphic>();
                        images1.Add(new MotionGraphic(Content.Load<Texture2D>("Art\\GameArt\\CutsceneArt\\Ending1"), new Vector2(960, 300), new Color(255, 255, 255, 2), Vector2.UnitY / 3, 0, 1));
                        images1.Add(new MotionGraphic(Content.Load<Texture2D>("Art\\GameArt\\CutsceneArt\\Ending1 Part2"), new Vector2(960, 300), new Color(255, 255, 255, 2), Vector2.UnitY / 3, 0, 1));

                        cutScene = new Cutscene(Content, images1, messages1);

                        currGameState = GameState.CutScene;
                        break;
                    case 1:

                        List<string> messages2 = new List<string>();
                        messages2.Add("Escaping their doom, humanity went on a great journey into the stars.");
                        messages2.Add("Few civilizations made contact with them from then on, and those who did \ncould not accurately recall the event.");
                        messages2.Add("Through their enlightenment, humans figured out that \nit was in fact they, who sent those monsters to destroy the human race.");
                        messages2.Add("As time went, they evolved into god-like beings \nof incredible power, and became one with the cosmos.");
                        messages2.Add("Now cosmic entities they observed the goings on in the universe \nfrom afar, or rather from everywhere.");
                        messages2.Add("And with that an alternate reality of observers was created!");

                        List<MotionGraphic> images2 = new List<MotionGraphic>();
                        images2.Add(new MotionGraphic(Content.Load<Texture2D>("Art\\GameArt\\CutsceneArt\\Ending2"), new Vector2(960, 300), new Color(255, 255, 255, 2), Vector2.UnitY / 4, 0, 1));

                        cutScene = new Cutscene(Content, images2, messages2);

                        currGameState = GameState.CutScene;
                        break;
                    case 2:

                        List<string> messages3 = new List<string>();
                        messages3.Add("Escaping their doom, humanity decided that it was going to protect \nother races in the cosmos from events much like this.");
                        messages3.Add("They became great defenders, great guardians of all life. \nTaking up the mantle of responsibility of care and nurture.");
                        messages3.Add("And with that an alternate reality of defenders was created!");

                        List<MotionGraphic> images3 = new List<MotionGraphic>();
                        images3.Add(new MotionGraphic(Content.Load<Texture2D>("Art\\GameArt\\CutsceneArt\\Ending3"), new Vector2(960, 300), new Color(255, 255, 255, 2), Vector2.UnitY / 2, 0, 1));

                        cutScene = new Cutscene(Content, images3, messages3);

                        currGameState = GameState.CutScene;
                        break;
                }

                #endregion
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            switch (currGameState)
            {
                case GameState.Login:
                    #region Draw Login
                    spriteBatch.Begin();
                    backgroundTest.DrawMe(spriteBatch);

                    spriteBatch.DrawString(gameFontNorm, "Press Esc to exit", Vector2.One, Color.White);

                    spriteBatch.DrawString(gameFontMassive, "Username", new Vector2(960, 348), Color.White, 0, gameFontMassive.MeasureString("Username") / 2, 1, SpriteEffects.None, 0);
                    spriteBatch.DrawString(gameFontMassive, "Password", new Vector2(960, 642), Color.White, 0, gameFontMassive.MeasureString("Password") / 2, 1, SpriteEffects.None, 0);

                    switch (typingIndex)
                    {
                        case 0:
                            spriteBatch.DrawString(gameFontMassive, tmpUser, new Vector2(960, 446), Color.White, 0, gameFontMassive.MeasureString(tmpUser) / 2, 1, SpriteEffects.None, 0);
                            typingManager.DrawTyper(spriteBatch, tmpUser, gameFontMassive, new Vector2(960, 446), 96, true);
                            break;
                        case 1:
                            spriteBatch.DrawString(gameFontMassive, tmpUser, new Vector2(960, 446), Color.White, 0, gameFontMassive.MeasureString(tmpUser) / 2, 1, SpriteEffects.None, 0);
                            spriteBatch.DrawString(gameFontMassive, tmpPass, new Vector2(960, 740), Color.White, 0, gameFontMassive.MeasureString(tmpPass) / 2, 1, SpriteEffects.None, 0);
                            typingManager.DrawTyper(spriteBatch, tmpPass, gameFontMassive, new Vector2(960, 740), 96, true);
                            break;
                    }

                    for (int i = 0; i < popUpList.Count; i++)
                    {
                        popUpList[i].DrawMe(spriteBatch);
                    }

                    spriteBatch.End();
                    #endregion
                    break;
                case GameState.MainMenu:
                    spriteBatch.Begin();
                    backgroundTest.DrawMe(spriteBatch);
                    title.DrawMe(spriteBatch);
#if DEBUG
                    spriteBatch.DrawString(debugFont, "UserIndex: " + currentUserIndex + "\nUsername: " + playerStats[currentUserIndex].Username + "\nPassword " + playerStats[currentUserIndex].Password + "\nLevelsComplete: " + playerStats[currentUserIndex].LevelsComplete + "\nLvl: " + playerStats[currentUserIndex].PlayerLVL, Vector2.One, Color.White);
#endif

                    spriteBatch.End();

                    mainMenu.DrawMe(spriteBatch, gameTime);

                    spriteBatch.Begin();

                    for (int i = 0; i < popUpList.Count; i++)
                    {
                        popUpList[i].DrawMe(spriteBatch);
                    }

                    spriteBatch.End();
                    break;
                case GameState.Tutorial:
                    tutorialMenu.DrawMenu(spriteBatch);
                    break;
                case GameState.Settings:
                    spriteBatch.Begin();
                    backgroundTest.DrawMe(spriteBatch);

                    spriteBatch.End();

                    setMenu.DrawMenu(spriteBatch, gameTime, musicManager);
                    break;
                case GameState.Achievements:
                    spriteBatch.Begin();
                    backgroundTest.DrawMe(spriteBatch);

                    spriteBatch.End();

                    achievementsMenu.DrawMe(spriteBatch, gameTime);
                    break;
                case GameState.LevelSelect:
                    spriteBatch.Begin();
                    levelSelectBackground.DrawMe(spriteBatch);

                    spriteBatch.End();

                    levelsMenu.DrawMe(spriteBatch, gameTime);
                    break;
                case GameState.MapGenerator:
                    #region Draw MapGen
                    spriteBatch.Begin();
                    backgroundTest.DrawMe(spriteBatch);
                    spriteBatch.End();

                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, cam.Transform);

                    if (map != null)
                    {
                        map.DrawMe(spriteBatch, gameTime);
                    }

                    for (int i = 0; i < friendlies.Count; i++)
                    {
                        friendlies[i].DrawMe(spriteBatch, gameTime);
                    }

                    spriteBatch.End();

                    if (miniMap != null)
                    {
                        miniMap.DrawMe(spriteBatch);
                    }

                    spriteBatch.Begin();
                    for (int i = 0; i < popUpList.Count; i++)
                    {
                        popUpList[i].DrawMe(spriteBatch);
                    }
                    spriteBatch.End();

                    mapGenMenu.DrawMe(spriteBatch, gameTime);
                    lineAlterButtons.DrawMe(spriteBatch, gameTime);

                    spriteBatch.Begin();
                    spriteBatch.DrawString(gameFontNorm, "Path Line Length: " + lineGenLength, new Vector2(740, 400), Color.White, 0, gameFontNorm.MeasureString("Path Line Length: " + lineGenLength) / 2, 1, SpriteEffects.None, 0);
                    spriteBatch.End();
                    #endregion
                    break;
                case GameState.GamePlay:

                    // Draw Game Objects
                    #region Game Objects
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, cam.Transform);
                    map.DrawMe(spriteBatch, gameTime);

                    for (int i = 0; i < friendlies.Count; i++)
                    {
                        friendlies[i].DrawMe(spriteBatch, gameTime);

#if DEBUG
                        spriteBatch.DrawString(debugFont, "MaxHealth: " + friendlies[i].MaxHealth + "\nHealth: " + friendlies[i].Health, friendlies[i].Position, Color.White);
#endif
                    }

                    for (int i = 0; i < enemies.Count; i++)
                    {
                        enemies[i].DrawMe(spriteBatch, gameTime);
                    }

                    for (int i = 0; i < towers.Count; i++)
                    {
                        towers[i].DrawTower(spriteBatch, gameTime);
                    }

                    for (int i = 0; i < popUpList.Count; i++)
                    {
                        popUpList[i].DrawMe(spriteBatch);
                    }

                    for (int i = 0; i < projectiles.Count; i++)
                    {
                        if (projectiles[i] is NormalProjectile)
                        {
                            NormalProjectile proj = (NormalProjectile)projectiles[i];
                            proj.DrawMe(spriteBatch, gameTime);
                        }
                        else if (projectiles[i] is HeavyProjectile)
                        {
                            HeavyProjectile proj = (HeavyProjectile)projectiles[i];
                            proj.DrawMe(spriteBatch, gameTime);
                        }
                        else if (projectiles[i] is Laser)
                        {
                            Laser proj = (Laser)projectiles[i];
                            proj.DrawLaser(spriteBatch);
                        }
                        else if (projectiles[i] is Missile)
                        {
                            Missile proj = (Missile)projectiles[i];
                            proj.DrawMe(spriteBatch, gameTime);
                        }
                        else if (projectiles[i] is FlameProjectile)
                        {
                            FlameProjectile proj = (FlameProjectile)projectiles[i];
                            proj.DrawMe(spriteBatch, gameTime);
                        }
                        else if (projectiles[i] is SplashWave)
                        {
                            SplashWave proj = (SplashWave)projectiles[i];
                            proj.DrawMe(spriteBatch, gameTime);
                        }
                        else if (projectiles[i] is EnemyProjectile)
                        {
                            EnemyProjectile proj = (EnemyProjectile)projectiles[i];
                            proj.DrawMe(spriteBatch, gameTime);
                        }
                    }

                    placementManager.DrawMe(spriteBatch);

                    spriteBatch.End();
                    #endregion

                    #region Draw Menus and Healthbars
                    for (int i = 0; i < towerMenus.Count; i++)
                    {
                        towerMenus[i].DrawMenu(spriteBatch, gameTime, cam, inputManager);
                    }

                    for (int i = 0; i < enemies.Count; i++)
                    {
                        enemies[i].HealthBar.DrawMe(spriteBatch, cam);
                    }

                    for (int i = 0; i < towers.Count; i++)
                    {
                        towers[i].HealthBar.DrawMe(spriteBatch, cam);
                    }

                    gameMenu.DrawShaded(spriteBatch, gameTime, enemies, projectiles, Content, enemySpawnManager);

                    for (int i = 0; i < friendlies.Count; i++)
                    {
                        friendlies[i].HealthBar.DrawMe(spriteBatch);
                    }
                    #endregion

                    // Show the player, their current stats
                    spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
                    spriteBatch.DrawString(gameFontNorm, "Cash: " + playerStats[currentUserIndex].Cash, new Vector2(20, 875), Color.White);
                    spriteBatch.DrawString(gameFontNorm, "Lvl: " + playerStats[currentUserIndex].PlayerLVL + " (" + playerStats[currentUserIndex].PlayerXP + "/" + playerStats[currentUserIndex].XPrequired + ")", new Vector2(300, 875), Color.White);
                    spriteBatch.End();

                    miniMap.DrawMe(spriteBatch);

                    enemySpawnManager.DrawData(spriteBatch);

                    break;
                case GameState.Pause:

                    spriteBatch.Begin();
                    backgroundTest.DrawMe(spriteBatch);
                    spriteBatch.End();

                    pauseMenu.DrawMe(spriteBatch, gameTime);

                    break;
                case GameState.ConstructionMenu:

                    constructMenu.DrawMenu(spriteBatch, gameTime, cam, typingManager);

                    spriteBatch.Begin();
                    for (int i = 0; i < popUpList.Count; i++)
                    {
                        popUpList[i].DrawMe(spriteBatch);
                    }
                    spriteBatch.End();

                    break;
                case GameState.CutScene:

                    spriteBatch.Begin();
                    cutScene.DrawCutScene(spriteBatch);
                    spriteBatch.End();

                    break;
                case GameState.Credits:

                    credits.DrawCredits(spriteBatch);

                    break;
            }

            if (takeShot)
            {
                //Texture2D t = TakeScreenshot();
                SaveShot();
                //t.Dispose();
                takeShot = false;
            }

            base.Draw(gameTime);

            
        }

        #region Menu Navigation

        private void MainMenuNav()
        {
            if (mainMenu.Buttons[0].IsPressed)
            {
                if (playerStats[currentUserIndex].IntroPlayed)
                {
                    currGameState = GameState.LevelSelect;
                }
                else
                {
                    introPlaying = true;
                    playerStats[currentUserIndex].IntroPlayed = true;

                    List<string> messages = new List<string>();
                    messages.Add("In the void, somewhere in the universe, a rift opens . . .");
                    messages.Add("Pouring out of the rift, creatures never seen before the in the universe.");
                    messages.Add("They travelled to human populated planets, destroying the civilizations residing on them . . .");
                    messages.Add("Only to disappear as quickly as they had arrived.");
                    messages.Add("However, standing between them and human extinction . . .");
                    messages.Add("Is YOU, whoever you are!");
                    messages.Add("Your mission? Escape this hell and ensure humanitys survival!");

                    List<MotionGraphic> images = new List<MotionGraphic>();
                    images.Add(new MotionGraphic(Content.Load<Texture2D>("Art\\GameArt\\CutsceneArt\\Intro1"), new Vector2(960, 300), new Color(255, 255, 255, 2), Vector2.UnitY / 3, 0, 1));
                    images.Add(new MotionGraphic(Content.Load<Texture2D>("Art\\GameArt\\CutsceneArt\\Intro2"), new Vector2(960, 300), new Color(255, 255, 255, 2), Vector2.UnitY / 3, 0, 1));

                    cutScene = new Cutscene(Content, images, messages);

                    currGameState = GameState.CutScene;
                }
            }
            if (mainMenu.Buttons[1].IsPressed)
            {
                currGameState = GameState.Tutorial;
                tutorialMenu.CurrentPage = 0;
            }
            if (mainMenu.Buttons[2].IsPressed)
            {
                currGameState = GameState.Settings;
                setMenu.FromGamePlay = false;
            }
            if (mainMenu.Buttons[3].IsPressed)
            {
                currGameState = GameState.Achievements;
            }
            if (mainMenu.Buttons[4].IsPressed)
            {
                popUpList.Add(new MessagePopup(Content.Load<Texture2D>("Art\\PlaceholderArt\\MenuGUI\\MessageBacking"), new Vector2(960, 540), Color.White, new Vector2(0, -0.3f), "Logged Out!", 0.3f, Game1.gameFontNorm));
                typingIndex = 0;
                tmpUser = "";
                tmpPass = "";
                currGameState = GameState.Login;
            }
        }

        private void SettingsNav()
        {
            if (setMenu.Menu.Buttons[0].IsPressed) // Shuffles the game music
            {
                musicManager.ShuffleMusic();
            }
            if (setMenu.Menu.Buttons[1].IsPressed) // Returns to pause or main menu depending on where the player came frome
            {
                if (setMenu.FromGamePlay)
                    currGameState = GameState.Pause;
                else
                    currGameState = GameState.MainMenu;
            }
        }

        private void AchievementsNav()
        {
            if (achievementsMenu.BackButton.IsPressed)
            {
                currGameState = GameState.MainMenu;
            }
        }

        private void LevelSelectNav()
        {
            // Set levels to locked and unlocked as appropriate
            for (int i = 0; i < levelsMenu.Buttons.Count; i++)
            {
                if (i < 4 && i > playerStats[currentUserIndex].LevelsComplete)
                    levelsMenu.Buttons[i].IsClickable = false;
                else
                {
                    levelsMenu.Buttons[i].IsClickable = true;
                }
            }

            if (levelsMenu.Buttons[0].IsPressed)
            {
                currGameState = GameState.MapGenerator;
                currentLevel = 1;
            }
            if (levelsMenu.Buttons[1].IsPressed)
            {
                currGameState = GameState.MapGenerator;
                currentLevel = 2;
            }
            if (levelsMenu.Buttons[2].IsPressed)
            {
                currGameState = GameState.MapGenerator;
                currentLevel = 3;
            }
            if (levelsMenu.Buttons[3].IsPressed)
            {
                currGameState = GameState.MapGenerator;
                currentLevel = 4;
            }
            if (levelsMenu.Buttons[4].IsPressed)
                currGameState = GameState.MainMenu;
        }

        // Alter the lengths of paths generated on the map
        private void AlterLineLength()
        {
            if (lineAlterButtons.Buttons[0].IsPressed)
                lineGenLength--;
            else if (lineAlterButtons.Buttons[1].IsPressed)
                lineGenLength++;

            if (lineGenLength < 3)
                lineGenLength = 3;
            else if (lineGenLength > 30)
                lineGenLength = 30;
        }
        private void MapGenNav()
        {
            // Resetting lists and class prevents crashes
            if (mapGenMenu.Buttons[0].IsPressed)
            {
                mapGen = null;
                map = null;
                vip = null;
                friendlies = new List<GameChar>();
                enemies = new List<EnemyChar>();
                towerMenus = new List<RadialMenu>();
                towers = new List<Tower>();
                miniMap = null;
                friendlies = new List<GameChar>();
                towers = new List<Tower>();
                towerMenus = new List<RadialMenu>();
                projectiles = new List<BaseProjectile>();
                selectionManager = null;
                enemySpawnManager = null;
                placementManager = null;
                currGameState = GameState.LevelSelect;
            }
            if (mapGenMenu.Buttons[1].IsPressed) // Generate new map
            {
                mapGen = new PathGen(80, 45, lineGenLength);
                map = new Map(mapGen.Data, Content, "Art\\GameArt\\WorldArt\\MapTileSheet", GraphicsDevice);
                map.LevelNumber = currentLevel;
                playerStats[currentUserIndex].Cash = 1500 * currentLevel;
                miniMap = new MiniMap(Content, mapGen.Data, cam);
                friendlies = new List<GameChar>();
                enemies = new List<EnemyChar>();
                towers = new List<Tower>();
                towerMenus = new List<RadialMenu>();
                projectiles = new List<BaseProjectile>();
                map.UpdateMap(towers);
                merger = new TileMerger(map, mapGen.Data);
                merger.UpdateMerger(GraphicsDevice);
                selectionManager = new SelectionManager(mapGen.Data);
                enemySpawnManager = new EnemySpawnManager(mapGen.Data, map);
                placementManager = new PlacementManager(mapGen.Data, Content, GraphicsDevice);
                PlacePlayerActors();
            }
            if (mapGenMenu.Buttons[2].IsPressed && map != null) // If the player likes the map, they can proceed to gameplay
                currGameState = GameState.GamePlay;
        }

        private void GameMenuNav()
        {
            if (gameMenu.Menu.Buttons[0].IsPressed) // Takes the player to construction to make towers
            {
                currGameState = GameState.ConstructionMenu;
                cam.InConstruction = true;
            }
        }

        private void ConstructMenuNav(GameTime gt)
        {
            // Prevents player from zooming when scrolling through open menu
            if (constructMenu.OpenMenu.ScrollRect.Contains(inputManager.Mouse.X, inputManager.Mouse.Y) && constructMenu.OpenMenu.HasDropped)
            {
                cam.ScrollAvailable = false;
            }
            else
            {
                cam.ScrollAvailable = true;

                // Prvents player from zooming when scrolling through any of the parts menus
                for (int i = 0; i < constructMenu.PartMenus.Count; i++)
                {
                    if (constructMenu.PartMenus[i].ScrollRect.Contains(inputManager.Mouse.X, inputManager.Mouse.Y) && constructMenu.PartMenus[i].HasDropped)
                    {
                        cam.ScrollAvailable = false;
                        break;
                    }
                    else
                    {
                        cam.ScrollAvailable = true;
                    }
                }
            }

            // Takes the player back to the game
            if (constructMenu.BackButton.IsPressed)
            {
                currGameState = GameState.GamePlay;
                cam.InConstruction = false;
                cam.Zoom = 1;
            }
        }

        private void PauseMenuNav()
        {
            if (pauseMenu.Buttons[0].IsPressed)
                currGameState = GameState.GamePlay;
            if (pauseMenu.Buttons[1].IsPressed)
            {
                currGameState = GameState.Settings;
                setMenu.FromGamePlay = true;
            }
            if (pauseMenu.Buttons[2].IsPressed) // If the player quits the level, then reset level
            {
                mapGen = null;
                map = null;
                miniMap = null;
                vip = null;
                friendlies = new List<GameChar>();
                enemies = new List<EnemyChar>();
                towerMenus = new List<RadialMenu>();
                towers = new List<Tower>();
                friendlies = new List<GameChar>();
                towers = new List<Tower>();
                towerMenus = new List<RadialMenu>();
                projectiles = new List<BaseProjectile>();
                selectionManager = null;
                enemySpawnManager = null;
                placementManager = null;
                currGameState = GameState.LevelSelect;
            }
        }
        #endregion

        // This method will place the player actors on the game map.
        private void PlacePlayerActors()
        {
            // This array will hold the indexes of tiles that are walkable.
            // From this list a rando index will be chosen to place the actors.
            List<int> walkableIndexes = new List<int>();

            List<MapTile> visitedTiles = new List<MapTile>();

            for (int i = 0; i < map.Tiles.GetLength(1); i++)
            {
                if (map.Tiles[0, i].IsWalkable)
                {
                    walkableIndexes.Add(i);
                }
            }

            int selectedIndex = walkableIndexes[RNG.Next(0, walkableIndexes.Count)];
            MapTile currTile = map.Tiles[0, selectedIndex];
            int xCoord = 0;
            int yCoord = selectedIndex;

            // Place the friendlies
            friendlies.Add(new ImportantChar(Content.Load<Texture2D>("Art\\GameArt\\CharacterArt\\AllyArt\\vipSheet"), map.Tiles[xCoord + 1, yCoord].Position, Color.White, 1, 8, 4, 4, 1000, 3, 2, mapGen.Data, Content, map));
            vip = (ImportantChar)friendlies[0];
            friendlies.Add(new GuardChar(Content.Load<Texture2D>("Art\\GameArt\\CharacterArt\\AllyArt\\GuardSheet"), map.Tiles[xCoord, yCoord].Position, Color.White, 1, 8, 4, 4, 250, 4, 2, 15, 0.5f, mapGen.Data, Content, map, new Vector2(1300, 1000)));
            friendlies.Add(new GuardChar(Content.Load<Texture2D>("Art\\GameArt\\CharacterArt\\AllyArt\\GuardSheet"), map.Tiles[xCoord + 2, yCoord].Position, Color.White, 1, 8, 4, 4, 250, 4, 2, 15, 0.5f, mapGen.Data, Content, map, new Vector2(1300, 1040)));
        }

        // This method will handle the login mechanics
        private void LoginMenuMechanics()
        {
            // 0 - Typing in username
            // 1 - Typing in password
            // 2 - Validation
            switch (typingIndex)
            {
                case 0:
                    tmpUser = typingManager.UpdateManager(inputManager, tmpUser, Content, popUpList);

                    if (typingManager.EnterPressed && tmpUser.Length > 0)
                        typingIndex++;
                    break;
                case 1:
                    tmpPass = typingManager.UpdateManager(inputManager, tmpPass, Content, popUpList);

                    if (typingManager.EnterPressed && tmpPass.Length > 0)
                        typingIndex++;

                    if (typingManager.BackPressed && typingManager.TextIsNothing)
                        typingIndex--;
                    break;
                case 2:
                    // If player stats already exists in some capacity
                    if (playerStats.Count > 0)
                    {
                        // Iterate through the stats list
                        for (int i = 0; i < playerStats.Count; i++)
                        {
                            // Check if the user is exactly as typed, if so then log them in
                            if ((playerStats[i].Username == tmpUser && playerStats[i].Password == tmpPass))
                            {
                                currentUserIndex = i;
                                currGameState = GameState.MainMenu;
                                popUpList.Add(new MessagePopup(Content.Load<Texture2D>("Art\\PlaceholderArt\\MenuGUI\\MessageBacking"), new Vector2(960, 540), Color.White, new Vector2(0, -0.3f), "Logged In!", 0.3f, gameFontNorm));
                                break;
                            }
                            else if ((playerStats[i].Username == tmpUser && playerStats[i].Password != tmpPass)) // Check if the password typed in is wrong
                            {
                                tmpPass = "";
                                typingIndex = 1;
                                popUpList.Add(new MessagePopup(Content.Load<Texture2D>("Art\\PlaceholderArt\\MenuGUI\\MessageBacking"), new Vector2(960, 540), Color.White, new Vector2(0, -0.3f), "Wrong Password!", 0.3f, gameFontNorm));
                                break;
                            }
                            else if ((i < playerStats.Count - 1 && playerStats[i].Username != tmpUser && playerStats[i].Password != tmpPass)) // Check if the user is entirely different
                            {
                                continue;
                            }
                            else if ((i >= playerStats.Count - 1 && playerStats[i].Username != tmpUser && playerStats[i].Password != tmpPass)) // If the end of the list is reached and no user with that username can be found then make a new account
                            {
                                PlayerStats newUser = new PlayerStats();
                                newUser.Username = tmpUser;
                                newUser.Password = tmpPass;
                                newUser.LevelsComplete = 0;
                                newUser.PlayerXP = 0;
                                newUser.PlayerLVL = 1;

                                currentUserIndex = i + 1;

                                playerStats.Add(newUser);

                                dataContainer = new StoredDataContainer(playerStats);
                                FileManager.SaveUsers(dataContainer, "Users.txt");

                                currGameState = GameState.MainMenu;
                                popUpList.Add(new MessagePopup(Content.Load<Texture2D>("Art\\PlaceholderArt\\MenuGUI\\MessageBacking"), new Vector2(960, 540), Color.White, new Vector2(0, -0.3f), "Account Created!", 0.3f, gameFontNorm));
                                break;
                            }
                        }
                    }
                    else // Add the new user
                    {
                        PlayerStats newUser = new PlayerStats();
                        newUser.Username = tmpUser;
                        newUser.Password = tmpPass;
                        newUser.LevelsComplete = 0;
                        newUser.PlayerXP = 0;
                        newUser.PlayerLVL = 1;

                        currentUserIndex = 0;

                        playerStats.Add(newUser);

                        dataContainer = new StoredDataContainer(playerStats);
                        FileManager.SaveUsers(dataContainer, "Users.txt");

                        currGameState = GameState.MainMenu;
                        popUpList.Add(new MessagePopup(Content.Load<Texture2D>("Art\\PlaceholderArt\\MenuGUI\\MessageBacking"), new Vector2(960, 540), Color.White, new Vector2(0, -0.3f), "Account Created!", 0.3f, gameFontNorm));
                    }
                    break;
            }
        }

        // Darkens all enemies
        private void DarkenActors()
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].Tint = new Color(80, 80, 80);
                enemies[i].IsShaded = true;
            }
        }

        // Checks if tiles and enemies are within range of towers or friendlies
        private void CreateVisibleZones()
        {
            // For every map tile, check if it is within range of a tower or within small distance of a friendly
            // If so then light it up
            foreach (MapTile m in map.Tiles)
            {
                for (int i = 0; i < towers.Count; i++)
                {
                    for (int k = 0; k < towers[i].TowerParts.Count; k++)
                    {
                        switch (towers[i].TowerParts[k].TypeIndex)
                        {
                            case 1:
                                BaseRotor rotor = (BaseRotor)towers[i].TowerParts[k];

                                // Get distance ratio from the rotor to the outer range of the tower
                                Vector2 distVec = rotor.Position - m.Position;
                                float distMult = distVec.Length() / rotor.RangeCircle.Radius;
                                distMult = 1 - distMult;

                                if (distMult > 1)
                                    distMult = 1;

                                if (rotor.RangeCircle.Contains(m.Position))
                                {
                                    // Allow for nice view range blending
                                    // Taking the mult to a power allows for adjustment for view range falloff
                                    if ((Math.Pow(distMult, 0.5) * 255) > m.RedCompontent)
                                    {
                                        m.Tint = Color.White;
                                        m.RedCompontent = (byte)(Math.Pow(distMult, 0.5) * 255);
                                        m.GreenCompontent = (byte)(Math.Pow(distMult, 0.5) * 255);
                                        m.BlueCompontent = (byte)(Math.Pow(distMult, 0.5) * 255);
                                    }

                                    if (m.RedCompontent < 60)
                                    {
                                        m.RedCompontent = 60;
                                        m.GreenCompontent = 60;
                                        m.BlueCompontent = 60;
                                    }
                                }
                                break;
                        }

                        if (towers[i].TowerParts[k] is BaseRotor)
                            break;
                    }
                }

                for (int i = 0; i < friendlies.Count; i++)
                {
                    if (friendlies[i].OuterDetectionCircle.Contains(m.Position))
                    {
                        // Get distance ratio from the friendly to the outer range of their view
                        Vector2 distVec = friendlies[i].Position - m.Position;
                        float distMult = distVec.Length() / friendlies[i].OuterDetectionCircle.Radius;
                        distMult = 1 - distMult;

                        if (distMult > 1)
                            distMult = 1;

                        // Allow for nice view range blending
                        // Taking the mult to a power allows for adjustment for view range falloff
                        if ((Math.Pow(distMult, 0.25) * 255) > m.RedCompontent)
                        {
                            m.Tint = Color.White;
                            m.RedCompontent = (byte)(Math.Pow(distMult, 0.25) * 255);
                            m.GreenCompontent = (byte)(Math.Pow(distMult, 0.25) * 255);
                            m.BlueCompontent = (byte)(Math.Pow(distMult, 0.25) * 255);
                        }

                        if (m.RedCompontent < 60)
                        {
                            m.RedCompontent = 60;
                            m.GreenCompontent = 60;
                            m.BlueCompontent = 60;
                        }
                    }
                }
            }

            // For every enemy, check if it is within range of a tower or within small distance of a friendly
            // If so then light it up
            foreach (EnemyChar e in enemies)
            {
                for (int i = 0; i < towers.Count; i++)
                {
                    for (int k = 0; k < towers[i].TowerParts.Count; k++)
                    {
                        switch (towers[i].TowerParts[k].TypeIndex)
                        {
                            case 1:
                                BaseRotor rotor = (BaseRotor)towers[i].TowerParts[k];

                                // Get distance ratio from the rotor to the outer range of the tower
                                Vector2 distVec = rotor.Position - e.Position;
                                float distMult = distVec.Length() / rotor.RangeCircle.Radius;
                                distMult = 1 - distMult;

                                if (rotor.RangeCircle.Contains(e.Position))
                                {
                                    // Allow for nice view range blending
                                    // Taking the mult to a power allows for adjustment for view range falloff
                                    if ((Math.Pow(distMult, 0.5) * 255) > e.RedCompontent)
                                    {
                                        e.Tint = Color.White;
                                        e.RedCompontent = (byte)(Math.Pow(distMult, 0.5) * 255);
                                        e.GreenCompontent = (byte)(Math.Pow(distMult, 0.5) * 255);
                                        e.BlueCompontent = (byte)(Math.Pow(distMult, 0.5) * 255);
                                    }

                                    if (e.RedCompontent < 60)
                                    {
                                        e.RedCompontent = 60;
                                        e.GreenCompontent = 60;
                                        e.BlueCompontent = 60;
                                    }

                                    e.IsShaded = false;
                                }
                                break;
                        }

                        if (towers[i].TowerParts[k] is BaseRotor)
                            break;
                    }
                }

                for (int i = 0; i < friendlies.Count; i++)
                {
                    if (friendlies[i].OuterDetectionCircle.Contains(e.Position))
                    {
                        // Get distance ratio from the friendly to the outer range of their view
                        Vector2 distVec = friendlies[i].Position - e.Position;
                        float distMult = distVec.Length() / friendlies[i].OuterDetectionCircle.Radius;
                        distMult = 1 - distMult;

                        if (distMult > 1)
                            distMult = 1;

                        // Allow for nice view range blending
                        // Taking the mult to a power allows for adjustment for view range falloff
                        if ((Math.Pow(distMult, 0.25) * 255) > e.RedCompontent)
                        {
                            e.Tint = Color.White;
                            e.RedCompontent = (byte)(Math.Pow(distMult, 0.25) * 255);
                            e.GreenCompontent = (byte)(Math.Pow(distMult, 0.25) * 255);
                            e.BlueCompontent = (byte)(Math.Pow(distMult, 0.25) * 255);
                        }

                        if (e.RedCompontent < 60)
                        {
                            e.RedCompontent = 60;
                            e.GreenCompontent = 60;
                            e.BlueCompontent = 60;
                        }

                        e.IsShaded = false;
                    }
                }
            }
        }

        private Texture2D TakeScreenshot()
        {
            int w, h;
            w = GraphicsDevice.PresentationParameters.BackBufferWidth;
            h = GraphicsDevice.PresentationParameters.BackBufferHeight;
            RenderTarget2D screenshot;
            screenshot = new RenderTarget2D(GraphicsDevice, w, h, false, SurfaceFormat.Vector4, DepthFormat.None);
            GraphicsDevice.SetRenderTarget(screenshot);
            GraphicsDevice.Present();
            GraphicsDevice.SetRenderTarget(null);
            return screenshot;
        }

        private void SaveShot()
        {
            int w = GraphicsDevice.PresentationParameters.BackBufferWidth;
            int h = GraphicsDevice.PresentationParameters.BackBufferHeight;

            //force a frame to be drawn (otherwise back buffer is empty) 
            //Draw(new GameTime());

            //pull the picture from the buffer 
            int[] backBuffer = new int[w * h];
            GraphicsDevice.GetBackBufferData(backBuffer);

            //copy into a texture 
            Texture2D texture = new Texture2D(GraphicsDevice, w, h, false, GraphicsDevice.PresentationParameters.BackBufferFormat);
            texture.SetData(backBuffer);
            string GetDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            DateTime n = DateTime.Now;
            string DT = "" + n.Second + n.Minute + n.Hour + n.Day + n.Month + n.Year;
            Stream stream = File.OpenWrite("ScreenShot" + DT + ".jpg");
            texture.SaveAsJpeg(stream, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
            stream.Dispose();
            texture.Dispose();
        }
    }
}