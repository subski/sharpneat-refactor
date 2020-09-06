﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SharpNeat.Drawing;
using SharpNeat.Drawing.Graph;
using SharpNeat.Graphs;

namespace UISandbox
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender,EventArgs e)
        {
            var graphViewportPainter = new GraphViewportPainter();
            this.viewportControl1.ViewportPainter = graphViewportPainter;

            DirectedGraphViewModel graphViewModel = CreateGraphViewModel(this.viewportControl1.Size);

            graphViewportPainter.GraphViewModel = graphViewModel;

            this.viewportControl1.Refresh();
        }


        #region Private Static Methods

        private static DirectedGraphViewModel CreateGraphViewModel(Size layoutArea)
        {
            // Simple acyclic graph.
            var connList = new List<WeightedDirectedConnection<float>>
            {
                new WeightedDirectedConnection<float>(0, 4, 1f),
                new WeightedDirectedConnection<float>(4, 5, 2f),
                new WeightedDirectedConnection<float>(5, 2, 3f),
                new WeightedDirectedConnection<float>(1, 2, 4f),
                new WeightedDirectedConnection<float>(2, 2, 5f),
                new WeightedDirectedConnection<float>(2, 3, 5f),
                new WeightedDirectedConnection<float>(2, 4, 5f),
                new WeightedDirectedConnection<float>(2, 5, 5f)
            };
            connList.Sort(WeightedDirectedConnectionComparer<float>.Default);

            // Create graph.
            var digraph = WeightedDirectedGraphBuilder<float>.Create(connList, 2, 2);

            
            int[] nodeIdByIdx = CreateNodeIdByIdx(digraph.TotalNodeCount);
            DirectedGraphViewModel graphViewModel = new DirectedGraphViewModel(digraph, digraph.WeightArray, nodeIdByIdx);
            return graphViewModel;
        }

        private static int[] CreateNodeIdByIdx(int length)
        {
            var arr = new int[length];
            for(int i=0; i < length; i++) {
                arr[i] = i;
            }
            return arr;
        }

        #endregion
    }
}