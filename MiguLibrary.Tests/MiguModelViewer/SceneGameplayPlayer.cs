using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiguModelViewer
{
    // Rhythm map player
    public class SceneGameplayPlayer
    {
        private bool mIsPlaying;

        private Stopwatch mWatch;

        public SceneGameplayPlayer()
        {
            mIsPlaying = false;
            mWatch = new Stopwatch();
        }

        public void Update()
        {

        }

        public void Render()
        {

        }

        public void SwitchState()
        {
            if(mIsPlaying)
            {
                mWatch.Stop();
            }
            else
            {
                mWatch.Start();
            }

            mIsPlaying = mIsPlaying == false;
        }
    }
}
