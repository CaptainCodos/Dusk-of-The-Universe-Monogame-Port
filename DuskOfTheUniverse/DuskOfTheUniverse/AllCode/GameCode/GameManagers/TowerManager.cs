using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{
    class TowerManager
    {
        public TowerManager()
        {
        }

        public void UpdateManager(PlacementManager placingManager, List<RadialMenu> towerMenus, List<Tower> towers, PlayerStats userData)
        {
            for(int i = 0; i < towerMenus.Count; i++)
            {
                // If the menu is open
                if (towerMenus[i].IsDeployed)
                {
                    // If the move button is pressed and the player has over 100 in cash
                    if (towerMenus[i].Buttons[0].IsPressed && userData.Cash >= 100)
                    {
                        // Initiate tower moving
                        placingManager.MovingActive = true;
                        placingManager.MovingTower = towers[i];
                        placingManager.UprootTower(placingManager.MovingTower, placingManager.Tiles[placingManager.MovingTower.Coords.X, placingManager.MovingTower.Coords.Y]);
                        placingManager.MovingMenu = towerMenus[i];
                    }
                    else if (towerMenus[i].Buttons[1].IsPressed)
                    {
                        // If the sell button is pressed and the player has over 100 in cash
                        if (userData.Cash > 100)
                        {
                            // Remove the tower and it's menu, and give the player cash equal to (Tower Price x (tower current health : tower max health)) - 100
                            // This gives cash with respect to the tower cost and how much health it had left minus 100.
                            // This prevents exploiting the selling mechanics to move a tower without the 100 cost.
                            userData.Cash += (int)(towers[i].TowerCost * (towers[i].Health / towers[i].MaxHP) - 100);
                            towers.RemoveAt(i);
                            towerMenus.RemoveAt(i);
                        }
                    }
                }
            }
        }
    }
}
