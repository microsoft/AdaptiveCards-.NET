﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace AdaptiveCards
{
    /// <summary>
    /// A set of columns (each column is a container of items)
    /// </summary>
    public partial class ColumnSet : CardElement
    {
        public ColumnSet()
        {
            this.Type = "ColumnSet";
        }

        /// <summary>
        /// Columns that are part of this group
        /// </summary>
        [JsonRequired]
#if DESKTOP
        [XmlElement(ElementName = "Column", Type = typeof(Column))]
#endif
        public List<Column> Columns { get; set; } = new List<Column>();
    }
}
