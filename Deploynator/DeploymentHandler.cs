using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DevLab.AzureAdapter;
using DevLab.AzureAdapter.DTOs;

namespace Deploynator
{
    public class DeploymentHandler
    {
        private readonly EventBus _eventBus;
        private readonly IAzureReleaseRepository _azureReleaseRepository;
        private readonly RandomFactsApiAdapter _randomFactsApiAdapter;
        public List<ReleaseDefinition> ReleaseDefinitions = new();
        public List<ReleaseDefinition> SelectedReleaseDefinitions = new();
        private int _index;
        private bool _isTellingJoke;

        public int CurrentIndex => _index;

        private readonly string[] LAUGH_VARIATIONS = {"HA HA HA", "HaHaHaHaHa HAHAHAHAH", "HaHa", "HEHEHEHEHE VERY FUNNY", "ROFL", "LOL", "I don't get it!"};

        public DeploymentHandler(IAzureReleaseRepository azureReleaseRepository, EventBus eventBus)
        {
            _azureReleaseRepository = azureReleaseRepository;
            _eventBus = eventBus;
            _randomFactsApiAdapter = new RandomFactsApiAdapter();

            _eventBus.ReleaseCountdownFinished += TriggerReleasesAsync;
            _eventBus.ReleaseButtonTriggered += StarteCountdownSequence;

            _eventBus.UpButtonTriggered += (_, _) => MoveU();
            _eventBus.DownButtonTriggered += (_, _) => MoveDown();
            _eventBus.SelectButtonTriggered += (_, _) => Select();
            _eventBus.DeselectButtonTriggered += (_, _) => Deselect();

            _eventBus.ServiceStarted += LoadReleases;
            _eventBus.ReleasesSucceeded += LoadReleases;
            _eventBus.JokeFinished += (_, _) => _isTellingJoke = false;
        }

        private void StarteCountdownSequence(object sender, EventArgs e)
        {
            if (SelectedReleaseDefinitions.Count == 0)
            {
                _eventBus.OnErrorNoReleasesSelected();
            }
            else
            {
                _eventBus.OnReleasesTriggered(SelectedReleaseDefinitions);
            }
        }

        public async void LoadReleases(object sender, EventArgs eventArgs)
        {
            ReleaseDefinitions = await _azureReleaseRepository.GetReleaseDefinitionsAsync() ?? new List<ReleaseDefinition>();
            SelectedReleaseDefinitions = new List<ReleaseDefinition>();
            _eventBus.OnReleaseLoaded(ReleaseDefinitions);
        }

        private async void TriggerReleasesAsync(object sender, EventArgs deployArgs)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            StartWaitingSequenceInBackground(cancellationTokenSource.Token);
            
            var results = await _azureReleaseRepository.DeployReleasesToProdAsync(((DeployArgs) deployArgs).SelectedDeloyments);

            cancellationTokenSource.Cancel();
            _eventBus.OnReleasesSucceeded(results);
        }

        private void StartWaitingSequenceInBackground(CancellationToken cancellationToken)
        {
            _eventBus.OnWaitingSequenceStarted();

            Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (!_isTellingJoke)
                    {
                        var waitingText = await _randomFactsApiAdapter.GetRandomFactAsync();
                        var randoMizer = new Random();
                        var randomJokeIndex = randoMizer.Next(0, 7);
                        _eventBus.OnFoundJoke($"{waitingText}. {LAUGH_VARIATIONS[randomJokeIndex]}");
                        _isTellingJoke = true;
                    }

                    await Task.Delay(2000, cancellationToken);
                }
            }, cancellationToken);
        }

        public string CurrentSelection => ReleaseDefinitions.Count > _index ? ReleaseDefinitions[_index].Name : "No deployment available";

        public void Select()
        {
            if (ReleaseDefinitions.Count <= _index) return;

            SelectedReleaseDefinitions.Add(ReleaseDefinitions[_index]);
            SelectedReleaseDefinitions = SelectedReleaseDefinitions.Distinct().ToList();
            _eventBus.OnSelectedDeloyment(ReleaseDefinitions[_index], _index + 1);
        }

        public void MoveDown()
        {
            if (_index <= 0) _index = ReleaseDefinitions.Count;
            _index--;
            _eventBus.OnPreselectedDeloyment(ReleaseDefinitions[_index], _index + 1, IsSelected());
        }

        private bool IsSelected()
        {
            return SelectedReleaseDefinitions.Any(s => s.Id == ReleaseDefinitions[_index].Id);
        }

        public void MoveU()
        {
            if (_index >= ReleaseDefinitions.Count - 1) _index = -1;
            _index++;
            _eventBus.OnPreselectedDeloyment(ReleaseDefinitions[_index], _index + 1, IsSelected());
        }

        public void Deselect()
        {
            SelectedReleaseDefinitions = SelectedReleaseDefinitions.Where(d => d.Id != ReleaseDefinitions[_index].Id).ToList();
            _eventBus.OnDeselectedDeloyment(ReleaseDefinitions[_index], _index + 1);
        }

    }
}