using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Helm
{
    [AddComponentMenu("")]
    public class SyncAudioAndSequencers : MonoBehaviour
    {
        public HelmSequencer sequencer;
        public AudioSource loop;
        public float waitTime = 3.0f;
        public Text text;

        int lastSecond = 0;

        void Start()
        {
            double time = AudioSettings.dspTime + waitTime;
            sequencer.StartSequencerScheduled(time);
            loop.PlayScheduled(time);
        }

        void Update()
        {
            waitTime -= Time.deltaTime;
            int second = Mathf.CeilToInt(waitTime);

            if (second != lastSecond)
            {
                lastSecond = second;

                if (lastSecond < 1)
                {
                    Destroy(this);
                    text.text = "PLAY";
                }
                else if (text)
                    text.text = "" + lastSecond;
            }
        }
    }
}
