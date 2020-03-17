using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{

    public static class NoteUtils
    {
        private static int[] octaveBlackKeysIndexes = new int[] { 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0 };

        public static bool IsBlackKey(byte note) {
            return octaveBlackKeysIndexes[note % 12] == 1;
        }
    }
}
