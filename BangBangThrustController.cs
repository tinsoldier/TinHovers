namespace TinHovers
{
    public class BangBangThrustController : IHoverThrustController
    {
        public float CalculateThrustMultiplier(double heightError, double deltaTime)
        {
            // Simplistic: full force if below target, none if above
            return heightError > 0 ? 1f : 0f;
        }
    }
}
