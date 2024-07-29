using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPInfoApp.Business.Tests
{
    /// <summary>
    /// Use this to avoid  mispelling the method name (SendAsync) safer for compile time
    /// </summary>
    public abstract class FakeableHttpMessageHandler : HttpMessageHandler
    {
        
        public abstract Task<HttpResponseMessage> FakeSendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken);

        // sealed so FakeItEasy won't intercept calls to this method
        protected sealed override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            => this.FakeSendAsync(request, cancellationToken);
    }
}
