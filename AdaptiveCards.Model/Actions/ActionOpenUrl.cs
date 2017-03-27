﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AdaptiveCards
{

    /// <summary>
    /// When ActionOpenUrl is invoked it will show the given url, either by launching it to an external web browser or showing in-situ with embedded web browser.
    /// </summary>
    public partial class ActionOpenUrl : ActionBase
    {
        public ActionOpenUrl()
        {
            this.Type = "Action.OpenUrl";
        }

        /// <summary>
        /// Url to open using default operating system browser
        /// </summary>
        [JsonRequired]
#if DESKTOP
        [XmlAttribute]
#endif
        public string Url { get; set; }
    }
}
