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
using System.Diagnostics;

namespace SharpNeat.Graphs.Acyclic
{
    /// <summary>
    /// For building instances of <see cref="WeightedDirectedGraphAcyclic{T}"/>.
    /// </summary>
    /// <typeparam name="T">Connection weight data type.</typeparam>
    public static class WeightedDirectedGraphAcyclicBuilder<T>
        where T : struct
    {
        #region Public Static Methods

        /// <summary>
        /// Create with the provided list of connections, and input/output node counts.
        /// </summary>
        /// <param name="connections">A span of weighted connections that describe the graph.</param>
        /// <param name="inputCount">Input node count.</param>
        /// <param name="outputCount">Output node count.</param>
        /// <returns>A new instance of <see cref="WeightedDirectedGraphAcyclic{T}"/>.</returns>
        public static WeightedDirectedGraphAcyclic<T> Create(
            Span<WeightedDirectedConnection<T>> connections,
            int inputCount, int outputCount)
        {
            // Convert the set of connections to a standardised graph representation.
            WeightedDirectedGraph<T> digraph = WeightedDirectedGraphBuilder<T>.Create(connections, inputCount, outputCount);

            // Invoke factory logic specific to acyclic graphs.
            return Create(digraph);
        }

        /// <summary>
        /// Create from the provided <see cref="WeightedDirectedGraph{T}"/>.
        /// </summary>
        /// <remarks>
        /// The provided graph is expected to describe an acyclic graph; this method asserts that is the case and builds
        /// a formal acyclic graph representation.
        /// </remarks>
        /// <param name="digraph">The directed graph.</param>
        /// <returns>A new instance of <see cref="WeightedDirectedGraphAcyclic{T}"/>.</returns>
        public static WeightedDirectedGraphAcyclic<T> Create(
            WeightedDirectedGraph<T> digraph)
        {
            // Calc the depth of each node in the digraph.
            // ENHANCEMENT: Use a re-usable instance of AcyclicGraphDepthAnalysis.
            GraphDepthInfo depthInfo = new AcyclicGraphDepthAnalysis().CalculateNodeDepths(digraph);

            return CreateInner(digraph, depthInfo);
        }

        /// <summary>
        /// Create from the provided <see cref="WeightedDirectedGraph{T}"/> and associated depth info.
        /// </summary>
        /// <param name="digraph">The directed graph.</param>
        /// <param name="depthInfo">Depth info associated with <paramref name="digraph"/>.</param>
        /// <returns>A new instance of <see cref="WeightedDirectedGraphAcyclic{T}"/>.</returns>
        /// <remarks>
        /// The provided graph is expected to describe an acyclic graph; this method asserts that is the case and builds
        /// a formal acyclic graph representation.
        /// </remarks>
        public static WeightedDirectedGraphAcyclic<T> Create(
            WeightedDirectedGraph<T> digraph,
            GraphDepthInfo depthInfo)
        {
            // Assert that the passed in depth info is correct.
            // Note. This test is expensive because it invokes a graph traversal algorithm to determine node depths.

            // ENHANCEMENT: Use a re-usable instance of AcyclicGraphDepthAnalysis.
            Debug.Assert(depthInfo.Equals(new AcyclicGraphDepthAnalysis().CalculateNodeDepths(digraph)));

            return CreateInner(digraph, depthInfo);
        }

        #endregion

        #region Private Static Methods [High Level]

        private static WeightedDirectedGraphAcyclic<T> CreateInner(
            WeightedDirectedGraph<T> digraph,
            GraphDepthInfo depthInfo)
        {
            // Create acyclic digraph.
            var acyclicDigraph = DirectedGraphAcyclicBuilderUtils.CreateDirectedGraphAcyclic(
                digraph,
                depthInfo,
                out int[] _,
                out int[] connectionIndexMap);

            // Copy weights into a new array and into their correct position.
            T[] genomeWeightArr = digraph.WeightArray;
            T[] weightArr = new T[genomeWeightArr.Length];

            for(int i=0; i < weightArr.Length; i++) {
                weightArr[i] = genomeWeightArr[connectionIndexMap[i]];
            }

            // Construct a new WeightedDirectedGraphAcyclic.
            return new WeightedDirectedGraphAcyclic<T>(
                acyclicDigraph.InputCount,
                acyclicDigraph.OutputCount,
                acyclicDigraph.TotalNodeCount,
                acyclicDigraph.ConnectionIds,
                acyclicDigraph.LayerArray,
                acyclicDigraph.OutputNodeIdxArr,
                weightArr);
        }

        #endregion
    }
}
