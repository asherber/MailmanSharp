/**
 * Copyright 2014-2018 Aaron Sherber
 * 
 * This file is part of MailmanSharp.
 *
 * MailmanSharp is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * MailmanSharp is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with MailmanSharp. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MailmanSharp
{
    [DebuggerDisplay("Name = {Name}")]
    public class Topic
    {
        public string Name { get; set; }
        /// <summary>
        /// Topic keywords, one per line, to match against each message.
        /// </summary>
        public List<string> Regexes { get; set; }
        public string Description { get; set; }

        public Topic() { }

        public Topic(string name, List<string> regexes, string description)
        {
            this.Name = name;
            this.Regexes = regexes;
            this.Description = description;
        }
    }
}
