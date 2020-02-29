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
            return trackSequencers
                .Select(item => item.Start())
                .Where(item => item != null)
                .SelectMany(list => list).ToList();
        }

        public List<MidiEvent> Advance (float deltaTime)
        {
            if (!Playing) {
                return null;
            }

            return trackSequencers
                .Select(item => item.Advance(deltaTime))
                .Where(item => item != null)
                .SelectMany(list => list).ToList();
        }
    }
}