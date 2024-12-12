namespace TinHovers
{
    /// <summary>
    /// Interface for different thrust control strategies.
    /// Implementations might be BangBangThrustController, PThrustController, PIDThrustController.
    /// </summary>
    public interface IHoverThrustController
    {
        float CalculateThrustMultiplier(float heightError, float heightDeltaError);
    }
}
