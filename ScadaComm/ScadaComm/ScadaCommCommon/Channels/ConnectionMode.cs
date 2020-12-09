﻿/*
 * Copyright 2020 Mikhail Shiryaev
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * 
 * Product  : Rapid SCADA
 * Module   : ScadaCommCommon
 * Summary  : Specifies the connection modes
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2020
 * Modified : 2020
 */

namespace Scada.Comm.Channels
{
    /// <summary>
    /// Specifies the connection modes.
    /// <para>Задает режимы работы соединения.</para>
    /// </summary>
    public enum ConnectionMode
    {
        /// <summary>
        /// One connection per device.
        /// </summary>
        Individual,

        /// <summary>
        /// Connection is shared for all devices on a communication line.
        /// </summary>
        Shared
    }
}
