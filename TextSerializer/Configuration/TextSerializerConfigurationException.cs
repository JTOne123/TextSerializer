﻿using System;

namespace TheCodingMonkey.Serialization.Configuration
{
    [Serializable]
    public class TextSerializationConfigurationException : Exception
    {
        public TextSerializationConfigurationException()
        { }

        public TextSerializationConfigurationException(string message) 
        : base(message)
        { }

        public TextSerializationConfigurationException(string message, Exception innerException) 
        : base(message, innerException)
        { }
    }
}