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
    public class PIDController
    {
        // Proportional, Integral, and Derivative gains
        private readonly double _proportionalGain;
        private readonly double _integralGain;
        private readonly double _derivativeGain;

        // Integral term and last input value
        private double _integral;
        private double _previousInput;

        // Integral term limits
        private readonly double _integralUpperLimit;
        private readonly double _integralLowerLimit;

        // Number of control steps per second
        private readonly double _stepsPerSecond;

        /// <summary>
        /// Initializes a new instance of the <see cref="PIDController"/> class with specified gains and integral limits.
        /// </summary>
        /// <param name="proportionalGain">Proportional gain (Kp)</param>
        /// <param name="integralGain">Integral gain (Ki)</param>
        /// <param name="derivativeGain">Derivative gain (Kd)</param>
        /// <param name="integralUpperLimit">Optional upper limit for the integral term (default is 0, meaning no limit)</param>
        /// <param name="integralLowerLimit">Optional lower limit for the integral term (default is 0, meaning no limit)</param>
        /// <param name="stepsPerSecond">Number of control steps executed per second (default is 60)</param>
        public PIDController(
            double proportionalGain,
            double integralGain,
            double derivativeGain,
            double integralUpperLimit = 0,
            double integralLowerLimit = 0,
            float stepsPerSecond = 60f)
        {
            _proportionalGain = proportionalGain;
            _integralGain = integralGain;
            _derivativeGain = derivativeGain;
            _integralUpperLimit = integralUpperLimit;
            _integralLowerLimit = integralLowerLimit;
            _stepsPerSecond = stepsPerSecond;
            _integral = 0;
            _previousInput = 0;
        }

        /// <summary>
        /// Computes the PID controller output based on the current input.
        /// </summary>
        /// <param name="input">The current input value to the controller.</param>
        /// <param name="decimalPrecision">Number of decimal places to round the input value.</param>
        /// <returns>The computed PID output.</returns>
        public double Compute(double input, int decimalPrecision)
        {
            // Round the input to the specified decimal precision
            double roundedInput = Math.Round(input, decimalPrecision);

            // Update the integral term with the current input
            _integral += input / _stepsPerSecond;

            // Clamp the integral term within the specified limits, if any
            if (_integralUpperLimit > 0 && _integral > _integralUpperLimit)
            {
                _integral = _integralUpperLimit;
            }
            if (_integralLowerLimit < 0 && _integral < _integralLowerLimit)
            {
                _integral = _integralLowerLimit;
            }

            // Calculate the derivative term based on the change in input
            double derivative = (_previousInput - roundedInput) * _stepsPerSecond;
            _previousInput = roundedInput;

            // Compute the PID output
            double output = (_proportionalGain * input) + (_integralGain * _integral) + (_derivativeGain * derivative);

            return output;
        }

        /// <summary>
        /// Resets the PID controller by clearing the integral and previous input.
        /// </summary>
        public void Reset()
        {
            _integral = 0;
            _previousInput = 0;
        }
    }

}
