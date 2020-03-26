using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{

    public static class NoteUtils
    {
        private static string[] notesNames = new string[] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        private static int[] octaveBlackKeysIndexes = new int[] { 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0 };
        private static int[] octaveBlackKeys = new int[] { 0, 0, 1, 1, 2, 2, 2, 3, 3, 4, 4, 5 };

        public static bool IsBlackKey(byte note) {
            return octaveBlackKeysIndexes[note % 12] == 1;
        }

        /// <summary>
        /// Calculates the x position of the keyboard key for a note.
        /// Note: this has a slight offset to the right
        /// </summary>
        /// <param name="note">Midi note value</param>
        /// <param name="keyboardWidth">Total width of the keyboard</param>
        /// <param name="alignLeft">In case instead of middle point the left point is desired</param>
        /// <returns>The x value of the middle of the key</returns>
        public static float GetKeyX(byte note, float keyboardWidth, bool toLeft = false)
        {
            int precedingBlackKeys = (note / 12) * 5 + octaveBlackKeys[note % 12];

            // 12 is the white keys count not visible on the piano on bottom
            int precedingWhiteKeys = note - precedingBlackKeys - 12;
            int totalWhiteKeys = 52;

            float whiteKeyWidth = keyboardWidth / totalWhiteKeys;
            float offsetX = whiteKeyWidth * precedingWhiteKeys;

            // black keys need an offset
            if (NoteUtils.IsBlackKey(note))
            {
                // there are the types of black keys left. middle, right
                // note index in octave
                int noteIndex = note % 12;

                // left black key c#, f#
                if (noteIndex == 1 || noteIndex == 6)
                {
                    offsetX -= whiteKeyWidth / 10 * 3.5f;
                }
                else if (noteIndex == 3 || noteIndex == 10)
                {
                    // right black key d#, a#
                    offsetX -= whiteKeyWidth / 10;
                }
                else
                {
                    // middle between two white notes
                    offsetX -= whiteKeyWidth / 10 * 2f;
                }

                if (!toLeft)
                    offsetX += whiteKeyWidth / 2 / 2;
            }
            else
            {
                // the white tile is smaller than the white key
                offsetX += whiteKeyWidth / 10 * 0.75f;

                if (!toLeft)
                    offsetX += whiteKeyWidth / 2;
            }

            return offsetX;
        }
    }
}
