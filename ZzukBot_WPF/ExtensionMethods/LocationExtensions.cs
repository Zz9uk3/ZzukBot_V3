﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using SharpDX;
using ZzukBot.Game.Statics;
using ZzukBot.Objects;
using Matrix = SharpDX.Matrix;
using ObjectManager = ZzukBot.Game.Statics.ObjectManager;

namespace ZzukBot.ExtensionMethods
{
    /// <summary>
    /// Contains extension methods for ingame positions
    /// </summary>
    public static class LocationExtensions
    {
        /// <summary>
        /// Transforms the given world coordinates to screen coordinates
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        public static Vector2 ToScreenCoordinates(this Location loc)
        {
            return DirectX.Instance.World2Screen(loc);
        }

        /// <summary>
        /// Transforms the relative position towards the given transport
        /// </summary>
        /// <param name="loc">The position</param>
        /// <param name="transport">The transport</param>
        /// <returns>The absolute position</returns>
        public static Location GetAbsoluteFromRelativeTransportPosition(this Location loc, WoWGameObject transport)
        {
            if (transport == null) return null;
            var matrix = transport.TransportMatrix;
            var x = matrix.M31 * loc.Z +
                        matrix.M11 * loc.X +
                        matrix.M21 * loc.Y +
                        matrix.M41;

            var y = matrix.M32 * loc.Z +
                    matrix.M12 * loc.X +
                    matrix.M22 * loc.Y +
                    matrix.M42;

            var z = matrix.M33 * loc.Z +
                    matrix.M13 * loc.X +
                    matrix.M23 * loc.Y +
                    matrix.M43;
            return new Location(x, y, z);
        }


        /// <summary>
        /// Transforms the absolute position to a position relative to transport object the player is on
        /// </summary>
        /// <param name="loc">The location</param>
        /// <returns>The location relative to the transport the player is on</returns>
        public static Location GetRelativeToPlayerTransport(this Location loc)
        {
            var player = ObjectManager.Instance.Player;
            var transport = player?.CurrentTransport;
            if (transport == null) return null;
            Matrix inverse;
            var transportMatrix = transport.TransportMatrix;
            Matrix.Invert(ref transportMatrix, out inverse);
            var x = inverse.M31 * loc.Z +
                        inverse.M11 * loc.X +
                        inverse.M21 * loc.Y +
                        inverse.M41;

            var y = inverse.M32 * loc.Z +
                    inverse.M12 * loc.X +
                    inverse.M22 * loc.Y +
                    inverse.M42;

            var z = inverse.M33 * loc.Z +
                    inverse.M13 * loc.X +
                    inverse.M23 * loc.Y +
                    inverse.M43;
            return new Location(x, y, z);
        }


        /// <summary>
        /// Transforms the relative position towards the transport the player is on to an absolute position
        /// </summary>
        /// <param name="loc">The position</param>
        /// <returns>The absolute position</returns>
        public static Location GetAbsoluteFromRelativeTransportPosition(this Location loc)
        {
            var player = ObjectManager.Instance.Player;
            var transport = player?.CurrentTransport;
            if (transport == null) return null;
            var matrix = transport.TransportMatrix;
            var x = matrix.M31 * loc.Z +
                        matrix.M11 * loc.X +
                        matrix.M21 * loc.Y +
                        matrix.M41;

            var y = matrix.M32 * loc.Z +
                    matrix.M12 * loc.X +
                    matrix.M22 * loc.Y +
                    matrix.M42;

            var z = matrix.M33 * loc.Z +
                    matrix.M13 * loc.X +
                    matrix.M23 * loc.Y +
                    matrix.M43;
            return new Location(x, y, z);

            
        }
    }
}
