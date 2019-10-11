using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{
    class MusicManager
    {
        // List of soundtracks to be played
        private List<Song> m_musicList;
        // Index of current song
        private int m_currindex;
        // Player volume
        private float m_volume;
        // Volum multiplier for the settings menu
        private float m_volumeMult;

        public float VolumeMult { get { return m_volumeMult; } set { m_volumeMult = value; } }
        
        public MusicManager(ContentManager content)
        {
            // Add soundtracks
            m_musicList = new List<Song>();
            m_musicList.Add(content.Load<Song>("Sound\\Music\\Heart of Nowhere"));
            m_musicList.Add(content.Load<Song>("Sound\\Music\\The Descent"));
            m_musicList.Add(content.Load<Song>("Sound\\Music\\Urban Gauntlet"));
            m_musicList.Add(content.Load<Song>("Sound\\Music\\Stormfront"));
            m_musicList.Add(content.Load<Song>("Sound\\Music\\Immersed"));
            m_musicList.Add(content.Load<Song>("Sound\\Music\\Crypto"));
            m_musicList.Add(content.Load<Song>("Sound\\Music\\Interloper"));

            // Pick a random soundtrack
            m_currindex = Game1.RNG.Next(0, m_musicList.Count);
            m_volume = 0;
            m_volumeMult = 0.5f;

            MediaPlayer.Volume = ClampVolume(ref m_volume) * m_volumeMult;
            MediaPlayer.Play(m_musicList[m_currindex]);
        }

        public void UpdateManager(GameTime gt)
        {
            HandleMusic(gt);

            ClampMult(ref m_volumeMult);

            MediaPlayer.Volume = ClampVolume(ref m_volume) * m_volumeMult;
        }

        // Handle music playing mechanics
        private void HandleMusic(GameTime gt)
        {
            switch (MediaPlayer.State)
            {
                case MediaState.Playing:
                    FadeMusic(gt);
                    break;
                case MediaState.Stopped:
                    ShuffleMusic();
                    break;
            }
        }

        private void FadeMusic(GameTime gt)
        {
            // Fade the music in
            if (MediaPlayer.PlayPosition < TimeSpan.FromSeconds(5))
            {
                m_volume += 0.2f * (float)gt.ElapsedGameTime.TotalSeconds;
            }
                // Fade music out when stopping
            else if (MediaPlayer.PlayPosition > m_musicList[m_currindex].Duration - TimeSpan.FromSeconds(5) && MediaPlayer.State == MediaState.Playing)
            {
                m_volume -= 0.2f * (float)gt.ElapsedGameTime.TotalSeconds;
            }
        }

        // Clamp the volume between 1 and 0
        private float ClampVolume(ref float volume)
        {
            if (volume > 1)
                volume = 1;
            if (volume < 0)
                volume = 0;

            return volume;
        }

        // Clamp the multiplier between 1 and 0
        private float ClampMult(ref float mult)
        {
            if (mult > 1)
                mult = 1;
            if (mult < 0)
                mult = 0;

            return mult;
        }

        // Shuffle to new soundtrack that isn't the current one
        public void ShuffleMusic()
        {
            int played = m_currindex;
            m_volume = 0;

            while (m_currindex == played)
            {
                m_currindex = Game1.RNG.Next(0, m_musicList.Count);
            }

            MediaPlayer.Play(m_musicList[m_currindex]);
        }
    }
}
