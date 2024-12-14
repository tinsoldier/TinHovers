using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox.Common.ObjectBuilders;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.GameSystems;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRageMath;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using SpaceEngineers.Game.ModAPI;
using VRage;
using VRage.Utils;
using VRage.Sync;
using Sandbox.Game.World;
using VRageRender;

namespace TinHovers
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Thrust), false,
                                 "LargeGaalsienHoverSuspension_LargeBlock",
                                 "LargeGaalsienArrayHoverSuspension_LargeBlock",
                                 "HugeGaalsienHoverSuspension_LargeBlock"
                                )]
    public partial class HoverSuspensionComponent : MyGameLogicComponent
    {
        private bool _init = false;
        private bool _initShowstate = false;

        private IMyThrust _block;
        private MyObjectBuilder_EntityBase _objectBuilder;

        #region Private fields
        private float _averageSpeed;
        private float _targetHeight;
        private float _smoothedTargetHeight;
        private float _targetHeightRange;
        private float _recalculatedTargetHeight;
        private float _thrustMultiplier;
        private float _thrustMax;

        private bool _seesaw;
        private int _tickCounter = 0;
        private int _lastGroundRaycastAtTick = 0;

        private bool _collisionAlert;

        private float _lastGroundForce;
        private float _lastDragForce;

        private bool _applyForceToCenterOfMass; // Whether force should be applied directly at the grid's center of mass
        //private bool _pushOnlyMode;             // Restricts thrust application to a single direction (e.g., upward only)
        private bool _enableDebug;              // Controls whether debug visuals and messages are displayed
        //private bool _enableGravityAlignment;   // Indicates if the hover block should attempt to align the grid with local gravity
        private bool _showStateOnNextFrame;     // Flag to display state information in the following update cycle

        // Hover height and scaling parameters
        private float _heightRegulationRange;   // Defines the distance over which the hover block regulates lift force
        private float _gridSizeAdjustment;      // Represents a scale factor or offset related to the grid's size or configuration
        private float _scalingMultiplier;       // Modifier used to adjust rates or scaling of height and force calculations

        // Downward raycast measurements
        private float _distanceToSurface;   // Current measured distance from the hover block to a detected surface below
        private float _downwardDistanceChange;  // Change in the measured distance (dist1) since the last iteration
        private float _previousDownwardDistance;// Last recorded distance measurement to track distance changes over time

        // Forward (or secondary) raycast measurements
        private float _forwardDistance;         // Current measured distance for a forward or secondary raycast
        private float _forwardDistanceChange;   // Change in the measured secondary distance (dist2) since the last iteration
        private float _previousForwardDistance; // Last recorded secondary distance measurement to track changes over time

        // Raycast timing and frequency controls for downward raycasts
        private int _groundRaycastCounter;    // Counter for controlling how often the primary downward raycast is performed
        private int _groundRaycastInterval;   // Interval control for reducing the frequency of the primary raycast under certain conditions

        // Raycast timing and frequency controls for forward raycasts
        private int _forwardRaycastCounter;     // Counter for controlling how often the secondary (forward) raycast is performed
        private int _forwardRaycastInterval;    // Interval control for reducing the frequency of the secondary raycast under certain conditions
        #endregion

        //// Current chosen controller (could be changed at runtime or configured)
        //private IHoverThrustController _thrustController = new BangBangThrustController();
        private float _maxHoverRange = 30;

        private IHoverThrustController _thrustController = new PIDHoverThrustController(new PIDController(2, 0.007, 0.1), 5f, -1f);

        bool checkVelocityOnce = false;

        public void BlockUpdate()
        {
            // Retrieve the grid attached to the block
            var grid = _block.CubeGrid as MyCubeGrid;
            if (grid?.Physics == null || grid.Physics.IsStatic) return;

            Vector3D velocityVector = grid.Physics.GetVelocityAtPoint(grid.Physics.CenterOfMassWorld);
            float speed = (float)velocityVector.Length();

            // Running average speed
            _averageSpeed = (59 * _averageSpeed + speed) / 60;

            // Down direction of the block (assuming block.WorldMatrix.Down is correct for "forward" in context)
            //Vector3D downDirection = _block.WorldMatrix.Down;
            Vector3D downDirectionNormalized = grid.Physics.Gravity.Normalized(); // Use gravity direction as down direction instead of block down facing

            // Get the maximum thrust of the block
            _thrustMax = _block.MaxThrust;

            // Velocity component in the down direction
            //float downSpeed = (float)downDirection.Dot(velocityVector);

            // If completely still and target heights are zero, disable thruster
            if (_targetHeight == 0 && _smoothedTargetHeight == 0 && _averageSpeed < 0.04f && speed < 0.04f)
            {
                _thrustMultiplier = 0;
                _block.ThrustMultiplier = _thrustMultiplier;
                return;
            }

            // Limit extreme speeds
            if (speed > 100f)
            {
                velocityVector = velocityVector / speed * 100f;
                speed = 100f;
            }

            // If dampeners are disabled, also disable thruster
            //if (!grid.DampenersEnabled)
            //{
            //    _showStateOnNextFrame = true;
            //    _block.ThrustMultiplier = 0;
            //    return;
            //}

            if (grid.DampenersEnabled)
            {
                // Smooth out the min target height
                // In theory, this should also provide a soft liftoff when enabling dampeners
                AdjustSmoothMinTargetHeight();

                //TODO: Determine if this "height range" feature when moving is necessary
                _recalculatedTargetHeight = _smoothedTargetHeight + _targetHeightRange * speed / 100f; 
            }
            else
            {
                // WIP Soft Landing
                // If dampeners are off, reduce the target height to zero over time
                _smoothedTargetHeight -= 0.04f * (_scalingMultiplier + 1f);
                _recalculatedTargetHeight = _smoothedTargetHeight;
                if (_smoothedTargetHeight < 0)
                {
                    _smoothedTargetHeight = 0;
                    _recalculatedTargetHeight = 0;
                }
                if (_smoothedTargetHeight == 0)
                {
                    return; //cut out all thrust
                }
            }

            // Perform raycasting logic
            _tickCounter++;
            if (_seesaw && _groundRaycastInterval-- <= 0)
            {
                // Ground detection raycast
                _groundRaycastInterval = 0; // Reset and prevent negative values
                var deltaTime = (_tickCounter - _lastGroundRaycastAtTick) / 60f;

                bool validSurface;
                float distanceToSurface, groundRatio;
                var groundForce = CalculateGroundForce(grid, downDirectionNormalized, velocityVector, deltaTime, out validSurface, out distanceToSurface, out groundRatio);
                _lastGroundForce = groundForce;
                _lastGroundRaycastAtTick = _tickCounter;
                _block.ThrustMultiplier = groundRatio; // Limit lateral thrust based on ground ratio (far from ground, less lateral thrust)

                if (validSurface)
                {
                    _downwardDistanceChange = distanceToSurface - _previousDownwardDistance;
                    _previousDownwardDistance = distanceToSurface;

                    if (_downwardDistanceChange > 0)
                    {
                        //Moving away from surface
                        //_lastGroundForce = 0;
                        _groundRaycastInterval = 2; 
                    }


                    var gravityDotVelocity = Vector3D.Dot(grid.Physics.Gravity, velocityVector);
                    var speedInGravityDirection = gravityDotVelocity / grid.Physics.Gravity.Length();
                    if (distanceToSurface < speedInGravityDirection) //If gravity component of our velocity vector is greater than distance to surface, we're moving towards it too quickly, intervene.
                    {
                        //Simple heuristic to predict ground impacts
                        _collisionAlert = true;
                        _lastGroundForce *= (float)(speedInGravityDirection / distanceToSurface); // Scale force based on distance to impact
                    }

                    if (_averageSpeed < 0.04f && speed < 0.04f)
                    {
                        _groundRaycastInterval = 5;
                    }
                }
                else
                {
                    _distanceToSurface = _recalculatedTargetHeight;
                    _previousDownwardDistance = _recalculatedTargetHeight;

                    _groundRaycastInterval = (speed < 20.0f) ? 2 : 1;
                }
            }
            else if(!_seesaw && _forwardRaycastInterval-- <= 0)
            {
                //Collision detection in direction of current movement vector
                _forwardRaycastInterval = 0; // Reset and prevent negative values
                bool collisionAlert;
                float distanceToObstacle;
                var collisionForce = CalculateCollisionAvoidanceForce(grid, velocityVector, speed, out collisionAlert, out distanceToObstacle);
                if(collisionAlert)
                {
                    _lastGroundForce = Math.Max(_lastGroundForce, collisionForce);

                    _forwardDistanceChange = distanceToObstacle - _previousForwardDistance;
                    _previousForwardDistance = distanceToObstacle;

                    if (_forwardDistanceChange > 0) _forwardRaycastInterval = 1;
                }
                else
                {
                    if (_averageSpeed < 10f && speed < 10f) _forwardRaycastInterval = 5; // Slow down raycast frequency if we're moving slowly and there was no collision alert recently
                }
            }
            _seesaw = !_seesaw;

            // Determine drag force
            var dragForce = CalculateDrag(grid, _lastGroundForce); // Too much ground force will trigger drag effects, grid orientation matters
            _lastDragForce = dragForce;

            // Decide where to apply the lift force
            Vector3D liftForceApplicationPoint = _applyForceToCenterOfMass ? grid.Physics.CenterOfMassWorld : _block.GetPosition();
            Vector3D liftForce = -downDirectionNormalized * _lastGroundForce;

            // Apply forces to the grid
            ApplyGroundForce(grid, liftForce, liftForceApplicationPoint);
            ApplyDragForce(grid, _lastDragForce, velocityVector, speed, 0.2f);
        }

        //private void ApplyDragForce(MyCubeGrid grid, float dragForce, Vector3D velocityVector, float speed)
        //{
        //    if(speed < 0.01f) return; // Don't apply drag if speed is very low

        //    //TODO: iterate over all of the subgrids and apply drag force based on their proportional mass
        //    //For now, just do the one grid

        //    Vector3D torque = Vector3D.Zero;
        //    double maxAcceleration = 9.81 * .25; 
        //    double maxForce = grid.Mass * maxAcceleration;
        //    var dragForceVector = -Math.Min(maxForce, dragForce) * (velocityVector / speed); // Apply constant drag force in opposite direction of velocity

        //    grid.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_FORCE, dragForceVector, grid.Physics.CenterOfMassWorld, torque);

        //    if(_enableDebug)
        //    {
        //        var startPoint = _block.GetPosition(); //grid.Physics.CenterOfMassWorld;
        //        var endPoint = startPoint + dragForceVector;
        //        var maxForceVector = -maxForce * (velocityVector / speed);
        //        MyTransparentGeometry.AddLineBillboard(MyStringId.GetOrCompute("Square"),
        //            Color.White,
        //            startPoint, dragForceVector, 20, 0.1f);
        //        MyTransparentGeometry.AddPointBillboard(MyStringId.GetOrCompute("WhiteDot"), Color.Red, startPoint + maxForceVector, radius: 0.25f, angle: 0, blendType: MyBillboard.BlendTypeEnum.AdditiveTop);
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        private void ApplyDragForce(
            MyCubeGrid grid,
            float totalDragForce,
            Vector3D velocityVector,
            float speed,
            float verticalDragRatio)
        {
            if (speed < 0.01f) return; // Don't apply drag if speed is very low

            // Gravity direction
            Vector3D gravityDir = grid.Physics.Gravity.Normalized();

            // Separate velocity into horizontal and vertical components w.r.t. gravity
            double verticalSpeed = Vector3D.Dot(velocityVector, gravityDir);
            Vector3D verticalVelocity = gravityDir * verticalSpeed;

            Vector3D horizontalVelocity = velocityVector - verticalVelocity;
            double horizontalSpeed = horizontalVelocity.Length();

            // Compute how much drag to apply to horizontal vs vertical
            float horizontalDragRatio = 1f - verticalDragRatio;
            float horizontalDragForce = totalDragForce * horizontalDragRatio;
            float verticalDragForce = totalDragForce * verticalDragRatio;

            // Final drag force vector
            Vector3D dragForceVector = Vector3D.Zero;

            // 1) Horizontal drag
            if (horizontalSpeed > 0.01f && horizontalDragForce > 0.01f)
            {
                // Opposite the horizontal velocity
                Vector3D horizDragDir = -horizontalVelocity / horizontalSpeed;
                Vector3D horizDragVec = horizDragDir * horizontalDragForce;
                dragForceVector += horizDragVec;
            }

            // 2) Vertical drag
            if (Math.Abs(verticalSpeed) > 0.01f && verticalDragForce > 0.01f)
            {
                // Opposite the vertical velocity
                Vector3D vertDragDir = -verticalVelocity / Math.Abs(verticalSpeed);
                Vector3D vertDragVec = vertDragDir * verticalDragForce;
                dragForceVector += vertDragVec;
            }

            // Apply the combined drag force at the center of mass
            grid.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_FORCE, dragForceVector, grid.Physics.CenterOfMassWorld, Vector3D.Zero);

            // Debug visualization
            if (_enableDebug)
            {
                Vector3D startPoint = _block.GetPosition();
                Vector3D endPoint = startPoint + dragForceVector;

                // Draw line for total drag force
                //MyTransparentGeometry.AddLineBillboard(MyStringId.GetOrCompute("Square"),
                //    Color.White,
                //    startPoint, dragForceVector, 20, 0.1f);

                //// Optionally visualize the separate horizontal and vertical drag vectors
                //if (horizontalSpeed > 0.01f)
                //{
                //    Vector3D horizDebugVec = (-horizontalVelocity / horizontalSpeed) * horizontalDragForce;
                //    MyTransparentGeometry.AddLineBillboard(MyStringId.GetOrCompute("Square"),
                //        Color.Green,
                //        startPoint, horizDebugVec, 15, 0.1f);
                //}
                //if (Math.Abs(verticalSpeed) > 0.01f)
                //{
                //    Vector3D vertDebugVec = -(verticalVelocity / Math.Abs(verticalSpeed)) * verticalDragForce;
                //    MyTransparentGeometry.AddLineBillboard(MyStringId.GetOrCompute("Square"),
                //        Color.Red,
                //        startPoint, vertDebugVec, 15, 0.1f);
                //}
            }
        }


        private float CalculateDrag(MyCubeGrid grid, float groundForceNewtons)
        {
            // Dot product to determine alignment
            double alignment = Vector3D.Dot(_block.WorldMatrix.Down, grid.Physics.Gravity.Normalized());

            // Clamp alignment to avoid division by zero or huge spikes
            alignment = MathHelper.Clamp(alignment, 0.1, 1);

            // Now inflate the ground force for drag calculation based on alignment
            double penalty = 1.0 / (alignment * alignment);
            float effectiveGroundForceForDrag = (float)(groundForceNewtons * penalty);

            var blockMaxThrust = _block.MaxThrust;

            // If the ground force necessary to keep block aloft is higher than the block thrust, apply drag
            if (effectiveGroundForceForDrag > blockMaxThrust)
            {
                var dragForce = effectiveGroundForceForDrag - blockMaxThrust; //Math.Min(effectiveGroundForceForDrag, blockMaxThrust*1);// (float)(effectiveGroundForceForDrag * (effectiveGroundForceForDrag / (blockMaxThrust * 2.0)));
                dragForce = MathHelper.Clamp(dragForce, 0, blockMaxThrust * 2);
                if (_enableDebug)
                    MyTransparentGeometry.AddPointBillboard(MyStringId.GetOrCompute("WhiteDot"), Color.Purple, _block.GetPosition() - grid.Physics.Gravity.Normalized()*10, radius: effectiveGroundForceForDrag / blockMaxThrust, angle: 0, blendType: MyBillboard.BlendTypeEnum.AdditiveTop);

                return dragForce;
            }

            return 0f;
        }

        #region Duty cycle scratch
        /*
            private int _overThrustTicks = 0;
            private const int OverThrustThresholdTicks = 600; // e.g., 10 seconds at 60 ticks/sec

            ...

            if (groundForceNewtons > blockMaxThrust)
            {
                _overThrustTicks++;
                float ratio = (float)(groundForceNewtons / (blockMaxThrust * 2.0));

                // If significantly over (like 200%), apply drag immediately
                if (ratio >= 1.0f)
                {
                    return (float)Math.Pow(ratio, 2);
                }
                // Otherwise, only apply drag if we've been over the threshold for some time
                else if (_overThrustTicks > OverThrustThresholdTicks)
                {
                    return (float)Math.Pow(ratio, 2);
                }
            }
            else
            {
                // Not over 100% thrust, reset counter
                _overThrustTicks = 0;
            }

            return 0f;
         */
        #endregion

        /// <summary>
        /// Calculates thrust in newtons to apply to the grid to keep it at the target height IFF the hover block is above a valid surface.
        /// </summary>
        /// <returns>Thrust force that should be applied in newtons</returns>
        private float CalculateGroundForce(MyCubeGrid grid, Vector3D downDirectionNormalized, Vector3D velocityVector, float deltaTime, out bool succesfulRaycast, out float distanceToSurface, out float groundRatio)
        {
            succesfulRaycast = TryGroundRaycast(grid, downDirectionNormalized, out distanceToSurface, out groundRatio);
            if (!succesfulRaycast)
            {
                //Hit ourselves or some other invalid surface
                return 0f;
            }
            
            //Successfully hit a hoverable surface
            var changeInDownwardDistance = distanceToSurface - _previousDownwardDistance;
            _previousDownwardDistance = distanceToSurface; // Store the distance for next tick

            var speedInGravityDirection = (float)Vector3D.Dot(downDirectionNormalized, velocityVector);

            float heightError = _recalculatedTargetHeight - distanceToSurface;// + speedInGravityDirection - changeInDownwardDistance; 
            //float deltaError = distanceToSurface - _previousDownwardDistance; // Change in distance since last tick
            
            var forceMultiplier = _thrustController.CalculateThrustMultiplier(heightError, deltaTime); //TODO delta time makes sense for a PID, but we're applying forces every tick even if the controller is only running periodically?
            return _block.MaxThrust * forceMultiplier;
        }

        private float CalculateCollisionAvoidanceForce(MyCubeGrid grid, Vector3D velocity, float speed, out bool collisionAlert, out float distanceToObstacle)
        {
            collisionAlert = TryForwardRaycast(velocity, _recalculatedTargetHeight, _distanceToSurface, speed, out distanceToObstacle);
            if (!collisionAlert)
            {
                return 0f;
            }

            _previousForwardDistance = distanceToObstacle;

            // Calculate force multiplier based on distance to obstacle
            var forceMultiplier = (1f - (distanceToObstacle / speed));
            return _block.MaxThrust * forceMultiplier;
        }

        private bool TryGroundRaycast(MyCubeGrid grid, Vector3D downDirection, out float distanceToSurface, out float groundRatio)
        {
            // Start the ray just below the block to ensure a clear path
            Vector3D startPoint = _block.GetPosition() + downDirection * _gridSizeAdjustment;

            // Cast downward a fixed distance
            Vector3D endPoint = startPoint + downDirection * _maxHoverRange;

            IHitInfo hitInfo;
            if (MyAPIGateway.Physics.CastRay(startPoint, endPoint, out hitInfo))
            {
                var isValid = IsValidHitEntity(hitInfo);

                // Debug visualization optional
                if (!isValid && !MyAPIGateway.Utilities.IsDedicated && _enableDebug)
                {
                    MyTransparentGeometry.AddLineBillboard(MyStringId.GetOrCompute("Square"), Color.Red * 0.5f, startPoint, hitInfo.Position - startPoint, 1, 0.05f);
                }
                else if (_enableDebug)
                {
                    MyTransparentGeometry.AddLineBillboard(MyStringId.GetOrCompute("Square"), Color.Blue * 0.5f, startPoint, endPoint - startPoint, 1, 0.05f);
                }

                if (isValid)
                {
                    distanceToSurface = (float)(_block.GetPosition() - hitInfo.Position).Length();
                    if (distanceToSurface <= _maxHoverRange)
                    {
                        // From 0 to 30m, always full effectiveness
                        groundRatio = 1f;
                    }
                    else
                    {
                        // From 30m to 60m, linearly reduce effectiveness
                        // (distanceToSurface - _maxHoverRange) will range from 0 at 30m to 30 at 60m.
                        // Dividing by _maxHoverRange (30m) will yield a range 0 to 1, which we use to scale linearly.
                        groundRatio = 1f - MathHelper.Clamp((distanceToSurface - _maxHoverRange) / _maxHoverRange, 0f, 1f);
                    }

                    return true; 
                }
            }

            distanceToSurface = 0f;
            groundRatio = 0f;
            return false;
        }

        private bool TryForwardRaycast(Vector3D velocityVector, float targetHeight, float distanceToSurface, float speed, out float distanceToObstacle)
        {
            if (speed > 10.0f)
            {
                Vector3D startPoint = _block.GetPosition() + _block.WorldMatrix.Down * distanceToSurface / 2;
                //Vector3D endPoint = _block.GetPosition() + _block.WorldMatrix.Down * targetHeight + velocityVector;
                Vector3D endPoint = startPoint + velocityVector * 3;

                IHitInfo hitInfo;
                if (MyAPIGateway.Physics.CastRay(startPoint, endPoint, out hitInfo))
                {
                    var isValid = IsValidHitEntity(hitInfo);

                    if (_enableDebug)
                    {
                        MyTransparentGeometry.AddLineBillboard(MyStringId.GetOrCompute("Square"),
                            isValid ? Color.Red : Color.Blue * 0.5f,
                            startPoint, endPoint - startPoint, 1, 0.05f);
                    }

                    if(isValid)
                    {
                        distanceToObstacle = (float)(_block.GetPosition() - hitInfo.Position).Length();
                        return true;
                    }
                }
            }

            distanceToObstacle = 0;
            return false;
        }

        private bool IsValidHitEntity(IHitInfo hitInfo)
        {
            //TODO: Verify hit entity is voxels or a detached grid. But if it's a floating object, like a piece of stone, ignore it.

            // Check if hit entity is part of the same grid
            var topMostParent = hitInfo.HitEntity.GetTopMostParent() as IMyCubeGrid;
            if (topMostParent != null)
            {
                if (MyAPIGateway.GridGroups.HasConnection(topMostParent, _block.CubeGrid as IMyCubeGrid, GridLinkTypeEnum.Physical) ||
                    MyAPIGateway.GridGroups.HasConnection(topMostParent, _block.CubeGrid as IMyCubeGrid, GridLinkTypeEnum.NoContactDamage))
                {
                    return false;
                }
            }

            // Check if hit entity is a character
            if (hitInfo.HitEntity is IMyCharacter)
            {
                return false;
            }

            return true;
        }

        private void ApplyGroundForce(MyCubeGrid grid, Vector3D force, Vector3D forceApplicationPoint)
        {
            Vector3D torque = Vector3D.Zero;
            grid.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_FORCE, force, forceApplicationPoint, torque);
            //_block.ThrustMultiplier = _thrustMultiplier;
        }

        /// <summary>
        /// If min target height is changed, adjust the smooth target height to match it over time rather than instantly.
        /// </summary>
        private void AdjustSmoothMinTargetHeight()
        {
            if (_targetHeight > _smoothedTargetHeight)
            {
                _smoothedTargetHeight += 0.04f;
            }
            if (_targetHeight < _smoothedTargetHeight)
            {
                _smoothedTargetHeight -= 0.0075f * (_scalingMultiplier + 1f);
            }
            if (_smoothedTargetHeight < 0) _smoothedTargetHeight = 0;
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
        }

        // Gamelogic update (each frame after simulation)
        public override void UpdateAfterSimulation()
        {
            if (_block == null || _block.MarkedForClose || _block.Closed) return;

            if (!_init)
            {
                Init(null);
                return;
            }
            if (!_initShowstate)
            {
                ShowState();
                _initShowstate = true;
            }

            //fix for not changing color after welding
            if (ShowStateNextFrame)
            {
                ShowStateNextFrame = false;
                ShowState();
            }

            if (!_block.IsWorking) return; // no update if block down, off or damaged

            BlockUpdate();
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            //MyAPIGateway.Utilities.ShowMessage("HoverEngine", "Init in Behavior:");
            _block = Entity as IMyThrust;
            this._objectBuilder = objectBuilder;
            NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME | MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME;

            if (_block == null) return;

            _block.IsWorkingChanged += Block_IsWorkingChanged;
            _block.EnabledChanged += BlockOnEnabledChanged;
            _block.AppendingCustomInfo += CustomInfo; //11-2019

            if (MyAPIGateway.Session == null) return;

            _init = true;

            switch (_block.BlockDefinition.SubtypeId)
            {

                case "LargeGaalsienHoverSuspension_LargeBlock":
                    _gridSizeAdjustment = 1.3f;
                    break;

                case "HugeGaalsienHoverSuspension_LargeBlock":
                    _gridSizeAdjustment = 1.3f;
                    break;

                case "LargeGaalsienArrayHoverSuspension_LargeBlock":
                    _gridSizeAdjustment = 1.3f;
                    break;
            }

            Load_default(_block);
            Load_data(_block);

            //MyAPIGateway.Utilities.ShowMessage("HoverEngine", "init controls and first color");
        }

        private void BlockOnEnabledChanged(IMyTerminalBlock b)
        {
            ShowStateNextFrame = true;
        }

        private void Block_IsWorkingChanged(VRage.Game.ModAPI.IMyCubeBlock obj)
        {
            ShowStateNextFrame = true;
        }

        private void CustomInfo(IMyTerminalBlock block, StringBuilder info) //11-2019
        {
            try
            {
                var grid = block.CubeGrid as MyCubeGrid;
                info.Append('\n');
                if (!grid.DampenersEnabled)
                {
                    info.Append("Hover Engine is deactivated because Dampeners are not enabled.\nActivate dampeners to use Hover Engines.");
                }
            }
            catch (Exception e)
            {
                //na
            }
        }
    }
}
