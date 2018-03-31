﻿using System;
using System.Collections.Generic;
using SharpNeatLib.Network;

namespace SharpNeat.Network
{
    public static class DirectedGraphUtils
    {
        /// <summary>
        /// Determine the set of node IDs, order them (thus assigning each node ID an index),
        /// and build a dictionary of indexes keyed by ID.
        /// </summary>
        public static INodeIdMap CompileNodeIdMap(int[] hiddenNodeIdArr, int inputOutputCount)
        {
            // Build dictionary of hidden node new ID/index keyed by old ID.
            // Note. the new IDs start immediately after the last input/output node ID (defined by inputOutputCount).
            var hiddenNodeIdxById = new Dictionary<int,int>(hiddenNodeIdArr.Length);
            for(int i=0, newId=inputOutputCount; i < hiddenNodeIdArr.Length; i++, newId++) {
                hiddenNodeIdxById.Add(hiddenNodeIdArr[i], newId);
            }

            return new DictionaryNodeIdMap(inputOutputCount, hiddenNodeIdxById);
        }
    }
}
