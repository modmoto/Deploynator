using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevLab.AzureAdapter;
using DevLab.AzureAdapter.DTOs;

namespace Deploynator
{
    public class DeploymentHandler
    {
        private readonly IAzureReleaseRepository _azureReleaseRepository;
        private readonly EventBus _eventBus;
        public List<ReleaseDefinition> ReleaseDefinitions = new();
        public List<ReleaseDefinition> SelectedReleaseDefinitions = new();
        private int _index;

        public DeploymentHandler(IAzureReleaseRepository azureReleaseRepository, EventBus eventBus)
        {
            _azureReleaseRepository = azureReleaseRepository;
            _eventBus = eventBus;

            _eventBus.ReleaseCountdownFinished += (_, args) => TriggerReleasesAsync((DeployArgs) args);
            _eventBus.ReleaseButtonTriggered += (_, _) => _eventBus.OnReleasesTriggered(SelectedReleaseDefinitions);

            _eventBus.UpButtonTriggered += (_, _) => MoveU();
            _eventBus.DownButtonTriggered += (_, _) => MoveDown();
            _eventBus.SelectButtonTriggered += (_, _) => Select();
            _eventBus.DeselectButtonTriggered += (_, _) => Deselect();

            _eventBus.ServiceStarted += (_, _) => LoadReleases();
            _eventBus.ReleaseSucceeded += (_, _) => SelectedReleaseDefinitions = new List<ReleaseDefinition>();
        }

        public async Task LoadReleases()
        {
            var releaseDefinitions = await _azureReleaseRepository.GetReleaseDefinitionsAsync();
            ReleaseDefinitions = releaseDefinitions.ToList();
            _eventBus.OnReleaseLoaded(ReleaseDefinitions);
        }

        private async Task TriggerReleasesAsync(DeployArgs deployArgs)
        {
            var tasks = deployArgs.SelectedDeloyments.Select(t =>  _azureReleaseRepository.DeployToProdAsync(t.Id));

            var results = await Task.WhenAll(tasks);

            if (results.All(r => r.Deployed))
            {
                _eventBus.OnReleaseSucceeded();
            }
            else
            {
                _eventBus.OnReleaseFailed();
            }
        }

        public string CurrentSelection => ReleaseDefinitions.Count > _index ? ReleaseDefinitions[_index].Name : "No deployment available";

        public void Select()
        {
            if (ReleaseDefinitions.Count <= _index) return;

            SelectedReleaseDefinitions.Add(ReleaseDefinitions[_index]);
            SelectedReleaseDefinitions = SelectedReleaseDefinitions.Distinct().ToList();
            _eventBus.OnSelectedDeloyment(ReleaseDefinitions[_index], _index);
        }

        public void MoveDown()
        {
            if (_index <= 0) return;
            _index--;
            _eventBus.OnPreselectedDeloyment(ReleaseDefinitions[_index], _index + 1, IsSelected());
        }

        private bool IsSelected()
        {
            return SelectedReleaseDefinitions.Any(s => s.Id == ReleaseDefinitions[_index].Id);
        }

        public void MoveU()
        {
            if (_index >= ReleaseDefinitions.Count - 1) return;
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