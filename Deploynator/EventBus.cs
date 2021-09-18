using System;
using System.Collections.Generic;
using DevLab.AzureAdapter.DTOs;

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
        public event EventHandler ReleasesSucceeded;
        public event EventHandler PreselectedDeloyment;
        public event EventHandler SelectedDeloyment;
        public event EventHandler ReleasesTriggered;
        public event EventHandler ReleaseCountdownFinished;
        public event EventHandler DeselectedDeloyment;
        public event EventHandler ReleaseLoaded;
        public event EventHandler LanguageChanged;
        public event EventHandler WaitingSequenceStarted;
        public event EventHandler FoundJoke;
        public event EventHandler JokeFinished;

        public virtual void OnLanguageChanged(string newLanguage)
        {
            var handler = LanguageChanged;
            handler?.Invoke(this, new LanguageArgs(newLanguage));
        }

        public virtual void OnReleaseButtonTriggered()
        {
            var handler = ReleaseButtonTriggered;
            handler?.Invoke(this, EventArgs.Empty);
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

        public virtual void OnReleasesSucceeded(IEnumerable<DeploymentResult> deploymentResults)
        {
            var handler = ReleasesSucceeded;
            handler?.Invoke(this, new DeploymentResultsArgs(deploymentResults));
        }

        public virtual void OnPreselectedDeloyment(ReleaseDefinition releaseDefinition, int index, bool isSelected)
        {
            var handler = PreselectedDeloyment;
            handler?.Invoke(this, new SelectReleaseDefinitionArgs(releaseDefinition, index, isSelected));
        }

        public virtual void OnSelectedDeloyment(ReleaseDefinition releaseDefinition, int index)
        {
            var handler = SelectedDeloyment;
            handler?.Invoke(this, new SelectReleaseDefinitionArgs(releaseDefinition, index, true));
        }

        public void OnReleasesTriggered(List<ReleaseDefinition> releaseDefinitions)
        {
            var handler = ReleasesTriggered;
            handler?.Invoke(this, new DeployArgs(releaseDefinitions));
        }

        public void OnReleaseCountdownFinished(List<ReleaseDefinition> selectedDeloyments)
        {
            var handler = ReleaseCountdownFinished;
            handler?.Invoke(this, new DeployArgs(selectedDeloyments));
        }

        public void OnDeselectedDeloyment(ReleaseDefinition releaseDefinition, int index)
        {
            var handler = DeselectedDeloyment;
            handler?.Invoke(this, new SelectReleaseDefinitionArgs(releaseDefinition, index, false));
        }

        public void OnReleaseLoaded(List<ReleaseDefinition> releaseDefinitions)
        {
            var handler = ReleaseLoaded;
            handler?.Invoke(this, new DeployArgs(releaseDefinitions));
        }

        public void OnWaitingSequenceStarted()
        {
            var handler = WaitingSequenceStarted;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public void OnFoundJoke(string joke)
        {
            var handler = FoundJoke;
            handler?.Invoke(this, new JokeArgs(joke));
        }

        public void OnJokeFinished()
        {
            var handler = JokeFinished;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }

    public class JokeArgs : EventArgs
    {
        public string Joke { get; }

        public JokeArgs(string joke)
        {
            Joke = joke;
        }
    }

    public class LanguageArgs : EventArgs
    {
        public string NewLanguage { get; }

        public LanguageArgs(string newLanguage)
        {
            NewLanguage = newLanguage;
        }
    }

    public class DeployArgs : EventArgs
    {
        public List<ReleaseDefinition> SelectedDeloyments { get; }

        public DeployArgs(List<ReleaseDefinition> selectedDeloyments)
        {
            SelectedDeloyments = selectedDeloyments;
        }
    }

    public class SelectReleaseDefinitionArgs : EventArgs
    {
        public ReleaseDefinition ReleaseDefinition { get; }
        public int Index { get; }
        public bool IsSelected { get; }

        public SelectReleaseDefinitionArgs(ReleaseDefinition releaseDefinition, int index, bool isSelected)
        {
            ReleaseDefinition = releaseDefinition;
            Index = index;
            IsSelected = isSelected;
        }
    }

    public class DeploymentResultsArgs : EventArgs
    {
        public IEnumerable<DeploymentResult> DeploymentResults { get; }

        public DeploymentResultsArgs(IEnumerable<DeploymentResult> deploymentResults)
        {
            DeploymentResults = deploymentResults;
        }
    }
}