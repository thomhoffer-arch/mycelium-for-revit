using System;
using System.Collections.Concurrent;
using System.Threading;
using Autodesk.Revit.UI;

namespace Loam.Revit.Connector.RevitBridge
{
    /// Marshals work onto Revit's UI thread via ExternalEvent. The Revit API is
    /// not thread-safe; tool calls arrive on HttpListener worker threads and
    /// must be funneled through here before touching anything in Autodesk.Revit.DB.
    public sealed class RevitContext
    {
        private readonly Handler _handler = new();
        private readonly ExternalEvent _event;
        private UIApplication _uiApp;

        public RevitContext(UIControlledApplication app)
        {
            _event = ExternalEvent.Create(_handler);
            app.ControlledApplication.ApplicationInitialized += (s, _) =>
            {
                _uiApp = new UIApplication((Autodesk.Revit.ApplicationServices.Application)s);
            };
        }

        public T Run<T>(Func<UIApplication, T> work, int timeoutMs = 30_000)
        {
            T result = default!;
            Exception err = null;
            using var done = new ManualResetEventSlim(false);

            _handler.Enqueue(() =>
            {
                try
                {
                    if (_uiApp is null) throw new InvalidOperationException("Revit UIApplication not yet available.");
                    result = work(_uiApp);
                }
                catch (Exception ex) { err = ex; }
                finally { done.Set(); }
            });
            _event.Raise();

            if (!done.Wait(timeoutMs))
                throw new TimeoutException("Revit UI thread did not respond in time.");
            if (err is not null) throw err;
            return result;
        }

        private sealed class Handler : IExternalEventHandler
        {
            private readonly ConcurrentQueue<Action> _q = new();
            public void Enqueue(Action a) => _q.Enqueue(a);
            public void Execute(UIApplication app)
            {
                while (_q.TryDequeue(out var a)) a();
            }
            public string GetName() => "LoamRevitConnector.RevitContext";
        }
    }
}
