﻿/* ***************************************************************************
 * This file is part of SharpNEAT - Evolution of Neural Networks.
 * 
 * Copyright 2004-2019 Colin Green (sharpneat@gmail.com)
 *
 * SharpNEAT is free software; you can redistribute it and/or modify
 * it under the terms of The MIT License (MIT).
 *
 * You should have received a copy of the MIT License
 * along with SharpNEAT; if not, see https://opensource.org/licenses/MIT.
 */
using System;
using SharpNeat.BlackBox;
using SharpNeat.Evaluation;

namespace SharpNeat.Tasks.SinglePoleBalancing
{
    /// <summary>
    /// Evaluator for the Single Pole Balancing task.
    /// </summary>
    public class SinglePoleBalancingEvaluator : IPhenomeEvaluator<IBlackBox<double>>
    {
        const double MaxForce = 10.0;                // Maximum force applied to the cart, in Newtons.
		const double TwelveDegrees = Math.PI / 15.0; //= 0.2094384 radians.
	
        #region Instance Fields

		readonly int	_maxTimesteps;
		readonly double _trackLengthThreshold;
		readonly double _poleAngleThreshold;
        readonly SinglePoleBalancingPhysics _physicsEngine;

        #endregion

        #region Constructors

        /// <summary>
        /// Construct evaluator with default task arguments/variables.
        /// </summary>
		public SinglePoleBalancingEvaluator() 
            : this(200_000, 2.4, TwelveDegrees)
		{}

        /// <summary>
        /// Construct evaluator with the provided task arguments/variables.
        /// </summary>
		public SinglePoleBalancingEvaluator(
            int maxTimesteps,
            double trackLengthThreshold,
            double poleAngleThreshold)
		{
			_maxTimesteps = maxTimesteps;
            _trackLengthThreshold = trackLengthThreshold;
			_poleAngleThreshold = poleAngleThreshold;
            _physicsEngine = new SinglePoleBalancingPhysics();
		}

		#endregion

        #region Public Methods

        /// <summary>
        /// Evaluate the provided black box against the Single Pole Balancing task,
        /// and return its fitness score.
        /// </summary>
        /// <param name="box">The black box to evaluate.</param>
        public FitnessInfo Evaluate(IBlackBox<double> box)
        {
			// Initialise state. 
            var physics = _physicsEngine;
            physics.ResetState();

			// Run the pole-balancing simulation.
            int timestep = 0;
			for(; timestep < _maxTimesteps; timestep++)
			{
				// Provide state info to the black box inputs (normalised to +-1.0).
                box.InputVector[0] = 1.0; // Bias input.
                box.InputVector[1] = physics.CartPosX / _trackLengthThreshold;  // CartPosX range is +-_trackLengthThreshold; here we normalize it to [-1,1].
                box.InputVector[2] = physics.CartVelocity;                      // cart_velocity_x is typically +-0.75
                box.InputVector[3] = physics.PoleAngle / TwelveDegrees;         // pole_angle is +-twelve_degrees; here we normalize it to [-1,1].
                box.InputVector[4] = physics.PoleAngularVelocity;               // pole_angular_velocity is typically +-1.0 radians; no scaling required.

				// Activate the network.
                box.Activate();

                // Read the output to determine the force to be applied to the cart by the controller.
                double force = box.OutputVector[0] - 0.5;
                ClipForce(ref force);
                force *= MaxForce;

                // Calculate model state at next timestep.
                physics.Update(force);

				// Check for failure state. I.e. has the cart run off the ends of the track, or has the pole
				// angle exceeded the defined threshold.
				if(Math.Abs(physics.CartPosX) > _trackLengthThreshold || Math.Abs(physics.PoleAngle) > _poleAngleThreshold) {
					break;
                }
			}

            // The controller's fitness is defined as the number of timesteps that elapsed before failure.
            double fitness = timestep + (_trackLengthThreshold - Math.Abs(physics.CartPosX)) * 5.0;
            return new FitnessInfo(fitness);
        }

        #endregion

        #region Private Static Methods

        private static void ClipForce(ref double x)
        {
            if(x < -1) x = -1.0;
            else if(x > 1.0) x = 1.0;
        }

        #endregion
    }
}
