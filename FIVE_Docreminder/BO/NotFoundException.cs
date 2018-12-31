using System;
using System.Runtime.Serialization;

namespace docreminder.BO
{

    /// <summary>
    /// The Class NotFoundException is used when a getter method does not find
    /// a value or an object for the specified parameter.
    /// </summary>
    public class NotFoundException: Exception {

	    /// <summary>
        /// Instantiates a new not found exception.
	    /// </summary>
	    public NotFoundException() : base() { }

        /// <summary>
        /// Instantiates a new not found exception.
        /// </summary>
        /// <param name="message">the message</param>
        public NotFoundException(string message) : base(message) { }

        /// <summary>
        /// Instantiates a new not found exception.
        /// </summary>
        /// <param name="info">the info</param>
        /// <param name="context">the context</param>
        public NotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	
	    /// <summary>
        /// Instantiates a new not found exception.
	    /// </summary>
	    /// <param name="message">the message</param>
	    /// <param name="innerException">the inner exception</param>
        public NotFoundException(string message, Exception innerException) : base(message, innerException) { }
	
    }

}
