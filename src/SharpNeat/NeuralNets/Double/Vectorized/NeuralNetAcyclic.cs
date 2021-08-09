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
using System.Buffers;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SharpNeat.BlackBox;
using SharpNeat.Graphs;
using SharpNeat.Graphs.Acyclic;

namespace SharpNeat.NeuralNets.Double.Vectorized
{
    /// <summary>
    /// A version of <see cref="Double.NeuralNetAcyclic"/> that utilises some vectorized operations
    /// for improved performance on hardware platforms that support them.
    /// </summary>
    public sealed class NeuralNetAcyclic : IBlackBox<double>
    {
        #region Instance Fields

        // Connection arrays.
        readonly ConnectionIds _connIds;
        readonly double[] _weightArr;

        // Array of layer information.
        readonly LayerInfo[] _layerInfoArr;

        // Node activation function.
        readonly VecFn<double> _activationFn;

        // Node activation level array (used for both pre and post activation levels).
        readonly double[] _activationArr;

        // Convenient counts.
        readonly int _inputCount;
        readonly int _outputCount;
        readonly int _totalNodeCount;

        // Connection inputs array.
        readonly double[] _conInputArr = new double[Vector<double>.Count];
        volatile bool _isDisposed;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a AcyclicNeuralNet with the provided neural net definition parameters.
        /// </summary>
        /// <param name="digraph">Network structure definition.</param>
        /// <param name="activationFn">Node activation function.</param>
        public NeuralNetAcyclic(
            WeightedDirectedGraphAcyclic<double> digraph,
            VecFn<double> activationFn)
            : this(digraph, digraph.WeightArray, activationFn)
        {}

        /// <summary>
        /// Constructs a AcyclicNeuralNet with the provided neural net definition parameters.
        /// </summary>
        /// <param name="digraph">Network structure definition.</param>
        /// <param name="weightArr">Connection weights array.</param>
        /// <param name="activationFn">Node activation function.</param>
        public NeuralNetAcyclic(
            DirectedGraphAcyclic digraph,
            double[] weightArr,
            VecFn<double> activationFn)
        {
            Debug.Assert(digraph.ConnectionIds.GetSourceIdSpan().Length == weightArr.Length);

            // Store refs to network structure data.
            _connIds = digraph.ConnectionIds;
            _weightArr = weightArr;
            _layerInfoArr = digraph.LayerArray;

            // Store network activation function.
            _activationFn = activationFn;

            // Store input/output node counts.
            _inputCount = digraph.InputCount;
            _outputCount = digraph.OutputCount;
            _totalNodeCount = digraph.TotalNodeCount;

            // Get a working array for node activation signals.
            _activationArr = ArrayPool<double>.Shared.Rent(_totalNodeCount);

            // Wrap a sub-range of the _activationArr that holds the activation values for the input nodes.
            this.InputVector = new VectorSegment<double>(_activationArr, 0, _inputCount);

            // Wrap the output nodes. Nodes have been sorted by depth within the network therefore the output
            // nodes can no longer be guaranteed to be in a contiguous segment at a fixed location. As such their
            // positions are indicated by outputNodeIdxArr, and so we package up this array with the node signal
            // array to abstract away the indirection described by outputNodeIdxArr.
            this.OutputVector = new MappingVector<double>(_activationArr, digraph.OutputNodeIdxArr);
        }

        #endregion

        #region IBlackBox

        /// <summary>
        /// Gets the number of input nodes.
        /// </summary>
        public int InputCount => _inputCount;

        /// <summary>
        /// Gets the number of output nodes.
        /// </summary>
        public int OutputCount => _outputCount;

        /// <summary>
        /// Gets an array for used for passing input signals to the network, i.e. the network input vector.
        /// </summary>
        public IVector<double> InputVector { get; }

        /// <summary>
        /// Gets an array of output signals from the network, i.e. the network output vector.
        /// </summary>
        public IVector<double> OutputVector { get; }

        /// <summary>
        /// Activate the network. Activation reads input signals from InputSignalArray and writes output signals
        /// to OutputSignalArray.
        /// </summary>
        public void Activate()
        {
            ReadOnlySpan<int> srcIds = _connIds.GetSourceIdSpan();
            ReadOnlySpan<int> tgtIds = _connIds.GetTargetIdSpan();
            ReadOnlySpan<double> weights = _weightArr.AsSpan();
            Span<double> activations = _activationArr.AsSpan(0, _totalNodeCount);
            Span<double> connInputs = _conInputArr.AsSpan();

            ref int srcIdsRef = ref MemoryMarshal.GetReference(srcIds);
            ref int tgtIdsRef = ref MemoryMarshal.GetReference(tgtIds);
            ref double weightsRef = ref MemoryMarshal.GetReference(weights);
            ref double activationsRef = ref MemoryMarshal.GetReference(activations);
            ref double connInputsRef = ref MemoryMarshal.GetReference(connInputs);

            // Reset hidden and output node activation levels, ready for next activation.
            // Note. this reset is performed here instead of after the below loop because this resets the output
            // node values, which are the outputs of the network as a whole following activation; hence
            // they need to be remain unchanged until they have been read by the caller of Activate().
            activations.Slice(_inputCount).Clear();

            // Process all layers in turn.
            int conIdx = 0;
            int nodeIdx = _inputCount;

            // Loop through network layers.
            for(int layerIdx=0; layerIdx < _layerInfoArr.Length - 1; layerIdx++)
            {
                LayerInfo layerInfo = _layerInfoArr[layerIdx];

                // Push signals through the previous layer's connections to the current layer's nodes.
                for(; conIdx <= layerInfo.EndConnectionIdx - Vector<double>.Count; conIdx += Vector<double>.Count)
                {
                    // Load source node output values into a vector.
                    ref int srcIdsRefSeg = ref Unsafe.Add(ref srcIdsRef, conIdx);

                    for(int i = 0; i < Vector<double>.Count; i++)
                    {
                        Unsafe.Add(ref connInputsRef, i) =
                            Unsafe.Add(
                                ref activationsRef,
                                Unsafe.Add(ref srcIdsRefSeg, i));
                    }

                    // Note. This obscure pattern is taken from the Vector<T> constructor source code.
                    var conVec = Unsafe.ReadUnaligned<Vector<double>>(
                        ref Unsafe.As<double, byte>(ref connInputsRef));

                    // Load connection weights into a vector.
                    var weightVec = Unsafe.ReadUnaligned<Vector<double>>(
                        ref Unsafe.As<double, byte>(
                            ref Unsafe.Add(
                                ref weightsRef, conIdx)));

                    // Multiply connection source inputs and connection weights.
                    conVec *= weightVec;

                    // Save/accumulate connection output values onto the connection target nodes.
                    ref int tgtIdsRefSeg = ref Unsafe.Add(ref tgtIdsRef, conIdx);

                    for(int i=0; i < Vector<double>.Count; i++)
                    {
                        Unsafe.Add(ref activationsRef, Unsafe.Add(ref tgtIdsRefSeg, i)) += conVec[i];
                    }
                }

                // Loop remaining connections
                for(; conIdx < layerInfo.EndConnectionIdx; conIdx++)
                {
                    // Get a reference to the target activation level 'slot' in the activations span.
                    ref double tgtSlot = ref Unsafe.Add(ref activationsRef, Unsafe.Add(ref tgtIdsRef, conIdx));

                    // Get the connection source signal, multiply it by the connection weight, add the result
                    // to the target node's current pre-activation level, and store the result.
                    tgtSlot = Math.FusedMultiplyAdd(
                                Unsafe.Add(ref activationsRef, Unsafe.Add(ref srcIdsRef, conIdx)),
                                Unsafe.Add(ref weightsRef, conIdx),
                                tgtSlot);
                }

                // Activate current layer's nodes.
                //
                // Pass the pre-activation levels through the activation function.
                // Note. The resulting post-activation levels are stored in _activationArr.
                layerInfo = _layerInfoArr[layerIdx + 1];
                _activationFn(
                    ref Unsafe.Add(ref activationsRef, nodeIdx),
                    layerInfo.EndNodeIdx - nodeIdx);

                // Update nodeIdx to point at first node in the next layer.
                nodeIdx = layerInfo.EndNodeIdx;
            }
        }

        /// <summary>
        /// Reset the network's internal state.
        /// </summary>
        public void ResetState()
        {
            // Unnecessary for this implementation. The node activation signal state is completely overwritten on each activation.
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Releases both managed and unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if(!_isDisposed)
            {
                _isDisposed = true;
                ArrayPool<double>.Shared.Return(_activationArr);
            }
        }

        #endregion
    }
}
