using System.Collections.Generic;
using DeltaEventPairList = System.Collections.Generic.List<SmfLite.MidiTrack.DeltaEventPair>;

namespace SmfLite
{
    public class MidiTrackSequencer
    {
        DeltaEventPairList.Enumerator enumerator;
        bool playing;
        int ppqn;
        float pulsePerSecond;
        float pulseToNext;
        float pulseCounter;

        public bool Playing {
            get { return playing; }
        }

        public MidiTrackSequencer (MidiTrack track, int ppqn, float bpm)
        {
            this.ppqn = ppqn;
            SetBPM(bpm);
            enumerator = track.GetEnumerator ();
        }

        public List<IMidiEvent> Start ()
        {
            if (enumerator.MoveNext ()) {
                pulseToNext = enumerator.Current.delta;
                playing = true;
                return Advance (0);
            } else {
                playing = false;
                return null;
            }
        }

        public List<IMidiEvent> Advance (float deltaTime)
        {
            if (!playing) {
                return null;
            }

            pulseCounter += pulsePerSecond * deltaTime;

            if (pulseCounter < pulseToNext) {
                return null;
            }

            var messages = new List<IMidiEvent> ();

            while (pulseCounter >= pulseToNext) {
                var pair = enumerator.Current;
                messages.Add (pair.midiEvent);
                if (!enumerator.MoveNext ()) {
                    playing = false;
                    break;
                }

                pulseCounter -= pulseToNext;
                pulseToNext = enumerator.Current.delta;
            }

            return messages;
        }

        public void SetBPM(float bpm) {
            pulsePerSecond = bpm / 60.0f * ppqn;
        }
    }
}