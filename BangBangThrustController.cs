namespace TinHovers
{
    public class BangBangThrustController : IHoverThrustController
    {
        public float CalculateThrustMultiplier(float heightError, float heightDeltaError)
        {
            // Simplistic: full force if below target, none if above
            return heightError > 0 ? 1f : 0f;
        }
    }
}
