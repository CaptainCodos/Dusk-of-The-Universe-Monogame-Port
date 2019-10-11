using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{
    class CompletionZone
    {
        // This rectangle IS the completion zone
        private Rectangle m_completionZone;

        // Has the player completed the level
        private bool m_complete;

        public bool Complete { get { return m_complete; } }

        public CompletionZone()
        {
            m_completionZone = new Rectangle(2820, 0, 300, 3000);
        }

        public void UpdateZone(ImportantChar vip)
        {
            // Checks if player is within the zone
            if (m_completionZone.Contains((int)vip.Position.X, (int)vip.Position.Y))
                m_complete = true;
            else
                m_complete = false;
        }
    }
}
