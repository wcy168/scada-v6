﻿/*
 * Copyright 2021 Rapid Software LLC
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
 * Module   : PlgMain
 * Summary  : Represents a plugin logic
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2021
 * Modified : 2021
 */

using Microsoft.Extensions.DependencyInjection;
using Scada.Data.Entities;
using Scada.Lang;
using Scada.Web.Plugins.PlgMain.Code;
using Scada.Web.Services;
using Scada.Web.TreeView;
using Scada.Web.Users;
using System.Collections.Generic;
using System.IO;

namespace Scada.Web.Plugins.PlgMain
{
    /// <summary>
    /// Represents a plugin logic.
    /// <para>Представляет логику плагина.</para>
    /// </summary>
    public class PlgMainLogic : PluginLogic
    {
        private readonly PluginContext pluginContext;


        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public PlgMainLogic(IWebContext webContext)
            : base(webContext)
        {
            pluginContext = new PluginContext();
        }


        /// <summary>
        /// Gets the plugin code.
        /// </summary>
        public override string Code => "PlgMain";

        /// <summary>
        /// Gets the plugin features.
        /// </summary>
        public override PluginFeatures Features => new()
        {
            CommandScriptUrl = "~/plugins/Main/js/command.js",
            EventAckScriptUrl = "~/plugins/Main/js/event-ack.js",
        };

        /// <summary>
        /// Gets the view specifications.
        /// </summary>
        public override ICollection<ViewSpec> ViewSpecs => new ViewSpec[] { new TableViewSpec() };

        /// <summary>
        /// Gets the data window specifications.
        /// </summary>
        public override ICollection<DataWindowSpec> DataWindowSpecs => new DataWindowSpec[] { new EventWindowSpec() };


        /// <summary>
        /// Loads language dictionaries.
        /// </summary>
        public override void LoadDictionaries()
        {
            if (!Locale.LoadDictionaries(Path.Combine(AppDirs.PluginDir, "Main", "lang"),
                "PlgMain", out string errMsg))
            {
                Log.WriteError(errMsg);
            }
        }

        /// <summary>
        /// Loads configuration.
        /// </summary>
        public override void LoadConfig()
        {
            pluginContext.LoadOptions(WebContext.AppConfig.GetOptions("Main"));
        }

        /// <summary>
        /// Adds services to the DI container.
        /// </summary>
        public override void AddServices(IServiceCollection services)
        {
            services.AddSingleton(pluginContext);
        }

        /// <summary>
        /// Gets menu items available for the specified user.
        /// </summary>
        public override List<MenuItem> GetUserMenuItems(User user, UserRights userRights)
        {
            MenuItem reportsItem = MenuItem.FromKnownMenuItem(KnownMenuItem.Reports);
            reportsItem.Subitems.Add(new MenuItem { Text = "Data report", Url = "~/Main/DataRep", SortOrder = 1 });
            reportsItem.Subitems.Add(new MenuItem { Text = "Event report", Url = "~/Main/EventRep", SortOrder = 2 });
            return new List<MenuItem>() { reportsItem };
        }
    }
}