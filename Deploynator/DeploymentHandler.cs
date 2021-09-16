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

            ReleaseDefinitions = _azureReleaseRepository.GetReleaseDefinitionsAsync().Result.ToList();
        }

        private async Task TriggerReleasesAsync(DeployArgs deployArgs)
        {
            foreach (var releaseDefinition in deployArgs.SelectedDeloyments)
            {
                var result = await _azureReleaseRepository.DeployToProdAsync(releaseDefinition.Id);
            }
            
            _eventBus.OnReleaseSuceeded();
        }

        public string CurrentSelection => ReleaseDefinitions[_index].Name;

        public void Select()
        {
            SelectedReleaseDefinitions.Add(ReleaseDefinitions[_index]);
            SelectedReleaseDefinitions = SelectedReleaseDefinitions.Distinct().ToList();
            _eventBus.OnSelectedDeloyment(ReleaseDefinitions[_index]);
        }

        public void MoveDown()
        {
            if (_index <= 0) return;
            _index--;
            _eventBus.OnPreselectedDeloyment(ReleaseDefinitions[_index]);
        }

        public void MoveU()
        {
            if (_index >= ReleaseDefinitions.Count - 1) return;
            _index++;
            _eventBus.OnPreselectedDeloyment(ReleaseDefinitions[_index]);
        }

        public void Deselect()
        {
            SelectedReleaseDefinitions = SelectedReleaseDefinitions.Where(d => d.Id !=ReleaseDefinitions[_index].Id).ToList();
            _eventBus.OnDeselectedDeloyment(ReleaseDefinitions[_index]);
        }

    }
}