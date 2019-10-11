using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{
    /// <summary>
    /// Handle updating of all actors
    /// </summary>
    class ActorUpdateHandler
    {
        // Manager Constructor
        public ActorUpdateHandler(ref List<GameChar> allies, ref List<EnemyChar> enemies, ref List<Tower> gameTowers)
        {
        }

        public void UpdateManager(GameTime gt, InputManager inputManager, ContentManager Content, Camera cam, PlayerStats userData, List<MessagePopup> popUpList, ImportantChar vip, List<BaseProjectile> projectiles, ref List<GameChar> allies, ref List<EnemyChar> enemies, ref List<Tower> gameTowers)
        {
            // Update Towers
            for (int i = 0; i < gameTowers.Count; i++)
            {
                gameTowers[i].UpdateTower(gt, enemies, allies, projectiles, Content, gameTowers);
            }

            // Update Allies
            for (int i = 0; i < allies.Count; i++)
            {
                if (allies[i] is ImportantChar)
                {
                    ImportantChar importantPerson = (ImportantChar)allies[i];
                    importantPerson.UpdateMe(inputManager, cam, gt);
                    vip = importantPerson;
                }
                else
                {
                    GuardChar guard = (GuardChar)allies[i];
                    guard.UpdateMe(vip, gt, enemies);
                }
            }

            // Update Enemies
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].UpdateMe(vip, gt, Content, enemies, popUpList, userData, gameTowers, allies, projectiles);
            }
        }
    }
}
