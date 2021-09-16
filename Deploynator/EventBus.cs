using System;

namespace Deploynator
{
    public class EventBus
    {
        public event EventHandler ReleaseTriggered;

        public virtual void OnReleaseTriggered()
        {
            var handler = ReleaseTriggered;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}