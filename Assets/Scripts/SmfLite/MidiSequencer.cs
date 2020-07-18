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

        /// <summary>
        /// Start sequencing and return a list of events for each track
        /// </summary>
        public List<List<IMidiEvent>> Start ()
        {
            return trackSequencers
                .Select(item => item.Start())
                .Where(item => item != null).ToList();
        }

        /// <summary>
        /// Advance the time of events and return a list of events for each track
        /// </summary>
        public List<List<IMidiEvent>> Advance (float deltaTime)
        {
            if (!Playing) {
                return null;
            }

            return trackSequencers
                .Select(item => item.Advance(deltaTime))
                .Where(item => item != null).ToList();
        }

        public void SetBPM(float bpm)
        {
            trackSequencers.ForEach(track => track.SetBPM(bpm));
        }
    }
}