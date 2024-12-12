using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.Components;
using VRage.Game;

namespace TinHovers
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation | MyUpdateOrder.AfterSimulation | MyUpdateOrder.Simulation)]
    public class HoverSuspensionCore : MySessionComponentBase
    {
        public static Dictionary<long, HoverSuspensionComponent> HoverEngines = new Dictionary<long, HoverSuspensionComponent>();

        MyObjectBuilder_SessionComponent m_objectBuilder;

        bool m_init = false;
        public const ushort HandlerId = 45339;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            m_objectBuilder = sessionComponent;
        }

        public override void UpdateBeforeSimulation()
        {
            if (!m_init)
            {
                Init();
            }
        }

        public override void UpdateAfterSimulation()
        {
            if (MyAPIGateway.Multiplayer.IsServer)
            {
                // if necessary
            }
        }

        public override MyObjectBuilder_SessionComponent GetObjectBuilder()
        {
            return m_objectBuilder;
        }

        protected override void UnloadData()
        {
            if (MyAPIGateway.Session.IsServer)
            {

            }

            if (m_init)
            {
                MyAPIGateway.Multiplayer.UnregisterMessageHandler(HandlerId, HoverEngineMessageHandler);
            }

            HoverSuspensionCore.HoverEngines.Clear();
        }

        void Init()
        {
            MyAPIGateway.Multiplayer.RegisterMessageHandler(HandlerId, HoverEngineMessageHandler);
            m_init = true;
        }

        private void HoverEngineMessageHandler(byte[] message)
        {
            HoverSuspensionComponent.DetailData details = MyAPIGateway.Utilities.SerializeFromXML<HoverSuspensionComponent.DetailData>(ASCIIEncoding.ASCII.GetString(message));
            foreach (var h in HoverSuspensionCore.HoverEngines)
            {
                if (details.EntityId == h.Key)
                {
                    h.Value.UpdateClients(details.Details);
                }
            }
        }
    }
}
