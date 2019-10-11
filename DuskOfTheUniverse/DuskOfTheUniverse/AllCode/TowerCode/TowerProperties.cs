using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{
    [Serializable]
    class TowerProperties
    {
        // Counts the number of modules of each booster type and then gets the final boost when combined
        #region Boost Variables
        private float m_finalRangeBoost;
        private int m_rangeBoostCount;

        private float m_finalDamageBoost;
        private int m_damageBoostCount;

        private float m_finalRateBoost;
        private int m_rateBoostCount;

        private float m_finalHealthBoost;
        private int m_healthBoostCount;
        #endregion

        // Tower health
        private int m_totalHealth;
        private int m_finalHealth;

        // List of parts to search
        private List<TowerMasterPart> m_towerParts;

        public List<TowerMasterPart> TowerParts { get { return m_towerParts; } set { m_towerParts = value; } }

        public float RangeBoostFactor { get { return m_finalRangeBoost; } }
        public float DamageBoostFactor { get { return m_finalDamageBoost; } }
        public float RateBoostFactor { get { return m_finalRateBoost; } }

        public TowerProperties(List<TowerMasterPart> towerParts)
        {
            m_towerParts = towerParts;

            m_totalHealth = 0;
        }

        public void UpdateProperties(ref float towerHealth, ref int towerCost)
        {
            m_totalHealth = 0;

            // Count boosters
            for (int i = 0; i < m_towerParts.Count; i++)
            {
                switch(m_towerParts[i].TypeIndex)
                {
                    case 4:
                        switch (m_towerParts[i].SubIndex)
                        {
                            case 0:
                                m_rangeBoostCount++;
                                break;
                            case 1:
                                m_damageBoostCount++;
                                break;
                            case 2:
                                m_rateBoostCount++;
                                break;
                            case 3:
                                m_healthBoostCount++;
                                break;
                        }
                        break;
                }

                m_totalHealth += m_towerParts[i].PartHealth;
                towerCost += m_towerParts[i].PartCost;
            }

            // Ensures a dimishing returns system from boosters
            // Gets the counts of each booster type, then calculates the final boost based on diminishing returns
            m_finalDamageBoost = 1 + (m_damageBoostCount / (1.5f + m_damageBoostCount));
            m_finalRangeBoost = 1 + (m_rangeBoostCount / (1.5f + m_rangeBoostCount));
            m_finalRateBoost = 1 + (m_rateBoostCount / (1.5f + m_rateBoostCount));
            m_finalHealthBoost = 1 + (m_healthBoostCount / (2 + m_healthBoostCount));

            // Boost health
            m_finalHealth = (int)(m_totalHealth * m_finalHealthBoost);
            towerHealth = m_finalHealth;
        }
    }
}
