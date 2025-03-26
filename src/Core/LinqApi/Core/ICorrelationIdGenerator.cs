﻿namespace LinqApi.Core
{
    /// <summary>
    /// Interface for generating correlation IDs.
    /// </summary>
    public interface ICorrelationIdGenerator
    {
        CorrelationId Generate(byte environment, byte sourceType);
    }


}

