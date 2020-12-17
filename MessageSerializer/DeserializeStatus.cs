using System;
using System.Collections.Generic;

namespace MessageSerializer
{
    public class DeserializeStatus
    {
        public DeserializeStatus()
        {
            Exception = null;
            Messages = new List<string>();
        }

        public override string ToString()
        {
            if (Results)
            {
                return "No problems encountered";
            }

            return Exception == null ? "" : Exception.ToString();
        }

        public List<string> Messages { get; set; }

        public bool Results
        {
            get { return Exception == null && Messages.Count == 0; }
        }

        public Exception Exception { get; set; }
    }
}
