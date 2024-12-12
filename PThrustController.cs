using VRageMath;
using static TinHovers.HoverSuspensionComponent;

namespace TinHovers
{
    public class PThrustController : IHoverThrustController
    {
        private float _kp;

        public PThrustController(float kp)
        {
            _kp = kp;
        }

        public float CalculateThrustMultiplier(double heightError, double deltaTime)
        {
            float output = (float)(_kp * heightError);
            return MathHelper.Clamp(output, 0f, 1f);
        }
    }
}
