﻿using System.Collections.Generic;
using softwrench.sW4.Shared2.Util;

namespace softwrench.sw4.Shared2.Metadata.Applications.Command {
    public class ResourceCommand : ICommandDisplayable {

        public string Id { get; set; }
        public string Path { get; set; }
        public string Position { get; set; }
        public string Role { get; set; }

        private readonly IDictionary<string, string> _parameters;

        public IDictionary<string, string> Parameters {
            get { return _parameters; }
        }

        public string OriginalParameters { get; set; }

        public string Type { get { return typeof(ResourceCommand).Name; } }

        public ResourceCommand(string id, string path, string role,string position,string parameters) {
            Id = id;
            Path = path;
            Role = role;
            Position = position;
            OriginalParameters = parameters;
            _parameters = PropertyUtil.ConvertToDictionary(parameters);
        }



    }
}
