using System;
using BlazorCMS.Server.Conductors;
using Xunit.Sdk;

namespace BlazorCMS.Tests.Conductors
{
    public abstract class IAuthorizationConductorTest<T> : IDisposable
    {
        public abstract IAuthorizationConductor<T> Sut();

        private long? _currentUserId = null;

        public long CurrentUserId
        {
            get
            {
                if (!_currentUserId.HasValue)
                {
                    _currentUserId = new Random().Next();
                }

                return _currentUserId.Value;
            }
        }

        public void Dispose()
        {
            _currentUserId = null;
        }
    }
}
