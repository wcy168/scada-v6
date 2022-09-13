﻿// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Scada.Lang;
using System.Reflection;

namespace Scada.Comm.Drivers.DrvDsOpcUaServer
{
    /// <summary>
    /// The class provides helper methods for the driver.
    /// <para>Класс, предоставляющий вспомогательные методы для драйвера.</para>
    /// </summary>
    public static class DriverUtils
    {
        /// <summary>
        /// The driver code.
        /// </summary>
        public const string DriverCode = "DrvDsOpcUaServer";
        /// <summary>
        /// The default filename of the OPC UA server configuration.
        /// </summary>
        public const string DefaultConfigFileName = DriverCode + ".xml";

        /// <summary>
        /// Gets the resource stream that contains the default OPC configuration.
        /// </summary>
        public static Stream GetConfigResourceStream(bool windows)
        {
            string suffix = windows ? "Win" : "Linux";
            string resourceName = $"Scada.Comm.Drivers.DrvDsOpcUaServer.Config.DrvDsOpcUaServer.{suffix}.xml";
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName) ??
                throw new ScadaException(string.Format(Locale.IsRussian ?
                    "Ресурс {0} не найден." :
                    "Resource {0} not found.", resourceName));
        }
    }
}
