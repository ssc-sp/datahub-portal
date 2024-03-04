using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Application.Exceptions
{
    public class OpenDataPublishingException : Exception
    {
        public OpenDataPublishingException() : base() { }
        public OpenDataPublishingException(string? message) : base(message) { }
        public OpenDataPublishingException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
