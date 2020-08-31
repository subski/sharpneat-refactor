﻿/* ***************************************************************************
 * This file is part of SharpNEAT - Evolution of Neural Networks.
 * 
 * Copyright 2004-2020 Colin Green (sharpneat@gmail.com)
 *
 * SharpNEAT is free software; you can redistribute it and/or modify
 * it under the terms of The MIT License (MIT).
 *
 * You should have received a copy of the MIT License
 * along with SharpNEAT; if not, see https://opensource.org/licenses/MIT.
 */
using System;
using System.Drawing;
using SharpNeat.Graphs;

namespace SharpNeat.Drawing
{
    /// <summary>
    /// Represents a directed graph, with supplementary data suitable for producing a 2D visual representation of the graph.
    /// </summary>
    public class DirectedGraphViewModel
    {
        /// <summary>
        /// Represents the directed graph topology.
        /// </summary>
        public DirectedGraph DirectedGraph { get; }

        /// <summary>
        /// Graph connection/vertex weights.
        /// </summary>
        public float[] WeightArr { get; }

        /// <summary>
        /// Provides a ID/label for each node.
        /// </summary>
        public int[] NodeIdByIdx { get; }

        /// <summary>
        /// Provides a 2D position for each node.
        /// </summary>
        public Point[] NodePosByIdx { get; }

        /// <summary>
        /// Construct with the provided digraph, and supplementary data suitable for producing a 2D visual representation of the graph.
        /// </summary>
        /// <param name="digraph">Directed graph.</param>
        /// <param name="weightArr">Graph connection/vertex weights.</param>
        /// <param name="nodeIdByIdx">Provides a ID/label for each node.</param>
        /// <param name="nodePosByIdx">Provides a 2D position for each node.</param>
        public DirectedGraphViewModel(
            DirectedGraph digraph,
            float[] weightArr,
            int[] nodeIdByIdx,
            Point[] nodePosByIdx)
        {
            if(weightArr.Length != digraph.TotalNodeCount) throw new ArgumentException(nameof(weightArr));
            if(nodeIdByIdx.Length != digraph.TotalNodeCount) throw new ArgumentException(nameof(nodeIdByIdx));
            if(nodePosByIdx.Length != digraph.TotalNodeCount) throw new ArgumentException(nameof(nodePosByIdx));

            this.DirectedGraph = digraph;
            this.WeightArr = weightArr;
            this.NodeIdByIdx = nodeIdByIdx;
            this.NodePosByIdx = nodePosByIdx;
        }
    }
}
