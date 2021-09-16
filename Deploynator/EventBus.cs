using System;
using System.Collections.Generic;
using DTOs;

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
        public event EventHandler DeselectedDeloyment;

        public virtual void OnReleaseButtonTriggered()
        {
            var handler = ReleaseButtonTriggered;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnDeploymentsLoaded(IEnumerable<ReleaseDefinition> releaseDefinitions)
        {
            var handler = DeploymentsLoaded;
            handler?.Invoke(this, new DeployArgs(releaseDefinitions));
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

        public virtual void OnPreselectedDeloyment(ReleaseDefinition releaseDefinition)
        {
            var handler = PreselectedDeloyment;
            handler?.Invoke(this, new SelectReleaseDefinitionArgs(releaseDefinition));
        }

        public virtual void OnSelectedDeloyment(ReleaseDefinition releaseDefinition)
        {
            var handler = SelectedDeloyment;
            handler?.Invoke(this, new SelectReleaseDefinitionArgs(releaseDefinition));
        }

        public void OnReleasesTriggered(List<ReleaseDefinition> releaseDefinitions)
        {
            var handler = ReleasesTriggered;
            handler?.Invoke(this, new DeployArgs(releaseDefinitions));
        }

        public void OnReleaseCountdownFinished(IEnumerable<ReleaseDefinition> selectedDeloyments)
        {
            var handler = ReleaseCountdownFinished;
            handler?.Invoke(this, new DeployArgs(selectedDeloyments));
        }

        public void OnDeselectedDeloyment(ReleaseDefinition releaseDefinition)
        {
            var handler = DeselectedDeloyment;
            handler?.Invoke(this, new SelectReleaseDefinitionArgs(releaseDefinition));
        }
    }

    public class DeployArgs : EventArgs
    {
        public IEnumerable<ReleaseDefinition> SelectedDeloyments { get; }

        public DeployArgs(IEnumerable<ReleaseDefinition> selectedDeloyments)
        {
            SelectedDeloyments = selectedDeloyments;
        }
    }

    public class SelectReleaseDefinitionArgs : EventArgs
    {
        public ReleaseDefinition ReleaseDefinition { get; }

        public SelectReleaseDefinitionArgs(ReleaseDefinition releaseDefinition)
        {
            ReleaseDefinition = releaseDefinition;
        }
    }
}