using System;

namespace Sdk.Runtime.JsonSerializable
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public  class JsonExcludeAttribute : Attribute
    {
    }
}