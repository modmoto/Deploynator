using System.Collections.Generic;
using System.Linq;

namespace Deploynator
{
    public class DeploymentHandler
    {
        private readonly AzureReleaseRepository _azureReleaseRepository;
        private readonly EventBus _eventBus;
        public List<string> Deloyments;
        public List<string> SelectedDeloyments;
        private int _index;

        public DeploymentHandler(AzureReleaseRepository azureReleaseRepository, EventBus eventBus)
        {
            _azureReleaseRepository = azureReleaseRepository;
            _eventBus = eventBus;

            _eventBus.ReleaseButtonTriggered += (_, _) => TriggerReleases();

            _eventBus.UpButtonTriggered += (_, _) => MoveU();
            _eventBus.DownButtonTriggered += (_, _) => MoveDown();
            _eventBus.SelectButtonTriggered += (_, _) => Select();
            _eventBus.DeselectButtonTriggered += (_, _) => Deselect();

            Deloyments = new List<string>
            {
                "Partner Srvice",
                "Messaging Service",
                "Activity Minutes Service"
            };

            _eventBus.OnDeploymentsLoaded(Deloyments[0]);

            SelectedDeloyments = new List<string>();
            SelectedDeloyments = new List<string>();
        }

        private void TriggerReleases()
        {
            _azureReleaseRepository.Do();
            _eventBus.OnReleasesTriggered(SelectedDeloyments);
        }

        public string CurrentSelection => Deloyments[_index];

        public void Select()
        {
            SelectedDeloyments.Add(Deloyments[_index]);
            SelectedDeloyments = SelectedDeloyments.Distinct().ToList();
            _eventBus.OnSelectedDeloyment(Deloyments[_index]);
        }

        public void MoveDown()
        {
            if (_index <= 0) return;
            _index--;
            _eventBus.OnPreselectedDeloyment(Deloyments[_index]);
        }

        public void MoveU()
        {
            if (_index >= Deloyments.Count - 1) return;
            _index++;
            _eventBus.OnPreselectedDeloyment(Deloyments[_index]);
        }

        public void Deselect()
        {
            SelectedDeloyments = SelectedDeloyments.Where(d => d !=Deloyments[_index]).ToList();
        }

    }

    public class AzureReleaseRepository
    {
        public void Do()
        {
        }
    }
}