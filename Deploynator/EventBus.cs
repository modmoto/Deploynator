using System;
using System.Collections.Generic;

namespace Deploynator
{
    public class EventBus
    {
        public event EventHandler ReleaseButtonTriggered;
        public event EventHandler UpButtonTriggered;
        public event EventHandler DownButtonTriggered;
        public event EventHandler SelectButtonTriggered;
        public event EventHandler DeselectButtonTriggered;
        public event EventHandler ReleaseButtonReleased;
        public event EventHandler ServiceStarted;
        public event EventHandler ReleaseSuceeded;
        public event EventHandler ReleaseFailed;
        public event EventHandler PreselectedDeloyment;
        public event EventHandler DeploymentsLoaded;
        public event EventHandler SelectedDeloyment;
        public event EventHandler ReleasesTriggered;
        public event EventHandler ReleaseCountdownFinished;

        public virtual void OnReleaseButtonTriggered()
        {
            var handler = ReleaseButtonTriggered;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnDeploymentsLoaded(string name)
        {
            var handler = DeploymentsLoaded;
            handler?.Invoke(this, new SelectArgs(name));
        }

        public virtual void OnUpButtonTriggered()
        {
            var handler = UpButtonTriggered;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnDownButtonTriggered()
        {
            var handler = DownButtonTriggered;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnSelectButtonTriggered()
        {
            var handler = SelectButtonTriggered;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnDeselectButtonTriggered()
        {
            var handler = DeselectButtonTriggered;
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

        public virtual void OnPreselectedDeloyment(string name)
        {
            var handler = PreselectedDeloyment;
            handler?.Invoke(this, new SelectArgs(name));
        }

        public virtual void OnSelectedDeloyment(string name)
        {
            var handler = SelectedDeloyment;
            handler?.Invoke(this, new SelectArgs(name));
        }

        public void OnReleasesTriggered(List<string> selectedDeloyments)
        {
            var handler = ReleasesTriggered;
            handler?.Invoke(this, new DeployArgs(selectedDeloyments));
        }

        public void OnReleaseCountdownFinished(List<string> selectedDeloyments)
        {
            var handler = ReleaseCountdownFinished;
            handler?.Invoke(this, new DeployArgs(selectedDeloyments));
        }
    }

    public class DeployArgs : EventArgs
    {
        public List<string> SelectedDeloyments { get; }

        public DeployArgs(List<string> selectedDeloyments)
        {
            SelectedDeloyments = selectedDeloyments;
        }
    }

    public class SelectArgs : EventArgs
    {
        public string Name { get; }

        public SelectArgs(string name)
        {
            Name = name;
        }
    }
}