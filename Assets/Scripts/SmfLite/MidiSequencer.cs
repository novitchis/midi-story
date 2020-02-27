using System.Collections.Generic;
using System.Linq;

namespace SmfLite
{
    public class MidiSequencer
    {
        private List<MidiTrackSequencer> trackSequencers = null;

        public bool Playing {
            get { return trackSequencers.Any(sequencer => sequencer.Playing); }
        }

        public MidiSequencer(List<MidiTrack> tracks, int ppqn, float bpm)
        {
            trackSequencers = tracks.Select(track => new MidiTrackSequencer(track, ppqn, bpm)).ToList();
        }

        public List<MidiEvent> Start ()
        {
            return trackSequencers.SelectMany(item => item.Start()).ToList();
        }

        public List<MidiEvent> Advance (float deltaTime)
        {
            if (!Playing) {
                return null;
            }

            return trackSequencers.SelectMany(item => item.Advance(deltaTime)).ToList();
        }
    }
}