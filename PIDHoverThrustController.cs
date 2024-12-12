namespace TinHovers
{
    /// <summary>
    /// A hover thrust controller implementation using a PID controller.
    /// This controller calculates a thrust multiplier (0 to 1 or possibly beyond)
    /// based on the current height error. 
    /// 
    /// Example Usage:
    /// heightError = (targetHeight - currentHeight)
    /// If positive, the PID might produce a positive output, meaning "increase thrust."
    /// If negative, reduce thrust.
    /// </summary>
    public class PIDHoverThrustController : IHoverThrustController
    {
        private readonly PIDController _pid;
        private readonly float _maxThrustMultiplier;
        private readonly float _minThrustMultiplier;

        /// <summary>
        /// Initializes a new PID-based hover thrust controller.
        /// </summary>
        /// <param name="pid">Configured PID controller</param>
        /// <param name="maxThrustMultiplier">Maximum thrust multiplier allowed (e.g., 1f)</param>
        /// <param name="minThrustMultiplier">Minimum thrust multiplier allowed (e.g., 0f)</param>
        public PIDHoverThrustController(PIDController pid, float maxThrustMultiplier = 1f, float minThrustMultiplier = 0f)
        {
            _pid = pid;
            _maxThrustMultiplier = maxThrustMultiplier;
            _minThrustMultiplier = minThrustMultiplier;
        }

        /// <summary>
        /// Calculate the thrust multiplier based on the height error.
        /// </summary>
        /// <param name="heightError">Difference between target and current height. Positive if too low, negative if too high.</param>
        /// <returns>A thrust multiplier clamped between min and max values.</returns>
        public float CalculateThrustMultiplier(double heightError, double deltaTime)
        {
            // Compute the PID output
            double pidOutput = _pid.Compute(heightError, deltaTime);

            // Convert PID output to a thrust multiplier.
            // We assume PID output ~0 means at target, >0 means need more thrust.
            float thrust = (float)pidOutput; // TODO tune scaling factor, not sure if a direct mapping will work

            // Clamp thrust multiplier
            if (thrust > _maxThrustMultiplier) thrust = _maxThrustMultiplier;
            if (thrust < _minThrustMultiplier) thrust = _minThrustMultiplier;

            return thrust;
        }

        /// <summary>
        /// Optionally reset the PID controller if needed.
        /// </summary>
        public void ResetController()
        {
            _pid.Reset();
        }
    }
}
