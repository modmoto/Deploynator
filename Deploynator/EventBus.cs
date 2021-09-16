using System;

namespace Deploynator
{
    public class EventBus
    {
        public event EventHandler ReleaseButtonTriggered;
        public event EventHandler ReleaseButtonReleased;

        public virtual void OnReleaseButtonTriggered()
        {
            var handler = ReleaseButtonTriggered;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnReleaseButtonReleased()
        {
            var handler = ReleaseButtonTriggered;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}