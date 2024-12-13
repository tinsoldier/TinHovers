using Sandbox.Game.Entities;
using Sandbox.ModAPI.Interfaces.Terminal;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Utils;
using VRageMath;

namespace TinHovers
{
    public partial class HoverSuspensionComponent
    {
        //Communications
        public class DetailData
        {
            public long EntityId { get; set; }
            public string Details { get; set; }
        }

        public const ushort HandlerId = 44448;
        public Color m_color1 = new Color(0, 255, 0, 255);
        bool ShowStateNextFrame = true;

        public void ShowState()
        {
            //MyAPIGateway.Utilities.ShowMessage("HoverEngine", "Showstate1: block:"+_block+" b:"+b);
            if (_block == null) return;

            var grid = _block.CubeGrid as MyCubeGrid;

            //MyAPIGateway.Utilities.ShowMessage("HoverEngine", "Showstate2: ");
            //MyAPIGateway.Utilities.ShowMessage("HoverEngine", "Showstate2, server: "+m_isserver);
            if (!_block.Enabled || !_block.IsWorking || !grid.DampenersEnabled)
            {
                _block.SetEmissiveParts("emissive10", Color.Red, 0.0f);
                _block.SetEmissiveParts("emissive11", Color.Red, 0.0f);
            }
            else
            {
                _block.SetEmissiveParts("emissive10", m_color1, 5.0f);
                _block.SetEmissiveParts("emissive11", Color.DarkRed, 1.0f);
            }
        }

        //______________________________________________________________________________________________________
        //||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
        //                                      TerminalControls
        //||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
        //______________________________________________________________________________________________________

        private static bool initterminals = false;

        DetailData details = new DetailData();

        float altitudemin_slider_min = 0f;
        float altitudemin_slider_max = 10f;
        float altitudemin_slider_default = 1.5f;

        float altituderange_slider_min = 0f;
        float altituderange_slider_max = 5f;
        float altituderange_slider_default = 2.5f;

        float altituderegulationdistance_slider_min = 1f;
        float altituderegulationdistance_slider_max = 7f;
        float altituderegulationdistance_slider_default = 4f;

        float scalemulti = 3f; // for large grid

        void Set_altitude_min_inc_S(IMyTerminalBlock b)
        {
            if (b == null) return;
            float t = b.GameLogic.GetAs<HoverSuspensionComponent>()._targetHeight + 0.5f;
            if (t > altitudemin_slider_max) { t = altitudemin_slider_max; }
            b.GameLogic.GetAs<HoverSuspensionComponent>()._targetHeight = t;
            Save_data(b);
        }
        void Set_altitude_min_inc_L(IMyTerminalBlock b)
        {
            if (b == null) return;
            float t = b.GameLogic.GetAs<HoverSuspensionComponent>()._targetHeight + 0.5f;
            if (t > altitudemin_slider_max * scalemulti) { t = altitudemin_slider_max * scalemulti; }
            b.GameLogic.GetAs<HoverSuspensionComponent>()._targetHeight = t;
            Save_data(b);
        }
        void Set_altitude_min_dec_S(IMyTerminalBlock b)
        {
            if (b == null) return;
            float t = b.GameLogic.GetAs<HoverSuspensionComponent>()._targetHeight - 0.5f;
            if (t < altitudemin_slider_min) { t = altitudemin_slider_min; }
            b.GameLogic.GetAs<HoverSuspensionComponent>()._targetHeight = t;
            Save_data(b);
        }
        void Set_altitude_min_dec_L(IMyTerminalBlock b)
        {
            if (b == null) return;
            float t = b.GameLogic.GetAs<HoverSuspensionComponent>()._targetHeight - 0.5f;
            if (t < altitudemin_slider_min * scalemulti) { t = altitudemin_slider_min * scalemulti; }
            b.GameLogic.GetAs<HoverSuspensionComponent>()._targetHeight = t;
            Save_data(b);
        }
        void Set_altitude_range_inc_S(IMyTerminalBlock b)
        {
            if (b == null) return;
            float t = b.GameLogic.GetAs<HoverSuspensionComponent>()._targetHeightRange + 0.5f;
            if (t > altituderange_slider_max) { t = altituderange_slider_max; }
            b.GameLogic.GetAs<HoverSuspensionComponent>()._targetHeightRange = t;
            Save_data(b);
        }
        void Set_altitude_range_inc_L(IMyTerminalBlock b)
        {
            if (b == null) return;
            float t = b.GameLogic.GetAs<HoverSuspensionComponent>()._targetHeightRange + 0.5f;
            if (t > altituderange_slider_max * scalemulti) { t = altituderange_slider_max * scalemulti; }
            b.GameLogic.GetAs<HoverSuspensionComponent>()._targetHeightRange = t;
            Save_data(b);
        }
        void Set_altitude_range_dec_S(IMyTerminalBlock b)
        {
            if (b == null) return;
            float t = b.GameLogic.GetAs<HoverSuspensionComponent>()._targetHeightRange - 0.5f;
            if (t < altituderange_slider_min) { t = altituderange_slider_min; }
            b.GameLogic.GetAs<HoverSuspensionComponent>()._targetHeightRange = t;
            Save_data(b);
        }
        void Set_altitude_range_dec_L(IMyTerminalBlock b)
        {
            if (b == null) return;
            float t = b.GameLogic.GetAs<HoverSuspensionComponent>()._targetHeightRange - 0.5f;
            if (t < altituderange_slider_min * scalemulti) { t = altituderange_slider_min * scalemulti; }
            b.GameLogic.GetAs<HoverSuspensionComponent>()._targetHeightRange = t;
            Save_data(b);
        }
        void Set_altitude_regdist_inc_S(IMyTerminalBlock b)
        {
            if (b == null) return;
            float t = b.GameLogic.GetAs<HoverSuspensionComponent>()._heightRegulationRange + 0.5f;
            if (t > altituderegulationdistance_slider_max) { t = altituderegulationdistance_slider_max; }
            b.GameLogic.GetAs<HoverSuspensionComponent>()._heightRegulationRange = t;
            Save_data(b);
        }
        void Set_altitude_regdist_inc_L(IMyTerminalBlock b)
        {
            if (b == null) return;
            float t = b.GameLogic.GetAs<HoverSuspensionComponent>()._heightRegulationRange + 0.5f;
            if (t > altituderegulationdistance_slider_max * scalemulti) { t = altituderegulationdistance_slider_max * scalemulti; }
            b.GameLogic.GetAs<HoverSuspensionComponent>()._heightRegulationRange = t;
            Save_data(b);
        }
        void Set_altitude_regdist_dec_S(IMyTerminalBlock b)
        {
            if (b == null) return;
            float t = b.GameLogic.GetAs<HoverSuspensionComponent>()._heightRegulationRange - 0.5f;
            if (t < altituderegulationdistance_slider_min) { t = altituderegulationdistance_slider_min; }
            b.GameLogic.GetAs<HoverSuspensionComponent>()._heightRegulationRange = t;
            Save_data(b);
        }
        void Set_altitude_regdist_dec_L(IMyTerminalBlock b)
        {
            if (b == null) return;
            float t = b.GameLogic.GetAs<HoverSuspensionComponent>()._heightRegulationRange - 0.5f;
            if (t < altituderegulationdistance_slider_min * scalemulti) { t = altituderegulationdistance_slider_min * scalemulti; }
            b.GameLogic.GetAs<HoverSuspensionComponent>()._heightRegulationRange = t;
            Save_data(b);
        }
        void Set_altitudemin(IMyTerminalBlock b, float f)
        {
            if (b == null) return;
            b.GameLogic.GetAs<HoverSuspensionComponent>()._targetHeight = f;
            Save_data(b);
        }
        void Set_altituderange(IMyTerminalBlock b, float f)
        {
            if (b == null) return;
            b.GameLogic.GetAs<HoverSuspensionComponent>()._targetHeightRange = f;
            Save_data(b);
        }
        void Set_altituderegdist(IMyTerminalBlock b, float f)
        {
            if (b == null) return;
            b.GameLogic.GetAs<HoverSuspensionComponent>()._heightRegulationRange = f;
            Save_data(b);
        }
        void Set_Color1(IMyTerminalBlock b, Color c)
        {
            if (b == null) return;
            b.GameLogic.GetAs<HoverSuspensionComponent>().m_color1 = c;
            Save_data(b);
            b.GameLogic.GetAs<HoverSuspensionComponent>().ShowStateNextFrame = true;
        }

        public void Buttons_SmallBlock()
        {
            // Altitude Min slider control and action---------------------------
            var S_altitudemin = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyThrust>("altitudemin_slider_S");
            S_altitudemin.Title = MyStringId.GetOrCompute("Altitude Min");
            S_altitudemin.Tooltip = MyStringId.GetOrCompute(altitudemin_slider_min + "m - " + altitudemin_slider_max + "m, minimum altitude at 0m/s speed");
            S_altitudemin.Writer = (b, t) => t.AppendFormat("{0:N1}", S_altitudemin.Getter(b)).Append(" m");
            S_altitudemin.SetLimits(altitudemin_slider_min, altitudemin_slider_max);
            S_altitudemin.Getter = (b) => b.GameLogic.GetAs<HoverSuspensionComponent>()._targetHeight;
            S_altitudemin.Setter = Set_altitudemin;
            S_altitudemin.Enabled = (b) => b.BlockDefinition.SubtypeId.Contains("HoverSuspension_S");
            S_altitudemin.Visible = (b) => b.BlockDefinition.SubtypeId.Contains("HoverSuspension_S");
            MyAPIGateway.TerminalControls.AddControl<IMyThrust>(S_altitudemin);

            var S_altitudemin_inc_action = MyAPIGateway.TerminalControls.CreateAction<Sandbox.ModAPI.Ingame.IMyThrust>("altitudemin_slider_inc_S");
            S_altitudemin_inc_action.Action = Set_altitude_min_inc_S;
            S_altitudemin_inc_action.Name = new StringBuilder("increase altitude min");
            S_altitudemin_inc_action.Writer = S_altitudemin.Writer;
            S_altitudemin_inc_action.Icon = @"Textures\GUI\Icons\Actions\Increase.dds";
            S_altitudemin_inc_action.Enabled = (b) => b.BlockDefinition.SubtypeId.Contains("HoverSuspension_S");
            MyAPIGateway.TerminalControls.AddAction<Sandbox.ModAPI.Ingame.IMyThrust>(S_altitudemin_inc_action);

            var S_altitudemin_dec_action = MyAPIGateway.TerminalControls.CreateAction<Sandbox.ModAPI.Ingame.IMyThrust>("altitudemin_slider_dec_S");
            S_altitudemin_dec_action.Action = Set_altitude_min_dec_S;
            S_altitudemin_dec_action.Name = new StringBuilder("decrease altitude min");
            S_altitudemin_dec_action.Writer = S_altitudemin.Writer;
            S_altitudemin_dec_action.Icon = @"Textures\GUI\Icons\Actions\Decrease.dds";
            S_altitudemin_dec_action.Enabled = (b) => b.BlockDefinition.SubtypeId.Contains("HoverSuspension_S");
            MyAPIGateway.TerminalControls.AddAction<Sandbox.ModAPI.Ingame.IMyThrust>(S_altitudemin_dec_action);
            //---------------------------

            // Altitude Range slider control and action---------------------------
            var S_altituderange = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyThrust>("altituderange_slider_S");
            S_altituderange.Title = MyStringId.GetOrCompute("Altitude Range");
            S_altituderange.Tooltip = MyStringId.GetOrCompute(altituderange_slider_min + "m - " + altituderange_slider_max + "m, altitude range between speed 0m/s and 100m/s");
            S_altituderange.Writer = (b, t) => t.AppendFormat("{0:N1}", S_altituderange.Getter(b)).Append(" m");
            S_altituderange.SetLimits(altituderange_slider_min, altituderange_slider_max);
            S_altituderange.Getter = (b) => b.GameLogic.GetAs<HoverSuspensionComponent>()._targetHeightRange;
            S_altituderange.Setter = Set_altituderange;
            S_altituderange.Enabled = (b) => b.BlockDefinition.SubtypeId.Contains("HoverSuspension_S");
            S_altituderange.Visible = (b) => b.BlockDefinition.SubtypeId.Contains("HoverSuspension_S");
            MyAPIGateway.TerminalControls.AddControl<IMyThrust>(S_altituderange);

            var S_altituderange_inc_action = MyAPIGateway.TerminalControls.CreateAction<Sandbox.ModAPI.Ingame.IMyThrust>("altituderange_slider_inc_S");
            S_altituderange_inc_action.Action = Set_altitude_range_inc_S;
            S_altituderange_inc_action.Name = new StringBuilder("increase altitude range");
            S_altituderange_inc_action.Writer = S_altituderange.Writer;
            S_altituderange_inc_action.Icon = @"Textures\GUI\Icons\Actions\Increase.dds";
            S_altituderange_inc_action.Enabled = (b) => b.BlockDefinition.SubtypeId.Contains("HoverSuspension_S");
            MyAPIGateway.TerminalControls.AddAction<Sandbox.ModAPI.Ingame.IMyThrust>(S_altituderange_inc_action);

            var S_altituderange_dec_action = MyAPIGateway.TerminalControls.CreateAction<Sandbox.ModAPI.Ingame.IMyThrust>("altituderange_slider_dec_S");
            S_altituderange_dec_action.Action = Set_altitude_range_dec_S;
            S_altituderange_dec_action.Name = new StringBuilder("decrease altitude range");
            S_altituderange_dec_action.Writer = S_altituderange.Writer;
            S_altituderange_dec_action.Icon = @"Textures\GUI\Icons\Actions\Decrease.dds";
            S_altituderange_dec_action.Enabled = (b) => b.BlockDefinition.SubtypeId.Contains("HoverSuspension_S");
            MyAPIGateway.TerminalControls.AddAction<Sandbox.ModAPI.Ingame.IMyThrust>(S_altituderange_dec_action);
            //---------------------------       	

            // Altitude regulation distance slider control and action---------------------------
            var S_altituderegdist = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyThrust>("altituderegdist_slider_S");
            S_altituderegdist.Title = MyStringId.GetOrCompute("Altitude regulation distance");
            S_altituderegdist.Tooltip = MyStringId.GetOrCompute(altituderegulationdistance_slider_min + "m - " + altituderegulationdistance_slider_max + "m, altitude regulation distance (range of spring), low = hard, high = soft");
            S_altituderegdist.Writer = (b, t) => t.AppendFormat("{0:N1}", S_altituderegdist.Getter(b)).Append(" m");
            S_altituderegdist.SetLimits(altituderegulationdistance_slider_min, altituderegulationdistance_slider_max);
            S_altituderegdist.Getter = (b) => b.GameLogic.GetAs<HoverSuspensionComponent>()._heightRegulationRange;
            S_altituderegdist.Setter = Set_altituderegdist;
            S_altituderegdist.Enabled = (b) => b.BlockDefinition.SubtypeId.Contains("HoverSuspension_S");
            S_altituderegdist.Visible = (b) => b.BlockDefinition.SubtypeId.Contains("HoverSuspension_S");
            MyAPIGateway.TerminalControls.AddControl<IMyThrust>(S_altituderegdist);

            var S_altituderegdist_inc_action = MyAPIGateway.TerminalControls.CreateAction<Sandbox.ModAPI.Ingame.IMyThrust>("altituderegdist_slider_inc_S");
            S_altituderegdist_inc_action.Action = Set_altitude_regdist_inc_S;
            S_altituderegdist_inc_action.Name = new StringBuilder("increase altitude regulation distance");
            S_altituderegdist_inc_action.Writer = S_altituderegdist.Writer;
            S_altituderegdist_inc_action.Icon = @"Textures\GUI\Icons\Actions\Increase.dds";
            S_altituderegdist_inc_action.Enabled = (b) => b.BlockDefinition.SubtypeId.Contains("HoverSuspension_S");
            MyAPIGateway.TerminalControls.AddAction<Sandbox.ModAPI.Ingame.IMyThrust>(S_altituderegdist_inc_action);

            var S_altituderegdist_dec_action = MyAPIGateway.TerminalControls.CreateAction<Sandbox.ModAPI.Ingame.IMyThrust>("altituderegdist_slider_dec_S");
            S_altituderegdist_dec_action.Action = Set_altitude_regdist_dec_S;
            S_altituderegdist_dec_action.Name = new StringBuilder("decrease altitude regulation distance");
            S_altituderegdist_dec_action.Writer = S_altituderegdist.Writer;
            S_altituderegdist_dec_action.Icon = @"Textures\GUI\Icons\Actions\Decrease.dds";
            S_altituderegdist_dec_action.Enabled = (b) => b.BlockDefinition.SubtypeId.Contains("HoverSuspension_S");
            MyAPIGateway.TerminalControls.AddAction<Sandbox.ModAPI.Ingame.IMyThrust>(S_altituderegdist_dec_action);
        }

        public void Buttons_LargeBlock()
        {
            // Altitude Min slider control and action---------------------------
            var L_altitudemin = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyThrust>("altitudemin_slider_L");
            L_altitudemin.Title = MyStringId.GetOrCompute("Altitude Min");
            L_altitudemin.Tooltip = MyStringId.GetOrCompute(altitudemin_slider_min * scalemulti + "m - " + altitudemin_slider_max * scalemulti + "m, minimum altitude at 0m/s speed");
            L_altitudemin.Writer = (b, t) => t.AppendFormat("{0:N1}", L_altitudemin.Getter(b)).Append(" m");
            L_altitudemin.SetLimits(altitudemin_slider_min * scalemulti, altitudemin_slider_max * scalemulti);
            L_altitudemin.Getter = (b) => b.GameLogic.GetAs<HoverSuspensionComponent>()._targetHeight;
            L_altitudemin.Setter = Set_altitudemin;
            L_altitudemin.Enabled = (b) => b.BlockDefinition.SubtypeId.Contains("HoverSuspension_L");
            L_altitudemin.Visible = (b) => b.BlockDefinition.SubtypeId.Contains("HoverSuspension_L");
            MyAPIGateway.TerminalControls.AddControl<IMyThrust>(L_altitudemin);

            var L_altitudemin_inc_action = MyAPIGateway.TerminalControls.CreateAction<Sandbox.ModAPI.Ingame.IMyThrust>("altitudemin_slider_inc_L");
            L_altitudemin_inc_action.Action = Set_altitude_min_inc_L;
            L_altitudemin_inc_action.Name = new StringBuilder("increase altitude min");
            L_altitudemin_inc_action.Writer = L_altitudemin.Writer;
            L_altitudemin_inc_action.Icon = @"Textures\GUI\Icons\Actions\Increase.dds";
            L_altitudemin_inc_action.Enabled = (b) => b.BlockDefinition.SubtypeId.Contains("HoverSuspension_L");
            MyAPIGateway.TerminalControls.AddAction<Sandbox.ModAPI.Ingame.IMyThrust>(L_altitudemin_inc_action);

            var L_altitudemin_dec_action = MyAPIGateway.TerminalControls.CreateAction<Sandbox.ModAPI.Ingame.IMyThrust>("altitudemin_slider_dec_L");
            L_altitudemin_dec_action.Action = Set_altitude_min_dec_L;
            L_altitudemin_dec_action.Name = new StringBuilder("decrease altitude min");
            L_altitudemin_dec_action.Writer = L_altitudemin.Writer;
            L_altitudemin_dec_action.Icon = @"Textures\GUI\Icons\Actions\Decrease.dds";
            L_altitudemin_dec_action.Enabled = (b) => b.BlockDefinition.SubtypeId.Contains("HoverSuspension_L");
            MyAPIGateway.TerminalControls.AddAction<Sandbox.ModAPI.Ingame.IMyThrust>(L_altitudemin_dec_action);
            //---------------------------

            // Altitude Range slider control and action---------------------------
            var L_altituderange = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyThrust>("altituderange_slider_L");
            L_altituderange.Title = MyStringId.GetOrCompute("Altitude Range");
            L_altituderange.Tooltip = MyStringId.GetOrCompute(altituderange_slider_min * scalemulti + "m - " + altituderange_slider_max * scalemulti + "m, altitude range between speed 0m/s and 100m/s");
            L_altituderange.Writer = (b, t) => t.AppendFormat("{0:N1}", L_altituderange.Getter(b)).Append(" m");
            L_altituderange.SetLimits(altituderange_slider_min * scalemulti, altituderange_slider_max * scalemulti);
            L_altituderange.Getter = (b) => b.GameLogic.GetAs<HoverSuspensionComponent>()._targetHeightRange;
            L_altituderange.Setter = Set_altituderange;
            L_altituderange.Enabled = (b) => b.BlockDefinition.SubtypeId.Contains("HoverSuspension_L");
            L_altituderange.Visible = (b) => b.BlockDefinition.SubtypeId.Contains("HoverSuspension_L");
            MyAPIGateway.TerminalControls.AddControl<IMyThrust>(L_altituderange);

            var L_altituderange_inc_action = MyAPIGateway.TerminalControls.CreateAction<Sandbox.ModAPI.Ingame.IMyThrust>("altituderange_slider_inc_L");
            L_altituderange_inc_action.Action = Set_altitude_range_inc_L;
            L_altituderange_inc_action.Name = new StringBuilder("increase altitude range");
            L_altituderange_inc_action.Writer = L_altituderange.Writer;
            L_altituderange_inc_action.Icon = @"Textures\GUI\Icons\Actions\Increase.dds";
            L_altituderange_inc_action.Enabled = (b) => b.BlockDefinition.SubtypeId.Contains("HoverSuspension_L");
            MyAPIGateway.TerminalControls.AddAction<Sandbox.ModAPI.Ingame.IMyThrust>(L_altituderange_inc_action);

            var L_altituderange_dec_action = MyAPIGateway.TerminalControls.CreateAction<Sandbox.ModAPI.Ingame.IMyThrust>("altituderange_slider_dec_L");
            L_altituderange_dec_action.Action = Set_altitude_range_dec_L;
            L_altituderange_dec_action.Name = new StringBuilder("decrease altitude range");
            L_altituderange_dec_action.Writer = L_altituderange.Writer;
            L_altituderange_dec_action.Icon = @"Textures\GUI\Icons\Actions\Decrease.dds";
            L_altituderange_dec_action.Enabled = (b) => b.BlockDefinition.SubtypeId.Contains("HoverSuspension_L");
            MyAPIGateway.TerminalControls.AddAction<Sandbox.ModAPI.Ingame.IMyThrust>(L_altituderange_dec_action);
            //---------------------------       	

            // Altitude regulation distance slider control and action---------------------------
            var L_altituderegdist = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyThrust>("altituderegdist_slider_L");
            L_altituderegdist.Title = MyStringId.GetOrCompute("Altitude regulation distance");
            L_altituderegdist.Tooltip = MyStringId.GetOrCompute(altituderegulationdistance_slider_min * scalemulti + "m - " + altituderegulationdistance_slider_max * scalemulti + "m, altitude regulation distance (range of spring), low = hard, high = soft");
            L_altituderegdist.Writer = (b, t) => t.AppendFormat("{0:N1}", L_altituderegdist.Getter(b)).Append(" m");
            L_altituderegdist.SetLimits(altituderegulationdistance_slider_min * scalemulti, altituderegulationdistance_slider_max * scalemulti);
            L_altituderegdist.Getter = (b) => b.GameLogic.GetAs<HoverSuspensionComponent>()._heightRegulationRange;
            L_altituderegdist.Setter = Set_altituderegdist;
            L_altituderegdist.Enabled = (b) => b.BlockDefinition.SubtypeId.Contains("HoverSuspension_L");
            L_altituderegdist.Visible = (b) => b.BlockDefinition.SubtypeId.Contains("HoverSuspension_L");
            MyAPIGateway.TerminalControls.AddControl<IMyThrust>(L_altituderegdist);

            var L_altituderegdist_inc_action = MyAPIGateway.TerminalControls.CreateAction<Sandbox.ModAPI.Ingame.IMyThrust>("altituderegdist_slider_inc_L");
            L_altituderegdist_inc_action.Action = Set_altitude_regdist_inc_L;
            L_altituderegdist_inc_action.Name = new StringBuilder("increase altitude regulation distance");
            L_altituderegdist_inc_action.Writer = L_altituderegdist.Writer;
            L_altituderegdist_inc_action.Icon = @"Textures\GUI\Icons\Actions\Increase.dds";
            L_altituderegdist_inc_action.Enabled = (b) => b.BlockDefinition.SubtypeId.Contains("HoverSuspension_L");
            MyAPIGateway.TerminalControls.AddAction<Sandbox.ModAPI.Ingame.IMyThrust>(L_altituderegdist_inc_action);

            var L_altituderegdist_dec_action = MyAPIGateway.TerminalControls.CreateAction<Sandbox.ModAPI.Ingame.IMyThrust>("altituderegdist_slider_dec_L");
            L_altituderegdist_dec_action.Action = Set_altitude_regdist_dec_L;
            L_altituderegdist_dec_action.Name = new StringBuilder("decrease altitude regulation distance");
            L_altituderegdist_dec_action.Writer = L_altituderegdist.Writer;
            L_altituderegdist_dec_action.Icon = @"Textures\GUI\Icons\Actions\Decrease.dds";
            L_altituderegdist_dec_action.Enabled = (b) => b.BlockDefinition.SubtypeId.Contains("HoverSuspension_L");
            MyAPIGateway.TerminalControls.AddAction<Sandbox.ModAPI.Ingame.IMyThrust>(L_altituderegdist_dec_action);
        }

        public void Buttons_AllBlock()
        {
            // color 
            var color1 = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlColor, IMyThrust>("emissive color1");
            color1.Title = MyStringId.GetOrCompute("Color");
            color1.Tooltip = MyStringId.GetOrCompute("Emissive Color");
            color1.Getter = (b) => b.GameLogic.GetAs<HoverSuspensionComponent>().m_color1;
            color1.Setter = Set_Color1;
            color1.Enabled = (b) => b.BlockDefinition.SubtypeId.Contains("HoverSuspension");
            color1.Visible = (b) => b.BlockDefinition.SubtypeId.Contains("HoverSuspension");
            MyAPIGateway.TerminalControls.AddControl<IMyThrust>(color1);

            // debug checkbox control, no action, did not safe !! ---------------------------
            var debug_btn = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyThrust>("debug_checkbox");
            debug_btn.OnText = MyStringId.GetOrCompute("On");
            debug_btn.OffText = MyStringId.GetOrCompute("Off");
            debug_btn.Title = MyStringId.GetOrCompute("debug, show hit");
            debug_btn.Tooltip = MyStringId.GetOrCompute("show scanline if hit for debug (temp)");
            debug_btn.Getter = (b) => b.GameLogic.GetAs<HoverSuspensionComponent>()._enableDebug;
            debug_btn.Setter = (b, t) => b.GameLogic.GetAs<HoverSuspensionComponent>()._enableDebug = t;
            debug_btn.Enabled = (b) => b.BlockDefinition.SubtypeId.Contains("HoverSuspension");
            debug_btn.Visible = (b) => b.BlockDefinition.SubtypeId.Contains("HoverSuspension");
            MyAPIGateway.TerminalControls.AddControl<IMyThrust>(debug_btn);
        }

        public void Save_data(IMyTerminalBlock b)
        {
            if (b == null) return;

            var data = "dont_change_this_please  : |";
            data += "targetHeight:" + b.GameLogic.GetAs<HoverSuspensionComponent>()._targetHeight.ToString(CultureInfo.InvariantCulture.NumberFormat) + "|";
            data += "altituderange:" + b.GameLogic.GetAs<HoverSuspensionComponent>()._targetHeightRange.ToString(CultureInfo.InvariantCulture.NumberFormat) + "|";
            data += "altituderegdist:" + b.GameLogic.GetAs<HoverSuspensionComponent>()._heightRegulationRange.ToString(CultureInfo.InvariantCulture.NumberFormat) + "|";
            data += "color1:" + b.GameLogic.GetAs<HoverSuspensionComponent>().m_color1.R.ToString() + ":" + b.GameLogic.GetAs<HoverSuspensionComponent>().m_color1.G.ToString() + ":" + b.GameLogic.GetAs<HoverSuspensionComponent>().m_color1.B.ToString() + "|";
            b.CustomData = data;

            //send to other clients
            details.EntityId = b.EntityId;
            details.Details = data;
            MyAPIGateway.Multiplayer.SendMessageToOthers(HandlerId, ASCIIEncoding.ASCII.GetBytes(MyAPIGateway.Utilities.SerializeToXML(details)));
            //MyAPIGateway.Utilities.ShowMessage("HoverEngine", "Saved");
        }

        public void Load_data(IMyTerminalBlock b)
        {
            if (b == null) return;

            var data = "";
            data = b.CustomData;
            if (data != "")
            {
                //load data from customdata
                char[] x = { '|' };
                string[] datafull = data.Split(x);
                foreach (string s in datafull)
                {
                    char[] y = { ':' };
                    string[] datapart = s.Split(y);
                    if (datapart[0] == "targetHeight")
                    {
                        _targetHeight = float.Parse(datapart[1], CultureInfo.InvariantCulture.NumberFormat);
                        _smoothedTargetHeight = float.Parse(datapart[1], CultureInfo.InvariantCulture.NumberFormat); //only at load
                    }
                    if (datapart[0] == "altituderange") { _targetHeightRange = float.Parse(datapart[1], CultureInfo.InvariantCulture.NumberFormat); }
                    if (datapart[0] == "altituderegdist") { _heightRegulationRange = float.Parse(datapart[1], CultureInfo.InvariantCulture.NumberFormat); }
                    if (datapart[0] == "color1") { m_color1 = Conv_to_color(datapart); }
                }
            }
            else
            {
                //load default
                _targetHeight = altitudemin_slider_default;
                _smoothedTargetHeight = altitudemin_slider_default;  //only at load
                _targetHeightRange = altituderange_slider_default;
                _heightRegulationRange = altituderegulationdistance_slider_default;
                m_color1 = new Color(0, 255, 0, 255); // lighter green after Keens graphic overhaoul 2/2018

                Save_data(b);
            }
            //update emissive after first load
            ShowStateNextFrame = true;
        }

        public void Load_default(IMyTerminalBlock b)
        {
            if (b == null) return;

            if (b.BlockDefinition.SubtypeId.Contains("LargeBlock"))
            {
                altitudemin_slider_default = altitudemin_slider_default * scalemulti;
                altituderange_slider_default = altituderange_slider_default * scalemulti;
                altituderegulationdistance_slider_default = altituderegulationdistance_slider_default * scalemulti;
            }
            //MyAPIGateway.Utilities.ShowMessage("HoverEngine", "blocktype: "+b.BlockDefinition.SubtypeId);         	
        }

        public override void UpdateOnceBeforeFrame()
        {
            //Do init and control creation here
            if (!initterminals)
            {
                //after Init for all engines (only once)
                Buttons_SmallBlock();
                Buttons_LargeBlock();
                Buttons_AllBlock();
                initterminals = true;
                //MyAPIGateway.Utilities.ShowMessage("HoverEngine", "UpdateOnceBeforeFrame");
            }

            //registration for new messagehandler           
            if (_block == null) return;
            if (!HoverSuspensionCore.HoverEngines.ContainsKey(_block.EntityId))
            {
                HoverSuspensionCore.HoverEngines.Add(_block.EntityId, this);
            }
        }

        public void UpdateClients(string Details)
        {
            //MyAPIGateway.Utilities.ShowMessage("HE", "Message to update data, bytes:"+bytes.ToString());
            if (_block == null) return;

            try
            {
                if (MyAPIGateway.Session == null || _block == null)
                    return;

                if (Details != "")
                {
                    // copy from save (b = _block, ...)
                    char[] x = { '|' };
                    string[] datafull = Details.Split(x);
                    foreach (string s in datafull)
                    {
                        char[] y = { ':' };
                        string[] datapart = s.Split(y);
                        if (datapart[0] == "height_target_min") { _block.GameLogic.GetAs<HoverSuspensionComponent>()._targetHeight = float.Parse(datapart[1], CultureInfo.InvariantCulture.NumberFormat); }
                        if (datapart[0] == "altituderange") { _block.GameLogic.GetAs<HoverSuspensionComponent>()._targetHeightRange = float.Parse(datapart[1], CultureInfo.InvariantCulture.NumberFormat); }
                        if (datapart[0] == "altituderegdist") { _block.GameLogic.GetAs<HoverSuspensionComponent>()._heightRegulationRange = float.Parse(datapart[1], CultureInfo.InvariantCulture.NumberFormat); }
                        if (datapart[0] == "color1") { _block.GameLogic.GetAs<HoverSuspensionComponent>().m_color1 = Conv_to_color(datapart); }

                        ShowStateNextFrame = true;
                    }
                }
            }
            catch// (Exception ex)
            {
                //                MyAPIGateway.Utilities.ShowMessage("HoverEngine", "error: "+ex.ToString());
            }
        }

        Color Conv_to_color(string[] s)
        {
            try
            {
                int r = MathHelper.Clamp(Convert.ToInt32(s[1]), 0, 255);
                int g = MathHelper.Clamp(Convert.ToInt32(s[2]), 0, 255);
                int b = MathHelper.Clamp(Convert.ToInt32(s[3]), 0, 255);
                return new Color(r, g, b, 255);
            }
            catch
            {
                return Color.Blue;
            }
        }
    }
}
