using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DTOs;

namespace Deploynator
{
    public class DeploymentHandler
    {
        private readonly IAzureReleaseRepository _azureReleaseRepository;
        private readonly EventBus _eventBus;
        public List<ReleaseDefinition> ReleaseDefinitions;
        public List<ReleaseDefinition> SelectedReleaseDefinitions;
        private int _index;

        public DeploymentHandler(IAzureReleaseRepository azureReleaseRepository, EventBus eventBus)
        {
            _azureReleaseRepository = azureReleaseRepository;
            _eventBus = eventBus;

            _eventBus.ReleaseButtonTriggered += (_, _) => TriggerReleasesAsync();

            _eventBus.UpButtonTriggered += (_, _) => MoveU();
            _eventBus.DownButtonTriggered += (_, _) => MoveDown();
            _eventBus.SelectButtonTriggered += (_, _) => Select();
            _eventBus.DeselectButtonTriggered += (_, _) => Deselect();

            var releases = _azureReleaseRepository.GetReleaseDefinitionsAsync().GetAwaiter().GetResult();

            _eventBus.OnDeploymentsLoaded(releases);

            SelectedReleaseDefinitions = new List<ReleaseDefinition>();
        }

        private async Task TriggerReleasesAsync()
        {
            var result = await _azureReleaseRepository.DeployToProdAsync(1);
            _eventBus.OnReleasesTriggered(SelectedReleaseDefinitions);
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
            if (_index >= ReleaseDefinitions.Count) return;
            _index++;
            _eventBus.OnPreselectedDeloyment(ReleaseDefinitions[_index]);
        }

        public void Deselect()
        {
            SelectedReleaseDefinitions = SelectedReleaseDefinitions.Where(d => d.Id !=ReleaseDefinitions[_index].Id).ToList();
        }

    }
}