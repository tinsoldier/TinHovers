using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinHovers
{
    using System;

    /// <summary>
    /// A PID (Proportional-Integral-Derivative) controller for managing control systems.
    /// </summary>
    /// <summary>
    /// A PID controller implementation suitable for hover thrust control.
    /// Error should be the difference between the target and current measurement.
    /// 
    /// Kp: proportional gain
    /// Ki: integral gain
    /// Kd: derivative gain
    ///
    /// Integral is computed over time, derivative is computed based on error change per unit time.
    /// stepsPerSecond should match your update rate (e.g., 60 for 60 ticks/sec).
    ///
    /// integralUpperLimit and integralLowerLimit can be used to prevent integral windup.
    /// </summary>
    public class PIDController
    {
        private readonly double _proportionalGain;
        private readonly double _integralGain;
        private readonly double _derivativeGain;
        private readonly double _integralUpperLimit;
        private readonly double _integralLowerLimit;
        private readonly double _stepsPerSecond;

        private double _integral;
        private double _previousError;

        /// <summary>
        /// Initializes a new instance of the <see cref="PIDController"/> class with specified gains and integral limits.
        /// </summary>
        /// <param name="proportionalGain">Proportional gain (Kp)</param>
        /// <param name="integralGain">Integral gain (Ki)</param>
        /// <param name="derivativeGain">Derivative gain (Kd)</param>
        /// <param name="integralUpperLimit">Optional upper limit for the integral term (0 means no limit)</param>
        /// <param name="integralLowerLimit">Optional lower limit for the integral term (0 means no limit)</param>
        public PIDController(
            double proportionalGain,
            double integralGain,
            double derivativeGain,
            double integralUpperLimit = 0,
            double integralLowerLimit = 0)
        {
            _proportionalGain = proportionalGain;
            _integralGain = integralGain;
            _derivativeGain = derivativeGain;
            _integralUpperLimit = integralUpperLimit;
            _integralLowerLimit = integralLowerLimit;
            _integral = 0;
            _previousError = 0;
        }

        /// <summary>
        /// Computes the PID controller output based on the current error.
        /// error = (target - current)
        /// </summary>
        /// <param name="error">The current error value for the controller.</param>
        /// <returns>The computed PID output.</returns>
        public double Compute(double error, double deltaTime)
        {
            // Update integral with actual elapsed time
            _integral += error * deltaTime;

            // Compute derivative based on error change per elapsed time
            double derivative = (error - _previousError) / deltaTime;

            _previousError = error;

            double output = (_proportionalGain * error)
                            + (_integralGain * _integral)
                            + (_derivativeGain * derivative);

            return output;
        }


        /// <summary>
        /// Resets the PID controller by clearing the integral and previous error.
        /// </summary>
        public void Reset()
        {
            _integral = 0;
            _previousError = 0;
        }
    }
}
