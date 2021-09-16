using System;

namespace Deploynator
{
    public class EventBus
    {
        public event EventHandler ReleaseButtonTriggered;
        public event EventHandler ReleaseButtonReleased;
        public event EventHandler ServiceStarted;
        public event EventHandler ReleaseSuceeded;
        public event EventHandler ReleaseFailed;

        public virtual void OnReleaseButtonTriggered()
        {
            var handler = ReleaseButtonTriggered;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnReleaseButtonReleased()
        {
            var handler = ReleaseButtonReleased;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnServiceStarted()
        {
            var handler = ServiceStarted;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnReleaseSuceeded()
        {
            var handler = ReleaseSuceeded;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnReleaseFailed()
        {
            var handler = ReleaseFailed;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}